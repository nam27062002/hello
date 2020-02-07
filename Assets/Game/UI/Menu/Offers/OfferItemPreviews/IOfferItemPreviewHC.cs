// IOfferItemPreviewHC.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple class to encapsulate the preview of an item.
/// </summary>
public abstract class IOfferItemPreviewHC : IOfferItemPreview {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// OfferItemPreview IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize preview with current item (m_item)
	/// </summary>
	protected override void InitInternal() {
		// Item must be a pet!
		Debug.Assert(m_item.reward is Metagame.RewardHardCurrency, "ITEM OF THE WRONG TYPE!", this);
	}

	/// <summary>
	/// Gets the description of this item, already localized and formatted.
	/// </summary>
	/// <returns>The localized description.</returns>
	public override string GetLocalizedDescription() {
		return FormatAmount(m_item.reward.amount);
	}

	/// <summary>
	/// Format an amount value according to this item type.
	/// </summary>
	/// <param name="_amount">Amount to be formatted.</param>
	/// <returns>Localized and formatted amount.</returns>
	public string FormatAmount(long _amount) {
		return LocalizationManager.SharedInstance.Localize(
			"TID_OFFER_ITEM_HC",    // x250
			StringUtils.FormatBigNumber(_amount, 1, 1000)  // Don't abbreviate if lower than 1K
		);
	}
}