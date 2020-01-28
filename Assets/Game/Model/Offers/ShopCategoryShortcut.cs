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
    private ShopCategory m_category;
    public ShopCategory category
    { get { return m_category; } }

    private ShopController shopController; // Keep a reference to the parent shop




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



	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

        /// <summary>
        /// Initializes the shortcut element
        /// </summary>
        /// <param name="_category">Shop category related to this shortcut</param>
        /// <param name="_shop">Parent shop</param>
    public void Initialize (ShopCategory _category, ShopController _shop)
    {
        if (_category == null || _shop == null)
        {
            return;
        }

        m_category = _category;
        m_text.Localize(category.tidShortcut);
        
        shopController = _shop;
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
        shopController.OnShortcutSelected(category);
    }
}