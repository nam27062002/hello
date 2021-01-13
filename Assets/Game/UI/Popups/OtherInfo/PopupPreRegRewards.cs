// PopupPreRegRewards.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/07/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

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
/// Controller for the Pre-registration rewards popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupPreRegRewards : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/OtherInfo/PF_PopupPreRegRewards";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private MetagameRewardView m_petRewardSlot = null;
	[SerializeField] private MetagameRewardView m_egg1RewardSlot = null;
	[SerializeField] private MetagameRewardView m_egg2RewardSlot = null;
	[SerializeField] private MetagameRewardView m_gfRewardSlot = null;
	[SerializeField] private MetagameRewardView m_pcRewardSlot = null;
	[SerializeField] private MetagameRewardView m_scRewardSlot = null;

	// Internal
	private List<Metagame.Reward> m_rewards = null;
	private bool m_collected = false;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Gather reward definitions and group them by type
		// [AOC] We want to group by type but also sku! (we don't want different pets grouped in the same reward slot)
		//		 To achieve that we will use a Dictionary whose key is an anonymous object combining type and sku.
		//		 See https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/anonymous-types
		//		 See https://stackoverflow.com/questions/26423896/initializing-a-dictionary-with-anonymous-objects
		List<DefinitionNode> rewardDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.PREREG_REWARDS);
		Dictionary<object, Metagame.Reward.Data> datas = new Dictionary<object, Metagame.Reward.Data>();
		for(int i = 0; i < rewardDefs.Count; ++i) {
			// Figure out key for this def
			var key = new { type = rewardDefs[i].GetAsString("type"), sku = rewardDefs[i].GetAsString("rewardSku") };

			// If there is no entry for this key, add a new one
			if(!datas.ContainsKey(key)) {
				// Create new data object
				Metagame.Reward.Data newData = new Metagame.Reward.Data();
				newData.typeCode = key.type;
				newData.sku = key.sku;
				newData.amount = 0;
				datas.Add(key, newData);
			}

			// Add amount
			datas[key].amount += rewardDefs[i].GetAsLong("amount");
		}

		// Create a reward object from eac reward data
		m_rewards = new List<Metagame.Reward>();
		foreach(KeyValuePair<object, Metagame.Reward.Data> kvp in datas) {
			Debug.Log(kvp.Value.typeCode + " | " + kvp.Value.sku + " | " + kvp.Value.amount);
			m_rewards.Add(Metagame.Reward.CreateFromData(kvp.Value, HDTrackingManager.EEconomyGroup.REWARD_PREREG, ""));
		}

		// For each reward, find out its designed slot and initialize it
		MetagameRewardView targetSlot = null;
		for(int i = 0; i < m_rewards.Count; ++i) {
			// Depends on reward type
			switch(m_rewards[i].type) {
				case Metagame.RewardSoftCurrency.TYPE_CODE: targetSlot = m_scRewardSlot; break;
				case Metagame.RewardHardCurrency.TYPE_CODE: targetSlot = m_pcRewardSlot; break;
				case Metagame.RewardGoldenFragments.TYPE_CODE: targetSlot = m_gfRewardSlot; break;
				case Metagame.RewardPet.TYPE_CODE: targetSlot = m_petRewardSlot; break;
				case Metagame.RewardEgg.TYPE_CODE: {
					// Depends on egg sku
					// [AOC] A tad dirty, we shouldn't be using hardcoded skus >_<
					switch(m_rewards[i].sku) {
						case Egg.SKU_STANDARD_EGG:
						case Egg.SKU_PREMIUM_EGG: {
							targetSlot = m_egg1RewardSlot;
						} break;

						default: {
							targetSlot = m_egg2RewardSlot;
						} break;
					}
				} break;
			}

			// Initialize target slot!
			targetSlot.InitFromReward(m_rewards[i]);
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to open.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Hide all menu UI
		InstanceManager.menuSceneController.GetUICanvasGO().SetActive(false);
	}

	/// <summary>
	/// The popup is about to close.
	/// </summary>
	public void OnClosePreAnimation() {
		// [AOC] Moved to OnCollectButton()
		// Restore menu HUD
		//InstanceManager.menuSceneController.GetUICanvasGO().SetActive(true);
	}

	/// <summary>
	/// The collect button has been pressed. Start the collect flow.
	/// </summary>
	public void OnCollectButton() {
		// Make sure it doesn't happen twice!
		if(m_collected) return;
		m_collected = true;

		// Restore menu HUD
		// [AOC] Mini-HACK!! Do it before pushing the rewards to the stack, otherwise 
		// 		 the Pending Rewards flow will trigger when re-activating the Menu 
		//		 (MenuDragonScreenController.OnEnable()), colliding with the flow we're
		//		 just launching.
		InstanceManager.menuSceneController.GetUICanvasGO().SetActive(true);

		// Push all rewards to the stack in the desired collection order (reversed)
		UsersManager.currentUser.PushReward(m_petRewardSlot.reward);
		UsersManager.currentUser.PushReward(m_egg2RewardSlot.reward);
		UsersManager.currentUser.PushReward(m_egg1RewardSlot.reward);
		UsersManager.currentUser.PushReward(m_gfRewardSlot.reward);
		UsersManager.currentUser.PushReward(m_pcRewardSlot.reward);
		UsersManager.currentUser.PushReward(m_scRewardSlot.reward);

		// Mark step as completed!
		UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.PRE_REG_REWARDS, true);

		// Close all open popups
		PopupManager.Clear(true);

		// Move to the rewards screen
		PendingRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.PENDING_REWARD).ui.GetComponent<PendingRewardScreen>();
		scr.StartFlow(false);   // No intro
		InstanceManager.menuSceneController.GoToScreen(MenuScreen.PENDING_REWARD);
	}
}