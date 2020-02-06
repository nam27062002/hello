// OfferItemPreviewSkin2d.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/03/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

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
public class OfferItemPreviewSkin2d : IOfferItemPreviewSkin {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public override ShopSettings.PrefabType type {
		get { return ShopSettings.PrefabType.PREVIEW_2D; }
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private UISpriteAddressablesLoader m_loader = null;

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize preview with current item (m_item)
	/// </summary>
	protected override void InitInternal() {
		// Call parent
		base.InitInternal();

		// Initialize image with the target skin icon
		if(m_def != null) {			
            m_loader.LoadAsync(m_def.Get("icon"));
		}
	}
}