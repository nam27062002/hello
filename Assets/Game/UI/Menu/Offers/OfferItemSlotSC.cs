// OfferItemSlotSC.cs
// Hungry Dragon
// 
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
public class OfferItemSlotSC : OfferItemSlot {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

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
        if (reloadPreview)
        {
            // Load the proper gem pack icon
            GameObject previewPrefab = ShopSettings.GetScIconPrefab(_order);
            if (previewPrefab == null)
            {
                Debug.LogError("No icon prefab defined for SC pack with order " + _order);
            }
            else { 
                // Instantiate preview! :)
                GameObject previewInstance = GameObject.Instantiate<GameObject>(previewPrefab, m_previewContainer, false);
                previewInstance.SetActive(true);
            }
        }


	}

}