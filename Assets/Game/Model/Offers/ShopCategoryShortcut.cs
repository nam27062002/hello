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
    private Transform m_anchor; // Reference to the category element in the scroll list
    private ShopController shopController;


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
        shopController = GetComponentInParent<ShopController>();
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
        /// Initializes the shortcut element
        /// </summary>
        /// <param name="_category">Shop category related to this shortcut</param>
        /// <param name="_anchor">UI element in the scroll list related to this shortcut</param>
    public void Initialize (ShopCategory _category, Transform _anchor = null)
    {
        if (_category == null)
        {
            return;
        }

        m_category = _category;
        m_text.Localize(m_category.tidShortcut);
        m_anchor = _anchor;
    }

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

    /// <summary>
    /// When clicking in the anchor, scroll to the related category
    /// </summary>
    public void OnClick ()
    {
        shopController.ScrollToItem(m_anchor);
    }
}