// ResultsScreenStepCollectibles.cs
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
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepCollectibles : ResultsScreenSequenceStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Space]
	[SerializeField] private string[] m_chestsSFX = new string[5];
	[Space]
	[SerializeField] private NumberTextAnimator m_coinsCounter = null;
	[SerializeField] private NumberTextAnimator m_pcCounter = null;

	// Internal
	private int m_collectedChests = 0;
	private int m_pendingRewardChests = 0;
	private int m_checkedChests = 0;
	private ResultsSceneChestSlot[] m_chestSlots = null;
	private List<ResultsSceneChestSlot> m_rewardedSlots = new List<ResultsSceneChestSlot>();	// The slots that we'll be actually using, sorted in order of appereance

	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {
		// Never during FTUX
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_CHESTS_AT_RUN) return false;


		return true;
	}

	/// <summary>
	/// Initialize the step.
	/// </summary>
	override protected void DoInit() {
		// Grab slots from the 3D scene
		m_chestSlots = new ResultsSceneChestSlot[m_controller.scene.chestSlots.Length];
		System.Array.Copy(m_controller.scene.chestSlots, m_chestSlots, m_chestSlots.Length);

		// Reset internal vars
		m_collectedChests = 0;
		m_pendingRewardChests = 0;
		m_checkedChests = 0;

		// Initialize chests.
		InitChests();

		// Initialize egg
		// Start with egg hidden
		m_controller.scene.eggSlot.gameObject.SetActive(false);
	}

	/// <summary>
	/// Initialize and launch this step.
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
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Perform all required initializations for the chest slots.
	/// </summary>
	private void InitChests() {

		// How many chests?
		int collectedAndPending = 0;
		List<Chest> sortedChests = new List<Chest>();
		List<Chest> sourceChests = ChestManager.sortedChests;
		if(CPResultsScreenTest.chestsMode == CPResultsScreenTest.ChestTestMode.NONE) {
			// Real logic
			for(int i = 0; i < sourceChests.Count; i++) {
				sortedChests.Add(sourceChests[i]);
				switch(sourceChests[i].state) {
					case Chest.State.COLLECTED: {
						m_collectedChests++; 
					} break;

					case Chest.State.PENDING_REWARD: {
						m_pendingRewardChests++; 
					}break;
				}
			}
			collectedAndPending = m_collectedChests + m_pendingRewardChests;
		} else {
			// [AOC] DEBUG ONLY!!
			m_pendingRewardChests = CPResultsScreenTest.chestsMode - CPResultsScreenTest.ChestTestMode.FIXED_0;
			if(CPResultsScreenTest.chestsMode == CPResultsScreenTest.ChestTestMode.RANDOM) {
				m_pendingRewardChests = Random.Range(0, 5);
			}

			// Adjust number of previously collected chests to prevent overflowing max chests
			m_collectedChests = ChestManager.collectedChests;
			collectedAndPending = m_collectedChests + m_pendingRewardChests;
			if(collectedAndPending > ChestManager.NUM_DAILY_CHESTS) {
				collectedAndPending = ChestManager.NUM_DAILY_CHESTS;
				m_collectedChests = collectedAndPending - m_pendingRewardChests;
			}

			// Either reuse actual chest or create a fake new one
			for(int i = 0; i < sourceChests.Count; i++) {
				if(i < m_collectedChests) {
					sortedChests.Add(sourceChests[i]);
				} else if(i < collectedAndPending) {
					Chest newChest = new Chest();
					newChest.ChangeState(Chest.State.PENDING_REWARD);
					sortedChests.Add(newChest);
				} else {
					Chest newChest = new Chest();
					newChest.ChangeState(Chest.State.NOT_COLLECTED);
					sortedChests.Add(newChest);
				}
			}
		}

		// Initialize chest slots
		// Show the daily chest progression, linear order (0-1-2-3-4) left to right
		m_rewardedSlots.Clear();
		for(int i = 0; i < m_chestSlots.Length; i++) {
			// Initialize based on chest state
			//Debug.Log("<color=orange>Initializing chest " + i + ": " + sortedChests[i].state + "</color>");
			m_chestSlots[i].InitFromChest(sortedChests[i], ChestManager.GetRewardData(i + 1));
			if(sortedChests[i].state == Chest.State.PENDING_REWARD) {
				m_rewardedSlots.Add(m_chestSlots[i]);
			}

			// Start hidden if not yet collected
			m_chestSlots[i].gameObject.SetActive(m_chestSlots[i].chest.state == Chest.State.COLLECTED);
		}

		// Start animating at the first non-collected chest
		m_checkedChests = m_collectedChests;

		// Assign SFX to non-collected chests
		for(int i = m_collectedChests; i < m_chestSlots.Length; ++i) {
			m_chestSlots[i].chestView.resultsSFX = m_chestsSFX[i - m_collectedChests];
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A chest has entered, show its reward if appliable.
	/// </summary>
	public void OnChestRewardCheck() {
		// Until we've checked all the chests!
		if(m_checkedChests >= m_chestSlots.Length) return;

		// Does the current chest have a reward?
		ResultsSceneChestSlot targetChest = m_chestSlots[m_checkedChests];
		if(m_rewardedSlots.Contains(targetChest)) {
			// Increase reward counters
			// PC or coins?
			switch(targetChest.rewardType) {
				case Chest.RewardType.SC: {
					// Update total rewarded coins and update counter
					m_controller.totalCoins += targetChest.rewardData.amount;
					m_coinsCounter.SetValue(m_controller.totalCoins, true);
				} break;

				case Chest.RewardType.PC: {
					// Update total rewarded pc and update counter
					m_controller.totalPc += targetChest.rewardData.amount;
					m_pcCounter.SetValue(m_controller.totalPc, true);
				} break;

			}

			// Update counters
			m_collectedChests++;
			m_pendingRewardChests--;
		}

		// Trigger animation!
		targetChest.gameObject.SetActive(true);
		targetChest.LaunchResultsAnim(true);

		// Increase counter
		m_checkedChests++;
	}

	/// <summary>
	/// Trigger the rewarded egg anim (if needed).
	/// </summary>
	public void OnEggRewardCheck() {
		if(m_controller.eggFound) {
			// Trigger animation!
			m_controller.scene.eggSlot.gameObject.SetActive(true);
			m_controller.scene.eggSlot.LaunchResultsAnim();
		}
	}

	/// <summary>
	/// Do the summary line for this step. Connect in the sequence.
	/// </summary>
	public void DoSummary() {
		// Aux vars
		int numChests = m_collectedChests;
		int numEggs = m_controller.eggFound ? 1 : 0;

		// Do summary!
		m_controller.summary.ShowCollectibles(numChests, numEggs);
	}

	/// <summary>
	/// Hide the given rewards.
	/// </summary>
	public void HideRewards() {
		for(int i = 0; i < m_chestSlots.Length; ++i) {
			m_chestSlots[i].HideResultsReward();
		}
	}
}