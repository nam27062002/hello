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
public class ResultsScreenStepMissions : ResultsScreenStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private ResultsScreenMissionPill[] m_pills = new ResultsScreenMissionPill[(int)Mission.Difficulty.COUNT];
	[Space]
	[SerializeField] private NumberTextAnimator m_coinsCounter = null;
	[SerializeField] private NumberTextAnimator m_pcCounter = null;
	[Space]
	[SerializeField] private ShowHideAnimator m_tapToContinue = null;
	[SerializeField] private TweenSequence m_sequence = null;
	
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
		for(int i = 0; i < m_pills.Length; ++i) {
			if(m_pills[i].MustBeDisplayed()) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Initialize this step.
	/// </summary>
	override protected void DoInit() {
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

			// Disable pill if it mustn't be displayed
			m_pills[i].gameObject.SetActive(m_pills[i].MustBeDisplayed());

			// Start hidden
			m_pills[i].animator.ForceHide(false);
		}

		// Notify when sequence is finished
		m_sequence.OnFinished.AddListener(() => OnFinished.Invoke());
	}

	/// <summary>
	/// Launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Init currency counters
		m_coinsCounter.SetValue(m_controller.totalCoins, false);
		m_pcCounter.SetValue(m_controller.totalPc, false);

		// Hide tap to continue
		m_tapToContinue.ForceHide(false);

		// Launch sequence!
		m_sequence.Launch();
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
			m_controller.totalCoins += _pill.mission.rewardCoins;
			m_coinsCounter.SetValue(m_controller.totalCoins, true);
		}, 0.15f);
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