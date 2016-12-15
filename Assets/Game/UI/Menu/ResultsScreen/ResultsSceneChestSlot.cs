﻿// ResultsSceneChestSlot.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/09/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single chest slot controller in the results scene.
/// </summary>
public class ResultsSceneChestSlot : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private static readonly string COINS_REWARD_PREFAB = "UI/Metagame/Rewards/PF_ChestCoinsReward";
	private static readonly string PC_REWARD_PREFAB = "UI/Metagame/Rewards/PF_ChestPCReward";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references, all required

	// Internal
	private ChestViewController m_chest = null;
	private GameObject m_rewardObj = null;
	private Chest.RewardData m_rewardData = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Remove placeholder
		this.transform.DestroyAllChildren(false);

		// Instantiate the actual chest
		GameObject chestPrefab = Resources.Load<GameObject>(ChestViewController.PREFAB_PATH);
		GameObject chestObj = GameObject.Instantiate<GameObject>(chestPrefab);
		chestObj.transform.SetParent(this.transform, false);
		m_chest = chestObj.GetComponentInChildren<ChestViewController>();

		// Subscribe to chest events
		m_chest.OnChestOpen.AddListener(OnChestOpened);
		m_chest.OnChestAnimLanded.AddListener(OnChestLanded);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Setup and start animation with the given chest info.
	/// </summary>
	/// <param name="_chestRewardData">Data to be displayed.</param>
	public void LaunchAnimation(Chest.RewardData _chestRewardData) {
		// Skip reward setup if def is not valid
		if(_chestRewardData != null) {
			// Aux vars
			string rewardPrefabPath = COINS_REWARD_PREFAB;	// [AOC] Let's show coins by default (debug purposes)

			// PC or coins?
			switch(_chestRewardData.type) {
				case Chest.RewardType.SC: {
					rewardPrefabPath = COINS_REWARD_PREFAB;
				} break;

				case Chest.RewardType.PC: {
					rewardPrefabPath = PC_REWARD_PREFAB;
				} break;
			}

			// Load and instantiate reward prefab
			GameObject rewardPrefab = Resources.Load<GameObject>(rewardPrefabPath);
			m_rewardObj = GameObject.Instantiate<GameObject>(rewardPrefab);
			m_rewardObj.transform.SetParent(this.transform, false);
			m_rewardObj.SetActive(false);

			// Set text
			TextMeshPro text = m_rewardObj.FindComponentRecursive<TextMeshPro>();
			if(text != null) {
				// Set formatted text
				text.text = "+" + StringUtils.FormatNumber(_chestRewardData.amount);

				// Make it look to parent camera
				ResultsSceneSetup parentScene = GetComponentInParent<ResultsSceneSetup>();
				LookAt lookAt = text.GetComponent<LookAt>();
				if(lookAt != null && parentScene != null) {
					lookAt.lookAtObject = parentScene.camera.transform;
				}
			}
		}

		// Store data for further use
		m_rewardData = _chestRewardData;

		// Launch animation sequence
		m_chest.ResultsAnim();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Event to sync with the open animation.
	/// </summary>
	public void OnChestOpened() {
		// Launch reward animation!
		if(m_rewardObj != null) {
			m_rewardObj.SetActive(true);
			m_rewardObj.GetComponent<Animator>().SetTrigger("in");
		}
	}

	/// <summary>
	/// Event to sync with the animation.
	/// </summary>
	public void OnChestLanded() {
		// Launch the open animation
		m_chest.Open(m_rewardData != null ? m_rewardData.type : Chest.RewardType.SC);
	}
}