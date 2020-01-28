// ShopCategory.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 21/01/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class ShopCategory {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    private DefinitionNode m_def;
    public DefinitionNode def
    {
        get
        {
            return m_def;
        }
    }

    private string m_sku;
    public string sku
    {
        get
        {
            return m_sku;
        }
    }

    private string m_tidShortcut;
    public string tidShortcut
    {
        get
        {
            return m_tidShortcut;
        }

    }

    private string m_containerPrefab;
    public string containerPrefab
    {
        get
        {
            return m_containerPrefab;
        }
    }

    private int m_order;
    public int order
    {
        get
        {
            return m_order;
        }
    }

    private bool m_enabled;
    public bool enabled
    {
        get
        {
            return m_enabled;
        }
    }


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    public ShopCategory() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~ShopCategory() {

	}

    /// <summary>
    /// Creates and initializes a category object from a definition node
    /// </summary>
    /// <param name="_def"></param>
    /// <returns></returns>
    public static ShopCategory CreateFromDefinition (DefinitionNode _def)
    {
        ShopCategory newCat = new ShopCategory();

        newCat.InitFromDefinition(_def);

        return newCat;

    }


    //------------------------------------------------------------------------//
    // INITIALIZATIONS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Initialize this category from the content definition
    /// </summary>
    /// <param name="_def"></param>
    public void InitFromDefinition (DefinitionNode _def)
    {
        // Safety check
        if (_def == null)
        {
            return;
        }

        // Store definition node
        m_def = _def;

        // Read all the columns
        m_sku = _def.GetAsString("sku");
        m_tidShortcut = _def.GetAsString("tidName");
        m_containerPrefab = _def.GetAsString("containerPrefab");
        m_order = _def.GetAsInt("order");
        m_enabled = _def.GetAsBool("enabled");

    }


}