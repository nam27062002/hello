// MenuDragonInfoController.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 23/07/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// This class listens to any dragon selection change and enables the proper info box depending on the dragon type
/// </summary>
public class MenuDragonInfoController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	
    [SerializeField] private GameObject m_classicDragonInfo;
    [SerializeField] private GameObject m_specialDragonInfo;
        

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

        Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);

    }


    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// A new dragon has been selected.
    /// </summary>
    /// <param name="_sku">The sku of the selected dragon.</param>
    private void OnDragonSelected(string _sku)
    {
        // Show the proper info box depending on the selected type of dragon
        bool special = DragonManager.GetDragonData(_sku).type == IDragonData.Type.SPECIAL;

        if (m_classicDragonInfo != null)
        {
            m_specialDragonInfo.SetActive(special);
        }

        if (m_specialDragonInfo != null)
        {
            m_classicDragonInfo.SetActive(!special);
        }
        
    }
}