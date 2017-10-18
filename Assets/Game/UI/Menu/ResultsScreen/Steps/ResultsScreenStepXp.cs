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
	[SerializeField] private TweenSequence m_sequence = null;

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
		m_sequence.OnFinished.AddListener(() => { OnFinished.Invoke(); });
	}

	/// <summary>
	/// Launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Hide "Tap to Continue"
		m_tapToContinue.ForceHide(false);

		// Launch show sequence - it should trigger the XP bar when adequate
		m_sequence.Launch();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// XP Bar animation has finished.
	/// </summary>
	private void OnXpBarAnimFinished() {
		// Show the "Tap to continue" except when a skin or dragon have been unlocked (go straight to next step in that case)
		if(m_controller.GetStep(ResultsScreenController.Step.DRAGON_UNLOCKED).MustBeDisplayed()
		|| m_controller.GetStep(ResultsScreenController.Step.SKIN_UNLOCKED).MustBeDisplayed()) {
			// Continue with the sequence
			m_sequence.Play();
		} else {
			// Show tap to continue
			m_tapToContinue.Show();
		}
	}

	/// <summary>
	/// The tap to continue button has been pressed.
	/// </summary>
	public void OnTapToContinue() {
		// Only if enabled! (to prevent spamming)
		// [AOC] Reuse visibility state to control whether tap to continue is enabled or not)
		if(!m_tapToContinue.visible) return;

		// Hide tap to continue to prevent spamming
		m_tapToContinue.Hide();

		// Continue with the sequence
		m_sequence.Play();
	}
}