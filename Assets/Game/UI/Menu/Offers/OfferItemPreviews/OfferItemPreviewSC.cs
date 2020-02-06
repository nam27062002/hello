// OfferItemPreviewSC.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

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
public class OfferItemPreviewSC : IOfferItemPreview {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public override ShopSettings.PrefabType type {
		get { return m_previewType; }
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Separator("Custom Fields")]
	[HideEnumValues(false, true)]
	[SerializeField] private ShopSettings.PrefabType m_previewType = ShopSettings.PrefabType.PREVIEW_2D;

	//------------------------------------------------------------------------//
	// OfferItemPreview IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize preview with current item (m_item)
	/// </summary>
	protected override void InitInternal() {
		// Item must be a pet!
		Debug.Assert(m_item.reward is Metagame.RewardSoftCurrency, "ITEM OF THE WRONG TYPE!", this);

		// Load preview - 2D or 3D?
		switch(m_previewType) {
			case ShopSettings.PrefabType.PREVIEW_2D: {
					// [AOC] TODO!! Load target image preview
					// Try to compose path from the pack "order" field (unfortunately we don't know to which pack we belong to)
				}
				break;

			case ShopSettings.PrefabType.PREVIEW_3D: {
					// Nothing to do, preview is already instantiated
				}
				break;
		}

	}

	/// <summary>
	/// Gets the description of this item, already localized and formatted.
	/// </summary>
	/// <returns>The localized description.</returns>
	public override string GetLocalizedDescription() {
		return LocalizationManager.SharedInstance.Localize(
					"TID_OFFER_ITEM_SC",    // x250
					StringUtils.FormatBigNumber(m_item.reward.amount, 1, 1000)  // Don't abbreviate if lower than 1K
				);
	}
}