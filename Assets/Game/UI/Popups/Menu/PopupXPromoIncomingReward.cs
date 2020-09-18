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

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	

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