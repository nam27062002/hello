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

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single chest slot controller in the results scene.
/// </summary>
[RequireComponent(typeof(Animator))]
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
	[SerializeField] private ChestViewController m_chest = null;
	[SerializeField] private ParticleSystem m_dustPS = null;

	// Internal
	private GameObject m_rewardObj = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_chest.OnChestOpen.AddListener(OnChestOpened);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Setup and start animation with the given chest info.
	/// </summary>
	/// <param name="_chest">The chest to be displayed.</param>
	public void LaunchAnimation(Chest _chest) {
		// [AOC] TODO!! Gather the reward for this chest and instantiate the appropiate reward prefab
		bool coins = Random.Range(0f, 1f) < 0.75f ? true : false;
		string rewardPrefabPath = coins ? COINS_REWARD_PREFAB : PC_REWARD_PREFAB;	// [AOC] Simulate variety
		GameObject rewardPrefab = Resources.Load<GameObject>(rewardPrefabPath);
		m_rewardObj = GameObject.Instantiate<GameObject>(rewardPrefab);
		m_rewardObj.transform.SetParent(this.transform, false);
		m_rewardObj.SetActive(false);

		// Set text
		TextMesh text = m_rewardObj.FindComponentRecursive<TextMesh>();
		if(text != null) {
			if(coins) {
				int[] values = {50, 200, 500, 1000};
				text.text = "+" + StringUtils.FormatNumber(values.GetRandomValue());
			} else {
				text.text = "+" + 5.ToString();
			}
		}

		// Launch animation sequence
		GetComponent<Animator>().SetTrigger("in");
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Event to sync with the open animation.
	/// </summary>
	public void OnChestOpened() {
		// Launch reward animation!
		m_rewardObj.SetActive(true);
		m_rewardObj.GetComponent<Animator>().SetTrigger("in");
	}

	/// <summary>
	/// Event to sync with the animation.
	/// </summary>
	public void OnChestLanded() {
		// Launch the open animation
		m_chest.Open();

		// [AOC] TODO!! Play some SFX

		// Play some VFX
		m_dustPS.Play();
	}

	/// <summary>
	/// Event to sync with the animation.
	/// </summary>
	public void OnCameraShake() {
		Messenger.Broadcast<float, float>(GameEvents.CAMERA_SHAKE, 0.1f, 0.5f);
	}
}