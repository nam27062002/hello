// ShopPackPill.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 05/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System;
using System.Collections.Generic;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class ShopMultiRewardPill: ShopBasePill
{
    //------------------------------------------------------------------------//
    // AUX CLASSES & STRUCTS												  //
    //------------------------------------------------------------------------//


    /// <summary>
    /// This class represents each layout for different amount of items
    /// (i.e. the 3 items layout)    
    /// </summary>
    [System.Serializable]
    public class OffersLayout
    {
        public Transform m_container;
        public List<OfferItemSlot> m_offerItems;
    }

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    [Separator("Multi reward Specifics")]

    // The list of 3 different layouts created. Has to be filled in the inspector.
    [SerializeField] private List<OffersLayout> m_layouts;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialize the pill with a given pack's data.
    /// </summary>
    /// <param name="_pack">Pack.</param>
    public override void InitFromOfferPack(OfferPack _pack)
    {
        base.InitFromOfferPack(_pack);

        // Items
        int amount = _pack.items.Count;

        if (m_layouts.Count < amount || m_layouts[amount -1 ] == null)
        {
            Debug.LogError("There is no layout defined for a pack with " + amount + " items");
        }

        // Find the proper layout for the amount of items in the pack
        OffersLayout layout = m_layouts[amount -1];
        for (int i = 0; i < layout.m_offerItems.Count; ++i)
        {
            // Skip if no slot (i.e. single item layouts)
            OfferItemSlot slot = layout.m_offerItems[i];
            if (slot == null) continue;

            // Start hidden and initialize after some delay
            // [AOC] We do this because initializing the slots at the same time
            //		 that the popup is being instantiated results in weird behaviours
            slot.InitFromItem(null);
            if (i < m_pack.items.Count)
            {
                OfferPackItem item = m_pack.items[i];
                m_itemsToSet.Add(item);
                m_slotsToSet.Add(slot);
            }
        }

        // Enable the current layout. Disable others
        foreach (OffersLayout lay in m_layouts)
        {
            lay.m_container.gameObject.SetActive(lay == layout);
        }

        
    }


    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}