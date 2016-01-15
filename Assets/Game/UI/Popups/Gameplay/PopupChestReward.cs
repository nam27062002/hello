// PopupChestReward.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using UnityEngine.UI;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Chest reward popup at the end of a game.
/// For now we will give it quite a lot of control over the logic, but maybe 
/// some stuff should be done elsewhere.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupChestReward : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string PATH = "UI/Popups/Gameplay/PF_PopupChestReward";
	private static readonly int TAPS_TO_OPEN = 3;

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// References
	[SerializeField] private GameObject m_chest = null;

	// Internal logic
	private int m_chestTapCount = 0;
	private bool m_chestOpened = false;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	private void Awake() {
		Debug.Assert(m_chest != null, "Required field");
	}
	
	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Opens the chest.
	/// </summary>
	private void OpenChest() {
		// Tell the manager to generate a reward
		ChestManager.GenerateReward();

		// Hide tap message
		this.FindObjectRecursive("TextMessage").SetActive(false);

		// Trigger different effects based on reward type
		switch(ChestManager.rewardType) {
			case ChestManager.RewardType.COINS: {
				GameObject rewardObj = this.FindObjectRecursive("RewardCoins");
				rewardObj.SetActive(true);
				rewardObj.FindComponentRecursive<Text>("Text").text = Localization.Localize("+{0}", StringUtils.FormatNumber(ChestManager.rewardAmount));	// [AOC] HARDCODED!!
				rewardObj.GetComponent<DOTweenAnimation>().DOPlay();
			} break;

			case ChestManager.RewardType.PC: {
				GameObject rewardObj = this.FindObjectRecursive("RewardPC");
				rewardObj.SetActive(true);
				rewardObj.FindComponentRecursive<Text>("Text").text = Localization.Localize("+{0}", StringUtils.FormatNumber(ChestManager.rewardAmount));	// [AOC] HARDCODED!!
				rewardObj.GetComponent<DOTweenAnimation>().DOPlay();
			} break;

			case ChestManager.RewardType.BOOSTER: {
				GameObject rewardObj = this.FindObjectRecursive("RewardBooster");
				rewardObj.SetActive(true);
				rewardObj.FindComponentRecursive<Text>("Text").text = Localization.Localize("You got a booster!");	// [AOC] HARDCODED!!
				rewardObj.GetComponent<DOTweenAnimation>().DOPlay();
			} break;

			case ChestManager.RewardType.EGG: {
				GameObject rewardObj = this.FindObjectRecursive("RewardEgg");
				rewardObj.SetActive(true);
				rewardObj.FindComponentRecursive<Text>("Text").text = Localization.Localize("You got an egg!");	// [AOC] HARDCODED!!
				rewardObj.GetComponent<DOTweenAnimation>().DOPlay();
			} break;
		}

		// Launch chest effect
		m_chest.GetComponent<Animator>().SetTrigger("open");

		// [AOC] CHECK!! Do it here?
		// Give the reward right now
		ChestManager.ApplyReward();
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The chest has been tapped.
	/// </summary>
	public void OnChestTap() {
		// Ignore if tap threshold was already reached
		if(m_chestTapCount >= TAPS_TO_OPEN || m_chestOpened) return;

		// Increase counter
		m_chestTapCount++;

		// If tap threshold reached, open chest!
		// Otherwise just show some feedback
		if(m_chestTapCount >= TAPS_TO_OPEN) {
			OpenChest();
		} else {
			m_chest.GetComponent<Animator>().SetTrigger("tap");
		}
	}
}
