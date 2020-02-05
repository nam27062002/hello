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
public class RotationalCategoryController : CategoryController
{


    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//


    // Pills prefabs
    [SerializeField] private IPopupShopPill m_rotationalPillPrefab = null;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    protected override void Clear()
    {
        base.Clear();

    }

    
    public override IPopupShopPill InstantiatePill(OfferPack.Type _type)
    {
        IPopupShopPill pill = null;

        // Create new instance of prefab
        switch (_type)
        {
            case OfferPack.Type.ROTATIONAL:
                {
                    pill = Instantiate(m_rotationalPillPrefab);
                }
                break;
        }

        return pill;

    }


    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}