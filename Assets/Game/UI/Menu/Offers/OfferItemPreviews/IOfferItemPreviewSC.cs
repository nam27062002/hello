// OfferItemPreviewSC.cs
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
public abstract class IOfferItemPreviewSC : IOfferItemPreview {
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
		Debug.Assert(m_item.reward is Metagame.RewardSoftCurrency, "ITEM OF THE WRONG TYPE!", this);
	}

	/// <summary>
	/// Gets the amount of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized amount. <c>null</c> if this item type doesn't have to show the amount for the given type of slot (i.e. dragon).</returns>
	public override string GetLocalizedAmountText(OfferItemSlot.Type _slotType) {
		// Show always, no matter the slot type
		return LocalizationManager.SharedInstance.Localize(
				"TID_OFFER_ITEM_SC",    // x250
				UIConstants.FormatCurrency(m_item.reward.amount)
			);
	}

	/// <summary>
	/// Gets the main text of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized main text. <c>null</c> if this item type doesn't have to show main text for the given type of slot (i.e. coins).</returns>
	public override string GetLocalizedMainText(OfferItemSlot.Type _slotType) {
		// Only in popups and tooltip
		switch(_slotType) {
			case OfferItemSlot.Type.POPUP_BIG:
			case OfferItemSlot.Type.POPUP_SMALL:
			case OfferItemSlot.Type.PILL_FREE: {
				return LocalizationManager.SharedInstance.Localize("TID_SC_NAME_PLURAL");
			} break;

			case OfferItemSlot.Type.TOOLTIP: {
				// Show the amount in the tooltip
				return string.Format(
					"{0} {1}",
					UIConstants.FormatCurrency(m_item.reward.amount),
					LocalizationManager.SharedInstance.Localize("TID_SC_NAME_PLURAL")
				);
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
		// Only in popups and tooltip
		switch(_slotType) {
			case OfferItemSlot.Type.TOOLTIP:
			case OfferItemSlot.Type.POPUP_BIG:
			case OfferItemSlot.Type.POPUP_SMALL: {
				return LocalizationManager.SharedInstance.Localize("TID_OFFER_ITEM_SC_DESC");
			} break;
		}
		return null;
	}
}