// OfferItemPreviewSkin2d.cs
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
public class OfferItemPreviewSkin2d : IOfferItemPreview {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public override OfferItemPrefabs.PrefabType type {
		get { return OfferItemPrefabs.PrefabType.PREVIEW_2D; }
	}

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
		// Item must be a skin!
		Debug.Assert(m_item.reward is Metagame.RewardSkin, "ITEM OF THE WRONG TYPE!", this);

		// Initialize image with the target skin icon
		m_def = m_item.reward.def;
		if(m_def != null) {			
            m_loader.LoadAsync(m_def.Get("icon"));
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
		return LocalizationManager.SharedInstance.Localize("TID_DISGUISE");	// (shouldn't happen) use generic
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The info button has been pressed.
	/// </summary>
	override public void OnInfoButton() {
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