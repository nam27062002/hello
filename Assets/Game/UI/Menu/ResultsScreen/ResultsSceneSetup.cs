// ResultsSceneSetup.cs
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
	private static readonly bool TEST = false;

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
		// [AOC] TODO!! Pending new chests system
		List<Chest> collectedChests = new List<Chest>();
		if(TEST) {
			// [AOC] DEBUG ONLY!!
			int NUM_COLLECTED_CHESTS = Random.Range(0, 5);
			for(int i = 0; i < NUM_COLLECTED_CHESTS; i++) {
				collectedChests.Add(ChestManager.selectedChest);
			}
		} else {
			if(ChestManager.selectedChest != null && ChestManager.selectedChest.collected) {
				// [AOC] TODO!! 5 chests logic
				collectedChests.Add(ChestManager.selectedChest);
			}
		}

		// Show the chests in the center
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

		// Program animation of selected
		float totalDelay = 0f;
		for(int i = 0; i < sortedSlots.Count; i++) {
			StartCoroutine(
				AnimateChestWithDelay(sortedSlots[i], collectedChests[i], totalDelay)
			);
			totalDelay += 0.5f;
		}

		// Egg found?
		bool eggFound = false;
		if(TEST) {
			eggFound = (Random.Range(0f, 1f) > 0.5f);	// [AOC] DEBUG!!
		} else {
			eggFound = EggManager.collectibleEgg != null && EggManager.collectibleEgg.collected;
		}
		if(eggFound) {
			totalDelay += 1f;	// Extra delay
			m_eggSlot.localScale = Vector3.zero;
			m_eggSlot.DOScale(1f, 0.5f).SetDelay(totalDelay).SetEase(Ease.OutBack);
		} else {
			m_eggSlot.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Lauches the animation of the given chest slot with a specific chest data and delay.
	/// </summary>
	/// <param name="_slot">Slot to be animated.</param>
	/// <param name="_chest">Chest data.</param>
	/// <param name="_delay">Delay before launching the animation.</param>
	private IEnumerator AnimateChestWithDelay(ResultsSceneChestSlot _slot, Chest _chest, float _delay) {
		// Delay
		yield return new WaitForSeconds(_delay);

		// Do it!
		_slot.gameObject.SetActive(true);
		_slot.LaunchAnimation(_chest);
	}
}