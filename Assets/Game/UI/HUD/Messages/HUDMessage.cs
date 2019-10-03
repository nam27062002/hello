// HUDMessage.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/06/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Base class to control a HUD message.
/// Simple in-idle-out behaviour, to be controlled either externally or by inheritance.
/// The attached animator must understand the "in" and "out" triggers.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CanvasGroup))]
public class HUDMessage : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Custom UnityEvent implementation to be able to transfer parameters with it.
	/// </summary>
	[Serializable]
	public class HUDMessageEvent : UnityEvent<HUDMessage> {}

	[Serializable]
	public class BoostMessageSetup {
		[Comment("Time before the message appears for the first time")]
		public float initialDelay = 20f;

		[Comment("Time before the message is dismissed if the player doesn't boost")]
		public float messageDuration = 8f;

		[Comment("Time before the message is displayed again")]
		public float respawnInterval = 150f;

		[Comment("Boosting time required to dismiss the message")]
		public float requiredBoostDuration = 1f;
	}

	// Message types
	public enum Type {
		LEVEL_UP,
		SURVIVAL_BONUS,
		HEALTH_EATMORE,
		HEALTH_STARVING,
		HEALTH_CRITICAL,
		POISON,
		NEED_BIGGER_DRAGON,
		MISSION_COMPLETED,
		CHEST_FOUND,
		BOOST_REMINDER,
		FIRE_RUSH,
		MEGA_FIRE_RUSH,
		EGG_FOUND,
		EGG_INVENTORY_FULL,
		BREAK_OBJECT_BIGGER_DRAGON,
		BREAK_OBJECT_NEED_TURBO,
		SHIELD_POISON_LOST,
		SHIELD_MINE_LOST,
		OLD_DRUNK,
		KEY_FOUND,
		KEY_LIMIT,
		BREAK_OBJECT_SHALL_NOT_PASS,
		DAMAGE_RECEIVED,
		MISSION_ZONE,
		BREAK_OBJECT_WITH_FIRE,
		BOOST_SPACE,
		TIMES_UP,
		TARGET_REACHED,
        BREAK_OBJECT_TO_OPEN,
        HAPPY_BIRTHDAY
	}

	// How to react with consecutive triggers
	public enum RepeatType {
		IGNORE,
		RESTART_TIMER,
		RESTART_ANIM
	}

	// How the hide is triggered
	public enum HideMode {
		TIMER,		// After a given time
		ANIMATION,	// After the idle animation has finished
		MANUAL		// Manual control
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private Type m_type;

	[SerializeField] private int m_priority = 0;
	public int priority {
		get { return m_priority; }
		set { m_priority = value; }
	}

	[Comment("Optional MessageSystem to manage multiple messages priorities, usage similar to Unity's ToggleGroup")]
	[SerializeField] private HUDMessageSystem m_messageSystem = null;

	[Comment("How to react with consecutive triggers")]
	[SerializeField] private RepeatType m_repeatType = RepeatType.RESTART_ANIM;

	[Comment("How the hide animation is triggered:\n- TIMER: After a defined amount of seconds\n- ANIMATION: Animation driven, once the idle animation has finished\n- MANUAL: Manually via the Hide() method\nA manual trigger will override this property in any case.")]
	[SerializeField] private HideMode m_hideMode = HideMode.TIMER;
	[SerializeField] private float m_idleDuration = 1f;	// Only applies for TIMER hide mode

	[SerializeField] private bool m_onlyFirstTime = false;	// Check if only one time!

	// Custom exposed setup for specific types - editor will decide when to show them
	[Separator]
	[SerializeField] private BoostMessageSetup m_boostMessageSetup = new BoostMessageSetup();
	[SerializeField] private BoostMessageSetup m_boostMessageTutorialSetup = new BoostMessageSetup();

	// Events
	[Separator]
	public HUDMessageEvent OnShow = new HUDMessageEvent();
	public HUDMessageEvent OnHide = new HUDMessageEvent();

	// Internal references
	private Animator m_anim = null;

	// Internal logic
	protected float m_hideTimer = 0f;
	protected bool m_visible = false;
	public bool visible {
		get { return m_visible; }
		set { 
			if(value) {
				Show();
			} else {
				Hide();
			}
		}
	}

	// Custom internal vars for specific types
	private float m_messageDuration;
	private string m_needBiggerDragonEntitySku = "";

	private BoostMessageSetup m_currentBoostSetup = null;
	private bool m_isBoosting = false;
	private float m_boostingTimer = 0f;
	private float m_boostSpawnTimer = 0f;
    private string m_defaultText = "";

	private bool m_gameStarted = false;
	private bool m_hasEverPerformedAction = false;

    private string m_zoneId = "";

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	virtual protected void Awake() {
		// Get references
		m_anim = GetComponent<Animator>();

		// Start hidden - the animator's default state should hide them
		m_visible = false;

		// Register to our assigned system
		if(m_messageSystem != null) {
			m_messageSystem.Add(this);
		}

		// Initialize some internal vars
		m_messageDuration = m_idleDuration;

		switch(m_type) {
			case Type.BOOST_REMINDER: {
            
				// Select target setup
				if(UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.BOOST)) {
					m_currentBoostSetup = m_boostMessageSetup;
				} else {
					m_currentBoostSetup = m_boostMessageTutorialSetup;
				}

				// Init timers
				m_messageDuration = m_currentBoostSetup.messageDuration;
				m_boostingTimer = m_currentBoostSetup.requiredBoostDuration;
				m_boostSpawnTimer = m_currentBoostSetup.initialDelay;	// First time, set initial delay
			} break;
		}

		m_isBoosting = false;
		m_hasEverPerformedAction = false;
	}

	private void Start()
	{
		  // Deactivate all childs
        SetOthersVisible( false );
        if ( m_type == Type.BOOST_REMINDER )
        {
            m_defaultText =  Localizer.ApplyCase(Localizer.Case.UPPER_CASE, LocalizationManager.SharedInstance.Localize(InstanceManager.player.data.tidBoostReminder));
        }
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	virtual protected void OnEnable() {
		// Subscribe to external events, based on type
		switch(m_type) {
			case Type.LEVEL_UP:				Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_LEVEL_UP, OnLevelUp);					break;
			case Type.SURVIVAL_BONUS:		Messenger.AddListener(MessengerEvents.SURVIVAL_BONUS_ACHIEVED, OnStandardMessage);				break;
			case Type.HEALTH_EATMORE:		Messenger.AddListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);	break;
			case Type.HEALTH_STARVING:		Messenger.AddListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);	break;
			case Type.HEALTH_CRITICAL:		Messenger.AddListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);	break;
			//case Type.POISON:				break;
			case Type.SHIELD_MINE_LOST:		Messenger.AddListener<DamageType, Transform>(MessengerEvents.PLAYER_LOST_SHIELD, OnShieldLostMine);	break;
			case Type.SHIELD_POISON_LOST:	Messenger.AddListener<DamageType, Transform>(MessengerEvents.PLAYER_LOST_SHIELD, OnShieldLostPoison);	break;
			case Type.NEED_BIGGER_DRAGON:	Messenger.AddListener<DragonTier, string>(MessengerEvents.BIGGER_DRAGON_NEEDED, OnBiggerDragonNeeded);	break;
			case Type.MISSION_COMPLETED:	Messenger.AddListener<Mission>(MessengerEvents.MISSION_COMPLETED, OnMissionCompleted);			break;
			case Type.CHEST_FOUND:			Messenger.AddListener<CollectibleChest>(MessengerEvents.CHEST_COLLECTED, OnChestCollected);					break;
			case Type.BOOST_REMINDER:		Broadcaster.AddListener(BroadcastEventType.BOOST_TOGGLED, this);						break;
			case Type.FIRE_RUSH:			Broadcaster.AddListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);	break;
			case Type.MEGA_FIRE_RUSH:		Broadcaster.AddListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);	break;
			case Type.EGG_FOUND:			Messenger.AddListener<CollectibleEgg>(MessengerEvents.EGG_COLLECTED, OnEggCollected);			break;
			case Type.EGG_INVENTORY_FULL:	Messenger.AddListener<CollectibleEgg>(MessengerEvents.EGG_COLLECTED_FAIL, OnEggCollectedFail);	break;
			case Type.BREAK_OBJECT_BIGGER_DRAGON:	Messenger.AddListener(MessengerEvents.BREAK_OBJECT_BIGGER_DRAGON, OnBreakObjectNeedBiggerDragon);			break;
			case Type.BREAK_OBJECT_NEED_TURBO:		Messenger.AddListener(MessengerEvents.BREAK_OBJECT_NEED_TURBO, OnBreakObjectNeedTurbo);	break;
			case Type.BREAK_OBJECT_SHALL_NOT_PASS:	Messenger.AddListener(MessengerEvents.BREAK_OBJECT_SHALL_NOT_PASS, OnBreakObjectShallNotPass);	break;			
            case Type.BREAK_OBJECT_TO_OPEN: Messenger.AddListener(MessengerEvents.BREAK_OBJECT_TO_OPEN, OnBreakObjectToOpen);   break;          
			case Type.KEY_FOUND:			Messenger.AddListener(MessengerEvents.TICKET_COLLECTED, OnKeyCollected);			break;
			case Type.KEY_LIMIT:			Messenger.AddListener(MessengerEvents.TICKET_COLLECTED_FAIL, OnKeyCollectedFail);			break;
			case Type.DAMAGE_RECEIVED: 		Messenger.AddListener<float, DamageType, Transform>(MessengerEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);			break;
			case Type.MISSION_ZONE: 		Messenger.AddListener<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, OnMissionZone);break;
			case Type.BREAK_OBJECT_WITH_FIRE:		Messenger.AddListener(MessengerEvents.BREAK_OBJECT_WITH_FIRE, OnBreakObjectWithFire);	break;
			case Type.BOOST_SPACE:			Messenger.AddListener(MessengerEvents.BOOST_SPACE, OnBoostSky); break;
			case Type.TIMES_UP:				Messenger.AddListener(MessengerEvents.TIMES_UP, ShowCallback); break;
			case Type.TARGET_REACHED:		Messenger.AddListener(MessengerEvents.TARGET_REACHED, ShowObjCompleted); break;
            case Type.HAPPY_BIRTHDAY:       Broadcaster.AddListener( BroadcastEventType.START_COLLECTIBLE_HUNGRY_MODE,this); break;

        }

		switch(m_hideMode) {
			case HideMode.TIMER:			Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);	break;
		}
	}
    
    public void SetOthersVisible( bool _visible)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(_visible);
        }
        TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
        if (text != null)
            text.enabled = _visible;
    }
    

	/// <summary>
	/// Component disabled.
	/// </summary>
	virtual protected void OnDisable() {
		switch(m_type) {
			// Unsubscribe from external events, based on type
			case Type.LEVEL_UP:				Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_LEVEL_UP, OnLevelUp);					break;
			case Type.SURVIVAL_BONUS:		Messenger.RemoveListener(MessengerEvents.SURVIVAL_BONUS_ACHIEVED, OnStandardMessage);				break;
			case Type.HEALTH_EATMORE:		Messenger.RemoveListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);	break;
			case Type.HEALTH_STARVING:		Messenger.RemoveListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);	break;
			case Type.HEALTH_CRITICAL:		Messenger.RemoveListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);	break;
			//case Type.POISON:				break;
			case Type.SHIELD_MINE_LOST:		Messenger.RemoveListener<DamageType, Transform>(MessengerEvents.PLAYER_LOST_SHIELD, OnShieldLostMine);	break;
			case Type.SHIELD_POISON_LOST:	Messenger.RemoveListener<DamageType, Transform>(MessengerEvents.PLAYER_LOST_SHIELD, OnShieldLostPoison);	break;
			case Type.NEED_BIGGER_DRAGON:	Messenger.RemoveListener<DragonTier, string>(MessengerEvents.BIGGER_DRAGON_NEEDED, OnBiggerDragonNeeded);	break;
			case Type.MISSION_COMPLETED:	Messenger.RemoveListener<Mission>(MessengerEvents.MISSION_COMPLETED, OnMissionCompleted);			break;
			case Type.CHEST_FOUND:			Messenger.RemoveListener<CollectibleChest>(MessengerEvents.CHEST_COLLECTED, OnChestCollected);					break;
			case Type.BOOST_REMINDER:		 Broadcaster.RemoveListener(BroadcastEventType.BOOST_TOGGLED, this);						break;
			case Type.FIRE_RUSH:			Broadcaster.RemoveListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);	break;
			case Type.MEGA_FIRE_RUSH:		Broadcaster.RemoveListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);	break;
			case Type.EGG_FOUND:			Messenger.RemoveListener<CollectibleEgg>(MessengerEvents.EGG_COLLECTED, OnEggCollected);				break;
			case Type.EGG_INVENTORY_FULL:	Messenger.RemoveListener<CollectibleEgg>(MessengerEvents.EGG_COLLECTED_FAIL, OnEggCollectedFail);	break;
			case Type.BREAK_OBJECT_BIGGER_DRAGON:	Messenger.RemoveListener(MessengerEvents.BREAK_OBJECT_BIGGER_DRAGON, OnBreakObjectNeedBiggerDragon);			break;
			case Type.BREAK_OBJECT_NEED_TURBO:		Messenger.RemoveListener(MessengerEvents.BREAK_OBJECT_NEED_TURBO, OnBreakObjectNeedTurbo);	break;
			case Type.BREAK_OBJECT_SHALL_NOT_PASS:	Messenger.RemoveListener(MessengerEvents.BREAK_OBJECT_SHALL_NOT_PASS, OnBreakObjectShallNotPass);	break;			
            case Type.BREAK_OBJECT_TO_OPEN: Messenger.RemoveListener(MessengerEvents.BREAK_OBJECT_TO_OPEN, OnBreakObjectToOpen);   break;          
			case Type.KEY_FOUND:			Messenger.RemoveListener(MessengerEvents.TICKET_COLLECTED, OnKeyCollected);			break;
			case Type.KEY_LIMIT:			Messenger.RemoveListener(MessengerEvents.TICKET_COLLECTED_FAIL, OnKeyCollectedFail);			break;
			case Type.DAMAGE_RECEIVED: 		Messenger.RemoveListener<float, DamageType, Transform>(MessengerEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);			break;
			case Type.MISSION_ZONE: 		Messenger.RemoveListener<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, OnMissionZone);break;
			case Type.BREAK_OBJECT_WITH_FIRE: Messenger.RemoveListener(MessengerEvents.BREAK_OBJECT_WITH_FIRE, OnBreakObjectWithFire);	break;
			case Type.BOOST_SPACE:			Messenger.RemoveListener(MessengerEvents.BOOST_SPACE, OnBoostSky); break;
			case Type.TIMES_UP:				Messenger.RemoveListener(MessengerEvents.TIMES_UP, ShowCallback); break;
			case Type.TARGET_REACHED:		Messenger.RemoveListener(MessengerEvents.TARGET_REACHED, ShowObjCompleted); break;
            case Type.HAPPY_BIRTHDAY:       Broadcaster.RemoveListener(BroadcastEventType.START_COLLECTIBLE_HUNGRY_MODE, this); break;
        }

		switch(m_hideMode) {
			case HideMode.TIMER:			Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);	break;
		}
	}
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.FURY_RUSH_TOGGLED:
            {
                FuryRushToggled furyRushToggled = (FuryRushToggled)broadcastEventInfo;
                OnFireRushToggled( furyRushToggled.activated, furyRushToggled.type );
            }break;
            case BroadcastEventType.BOOST_TOGGLED:
            {
                ToggleParam toggleParam = (ToggleParam)broadcastEventInfo;
                OnBoostToggled(toggleParam.value); 
            }break;
			case BroadcastEventType.START_COLLECTIBLE_HUNGRY_MODE:
			{
				OnStartBirthdayMode();
			}break;
        }
    }
    

	/// <summary>
	/// Called every frame
	/// </summary>
	virtual protected void Update() {
		// Don't update timers until game has actually started
		if(!m_gameStarted) return;

		// Update timer - only for TIMER hide mode
		if(m_hideMode == HideMode.TIMER) {
			if(m_hideTimer > 0f) {
				m_hideTimer -= Time.deltaTime;

				// Has timer finished?
				if(m_hideTimer <= 0f) {
					// Hide message!
					Hide();
				}
			}
		}

		if(m_hasEverPerformedAction) {
			return;
		}

		// Custom actions depending on message type
		switch(m_type) {
			case Type.BOOST_REMINDER: {
				// Always update the spawn timer
				m_boostSpawnTimer -= Time.deltaTime;

				// If boosting, check whether we've reached the required threshold
				if(m_isBoosting) { 
					m_boostingTimer -= Time.deltaTime;
					if(m_boostingTimer <= 0f) {
						if(m_visible) {
							// Show again with a custom message and duration to confirm the action
							m_messageDuration = m_boostMessageSetup.messageDuration;
							TextMeshProUGUI text = this.FindComponentRecursive<TextMeshProUGUI>();							
							text.text = LocalizationManager.SharedInstance.Localize("TID_FEEDBACK_TUTO_BOOST_SUCCESS");
							Show();

							// Mark tutorial as completed!
							UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.BOOST, true);
						}

						// Don't show anymore!
						m_hasEverPerformedAction = true;
					}
				} else {
					// Do we need to show ther reminder? Not while we're boosting!
					if(m_boostSpawnTimer <= 0f) {
						// Show feedback!
                        if ( !m_visible )
                        {
                            // Check boost tid
                            TextMeshProUGUI text = this.FindComponentRecursive<TextMeshProUGUI>();
                            text.text = m_defaultText;
                        }
                        
						// Don't reset timers if it couldn't be shown! Will be displayed asap
						if(Show()) {
							m_boostSpawnTimer = m_currentBoostSetup.respawnInterval;
						}
					}
				}
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Trigger the "in" animation.
	/// If the message has an idle duration defined, it will automatically hide
	/// after that period has elapsed. Otherwise <c>Hide()</c> must be manually called.
	/// If this message is linked to a HUDMessageSystem, it will check whether it 
	/// can be shown or not based on priorities vs other messages in the same system.
	/// </summary>
	/// <returns>Whether the message could be displayed or not (HUDMessageSystem priorities).</returns>
	virtual public bool Show() {
		if ( m_type == Type.MISSION_ZONE )
		{
			TextMeshProUGUI text = this.FindComponentRecursive<TextMeshProUGUI>();
			Debug.Log(Color.green.Tag("HUDMESSAGE Show " + text.text + " "+ m_visible));
		}
		
		// If already active, decide how to proceed
		bool force = false;
		if(m_visible) {
			switch(m_repeatType) {
				case RepeatType.IGNORE: {
					return true; 
				} 

				case RepeatType.RESTART_TIMER: {
					m_hideTimer = m_messageDuration;
					return true;
				} 

				case RepeatType.RESTART_ANIM: {
					force = true;
				} break;
			}
		}

		// Request to attached system (if any)
		if(!force && m_messageSystem != null) {
			// If show not authorized, don't do anything
			if(!m_messageSystem.RequestShow(this)) return false;
		}

        // Activate
        SetOthersVisible(true);
        
        // All checks passed! Show the message
		// Update internal state
		m_visible = true;

		// Trigger anim
		m_anim.ResetTrigger( GameConstants.Animator.OUT );
		m_anim.SetTrigger( GameConstants.Animator.IN );


		// Setup hide mode
		m_hideTimer = m_messageDuration;
		m_anim.SetBool( GameConstants.Animator.OUT_AUTO , m_hideMode == HideMode.ANIMATION);

		// Notify
		OnShow.Invoke(this);

		return true;
	}

    public void OnHideMessage()
    {
		if ( m_type == Type.MISSION_ZONE )
		{
			TextMeshProUGUI text = this.FindComponentRecursive<TextMeshProUGUI>();
			Debug.Log(Colors.orange.Tag("HUDMESSAGE OnHideMessage " + text.text + " "+ m_visible));
		}

        if (m_hideMode == HideMode.ANIMATION)
        {
            Hide(true);
        }

		if (!m_visible)
		{
			// Deactivate all
        	SetOthersVisible(false);
		}
        
    }

	/// <summary>
	/// Trigger the "out" animation.
	/// </summary>
	virtual public void Hide( bool _outDone = false ) {
		if ( m_type == Type.MISSION_ZONE )
		{
			TextMeshProUGUI text = this.FindComponentRecursive<TextMeshProUGUI>();
			Debug.Log(Colors.purple.Tag("HUDMESSAGE Hide " + text.text + " "+ m_visible));
		}

		// Skip if already inactive
		if(!m_visible) return;

		// Update internal state
		m_visible = false;
		m_hideTimer = 0f;

		m_anim.ResetTrigger( GameConstants.Animator.IN );
		// Trigger anim
        if ( !_outDone )
		    m_anim.SetTrigger( GameConstants.Animator.OUT );

		// Notify
		OnHide.Invoke(this);

		// Custom actions depending on message type
		switch(m_type) {
			case Type.BOOST_REMINDER: {
				// Reset timer
				m_boostSpawnTimer = m_currentBoostSetup.respawnInterval;
			} break;
		}

        
	}

	/// <summary>
	/// Helper method to control particle systems from animations.
	/// </summary>
	/// <param name="_psName">The name of the particle system to be restarted.</param>
	private void RestartParticleSystem(string _psName) {
		// Get target particle system
		ParticleSystem ps = this.FindComponentRecursive<ParticleSystem>(_psName);
		if(ps == null) return;

		// Restart effect
		ps.Stop();
		ps.Play();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Game has started.
	/// </summary>
	private void OnGameStarted() {
		m_gameStarted = true;
	}

	/// <summary>
	/// Generic callback to trigger standard messages.
	/// </summary>
	private void OnStandardMessage() {
		Show();
	}

	/// <summary>
	/// Generic callback to trigger toggle messages.
	/// </summary>
	/// <param name="_toggle">Whether the message should be displayed or not.</param>
	private void OnToggleMessage(bool _toggle) {
		if(_toggle) {
			Show();
		} else {
			Hide();
		}
	}

	/// <summary>
	/// The dragon's health modifier has been changed.
	/// </summary>
	/// <param name="_oldModifier">Old modifier.</param>
	/// <param name="_newModifier">New modifier.</param>
	private void OnHealthModifierChanged(DragonHealthModifier _oldModifier, DragonHealthModifier _newModifier) {
		// If no modifier, turn off
		if(_newModifier == null) {
			Hide();
		} else {
			// Toggle depending on new modifier and message type
			switch(m_type) {
				case Type.HEALTH_EATMORE: OnToggleMessage(_newModifier.IsEatMore());	break;
				case Type.HEALTH_STARVING: OnToggleMessage(_newModifier.IsStarving());	break;
				case Type.HEALTH_CRITICAL: OnToggleMessage(_newModifier.IsCritical());	break;
			}
		}
	}

	/// <summary>
	/// A dragon has leveled up.
	/// </summary>
	/// <param name="_dragon">The dragon that has leveled up.</param>
	private void OnLevelUp(IDragonData _dragon) {
		Show();
	}

	/// <summary>
	/// A bigger dragon is required.
	/// </summary>
	/// <param name="_requiredTier">The required tier. DragonTier.COUNT if not defined.</param>
	/// <param name="_entitySku">The entity we're trying to eat.</param>
	private void OnBiggerDragonNeeded(DragonTier _requiredTier, string _entitySku)  {
		if (_requiredTier < DragonTier.COUNT) {
			// Setup text
			DefinitionNode tierDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.DRAGON_TIERS, "order", ((int)_requiredTier).ToString());
			TextMeshProUGUI text = this.FindComponentRecursive<TextMeshProUGUI>();
			if(tierDef == null) {
				// We don't know the exact tier, use generic text
				text.text = LocalizationManager.SharedInstance.Localize("TID_FEEDBACK_NEED_BIGGER_DRAGON");
			} else {
				// Use tier icon
				text.text = LocalizationManager.SharedInstance.Localize("TID_FEEDBACK_NEED_TIER_DRAGON", UIConstants.GetSpriteTag(tierDef.Get("icon")));
			}

			// If already visible and trying to eat the same entity, don't restart the animation
			if(m_visible && m_needBiggerDragonEntitySku == _entitySku) {
				m_repeatType = RepeatType.RESTART_TIMER;
			} else {
				m_repeatType = RepeatType.RESTART_ANIM;
			}
			m_needBiggerDragonEntitySku = _entitySku;

			// Show!
			Show();
		}
	}

	/// <summary>
	/// A mission has been completed.
	/// </summary>
	/// <param name="_mission">The mission that has been completed.</param>
	private void OnMissionCompleted(Mission _mission) {
		// Init text
		this.FindComponentRecursive<TextMeshProUGUI>("MissionObjectiveText").text = _mission.objective.GetDescription();

		// Show!
		Show();
	}

	/// <summary>
	/// A chest has been found.
	/// </summary>
	/// <param name="_chest">The chest that has been collected.</param>
	private void OnChestCollected(CollectibleChest _chest) {
		Show();
	}

	/// <summary>
	/// The player has started/stopped boosting.
	/// </summary>
	/// <param name="_toggled">Whether the boost was turned on or off.</param>
	private void OnBoostToggled(bool _toggled) {
		m_isBoosting = _toggled;

		// Reset timer
		m_boostingTimer = m_currentBoostSetup.requiredBoostDuration;
	}

	private void OnBoostSky() {
		Show();
	}

	private void ShowCallback()
	{
		Show();
	}

	private void ShowObjCompleted()
	{

        // Not very efficient, but only called once after the mission.
        BaseIcon objectiveIcon = gameObject.GetComponentInChildren<BaseIcon>();
        
		if (objectiveIcon != null )
		{

            // Get the icon definition
            HDTournamentDefinition def = HDLiveDataManager.tournament.data.definition as HDTournamentDefinition;
            string iconSku = def.m_goal.m_icon;

            // The BaseIcon component will load the proper image or 3d model according to iconDefinition.xml
            objectiveIcon.LoadIcon(iconSku);
            objectiveIcon.gameObject.SetActive(true);


		}
		Show();
	}

	/// <summary>
	/// Fire rush has been toggled.
	/// </summary>
	/// <param name="_active">Whether the fury rush has been activated or not.</param>
	/// <param name="_type">The type of fire rush that was toggled.</param>
	private void OnFireRushToggled(bool _active, DragonBreathBehaviour.Type _type) {
		// Just trigger the animation
		if(_active) {
			// Only trigger with the right type!
			if(m_type == Type.FIRE_RUSH && _type == DragonBreathBehaviour.Type.Standard) {
				Show();
			} else if(m_type == Type.MEGA_FIRE_RUSH && _type == DragonBreathBehaviour.Type.Mega) {
				Show();
			}
		}
	}

	/// <summary>
	/// The player has received some damage.
	/// </summary>
	/// <param name="_damage">Amount of damage.</param>
	/// <param name="_type">Type of damage.</param>
	/// <param name="_source">Source of the damage (optional).</param>
	private void OnDamageReceived(float _damage, DamageType _type, Transform _source) {
		if (_source != null && !m_visible) {
			// For now we're only interested in the type
			Entity entity = _source.GetComponent<Entity>();
			if (entity != null) {
				// Yes!! Show a message?
				string msg = entity.feedbackData.GetFeedback(FeedbackData.Type.DAMAGE);
				if(!String.IsNullOrEmpty(msg)) {
					TextMeshProUGUI text = this.FindComponentRecursive<TextMeshProUGUI>();
					text.text = msg;
					Show();
				}
			}
		}
	}


	private void OnMissionZone(bool toggle, ZoneTrigger zone){
		if ( toggle ){
			if ( m_onlyFirstTime )
			{
				// Get text to show
                if (!UsersManager.currentUser.m_visitedZones.Contains(zone.m_zoneId))
                {
                    m_zoneId = zone.m_zoneId;
    				TextMeshProUGUI text = this.FindComponentRecursive<TextMeshProUGUI>();
    				string localizedZone = LocalizationManager.SharedInstance.Localize(zone.m_zoneTid);
    				string localized = LocalizationManager.SharedInstance.Localize("TID_LEVEL_AREA_WELCOME", localizedZone);
    		        text.text = localized;
                    // Delay register zone so the other message can check that the zone is not in the list
                    OnHide.AddListener( RegisterZone );
    		        Show();
                }
			}
			else
			{
                if (UsersManager.currentUser.m_visitedZones.Contains(zone.m_zoneId))
                {
					m_zoneId = zone.m_zoneId;
    				// Get text to show
    				TextMeshProUGUI text = this.FindComponentRecursive<TextMeshProUGUI>();
    		        string localized = LocalizationManager.SharedInstance.Localize(zone.m_zoneTid);
    		        text.text = localized;
    		        Show();			
                }
			}
		}
        else
        {
            // Hide small reminder
            if (!m_onlyFirstTime && zone.m_zoneId == m_zoneId)	// Only hide if still the same
			{
                Hide();
			}
        }
	}
    
    private void RegisterZone( HUDMessage message )
    {
        if ( !UsersManager.currentUser.m_visitedZones.Contains(m_zoneId))
            UsersManager.currentUser.m_visitedZones.Add( m_zoneId );
        OnHide.RemoveListener( RegisterZone );
    }

	private void OnShieldLostMine(DamageType _type, Transform _tr) {
		// For now we're only interested in the type
		if(_type == DamageType.MINE) {
			Show();
		}
	}

	private void OnShieldLostPoison(DamageType _type, Transform _tr) {
		// For now we're only interested in the type
		if(_type == DamageType.POISON) {
			Show();
		}
	}

	/// <summary>
	/// An egg has been found while playing.
	/// </summary>
	/// <param name="_egg">The found egg.</param>
	private void OnEggCollected(CollectibleEgg _egg) {
		Show();
	}

    /// <summary>
	/// All the pieces of the cake have been collected.
	/// </summary>
	/// <param name="_egg">The found egg.</param>
    private void OnStartBirthdayMode ()
    {
        Show();
    }


    /// <summary>
    /// An egg has been found while playing but we failed to collect it (inventory full).
    /// </summary>
    /// <param name="_egg">The found egg.</param>
    private void OnEggCollectedFail(CollectibleEgg _egg) {
		Show();
	}

	void OnBreakObjectNeedBiggerDragon()
	{
		Show();
	}

	void OnBreakObjectNeedTurbo()
	{
		Show();
	}

	void OnBreakObjectShallNotPass() {
		Show();
	}

	void OnBreakObjectWithFire() {
		Show();
	}

    void OnBreakObjectToOpen() {
        Show();
    }

	/// <summary>
	/// A key has been found while playing.
	/// </summary>
	/// <param name="_key">The found key.</param>
	private void OnKeyCollected() {
		Show();
	}

	/// <summary>
	/// A key has been found while playing, but we failed to collect it (limit reached).
	/// </summary>
	/// <param name="_key">The found key.</param>
	private void OnKeyCollectedFail() {
		Show();
	}
}