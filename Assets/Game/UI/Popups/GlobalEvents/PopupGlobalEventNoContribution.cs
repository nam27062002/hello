// PopupGlobalEventNoContribution.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/08/2017.
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
/// Popup to be displayed at the end of the run when the player didn't score for the global event.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupGlobalEventNoContribution : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/GlobalEvents/PF_PopupGlobalEventNoContribution";
	private const float EVENT_COUNTDOWN_UPDATE_INTERVAL = 1f;	// Seconds

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Image m_eventIcon = null;
	[SerializeField] private TextMeshProUGUI m_descriptionText = null;
	[SerializeField] private TextMeshProUGUI m_timerText = null;

	// Internal
	private GlobalEvent m_event = null;
	private float m_timerUpdateInterval = 0f;
	private bool m_continueEnabled = true;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
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
	/// The popup is about to be opened.
	/// </summary>
	public void OnOpenPreAnimation() {
		// If we have no event data cached, get it now
		if(m_event == null) {
			m_event = GlobalEventManager.currentEvent;	// Should never be null (we shouldn't be displaying this popup if event is null
		}

		// Make sure event timer will be updated
		m_timerUpdateInterval = 0f;

		// Initialize event info
		if(m_event != null && m_event.objective != null) {
			// Objective image
			m_eventIcon.sprite = Resources.Load<Sprite>(UIConstants.MISSION_ICONS_PATH + m_event.objective.icon);

			// Event description
			m_descriptionText.text = m_event.objective.GetDescription();
		}

		// Don't allow continue
		m_continueEnabled = false;
	}

	/// <summary>
	/// The popup has just been opened.
	/// </summary>
	public void OnOpenPostAnimation() {
		// Allow continue
		m_continueEnabled = true;
	}

	/// <summary>
	/// The popup is about to close.
	/// </summary>
	public void OnClosePreAnimation() {
		// Disable continue spamming!
		m_continueEnabled = false;
	}

	/// <summary>
	/// The submit score button has been pressed.
	/// </summary>
	public void OnTapToContinue() {
		// Discard contribution if allowed
		if(m_continueEnabled) {
			GetComponent<PopupController>().Close(true);
		}
	}
}