// ChestsScreenInfoPanel.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/10/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Initializes and controls the Chests info panel in the Goals Screen.
/// </summary>
public class ChestsScreenInfoPanel : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// External refs
	[SerializeField] private Localizer m_collectedText = null;
	[SerializeField] private TextMeshProUGUI m_timerText = null;
	[SerializeField] private Slider m_timerBar = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
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
		Messenger.AddListener(MessengerEvents.CHESTS_RESET, Refresh);
		Messenger.AddListener(MessengerEvents.CHESTS_PROCESSED, Refresh);

		// Refresh
		Refresh();
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.CHESTS_RESET, Refresh);
		Messenger.RemoveListener(MessengerEvents.CHESTS_PROCESSED, Refresh);
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		if(!isActiveAndEnabled) return;

		// Refresh time
		RefreshTime();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh all info.
	/// </summary>
	private void Refresh() {
		// Collected count
		if(m_collectedText != null) {
			m_collectedText.Localize(
				"TID_CHEST_DAILY_DESC", 
				StringUtils.FormatNumber(ChestManager.collectedChests), 
				StringUtils.FormatNumber(ChestManager.NUM_DAILY_CHESTS)
			);
		}

		// Time info
		RefreshTime();
	}

	/// <summary>
	/// Refresh timer info.
	/// </summary>
	private void RefreshTime() {
		// Aux vars
		TimeSpan timeToReset = ChestManager.timeToReset;

		// Text
		if(m_timerText != null) {
			m_timerText.text = TimeUtils.FormatTime(timeToReset.TotalSeconds, TimeUtils.EFormat.DIGITS, 3, TimeUtils.EPrecision.HOURS, true);
		}

		// Bar
		if(m_timerBar != null) {
			m_timerBar.normalizedValue = (float)((24 - timeToReset.TotalHours)/24);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}