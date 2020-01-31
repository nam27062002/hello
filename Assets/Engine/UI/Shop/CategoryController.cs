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
public class CategoryController : MonoBehaviour {


    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    [SerializeField] protected Transform m_pillsContainer;

    // Pills prefabs
    [Space]
    [SerializeField] private IPopupShopPill m_offerPackPillPrefab = null;
    [SerializeField] private IPopupShopPill m_freeOfferPillPrefab = null;
    [SerializeField] private IPopupShopPill m_removeAdsPillPrefab = null;

    // Internal
    protected ShopCategory m_shopCategory;
    protected ShopController m_shopController;
    protected List<OfferPack> m_offers;

    protected List<IPopupShopPill> m_offerPills;
    public List<IPopupShopPill> offerPills
        { get { return m_offerPills; } }


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {

        m_offers = new List<OfferPack>();
        m_offerPills = new List<IPopupShopPill>();

    }

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

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
    public virtual void Initialize (ShopCategory _shopCategory, List<OfferPack> offers = null)
    {

        /// Remove possible mockup elements saved in the prefab
        Clear();

        // Keep a reference to the shop category
        m_shopCategory = _shopCategory;

        // If offers are provided, use them!
        Refresh(offers);
        
    }

    /// <summary>
    /// Refresh all the timers in this category container, and cascade it to the offer pills inside
    /// </summary>
    public virtual void RefreshTimers()
    {

        if (m_offerPills != null)
        {
            foreach (IPopupShopPill pill in m_offerPills)
            {
                pill.RefreshTimer();
            }
        }
    }

    /// <summary>
    /// Enable/Disable all the horizontal/vertical layouts of the shop for performance reasons
    /// </summary>
    /// <param name="enable">True to enable, false to disable</param>
    public void SetLayoutGroupsActive(bool enable)
    {
        HorizontalOrVerticalLayoutGroup layout;

        layout = GetComponent<VerticalLayoutGroup>();
        if (layout != null) {
            layout.enabled = enable;
        }

        layout = m_pillsContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout != null) {
            layout.enabled = enable;
        }

        GetComponent<ContentSizeFitter>().enabled = enable;
        m_pillsContainer.GetComponent<ContentSizeFitter>().enabled = enable;
        
    }


    /// <summary>
    /// Remove all the pills and the header from this container
    /// </summary>
    protected virtual void Clear()
    {
        m_pillsContainer.DestroyAllChildren(true);
        m_offers.Clear();
        m_offerPills.Clear();
    }

    protected virtual void Refresh(List<OfferPack> _offers = null)
    {
        // Remove existing content
        Clear();

        PopulatePills(_offers);
    }

    /// <summary>
    /// Populate the container with all the active offers in this category
    /// </summary>
    /// <param name="_offers">If null, will query the offers from the offer manager</param>
    protected virtual void PopulatePills (List<OfferPack> _offers = null)
    {

        if (_offers == null)
        {
            // Get all the offers in this category
            _offers = OffersManager.GetOfferPacksByCategory(m_shopCategory);
        }

        foreach (OfferPack offer in _offers)
        {

            // Instantiate the offer with the proper prefab
            IPopupShopPill pill = InstantiatePill(offer.type);
            

            if (pill != null)
            {
                // Initialize the prefab with the offer values
                pill.InitFromOfferPack(offer);

                // Insert the pill in the container
                pill.transform.SetParent(m_pillsContainer,false);

                // Keep a record of all the offers, and pills in this category
                m_offers.Add(offer);
                m_offerPills.Add(pill);

                pill.gameObject.SetActive(true);
            }

        }
    }


    /// <summary>
    /// Return true if there are no offers in this category
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
        return (m_offers.Count == 0);
    }

    /// <summary>
    /// Instantiate a pill of the specified type
    /// </summary>
    /// <param name="_type">Type of the offer pack</param>
    /// <returns></returns>
    public virtual IPopupShopPill InstantiatePill(OfferPack.Type _type)
    {
        IPopupShopPill pill;
        
        // Create new instance of prefab
        switch (_type)
        {
            case OfferPack.Type.FREE:
                {
                    pill = Instantiate(m_freeOfferPillPrefab);
                }
                break;

            case OfferPack.Type.REMOVE_ADS:
                {
                    pill = Instantiate(m_removeAdsPillPrefab);
                }
                break;

            default:
                {
                    pill = Instantiate(m_offerPackPillPrefab);
                }
                break;
        }

        return pill;

    }


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}