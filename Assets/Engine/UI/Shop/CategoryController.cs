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

    // Internal
    protected ShopCategory m_shopCategory;
    protected ShopController m_shopController;
    protected List<OfferPack> m_offers;

    // Asynch loading for better performance. Initialize one category per frame.
    private Queue<OfferPack> pillsToInitialize;

    protected List<IShopPill> m_offerPills;
    public List<IShopPill> offerPills
        { get { return m_offerPills; } }


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {

        m_offers = new List<OfferPack>();
        m_offerPills = new List<IShopPill>();
        pillsToInitialize = new Queue<OfferPack>();
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

        // Initialize one pill each frame
        if (pillsToInitialize.Count > 0)
        {
            InitializePill(pillsToInitialize.Dequeue());
        }
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
            foreach (IShopPill pill in m_offerPills)
            {
                pill.RefreshTimer();
            }
        }
    }

    /// <summary>
    /// Enable/Disable all the horizontal/vertical layouts of the shop for performance reasons
    /// </summary>
    /// <param name="enable">True to enable, false to disable</param>
    public virtual void SetLayoutGroupsActive(bool enable)
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
        pillsToInitialize.Clear();
    }

    protected virtual void Refresh(List<OfferPack> _offers = null)
    {
        // Remove existing content
        Clear();

        PopulatePills(_offers);
    }

    /// <summary>
    /// Enqueue all the active offers in this category so they will be initialized progressively
    /// </summary>
    /// <param name="_offers">If null, will query the offers from the offer manager</param>
    protected virtual void PopulatePills (List<OfferPack> _offers = null)
    {

        if (_offers == null)
        {
            // If offers are not provided, get them all from the offers manager
            _offers = OffersManager.GetOfferPacksByCategory(m_shopCategory);
        }

        // Enqueue all the offers so they are initialized once per frame
        foreach (OfferPack offer in _offers)
        {
            pillsToInitialize.Enqueue(offer);
        }
    }


    /// <summary>
    /// Initialize one pill
    /// </summary>
    /// <param name="_offer"></param>
    protected virtual void InitializePill (OfferPack _offer)
    {
        // Instantiate the offer with the proper prefab
        IShopPill pill = InstantiatePill(_offer.type);


        if (pill != null)
        {
            // Initialize the prefab with the offer values
            pill.InitFromOfferPack(_offer);

            // Insert the pill in the container
            pill.transform.SetParent(m_pillsContainer, false);

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
    /// Return true if there are no offers in this category
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
        return (m_offers.Count == 0);
    }

    /// <summary>
    /// Returns true if the pills initialization has been completed
    /// </summary>
    /// <returns></returns>
    public bool IsFinished ()
    {
        return (pillsToInitialize.Count == 0);
    }

    /// <summary>
    /// Instantiate a pill of the specified type
    /// </summary>
    /// <param name="_type">Type of the offer pack</param>
    /// <returns></returns>
    private IShopPill InstantiatePill(OfferPack.Type _type)
    {
        IShopPill pill;

        // Create new instance of prefab
        IShopPill prefab = ShopSettings.GetPrefab(_type);

        Debug.Assert(prefab != null, "There is no prefab defined for offer type " + _type + " in ShopSettings");

        pill = Instantiate<IShopPill>(prefab);
        
        return pill;

    }


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}