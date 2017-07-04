// PopupGlobalEventContribution.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupGlobalEventContribution : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/InGame/ResultsScreen/PF_PopupGlobalEventContribution";

	private const int MAX_SUBMIT_ATTEMPTS = 2;

	private enum Panel {
		OFFLINE,
		LOG_IN,
		DOUBLE_UP
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Score Group
	[SerializeField] private NumberTextAnimator m_scoreText = null;
	[SerializeField] private Image m_eventIcon = null;

	// State panels
	[Space]
	[SerializeField] private ShowHideAnimator m_offlineGroupAnim = null;
	[SerializeField] private ShowHideAnimator m_loginGroupAnim = null;
	[SerializeField] private ShowHideAnimator m_doubleUpGroupAnim = null;

	// Double up panel
	[Space]
	[SerializeField] private TextMeshProUGUI m_keyCounterText = null;
	[SerializeField] private TextMeshProUGUI m_doubleUpButtonText = null;

	// Internal logic
	private Panel m_activePanel = Panel.OFFLINE;
	private GlobalEvent m_event = null;
	private int m_submitAttempts = 0;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<bool>(GameEvents.GLOBAL_EVENT_SCORE_REGISTERED, OnContributionConfirmed);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<bool>(GameEvents.GLOBAL_EVENT_SCORE_REGISTERED, OnContributionConfirmed);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh data based on current event state.
	/// </summary>
	/// <param name="_animate">Whether to animate or do an instant refresh.</param>
	private void Refresh(bool _animate) {
		// Select visible panel based on event state
		GlobalEventManager.ErrorCode error = GlobalEventManager.CanContribute();
		switch(error) {
			case GlobalEventManager.ErrorCode.NONE:				m_activePanel = Panel.DOUBLE_UP;	break;
			case GlobalEventManager.ErrorCode.OFFLINE:			m_activePanel = Panel.OFFLINE;		break;
			case GlobalEventManager.ErrorCode.NOT_LOGGED_IN:	m_activePanel = Panel.LOG_IN;		break;
		}

		// Initialize active panel
		switch(m_activePanel) {
			case Panel.DOUBLE_UP: {
				// Aux vars
				int doubleUpCostKeys = 1;	// [AOC] TODO!!
				int currentKeys = 3;		// [AOC] TODO!!
				int totalKeys = 10;			// [AOC] TODO!!

				// Key counter text
				string text = LocalizationManager.SharedInstance.Localize("TID_FRACTION", StringUtils.FormatNumber(currentKeys), StringUtils.FormatNumber(totalKeys));
				m_keyCounterText.text = text + " <sprite name=\"icon_key\"/>";

				// Initialize double-up price button
				m_doubleUpButtonText.text = StringUtils.FormatNumber(doubleUpCostKeys);
			} break;

			case Panel.OFFLINE:
			case Panel.LOG_IN: {
				// Nothing to do!
			} break;
		}

		// Set panels visibility
		m_doubleUpGroupAnim.Set(m_activePanel == Panel.DOUBLE_UP);
		m_offlineGroupAnim.Set(m_activePanel == Panel.OFFLINE);
		m_loginGroupAnim.Set(m_activePanel == Panel.LOG_IN);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to be opened.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Reset submit attempts
		m_submitAttempts = 0;

		// If we have no event data cached, get it now
		if(m_event == null) {
			m_event = GlobalEventManager.currentEvent;	// Should never be null (we shouldn't be displaying this popup if event is null
		}

		// Initialize static stuff
		// Objective image
		m_eventIcon.sprite = Resources.Load<Sprite>(UIConstants.MISSION_ICONS_PATH + m_event.objective.goalDef.Get("icon"));

		// Reset number score
		m_scoreText.SetValue(0, false);

		// Do a first refresh
		Refresh(false);
	}

	/// <summary>
	/// The popup has just been opened.
	/// </summary>
	public void OnOpenPostAnimation() {
		// Trigger score number animation
		m_scoreText.SetValue((long)m_event.objective.currentValue, true);
	}

	/// <summary>
	/// Retry connection button has been pressed.
	/// </summary>
	public void OnRetryConnectionButton() {
		// Just refreshing is enough
		// [AOC] TODO!! Show some feedback
		Refresh(true);
	}

	/// <summary>
	/// Log In button has been pressed.
	/// </summary>
	public void OnLoginButton() {
		// [AOC] TODO!!
		UIFeedbackText.CreateAndLaunch(
			LocalizationManager.SharedInstance.Localize("TODO!!"),
			new Vector2(0.5f, 0.5f),
			(RectTransform)this.GetComponentInParent<Canvas>().transform
		);
	}

	/// <summary>
	/// Double up button has been pressed.
	/// </summary>
	public void OnDoubleUpButton() {
		// [AOC] TODO!!
		UIFeedbackText.CreateAndLaunch(
			LocalizationManager.SharedInstance.Localize("TID_GEN_COMING_SOON"),
			new Vector2(0.5f, 0.5f),
			(RectTransform)this.GetComponentInParent<Canvas>().transform
		);
	}

	/// <summary>
	/// Buy more keys button has been pressed.
	/// </summary>
	public void OnBuyMoreKeysButton() {
		// [AOC] TODO!!
		UIFeedbackText.CreateAndLaunch(
			LocalizationManager.SharedInstance.Localize("TID_GEN_COMING_SOON"),
			new Vector2(0.5f, 0.5f),
			(RectTransform)this.GetComponentInParent<Canvas>().transform
		);
	}

	/// <summary>
	/// The submit score button has been pressed.
	/// </summary>
	public void OnSubmitButton() {
		// Attempt to do the contribution (we may have lost connectivity)
		if(GlobalEventManager.Contribute(1f, 1f) == GlobalEventManager.ErrorCode.NONE) {
			// Success! Wait for the confirmation from the server
			BusyScreen.Toggle(true);
		} else {
			// We can't contribute! Refresh panel
			Refresh(true);

			// Reset submission attempts
			m_submitAttempts = 0;
		}
	}

	/// <summary>
	/// We've received a response from the server.
	/// </summary>
	/// <param name="_success">Was the contribute operation successful?</param>
	private void OnContributionConfirmed(bool _success) {
		// Hide busy screen
		BusyScreen.Toggle(false);

		// Successful?
		if(_success) {
			// Just close the popup!
			GetComponent<PopupController>().Close(true);
		} else {
			// Something went wrong!
			m_submitAttempts++;	// Increase sumbission attempts

			// If we've reached max submission attempts, show different feedback and close popup
			if(m_submitAttempts >= MAX_SUBMIT_ATTEMPTS) {
				// Show feedback
				UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
					LocalizationManager.SharedInstance.Localize("Max submission attempts reached.\nIgnoring score!"),	// [AOC] HARDCODED!!
					new Vector2(0.5f, 0.5f),
					(RectTransform)this.GetComponentInParent<Canvas>().transform
				);
				text.text.color = Color.red;

				// Close popup
				GetComponent<PopupController>().Close(true);
			} else {
				// Show feedback
				UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
					LocalizationManager.SharedInstance.Localize("Something went wrong!"),	// [AOC] HARDCODED!!
					new Vector2(0.5f, 0.5f),
					(RectTransform)this.GetComponentInParent<Canvas>().transform
				);
				text.text.color = Color.red;

				// Refresh info
				Refresh(true);
			}
		}
	}
}