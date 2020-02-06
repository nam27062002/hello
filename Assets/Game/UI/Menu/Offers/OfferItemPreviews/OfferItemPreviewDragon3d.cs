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
public class OfferItemPreviewDragon3d : IOfferItemPreviewDragon {
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
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize preview with current item (m_item)
	/// </summary>
	protected override void InitInternal() {
		// Call parent
		base.InitInternal();

		// Initialize dragon loader with the target dragon and skin!
		if(m_def == null) {
			m_dragonLoader.LoadDragon("");
		} else {
			m_dragonLoader.LoadDragon(m_def.sku, IDragonData.GetDefaultDisguise(m_def.sku).sku);
		}
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