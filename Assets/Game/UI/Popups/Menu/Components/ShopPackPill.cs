// ShopPackPill.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 05/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System;
using System.Collections.Generic;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class ShopPackPill:PopupShopOffersPill {
    //------------------------------------------------------------------------//
    // AUX CLASSES & STRUCTS												  //
    //------------------------------------------------------------------------//


    /// <summary>
    /// This class represents each layout for different amount of items
    /// (i.e. the 3 items layout)    
    /// </summary>
    [System.Serializable]
    public class OffersLayout
    {
        public Transform m_container;
        public List<OfferItemSlot> m_offerItems;
    }

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    [Separator("Shop Pack Specifics")]

    // The list of 3 different layouts created. Has to be filled in the inspector.
    [SerializeField] private List<OffersLayout> m_layouts;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    public ShopPackPill() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~ShopPackPill() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}