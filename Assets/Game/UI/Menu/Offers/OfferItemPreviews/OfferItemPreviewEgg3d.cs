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
public class OfferItemPreviewEgg3d : IOfferItemPreview {
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
	[SerializeField] private MenuEggLoader m_eggLoader = null;

	// Internal
	private bool m_restoreVFX = false;

	//------------------------------------------------------------------------//
	// OfferItemPreview IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize preview with current item (m_item)
	/// </summary>
	protected override void InitInternal() {
		// Item must be an egg!
		Debug.Assert(m_item.type == Metagame.RewardEgg.TYPE_CODE, "ITEM OF THE WRONG TYPE!", this);

		// Initialize loader with the target egg
		m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, m_item.sku);
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

	/// <summary>
	/// Gets the description of this item, already localized and formatted.
	/// </summary>
	/// <returns>The localized description.</returns>
	public override string GetLocalizedDescription() {
		if(m_def != null) {
			// Singular or plural?
			long amount = m_item.reward.amount;
			string tidName = m_def.GetAsString("tidName");
			return LocalizationManager.SharedInstance.Localize(
				"TID_OFFER_ITEM_EGGS",
				StringUtils.FormatNumber(amount),
				LocalizationManager.SharedInstance.Localize(amount > 1 ? tidName + "_PLURAL" : tidName)
			);
		}
		return LocalizationManager.SharedInstance.Localize("TID_EGG_PLURAL");	// (shouldn't happen) use generic
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The info button has been pressed.
	/// </summary>
	override public void OnInfoButton() {
		// Intiialize info popup
		PopupController popup = PopupManager.LoadPopup(PopupInfoEggDropChance.PATH);
		popup.GetComponent<PopupInfoEggDropChance>().Init(m_item.sku);

		// Move it forward in Z so it doesn't conflict with our 3d preview!
		popup.transform.SetLocalPosZ(-2500f);

		// Open it!
		popup.Open();
	}

	/// <summary>
	/// The info popup is about to close.
	/// </summary>
	private void OnInfoPopupClosed() {
		if(m_restoreVFX) {
			m_eggLoader.eggView.idleFX.SetActive(true);
			m_restoreVFX = false;
		}
	}
}