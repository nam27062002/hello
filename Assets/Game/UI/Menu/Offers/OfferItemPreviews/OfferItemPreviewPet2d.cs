// OfferItemPreviewPet2d.cs
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
public class OfferItemPreviewPet2d : IOfferItemPreview {
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
    // GENERIC METHODS                                                        //
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

        // Initialize image with the target pet icon
        m_def = m_item.reward.def;
        if (m_def != null) {
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
		return LocalizationManager.SharedInstance.Localize("TID_PET");	// (shouldn't happen) use generic
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The info button has been pressed.
	/// </summary>
	/// <param name="_trackingLocation">Where is this been triggered from?</param>
	override public void OnInfoButton(string _trackingLocation) {
		// Intiialize info popup
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