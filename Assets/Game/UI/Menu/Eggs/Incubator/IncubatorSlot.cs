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
	[SerializeField] private Slider m_incubationTimeSlider = null;
	[SerializeField] private Text m_incubationTimeText = null;
	[SerializeField] private Text m_skipCostText = null;
	[SerializeField] private UINotification m_newNotification = null;

	// Show/Hide elements
	[Space]
	[SerializeField] private ShowHideAnimator m_emptySlotAnim = null;
	[SerializeField] private ShowHideAnimator m_pendingIncubationAnim = null;
	[SerializeField] private ShowHideAnimator m_incubatingAnim = null;
	[SerializeField] private ShowHideAnimator m_readyAnim = null;

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
		m_newNotification.Hide(false);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Perform a first refresh
		//Refresh();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<Egg, Egg.State, Egg.State>(GameEvents.EGG_STATE_CHANGED, OnEggStateChanged);

		// Make sure we're updated
		Refresh();
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
			m_incubationTimeText.text = TimeUtils.FormatTime(targetEgg.incubationRemaining.TotalSeconds, TimeUtils.EFormat.DIGITS, 3);

			// Skip PC cost - only when changed
			int costPC = targetEgg.GetIncubationSkipCostPC();
			if(costPC != m_previousCostPC) {
				// Update control var
				m_previousCostPC = costPC;

				// If cost is 0, use the "free" word instead
				if(costPC == 0) {
					m_skipCostText.text = LocalizationManager.SharedInstance.Localize("TID_GEN_EXCLAMATION_EXPRESSION", LocalizationManager.SharedInstance.Localize("TID_GEN_FREE"));
				} else {
					m_skipCostText.text = StringUtils.FormatNumber(costPC);
				}
			}
		}
	}

	/// <summary>
	/// Refresh this slot with the latest data from the manager.
	/// </summary>
	public void Refresh() {
		Debug.Log("Refreshing slot " + slotIdx + ": " + (targetEgg != null ? targetEgg.state.ToString() : "EMPTY"));

		// New notification
		m_newNotification.Set(targetEgg != null && targetEgg.isNew);

		// Show/Hide elements based on egg state
		//m_emptySlotAnim.Set(targetEgg == null);
		m_emptySlotAnim.Set(false);	// [AOC] Never show (for now)
		m_pendingIncubationAnim.Set(targetEgg != null && targetEgg.state == Egg.State.STORED && EggManager.incubatingEgg == null);	// Pending incubation is a bit more tricky: only allow if there is no other egg already incubating
		m_incubatingAnim.Set(targetEgg != null && targetEgg.state == Egg.State.INCUBATING);
		m_readyAnim.Set(targetEgg != null && targetEgg.state == Egg.State.READY);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// An egg has been added to the incubator.
	/// </summary>
	/// <param name="_egg">The egg.</param>
	private void OnEggStateChanged(Egg _egg, Egg.State _from, Egg.State _to) {
		// Does it match our egg?
		// Refresh as well when any egg has started/finished incubating
		if(_egg == targetEgg || _to == Egg.State.INCUBATING || _from == Egg.State.INCUBATING) {
			// Refresh view
			// [AOC] TODO!! Trigger different FX depending on state
			Refresh();
		}
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

		// Resources check
		long pricePC = (long)targetEgg.GetIncubationSkipCostPC();
		if(UserProfile.pc >= pricePC) {
			// Instantly finish current incubation
			if(targetEgg.SkipIncubation()) {
				UserProfile.AddPC(-pricePC);
				PersistenceManager.Save();
			}
		} else {
			// Open PC shop popup
			//PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);

			// Currency popup / Resources flow disabled for now
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PC_NOT_ENOUGH"), new Vector2(0.5f, 0.33f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}

	/// <summary>
	/// The button has been pressed.
	/// </summary>
	public void OnOpenButton() {
		// Incubator screen will take care of it
		MenuScreensController screensController = InstanceManager.sceneController.GetComponent<MenuScreensController>();
		IncubatorScreenController incubatorScreen = screensController.GetScreen((int)MenuScreens.INCUBATOR).GetComponent<IncubatorScreenController>();
		if(incubatorScreen != null) {
			incubatorScreen.StartOpenEggFlow(targetEgg);
		}
	}
}

