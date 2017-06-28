// GlobalEventsScreenActivePanel.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/06/2017.
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
/// Panel corresponding to an active global event.
/// </summary>
public class GlobalEventsPanelActive : GlobalEventsPanel {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private TextMeshProUGUI m_timerText = null;
	[SerializeField] private Slider m_progressBar = null;
	[SerializeField] private GlobalEventsRewardInfo[] m_rewardInfos = new GlobalEventsRewardInfo[0];
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Just in case
		if(GlobalEventManager.currentEvent == null) return;

		// Update timer
		// [AOC] Could be done with less frequency
		m_timerText.text = TimeUtils.FormatTime(
			GlobalEventManager.currentEvent.remainingTime.TotalSeconds,
			TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
			4
		);

		// [AOC] TODO!! Manage event end when this panel is active
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh displayed data.
	/// </summary>
	override public void Refresh() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}