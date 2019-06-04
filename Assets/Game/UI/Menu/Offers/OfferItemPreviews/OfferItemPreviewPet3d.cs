// OfferItemPreviewPet3d.cs
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
public class OfferItemPreviewPet3d : IOfferItemPreview {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private MenuPetLoader m_petPreview = null;


    //------------------------------------------------------------------------//
    // GENERIC METHODS                                                        //
    //------------------------------------------------------------------------//
    protected void OnDestroy() {
        m_petPreview.OnLoadingComplete.RemoveListener(OnLoadingComplete);
    }


    //------------------------------------------------------------------------//
    // OfferItemPreview IMPLEMENTATION										  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialize preview with current item (m_item)
    /// </summary>
    protected override void InitInternal() {
		// Item must be a pet!
		Debug.Assert(m_item != null && m_item.reward != null && m_item.reward is Metagame.RewardPet, "ITEM IS NULL OR OF THE WRONG TYPE!", this);

		// Store definition
		m_def = m_item.reward.def;

		// Initialize pet loader with the target pet preview!
		m_petPreview.Load(m_item.reward.sku);
        m_petPreview.OnLoadingComplete.AddListener(OnLoadingComplete);
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
	/// Set this preview's parent and adjust its size to fit it.
	/// </summary>
	/// <param name="_t">New parent!</param>
	public override void SetParentAndFit(RectTransform _t) {
		// Let parent do its thing
		base.SetParentAndFit(_t);

		// Refresh particle scaler
		m_petPreview.pscaler.DoScale();
	}

	/// <summary>
	/// The info button has been pressed.
	/// </summary>
	override public void OnInfoButton() {
		// Intiialize info popup
		PopupController popup = PopupManager.LoadPopup(PopupInfoPet.PATH_SIMPLE);
		popup.GetComponent<PopupInfoPet>().Init(m_def);

		// Move it forward in Z so it doesn't conflict with our 3d preview!
		popup.transform.SetLocalPosZ(-2500f);

		// Open it!
		popup.Open();
    }

    //------------------------------------------------------------------------//
    // CALLBACKS                                                              //
    //------------------------------------------------------------------------//
    private void OnLoadingComplete(MenuPetLoader _loader) {
        InitParticles(m_petPreview.petInstance.gameObject);
        _loader.OnLoadingComplete.RemoveListener(OnLoadingComplete);
    }
}