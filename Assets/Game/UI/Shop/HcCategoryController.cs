// ShopCategoryController.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 22/01/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System;
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

    [Separator("Happy Hour")]
    [SerializeField]
    protected Localizer m_timer;
    [SerializeField]
    protected GameObject m_happyHourBadge;
    [SerializeField]
    protected Localizer m_happyHourBadgeText;




    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    public HcCategoryController()
    {
        // Subscribe to happy hour events
        Messenger.AddListener(MessengerEvents.HAPPY_HOUR_CHANGED, RefreshHappyHour);
    }

    ~HcCategoryController()
    {
        // Unsubscribe to events
        Messenger.RemoveListener(MessengerEvents.HAPPY_HOUR_CHANGED, RefreshHappyHour);
    }


    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Initialize a category container
    /// </summary>
    /// <param name="_shopCategory">The shop category related</param>
    /// <param name="offers">Offer packs that will be contained in this category. If null, they will be
    /// queried from the offer manager.</param>
    public override void Initialize(ShopCategory _shopCategory, List<OfferPack> offers = null)
    {
        base.Initialize(_shopCategory, offers);


        RefreshHappyHour();

    }


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

    /// <summary>
    /// Refresh all the timers in this category container, and cascade it to the offer pills inside
    /// </summary>
    public override void RefreshTimers()
    {

        base.RefreshTimers();

        
        

    }

    /// <summary>
    /// Update happy hour timer and badge in the header
    /// </summary>
    private void RefreshHappyHour ()
    {
        if (m_timer != null && m_happyHourBadge != null)
        {
            HappyHour happyHour = OffersManager.happyHourManager.happyHour;

            if (happyHour.IsActive())
            {
                // Show time left in the proper format (1h 20m 30s)

                string timeLeft = TimeUtils.FormatTime(happyHour.TimeLeftSecs(), TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 3);
                m_timer.Set(timeLeft);
                m_timer.gameObject.SetActive(true);

                // Happy hour rate

                // Convert offer rate to percentage (example: .5f to +50%) 
                float percentage = happyHour.extraGemsFactor * 100;
                string gemsPercentage = String.Format("{0}", Math.Round(percentage));

                // Show texts with offer rate
                string badgeText = LocalizationManager.SharedInstance.Localize("TID_SHOP_BONUS_AMOUNT", gemsPercentage + "%"); 
                m_happyHourBadgeText.Set(badgeText);

                m_happyHourBadge.SetActive(true);

            }
            else
            {
                m_timer.gameObject.SetActive(false);
                m_happyHourBadge.SetActive(false);
            }
        }
    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}