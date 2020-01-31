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