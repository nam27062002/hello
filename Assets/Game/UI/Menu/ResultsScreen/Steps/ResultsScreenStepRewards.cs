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
public class ResultsScreenStepRewards : ResultsScreenSequenceStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Space]
	[SerializeField] private NumberTextAnimator m_coinsText = null;
	[SerializeField] private Localizer m_survivalBonusText = null;
	[SerializeField] private Transform m_coinsSpawnPoint = null;
	[Space]
	[SerializeField] private NumberTextAnimator m_coinsCounter = null;
	[SerializeField] private NumberTextAnimator m_pcCounter = null;

	// Internal
	private ParticlesTrailFX m_coinsFX = null;

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
	override protected void DoLaunch() {
		// Init currency counters
		m_coinsCounter.SetValue(m_controller.totalCoins, false);
		m_pcCounter.SetValue(m_controller.totalPc, false);

		// Update total coins
		m_controller.totalCoins += m_controller.coins + m_controller.survivalBonus;

		// Instantly set total amount of rewarded coins
		m_coinsText.SetValue(m_controller.coins + m_controller.survivalBonus, true);
		m_survivalBonusText.Localize(
			m_survivalBonusText.tid,
			StringUtils.FormatNumber(m_controller.survivalBonus)
		);
	}

	/// <summary>
	/// Called when skip is triggered.
	/// </summary>
	override protected void OnSkip() {
		// Instantly finish counter texts animations
		m_coinsCounter.SetValue(m_controller.totalCoins, false);

		// Kill transfer FX (if any)
		if(m_coinsFX != null) {
			m_coinsFX.KillFX();
			m_coinsFX = null;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Transfer coins from main screen to counter.
	/// </summary>
	public void OnCoinsTransfer() {
		// Update counter
		m_coinsCounter.SetValue(m_controller.totalCoins, true);

		// Show nice FX! (unless skipped)
		if(!m_skipped) {
			m_coinsFX = ParticlesTrailFX.LoadAndLaunch(
				ParticlesTrailFX.COINS,
				this.GetComponentInParent<Canvas>().transform,
				m_coinsSpawnPoint.position + new Vector3(0f, 0f, -0.5f),		// Offset Z so the coins don't collide with the UI elements
				m_coinsCounter.transform.position + new Vector3(0f, 0f, -0.5f)
			);
			m_coinsFX.totalDuration = m_coinsCounter.duration * 0.5f;	// Match the text animator duration (more or less)
			m_coinsFX.OnFinish.AddListener(() => { m_coinsFX = null; });
		}
	}

	/// <summary>
	/// Do the summary line for this step. Connect in the sequence.
	/// </summary>
	public void DoSummary() {
		// Show coins
		m_controller.summary.ShowCoins(m_controller.coins + m_controller.survivalBonus);
	}

	/// <summary>
	/// Do an extra summary line for this step when in special dragons mode.
	/// </summary>
	public void DoSpecialDragonsSummary() {
		// Ignore if not in special dragons mode
		if(SceneController.mode != SceneController.Mode.SPECIAL_DRAGONS) {
			return;
		}

		// Show collectibles summary :)
		ResultsScreenStepCollectibles collectiblesStep = m_controller.GetStep(ResultsScreenController.Step.COLLECTIBLES) as ResultsScreenStepCollectibles;
		if(collectiblesStep != null) {
			collectiblesStep.DoSummary();
		}
	}

	/// <summary>
	/// Show the run duration in the summary
	/// </summary>
	override public void ShowSummary() {
		// Only in Special Dragons mode
		if(SceneController.mode != SceneController.Mode.SPECIAL_DRAGONS) {
			return;
		}

		// Show time group
		m_controller.summary.ShowTime(m_controller.time, false);

		// Call parent
		base.ShowSummary();
	}
}