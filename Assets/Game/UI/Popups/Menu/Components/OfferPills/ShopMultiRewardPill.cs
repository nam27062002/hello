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

    private OffersLayout m_currentLayout;
    public OffersLayout currentLayout { get { return m_currentLayout; } }

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

		// Special case if pack is null: clear all layouts
		if(_pack == null) {
			Clear();
			return;
		}

		// Find out the proper layout based on the amount of items in the pack
        int amount = _pack.items.Count;

		for(int i = 0; i < m_layouts.Count; ++i) {
			// Is it our target layout?
			if(i + 1 == amount) {
				// Yes!! Is the layout valid?
				if(m_layouts[i].m_container != null) {
					// Yes!! Use it
					m_currentLayout = m_layouts[i];
					for(int j = 0; j < m_currentLayout.m_offerItems.Count; ++j) {
						// Skip if no slot (i.e. single item layouts)
						OfferItemSlot slot = m_currentLayout.m_offerItems[j];
						if(slot == null) continue;

						// Start hidden and initialize after some delay
						// [AOC] We do this because initializing the slots at the same time
						//		 that the popup is being instantiated results in weird behaviours
						slot.InitFromItem(null);
						if(j < m_pack.items.Count) {
							OfferPackItem item = m_pack.items[j];
							m_itemsToSet.Add(item);
							m_slotsToSet.Add(slot);
						}
					}
				} else {
					// No! Throw error
					Debug.LogError("There is no layout defined for a pack with " + amount + " items");
				}
			}

			// Only activate target layout
			if(m_layouts[i].m_container != null) {
                if (m_currentLayout == m_layouts[i])
                {
                    
				    m_layouts[i].m_container.gameObject.SetActive(true);

				}
                else
                {
					m_layouts[i].m_container.gameObject.SetActive(false);

				}
			}
		}

		// We need to register if the user has seen the progression packs for tracking purposes.
        // This solution is not optimal, because push offers also use this pill, but as long as they are
        // activated in the same run, this should do the trick
		UsersManager.currentUser.progressionPacksDiscovered = true;
    }

	/// <summary>
	/// Clear all layouts and item slots.
	/// </summary>
	protected void Clear() {
		for(int i = 0; i < m_layouts.Count; ++i) {
			// Skip if layout is not valid
			if(m_layouts[i].m_container == null) continue;

			// Clear all item slots in the layout
			for(int j = 0; j < m_layouts[i].m_offerItems.Count; ++j) {
				m_layouts[i].m_offerItems[j].InitFromItem(null);
			}

			// Disable the layout
			m_layouts[i].m_container.gameObject.SetActive(false);
		}
	}

    protected override void ApplyShopPack()
    {
        base.ApplyShopPack();

        // Tell the menu controller to open the shop after the rewards screen
        InstanceManager.menuSceneController.interstitialPopupsController.SetFlag(MenuInterstitialPopupsController.StateFlag.OPEN_SHOP, true);
    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}