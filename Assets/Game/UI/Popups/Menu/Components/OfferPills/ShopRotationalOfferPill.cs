// PopupShopFreeOfferPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/10/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System;
using System.Text;

using TMPro;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single pill in the shop for a FREE offer pack.
/// </summary>
public class ShopRotationalOfferPill : ShopMonoRewardPill {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

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
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the pill with a given pack's data.
	/// </summary>
	/// <param name="_pack">Pack.</param>
	public override void InitFromOfferPack(OfferPack _pack) {
		// Call parent
		base.InitFromOfferPack(_pack);

		// Perform a first refresh
		RefreshTimer();
	}

	/// <summary>
	/// Get the info button mode for this pill's pack.
	/// </summary>
	/// <returns>The desired button mode.</returns>
	protected override InfoButtonMode GetInfoButtonMode() {
		// After the playtest, always show tooltip for the rotational offers
		return InfoButtonMode.TOOLTIP;
	}

    protected override void ApplyShopPack()
    {
        base.ApplyShopPack();

		// If we are in the shop tell the menu controller to open the shop after the rewards screen
		if (InstanceManager.menuSceneController.currentScreen == MenuScreen.SHOP)
		{
			InstanceManager.menuSceneController.interstitialPopupsController.SetFlag(MenuInterstitialPopupsController.StateFlag.OPEN_SHOP, true);
		}
	}

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//

}