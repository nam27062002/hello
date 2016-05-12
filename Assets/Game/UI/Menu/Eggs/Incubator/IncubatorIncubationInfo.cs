// IncubatorTimerButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controls the incubation timer in the incubator menu.
/// </summary>
[RequireComponent(typeof(ShowHideAnimator))]
public class IncubatorIncubationInfo : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Scene references
	[SerializeField] private Slider m_timerBar = null;
	[SerializeField] private Text m_timerText = null;
	[SerializeField] private Text m_skipPCText = null;
	private ShowHideAnimator m_anim = null;

	// Internal logic
	private int m_previousCostPC = -1;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check references
		Debug.Assert(m_timerBar != null, "Required field!");
		Debug.Assert(m_timerText != null, "Required field!");
		Debug.Assert(m_skipPCText != null, "Required field!");

		// Get external assets
		m_anim = GetComponent<ShowHideAnimator>();

		// Subscribe to external events
		Messenger.AddListener<Egg>(GameEvents.EGG_INCUBATION_STARTED, OnEggIncubationStarted);
		Messenger.AddListener<Egg>(GameEvents.EGG_INCUBATION_ENDED, OnEggIncubationEnded);
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Perform a first refresh
		Refresh();

		// Setup initial visibility
		m_anim.Set(EggManager.isIncubating, false);
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Since this element has a timer, requires a constant refresh
		Refresh();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Egg>(GameEvents.EGG_INCUBATION_STARTED, OnEggIncubationStarted);
		Messenger.RemoveListener<Egg>(GameEvents.EGG_INCUBATION_ENDED, OnEggIncubationEnded);
	}

	/// <summary>
	/// Refresh this slot with the latest data from the manager.
	/// </summary>
	public void Refresh() {
		// Skip if not incubating
		if(!EggManager.isIncubating) return;

		// Timer bar
		m_timerBar.normalizedValue = EggManager.incubationProgress;

		// Timer text
		m_timerText.text = TimeUtils.FormatTime(EggManager.incubationRemaining.TotalSeconds, TimeUtils.EFormat.DIGITS, 3);

		// Skip PC cost - only when changed
		int costPC = EggManager.GetIncubationSkipCostPC();
		if(costPC != m_previousCostPC) {
			// Update control var
			m_previousCostPC = costPC;

			// If cost is 0, use the "free" word instead
			if(costPC == 0) {
				m_skipPCText.text = Localization.Localize("TID_GEN_EXCLAMATION_EXPRESSION", Localization.Localize("TID_GEN_FREE"));
			} else {
				m_skipPCText.text = StringUtils.FormatNumber(costPC);
			}
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// An egg has been added to the incubator.
	/// </summary>
	/// <param name="_egg">The egg.</param>
	private void OnEggIncubationStarted(Egg _egg) {
		// Show and refresh data
		Refresh();
		m_anim.Show();
	}

	/// <summary>
	/// The incubating timer has finished.
	/// </summary>
	/// <param name="_egg">The egg.</param>
	private void OnEggIncubationEnded(Egg _egg) {
		// Hide!
		m_anim.Hide();
	}

	/// <summary>
	/// The skip button has been pressed.
	/// </summary>
	public void OnSkipButton() {
		// Play Sound
		AudioManager.instance.PlayClip("audio/sfx/UI/hsx_ui_button_select");

		// Resources check
		long pricePC = (long)EggManager.GetIncubationSkipCostPC();
		if(UserProfile.pc >= pricePC) {
			// Instantly finish current incubation
			if(EggManager.SkipIncubation()) {
				UserProfile.AddPC(-pricePC);
				PersistenceManager.Save();
			}
		} else {
			// Open PC shop popup
			//PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);

			// Currency popup / Resources flow disabled for now
			UIFeedbackText.CreateAndLaunch(Localization.Localize("TID_PC_NOT_ENOUGH"), new Vector2(0.5f, 0.33f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}
}

