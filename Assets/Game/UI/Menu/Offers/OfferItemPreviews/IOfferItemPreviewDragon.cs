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
	/// Gets the description of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized description. <c>null</c> if this item type doesn't have to show any description for the given type of slot.</returns>
	public override string GetLocalizedDescriptionText(OfferItemSlot.Type _slotType) {
		// Only in popups - show dragon description
		switch(_slotType) {
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
	/// The info button has been pressed.
	/// </summary>
	/// <param name="_trackingLocation">Where is this been triggered from?</param>
	override public void OnInfoButton(string _trackingLocation) {
		// Open info popup
		// [AOC] We haven't purchased the dragon yet, create fake data of the dragon
		IDragonData dragonData = IDragonData.CreateFromDef(m_def);
		PopupDragonInfo popup = PopupDragonInfo.OpenPopupForDragon(dragonData, _trackingLocation);

		// Move it forward in Z so it doesn't conflict with 3d previews!
		popup.transform.SetLocalPosZ(-2500f);
	}
}