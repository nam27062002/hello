// OfferItemPreviewDragon3d.cs
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
public class OfferItemPreviewDragon3d : IOfferItemPreview {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public override OfferItemPrefabs.PrefabType type {
		get { return OfferItemPrefabs.PrefabType.PREVIEW_3D; }
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private MenuDragonLoader m_dragonLoader = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Make sure dragon loader doesn't have any sku assigned
		m_dragonLoader.dragonSku = "";
	}

	//------------------------------------------------------------------------//
	// OfferItemPreview IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize preview with current item (m_item)
	/// </summary>
	protected override void InitInternal() {
		// Item must be a dragon!
		Debug.Assert(m_item.reward is Metagame.RewardDragon, "ITEM OF THE WRONG TYPE!", this);

		// Initialize dragon loader with the target dragon and skin!
		m_def = m_item.reward.def;
		if(m_def == null) {
			m_dragonLoader.LoadDragon("");
		} else {
			m_dragonLoader.LoadDragon(m_def.sku, IDragonData.GetDefaultDisguise(m_def.sku).sku);
		}

		// Particle systems require a special initialization
		InitParticles(m_dragonLoader.dragonInstance.gameObject);
	}

	/// <summary>
	/// Gets the description of this item, already localized and formatted.
	/// </summary>
	/// <returns>The localized description.</returns>
	public override string GetLocalizedDescription() {
		if(m_def != null) {
			return m_def.GetLocalized("tidName");
		}
		return string.Empty;    // Shouldn't happen
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The info button has been pressed.
	/// </summary>
	override public void OnInfoButton() {
		// Initialize and open info popup
		// Different popups for classic and special dragons
		PopupController popup = null;
		IDragonData dragonData = DragonManager.GetDragonData(m_def.sku);
		if(dragonData.type == IDragonData.Type.CLASSIC) {
			popup = PopupManager.LoadPopup(PopupDragonInfo.PATH);
			popup.GetComponent<PopupDragonInfo>().Init(dragonData);
		} else if(dragonData.type == IDragonData.Type.SPECIAL) {
			popup = PopupManager.LoadPopup(PopupSpecialDragonInfo.PATH);
			popup.GetComponent<PopupSpecialDragonInfo>().Init(dragonData);
		}

		// Move it forward in Z so it doesn't conflict with our 3d preview!
		popup.transform.SetLocalPosZ(-2500f);

		// Open the popup!
		popup.Open();
	}
}