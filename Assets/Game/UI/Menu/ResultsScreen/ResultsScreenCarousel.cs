// ResultsScreenCarousel.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Misc. information carousel controller.
/// </summary>
public class ResultsScreenCarousel : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private enum Step {
		IDLE,
		MISSION_0,
		MISSION_1,
		MISSION_2,
		CHESTS,
		FINISHED
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private ResultsScreenMissionPill m_missionPill = null;
	public ResultsScreenMissionPill missionPill {
		get { return m_missionPill; }
	}

	[SerializeField] private ResultsScreenChestsPill m_chestsPill = null;
	public ResultsScreenChestsPill chestsPill {
		get { return m_chestsPill; }
	}

	// Internal Logic
	private ResultsScreenCarouselPill m_currentPill = null;
	private Step m_step = Step.IDLE;

	// Step checks
	public bool isIdle {
		get { return m_step == Step.IDLE; }
	}

	public bool isFinished {
		get { return m_step == Step.FINISHED; }
	}

	public bool isIdleOrFinished {
		get { return isIdle || isFinished; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_missionPill != null, "Required field!");
		Debug.Assert(m_chestsPill != null, "Required field!");

		// Subscribe to the OnFinished event on each pill
		m_missionPill.OnFinished.AddListener(OnPillFinished);
		m_chestsPill.OnFinished.AddListener(OnPillFinished);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from the OnFinished event on each pill
		m_missionPill.OnFinished.RemoveListener(OnPillFinished);
		m_chestsPill.OnFinished.RemoveListener(OnPillFinished);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this instance.
	/// </summary>
	public void Init() {
		// Disable all carousel elements
		m_missionPill.animator.ForceHide(false);
		m_chestsPill.animator.ForceHide(false);

		// Reset current step
		StartCoroutine(DoStep(Step.IDLE));
	}

	/// <summary>
	/// Launch the missions pills.
	/// Will interrupt current pills.
	/// </summary>
	public void DoMissions() {
		StartCoroutine(DoStep(Step.MISSION_0));
	}

	/// <summary>
	/// Launch the chests pill.
	/// Will interrupt current pills.
	/// </summary>
	public void DoChests() {
		StartCoroutine(DoStep(Step.CHESTS));
	}

	/// <summary>
	/// Go to the finished state.
	/// Will interrupt current pills.
	/// </summary>
	public void Finish() {
		StartCoroutine(DoStep(Step.FINISHED));
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Launches a specific step and programs next step.
	/// </summary>
	/// <param name="_step">The step to be launched.</param>
	private IEnumerator DoStep(Step _step) {
		// Skip if already in that step
		if(m_step == _step) yield return null;

		// Store new step
		m_step = _step;

		// Select and init target pill
		float hideAnimDuration = 0.25f;	// As setup in the ShowHideAnimator component
		switch(_step) {
			case Step.IDLE: {
				// Nothing to do
			} break;

			case Step.MISSION_0:
			case Step.MISSION_1:
			case Step.MISSION_2: {
				// Choose target mission
				// Check cheats!
				int missionIdx = _step - Step.MISSION_0;	// Quick correlation between Step and Mission.Difficulty enums
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
							DefinitionsManager.SharedInstance.GetDefinition(missionDef.Get("type"), DefinitionsCategory.MISSION_TYPES),
							Random.Range(missionDef.GetAsInt("objectiveBaseQuantityMin"), missionDef.GetAsInt("objectiveBaseQuantityMax")),
							Random.value < 0.5f	// 50% chace
						);
						targetMission.difficulty = Mission.Difficulty.MEDIUM;

						// Mark mission as completed
						targetMission.objective.enabled = true;
						targetMission.objective.currentValue = targetMission.objective.targetValue;	// This will do it
					}
				}

				// Initialize mission pill to check whether it must be displayed or not
				m_missionPill.InitFromMission(targetMission);
				if(m_missionPill.MustBeDisplayed()) {
					// Hide current pill
					HideCurrentPill();

					// Show mission pill
					m_currentPill = m_missionPill;
					m_currentPill.ShowAndAnimate(hideAnimDuration);	// Add enough delay to let the previous pill hide
				} else {
					// Mission doesn't have to be displayed, check next mission or go to idle if last mission
					if(_step == Step.MISSION_2) {
						StartCoroutine(DoStep(Step.IDLE));
					} else {
						StartCoroutine(DoStep(_step + 1));
					}
				}
			} break;

			case Step.CHESTS: {
				// Hide current pill
				HideCurrentPill();

				// Show pill if required
				if(m_chestsPill.MustBeDisplayed()) {
					m_currentPill = m_chestsPill;
					m_currentPill.ShowAndAnimate(hideAnimDuration);	// Add enough delay to let the previous pill hide
				} else {
					// Pill doesn't have to be displayed, go to idle
					StartCoroutine(DoStep(Step.IDLE));
				}
			} break;

			case Step.FINISHED: {
				// Hide current pill, unless it's the chests pill!
				if(m_currentPill != m_chestsPill) {
					HideCurrentPill();
				}

				// If possible, leave the chests pill visible, but don't animate it again!
				if(m_chestsPill.MustBeDisplayed()) {
					yield return new WaitForSeconds(hideAnimDuration);
					m_currentPill = m_chestsPill;
					m_chestsPill.ShowCompleted();	// Nothing will happen if already visible
				} else {
					// Just hide current pill and leave no pill visible
					HideCurrentPill();
				}
			} break;
		}
	}

	/// <summary>
	/// Hides the current pill and loses its reference, if any.
	/// </summary>
	private void HideCurrentPill() {
		if(m_currentPill != null) {
			m_currentPill.animator.Hide();
			m_currentPill = null;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A pill has finished its animation.
	/// </summary>
	public void OnPillFinished() {
		// Depending on current step, launch next step
		switch(m_step) {
			case Step.MISSION_0:
			case Step.MISSION_1:
			case Step.MISSION_2: {
				// Check next mission or go to idle if last mission
				if(m_step == Step.MISSION_2) {
					StartCoroutine(DoStep(Step.IDLE));
				} else {
					StartCoroutine(DoStep(m_step + 1));
				}
			} break;

			case Step.CHESTS: {
				// Nothing to do (actually it's never called), the chests pill is controlled from the main flow (ResultsScreenController)
			} break;

			case Step.IDLE:
			case Step.FINISHED: {
				// Do nothing (should never be here)
			} break;
		}
	}
}