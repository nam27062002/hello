// IOfferItemPreviewEgg.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/02/2020.
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
public abstract class IOfferItemPreviewEgg : IOfferItemPreview {
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
		// Item must be an egg!
		Debug.Assert(m_item.type == Metagame.RewardEgg.TYPE_CODE, "ITEM OF THE WRONG TYPE!", this);

		// Store definition
		m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, m_item.sku);
	}

	/// <summary>
	/// Gets the amount of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized amount. <c>null</c> if this item type doesn't have to show the amount for the given type of slot (i.e. dragon).</returns>
	public override string GetLocalizedAmountText(OfferItemSlot.Type _slotType) {
		// Show always, no matter the slot type
		return LocalizationManager.SharedInstance.Localize(
				"TID_OFFER_ITEM_EGGS",    // x5
				StringUtils.FormatNumber(m_item.reward.amount)
			);
	}

	/// <summary>
	/// Gets the main text of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized main text. <c>null</c> if this item type doesn't have to show main text for the given type of slot (i.e. coins).</returns>
	public override string GetLocalizedMainText(OfferItemSlot.Type _slotType) {
		// Only in popups
		switch(_slotType) {
			case OfferItemSlot.Type.POPUP_BIG:
			case OfferItemSlot.Type.POPUP_SMALL: {
				// Each type of egg has its own name, grab it from definition
				if(m_def != null) {
					// Singular or plural?
					long amount = m_item.reward.amount;
					string tidName = m_def.GetAsString("tidName");
					return LocalizationManager.SharedInstance.Localize(amount > 1 ? tidName + "_PLURAL" : tidName);
				} else {
					return LocalizationManager.SharedInstance.Localize("TID_EGG_PLURAL");   // (shouldn't happen) use generic
				}
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
			case OfferItemSlot.Type.POPUP_BIG:
			case OfferItemSlot.Type.POPUP_SMALL: {
				if(m_def != null) {
					return m_def.GetLocalized("tidDesc");
				} else {
					LocalizationManager.SharedInstance.Localize("TID_EGG_PLURAL");	// (shouldn't happen) just in case
				}
			} break;
		}
		return null;
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The info button has been pressed.
	/// </summary>
	/// <param name="_trackingLocation">Where is this been triggered from?</param>
	override public void OnInfoButton(string _trackingLocation) {
		// [AOC] TODO!! Tooltip

		// Intialize info popup
		/*PopupController popup = PopupManager.LoadPopup(PopupInfoEggDropChance.PATH);
		popup.GetComponent<PopupInfoEggDropChance>().Init(m_item.sku);

		// Move it forward in Z so it doesn't conflict with our 3d preview!
		popup.transform.SetLocalPosZ(-2500f);

		// Open it!
		popup.Open();

		// Tracking
		string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupInfoPet.PATH_SIMPLE);
		HDTrackingManager.Instance.Notify_InfoPopup(popupName, _trackingLocation);
		*/
	}
}