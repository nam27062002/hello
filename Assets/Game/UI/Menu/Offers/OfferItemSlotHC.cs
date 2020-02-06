// OfferItemSlotHC.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

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
/// Specialization of the OfferItemSlot class to add some extra functionality (Happy
/// Hour) to slots belonging to HC packs.
/// </summary>
public class OfferItemSlotHC : OfferItemSlot {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Header("Custom Fields")]
	[SerializeField] protected TextMeshProUGUI m_previousAmountText = null;
	[SerializeField] protected Color m_happyHourTextColor = Colors.yellow;

	// Internal
	private HappyHour m_appliedHappyHour = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the widget with the data of a specific offer item.
	/// </summary>
	/// <param name="_item">Item to be used to initialize the slot.</param>
	public override void InitFromItem(OfferPackItem _item) {
		// Let parent do the big stuff
		base.InitFromItem(_item);

		// If given item is null, reset some data
		if(m_item == null) {
			m_appliedHappyHour = null;
			return;	// Nothing else to do
		}

		// Re-apply happy hour modifications
		ApplyHappyHour(m_appliedHappyHour);
	}

	/// <summary>
	/// Apply Happy Hour visuals to this item.
	/// </summary>
	/// <param name="_happyHour">The happy hour to be applied. <c>null</c> if not active or the pack this item belongs to is not affected by it.</param>
	public void ApplyHappyHour(HappyHour _happyHour) {
		// Nothing to do if item is not valid
		if(m_item == null) return;

		// Toggle on or off?
		// Assume the pack this item belongs to is affected by the happy hour,
		// otherwise the parent pill should pass null as argument
		bool validHH = _happyHour != null && _happyHour.IsActive();
		m_appliedHappyHour = _happyHour;

		// We need a valid preview to be able to format texts
		OfferItemPreviewHC previewHC = m_preview as OfferItemPreviewHC;
		validHH &= m_preview != null;

		// Previous amount
		if(m_previousAmountText != null) {
			m_previousAmountText.gameObject.SetActive(validHH);
			if(validHH) {
				// Unmodified amount
				m_previousAmountText.text = previewHC.FormatAmount(m_item.reward.amount);
			}
		}

		// New amount
		if(m_mainText != null) {
			if(validHH) {
				// Amount with HH bonus applied
				float bonusAmount = _happyHour.extraGemsFactor;
				long newAmount = _happyHour.ApplyHappyHourExtra(m_item.reward.amount);

				// Add some color!
				m_mainText.text = m_happyHourTextColor.Tag(previewHC.FormatAmount(newAmount));
			} else {
				// Unmodified amount
				if(previewHC != null) {
					m_mainText.text = previewHC.FormatAmount(m_item.reward.amount);
				} else {
					// Fallback, we should never reach this point
					m_mainText.text = StringUtils.FormatNumber(m_item.reward.amount);
				}
			}
		}
	}
}