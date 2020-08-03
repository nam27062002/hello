// PopupShopMonorewardPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/03/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System;
using System.Text;
using System.Collections.Generic;

using TMPro;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single pill in the shop for an offer pack.
/// </summary>
public class ShopMonoRewardPill : ShopBasePill {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed
    [Separator("Mono Reward Specifics")]
    [SerializeField] protected OfferItemSlot m_offerItemSlot = null;

    public OfferItemSlot offerItemSlot
    {
        set
        {
            m_offerItemSlot = value;
            InitFromOfferPack(m_pack);
        }
    }

    // Internal
    protected int m_itemIndex = 0;

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
    /// <param name="_itemIndex">Specify which item will be shown in offerPacks containing several items</param>
    public override void InitFromOfferPack(OfferPack _pack) {

        base.InitFromOfferPack(_pack);


		// There is only one item to initialized in monoRewards
        if (m_offerItemSlot != null)
        {

            // Start hidden and initialize after some delay
            // [AOC] We do this because initializing the slots at the same time
            //		 that the popup is being instantiated results in weird behaviours
            m_offerItemSlot.InitFromItem(null);

            if (m_pack == null)
            {
                // we user m_pack = null when we want to hide the pill
                return;
            }

            if (m_pack.items !=null && m_pack.items.Count > m_itemIndex && m_pack.items[m_itemIndex] != null)
            {
                OfferPackItem item = m_pack.items[m_itemIndex];
                m_itemsToSet.Add(item);
                m_slotsToSet.Add(m_offerItemSlot);
            } else {
				Debug.LogError("Attempting to initialize a MonoRewardPill (" + this.name + ") but pack has no valid items");
			}
        }
		
	}







	
}
