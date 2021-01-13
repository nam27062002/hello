// IOfferItemPreviewDragon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/02/2020.
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
public abstract class IOfferItemPreviewDragon : IOfferItemPreview {
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
		// Item must be a dragon!
		Debug.Assert(m_item.reward is Metagame.RewardDragon, "ITEM OF THE WRONG TYPE!", this);
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Gets the main text of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized main text. <c>null</c> if this item type doesn't have to show main text for the given type of slot (i.e. coins).</returns>
	public override string GetLocalizedMainText(OfferItemSlot.Type _slotType) {
		// Always, show dragon name
		if(m_def != null) {
			return m_def.GetLocalized("tidName");
		}
		return LocalizationManager.SharedInstance.Localize("A Dragon");  // (shouldn't happen) use generic
	}

	/// <summary>
	/// Gets the secondary text of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized secondary text. <c>null</c> if this item type doesn't have to show any secondary text for the given type of slot (i.e. coins).</returns>
	public override string GetLocalizedSecondaryText(OfferItemSlot.Type _slotType) {
		// Only in the selection tabs in the popups - show dragon name
		switch(_slotType) {
			case OfferItemSlot.Type.POPUP_MINI: {
				return GetLocalizedMainText(_slotType);
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
		// Only in popups - show dragon description
		switch(_slotType) {
			case OfferItemSlot.Type.TOOLTIP:
			case OfferItemSlot.Type.POPUP_BIG:
			case OfferItemSlot.Type.POPUP_SMALL: {
				if(m_def != null) {
					return m_def.GetLocalized("tidDesc");
				} else {
					return LocalizationManager.SharedInstance.Localize("Big, powerful dragon");	// (shouldn't happen) just in case
				}
			} break;
		}
		return null;
	}

	/// <summary>
	/// Initialize the given tier icon instance with data from this reward.
	/// Will disable it if reward type doesn't support tiers, as well as depending on the setup from offerSettings.
	/// </summary>
	/// <param name="_tierIconContainer">Where to instantiate the tier icon.</param>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	public override void InitTierIcon(GameObject _tierIconContainer, OfferItemSlot.Type _slotType) {
		// Should it be displayed by this slot type?
		bool show = ShowTierIconBySlotType(_slotType);
		_tierIconContainer.SetActive(show);

		// If not, nothing else to do
		if(!show) return;

		// Clear any previously loaded icon
		_tierIconContainer.transform.DestroyAllChildren(false);

		// Load tier icon
		DragonTier tier = IDragonData.SkuToTier(m_def.GetAsString("tier", "tier_6")); // Specials definitions don't have the tier field, so use special tier as default value
		GameObject tierIconPrefab = UIConstants.GetTierIcon(tier);
		Instantiate(tierIconPrefab, _tierIconContainer.transform, false);
	}
}