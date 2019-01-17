// LeaguesPanelWaitingNewSeason.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

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
/// Panel corresponding to a season cooldown.
/// </summary>
public class LeaguesPanelWaitingNewSeason : LeaguesScreenPanel {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float EVENT_COUNTDOWN_UPDATE_INTERVAL = 1f;	// Seconds
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private TextMeshProUGUI m_timerText = null;
	[SerializeField] private Slider m_timerProgressBar = null;

    private HDSeasonData m_season;



	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	public void OnEnable() {
		// Initialize progress bar
		if(m_timerProgressBar != null) {
			m_timerProgressBar.minValue = 0;
			m_timerProgressBar.maxValue = 1;
		}

        m_season = HDLiveDataManager.league.season;

        // Program periodic update call
        InvokeRepeating("UpdatePeriodic", 0f, EVENT_COUNTDOWN_UPDATE_INTERVAL);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	public void OnDisable() {
		// Cancel periodic call
		CancelInvoke();
	}

	/// <summary>
	/// Called periodically.
	/// </summary>
	private void UpdatePeriodic() {		
		// Update timer
		double remainingSeconds = m_season.timeToEnd.TotalSeconds;
		m_timerText.text = TimeUtils.FormatTime(
			System.Math.Max(0, remainingSeconds),	// Never show negative time!
			TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
			4
		);

		// Update progress bar
		if(m_timerProgressBar != null) {
			double progress = remainingSeconds / m_season.durationWaitNewSeason.TotalSeconds;
			m_timerProgressBar.value = 1f - (float)progress;
		}		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	override public void Refresh() {
		// Force a first update on the timer
		UpdatePeriodic();
	}
}