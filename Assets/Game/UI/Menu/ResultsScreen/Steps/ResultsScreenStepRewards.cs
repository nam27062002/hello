// ResultsScreenStepRewards.cs
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
public class ResultsScreenStepRewards : ResultsScreenStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private NumberTextAnimator m_coinsText = null;
	[Space]
	[SerializeField] private ShowHideAnimator m_tapToContinue = null;
	[SerializeField] private TweenSequence m_sequence = null;
	[Space]
	[SerializeField] private NumberTextAnimator m_coinsCounter = null;
	[SerializeField] private NumberTextAnimator m_pcCounter = null;

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
	/// Initialize and launch this step.
	/// </summary>
	override protected void DoInit() {
		// Notify when sequence is finished
		m_sequence.OnFinished.AddListener(() => OnFinished.Invoke());
	}

	/// <summary>
	/// Initialize and launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Init currency counters
		m_coinsCounter.SetValue(m_controller.totalCoins, false);
		m_pcCounter.SetValue(m_controller.totalPc, false);

		// Instantly set total amount of rewarded coins
		m_coinsText.SetValue(m_controller.coins + m_controller.survivalBonus, false);

		// Hide tap to continue
		m_tapToContinue.ForceHide(false);

		// Launch sequence!
		m_sequence.Launch();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Transfer coins from main screen to counter.
	/// </summary>
	public void OnCoinsTransfer() {
		// Update total rewarded coins and update counter
		m_controller.totalCoins += m_controller.coins + m_controller.survivalBonus;
		m_coinsCounter.SetValue(m_controller.totalCoins, true);

		// Show nice FX!
		CurrencyTransferFX fx = CurrencyTransferFX.LoadAndLaunch(
			CurrencyTransferFX.COINS,
			this.GetComponentInParent<Canvas>().transform,
			m_coinsText.transform.position + new Vector3(0f, 0f, -0.5f),		// Offset Z so the coins don't collide with the UI elements
			m_coinsCounter.transform.position + new Vector3(0f, 0f, -0.5f)
		);
		fx.totalDuration = m_coinsCounter.duration;	// Match the text animator duration (more or less)
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

		// Launch end sequence
		m_sequence.Play();
	}
}