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

	/// <summary>
	/// Gets the amount of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized amount. <c>null</c> if this item type doesn't have to show the amount for the given type of slot (i.e. dragon).</returns>
	public override string GetLocalizedAmountText(OfferItemSlot.Type _slotType) {
		// Show always, no matter the slot type
		return FormatAmount(m_item.reward.amount);
	}

	/// <summary>
	/// Gets the main text of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized main text. <c>null</c> if this item type doesn't have to show main text for the given type of slot (i.e. coins).</returns>
	public override string GetLocalizedMainText(OfferItemSlot.Type _slotType) {
		// Only in popups
		switch(_slotType) {
			case OfferItemSlot.Type.TOOLTIP:
			case OfferItemSlot.Type.POPUP_BIG:
			case OfferItemSlot.Type.POPUP_SMALL: {
				return LocalizationManager.SharedInstance.Localize("TID_HC_NAME_PLURAL");
			} break;
		}
		return null;
	}

	/// <summary>
	/// Gets the description of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized description. <c>null</c> if this item type doesn't have to show any description for the given type of slot.</returns>
	public override string GetLocalizedDescriptionText(OfferItemSlot.Type _slotType) {
		// Only in popups
		switch(_slotType) {
			case OfferItemSlot.Type.TOOLTIP:
			case OfferItemSlot.Type.POPUP_BIG:
			case OfferItemSlot.Type.POPUP_SMALL: {
				return LocalizationManager.SharedInstance.Localize("TID_OFFER_ITEM_HC_DESC");
			} break;
		}
		return null;
	}
}