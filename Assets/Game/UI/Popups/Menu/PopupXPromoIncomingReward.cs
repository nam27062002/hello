// PopupXPromoRewards.cs
// Hungry Dragon
// 
// Created by Jose M. Olea on 03/09/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using XPromo;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the XPromo popup
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupXPromoIncomingReward : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Menu/PF_PopupXPromoIncomingReward";


	public const string WELCOME_TID = "TID_XPROMO_REWARD_POPUP_DESC";
	public const string WELCOME_ALT_TID = "TID_XPROMO_REWARD_POPUP_ALTERNATIVE_DESC";
	public const string WELCOME_FTUX_TID = "TID_XPROMO_REWARD_POPUP_FTUX_DESC";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	[SerializeField] private Localizer m_welcomeDescription;

	[SerializeField] private MetagameRewardView m_rewardPreview;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//


	private void Start()
	{
		bool isAlternative = false;

        // Show the next incoming reward in the preview
		Metagame.Reward reward = XPromoManager.instance.GetNextWaitingReward(false);

        if (reward == null)
        	return;

		// The player already have this reward?
		if (reward.IsAlreadyOwned())
        {
            // Ask for the alternative reward instead.
			reward = XPromoManager.instance.GetNextWaitingReward(true);
            isAlternative = true;

			if (reward == null)
				return;
		}

        // Initialize the preview with the reward
		m_rewardPreview.InitFromReward(reward);

		// If this is the first run (the player just installed the game), show the proper welcome message
		if (UsersManager.currentUser.gamesPlayed == 0)
        {
			m_welcomeDescription.Localize(WELCOME_FTUX_TID);
        }
        else
        {
            // Are we giving the alternative reward?
            if (! isAlternative)
            {
                // Base case
				m_welcomeDescription.Localize(WELCOME_TID);
			}
            else
            {
                // Inform the player that he is receiveing an alternative reward
				m_welcomeDescription.Localize(WELCOME_ALT_TID);
			}
        }
	}


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//



	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// The player clicked the collect reward button
	/// </summary>
	public void OnCollectButton()
	{
		// Send tracking event
		HDTrackingManager.Instance.Notify_XPromoUIButton("Collect_HSE_Reward");

		// Tell the manager to put the rewards in the pending reward queue
		XPromoManager.instance.OnCollectAllIncomingRewards();

		// Move to the rewards screen
		PendingRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.PENDING_REWARD).ui.GetComponent<PendingRewardScreen>();
		scr.StartFlow(false);   // No intro
		InstanceManager.menuSceneController.GoToScreen(MenuScreen.PENDING_REWARD);
		

		// Close the popup now
		GetComponent<PopupController>().Close(true);

	}

}