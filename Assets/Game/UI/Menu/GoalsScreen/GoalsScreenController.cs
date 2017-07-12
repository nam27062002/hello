// GoalsScreenController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/07/2017.
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
/// Main controller for the goals screen.
/// </summary>
public class GoalsScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float EVENT_COUNTDOWN_UPDATE_INTERVAL = 1f;	// Seconds

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField]
	private TextMeshProUGUI m_eventCountdownText = null;

	// Internal
	private float m_eventCountdownUpdateTimer = 0f;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure event timer will be updated
		m_eventCountdownUpdateTimer = 0f;
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Update countdown at intervals
		m_eventCountdownUpdateTimer -= Time.deltaTime;
		if(m_eventCountdownUpdateTimer <= 0f) {
			// Reset timer
			m_eventCountdownUpdateTimer = EVENT_COUNTDOWN_UPDATE_INTERVAL;

			// Show countdown only if there is an active event
			GlobalEvent evt = GlobalEventManager.currentEvent;
			bool showTimer = evt != null && evt.isActive;
			m_eventCountdownText.gameObject.SetActive(showTimer);
			if(showTimer) {
				m_eventCountdownText.text = TimeUtils.FormatTime(
					evt.remainingTime.TotalSeconds,
					TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
					2
				);
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}