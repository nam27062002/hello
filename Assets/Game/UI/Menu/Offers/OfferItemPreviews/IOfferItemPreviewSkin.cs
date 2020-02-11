// IOfferItemPreviewSkin.cs
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
public abstract class IOfferItemPreviewSkin : IOfferItemPreview {
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
		// Item must be a skin!
		Debug.Assert(m_item.reward is Metagame.RewardSkin, "ITEM OF THE WRONG TYPE!", this);
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
		return LocalizationManager.SharedInstance.Localize("TID_DISGUISE");  // (shouldn't happen) use generic
	}

	/// <summary>
	/// Gets the secondary text of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized secondary text. <c>null</c> if this item type doesn't have to show any secondary text for the given type of slot (i.e. coins).</returns>
	public override string GetLocalizedSecondaryText(OfferItemSlot.Type _slotType) {
		// Always, show belonging dragon
		if(m_def != null) {
			DefinitionNode dragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, m_def.Get("dragonSku"));
			if(dragonDef != null) {
				return dragonDef.GetLocalized("tidName");	// [AOC] TODO!! More detailed text? i.e. "Blaze Skin", "Draconis Skin"
			}
		}
		return null;	// (shouldn't happen) don't show if not valid
	}

	/// <summary>
	/// Gets the description of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized description. <c>null</c> if this item type doesn't have to show any description for the given type of slot.</returns>
	public override string GetLocalizedDescriptionText(OfferItemSlot.Type _slotType) {
		// Only in popups - show skin's power description
		switch(_slotType) {
			case OfferItemSlot.Type.TOOLTIP:
			case OfferItemSlot.Type.POPUP_BIG:
			case OfferItemSlot.Type.POPUP_SMALL: {
				if(m_def != null) {
					DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, m_def.Get("powerup"));
					return DragonPowerUp.GetDescription(powerDef, false, true);   // Custom formatting depending on powerup type, already localized
				} else {
					return LocalizationManager.SharedInstance.Localize("Cool, powerful skin");	// (shouldn't happen) just in case
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
					// Get the power definition linked to this skin - PowerIcon will do the rest
					DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, m_def.Get("powerup"));
					_powerIcon.InitFromDefinition(powerDef, false, false, PowerIcon.Mode.SKIN);
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
		// Open info popup
		// [AOC] TODO!!
		UIFeedbackText.CreateAndLaunch(
			LocalizationManager.SharedInstance.Localize("TID_GEN_COMING_SOON"),
			GameConstants.Vector2.center,
			GetComponentInParent<Canvas>().transform as RectTransform
		);
		/*PopupController popup = PopupManager.LoadPopup(PopupInfoEggDropChance.PATH);
		popup.GetComponent<PopupInfoEggDropChance>().Init(m_item.sku);
		popup.Open();*/
	}
}