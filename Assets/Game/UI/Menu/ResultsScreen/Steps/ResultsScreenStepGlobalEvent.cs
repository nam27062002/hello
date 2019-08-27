// ResultsScreenStepGlobalEvent.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/09/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepGlobalEvent : ResultsScreenStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const int MAX_SUBMIT_ATTEMPTS = 2;

	private enum Panel {
		OFFLINE,
		ACTIVE
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private ShowHideAnimator m_offlineGroupAnim = null;
	[SerializeField] private ShowHideAnimator m_activeGroupAnim = null;
	[Space]
	[SerializeField] private Localizer m_tapToContinueText = null;
	[SerializeField] private ShowHideAnimator m_tapToContinueAnim = null;
	[Space]
	[SerializeField] private TweenSequence m_sequence = null;
	[Space]
	[SerializeField] private NumberTextAnimator m_runScoreText = null;
	[SerializeField] private Transform m_scoreTransferFXFrom = null;
	[SerializeField] private Transform m_scoreTransferFXTo = null;
	[SerializeField] private string m_transferSFX = "";

	// Internal logic
	private HDQuestManager m_questManager = null;
	private Panel m_activePanel = Panel.OFFLINE;
	private Sequence m_activePanelSequence = null;

	private bool m_continueEnabled = false;
	private bool m_panelActiveInitialized = false;
	private GlobalEventsPanelActive m_eventPanelActive = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
    void Awake()
    {
		m_eventPanelActive = gameObject.GetComponentInChildren<GlobalEventsPanelActive>();
    }

	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {
		// Never during FTUX
		// By this point the gamesPlayed var has already been increased, so we must actually count one less game
		if(UsersManager.currentUser.gamesPlayed - 1 < GameSettings.ENABLE_QUESTS_AT_RUN) {
			return false;
		}

		HDQuestManager questManager = HDLiveDataManager.quest;

		if (	questManager.EventExists() &&
				questManager.IsRunning() && 
				questManager.isActive &&
				questManager.m_questData.remainingTime.TotalSeconds > 0 &&
				Application.internetReachability != NetworkReachability.NotReachable
		)
		{
			if ( questManager.GetRunScore() > 0 )
				return true;
		}
		return false;
	}

	/// <summary>
	/// Init this step.
	/// </summary>
	override protected void DoInit() {
		// Get event data!
		m_questManager = HDLiveDataManager.quest;

		// Make sure the number animator respect the event's formatting
		m_runScoreText.CustomTextSetter = OnSetScoreText;

		// Listen to sequence ending
		m_sequence.OnFinished.AddListener(OnHidePostAnimation);
	}

	/// <summary>
	/// Launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Make sure we have the latest event data
		m_questManager = HDLiveDataManager.quest;

		// Subscribe to external events
		// Messenger.AddListener<HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.QUEST_SCORE_SENT, OnContributionConfirmed);

		// Do a first refresh
		InitPanel(false, true);

		// Don't allow continue
		m_continueEnabled = false;

		// Launch sequence!
		m_sequence.Launch();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh data based on current event state.
	/// </summary>
	/// <param name="_animate">Whether to animate or do an instant refresh.</param>
	private void InitPanel(bool _animate, bool _resetValues) {

		m_activePanel = Panel.ACTIVE;
		if ( Application.internetReachability == NetworkReachability.NotReachable )
		{
			m_activePanel = Panel.OFFLINE;
		}


		// Initialize active panel
		switch(m_activePanel) {
			case Panel.ACTIVE: {
				if(_resetValues) {
					m_eventPanelActive.Refresh();
                    m_panelActiveInitialized = true;
				}

				m_runScoreText.SetValue(m_questManager.GetRunScore(), false);

				m_tapToContinueText.Localize("TID_RESULTS_TAP_TO_CONTINUE");
			} break;

			case Panel.OFFLINE: {
				// Nothing to do!
				m_tapToContinueText.Localize("TID_RESULTS_TAP_TO_SKIP");
			} break;
		}

		// Set panels visibility
		m_activeGroupAnim.Set(m_activePanel == Panel.ACTIVE, _animate);
		m_offlineGroupAnim.Set(m_activePanel == Panel.OFFLINE, _animate);
	}

	/// <summary>
	/// Trigger the active panel animation.
	/// </summary>
	private void LaunchActivePanelAnimation() {
		// Kill any existing tween
		string tweenId = "ResultsScreenStepGlobalEvent.ActivePanel";
		DOTween.Kill(tweenId);

		// Init some stuff
		m_continueEnabled = false;
		m_tapToContinueAnim.Hide();

		// Sequentially update values
		float scoreAnimDuration = 1.5f;
		m_activePanelSequence = DOTween.Sequence()
			.SetId(tweenId)
			.AppendInterval(1f)

			// Trigger Score Anim
			.AppendCallback( () => { 
				// Bar anim
				if (m_eventPanelActive != null) {
					m_eventPanelActive.MoveScoreTo(
						m_questManager.m_questData.m_globalScore, 
						m_questManager.m_questData.m_globalScore + m_questManager.GetRunScore(),
						scoreAnimDuration
					);
				}

				// Text anim
				// [AOC] Don't do it, looks weird ending with 0 score
				/*if(m_runScoreText != null) {
					m_runScoreText.duration = scoreAnimDuration;
					m_runScoreText.SetValue(0, true);
				}*/

				// FX anim
				if(m_scoreTransferFXFrom != null && m_scoreTransferFXTo != null) {
					ParticlesTrailFX scoreTransferFX = ParticlesTrailFX.LoadAndLaunch(
						"UI/FX/PF_ScoreTransferFX",
						this.GetComponentInParent<Canvas>().transform,
						m_scoreTransferFXFrom.position + new Vector3(0f, 0f, -0.5f),		// Offset Z so the coins don't collide with the UI elements
						m_scoreTransferFXTo.position + new Vector3(0f, 0f, -0.5f)
					);
					scoreTransferFX.totalDuration = scoreAnimDuration;
				}

				// SFX
				if(!string.IsNullOrEmpty(m_transferSFX)) {
					AudioController.Play(m_transferSFX);
				}
			} )
			.AppendInterval(scoreAnimDuration)

			// Tap to continue
			.AppendCallback(() => {
				// Allow continue
				m_continueEnabled = true;
				m_tapToContinueAnim.Show();
				m_activePanelSequence = null;
			});
	}

	/// <summary>
	/// Discard event contribution and close the popup.
	/// </summary>
	private void CloseAndDiscard() {
		// Continue sequence
		m_sequence.Play();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Tap to continue has been pressed.
	/// </summary>
	public void OnTapToContinue() {
		// Depends on active panel
		switch(m_activePanel) {
			case Panel.ACTIVE: {
				// If the sequence is running, fast forward
				if(m_activePanelSequence != null) {
					// Accelerate everything
					m_activePanelSequence.timeScale = 10;
				}

				// Sequence has finished
				else if(m_continueEnabled) {
					CloseAndDiscard();
				}
			} break;

			case Panel.OFFLINE: {
				// Discard contribution if allowed
				if(m_continueEnabled) {
					CloseAndDiscard();
				}
			} break;
		}
	}

	/// <summary>
	/// Show animation just finished.
	/// </summary>
	public void OnShowPostAnimation() {
		// Trigger animation
		if(m_activePanel == Panel.ACTIVE) {
			LaunchActivePanelAnimation();
		} else {
			// Allow continue
			m_continueEnabled = true;
		}
	}

	/// <summary>
	/// Hide animation is about to start.
	/// </summary>
	public void OnHidePreAnimation() {
		// Disable continue spamming!
		m_continueEnabled = false;

		// Remove all keys from the user. With the new design keys are no longer stockable!
		if(UsersManager.currentUser.keys > 0) {
			ResourcesFlow flow = new ResourcesFlow();
			flow.Begin(
				UsersManager.currentUser.keys, 	// Spend as many keys as we currently have
				UserProfile.Currency.KEYS, 
				HDTrackingManager.EEconomyGroup.GLOBAL_EVENT_KEYS_RESET,
				null
			);
		}
	}

	/// <summary>
	/// Hide animation has finished.
	/// </summary>
	private void OnHidePostAnimation() {
		// Unsubscribe from external events
		// Messenger.RemoveListener<HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.QUEST_SCORE_SENT, OnContributionConfirmed);

		// Clear sequence
		if(m_activePanelSequence != null) {
			m_activePanelSequence.Kill();
			m_activePanelSequence = null;
		}

		// Mark as finished
		OnFinished.Invoke();
	}

	/// <summary>
	/// Retry connection button has been pressed.
	/// </summary>
	public void OnRetryConnectionButton() {
        // If the active panel hasn't been initialized yet then we need to refresh values in case the popup was launched with no connection so it didn't get to be setup with the values of the event (HDK-1962)
        InitPanel(true, !m_panelActiveInitialized);

		// If suceeded, launch intro anim
		if(m_activePanel == Panel.ACTIVE) {
			LaunchActivePanelAnimation();
		}

		// Otherwise fake we're doing something (checking connectivity is immediate, but the player should receive some feedback)
		else {
			BusyScreen.Show(this);

			// Hide after some delay
			UbiBCN.CoroutineManager.DelayedCall(
				() => { 
					BusyScreen.Hide(this); 
					UIFeedbackText.CreateAndLaunch(
						LocalizationManager.SharedInstance.Localize("TID_GEN_NO_CONNECTION"), 
						new Vector2(0.5f, 0.5f), 
						this.GetComponentInParent<Canvas>().transform as RectTransform
					);
				}, 1f
			);
		}
	}

	/// <summary>
	/// We've received a response from the server.
	/// </summary>
	/// <param name="_success">Was the contribute operation successful?</param>
	/*
	private void OnContributionConfirmed(HDLiveDataManager.ComunicationErrorCodes _error) {
		// Hide busy screen
		BusyScreen.Hide(this);

		// Successful?
		if(_error != HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
			// Continue sequence to close the popup
			m_sequence.Play();
		} else {
			// SHOW RETRY OR SKIP POPUP

			// Discard contribution
			// CloseAndDiscard();
		}
	}
	*/

	/// <summary>
	/// The score number animator needs to format a new value.
	/// </summary>
	/// <param name="_animator">The number animator requesting the formatting.</param>
	public void OnSetScoreText(NumberTextAnimator _animator) {
		// Depends on tournament type
		_animator.text.text = m_questManager.FormatScore(_animator.currentValue);
	}
}