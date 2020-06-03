// ShopController.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 21/01/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//#define LOG

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System;
using System.Diagnostics;
using System.Collections.Generic;

using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class ShopController : MonoBehaviour
{


    //------------------------------------------------------------------------//
    // ENUM															  //
    //------------------------------------------------------------------------//

    public enum Mode
    {
        DEFAULT,
        SC_ONLY,
        PC_ONLY,
        JUMP_TO_PC
    };


    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    private const string SHOP_CATEGORIES_CONTAINER_PREFABS_PATH = "UI/Shop/CategoryContainers/";
    private const float REFRESH_FREQUENCY = 1f;	// Seconds

    private const string PC_CATEGORY_SKU = "hcPacks";
    private const string SC_CATEGORY_SKU = "scPacks";

	private const float TRACKING_VIEW_MIN_DURATION = 1f;	// Minimum time without scrolling before sending the "view" tracking events

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

    // HUD currency anchors
    [SerializeField] private Transform m_scCounterPosition;
    public Transform scCounterPosition { get { return m_scCounterPosition; } }
    [SerializeField] private Transform m_hcCounterPosition;
    public Transform hcCounterPosition { get { return m_hcCounterPosition; } }

    public bool ShopReady // All the pills have been initialized
        { get; set; } = false;

    //Internal
    private Mode m_mode;    // Shop mode (all offers, only PC, etc)
    private bool m_scrolling = false; // The tweener scrolling animation is still running
    private float m_scrollViewOffset;
    
    //Filtering categories
    private string m_categoryToShow;

	// Cache the category containers and pills
	private List<ShopCategory> m_activeCategories = new List<ShopCategory>();
    private List<CategoryController> m_categoryContainers;
    private List<IShopPill> m_pills;

    // Cache the pill prefabs
    private Dictionary<OfferPack.Type, IShopPill> m_pillPrefabs;




    // Shortcuts
    private List<ShopCategoryShortcut> m_shortcuts; 
    private Dictionary<string, ShopCategoryShortcut> m_skuToShorcut; // Cache the sku-shortcut pair to improve performance
    private string m_lastShortcut;
    private ShopCategory m_lastCategory;

    // Keep the bounds of the current category, so we dont recalculate every time the user scrolls the shop
    private float categoryLeftBorder, categoryRightBorder;


    private bool m_optimizationActive = false; // Current state of the performance optimizations

    // Aux var to turn optimization on/off from the inspector
    [System.NonSerialized]
    public bool m_useOptimization = true;

    // Optimization #1: disable layouts after refresh
    private bool m_layoutGropusActive = false;
    private bool m_disableLayoutsGroupsInNextFrame = false; // If true, will disable layout groups in the next update

    // Optimization #2: Disable pills that are outside of the visible scrollable panel
    private float m_normalizedViewportWidth;
    private bool m_hidePillsOutOfView = false;
	private List<IShopPill> m_visiblePills = new List<IShopPill>();

    // Optimization #3: initialize one pill per frame
    private Queue<ShopCategory> m_categoriesToInitialize;
    private CategoryController m_catBeingInitialized;



    // Parallax effect
    private CameraTraveling m_cameraTraveling;
    public bool m_cameraTravelingEnabled;

    // Remember scroll position
    float m_lastScrollPos;

    // Callback for successful purchases
    private UnityAction<IShopPill> m_purchaseCompletedCallback;
    public UnityAction<IShopPill> purchaseCompletedCallback { get { return m_purchaseCompletedCallback; } }




    // Benchmarking
    private int timestamp, timestamp2;

	// Tracking
	private string m_trackingOrigin = "";	// Track from where the shop has been opened
	private float m_trackingViewTimer = -1f;

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
        m_pillPrefabs = new Dictionary<OfferPack.Type, IShopPill>();
        m_categoriesToInitialize = new Queue<ShopCategory>();

        m_cameraTraveling = GetComponent<CameraTraveling>();

        // React to offers being reloaded 
        Messenger.AddListener(MessengerEvents.OFFERS_RELOADED, OnOffersReloaded);
        Messenger.AddListener<List<OfferPack>>(MessengerEvents.OFFERS_CHANGED, OnOffersChanged);
        Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnTransitionStart);
        Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnTransitionEnd);
    }

    /// <summary>
    /// Destructor
    /// </summary>
    private void OnDestroy()
    {
        Messenger.RemoveListener(MessengerEvents.OFFERS_RELOADED, OnOffersReloaded);
        Messenger.RemoveListener<List<OfferPack>>(MessengerEvents.OFFERS_CHANGED, OnOffersChanged);
        Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnTransitionStart);
        Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnTransitionEnd);
    }

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Reset tracking timer
		m_trackingViewTimer = TRACKING_VIEW_MIN_DURATION;

        m_cameraTravelingEnabled = true;

    }

    private void OnDisable()
    {
        // Dont move the camera while the shop is not active
        m_cameraTravelingEnabled = false;
    }


    /// <summary>
    /// First update call.
    /// </summary>
    private void Start() {

        InvokeRepeating("PeriodicRefresh", 0f, REFRESH_FREQUENCY);
        ShopReady = false;
    }


	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

        if (Input.GetKeyDown(KeyCode.R))
        {
            // Refresh the shop for debugging purposes
            OnOffersChanged();
        }

        // Do not update if the shop is not open
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        // When the shop is ready, turn on the optimizations
        if (ShopReady && !m_optimizationActive && m_useOptimization)
        {
            // Wait one frame
            UbiBCN.CoroutineManager.DelayedCallByFrames(() => {
                // Enable the performance optimization
                SetOptimizationActive(true);

                // Load the previews of the items that are now in the screen
                ShowPreviewInActivePills();

            }, 1);
        }

        // Has the current category initialization already finished ?
        if (m_catBeingInitialized != null && m_catBeingInitialized.IsFinished())
        {
            // Keep a reference to the new pills created
            m_pills.AddRange(m_catBeingInitialized.offerPills);


            
            // This category has been initialized succesfully!
            m_catBeingInitialized = null;
                       
            // Are there some categories left to initialize?
            if (m_categoriesToInitialize.Count == 0)
            {

                // Move the scroll to the proper position

                if (m_mode == Mode.JUMP_TO_PC)
                {
                    // Jump to the HC section
                    ShopCategoryShortcut sc = m_skuToShorcut[PC_CATEGORY_SKU];
                    if ( sc!= null )
                        OnShortcutSelected( sc );
                }
                else if (m_lastScrollPos != 0)
                {
                    // Jump to last known position
                    ScrollToPosition(m_lastScrollPos);
                    // Reset the value so it wont scroll again in the next refresh
                    m_lastScrollPos = 0f;
                }

                ShopReady = true;

                // Benchmarking
                Log(Colors.paleYellow.Tag("Shop initialized in " + (Environment.TickCount - timestamp) + " ms"));
            }
        }

        // Initialize the next category in the queue
        if (m_categoriesToInitialize.Count > 0)
        {
            if (m_catBeingInitialized == null)
            {

                ShopCategory cat = m_categoriesToInitialize.Dequeue();
                m_catBeingInitialized = InitializeCategory(cat, cat.offers);

            }
        }

		// Tracking
		if(m_trackingViewTimer > 0f) {
			m_trackingViewTimer -= Time.unscaledDeltaTime;
			if(m_trackingViewTimer <= 0f) {
				// Notify tracking manager!
				NotifyViewTracking();
			}
		}
    }

	//------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialize the shop with the requested mode. Should be called before opening the popup.
    /// Notice that this method is not painting the shop. 
    /// </summary>
    /// <param name="_mode">Target mode.</param>
    /// <param name="_purchaseCompletedCallback">If provided, this action will be called each time an offer in
    /// this shop is successfully purchased</param>
    public void Init(Mode _mode, UnityAction<IShopPill> _purchaseCompletedCallback = null)
    {

        m_mode = _mode;
        switch (m_mode)
        {
            case Mode.PC_ONLY:
                m_categoryToShow = PC_CATEGORY_SKU;
                CenterItemsAndLockScroll();
                break;
            case Mode.SC_ONLY:
                m_categoryToShow = SC_CATEGORY_SKU;
                CenterItemsAndLockScroll();
                break;
        }

        // Clear everything to avoid the shop content being flashed before the transition clouds
        Clear();

        // In case we need to do something after the user purchases an offer
        m_purchaseCompletedCallback = _purchaseCompletedCallback;

    }


    /// <summary>
    /// Remove all the content of the shop
    /// </summary>
    public void Clear()
    {

        // Clean the containers
        m_categoriesContainer.transform.DestroyAllChildren(true);
        m_shortcutsContainer.transform.DestroyAllChildren(true);

		// Clean categories
		m_activeCategories.Clear();
        m_categoryContainers.Clear();

        // Clean pills cache
        m_pills.Clear();
		
        // Remove the shortcut references
        m_shortcuts.Clear();
        m_skuToShorcut.Clear();

        m_categoriesToInitialize.Clear();

        m_layoutGropusActive = false;
        m_disableLayoutsGroupsInNextFrame = false;
        m_hidePillsOutOfView = false;
		m_visiblePills.Clear();

    }



    /// <summary>
    /// Clear the shop and populate the shop with all the categories, shortcuts and offer pills
    /// </summary>
    public void Refresh ()
    {
        // Benchmarking
        timestamp = Environment.TickCount;

		// Reset tracking timer
		m_trackingViewTimer = TRACKING_VIEW_MIN_DURATION;

		// Clear everything
        Clear();

        // Turn off optimizations while refreshing
        SetOptimizationActive(false);
        ShopReady = false;

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
						// Store as active category
						m_activeCategories.Add(category);

						// Create shortcut if needed
                        CreateShortcut(category);

                        // Enqueue the categories so they are initialized one per frame
                        m_categoriesToInitialize.Enqueue(category);
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

        if (ShopReady)
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
        ShopReady = false;

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
            m_catBeingInitialized = container;

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
            ShopReady = true;

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

        // Make sure there are offers in this category
        if (_offers.Count == 0)
        {
            return null;
        }

        // Instantiate the shop category
        string containerPrefabPath = SHOP_CATEGORIES_CONTAINER_PREFABS_PATH + _cat.containerPrefab;
        CategoryController containerPrefab = Resources.Load<CategoryController>(containerPrefabPath);


        if (containerPrefab == null)
        {
            // The container prefab was not found
            Debug.LogError("The prefab " + containerPrefabPath + " was not found in the project");
            return null;
        }



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


    /// <summary>
    /// Scroll the viewport to the left border of the shop
    /// </summary>
    public void ScrollToStart()
    {
        if (m_pills!= null && m_pills.Count != 0)
        {
            ScrollToItem(m_pills[0].transform);
        }
    }

    /// <summary>
    /// In some cases we dont want the pills to stack in the left side, specially when we are showing only one category
    /// So with this method we disable scrolling and make sure the items are centered in the viewport. Scrolling is disabled 
    /// so you need to make sure that we dont have too many pills, or they will be out of the screen.
    /// </summary>
    private void CenterItemsAndLockScroll()
    {
        m_scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        m_scrollRect.horizontal = false;

        HorizontalLayoutGroup layout = m_scrollRect.GetComponent<HorizontalLayoutGroup>();
        ContentSizeFitter fitter = m_scrollRect.GetComponent<ContentSizeFitter>();
        if (layout != null && fitter != null )
        {
            layout.enabled = true;
            fitter.enabled = true;
        }

        layout = m_scrollRect.viewport.GetComponent<HorizontalLayoutGroup>();
        fitter = m_scrollRect.viewport.GetComponent<ContentSizeFitter>();
        if (layout != null && fitter != null)
        {
            layout.enabled = true;
            fitter.enabled = true;
        }

    }


    /// <summary>
    /// Find all the pills that are visible in the scrollable viewport and 
    /// make sure their item previews are loaded.
    /// </summary>
    private void ShowPreviewInActivePills()
    {
        // Define the width of the viewport. Out of this range, pills are disabled.
        m_normalizedViewportWidth = m_scrollRect.viewport.rect.width / m_scrollRect.content.rect.width;

        foreach (IShopPill pill in m_pills)
        {
            // Is this pill inside the visible limits of the scrollview (leave some margin)
            float pillPositionX = m_scrollRect.GetRelativePositionOfItem(pill.transform);
            float safetyMargin = .2f;
            bool visible = (pillPositionX < 1 + safetyMargin && pillPositionX > -safetyMargin );

            if (visible)
            {
                // The preview will be loaded in the next frame
                pill.loadPillPreview = true;
            }
        }
    }

    public void StopCameraTraveling()
    {
        m_cameraTravelingEnabled = false;

    }

    /// <summary>
    /// Keep a cache with all the offer pack pills already loaded.
    /// This way we dont make a Load call for every pill in the shop.
    /// Despite Unity uses its own Resources.Load cache, this additional cache
    /// layer seems to improve the performance slighty.
    /// </summary>
    /// <param name="_type"></param>
    public IShopPill GetPillPrefab(OfferPack.Type _type)
    {

        if (!m_pillPrefabs.ContainsKey(_type))
        {
            // The reqwuested pill type is not cached yet. So load it... 
            IShopPill newPill = ShopSettings.GetPillPrefab(_type);
            if ( newPill != null)
            {
                // ... and store it in the cache
                m_pillPrefabs.Add(_type, ShopSettings.GetPillPrefab(_type));
                return newPill;

            } else
            {
                Debug.LogError("Pill prefab not specified for offer type " + _type.ToString());
                return null;
            }
            
        }
        else
        {
            // We have it cached. So just fetch it
            return (m_pillPrefabs[_type]);
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
    /// Scroll the viewport to the position [0,1]
    /// </summary>
    /// <param name="anchor"></param>
    private void ScrollToPosition(float _pos)
    {
        m_scrolling = true;

        // Create a tweener to animate the scroll
        m_scrollRect.DOGoToNormalizedPosition(new Vector2 (_pos,0) ,.5f)
        .SetEase(Ease.OutBack)
        .OnComplete(delegate () { m_scrolling = false; });


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

        if (m_optimizationActive == _enable)
        {
            // Already active/inactive
            return;
        }

        m_optimizationActive = _enable;

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

        m_layoutGropusActive = enable;
    }


    /// <summary>
    /// Enable/disable pill that are inside/outside of the visible limits of the scrollview.
    /// </summary>
    /// <param name="_scrollPosition"></param>
    private void UpdatePillsVisibility(Vector2 _scrollPosition)
    {
        // Define the width of the viewport. Out of this range, pills are disabled.
        m_normalizedViewportWidth = m_scrollRect.viewport.rect.width / m_scrollRect.content.rect.width;

		m_visiblePills.Clear();
        foreach (IShopPill pill in m_pills)
        {
            // Is this pill inside the visible limits of the scrollview (leave some margin)
            float pillPositionX = m_scrollRect.GetRelativePositionOfItem(pill.transform);
            float safetyMargin = .2f;
            bool visible = (pillPositionX > -safetyMargin && pillPositionX < 1 + safetyMargin);

            // Enable/disable the pill
            pill.gameObject.SetActive(visible);

			// Cache visible pills
			if(visible) {
				m_visiblePills.Add(pill);
			}
        }

    }



	//------------------------------------------------------------------------//
	// TRACKING																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Tell the tracking manager the "view" events.
	/// </summary>
	private void NotifyViewTracking() {
		// Compute Viewport bounds
		Rect viewportRect = m_scrollRect.viewport.rect; // Viewport in local coords
		float viewportCenter = viewportRect.center.x;
		Log(Colors.yellow.Tag("Viewport Rect: " + DebugUtils.RectToString(viewportRect)));

		// Check pills to select those relevant for tracking params
		List<IShopPill> centerPills = new List<IShopPill>();
		List<IShopPill> fullyVisiblePills = new List<IShopPill>();
		List<IShopPill> closestPillsToCenter = new List<IShopPill>();   // Just in case there are no pills actually in the center
		float minDistToCenter = float.MaxValue;
		bool centerPillFound = false;
		IShopPill pill = null;
		Rect pillRect = new Rect();

		// We already have visible pills cached, so no need to go through all of them
		for(int i = 0; i < m_visiblePills.Count; ++i) {
			// Compute pill's rect in viewport coords
			pill = m_visiblePills[i];
			pillRect = (pill.transform as RectTransform).rect; // Pill in local coords
			pillRect = pill.transform.TransformRect(pillRect, m_scrollRect.viewport);   // Pill in viewport coords

			// Check pill boundaries
			if(viewportRect.xMin < pillRect.xMin && pillRect.xMax < viewportRect.xMax) {
				// Pill is fully in the viewport
				fullyVisiblePills.Add(pill);

				// Does it intersect the center?
				if(pillRect.xMin < viewportCenter && viewportCenter < pillRect.xMax) {
					// Yes!
					centerPills.Add(pill);
					centerPillFound = true;
				}
			}

			// While a pill in the center is not found, look for the closest ones to it 
			if(!centerPillFound) {
				// Is it the closest to the center?
				float distToCenter = Mathf.Abs(viewportCenter - pillRect.center.x);
				if(distToCenter < minDistToCenter - float.Epsilon) {
					// This pill is closest to the center than previous selected ones
					minDistToCenter = distToCenter;
					closestPillsToCenter.Clear();
					closestPillsToCenter.Add(pill);
				} else if(Mathf.Abs(minDistToCenter - distToCenter) < float.Epsilon) {
					// This pill is at the same distance from the center than previous selected ones
					closestPillsToCenter.Add(pill);
				}
			}

			// Debugging
#if LOG
			Color c = Colors.red;
			if(fullyVisiblePills.Contains(pill)) {
				c = Colors.lime;
			}
			Log(c.Tag(pill.name + ": " + DebugUtils.RectToString(pillRect)));
#endif
		}

		// Process categories
		// Find all categories that are fully visible. If none, pick the one that takes the most of the viewport.
		List<CategoryController> fullyVisibleCategories = new List<CategoryController>();
		List<float> categoriesViewportCoverage = new List<float>(m_categoryContainers.Count);
		CategoryController cat = null;
		Rect catRect = new Rect();
		for(int i = 0; i < m_categoryContainers.Count; ++i) {
			// Compute category's rect in viewport coords
			cat = m_categoryContainers[i];
			catRect = (cat.transform as RectTransform).rect; // Pill in local coords
			catRect = cat.transform.TransformRect(catRect, m_scrollRect.viewport);   // Pill in viewport coords

			// Is the category fully within the viewport?
			if(viewportRect.xMin < catRect.xMin && viewportRect.xMax > catRect.xMax) {
				fullyVisibleCategories.Add(cat);
			}

			// Compute percentage of the screen covered by this category
			// We only care about X axis, assume all categories have the same height
			float visibleWidth = Mathf.Min(catRect.xMax, viewportRect.xMax) - Mathf.Max(catRect.xMin, viewportRect.xMin);
			categoriesViewportCoverage.Add(visibleWidth / viewportRect.width);

			// Debugging
#if LOG
			Color c = Colors.red;
			if(fullyVisibleCategories.Contains(cat)) {
				c = Colors.lime;
			}
			Log(c.Tag(cat.name + ": " + DebugUtils.RectToString(catRect) + " | " + Mathf.RoundToInt(categoriesViewportCoverage.Last() * 100) + "%"));
#endif
		}

		// Debugging
#if LOG
		if(true) {	// To be able to fold it in the editor
			string strLog = Colors.yellow.Tag("FULLY VISIBLE PILLS: ");
			strLog += DebugUtils.ListToString(fullyVisiblePills, true, (IShopPill _pill) => { return _pill.def.sku; });
			Log(strLog);

			strLog = Colors.yellow.Tag("CENTER PILLS: ");
			strLog += DebugUtils.ListToString(centerPills, true, (IShopPill _pill) => { return _pill.def.sku; });
			Log(strLog);

			strLog = Colors.yellow.OpenTag() + "CLOSEST PILLS TO CENTER: ";
			if(centerPillFound) {
				strLog += "\n\tNOT NEEDED";
			} else {
				strLog += "\n\t";
				if(closestPillsToCenter.Count == 0) {
					strLog += Colors.red.Tag("NONE");
				} else {
					strLog += DebugUtils.ListToString(centerPills);
				}
			}
			strLog += Colors.yellow.CloseTag();
			Log(strLog);

			strLog = Colors.yellow.Tag("FULLY VISIBLE CATEGORIES: ");
			strLog += DebugUtils.ListToString(fullyVisibleCategories, true, (CategoryController _c) => { return _c.category.def.sku; });
			Log(strLog);

			strLog = Colors.yellow.Tag("CATEGORY VIEWPORT %: ");
			strLog += DebugUtils.ListToString(categoriesViewportCoverage, true, (float _value) => { return Mathf.RoundToInt(_value * 100f) + "%"; });
			Log(strLog);
		}
#endif

		// If pills exactly in the center were not found, use closest to center pills instead
		if(!centerPillFound) {
			centerPills = closestPillsToCenter;
		}

		// Find out relevant categories for tracking
		// Central category
		string centralCategory = "";
		if(centerPills.Count > 0) centralCategory = centerPills[0].def.GetAsString("shopCategory"); // [AOC] If there is more than one, they typically belong to the same category, so just use first one's category

		// All visible categories
		// [AOC] TODO!! Specs say to exclude sections that appear partially, but this might result in empty value if the current category is bigger than the viewport.
		//		 Send category covering most of the screen in such case
		List<string> visibleCategoriesParam = new List<string>();
		if(fullyVisibleCategories.Count > 0) {
			for(int i = 0; i < fullyVisibleCategories.Count; ++i) {
				visibleCategoriesParam.Add(fullyVisibleCategories[i].category.def.sku);
			}
		} else {
			// Find the category covering more screen %
			int bestCategoryIdx = -1;
			float maxPercentage = -1f;
			for(int i = 0; i < categoriesViewportCoverage.Count; ++i) {
				// Is it the best category?
				if(categoriesViewportCoverage[i] > maxPercentage) {
					maxPercentage = categoriesViewportCoverage[i];
					bestCategoryIdx = i;
				}
			}

			// If we found a valid category, push it to the tracking parameter values
			if(bestCategoryIdx >= 0) {
				visibleCategoriesParam.Add(m_categoryContainers[bestCategoryIdx].category.def.sku);
			}
		}

		// Format parameters in the way expected by the tracking manager
		List<string> centerItemsParam = new List<string>();
		for(int i = 0; i < centerPills.Count; ++i) {
			centerItemsParam.Add(centerPills[i].def.sku);
		}

		List<string> visibleItemsParam = new List<string>();
		for(int i = 0; i < fullyVisiblePills.Count; ++i) {
			visibleItemsParam.Add(fullyVisiblePills[i].def.sku);
		}

		// Finally, we have all the data to send the tracking event - do it!
		HDTrackingManager.Instance.Notify_StoreView(
			centralCategory,
			centerItemsParam.ToArray(),
			visibleCategoriesParam.ToArray(),
			visibleItemsParam.ToArray()
		);

#if LOG
		Log(Colors.magenta.Tag("STORE VIEW:"));
		Log(Colors.magenta.Tag("\tcentral_section: " + centralCategory));
		Log(Colors.magenta.Tag("\tcentral_itemID: " + DebugUtils.ListToString(centerItemsParam)));
		Log(Colors.magenta.Tag("\tall_section: " + DebugUtils.ListToString(visibleCategoriesParam)));
		Log(Colors.magenta.Tag("\tall_itemID: " + DebugUtils.ListToString(visibleItemsParam)));
#endif
	}

	/// <summary>
	/// Perform all required tracking upon entering the shop.
	/// </summary>
	private void NotifyShopEnterTracking() {
		HDTrackingManager.Instance.Notify_StoreVisited(m_trackingOrigin);
	}

	/// <summary>
	/// Tell the tracking manager about categories order.
	/// </summary>
	private void NotifyCategoryOrderTracking() {
        // We already have the categories sorted, so just send an event for each
        int i = 0;
        foreach (ShopCategory category in OffersManager.instance.activeCategories )
        {
            if (category.enabled)
            {
                HDTrackingManager.Instance.Notify_StoreCategoryOrder(category.sku, i);
                i++;
            }
        }
	}

	/// <summary>
	/// Tell the tracking manager when a shortcut is pressed.
	/// </summary>
	/// <param name="_sc">The pressed shortcut.</param>
	private void NotifyShortcutPressedTracking(ShopCategoryShortcut _sc) {
		// Propagate to tracking manager
		HDTrackingManager.Instance.Notify_StoreShortcutClick(_sc.id);
	}

    /// <summary>
    /// Fade out all the items in the shop, for a nicer trasition when leaving this screen
    /// </summary>
    private void FadeElementsOut ()
    {
        foreach (IShopPill pill in m_visiblePills)
        {
            pill.GetComponent<ShowHideAnimator>().Hide(true);
        }
        
    }

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// To be called whenever the shop is entered - either from the screen or the popup.
	/// </summary>
	/// <param name="_origin">Where do we come from? Tracking purposes.</param>
	public void OnShopEnter(string _origin) {
		// Tracking
		m_trackingOrigin = _origin;
		NotifyShopEnterTracking();
		NotifyCategoryOrderTracking();
	}

	/// <summary>
	/// A shortcut was pressed
	/// </summary>
	/// <param name="_sc">The category related to the shortcut</param>
	public void OnShortcutSelected(ShopCategoryShortcut _sc)
    {
		// Tracking! Even if already selected or not yet loaded
		NotifyShortcutPressedTracking(_sc);

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


        // Reset tracking timer
        m_trackingViewTimer = TRACKING_VIEW_MIN_DURATION;

        // create a paralax effect with the 3d bground (if camera traveling exists)
        if (m_cameraTraveling != null && m_cameraTravelingEnabled)
        {
            // Avoid camera traveling when the scroll container width is lower than the viewport
            if (m_scrollRect.content.sizeDelta.x > m_scrollRect.viewport.sizeDelta.x)
            {
                m_cameraTraveling.UpdateCameraPosition(m_scrollRect.horizontalNormalizedPosition);
            }
            else
            {
                m_cameraTraveling.UpdateCameraPosition(.5f);
            }
        }

        // Disable pills outside of the view limits
        if (m_hidePillsOutOfView)
        {
            UpdatePillsVisibility(_newPos);
        }

        // Scrolling animation still running
        if (m_scrolling || !ShopReady)
        {
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
        if (ShopReady)
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
        if (ShopReady)
        {

            // In order to fix https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-7731
            // We will just redraw the whole shop

            m_lastScrollPos = m_scrollRect.horizontalNormalizedPosition;
            Refresh();
            return;

            // TODO: refresh only the affected category (check before if it worth it)

        }
    }

    /// <summary>
    /// The current menu screen is changing (animation starts now).
    /// </summary>
    /// <param name="_from">Source screen.</param>
    /// <param name="_to">Target screen.</param>
    private void OnTransitionStart(MenuScreen _from, MenuScreen _to)
    {
        // We are leaving the shop
        if (_from == MenuScreen.SHOP)
        {
            // Start fading the pills out for a nicer transition
            FadeElementsOut();
        }
    }

    /// <summary>
    /// The current menu screen has changed (animation ends now).
    /// At this point we are just in the middle of the clouds transition
    /// </summary>
    /// <param name="_from">Source screen.</param>
    /// <param name="_to">Target screen.</param>
    private void OnTransitionEnd(MenuScreen _from, MenuScreen _to)
    {
        // If entering this screen
        if (_to == MenuScreen.SHOP)
        {
            // Start painting the shop while the clouds transition is happening so
            // we hide the FPS drop to the player
            Refresh();
        }
    }


    //------------------------------------------------------------------------//
    // DEBUG																  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Check the debug conditions and log given text if matching.
    /// </summary>
    /// <param name="_text">Text to be logged.</param>
#if LOG
	[Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
	public static void Log(string _text) {
		Debug.Log("[Shop] "+ _text);
	}
}
