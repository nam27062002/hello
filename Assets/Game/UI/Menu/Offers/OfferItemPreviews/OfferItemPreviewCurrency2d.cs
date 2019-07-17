// OfferItemPreviewCurrency2d.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/03/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple class to encapsulate the preview of an item.
/// </summary>
public class OfferItemPreviewCurrency2d : IOfferItemPreview {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public override OfferItemPrefabs.PrefabType type {
		get { return OfferItemPrefabs.PrefabType.PREVIEW_2D; }
	}

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
		Debug.Assert(m_item.reward is Metagame.RewardCurrency, "ITEM OF THE WRONG TYPE!", this);

		// Image is already defined in the prefab, nothing to do!
	}

	/// <summary>
	/// Gets the description of this item, already localized and formatted.
	/// </summary>
	/// <returns>The localized description.</returns>
	public override string GetLocalizedDescription() {
		// Depends on item type
		switch(m_item.reward.type) {
			case Metagame.RewardHardCurrency.TYPE_CODE: {
				return LocalizationManager.SharedInstance.Localize(
					"TID_OFFER_ITEM_HC",
					StringUtils.FormatNumber(m_item.reward.amount)
				);
			} break;

			case Metagame.RewardSoftCurrency.TYPE_CODE: {
				return LocalizationManager.SharedInstance.Localize(
					"TID_OFFER_ITEM_SC",
					StringUtils.FormatNumber(m_item.reward.amount)
				);
			} break;

			case Metagame.RewardGoldenFragments.TYPE_CODE: {
				return LocalizationManager.SharedInstance.Localize(
					"TID_OFFER_ITEM_GOLDEN_FRAGMENTS",
					StringUtils.FormatNumber(m_item.reward.amount)
				);
			} break;
		}
		return LocalizationManager.SharedInstance.Localize("Tons of some unknown currency!");	// (shouldn't happen) use generic
	}
}