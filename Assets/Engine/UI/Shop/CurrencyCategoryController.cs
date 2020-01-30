// ShopCategoryController.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 22/01/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections.Generic;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class CurrencyCategoryController : CategoryController
{


    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    [Space]
    [SerializeField] protected Transform m_headerContainer;

    // Pills prefabs
    [SerializeField] private IPopupShopPill m_scPackPillPrefab = null;
    [SerializeField] private IPopupShopPill m_hcPackPillPrefab = null;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    protected override void Clear()
    {
        base.Clear();

        m_headerContainer.DestroyAllChildren(true);

    }

    /// <summary>
    /// Populate the container with all the active offers in this category
    /// </summary>
    protected override void PopulatePills(List<OfferPack> _offers = null)
    {

        if (_offers == null)
        {
            // Get all the offers in this category
            _offers = OffersManager.GetOfferPacksByCategory(m_shopCategory);
        }

        foreach (OfferPack offer in _offers)
        {

            // Instantiate the offer with the proper prefab
            IPopupShopPill pill;

            // Create new instance of prefab
            switch (offer.type)
            {
                case OfferPack.Type.SC:
                    {
                        pill = Instantiate(m_scPackPillPrefab);
                    }
                    break;
                case OfferPack.Type.HC:
                    {
                        pill = Instantiate(m_hcPackPillPrefab);
                    }
                    break;

                default:
                    return;
               
            }

            if (pill != null)
            {
                // Initialize the prefab with the offer values
                pill.InitFromOfferPack(offer);

                // Insert the pill in the container
                pill.transform.SetParent(m_pillsContainer, false);

                // Keep a record of all the offers, and pills in this category
                m_offers.Add(offer);
                m_offerPills.Add(pill);

                pill.gameObject.SetActive(true);
            }

        }

    }


    public override IPopupShopPill InstantiatePill(OfferPack.Type _type)
    {
        IPopupShopPill pill = null;

        // Create new instance of prefab
        switch (_type)
        {
            case OfferPack.Type.SC:
                {
                    pill = Instantiate(m_scPackPillPrefab);
                }
                break;
            case OfferPack.Type.HC:
                {
                    pill = Instantiate(m_hcPackPillPrefab);
                }
                break;
        }

        return pill;

    }


    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}