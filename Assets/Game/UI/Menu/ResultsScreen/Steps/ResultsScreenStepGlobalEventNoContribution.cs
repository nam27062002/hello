// ResultsScreenStepGlobalEventNoContribution.cs
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
public class ResultsScreenStepGlobalEventNoContribution : ResultsScreenStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float EVENT_COUNTDOWN_UPDATE_INTERVAL = 1f;	// Seconds
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Image m_eventIcon = null;
	[SerializeField] private TextMeshProUGUI m_descriptionText = null;
	[SerializeField] private TextMeshProUGUI m_timerText = null;
	[Space]
	[SerializeField] private ShowHideAnimator m_tapToContinue = null;
	[SerializeField] private TweenSequence m_sequence;

	// Internal logic
	private GlobalEvent m_event = null;
	private float m_timerUpdateInterval = 0f;
	
	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {
		// Never during first run!
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_RUN)) return false;

		// Is there a valid current event to display? Check error codes to know so.
		GlobalEventManager.ErrorCode canContribute = GlobalEventManager.CanContribute();
		if((canContribute == GlobalEventManager.ErrorCode.NONE
			|| canContribute == GlobalEventManager.ErrorCode.OFFLINE
			|| canContribute == GlobalEventManager.ErrorCode.NOT_LOGGED_IN)
			&& GlobalEventManager.currentEvent != null
			&& GlobalEventManager.currentEvent.objective.enabled
			&& GlobalEventManager.currentEvent.remainingTime.TotalSeconds > 0	// We check event hasn't finished while playing
		) {	// [AOC] This will cover cases where the event is active but not enabled for this player (i.e. during the tutorial).

			// In addition to all that, check that we don't actually have any score to register
			if(GlobalEventManager.currentEvent.objective.currentValue <= 0) {
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Init this step.
	/// </summary>
	override protected void DoInit() {
		// Get event data!
		m_event = GlobalEventManager.currentEvent;

		// Notify when sequence is finished
		m_sequence.OnFinished.AddListener(() => OnFinished.Invoke());
	}

	/// <summary>
	/// Launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Make sure we have the latest event data
		m_event = GlobalEventManager.currentEvent;

		// Make sure event timer will be updated
		m_timerUpdateInterval = 0f;

		// Initialize event info
		if(m_event != null && m_event.objective != null) {
			// Objective image
			m_eventIcon.sprite = Resources.Load<Sprite>(UIConstants.MISSION_ICONS_PATH + m_event.objective.icon);

			// Event description
			m_descriptionText.text = m_event.objective.GetDescription();
		}

		// Hide tap to continue
		m_tapToContinue.ForceHide(false);

		// Launch sequence!
		m_sequence.Launch();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Just in case
		if(m_event == null) return;

		// Update countdown at intervals
		m_timerUpdateInterval -= Time.deltaTime;
		if(m_timerUpdateInterval <= 0f) {
			// Reset timer
			m_timerUpdateInterval = EVENT_COUNTDOWN_UPDATE_INTERVAL;

			// Parse remaining time
			m_timerText.text = TimeUtils.FormatTime(
				System.Math.Max(m_event.remainingTime.TotalSeconds, 0),
				TimeUtils.EFormat.ABBREVIATIONS,
				4
			);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The tap to continue button has been pressed.
	/// </summary>
	public void OnTapToContinue() {
		// Only if enabled! (to prevent spamming)
		// [AOC] Reuse visibility state to control whether tap to continue is enabled or not)
		if(!m_tapToContinue.visible) return;

		// Hide tap to continue to prevent spamming
		m_tapToContinue.Hide();

		// Launch end sequence
		m_sequence.Play();
	}
}