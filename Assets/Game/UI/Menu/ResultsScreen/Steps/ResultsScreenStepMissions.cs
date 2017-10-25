// ResultsScreenStepMissions.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/09/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepMissions : ResultsScreenSequenceStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Space]
	[SerializeField] private ResultsScreenMissionPill[] m_pills = new ResultsScreenMissionPill[(int)Mission.Difficulty.COUNT];
	[Space]
	[SerializeField] private NumberTextAnimator m_coinsCounter = null;
	[SerializeField] private NumberTextAnimator m_pcCounter = null;

	// Internal
	private int m_completedMissions = 0;
	private List<CurrencyTransferFX> m_currencyFX = new List<CurrencyTransferFX>();
	
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

		// Display if we have at least one mission with its objective completed
		return (m_completedMissions > 0);
	}

	/// <summary>
	/// Initialize this step.
	/// </summary>
	override protected void DoInit() {
		// Reset some vars
		m_completedMissions = 0;

		// Initialize pills
		for(int i = 0; i < m_pills.Length; ++i) {
			// Check cheats!
			int missionIdx = i;	// Quick correlation between Step and Mission.Difficulty enums
			Mission targetMission = null;
			if(CPResultsScreenTest.missionsMode == CPResultsScreenTest.MissionsTestMode.NONE) {
				// Not using cheats
				targetMission = MissionManager.GetMission((Mission.Difficulty)missionIdx);
			} else {
				// Display mission?
				int numCheatMissions = CPResultsScreenTest.missionsMode - CPResultsScreenTest.MissionsTestMode.FIXED_0;
				if(numCheatMissions > missionIdx) {
					// Yes! Create a fake temp mission
					List<DefinitionNode> missionDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.MISSIONS);
					DefinitionNode missionDef = missionDefs.GetRandomValue();
					targetMission = new Mission();
					targetMission.InitWithParams(
						missionDef,
						DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_TYPES, missionDef.Get("type")),
						Random.Range(missionDef.GetAsFloat("objectiveBaseQuantityMin"), missionDef.GetAsFloat("objectiveBaseQuantityMax")),
						Random.value < 0.5f	// 50% chace
					);
					targetMission.difficulty = Mission.Difficulty.MEDIUM;

					// Mark mission as completed
					targetMission.objective.enabled = true;
					targetMission.objective.currentValue = targetMission.objective.targetValue;	// This will do it
				}
			}

			// Assign mission to each pill
			m_pills[i].InitFromMission(targetMission);

			// Show this mission? 
			bool show = m_pills[i].MustBeDisplayed();
			if(show) m_completedMissions++;	// Keep count of completed missions

			// Disable pill if it mustn't be displayed
			m_pills[i].gameObject.SetActive(show);

			// Start hidden
			m_pills[i].animator.ForceHide(false);
		}
	}

	/// <summary>
	/// Launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Init currency counters
		m_coinsCounter.SetValue(m_controller.totalCoins, false);
		m_pcCounter.SetValue(m_controller.totalPc, false);
	}

	/// <summary>
	/// Called when skip is triggered.
	/// </summary>
	override protected void OnSkip() {
		// Instantly finish counter texts animations
		m_coinsCounter.SetValue(m_controller.totalCoins, false);
		m_pcCounter.SetValue(m_controller.totalPc, false);

		// Kill active transfer fx's
		for(int i = 0; i < m_currencyFX.Count; ++i) {
			m_currencyFX[i].KillFX();
		}
		m_currencyFX.Clear();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Show mission pill if needed.
	/// </summary>
	public void CheckMission(ResultsScreenMissionPill _pill) {
		// Do nothing if pill should not be displayed
		if(!_pill.MustBeDisplayed()) return;

		// Trigger animation
		_pill.animator.Show();

		// Update total rewarded coins and update counter
		// [AOC] Give it some delay!
		UbiBCN.CoroutineManager.DelayedCall(() => {
			// Update counter
			m_controller.totalCoins += _pill.mission.rewardCoins;
			m_coinsCounter.SetValue(m_controller.totalCoins, true);

			// Show some coins flying around!
			CurrencyTransferFX fx = CurrencyTransferFX.LoadAndLaunch(
				CurrencyTransferFX.COINS,
				this.GetComponentInParent<Canvas>().transform,
				_pill.rewardText.transform.position + new Vector3(0f, 0f, -0.5f),		// Offset Z so the coins don't collide with the UI elements
				m_coinsCounter.transform.position + new Vector3(0f, 0f, -0.5f)
			);
			fx.totalDuration = m_coinsCounter.duration * 0.5f;	// Match the text animator duration (more or less)
			fx.OnFinish.AddListener(() => { m_currencyFX.Remove(fx); });
			m_currencyFX.Add(fx);
		}, 0.15f);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Do the summary line for this step. Connect in the sequence.
	/// </summary>
	public void DoSummary() {
		m_controller.summary.ShowMissions(m_completedMissions);
	}
}