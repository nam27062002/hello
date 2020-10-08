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

	// Internal vars
	private Metagame.Reward m_reward = null;
	private ShowHideAnimator m_rewardAnimator = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Cache reference to reward's animator
		m_rewardAnimator = m_rewardPreview.GetComponent<ShowHideAnimator>();
	}

	private void Start()
	{

		// Show the next incoming reward in the preview
		m_reward = XPromoManager.instance.GetNextWaitingReward();

        if (m_reward == null)
        	return;

        // Initialize the preview with the reward
		m_rewardPreview.InitFromReward(m_reward);

		// If this is the first run (the player just installed the game), show the proper welcome message
		if (UsersManager.currentUser.gamesPlayed == 0)
        {
			m_welcomeDescription.Localize(WELCOME_FTUX_TID);
        }
        else
        {
            // Are we giving the alternative reward?
            if (m_reward.IsAlreadyOwned() && m_reward.replacement != null)
            {
				// Inform the player that he is receiveing an alternative reward
				m_welcomeDescription.Localize(WELCOME_ALT_TID);


			}
            else 
            {
				// Base case
				m_welcomeDescription.Localize(WELCOME_TID);
			}
        }
	}


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Shortcut method to show/hide reward, either using animator or GameObject 
	/// if no animator was found.
	/// </summary>
	/// <param name="_show">Whether to show or hide the reward.</param>
	/// <param name="_animate">Animate? (Only if a ShowHideAnimator was found).</param>
	private void ShowReward(bool _show, bool _animate) {
		if(m_rewardAnimator != null) {
			m_rewardAnimator.ForceSet(_show, _animate);
		} else {
			m_rewardPreview.gameObject.SetActive(_show);
		}
	}


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to open.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Hide reward
		ShowReward(false, false);
	}

	/// <summary>
	/// The popup has finished opening.
	/// </summary>
	public void OnOpenPostAnimation() {
		// Initialize and show reward
		m_rewardPreview.InitFromReward(m_reward);
		ShowReward(true, true);
	}

	/// <summary>
	/// The popup is about to close.
	/// </summary>
	public void OnClosePreAnimation() {
		// Hide reward
		ShowReward(false, true);
	}

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