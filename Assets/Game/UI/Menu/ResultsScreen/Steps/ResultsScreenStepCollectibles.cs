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

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepCollectibles : ResultsScreenStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private ShowHideAnimator m_eggFoundAnim = null;
	[SerializeField] private ShowHideAnimator m_eggNotFoundAnim = null;
	[SerializeField] private ResultsSceneChestSlot[] m_chestSlots = new ResultsSceneChestSlot[5];
	[Space]
	[SerializeField] private NumberTextAnimator m_coinsCounter = null;
	[SerializeField] private NumberTextAnimator m_pcCounter = null;
	[Space]
	[SerializeField] private ShowHideAnimator m_tapToContinue = null;
	[SerializeField] private TweenSequence m_sequence = null;

	// Internal
	private int m_checkedChests = 0;
	private List<ResultsSceneChestSlot> m_rewardedSlots = new List<ResultsSceneChestSlot>();	// The slots that we'll be actually using, sorted in order of appereance

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
		// Reset internal vars
		m_checkedChests = 0;

		// Initialize chests. Can't do it in the DoInit call because we need the chest slots to be active!
		InitChests();

		// Hide both egg anims
		m_eggFoundAnim.ForceHide(false);
		m_eggNotFoundAnim.ForceHide(false);

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
	/// Perform all required initializations for the chest slots.
	/// </summary>
	private void InitChests() {
		// How many chests?
		int preCollectedChests = 0;
		int pendingChests = 0;
		int collectedAndPendingChests = 0;
		List<Chest> sortedChests = new List<Chest>();
		List<Chest> sourceChests = ChestManager.sortedChests;
		if(CPResultsScreenTest.chestsMode == CPResultsScreenTest.ChestTestMode.NONE) {
			// Real logic
			for(int i = 0; i < sourceChests.Count; i++) {
				sortedChests.Add(sourceChests[i]);
				switch(sourceChests[i].state) {
					case Chest.State.COLLECTED: {
						preCollectedChests++; 
					} break;

					case Chest.State.PENDING_REWARD: {
						pendingChests++; 
					}break;
				}
			}
			collectedAndPendingChests = preCollectedChests + pendingChests;
		} else {
			// [AOC] DEBUG ONLY!!
			pendingChests = CPResultsScreenTest.chestsMode - CPResultsScreenTest.ChestTestMode.FIXED_0;
			if(CPResultsScreenTest.chestsMode == CPResultsScreenTest.ChestTestMode.RANDOM) {
				pendingChests = Random.Range(0, 5);
			}

			// Adjust number of previously collected chests to prevent overflowing max chests
			preCollectedChests = ChestManager.collectedChests;
			collectedAndPendingChests = preCollectedChests + pendingChests;
			if(collectedAndPendingChests > ChestManager.NUM_DAILY_CHESTS) {
				collectedAndPendingChests = ChestManager.NUM_DAILY_CHESTS;
				preCollectedChests = collectedAndPendingChests - pendingChests;
			}

			// Either reuse actual chest or create a fake new one
			for(int i = 0; i < sourceChests.Count; i++) {
				if(i < preCollectedChests) {
					sortedChests.Add(sourceChests[i]);
				} else if(i < collectedAndPendingChests) {
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
		// Testing different layouts!
		switch(CPResultsScreenTest.chestsLayout) {
			// Option A) Show the chests in the center, left to right
			case CPResultsScreenTest.ChestsLayout.ONLY_COLLECTED_CHESTS: {
				m_rewardedSlots.Clear();
				int startIdx = (Mathf.CeilToInt(m_chestSlots.Length/2f) - Mathf.FloorToInt(pendingChests/2f)) - 1;	// -1 for 0-based index
				int endIdx = startIdx + pendingChests;
				int chestIdx = preCollectedChests + 1;
				for(int i = startIdx; i < endIdx; i++) {
					m_rewardedSlots.Add(m_chestSlots[i]);
					m_chestSlots[i].InitFromChest(sortedChests[chestIdx], ChestManager.GetRewardData(chestIdx + 1));
				}

				// Hide all slots
				for(int i = 0; i < m_chestSlots.Length; i++) {
					m_chestSlots[i].gameObject.SetActive(false);
				}
			} break;

			// Option B) Show the daily chest progression, linear order (0-1-2-3-4) left to right
			case CPResultsScreenTest.ChestsLayout.FULL_PROGRESSION: {
				// Using all chest slots
				m_rewardedSlots.Clear();
				for(int i = 0; i < m_chestSlots.Length; i++) {
					// Initialize based on chest state
					Debug.Log("<color=red>Initializing chest " + i + ": " + sortedChests[i].state + "</color>");
					m_chestSlots[i].InitFromChest(sortedChests[i], ChestManager.GetRewardData(i + 1));
					if(sortedChests[i].state == Chest.State.PENDING_REWARD) {
						m_rewardedSlots.Add(m_chestSlots[i]);
					}
				}
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A chest has entered, show its reward if appliable.
	/// </summary>
	public void OnChestRewardCheck() {
		// Does the current chest have a reward?
		ResultsSceneChestSlot currentChest = m_chestSlots[m_checkedChests];
		if(m_rewardedSlots.Contains(currentChest)) {
			// Display reward animation!
			currentChest.LaunchRewardAnim();

			// Increase reward counters
			// PC or coins?
			switch(currentChest.rewardType) {
				case Chest.RewardType.SC: {
					// Update total rewarded coins and update counter
					m_controller.totalCoins += currentChest.rewardData.amount;
					m_coinsCounter.SetValue(m_controller.totalCoins, true);
				} break;

				case Chest.RewardType.PC: {
					// Update total rewarded coins and update counter
					m_controller.totalPc += currentChest.rewardData.amount;
					m_pcCounter.SetValue(m_controller.totalPc, true);
				} break;
			}
		}

		// Increase counter
		m_checkedChests++;
	}

	/// <summary>
	/// Trigger the rewarded egg anim (if needed).
	/// </summary>
	public void OnEggRewardCheck() {
		if(m_controller.eggFound) {
			m_eggFoundAnim.ForceShow();
		} else {
			m_eggNotFoundAnim.ForceShow();
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

		// Launch end sequence
		m_sequence.Play();
	}
}