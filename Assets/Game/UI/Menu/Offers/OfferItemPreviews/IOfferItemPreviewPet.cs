// IOfferItemPreviewPet.cs
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
public abstract class IOfferItemPreviewPet : IOfferItemPreview {
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
		Debug.Assert(m_item.reward is Metagame.RewardPet, "ITEM OF THE WRONG TYPE!", this);
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
		// Always, show pet name
		if(m_def != null) {
			return m_def.GetLocalized("tidName");
		}
		return LocalizationManager.SharedInstance.Localize("TID_PET");  // (shouldn't happen) use generic
	}

	/// <summary>
	/// Gets the secondary text of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized secondary text. <c>null</c> if this item type doesn't have to show any secondary text for the given type of slot (i.e. coins).</returns>
	public override string GetLocalizedSecondaryText(OfferItemSlot.Type _slotType) {
		// Always, show pet rarity
		if(m_def != null) {
			// Show tinted rarity + icon (except common, which has no icon nor color)
			string raritySku = m_def.Get("rarity");
			Metagame.Reward.Rarity rarity = Metagame.Reward.SkuToRarity(raritySku);
			DefinitionNode rarityDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.RARITIES, raritySku);
			if(rarity != Metagame.Reward.Rarity.COMMON) {
				// "▴Rare Pet", "♦Epic Pet"
				string rarityIcon = string.Format("<sprite name=\"icon_rarity_{0}\">", raritySku);
				Color rarityColor = UIConstants.GetRarityColor(rarity);
				return LocalizationManager.SharedInstance.Localize(
					"TID_PET_WITH_RARITY",
					string.Format("{0}<color={1}>{2}</color>", rarityIcon, rarityColor.ToHexString(), rarityDef.GetLocalized("tidName"))
				);
			} else {
				// "Common Pet"
				return LocalizationManager.SharedInstance.Localize(
					"TID_PET_WITH_RARITY",
					rarityDef.GetLocalized("tidName")
				);
			}
		}
		return LocalizationManager.SharedInstance.Localize("TID_PET");  // (shouldn't happen) use generic
	}

	/// <summary>
	/// Gets the description of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized description. <c>null</c> if this item type doesn't have to show any description for the given type of slot.</returns>
	public override string GetLocalizedDescriptionText(OfferItemSlot.Type _slotType) {
		// Only in popups - show power description
		switch(_slotType) {
			case OfferItemSlot.Type.TOOLTIP:
			case OfferItemSlot.Type.POPUP_BIG:
			case OfferItemSlot.Type.POPUP_SMALL: {
				if(m_def != null) {
					DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, m_def.Get("powerup"));
					return DragonPowerUp.GetDescription(powerDef, false, true);   // Custom formatting depending on powerup type, already localized
				} else {
					return LocalizationManager.SharedInstance.Localize("TID_PET");	// (shouldn't happen) just in case
				}
			} break;
		}
		return null;
	}

	/// <summary>
	/// Initialize the given power icon instance with data from this reward.
	/// Will disable it item doesn't have a power assigned.
	/// </summary>
	/// <param name="_powerIcon">The power icon to be initialized.</param>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	public override void InitPowerIcon(PowerIcon _powerIcon, OfferItemSlot.Type _slotType) {
		// Only in popups
		switch(_slotType) {
			case OfferItemSlot.Type.POPUP_BIG:
			case OfferItemSlot.Type.POPUP_SMALL: {
				if(m_def != null) {
					// Get the power definition linked to this pet - PowerIcon will do the rest
					DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, m_def.Get("powerup"));
					_powerIcon.InitFromDefinition(powerDef, false, false, PowerIcon.Mode.PET);
					return;
				}
			} break;
		}

		// Don't show in the rest of cases
		_powerIcon.InitFromDefinition(null, false, false);
	}

	/// <summary>
	/// The info button has been pressed.
	/// </summary>
	/// <param name="_trackingLocation">Where is this been triggered from?</param>
	override public void OnInfoButton(string _trackingLocation) {
		// Initialize info popup
		PopupController popup = PopupManager.LoadPopup(PopupInfoPet.PATH_SIMPLE);
		popup.GetComponent<PopupInfoPet>().Init(m_def);

		// Move it forward in Z so it doesn't conflict with our 3d preview!
		popup.transform.SetLocalPosZ(-2500f);

		// Open it!
		popup.Open();

		// Tracking
		string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupInfoPet.PATH_SIMPLE);
		HDTrackingManager.Instance.Notify_InfoPopup(popupName, _trackingLocation);
	}
}