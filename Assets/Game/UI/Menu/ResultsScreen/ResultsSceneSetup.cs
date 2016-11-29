﻿// ResultsSceneSetup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/09/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Setup to define a 3D area in the level to use for the results screen.
/// </summary>
public class ResultsSceneSetup : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references, all required
	[SerializeField] private Camera m_camera = null;

	[Comment("DragonLoader should be set to \"CURRENT\" mode", 10)]
	[SerializeField] private MenuDragonLoader m_dragonSlot = null;
	[SerializeField] private Transform m_eggSlot = null;

	[Comment("Sort chest slots from left to right, chests will be spawned from the center depending on how many were collected.\nAlways 5 slots, please.", 10)]
	[SerializeField] private ResultsSceneChestSlot[] m_chestSlots = new ResultsSceneChestSlot[5];

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// A change has occurred on the inspector. Validate its values.
	/// </summary>
	private void OnValidate() {
		// There must be exactly 5 chest slots
		if(m_chestSlots.Length != 5) {
			// Create a new array with exactly 5 slots and copy as many values as we can
			ResultsSceneChestSlot[] chestSlots = new ResultsSceneChestSlot[5];
			for(int i = 0; i < m_chestSlots.Length && i < chestSlots.Length; i++) {
				chestSlots[i] = m_chestSlots[i];
			}
			m_chestSlots = chestSlots;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Setup and launch results animation based on current game stats (RewardManager, etc.).
	/// </summary>
	public void LaunchAnim() {
		// How many chests?
		List<Chest> collectedChests = new List<Chest>();
		if(CPResultsScreenTest.chestsMode == CPResultsScreenTest.ChestTestMode.NONE) {
			// Real logic
			// Grab all the chests in the REWARD_PENDING state
			for(int i = 0; i < ChestManager.dailyChests.Length; i++) {
				if(ChestManager.dailyChests[i].state == Chest.State.PENDING_REWARD) {
					collectedChests.Add(ChestManager.dailyChests[i]);
				}
			}
		} else {
			// [AOC] DEBUG ONLY!!
			int numCollectedChests = (int)CPResultsScreenTest.chestsMode;
			numCollectedChests -= 1;	// enum starts at 1
			if(CPResultsScreenTest.chestsMode == CPResultsScreenTest.ChestTestMode.RANDOM) {
				numCollectedChests = Random.Range(0, 5);
			}

			for(int i = 0; i < numCollectedChests; i++) {
				Chest newChest = new Chest();
				newChest.ChangeState(Chest.State.PENDING_REWARD);
				collectedChests.Add(newChest);
			}
		}

		// Show the chests in the center, left to right
		// Another option (check with design/UI) would be to show the daily chest progression, 
		// in which case order should be linear (0-1-2-3-4) and probably the chest layout 
		// different to give more relevance to latest chests (maybe some different asset?)
		List<ResultsSceneChestSlot> sortedSlots = new List<ResultsSceneChestSlot>();
		int startIdx = (Mathf.CeilToInt(m_chestSlots.Length/2f) - Mathf.FloorToInt(collectedChests.Count/2f)) - 1;	// -1 for 0-based index
		int endIdx = startIdx + collectedChests.Count;
		for(int i = startIdx; i < endIdx; i++) {
			sortedSlots.Add(m_chestSlots[i]);
		}

		// Hide all slots
		for(int i = 0; i < m_chestSlots.Length; i++) {
			m_chestSlots[i].gameObject.SetActive(false);
		}

		// Program animation of selected slots
		float totalDelay = 0f;
		for(int i = 0; i < sortedSlots.Count; i++) {
			// Get reward definition corresponding to this chest
			int chestIdx = RewardManager.initialCollectedChests + i + 1;
			if(CPResultsScreenTest.chestsMode != CPResultsScreenTest.ChestTestMode.NONE) {
				chestIdx = i + 1;
			}
			Chest.RewardData rewardData = ChestManager.GetRewardData(chestIdx);

			// Launch with delay
			StartCoroutine(
				AnimateChestWithDelay(sortedSlots[i], rewardData, totalDelay)
			);
			totalDelay += 0.15f;
		}

		// Egg found?
		bool eggFound = false;
		switch(CPResultsScreenTest.eggMode) {
			case CPResultsScreenTest.EggTestMode.FOUND: {
				eggFound = true; 
			} break;

			case CPResultsScreenTest.EggTestMode.NOT_FOUND: {
				eggFound = false; 
			} break;

			case CPResultsScreenTest.EggTestMode.RANDOM: {
				eggFound = (Random.Range(0f, 1f) > 0.5f); 
			} break;

			case CPResultsScreenTest.EggTestMode.NONE: {
				eggFound = EggManager.collectibleEgg != null && EggManager.collectibleEgg.collected;
			} break;
		}

		if(eggFound) {
			totalDelay += 0.5f;	// Extra delay
			m_eggSlot.localScale = Vector3.zero;
			m_eggSlot.DOScale(1f, 0.5f).SetDelay(totalDelay).SetEase(Ease.OutBack);
		} else {
			m_eggSlot.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Lauches the animation of the given chest slot with a specific chest reward data and delay.
	/// </summary>
	/// <param name="_slot">Slot to be animated.</param>
	/// <param name="_chestRewardData">Chest reward data.</param>
	/// <param name="_delay">Delay before launching the animation.</param>
	private IEnumerator AnimateChestWithDelay(ResultsSceneChestSlot _slot, Chest.RewardData _chestRewardData, float _delay) {
		// Delay
		yield return new WaitForSeconds(_delay);

		// Do it!
		_slot.gameObject.SetActive(true);
		_slot.LaunchAnimation(_chestRewardData);
	}
}