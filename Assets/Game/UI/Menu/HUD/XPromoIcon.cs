// XPromoIcon.cs
// Hungry Dragon
// 
// Created by Jose M. Olea on 03/09/2020.
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
public class XPromoIcon : MonoBehaviour {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    // Internal
    private float m_timer;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    public void Start()
    {
        Refresh();
    }

    public void Update()
    {
        // Refresh periodically for better performance
        if (m_timer <= 0)
        {
            m_timer = 1f; // Refresh every second
            Refresh();
        }
        m_timer -= Time.deltaTime;
    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Update the UI
    /// </summary>
    public void Refresh()
    {
        //Once this element is disabled it wont be enabled again, as Update() wont be executed.
        gameObject.SetActive(XPromoManager.instance.xPromoCycle.IsActive());
    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// The xPromo icon was taped
    /// </summary>
    public void OnTapIcon ()
    {
        // Open the xpromo rewards popup
		PopupManager.OpenPopupInstant(PopupXPromo.PATH);
	}
}