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
/// This class listens to any dragon selection change and enables the proper object depending on the dragon type
/// </summary>
public class DragonTypeSelector : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	
    [Tooltip("Enable this group is a classic dragon is selected")]
    [SerializeField] private ShowHideAnimator m_classicDragonGroup;

    [Tooltip("Enable this group is a special dragon is selected")]
    [SerializeField] private ShowHideAnimator m_specialDragonGroup;

    [SerializeField] private bool m_animate;


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

        if (m_classicDragonGroup != null)
        {
            if (!special)
            {
                m_classicDragonGroup.Show(m_animate);
            }
            else{
                m_classicDragonGroup.Hide(m_animate);
            }

        }

        if (m_specialDragonGroup != null)
        {
            if (special)
            {
                m_specialDragonGroup.Show(m_animate);
            }
            else
            {
                m_specialDragonGroup.Hide(m_animate);
            }
        }
        
    }
}