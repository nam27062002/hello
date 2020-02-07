// OfferItemPreviewHC2d.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple class to encapsulate the preview of an item.
/// </summary>
public class OfferItemPreviewHC2d : IOfferItemPreviewHC {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public override IOfferItemPreview.Type type {
		get { return IOfferItemPreview.Type._2D; }
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Transform m_iconRoot = null;

	// Internal
	private GameObject m_iconInstance = null;

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize preview with current item (m_item)
	/// </summary>
	protected override void InitInternal() {
		// Call parent
		base.InitInternal();

		// Initialize image with the target egg icon
		if(m_def == null) {
			if(m_iconInstance != null) Destroy(m_iconInstance);
		} else {
			// [AOC] TODO!! Load different icon prefabs based on pack sku
			//				Consider also the case where the HC reward doesn't belong to a pack
			GameObject iconPrefab = Resources.Load<GameObject>(UIConstants.SHOP_ICONS_PATH);
			if(iconPrefab != null) {
				m_iconInstance = Instantiate<GameObject>(iconPrefab, m_iconRoot, false);
			} else {
				// Invalid prefab, let's show the default icon already in the prefab
				// Nothing to do
			}
		}
	}
}