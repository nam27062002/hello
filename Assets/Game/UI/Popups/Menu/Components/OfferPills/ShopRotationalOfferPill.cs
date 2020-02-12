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
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

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
		// Only for Eggs
		InfoButtonMode mode = InfoButtonMode.NONE; // None by default with rotationals
		if(m_pack != null && m_pack.items.Count > 0 && m_pack.items[0] != null) {
			// Info button mode depends on pack's item type
			switch(m_pack.items[0].type) {
				case Metagame.RewardEgg.TYPE_CODE: {
					mode = InfoButtonMode.TOOLTIP;
				} break;
			}
		}

		return mode;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}