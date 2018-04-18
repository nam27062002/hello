// OfferItemPreviewEgg2d.cs
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
public class OfferItemPreviewEgg2d : IOfferItemPreview {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Image m_image = null;

	//------------------------------------------------------------------------//
	// OfferItemPreview IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize preview with current item (m_item)
	/// </summary>
	protected override void InitInternal() {
		// Item must be an egg!
		Debug.Assert(m_item.type == Metagame.RewardEgg.TYPE_CODE, "ITEM OF THE WRONG TYPE!", this);

		// Initialize image with the target egg icon
		m_def = DefinitionsManager.SharedInstance.GetDefinition(m_item.sku);
		if(m_def == null) {
			m_image.sprite = null;
		} else {
			m_image.sprite = Resources.Load<Sprite>(UIConstants.EGG_ICONS_PATH + m_def.GetAsString("icon"));
		}
	}

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