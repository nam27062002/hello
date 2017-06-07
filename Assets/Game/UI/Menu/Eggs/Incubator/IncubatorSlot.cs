// IncubatorSlot.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controls a single slot on the incubator menu.
/// </summary>
public class IncubatorSlot : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] [Range(0, 2)] private int m_slotIdx = 0;	// Change range if EggManager.INVENTORY_SIZE changes
	public int slotIdx { 
		get { return m_slotIdx; }
	}

	// External references
	[Space]
	[SerializeField] private MenuEggUISceneLoader m_eggPreview = null;

	[Space]
	[SerializeField] private Slider m_incubationTimeSlider = null;
	[SerializeField] private TextMeshProUGUI m_incubationTimeText = null;
	[SerializeField] private Localizer m_skipButtonText = null;
	[SerializeField] private UINotification m_newNotification = null;

	// Show/Hide elements
	[Space]
	[SerializeField] private ShowHideAnimator m_emptySlotAnim = null;
	[SerializeField] private ShowHideAnimator m_pendingIncubationAnim = null;
	[SerializeField] private ShowHideAnimator m_incubatingAnim = null;
	[SerializeField] private ShowHideAnimator m_readyAnim = null;
	[SerializeField] private ShowHideAnimator m_glowAnim = null;
	[SerializeField] private ShowHideAnimator m_emptyInfoAnim = null;

	// Internal logic
	private int m_previousCostPC = -1;

	// Properties
	public Egg targetEgg {
		get { return EggManager.inventory[m_slotIdx]; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Notification hidden at start
		if(m_newNotification != null) {
			m_newNotification.Hide(false);
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Perform a first refresh
		//Refresh();
	}

    private void OnDestroy() {     
    	if ( ApplicationManager.IsAlive )   
    	{
        	m_eggPreview.Unload();
        }
    }

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Initialize all animators hidden
		if(m_emptySlotAnim != null)			m_emptySlotAnim.ForceHide(false);
		if(m_pendingIncubationAnim != null)	m_pendingIncubationAnim.ForceHide(false);
		if(m_incubatingAnim != null)		m_incubatingAnim.ForceHide(false);
		if(m_readyAnim != null)				m_readyAnim.ForceHide(false);
		if(m_glowAnim != null)				m_glowAnim.ForceHide(false);
		if(m_emptyInfoAnim != null)			m_emptyInfoAnim.ForceHide(false);

		// Make sure we're updated
		Refresh();

		// Subscribe to external events
		// [AOC] Order is super-important, otherwise the initial state change on the egg will trigger another Refresh which will try to create another view for the egg
		Messenger.AddListener<Egg, Egg.State, Egg.State>(GameEvents.EGG_STATE_CHANGED, OnEggStateChanged);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Egg, Egg.State, Egg.State>(GameEvents.EGG_STATE_CHANGED, OnEggStateChanged);
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Skip time and cost
		if(targetEgg != null && targetEgg.state == Egg.State.INCUBATING) {
			// Timer bar
			m_incubationTimeSlider.normalizedValue = targetEgg.incubationProgress;

			// Timer text
			m_incubationTimeText.text = TimeUtils.FormatTime(targetEgg.incubationRemaining.TotalSeconds, TimeUtils.EFormat.DIGITS, 3, TimeUtils.EPrecision.HOURS, true);

			// Skip PC cost - only when changed
			int costPC = targetEgg.GetIncubationSkipCostPC();
			if(costPC != m_previousCostPC) {
				// Update control var
				m_previousCostPC = costPC;

				// If cost is 0, use the "free" word instead
				if(costPC == 0) {
					m_skipButtonText.Localize("TID_INCUBATOR_SKIP_FREE");
				} else {
					m_skipButtonText.Localize("TID_INCUBATOR_SKIP_FOR", StringUtils.FormatNumber(costPC));
				}
			}
		}
	}

	/// <summary>
	/// Refresh this slot with the latest data from the manager.
	/// </summary>
	public void Refresh() {
		// New notification
		if(m_newNotification != null) {
			m_newNotification.Set(targetEgg != null && targetEgg.isNew);
		}

		// Make sure egg's preview is loaded/unloaded
		m_eggPreview.Load(targetEgg);
		m_eggPreview.gameObject.SetActive(targetEgg != null);

		// Show/Hide elements based on egg state
		if(m_emptySlotAnim != null)			m_emptySlotAnim.Set(targetEgg == null || m_slotIdx != 0);	// Show always for secondary slots
		if(m_pendingIncubationAnim != null) m_pendingIncubationAnim.Set(targetEgg != null && targetEgg.state == Egg.State.READY_FOR_INCUBATION);
		if(m_incubatingAnim != null) 		m_incubatingAnim.Set(targetEgg != null && targetEgg.state == Egg.State.INCUBATING);
		if(m_readyAnim != null) 			m_readyAnim.Set(targetEgg != null && targetEgg.state == Egg.State.READY);
		if(m_glowAnim != null)				m_glowAnim.Set(targetEgg != null);
		if(m_emptyInfoAnim != null)			m_emptyInfoAnim.Set(targetEgg == null);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// An egg has been added to the incubator.
	/// </summary>
	/// <param name="_egg">The egg.</param>
	private void OnEggStateChanged(Egg _egg, Egg.State _from, Egg.State _to) {
		// Refresh view
		Refresh();
	}

	/// <summary>
	/// The start incubation button has been pressed.
	/// </summary>
	public void OnStartIncubationButton() {
		// Just in case
		if(targetEgg == null) return;

		// Just do it
		targetEgg.ChangeState(Egg.State.INCUBATING);
	}

	/// <summary>
	/// The skip button has been pressed.
	/// </summary>
	public void OnSkipButton() {
		// Just in case
		if(targetEgg == null) return;

		// Start purchase flow
		ResourcesFlow purchaseFlow = new ResourcesFlow("SKIP_EGG_INCUBATION");
		purchaseFlow.OnSuccess.AddListener(
			(ResourcesFlow _flow) => {
				// Instantly finish current incubation
				if(targetEgg.SkipIncubation()) {
					PersistenceManager.Save();
				}
			}
		);
		purchaseFlow.Begin((long)targetEgg.GetIncubationSkipCostPC(), UserProfile.Currency.HARD, targetEgg.def);
	}

	/// <summary>
	/// The button has been pressed.
	/// </summary>
	public void OnOpenButton() {
		// Screen controller will take care of it
		InstanceManager.sceneController.GetComponent<MenuScreensController>().StartOpenEggFlow(targetEgg);
	}
}

