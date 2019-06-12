// OfferItemPreviewDragon2d.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/11/2018.
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
public class OfferItemPreviewDragon2d : IOfferItemPreview {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private UISpriteAddressablesLoader m_loader = null;

	//------------------------------------------------------------------------//
	// OfferItemPreview IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize preview with current item (m_item)
	/// </summary>
	protected override void InitInternal() {
		// Item must be a dragon!
		Debug.Assert(m_item.reward is Metagame.RewardDragon, "ITEM OF THE WRONG TYPE!", this);

		// Initialize image with the target dragon icon
		m_def = m_item.reward.def;
        if (m_def != null) {			
            string defaultIcon = IDragonData.GetDefaultDisguise(m_def.sku).Get("icon");
            m_loader.LoadAsync(defaultIcon);
		}
	}

	/// <summary>
	/// Gets the description of this item, already localized and formatted.
	/// </summary>
	/// <returns>The localized description.</returns>
	public override string GetLocalizedDescription() {
		if(m_def != null) {
			return m_def.GetLocalized("tidName");
		}
		return string.Empty;	// Shouldn't happen
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The info button has been pressed.
	/// </summary>
	override public void OnInfoButton() {
		// Open info popup
		// Different popups for classic and special dragons
		IDragonData dragonData = DragonManager.GetDragonData(m_def.sku);
		if(dragonData.type == IDragonData.Type.CLASSIC) {
			PopupController popup = PopupManager.LoadPopup(PopupDragonInfo.PATH);
			popup.GetComponent<PopupDragonInfo>().Init(dragonData);
			popup.Open();
		} else if(dragonData.type == IDragonData.Type.SPECIAL) {
			PopupController popup = PopupManager.LoadPopup(PopupSpecialDragonInfo.PATH);
			popup.GetComponent<PopupSpecialDragonInfo>().Init(dragonData);
			popup.Open();
		}
	}
}