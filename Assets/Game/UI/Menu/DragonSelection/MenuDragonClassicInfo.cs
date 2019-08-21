// MenuDragonLevelBar.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for a dragon level bar in the menu.
/// </summary>
public class MenuDragonClassicInfo : MenuDragonInfo {
    //------------------------------------------------------------------------//
    // PROPERTIES															  //
    //------------------------------------------------------------------------//


    [SerializeField] private DragonXPBar m_xpBar;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    // Implemented in parent class


    //------------------------------------------------------------------------//
    // INTERNAL METHODS														  //
    //------------------------------------------------------------------------//




    /// <summary>
    /// Update all fields with given dragon data
    /// </summary>
    /// <param name="_data">Dragon data.</param>
    /// <param name="_force">If true forces the refresh, even if the dragon has not changed since the las refresh</param>
    protected override void Refresh(IDragonData _data, bool _force = false)
    {

        // Check params
        if (_data == null) return;

        // Only show classic dragons bar
        if (!(_data is DragonDataClassic)) return;


        // Things to update only when target dragon has changed
        if (m_dragonData != _data || _force)
        {

            // Dragon Name
            if (m_dragonNameText != null)
            {
                switch (_data.GetLockState())
                {
                    case DragonDataClassic.LockState.SHADOW:
                    case DragonDataClassic.LockState.REVEAL:
                        m_dragonNameText.Localize("TID_SELECT_DRAGON_UNKNOWN_NAME");
                        break;
                    default:
                        m_dragonNameText.Localize(_data.def.GetAsString("tidName"));
                        break;
                }
            }


            // Description
            if (m_dragonDescText != null)
            {
                // Remove it when the player owns the dragon.
                m_dragonDescText.gameObject.SetActive(!_data.isOwned);

                m_dragonDescText.Localize(_data.def.GetAsString("tidDesc"));
            }


            // XPBar
            if (m_xpBar != null)
            {
                if (_data.isOwned)
                {
                    // Show it only in owned dragons
                    m_xpBar.GetComponent<ShowHideAnimator>().ForceShow();
                    m_xpBar.Refresh(_data as DragonDataClassic);
                }
                else
                {
                    m_xpBar.GetComponent<ShowHideAnimator>().Hide();
                }
                
            }

            // Store new dragon data
            m_dragonData = _data;
        }
    }


    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    // Others implemented in parent class

    /// <summary>
    /// Info button has been pressed.
    /// </summary>
    public override void OnInfoButton()
    {
        // Skip if dragon data is not valid
        if (m_dragonData == null) return;

        // Tracking
        string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupDragonInfo.PATH);
        HDTrackingManager.Instance.Notify_InfoPopup(popupName, "info_button");

        // Open the dragon info popup and initialize it with the current dragon's data
        PopupDragonInfo popup = PopupManager.OpenPopupInstant(PopupDragonInfo.PATH).GetComponent<PopupDragonInfo>();
        popup.Init(m_dragonData);
    }
}
