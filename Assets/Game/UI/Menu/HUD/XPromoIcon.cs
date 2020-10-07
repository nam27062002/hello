// XPromoIcon.cs
// Hungry Dragon
// 
// Created by Jose M. Olea on 03/09/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//

using System;
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

    [SerializeField] protected ShowHideAnimator m_animationRoot;

    [SerializeField] protected UINotification m_newNotification;

    // Internal
    private float m_timer;
    private bool m_active; // Use this flag to detect a change in the xpromo state∫

    
    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    
    public void Start()
    {
        Refresh();

        // Start with the icon hidden
        m_active = false;
        if (m_animationRoot != null)
            m_animationRoot.ForceHide(false);
        
        m_newNotification.Show(true);
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
    /// Update the UI. Show/hide the xpromo icon.
    /// </summary>
    private void Refresh()
    {
        
        m_active = XPromoManager.instance.xPromoCycle.IsActive();
        if (m_active)
        {
            // Display the xpromo icon
            m_animationRoot.Show(true);
        }
        else
        {
            // Hide the xPromo icon
            m_animationRoot.Hide(true);
            
            // No xpromo, no cookie. Nothing else to do here.
            return;
        }
        
        // If there is a reward ready to collect, show the NEW notification bubble
        bool rewardReadyToCollext = XPromoManager.instance.xPromoCycle.CanCollectNextReward();
        if (rewardReadyToCollext)
        {
            m_newNotification.Show(true);
        }
        else
        {
            m_newNotification.Hide(true);
        }
        
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