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
using TMPro;

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
	[SerializeField] private TweenSequence m_coinsBonusAnimation = null;
	[SerializeField] private Localizer m_survivalBonusText = null;
	[SerializeField] private Transform m_coinsSpawnPoint = null;
	[Space]
	[SerializeField] private NumberTextAnimator m_coinsCounter = null;
	[SerializeField] private NumberTextAnimator m_pcCounter = null;
	[Space]
	[SerializeField] private GameObject m_adButtonsRoot = null;
	[SerializeField] private TextMeshProUGUI m_adButtonText = null;
	[SerializeField] private GameObject m_adButtonIcon = null;

	// Internal
	private ParticlesTrailFX m_coinsFX = null;
	
	private long m_totalCoinsReward = 0;    // Without Ad multiplier
	private float m_adCoinsMultiplier = 0f;
	private long m_adExtraCoins = 0;

	private bool m_adModifierEnabled = false;
	private bool m_removeAdsActive = false;

	private bool m_spamPreventer = false;
	private bool m_modifierApplied = false;

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

	protected override void DoInit() {
		// Internal vars
		m_totalCoinsReward = m_controller.coins + m_controller.survivalBonus;
		m_adCoinsMultiplier = RewardManager.rewardAdModifierSettings.watchAdCoinsMultiplier;
		m_adExtraCoins = (long)(Mathf.Ceil(m_totalCoinsReward * m_adCoinsMultiplier) - m_totalCoinsReward);
		m_spamPreventer = false;
		m_modifierApplied = false;

		// Is the Ad multiplier enabled?
		m_adModifierEnabled = RewardManager.rewardAdModifierSettings.isEnabled;
		m_adModifierEnabled &= m_adExtraCoins > 0;	// [AOC] Disable if no extra coins

		// Is this player a VIP?
		m_removeAdsActive = UsersManager.currentUser.removeAds.IsActive;
		m_removeAdsActive &= RewardManager.rewardAdModifierSettings.freeForVip;	// [AOC] If settings say VIP players have no privileges, consider this player not VIP for this feature

		// Only allow skipping if ad multiplier is not enabled
		m_skipAllowed = !m_adModifierEnabled;

		// Adapt some UI elements
		if(m_adButtonsRoot != null) {
			m_adButtonsRoot.SetActive(m_adModifierEnabled);
		}

		if(m_adButtonIcon != null) {
			m_adButtonIcon.SetActive(!m_removeAdsActive);	// Don't show icon if remove ads is purchased
		}
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

		// Set proper text to the ad multiply reward button
		if(m_adModifierEnabled && m_adButtonText != null) {
			// Different formatting options, defined in content
			string formatType = RewardManager.rewardAdModifierSettings.settingsDef.GetAsString("multiplierFormatting");
			string formattedText = "";
			switch(formatType) {
				case "multiplier": {
					formattedText = "x" + StringUtils.FormatNumber(m_adCoinsMultiplier, 2);	// x1.5
					formattedText = UIConstants.GetIconString(formattedText, UIConstants.IconType.COINS, UIConstants.IconAlignment.LEFT);  // Add coins icon
				} break;

				case "percentage": {
					formattedText = StringUtils.MultiplierToPercentageIncrease(m_adCoinsMultiplier, true); // +50%
					formattedText = UIConstants.GetIconString(formattedText, UIConstants.IconType.COINS, UIConstants.IconAlignment.LEFT);  // Add coins icon
				} break;

				case "extra_coins": {
					formattedText = UIConstants.GetIconString(m_adExtraCoins, UIConstants.IconType.COINS, UIConstants.IconAlignment.LEFT);  // Add coins icon
					if(m_adExtraCoins >= 0) formattedText = "+" + formattedText;	// +1000
				} break;

				case "total_coins": {
					formattedText = UIConstants.GetIconString(m_totalCoinsReward + m_adExtraCoins, UIConstants.IconType.COINS, UIConstants.IconAlignment.LEFT);  // Add coins icon
				} break;

				case "extra_coins_text": {
					formattedText = UIConstants.GetIconString(m_adExtraCoins, UIConstants.IconType.COINS, UIConstants.IconAlignment.LEFT);  // Add coins icon
					formattedText = LocalizationManager.SharedInstance.Localize("TID_REWARD_BONUS_1", formattedText);   // "Get 1000 extra!"
					formattedText = Localizer.ApplyCase(Localizer.Case.UPPER_CASE, formattedText);
				} break;

				case "total_coins_text": {
					formattedText = UIConstants.GetIconString(m_totalCoinsReward + m_adExtraCoins, UIConstants.IconType.COINS, UIConstants.IconAlignment.LEFT);  // Add coins icon
					formattedText = LocalizationManager.SharedInstance.Localize("TID_REWARD_BONUS_2", formattedText);   // "Make it 3000!"
					formattedText = Localizer.ApplyCase(Localizer.Case.UPPER_CASE, formattedText);
				} break;
			}

			// Set the text!
			m_adButtonText.text = formattedText;
		}
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

	/// <summary>
	/// Show the run duration in the summary
	/// </summary>
	override public void ShowSummary() {
		// Show time group
		m_controller.summary.ShowTime(m_controller.time, false);

		// Call parent
		base.ShowSummary();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Apply the reward multiplier!
	/// </summary>
	private void ApplyMultiplierAndContinue() {
		// Make sure we do it only once
		if(m_modifierApplied) return;
		m_modifierApplied = true;

		// Tell the scene controller
		m_controller.multipliedCoinsExtra = m_adExtraCoins;
		m_controller.totalCoins += m_adExtraCoins;

		// Update total amount text
		m_coinsText.SetValue(m_totalCoinsReward + m_adExtraCoins, true);

		// Trigger animation
		m_coinsBonusAnimation.Launch();

		// Continue with the animation sequence
		m_sequence.Play();
	}

	//------------------------------------------------------------------------//
	// ANIMATION CALLBACKS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether we need to pause to allow player to make a decision or not.
	/// </summary>
	public void OnCheckPause() {
		// Is the ad multiply feature enabled?
		if(m_adModifierEnabled) {
			// Yes! Pause the sequence to allow player to interact
			if(!m_modifierApplied) {	// Just in case, make sure we haven't applied the reward yet (spammers -_-)
				m_sequence.Pause();
			}
		}
	}

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
	public void OnDoSummary() {
		// Show coins
		// Have we multiplied the reward?
		long amountToDisplay = m_controller.coins + m_controller.survivalBonus;
		if(m_modifierApplied) {
			amountToDisplay += m_adExtraCoins;
		}

		// Do it!
		m_controller.summary.ShowCoins(amountToDisplay, m_modifierApplied);
	}

	/// <summary>
	/// Do an extra summary line for this step when in special dragons mode.
	/// </summary>
	public void OnDoSpecialDragonsSummary() {
		// Show collectibles summary :)
		ResultsScreenStepCollectibles collectiblesStep = m_controller.GetStep(ResultsScreenController.Step.COLLECTIBLES) as ResultsScreenStepCollectibles;
		if(collectiblesStep != null) {
			collectiblesStep.DoSummary();
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Player doesn't want to watch the ad :(
	/// </summary>
	public void OnContinueButton() {
		// Prevent spamming
		if(m_spamPreventer) return;
		m_spamPreventer = true;

		// Just keep going with the animation sequence
		m_sequence.Play();
	}

	/// <summary>
	/// The multiply reward button has been pressed.
	/// </summary>
	public void OnAdRewardMultiplyButton() {
		// Prevent spamming
		if(m_spamPreventer) return;
		if(m_modifierApplied) return;

		// Are we VIP?
		if(m_removeAdsActive) {
			// Give the reward directly
			ApplyMultiplierAndContinue();
		} else {
			// Trigger rewarded Ad
			m_spamPreventer = true;
			PopupAdBlocker.LaunchAd(true, GameAds.EAdPurpose.RUN_REWARD_MULTIPLIER, OnAdRewardCallback);
		}
	}

	/// <summary>
	/// A rewarded ad has finished.
	/// </summary>
	/// <param name="_success">Has the ad been successfully played?</param>
	public void OnAdRewardCallback(bool _success) {
		// Successful?
		if(_success) {
			// Yes! Apply multiplier
			ApplyMultiplierAndContinue();
		} else {
			// Unlock spam prevention to allow player to try again
			m_spamPreventer = false;
		}
	}
}