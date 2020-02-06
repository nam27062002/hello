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
using UnityEngine;
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

    private string SHOP_CATEGORIES_CONTAINER_PREFABS_PATH = "UI/Popups/Economy/CategoryContainers/";
    private const float REFRESH_FREQUENCY = 1f;	// Seconds

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


    //Internal
    private float m_timer = 0; // Refresh timer
    private bool m_scrolling = false; // The tweener scrolling animation is running


    // Cache the category containers and pills
    private List<CategoryController> m_categoryContainers;
    private List<IShopPill> m_pills;

    // Shortcuts
    private List<ShopCategoryShortcut> m_shortcuts; 
    private Dictionary<string, ShopCategoryShortcut> m_skuToShorcut; // Cache the sku-shortcut pair to improve performance
    private string m_lastShortcut;

    // Keep the bounds of the current category, so we dont recalculate every time the user scrolls the shop
    private float categoryLeftBorder, categoryRightBorder;


    // Optimization #1: disable layouts after refresh
    private bool layoutGropusActive = false;
    private bool disableLayoutsGroupsInNextFrame = false; // If true, disable layout groups in the next update

    // Optimization #2: Disable pills that are outside of the visible scrollable panel
    private float normalizedViewportWidth;
    private bool m_hidePillsOutOfView = false;

    // Optimization #3: initialize one pill per frame
    private Queue<ShopCategory> categoriesToInitialize;
    private CategoryController catBeingInitialized;


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
    }

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

        InvokeRepeating("PeriodicRefresh", 0f, REFRESH_FREQUENCY);

        // React to offers being reloaded while tab is active
        Messenger.AddListener(MessengerEvents.OFFERS_RELOADED, OnOffersReloaded);
        Messenger.AddListener(MessengerEvents.OFFERS_CHANGED, OnOffersChanged);


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

        // Has this category initialization already finished ?
        if (catBeingInitialized != null && catBeingInitialized.IsFinished())
        {
            // Keep a reference to the new pills created
            m_pills.AddRange(catBeingInitialized.offerPills);

            catBeingInitialized = null;

            // Have all the categories been initialized?
            if (categoriesToInitialize.Count == 0)
            {
                // Disable layouts for better performance
                SetLayoutGroupsActive(false);

                // At the end of initialization, user will be looking at the first category
                if (m_shortcuts.Count > 0)
                {
                    CalculateCategoryBounds(m_shortcuts[0].category);
                }

                // Hide pills out the view
                m_hidePillsOutOfView = true;
            }
        }

        // Initialize the next category in the queue
        if (categoriesToInitialize.Count > 0)
        {
            if (catBeingInitialized == null)
            {
                ShopCategory cat = categoriesToInitialize.Dequeue();
                catBeingInitialized = InitializeCategory(cat);
            }
        }
    }



    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//



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

    }

    /// <summary>
    /// Initialize the shop with the requested mode. Should be called before opening the popup.
    /// </summary>
    /// <param name="_mode">Target mode.</param>
    public void Init(PopupShop.Mode _mode, string _origin)
    {
        int timer = Environment.TickCount;

        Refresh();

        Debug.Log("Init time: " + (Environment.TickCount - timer) + " ms");
    }

    /// <summary>
    /// Populate the shop with all the categories, shortcuts and offer pills
    /// </summary>
    public void Refresh ()
    {
        Clear();

        m_lastShortcut = null;


        // Iterate all the active categories
        foreach (ShopCategory category in OffersManager.instance.activeCategories)
        {
            // If this cat is active 
            if (category.enabled)
            {
                // Enqueue the categories so they are initialized one per frame
                categoriesToInitialize.Enqueue(category);
            }
        }

        // Enable layout groups just for one frame
        SetLayoutGroupsActive(true);
    }


    /// <summary>
    /// Called at regular intervals.
    /// </summary>
    private void PeriodicRefresh()
    {
        // Nothing if not enabled
        if (!this.isActiveAndEnabled) return;

        foreach (CategoryController cat in m_categoryContainers)
        {
            // Propagate to categories
            cat.RefreshTimers();
        }

    }


    /// <summary>
    /// Initialize one category and create its shortcut if needed
    /// </summary>
    /// <param name="_cat">Shop category</param>
    /// <returns>Reference to the category container created</returns>
    private CategoryController InitializeCategory (ShopCategory _cat)
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

        // TODO: Optimize this: dont call twice to this method! (here and inside the category initialization)
        List<OfferPack> offers = OffersManager.GetOfferPacksByCategory(_cat);

        // Make sure there are offers in this category
        if (offers.Count > 0)
        {

            CategoryController categoryContainer = Instantiate<CategoryController>(containerPrefab);

            // Add the category container to the hierarchy
            categoryContainer.transform.SetParent(m_categoriesContainer, false);
            categoryContainer.Initialize(_cat, offers);

            m_categoryContainers.Add(categoryContainer);

            // Has a shortcut in the bottom menu?
            if (!string.IsNullOrEmpty(_cat.tidShortcut))
            {
                // If two categories share a shortcut, dont create it twice
                if (m_lastShortcut != _cat.tidShortcut)
                {

                    // Create a new shortcut :D

                    // Instantiate a shortcut and add it to the bottom bar
                    ShopCategoryShortcut newShortcut = Instantiate<ShopCategoryShortcut>(m_shortcutPrefab, m_shortcutsContainer, false);
                    newShortcut.Initialize(_cat, categoryContainer.transform, this);
                    m_shortcuts.Add(newShortcut);
                    m_skuToShorcut.Add(_cat.sku, newShortcut);

                    // Keep a record of the last shortcut created
                    m_lastShortcut = _cat.tidShortcut;
                }
            }

            return categoryContainer;
        }

        // Empty category
        return null;
    }


    /// <summary>
    /// Highlight the selected shortcut (and deselect the rest)
    /// </summary>
    /// <param name="_sku">SKU of the selected category</param>
    private void SelectShortcut (ShopCategoryShortcut _sc)
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
    private void ScrollToItem (Transform anchor)
    {

        if (anchor != null)
        {

            m_scrolling = true;

            // Create a tweener to animate the scroll
            // Add a little offset to avoid conflict in between categories
            m_scrollRect.DOGoToItem(anchor, .5f, .001f)
            .SetEase(Ease.OutQuad)
            .OnComplete( delegate() { m_scrolling = false; } );
            
        }
    }


    /// <summary>
    /// Returns the category that is located in the specified position of the scrollbar
    /// ignore the categories that dont have a shortcut
    /// </summary>
    /// <param name="pos">The coordenates inside the scrollbar</param>
    /// <returns>SKU of the category</returns>
    private ShopCategory GetCategoryAtPosition (float _posX)
    {
        if (m_shortcuts.Count <= 0)
            return null;


        int candidate = m_shortcuts.Count - 1;

        // Iterate all the shortcuts (from right to left)
        for (int i = m_shortcuts.Count - 1 ; i >= 0 ; i--)
        {
            ShopCategoryShortcut shortcut = m_shortcuts[i];

            // Get normalized position of the anchor
            Vector2 categoryAnchor = m_scrollRect.GetNormalizedPositionForItem(shortcut.anchor, true);

            if (_posX >= categoryAnchor.x )
            {
                break;
            }
            else {
                candidate--;
            }
        }

        if (candidate < 0) {
            candidate = 0;
        }

        return m_shortcuts[candidate].category;

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
            categoryLeftBorder = m_scrollRect.GetNormalizedPositionForItem(m_shortcuts[index].anchor).x;
        }


        // Calculate right border of the current category
        if (index == m_shortcuts.Count - 1)
        {
            categoryRightBorder = float.MaxValue;
        }
        else
        {
            categoryRightBorder = m_scrollRect.GetNormalizedPositionForItem(m_shortcuts[index + 1].anchor).x;
        }

    }


    /// <summary>
    /// Enable/Disable all the horizontal/vertical layouts of the shop for performance reasons
    /// </summary>
    /// <param name="enable">True to enable, false to disable</param>
    private void SetLayoutGroupsActive (bool enable)
    {
        m_categoriesContainer.GetComponent<HorizontalLayoutGroup>().enabled = enable;
        m_categoriesContainer.GetComponent<ContentSizeFitter>().enabled = enable;

        layoutGropusActive = enable;

        // Cascade it to categories and pills
        foreach (CategoryController cat in m_categoryContainers)
        {
            cat.SetLayoutGroupsActive(enable);
        }
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
            // Is this pill inside the visible limits of the scrollview
            Vector2 pillPosition = m_scrollRect.GetNormalizedPositionForItem(pill.transform, true, false);
            bool visible = (Mathf.Abs(pillPosition.x - _scrollPosition.x) < normalizedViewportWidth);

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
        m_scrolling = false;

        SelectShortcut(_sc);

        Transform categoryAnchor = _sc.anchor;
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
        if (m_scrolling) {
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

        // Refresh the shop
        Refresh();
    }

    /// <summary>
    /// Offers list has changed.
    /// </summary>
    private void OnOffersChanged()
    {
        // Ignore if not active
        if (!this.isActiveAndEnabled) return;

        // Refresh the shop
        Refresh();
    }
}