// ShopController.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 21/01/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using DG.Tweening;
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


    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    // Offer categories
    [SerializeField] private Transform m_categoriesContainer;

    // Horizontal Scroller
    [SerializeField] private ScrollRect m_scrollRect;

    // Shortcuts
    [SerializeField] private Transform m_shortcutsContainer;
    [SerializeField] private List<ShopCategoryShortcut> m_shortcuts;
    [SerializeField] private ShopCategoryShortcut m_shortcutPrefab;


    //Internal
    private float m_timer = 0; // Refresh timer
    private bool m_refreshed = false; // Did we perform the initial refresh?
    private Dictionary<string, Transform> m_anchors; // Reference to the UI elements in the shop. Used to scroll the view to the desired category.



    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {
        m_shortcuts = new List<ShopCategoryShortcut>();
        m_anchors = new Dictionary<string, Transform>();
    }

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
        // Refresh offers periodically for better performance
        if (m_timer <= 0)
        {
            m_timer = 1f; // Refresh every second
            //Refresh();
        }
        m_timer -= Time.deltaTime;

        if (!m_refreshed)
        {
            // Force unity to refresh the layout the next frame after start
            // I know, it looks awful but it makes the work
            /*m_categoriesContainer.gameObject.SetActive(false);
            m_categoriesContainer.gameObject.SetActive(true);*/
            m_refreshed = true;
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
    /// Remove all the content of the shop
    /// </summary>
    public void Clear()
    {

        // Clean the containers
        m_categoriesContainer.transform.DestroyAllChildren(true);
        m_shortcutsContainer.transform.DestroyAllChildren(true);

        // Remove the shortcut references
        m_shortcuts.Clear();

        m_refreshed = false;
    }

    /// <summary>
    /// Initialize the shop with the requested mode. Should be called before opening the popup.
    /// </summary>
    /// <param name="_mode">Target mode.</param>
    public void Init(PopupShop.Mode _mode, string _origin)
    {
        Refresh();
    }

    /// <summary>
    /// Populate the shop with all the categories, shortcuts and offer pills
    /// </summary>
    public void Refresh ()
    {
        Clear();

        string lastShortcut = null;

        // Iterate all the active categories
        foreach (ShopCategory category in OffersManager.instance.activeCategories)
        {
            // If this cat is active 
            if (category.enabled)
            {
                // Instantiate the shop category
                string containerPrefabPath = SHOP_CATEGORIES_CONTAINER_PREFABS_PATH + category.containerPrefab;
                CategoryController containerPrefab = Resources.Load<CategoryController>(containerPrefabPath);

                if (containerPrefab == null)
                {
                    // The container prefab was not found
                    Debug.LogError("The prefab " + containerPrefabPath + " was not found in the project");
                    continue;
                }

                // TODO: Optimize this: dont call twice to this method! (here and inside the category initialization)
                List<OfferPack> offers = OffersManager.GetOfferPacksByCategory(category);

                // Make sure there are offers in this category
                if (offers.Count > 0)
                {

                    CategoryController categoryContainer = Instantiate<CategoryController>(containerPrefab);

                    // Add the category container to the hierarchy
                    categoryContainer.transform.SetParent(m_categoriesContainer, false);

                    categoryContainer.Initialize(category);


                    // Has a shortcut in the bottom menu?
                    if (!string.IsNullOrEmpty(category.tidShortcut))
                    {
                        // If two categories share a shortcut, dont create it twice
                        if (lastShortcut != category.tidShortcut)
                        {

                            // Create a new shortcut :D

                            // Instantiate a shortcut and add it to the bottom bar
                            ShopCategoryShortcut newShortcut = Instantiate<ShopCategoryShortcut>(m_shortcutPrefab, m_shortcutsContainer, false);
                            newShortcut.Initialize(category, this);
                            m_shortcuts.Add(newShortcut);

                            // Store the category container position to scroll in the future
                            m_anchors.Add(category.sku, categoryContainer.transform);

                            // Keep a record of the last shortcut created
                            lastShortcut = category.tidShortcut;
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// A shortcut was pressed
    /// </summary>
    /// <param name="_category">The category related to the shortcut</param>
    public void OnShortcutSelected( ShopCategory _category)
    {
        // Deselect all shortcuts except the chosen one
        foreach (ShopCategoryShortcut shortcut in m_shortcuts)
        {
            bool selectedCat = ( shortcut.category.sku == _category.sku );
            shortcut.Select(selectedCat);
        }

        Transform categoryAnchor = m_anchors[_category.sku];
        ScrollToItem(categoryAnchor);

    }


    /// <summary>
    /// Scroll the viewport to the selected category
    /// </summary>
    /// <param name="anchor"></param>
    private void ScrollToItem (Transform anchor)
    {

        if (anchor != null)
        {

            // Create a tweener to animate the scroll
            m_scrollRect.DOGoToItem(anchor, .5f)
            .SetEase(Ease.OutQuad);
            
        }
    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}