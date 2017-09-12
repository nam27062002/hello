// ResultsScreenStepXp.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/09/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepXp : ResultsScreenStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private ResultsScreenXPBar m_xpBar = null;
	[SerializeField] private ShowHideAnimator m_tapToContinue = null;
	[Space]
	[SerializeField] private TweenSequence m_showSequence = null;
	[SerializeField] private TweenSequence m_hideSequence = null;

	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {
		return true;
	}

	/// <summary>
	/// Initialize this step.
	/// </summary>
	override protected void DoInit() {
		// Initialize XP bar
		m_xpBar.Init();

		// Listen to the xp bar anim finish
		m_xpBar.OnAnimationFinished.AddListener(OnXpBarAnimFinished);

		// Mark step as finished when finish animation ends
		m_hideSequence.OnFinished.AddListener(() => { OnFinished.Invoke(); });
	}

	/// <summary>
	/// Launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Hide "Tap to Continue"
		m_tapToContinue.ForceHide(false);

		// Launch show sequence - it should trigger the XP bar when adequate
		m_showSequence.Launch();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// XP Bar animation has finished.
	/// </summary>
	private void OnXpBarAnimFinished() {
		// If there aren't any more steps to be launched, show the "Tap to continue"
		ResultsScreenController_NEW.Step nextStep = m_controller.CheckNextStep();

		// Ignore TRACKING and APPLY_REWARDS steps
		while(nextStep == ResultsScreenController_NEW.Step.TRACKING
		   || nextStep == ResultsScreenController_NEW.Step.APPLY_REWARDS) {
			nextStep = m_controller.CheckNextStep(nextStep);
		}

		// Show tap to continue or auto-finish?
		if(nextStep == ResultsScreenController_NEW.Step.FINISHED) {
			m_tapToContinue.Show();
		} else {
			// Launch end sequence after a delay
			UbiBCN.CoroutineManager.DelayedCall(() => {
				m_hideSequence.Launch();
			}, 0.5f);
		}
	}

	/// <summary>
	/// The tap to continue button has been pressed.
	/// </summary>
	public void OnTapToContinue() {
		// Hide tap to continue to prevent spamming
		m_tapToContinue.Hide();

		// Launch end sequence
		m_hideSequence.Launch();
	}
}