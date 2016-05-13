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
		INIT,
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
	[SerializeField] private ResultsScreenMissionPill m_missionPill = null;

	// Internal Logic
	private ResultsScreenCarouselPill m_currentPill = null;
	private Step m_step = Step.INIT;
	
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
	public void StartCarousel() {
		// Disable all carousel elements
		m_progressionPill.animator.ForceHide(false);
		m_missionPill.animator.ForceHide(false);

		// Reset current step
		m_step = Step.INIT;

		// Start!
		StartCoroutine(CheckNextStep());
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Checks the new step to be launched.
	/// </summary>
	private IEnumerator CheckNextStep() {
		// Skip if we've finished
		if(m_step == Step.FINISHED) yield return null;

		// Increase step
		m_step++;

		// Select and init target pill
		float pillAnimDuration = 0.25f;	// As setup in the ShowHideAnimator component
		switch(m_step) {
			case Step.PROGRESSION: {
				// Hide current pill (there shouldn't be any since it's the first step, but just in case)
				HideCurrentPill();

				// Show progress pill if required
				if(m_progressionPill.MustBeDisplayed()) {
					m_currentPill = m_progressionPill;
					m_currentPill.ShowAndAnimate(pillAnimDuration);	// Add enough delay to let the previous pill hide
				} else {
					StartCoroutine(CheckNextStep());
				}
			} break;

			case Step.MISSION_0:
			case Step.MISSION_1:
			case Step.MISSION_2: {
				// Set mission difficulty to check whether it must be displayed or not
				m_missionPill.difficulty = (Mission.Difficulty)(m_step - Step.MISSION_0);	// Quick correlation between Step and Mission.Difficulty enums
				if(m_missionPill.MustBeDisplayed()) {
					// Hide current pill
					HideCurrentPill();

					// Show mission pill
					m_currentPill = m_missionPill;
					m_currentPill.ShowAndAnimate(pillAnimDuration);	// Add enough delay to let the previous pill hide
				} else {
					StartCoroutine(CheckNextStep());
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
					m_progressionPill.animator.Show();
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
		// Just check next step
		StartCoroutine(CheckNextStep());
	}
}