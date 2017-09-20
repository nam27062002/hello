﻿// ResultsSceneSetup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/09/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// PREPROCESSOR																  //
//----------------------------------------------------------------------------//
// [AOC] Chests and eggs are no longer in the 3D scene with the new results screen
//		 Keep the code just in case
//#define SHOW_COLLECTIBLES

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

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
	public Camera camera {
		get { return m_camera; }
	}

	[Comment("DragonLoader should be set to \"CURRENT\" mode", 10)]
	[SerializeField] private MenuDragonLoader m_dragonSlot = null;
	[SerializeField] private Transform m_dragonSlotViewPosition = null;
	[SerializeField] private Transform m_eggSlot = null;
	[SerializeField] private ParticleSystem m_confettiFX = null;

	[Comment("Sort chest slots from left to right, chests will be spawned from the center depending on how many were collected.\nAlways 5 slots, please.", 10)]
	[SerializeField] private ResultsSceneChestSlot[] m_chestSlots = new ResultsSceneChestSlot[5];

	[Comment("Fog Settings used", 10)]
	[SerializeField] FogManager.FogAttributes m_fog;

	// Internal
	private List<ResultsSceneChestSlot> m_rewardedSlots = new List<ResultsSceneChestSlot>();	// The slots that we'll be actually using, sorted in order of appereance
	private bool m_eggFound = false;

	/*
	// Test To recolocate the dragons view!
	public bool recolocate = false; //"run" or "generate" for example
	void Update()
	{
		if (recolocate)
		{
			m_dragonSlot.SetViewPosition( m_dragonSlotViewPosition.position );
			m_dragonSlot.dragonInstance.transform.rotation = m_dragonSlot.transform.rotation;
			if ( m_dragonSlot.dragonSku == "dragon_chinese" || m_dragonSlot.dragonSku == "dragon_reptile" || m_dragonSlot.dragonSku == "dragon_balrog")
			{
				m_dragonSlot.dragonInstance.transform.Rotate(Vector3.up * -45);
			}
		}
	}
	*/

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Hide dragon slot
		m_dragonSlot.gameObject.SetActive(false);

		if ( InstanceManager.fogManager != null )
			InstanceManager.fogManager.ForceAttributes( m_fog );
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
	/// Initialize the scene and leave it ready for the LaunchAnim() call.
	/// </summary>
	public void Init() {
		#if SHOW_COLLECTIBLES
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
					m_chestSlots[i].InitFromChest(sortedChests[i], ChestManager.GetRewardData(i + 1));
					if(sortedChests[i].state == Chest.State.PENDING_REWARD) {
						m_rewardedSlots.Add(m_chestSlots[i]);
					}
				}
			} break;
		}

		// Egg found?
		m_eggFound = false;
		switch(CPResultsScreenTest.eggMode) {
			case CPResultsScreenTest.EggTestMode.FOUND: {
				m_eggFound = true; 
			} break;

			case CPResultsScreenTest.EggTestMode.NOT_FOUND: {
				m_eggFound = false; 
			} break;

			case CPResultsScreenTest.EggTestMode.RANDOM: {
				m_eggFound = (Random.Range(0f, 1f) > 0.5f); 
			} break;

			case CPResultsScreenTest.EggTestMode.NONE: {
				m_eggFound = CollectiblesManager.egg != null && CollectiblesManager.egg.collected;
			} break;
		}

		// Hide egg slot
		m_eggSlot.gameObject.SetActive(m_eggFound);
		#endif


	}

	/// <summary>
	/// Launches the dragon intro animation.
	/// </summary>
	public void LaunchDragonAnim() {
		// Launch gold mountain animation

		// Show and trigger dragon animation
		m_dragonSlot.gameObject.SetActive(true);
		m_dragonSlot.dragonInstance.SetAnim(MenuDragonPreview.Anim.RESULTS_IN);
		m_dragonSlot.SetViewPosition( m_dragonSlotViewPosition.position );
		m_dragonSlot.dragonInstance.transform.rotation = m_dragonSlot.transform.rotation;
		if ( m_dragonSlot.dragonSku == "dragon_chinese" || m_dragonSlot.dragonSku == "dragon_reptile" || m_dragonSlot.dragonSku == "dragon_balrog")
		{
			m_dragonSlot.dragonInstance.transform.Rotate(Vector3.up * -45);
		}

		// Trigger confetti anim
		LaunchConfettiFX();
	}

	/// <summary>
	/// Launches the disguise purchased FX on the selected dragon.
	/// </summary>
	public void LaunchConfettiFX() {
		// Restart effect
		m_confettiFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		m_confettiFX.Play(true);

		// Restart SFX
		string audioId = "hd_unlock_dragon";
		AudioController.Stop(audioId);
		AudioController.Play(audioId);
	}
}