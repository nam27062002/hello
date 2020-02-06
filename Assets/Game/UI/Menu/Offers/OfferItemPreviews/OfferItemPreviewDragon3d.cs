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
	public override ShopSettings.PrefabType type {
		get { return ShopSettings.PrefabType.PREVIEW_3D; }
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
		m_dragonLoader.onDragonLoaded += OnDragonLoaded;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		m_dragonLoader.onDragonLoaded -= OnDragonLoaded;
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
	/// <param name="_trackingLocation">Where is this been triggered from?</param>
	override public void OnInfoButton(string _trackingLocation) {
		// Open info popup
		// [AOC] We haven't purchased the dragon yet, create fake data of the dragon
		IDragonData dragonData = IDragonData.CreateFromDef(m_def);
		PopupDragonInfo popup = PopupDragonInfo.OpenPopupForDragon(dragonData, _trackingLocation);

		// Move it forward in Z so it doesn't conflict with our 3d preview!
		popup.transform.SetLocalPosZ(-2500f);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Dragon has been loaded.
	/// </summary>
	/// <param name="_loader">Loader that triggered the event.</param>
	private void OnDragonLoaded(MenuDragonLoader _loader) {
		// Particle systems require a special initialization
		InitParticles(m_dragonLoader.dragonInstance.gameObject);
	}
}