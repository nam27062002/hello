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
		PROGRESSION,
		MISSION_0,
		MISSION_1,
		MISSION_2,
		FINISHED
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private ResultsScreenProgressionPill m_progressionPill = null;
	public ResultsScreenProgressionPill progressionPill {
		get { return m_progressionPill; }
	}

	[SerializeField] private ResultsScreenMissionPill m_missionPill = null;
	public ResultsScreenMissionPill missionPill {
		get { return m_missionPill; }
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
		Debug.Assert(m_progressionPill != null, "Required field!");
		Debug.Assert(m_missionPill != null, "Required field!");

		// Subscribe to the OnFinished event on each pill
		m_progressionPill.OnFinished.AddListener(OnPillFinished);
		m_missionPill.OnFinished.AddListener(OnPillFinished);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from the OnFinished event on each pill
		m_progressionPill.OnFinished.RemoveListener(OnPillFinished);
		m_missionPill.OnFinished.RemoveListener(OnPillFinished);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this instance.
	/// </summary>
	public void Init() {
		// Disable all carousel elements
		m_progressionPill.animator.ForceHide(false);
		m_missionPill.animator.ForceHide(false);

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
	/// Launch the progression pill.
	/// Will interrupt current pills.
	/// </summary>
	public void DoProgression() {
		StartCoroutine(DoStep(Step.PROGRESSION));
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
		float pillAnimDuration = 0.25f;	// As setup in the ShowHideAnimator component
		switch(_step) {
			case Step.IDLE: {
				// Just hide current pill
				HideCurrentPill();
			} break;

			case Step.PROGRESSION: {
				// Hide current pill
				HideCurrentPill();

				// Show pill if required
				if(m_progressionPill.MustBeDisplayed()) {
					m_currentPill = m_progressionPill;
					m_currentPill.ShowAndAnimate(pillAnimDuration);	// Add enough delay to let the previous pill hide
				} else {
					// Pill doesn't have to be displayed, go to idle
					StartCoroutine(DoStep(Step.IDLE));
				}
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
						List<DefinitionNode> missionDefs = new List<DefinitionNode>();
						DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.MISSIONS, ref missionDefs);
						targetMission = new Mission();
						targetMission.InitFromDefinition(missionDefs.GetRandomValue());

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
					m_currentPill.ShowAndAnimate(pillAnimDuration);	// Add enough delay to let the previous pill hide
				} else {
					// Mission doesn't have to be displayed, check next mission or go to idle if last mission
					if(_step == Step.MISSION_2) {
						StartCoroutine(DoStep(Step.IDLE));
					} else {
						StartCoroutine(DoStep(_step + 1));
					}
				}
			} break;

			case Step.FINISHED: {
				// Hide current pill, unless it's the progression pill!
				if(m_currentPill != m_progressionPill) {
					HideCurrentPill();
				}

				// If possible, leave the progression pill visible, but don't animate it again!
				if(m_progressionPill.MustBeDisplayed()) {
					yield return new WaitForSeconds(pillAnimDuration);
					m_currentPill = m_progressionPill;
					m_progressionPill.animator.Show();	// Nothing will happen if already visible
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
			case Step.PROGRESSION: {
				// Go to idle
				StartCoroutine(DoStep(Step.IDLE));
			} break;

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

			case Step.IDLE:
			case Step.FINISHED: {
				// Do nothing (should never be here)
			} break;
		}
	}
}