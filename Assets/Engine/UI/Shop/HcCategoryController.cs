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
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class HcCategoryController : CategoryController
{


    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    [Separator("HC category specifics")]
    [SerializeField]
    protected IShopPill m_HcPillPrefab;


    [Tooltip("The HC offer pack with this order will be displayed in a big pill at the end")]
    [SerializeField]
    protected int m_bigPillOrder = 6;

    [SerializeField]
    protected IShopPill m_HcBigPillPrefab;

    [SerializeField]
    protected Transform m_bigPillContainer;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Initialize one pill
    /// </summary>
    /// <param name="_offer"></param>
    protected override void InitializePill(OfferPack _offer)
    {

        bool isLarge = _offer.order == m_bigPillOrder;

        // Instantiate the offer with the proper prefab
        IShopPill pill = InstantiatePill(isLarge);

        if (pill != null)
        {
            // Initialize the prefab with the offer values
            pill.InitFromOfferPack(_offer);


            // Insert the pill in the proper container
            if (isLarge) {
                pill.transform.SetParent(m_bigPillContainer, false);
            }
            else
            {
                pill.transform.SetParent(m_pillsContainer, false);
            }
            
            // Keep a record of all the offers, and pills in this category
            m_offers.Add(_offer);
            m_offerPills.Add(pill);

            // Show the pill with an animation
            if (pill.gameObject.GetComponent<ShowHideAnimator>() != null)
            {
                pill.gameObject.GetComponent<ShowHideAnimator>().ForceShow(true);
            }
        }

    }

    /// <summary>
    /// Instantiate a pill of the specified type
    /// </summary>
    /// <param name="_type">Type of the offer pack</param>
    /// <returns></returns>
    private IShopPill InstantiatePill(bool large)
    {

        IShopPill pill;

        if (large)
        {
            pill = Instantiate<IShopPill>(m_HcBigPillPrefab);
        }
        else
        {
            pill = Instantiate<IShopPill>(m_HcPillPrefab);
        }
        

        return pill;

    }

    /// <summary>
    /// Remove all the pills and the header from this container
    /// </summary>
    protected override void Clear()
    {
        base.Clear();

        if (m_bigPillContainer != null)
        {
            m_bigPillContainer.DestroyAllChildren(true);
        }
    }

    /// <summary>
    /// Enable/Disable all the horizontal/vertical layouts of the shop for performance reasons
    /// </summary>
    /// <param name="enable">True to enable, false to disable</param>
    public override void SetLayoutGroupsActive(bool enable)
    {

        VerticalLayoutGroup layout = GetComponent<VerticalLayoutGroup>();
        if (layout != null)
        {
            layout.enabled = enable;
        }

        HorizontalLayoutGroup layoutHoriz = m_pillsContainer.parent.GetComponent<HorizontalLayoutGroup>();
        if (layoutHoriz != null)
        {
            layoutHoriz.enabled = enable;
        }

        GridLayoutGroup layouGrid = m_pillsContainer.GetComponent<GridLayoutGroup>();
        if (layouGrid != null)
        {
            layouGrid.enabled = enable;
        }

        GetComponent<ContentSizeFitter>().enabled = enable;
        m_pillsContainer.parent.GetComponent<ContentSizeFitter>().enabled = enable;
        m_pillsContainer.GetComponent<ContentSizeFitter>().enabled = enable;

    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}