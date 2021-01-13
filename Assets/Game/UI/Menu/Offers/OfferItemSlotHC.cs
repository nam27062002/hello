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
using System.Collections.Generic;

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
    private bool m_applyHappyHour = false;
    private HappyHour m_appliedHappyHour = null;
    public bool applyHappyHour
    {
        set => m_applyHappyHour = value;
    }


    //------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the widget with the data of a specific offer item.
	/// </summary>
	/// <param name="_item">Item to be used to initialize the slot.</param>
	/// <param name="_order">Used to select the proper HC or SC icon</param>
	public override void InitFromItem(OfferPackItem _item, int _order)
    {
		m_order = _order;

        // Force reloading preview if item is different than the current one
		bool reloadPreview = false;
        if (m_item != _item) reloadPreview = true;

        // Store new item
        m_item = _item;

        // If given item is null, disable game object and don't do anything else
        if (m_item == null)
        {
            this.gameObject.SetActive(false);
            if (reloadPreview) ClearPreview();
            return;
        }

        // Aux vars
        Metagame.Reward reward = item.reward;

        // Activate game object
        this.gameObject.SetActive(true);

        // If a preview was already created, destroy it
        if (reloadPreview) ClearPreview();

		// Load new preview (if required)
		if(m_previewContainer != null) {
			if(reloadPreview) {
				// Load the proper gem pack icon
				IOfferItemPreviewHC previewPrefab = ShopSettings.GetHcIconPrefab(_order);
				if(previewPrefab == null) {
					Debug.LogError("No icon prefab defined for HC pack with order " + _order);
				} else {
					if(m_previewContainer != null) {
						// Instantiate preview! :)
						m_preview = GameObject.Instantiate<IOfferItemPreviewHC>(previewPrefab, m_previewContainer, false);
						m_preview.gameObject.SetActive(true);
					}

				}
			}

			// Initialize preview with item data
			if(m_preview != null) {
				m_preview.InitFromItem(m_item, m_slotType);
				m_preview.SetParentAndFit(m_previewContainer as RectTransform);
			} else {
				// Skip if preview is not initialized (something went very wrong :s)
				Debug.LogError("Attempting to initialize slot for item " + m_item.sku + " but reward preview is null!" +
					"\n" + "order: " + _order + " | " + this.transform.GetHierarchyPath());
#if UNITY_EDITOR
				UnityEditor.Selection.activeObject = this.gameObject;
#endif
			}
		}

        // Refresh UI 
        Refresh();

	}

	/// <summary>
	/// Refresh visuals of this item.
	/// </summary>
	public void Refresh()
    {

        // Nothing to do if item is not valid
		if(m_item == null) return;

		// Make sure happy hour is active, and affects this item
        HappyHour happyHour = OffersManager.happyHourManager.happyHour;

        // Previous amount
		if(m_previousAmountText != null) {
			m_previousAmountText.gameObject.SetActive(m_applyHappyHour);
			if(m_applyHappyHour && happyHour != null) {
				// Unmodified amount
				m_previousAmountText.text = IOfferItemPreviewHC.FormatAmount(m_item.reward.amount);
			}
		}

		// New amount
		if(m_amountText != null) {
			if(m_applyHappyHour && happyHour != null) {
				// Amount with HH bonus applied
				float bonusAmount = happyHour.extraGemsFactor;
				long newAmount = happyHour.ApplyHappyHourExtra(m_item.reward.amount);

                // Add some color!
                m_amountText.text = m_happyHourTextColor.Tag(IOfferItemPreviewHC.FormatAmount(newAmount));
			} else {
                m_amountText.text = IOfferItemPreviewHC.FormatAmount(m_item.reward.amount);
			}
		}
	}
}