// OfferItemPreviewEgg3d.cs
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
public class OfferItemPreviewEgg3d : IOfferItemPreviewEgg {
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
	[SerializeField] private MenuEggLoader m_eggLoader = null;

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize preview with current item (m_item)
	/// </summary>
	protected override void InitInternal() {
		// Call parent
		base.InitInternal();

		// Initialize loader with the target egg
		if(m_def == null) {
			m_eggLoader.Load("");
		} else {
			m_eggLoader.Load(m_def.sku);
		}

		// Disable VFX whenever a popup is opened in top of this preview (they don't render well with a popup on top)
		if(m_eggLoader.eggView.idleFX != null) {
			// [AOC] At this point the popup containing this preview hasn't yet been 
			// registered into the PopupManager, so we need to count for it in order 
			// for the disabler to work as expected.
			// By doing this, we are assuming the item preview belongs ALWAYS to a popup.
			DisableOnPopup disabler = m_eggLoader.eggView.idleFX.AddComponent<DisableOnPopup>();
			disabler.refPopupCount = PopupManager.openPopupsCount + 1;
		}
	}
}