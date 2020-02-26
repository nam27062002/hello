// ShopController.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 21/01/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class ShopController : MonoBehaviour {


    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    private const string SHOP_CATEGORIES_CONTAINER_PREFABS_PATH = "UI/Shop/CategoryContainers/";
    private const float REFRESH_FREQUENCY = 1f;	// Seconds

    private const string OFFERS_CATEGORY_SKU = "progressionPacks";
    private const string PC_CATEGORY_SKU = "hcPacks";
    private const string SC_CATEGORY_SKU = "scPacks";  

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    // Offer categories
    [SerializeField] private Transform m_categoriesContainer;

    // Horizontal Scroller
    [SerializeField] private ScrollRect m_scrollRect;

    // Shortcuts
    [SerializeField] private Transform m_shortcutsContainer;
    [SerializeField] private ShopCategoryShortcut m_shortcutPrefab;

    // Pills initialization
    [Range(1,5)]
    [Tooltip("Amount of frames that will take for each pill to be displayed. Creates a cool effect when opening the shop")]
    [SerializeField]
    private int m_framesDelayPerPill = 1;


    //Internal
    private float m_timer = 0; // Refresh timer
    private bool m_scrolling = false; // The tweener scrolling animation is running
    private float m_scrollViewOffset;
    
    //Filtering categories
    private string m_categoryToShow; 

    // Cache the category containers and pills
    private List<CategoryController> m_categoryContainers;
    private List<IShopPill> m_pills;

    // Shortcuts
    private List<ShopCategoryShortcut> m_shortcuts; 
    private Dictionary<string, ShopCategoryShortcut> m_skuToShorcut; // Cache the sku-shortcut pair to improve performance
    private string m_lastShortcut;
    private ShopCategory m_lastCategory;

    // Keep the bounds of the current category, so we dont recalculate every time the user scrolls the shop
    private float categoryLeftBorder, categoryRightBorder;

    // True if all the pills have already been drawn
    private bool shopReady = false;
    private bool optimizationActive = false;

    // Optimization #1: disable layouts after refresh
    private bool layoutGropusActive = false;
    private bool disableLayoutsGroupsInNextFrame = false; // If true, disable layout groups in the next update

    // Optimization #2: Disable pills that are outside of the visible scrollable panel
    private float normalizedViewportWidth;
    private bool m_hidePillsOutOfView = false;

    // Optimization #3: initialize one pill per frame
    private Queue<ShopCategory> categoriesToInitialize;
    private CategoryController catBeingInitialized;

    // Aux var to turn optimization on/off from the inspector
    [System.NonSerialized]
    public bool useOptimization = true;


    // Callback for successful purchases
    private UnityAction<IShopPill> m_purchaseCompletedCallback;
    public UnityAction<IShopPill> purchaseCompletedCallback { get { return m_purchaseCompletedCallback; } }

    // Frame counter for pills initialization effect
    private int m_frameCounter;
    public int frameCounter { get { return m_frameCounter; } }

    

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//



    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {
        m_shortcuts = new List<ShopCategoryShortcut>();
        m_skuToShorcut = new Dictionary<string, ShopCategoryShortcut>();
        m_categoryContainers = new List<CategoryController>();
        m_pills = new List<IShopPill>();
        categoriesToInitialize = new Queue<ShopCategory>();

        // React to offers being reloaded 
        Messenger.AddListener(MessengerEvents.OFFERS_RELOADED, OnOffersReloaded);
        Messenger.AddListener<List<OfferPack>>(MessengerEvents.OFFERS_CHANGED, OnOffersChanged);
    }

    /// <summary>
    /// Destructor
    /// </summary>
    private void OnDestroy()
    {
        Messenger.RemoveListener(MessengerEvents.OFFERS_RELOADED, OnOffersReloaded);
        Messenger.RemoveListener<List<OfferPack>>(MessengerEvents.OFFERS_CHANGED, OnOffersChanged);
    }
    

    /// <summary>
    /// First update call.
    /// </summary>
    private void Start() {

        InvokeRepeating("PeriodicRefresh", 0f, REFRESH_FREQUENCY);
        shopReady = false;
    }


	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

        // Do not update if the shop is not open
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        // When the shop is ready, turn on the optimizations
        if (shopReady && !optimizationActive && useOptimization)
        {
            // Wait one frame
            UbiBCN.CoroutineManager.DelayedCallByFrames(() => {
                // Enable the performance optimization
                SetOptimizationActive(true);
            }, 1);
        }

        // Has this category initialization already finished ?
        if (catBeingInitialized != null && catBeingInitialized.IsFinished())
        {
            // Keep a reference to the new pills created
            m_pills.AddRange(catBeingInitialized.offerPills);

            // Force the category as dirty to be redrawn
            catBeingInitialized.GetComponent<LayoutGroup>().enabled = false;
            catBeingInitialized.GetComponent<LayoutGroup>().enabled = true;

            // This category has been initialized succesfully!
            catBeingInitialized = null;

            // Are there some categories left to initialize?
            if (categoriesToInitialize.Count == 0)
            {
                // At the end of initialization, user will be looking at the first category
                if (m_shortcuts.Count > 0)
                {
                    //CalculateCategoryBounds(m_shortcuts[0].categoryController.category);
                }

                shopReady = true;
            }
        }

        // Initialize the next category in the queue
        if (categoriesToInitialize.Count > 0)
        {
            if (catBeingInitialized == null)
            {
                ShopCategory cat = categoriesToInitialize.Dequeue();
                catBeingInitialized = InitializeCategory(cat, cat.offers);
            }
        }


        // Update frame counter
        m_frameCounter--;
        if (m_frameCounter < 0)
        {
            m_frameCounter = m_framesDelayPerPill;
        }

    }



    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Initialize the shop with the requested mode. Should be called before opening the popup.
    /// </summary>
    /// <param name="_mode">Target mode.</param>
    /// <param name="_purchaseCompletedCallback">If provided, this action will be called each time an offer in
    /// this shop is successfully purchased</param>
    public void Init(PopupShop.Mode _mode, UnityAction<IShopPill> _purchaseCompletedCallback = null)
    {
        int timer = Environment.TickCount;

        switch (_mode)
        {
            case PopupShop.Mode.PC_ONLY:
                m_categoryToShow = PC_CATEGORY_SKU;
                break;
            case PopupShop.Mode.SC_ONLY:
                m_categoryToShow = SC_CATEGORY_SKU;
                break;
        }

        // In case we need to do something after the user purchases an offer
        m_purchaseCompletedCallback = _purchaseCompletedCallback;

        Refresh();

        Debug.Log("Init time: " + (Environment.TickCount - timer) + " ms");
    }


    /// <summary>
    /// Remove all the content of the shop
    /// </summary>
    public void Clear()
    {

        Debug.Log("Clear");

        // Clean the containers
        m_categoriesContainer.transform.DestroyAllChildren(true);
        m_shortcutsContainer.transform.DestroyAllChildren(true);

        // Clean categories
        m_categoryContainers.Clear();

        // Clean pills cache
        m_pills.Clear();

        // Remove the shortcut references
        m_shortcuts.Clear();
        m_skuToShorcut.Clear();

        categoriesToInitialize.Clear();

        layoutGropusActive = false;
        disableLayoutsGroupsInNextFrame = false;
        m_hidePillsOutOfView = false;

        m_frameCounter = 0;
    }



    /// <summary>
    /// Clear the shop and populate the shop with all the categories, shortcuts and offer pills
    /// </summary>
    public void Refresh ()
    {
        Clear();

        // Turn off optimizations while refreshing
        SetOptimizationActive(false);
        shopReady = false;

        m_lastShortcut = null;

        // Iterate all the active categories
        foreach (ShopCategory category in OffersManager.instance.activeCategories)
        {
            // Filtering categories if needed
            if (m_categoryToShow == null || category.sku == m_categoryToShow)
            {
                // If this cat is active
                if (category.enabled)
                {

                    // Get the offer packs that belongs to this category
                    category.offers = OffersManager.GetOfferPacksByCategory(category);

                    // Make sure there are offers in this category
                    if (category.offers.Count > 0)
                    {

                        CreateShortcut(category);

                        // Enqueue the categories so they are initialized one per frame
                        categoriesToInitialize.Enqueue(category);
                    }
                }
            }
        }
    }


    /// <summary>
    /// Called at regular intervals.
    /// </summary>
    private void PeriodicRefresh()
    {
        // Nothing if not enabled
        if (!this.isActiveAndEnabled) return;

        if (shopReady)
        {
            foreach (CategoryController cat in m_categoryContainers)
            {
                // Propagate to categories
                cat.RefreshTimers();
            }
        }

    }

    /// <summary>
    /// Refresh the pills inside a category. Instead of refreshing the whole shop just clear 
    /// the category container affected and repopulate it. More performance eficient this way.
    /// </summary>
    /// <param name="_categorySKU">The sku of the shop category to refresh</param>
    public void RefreshCategory(ShopCategory _category)
    {

        // If this cat is inactive just return
        if (_category == null || ! _category.enabled)
            return;


        // Turn off optimizations while refreshing
        SetOptimizationActive(false);
        shopReady = false;

        // Find the category container
        CategoryController container = m_categoryContainers.Find(c => c.category.sku == _category.sku);

        // Safety check
        if (container == null)
            return; 

        // Remove pill references
        foreach (IShopPill pill in container.offerPills)
        {
            m_pills.Remove(pill);
        }

        // Get the offer packs that belongs to this category
        List<OfferPack> offers = OffersManager.GetOfferPacksByCategory(_category);

        // Make sure there are offers in this category
        if (offers.Count > 0 )
        {
            container.Initialize(_category, offers);
            catBeingInitialized = container;

        }
        else
        {
            // The category is now empty. Remove it from the shop:

            //Remove the container
            GameObject.DestroyImmediate(container);

            //Remove the shortcut from the bottom bar.
            GameObject.DestroyImmediate(m_skuToShorcut[_category.sku].gameObject);

            // Remove cached references
            m_categoryContainers.Remove(container);
            m_shortcuts.Remove(m_skuToShorcut[_category.sku]);
            m_skuToShorcut.Remove(_category.sku);

            // Nothing else to do
            shopReady = true;

        }
    }


    /// <summary>
    /// Initialize one category container with offers and create its shortcut if needed
    /// </summary>
    /// <param name="_cat">Shop category</param>
    /// <param name="_offers">List of offers contained in this category</param>
    /// <returns>Reference to the category container created</returns>
    private CategoryController InitializeCategory (ShopCategory _cat, List<OfferPack> _offers)
    {

        Debug.Log("Initialize category " + _cat.sku);

        // Instantiate the shop category
        string containerPrefabPath = SHOP_CATEGORIES_CONTAINER_PREFABS_PATH + _cat.containerPrefab;
        CategoryController containerPrefab = Resources.Load<CategoryController>(containerPrefabPath);


        if (containerPrefab == null)
        {
            // The container prefab was not found
            Debug.LogError("The prefab " + containerPrefabPath + " was not found in the project");
            return null;
        }

        // Make sure there are offers in this category
        if (_offers.Count > 0)
        {

            CategoryController categoryContainer = Instantiate<CategoryController>(containerPrefab);

            // Add the category container to the hierarchy
            categoryContainer.transform.SetParent(m_categoriesContainer, false);
            categoryContainer.Initialize(_cat, _offers);

            m_categoryContainers.Add(categoryContainer);


            // Link the shortcut to the first container of the category
            if (m_lastCategory == null || m_lastCategory != _cat)
            {
                
                if (m_skuToShorcut.ContainsKey(_cat.sku))
                {
                    ShopCategoryShortcut sc = m_skuToShorcut[_cat.sku];
                    sc.SetCategory(categoryContainer);
                }
                
            }

            m_lastCategory = _cat;
            return categoryContainer;
        }

        // Empty category
        return null;
    }


    /// <summary>
    /// Scroll the viewport to the left border of the shop
    /// </summary>
    public void ScrollToStart()
    {
        if (m_pills.Count != 0)
        {
            ScrollToItem(m_pills[0].transform);
        }
    }


    //------------------------------------------------------------------------//
    // SHORTCUTS                											  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Create a new shortcut in the navigation bar. 
    /// Keeps a record of the last shortcut, so it doesnt duplicate them.
    /// </summary>
    /// <param name="_cat">Shop category. </param>
    private void CreateShortcut(ShopCategory _cat)
    {
        // Has a shortcut in the bottom menu?
        if (!string.IsNullOrEmpty(_cat.tidShortcut))
        {
            // If two categories share a shortcut, dont create it twice
            if (m_lastShortcut != _cat.tidShortcut)
            {

                // Create a new shortcut :D
                // Instantiate a shortcut and add it to the bottom bar
                ShopCategoryShortcut newShortcut = Instantiate<ShopCategoryShortcut>(m_shortcutPrefab, m_shortcutsContainer, false);

                // Set the title of the shortcat
                newShortcut.Initialize(_cat.tidShortcut, this);

                m_shortcuts.Add(newShortcut);
                m_skuToShorcut.Add(_cat.sku, newShortcut);

                // Keep a record of the last shortcut created
                m_lastShortcut = _cat.tidShortcut;
            }
        }
    }

    /// <summary>
    /// Highlight the selected shortcut (and deselect the rest)
    /// </summary>
    /// <param name="_sku">SKU of the selected category</param>
    private void SelectShortcut(ShopCategoryShortcut _sc)
    {
        // Deselect all shortcuts except the chosen one
        foreach (ShopCategoryShortcut shortcut in m_shortcuts)
        {
            bool selectedCat = (shortcut == _sc);
            shortcut.Select(selectedCat);
        }

    }


    /// <summary>
    /// Scroll the viewport to the selected category
    /// </summary>
    /// <param name="anchor"></param>
    private void ScrollToItem(Transform anchor)
    {

        if (anchor != null)
        {

            m_scrolling = true;

            // Create a tweener to animate the scroll
            m_scrollRect.DOGoToItem(anchor, .5f, 0.001f)
            .SetEase(Ease.OutBack)
            .OnComplete(delegate () { m_scrolling = false; });

        }
    }


    /// <summary>
    /// Finds the horizontal bounds of the category, so wont recalculate the current category
    /// unless the user scrolls outside of these bounds. 
    /// </summary>
    /// <param name="_cat">The category. Only works for categories that have a shortcut.</param>
    private void CalculateCategoryBounds (ShopCategory _cat)
    {

        // Only works for categories that have a shortcut
        if (! m_skuToShorcut.ContainsKey(_cat.sku) )
            return;

        // Get the related shortcut
        ShopCategoryShortcut sc = m_skuToShorcut[_cat.sku];

        int index = m_shortcuts.IndexOf(sc);

        // Calculate left border of the current category
        if (index == 0)
        {
            categoryLeftBorder = float.MinValue;
        }
        else
        {
            categoryLeftBorder = m_scrollRect.GetNormalizedPositionForItem(m_shortcuts[index].categoryController.transform).x + m_scrollViewOffset;
        }


        // Calculate right border of the current category
        if (index == m_shortcuts.Count - 1)
        {
            categoryRightBorder = float.MaxValue;
        }
        else
        {
            categoryRightBorder = m_scrollRect.GetNormalizedPositionForItem(m_shortcuts[index + 1].categoryController.transform).x + m_scrollViewOffset;
        }

    }

    /// <summary>
    /// Returns the category that is located in the specified position of the scrollbar
    /// ignore the categories that dont have a shortcut
    /// </summary>
    /// <param name="pos">The coordenates inside the scrollbar</param>
    /// <returns>SKU of the category</returns>
    private ShopCategory GetCategoryAtPosition(float _posX)
    {
        if (m_shortcuts.Count <= 0)
            return null;


        int candidate = m_shortcuts.Count - 1;

        // Iterate all the shortcuts (from right to left)
        for (int i = m_shortcuts.Count - 1; i >= 0; i--)
        {
            ShopCategoryShortcut shortcut = m_shortcuts[i];

            // Get normalized position of the anchor
            Vector2 categoryAnchor = m_scrollRect.GetNormalizedPositionForItem(shortcut.categoryController.transform, true) + new Vector2(m_scrollViewOffset, 0);

            if (_posX >= categoryAnchor.x)
            {
                break;
            }
            else
            {
                candidate--;
            }
        }

        if (candidate < 0)
        {
            candidate = 0;
        }

        return m_shortcuts[candidate].categoryController.category;

    }


    //------------------------------------------------------------------------//
    // OPTIMIZATION METHODS													  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Enables/Disables the performance optimization features
    /// </summary>
    /// <param name="_enable">True to activate it</param>
    public void SetOptimizationActive(bool _enable)
    {
        optimizationActive = _enable;

        if (_enable)
        {
            // Hide pills that are out the view at this moment
            UpdatePillsVisibility(m_scrollRect.normalizedPosition);
        }
        else
        {
            // Show the pills that could be hidden
            foreach (IShopPill pill in m_pills)
            {
                pill.gameObject.SetActive(true);
            }

        }

        // Disable layouts for better performance
        SetLayoutGroupsActive(!_enable);

        // Hide pills out the view
        m_hidePillsOutOfView = _enable;

    }


    /// <summary>
    /// Enable/Disable all the horizontal/vertical layouts of the shop for performance reasons
    /// </summary>
    /// <param name="enable">True to enable, false to disable</param>
    private void SetLayoutGroupsActive (bool enable)
    {
        // Propagate it to categories and pills
        foreach (CategoryController cat in m_categoryContainers)
        {
            cat.SetLayoutGroupsActive(enable);
        }

        m_categoriesContainer.GetComponent<HorizontalLayoutGroup>().enabled = enable;
        m_categoriesContainer.GetComponent<ContentSizeFitter>().enabled = enable;

        layoutGropusActive = enable;
    }


    /// <summary>
    /// Enable/disable pill that are inside/outside of the visible limits of the scrollview.
    /// </summary>
    /// <param name="_scrollPosition"></param>
    private void UpdatePillsVisibility(Vector2 _scrollPosition)
    {

        // Define the width of the viewport. Out of this range, pills are disabled.
        normalizedViewportWidth = m_scrollRect.viewport.rect.width / m_scrollRect.content.rect.width;

        foreach (IShopPill pill in m_pills)
        {
            // Is this pill inside the visible limits of the scrollview (leave some margin)
            float pillPositionX = m_scrollRect.GetRelativePositionOfItem(pill.transform);
            float safetyMargin = .2f;
            bool visible = (pillPositionX > -safetyMargin && pillPositionX < 1 + safetyMargin);

            // Enable/disable the pill
            pill.gameObject.SetActive(visible);
        }
    }



    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// A shortcut was pressed
    /// </summary>
    /// <param name="_category">The category related to the shortcut</param>
    public void OnShortcutSelected(ShopCategoryShortcut _sc)
    {
        if (_sc.categoryController == null)
            return;

        m_scrolling = false;

        SelectShortcut(_sc);

        Transform categoryAnchor = _sc.categoryController.anchor;
        ScrollToItem(categoryAnchor);

    }


    /// <summary>
    /// The user moved the scrolled the items in the shop. Find the category that is in the middle
    /// of the viewport and highlight the proper shortcut in the bottom bar
    /// </summary>
    /// <param name="_newPos">Normalized position of the scroll view</param>
    public void OnScrollChanged(Vector2 _newPos)
    {

        // Wait for the layouts groups to be rendered
        if (m_hidePillsOutOfView)
        {
            // Disable pills outside of the view limits
            UpdatePillsVisibility(_newPos);
        }

        // Scrolling animation still running
        if (m_scrolling || !shopReady) {
            return;
        }

        float posX = Mathf.Clamp01(_newPos.x);

        // Calculate the new category position only when the user moved out of the current category bounds 
        if (posX < categoryLeftBorder || posX > categoryRightBorder)
        {

            // What category are we looking at now?
            ShopCategory focusedCategory = GetCategoryAtPosition(posX);

            if (focusedCategory != null)
            {
                // Recalculate the new bounds
                CalculateCategoryBounds(focusedCategory);

                // Highlight the proper shortcut
                SelectShortcut(m_skuToShorcut[focusedCategory.sku]);

            }
        }
    }

    /// <summary>
    /// Offers have been reloaded.
    /// </summary>
    private void OnOffersReloaded()
    {
        // Ignore if not active
        if (!this.isActiveAndEnabled) return;

        // Refresh the shop if the shop is ready (avoid interrupting init animation bug)
        if (shopReady)
        {
            Refresh();
        }
    }

    /// <summary>
    /// Offers list has changed.
    /// </summary>
    /// <param name="offersChanged">A list with the offer packs that have changed</param>
    private void OnOffersChanged(List<OfferPack> offersChanged = null)
    {
        // Ignore if not active
        if (!this.isActiveAndEnabled) return;

        // Refresh the shop if the shop is ready (avoid interrupting init animation bug)
        if (shopReady)
        {

            List<ShopCategory> categoriesAffected = new List<ShopCategory>();

            foreach (OfferPack offer in offersChanged)
            {
                if (offer.shopCategory != null)
                {
                    ShopCategory category = OffersManager.instance.activeCategories.Find(sc => sc.sku == offer.shopCategory);
                    
                    // Avoid refreshing several times the same category
                    if (!categoriesAffected.Contains(category))
                    {
                        categoriesAffected.Add(category);
                    }
                }
            }

            foreach (ShopCategory cat in categoriesAffected)
            {
                RefreshCategory(cat);
            }
        }
    }
}