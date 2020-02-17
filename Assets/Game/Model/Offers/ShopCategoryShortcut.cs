// ShopCategoryShortcut.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 21/01/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class ShopCategoryShortcut : MonoBehaviour {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    [SerializeField] private Localizer m_text;

    // Internal
    private ShopController m_shopController; // Keep a reference to the parent shop

    private CategoryController m_categoryController;
    public CategoryController categoryController
    { get { return m_categoryController; } }




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


    public void OnEnable()
    {
        ShowHideAnimator sh =GetComponent<ShowHideAnimator>();
        if (sh != null)
        {
            // Fade in smoothly
            sh.ForceShow(true);
        }
    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Initializes the shortcut element
    /// </summary>
    /// <param name="_categoryController">Category controller related to this shortcut</param>
    /// <param name="_shop">Parent shop</param>
    public void Initialize (CategoryController _categoryController, ShopController _shop)
    {
        if (_categoryController == null || _shop == null)
        {
            return;
        }

        m_categoryController = _categoryController;
        m_text.Localize(_categoryController.category.tidShortcut);
        
        m_shopController = _shop;
    }

    /// <summary>
    /// Select/deselect the current shortcut button and show the proper visual state
    /// </summary>
    /// <param name="_value"></param>
    public void Select(bool _value)
    {
        GetComponent<SelectableButton>().SetSelected(_value);
    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// When clicking in the anchor, scroll to the related category
    /// </summary>
    public void OnClick ()
    {
        m_shopController.OnShortcutSelected(this);
    }
}