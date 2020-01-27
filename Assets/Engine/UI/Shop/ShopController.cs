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
    [SerializeField] private ShopCategoryShortcut m_shortcutPrefab;

    //Internal
    private float m_timer = 0; // Refresh timer
    private bool m_refreshed = false; // Did we perform the initial refresh?

    

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {

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

                    // Initialize the category
                    categoryContainer.Initialize(category);


                    // Has a shortcut in the bottom menu?
                    if (!string.IsNullOrEmpty(category.tidShortcut))
                    {
                        // If two categories share a shortcut, dont create another one
                        if (lastShortcut != category.tidShortcut)
                        {
                            // Instantiate a shortcut and add it to the bottom bar
                            ShopCategoryShortcut newShortcut = Instantiate<ShopCategoryShortcut>(m_shortcutPrefab, m_shortcutsContainer);
                            newShortcut.Initialize(category, categoryContainer.transform);
                            
                            // Keep an eye in the last shortcut created
                            lastShortcut = category.tidShortcut;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Scroll the viewport to the selected category
    /// </summary>
    /// <param name="anchor"></param>
    public void ScrollToItem (Transform anchor)
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