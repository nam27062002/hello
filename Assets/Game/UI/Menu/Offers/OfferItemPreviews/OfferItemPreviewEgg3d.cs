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
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private MenuEggLoader m_eggLoader = null;

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
		m_def = DefinitionsManager.SharedInstance.GetDefinition(m_item.sku);
		if(m_def == null) {
			m_eggLoader.Load("");
		} else {
			m_eggLoader.Load(m_def.sku);
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
}