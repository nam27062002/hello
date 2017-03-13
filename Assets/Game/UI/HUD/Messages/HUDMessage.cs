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
public class HUDMessage : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Custom UnityEvent implementation to be able to transfer parameters with it.
	/// </summary>
	[Serializable]
	public class HUDMessageEvent : UnityEvent<HUDMessage> {}

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

	[HideInInspector]
	[SerializeField] private float m_idleDuration = 1f;	// Only applies for TIMER hide mode

	// Custom exposed setup for specific types - editor will decide when to show them
	[HideInInspector]
	[SerializeField] private float m_boostReminderTriggerTime = 150f;
	[HideInInspector]
	[SerializeField] private float m_boostReminderFirstSessionTriggerTime = 20f;
	private float m_boostDurationTime = 1f;

	// Events
	[Space]
	[HideInInspector] public HUDMessageEvent OnShow = new HUDMessageEvent();
	[HideInInspector] public HUDMessageEvent OnHide = new HUDMessageEvent();

	// Internal references
	private Animator m_anim = null;

	// Internal logic
	protected float m_timer = 0f;
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
	private float m_boostReminderTime = 0f;
	private float m_boostReminderIntervalTime = 0f;
	private float m_timeSinceLastBoostReminder = 0f;
	private float m_currentBoostTime = 0f;
	private string m_needBiggerDragonEntitySku = "";
	private bool m_isBoosting;

	private bool m_hasEverPerformedAction;

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
	}

	virtual protected void Start() {
		m_messageDuration = m_idleDuration;

		switch(m_type) {
			case Type.BOOST_REMINDER: {
					if (UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.DRAGON_SELECTION)) {
						m_boostReminderTime = m_boostReminderTriggerTime;
						m_boostReminderIntervalTime = m_boostReminderTime;
						m_messageDuration = m_idleDuration * 0.5f;
					} else {
						m_boostReminderTime = m_boostReminderFirstSessionTriggerTime;
						m_boostReminderIntervalTime = m_boostReminderTime * 2f;
					}
				}
				break;
		}

		m_isBoosting = false;
		m_hasEverPerformedAction = false;
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	virtual protected void OnEnable() {
		// Subscribe to external events, based on type
		switch(m_type) {
			case Type.LEVEL_UP:				Messenger.AddListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);					break;
			case Type.SURVIVAL_BONUS:		Messenger.AddListener(GameEvents.SURVIVAL_BONUS_ACHIEVED, OnStandardMessage);				break;
			case Type.HEALTH_EATMORE:		Messenger.AddListener<DragonHealthModifier, DragonHealthModifier>(GameEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);	break;
			case Type.HEALTH_STARVING:		Messenger.AddListener<DragonHealthModifier, DragonHealthModifier>(GameEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);	break;
			case Type.HEALTH_CRITICAL:		Messenger.AddListener<DragonHealthModifier, DragonHealthModifier>(GameEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);	break;
			case Type.POISON:				Messenger.AddListener<float, DamageType, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);	break;
			case Type.SHIELD_MINE_LOST:		Messenger.AddListener<DamageType, Transform>(GameEvents.PLAYER_LOST_SHIELD, OnShieldLostMine);	break;
			case Type.SHIELD_POISON_LOST:	Messenger.AddListener<DamageType, Transform>(GameEvents.PLAYER_LOST_SHIELD, OnShieldLostPoison);	break;
			case Type.NEED_BIGGER_DRAGON:	Messenger.AddListener<DragonTier, string>(GameEvents.BIGGER_DRAGON_NEEDED, OnBiggerDragonNeeded);	break;
			case Type.MISSION_COMPLETED:	Messenger.AddListener<Mission>(GameEvents.MISSION_COMPLETED, OnMissionCompleted);			break;
			case Type.CHEST_FOUND:			Messenger.AddListener<CollectibleChest>(GameEvents.CHEST_COLLECTED, OnChestCollected);					break;
			case Type.BOOST_REMINDER:		Messenger.AddListener<bool>(GameEvents.BOOST_TOGGLED, OnBoostToggled);						break;
			case Type.FIRE_RUSH:			Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFireRushToggled);	break;
			case Type.MEGA_FIRE_RUSH:		Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFireRushToggled);	break;
			case Type.EGG_FOUND:			Messenger.AddListener<CollectibleEgg>(GameEvents.EGG_COLLECTED, OnEggCollected);			break;
			case Type.EGG_INVENTORY_FULL:	Messenger.AddListener<CollectibleEgg>(GameEvents.EGG_COLLECTED_FAIL, OnEggCollectedFail);	break;
			case Type.BREAK_OBJECT_BIGGER_DRAGON:	Messenger.AddListener(GameEvents.BREAK_OBJECT_BIGGER_DRAGON, OnBreakObjectNeedBiggerDragon);			break;
			case Type.BREAK_OBJECT_NEED_TURBO:		Messenger.AddListener(GameEvents.BREAK_OBJECT_NEED_TURBO, OnBreakObjectNeedTurbo);	break;
		}
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	virtual protected void OnDisable() {
		switch(m_type) {
			// Unsubscribe from external events, based on type
			case Type.LEVEL_UP:				Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);					break;
			case Type.SURVIVAL_BONUS:		Messenger.RemoveListener(GameEvents.SURVIVAL_BONUS_ACHIEVED, OnStandardMessage);				break;
			case Type.HEALTH_EATMORE:		Messenger.RemoveListener<DragonHealthModifier, DragonHealthModifier>(GameEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);	break;
			case Type.HEALTH_STARVING:		Messenger.RemoveListener<DragonHealthModifier, DragonHealthModifier>(GameEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);	break;
			case Type.HEALTH_CRITICAL:		Messenger.RemoveListener<DragonHealthModifier, DragonHealthModifier>(GameEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);	break;
			case Type.POISON:				Messenger.RemoveListener<float, DamageType, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);	break;
			case Type.SHIELD_MINE_LOST:		Messenger.RemoveListener<DamageType, Transform>(GameEvents.PLAYER_LOST_SHIELD, OnShieldLostMine);	break;
			case Type.SHIELD_POISON_LOST:	Messenger.RemoveListener<DamageType, Transform>(GameEvents.PLAYER_LOST_SHIELD, OnShieldLostPoison);	break;
			case Type.NEED_BIGGER_DRAGON:	Messenger.RemoveListener<DragonTier, string>(GameEvents.BIGGER_DRAGON_NEEDED, OnBiggerDragonNeeded);	break;
			case Type.MISSION_COMPLETED:	Messenger.RemoveListener<Mission>(GameEvents.MISSION_COMPLETED, OnMissionCompleted);			break;
			case Type.CHEST_FOUND:			Messenger.RemoveListener<CollectibleChest>(GameEvents.CHEST_COLLECTED, OnChestCollected);					break;
			case Type.BOOST_REMINDER:		Messenger.RemoveListener<bool>(GameEvents.BOOST_TOGGLED, OnBoostToggled);						break;
			case Type.FIRE_RUSH:			Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFireRushToggled);	break;
			case Type.MEGA_FIRE_RUSH:		Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFireRushToggled);	break;
			case Type.EGG_FOUND:			Messenger.RemoveListener<CollectibleEgg>(GameEvents.EGG_COLLECTED, OnEggCollected);				break;
			case Type.EGG_INVENTORY_FULL:	Messenger.RemoveListener<CollectibleEgg>(GameEvents.EGG_COLLECTED_FAIL, OnEggCollectedFail);	break;
			case Type.BREAK_OBJECT_BIGGER_DRAGON:	Messenger.RemoveListener(GameEvents.BREAK_OBJECT_BIGGER_DRAGON, OnBreakObjectNeedBiggerDragon);			break;
			case Type.BREAK_OBJECT_NEED_TURBO:		Messenger.RemoveListener(GameEvents.BREAK_OBJECT_NEED_TURBO, OnBreakObjectNeedTurbo);	break;
		}
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	virtual protected void Update() {
		// Update timer - only for TIMER hide mode
		if(m_hideMode == HideMode.TIMER) {
			if(m_timer > 0f) {
				m_timer -= Time.deltaTime;

				// Has timer finished?
				if(m_timer <= 0f) {
					// Hide message!
					Hide();
				}
			}
		}

		if (m_hasEverPerformedAction) {
			return;
		}

		// Custom actions depending on message type
		switch(m_type) {
			case Type.BOOST_REMINDER:
				if (m_isBoosting) { 
					m_currentBoostTime += Time.deltaTime;
					if (m_currentBoostTime >= m_boostDurationTime) {
						if (m_visible) {
							TextMeshProUGUI text = this.FindComponentRecursive<TextMeshProUGUI>();							
							text.text = LocalizationManager.SharedInstance.Localize("TID_FEEDBACK_TUTO_BOOST_SUCCESS");
							Show();
						}
						m_hasEverPerformedAction = true;
					}
				} else {
					m_currentBoostTime = 0f;
					m_timeSinceLastBoostReminder += Time.deltaTime;
					if (m_timeSinceLastBoostReminder >= m_boostReminderTime) {
						// Show feedback!
						Show();
						m_timeSinceLastBoostReminder = 0f;
						m_boostReminderTime = m_boostReminderIntervalTime;
					}
				}
			break;
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
	virtual public void Show() {
		// If already active, decide how to proceed
		bool force = false;
		if(m_visible) {
			switch(m_repeatType) {
				case RepeatType.IGNORE: {
					return; 
				} break;

				case RepeatType.RESTART_TIMER: {
					m_timer = m_messageDuration;
					return;
				} break;

				case RepeatType.RESTART_ANIM: {
					force = true;
				} break;
			}
		}

		// Request to attached system (if any)
		if(!force && m_messageSystem != null) {
			// If show not authorized, don't do anything
			if(!m_messageSystem.RequestShow(this)) return;
		}

		// All checks passed! Show the message
		// Update internal state
		m_visible = true;

		// Trigger anim
		m_anim.SetTrigger("in");

		// Setup hide mode
		m_timer = m_messageDuration;
		m_anim.SetBool("out_auto", m_hideMode == HideMode.ANIMATION);

		// Notify
		OnShow.Invoke(this);
	}

	/// <summary>
	/// Trigger the "out" animation.
	/// </summary>
	virtual public void Hide() {
		// Skip if already inactive
		if(!m_visible) return;

		// Update internal state
		m_visible = false;
		m_timer = 0f;

		// Trigger anim
		m_anim.SetTrigger("out");

		// Notify
		OnHide.Invoke(this);

		// Custom actions depending on message type
		switch(m_type) {
			case Type.BOOST_REMINDER: {
				// Reset timer
				m_timeSinceLastBoostReminder = 0f;
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
	//------------------------------------------------------------------------///// <summary>
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
	private void OnLevelUp(DragonData _dragon) {
		Show();
	}

	/// <summary>
	/// A bigger dragon is required.
	/// </summary>
	/// <param name="_requiredTier">The required tier. DragonTier.COUNT if not defined.</param>
	/// <param name="_entitySku">The entity we're trying to eat.</param>
	private void OnBiggerDragonNeeded(DragonTier _requiredTier, string _entitySku)  {
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
			} else if(m_type == Type.MEGA_FIRE_RUSH && _type == DragonBreathBehaviour.Type.Super) {
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
		// For now we're only interested in the type
		if(_type == DamageType.POISON) {
			Show();
		}
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
}