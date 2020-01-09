// ResultsScreenStepTournamentInvalidRun.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/05/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepTournamentInvalidRun : ResultsScreenSequenceStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	
    [SerializeField] private BaseIcon m_goalIcon;
    [SerializeField] private TextMeshProUGUI m_goalText = null;
	[SerializeField] private TextMeshProUGUI m_countdownText = null;
	private float m_counter = 1.0f;

    //------------------------------------------------------------------------//
    // ResultsScreenStep IMPLEMENTATION										  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Check whether this step must be displayed or not based on the run results.
    /// </summary>
    /// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
    override public bool MustBeDisplayed() {
		// Only if run was not valid
		return !HDLiveDataManager.tournament.WasLastRunValid();
	}

	/// <summary>
	/// Initialize this step.
	/// </summary>
	override protected void DoInit() {
		// [AOC] TODO!!
	}

	/// <summary>
	/// Initialize and launch this step.
	/// </summary>
	override protected void DoLaunch() {
        
        // Get tournament info
        HDTournamentManager m_tournament = HDLiveDataManager.tournament;
        HDTournamentDefinition m_definition = m_tournament.data.definition as HDTournamentDefinition;

        // goals
        m_goalText.text = m_tournament.GetDescription();

        // Get the icon definition
        string iconSku = m_definition.m_goal.m_icon;

        // The BaseIcon component will load the proper image or 3d model according to iconDefinition.xml
        if (m_goalIcon != null)
        {
            m_goalIcon.LoadIcon(iconSku);
            m_goalIcon.gameObject.SetActive(true);
        }
		m_counter = 1.0f;
		UpdateTimeRemaining();
    }

	/// <summary>
	/// Called when skip is triggered.
	/// </summary>
	override protected void OnSkip() {
		// Nothing to do for now
	}

	void Update(){
		m_counter -= Time.deltaTime;
		if ( m_counter <= 0 ) {
			UpdateTimeRemaining();
			m_counter = 1.0f;
		}
		
	}


	void UpdateTimeRemaining(){
		if ( m_countdownText != null)
		{
			// Update text
			double remainingSeconds = HDLiveDataManager.tournament.data.remainingTime.TotalSeconds;
			string timeString = TimeUtils.FormatTime(
							System.Math.Max(0, remainingSeconds), // Just in case, never go negative
							TimeUtils.EFormat.ABBREVIATIONS,
							3
						);
			m_countdownText.text = timeString;
		}
		
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}