// MenuDragonSpecialLevelBar.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 22/07/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class MenuDragonSpecialInfo : MenuDragonInfo {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//


    [SerializeField] private SpecialDragonBar m_specialDragonLevelBar;

    [SerializeField] private SpecialStatUpgrader[] m_stats = new SpecialStatUpgrader[0];
    [SerializeField] private DragonPowerUpgrader m_powerUpgrade;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    // Implemented in parent class

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//


    protected override void Refresh(IDragonData _data, bool _force = false)
    {

        // Check params
        if (_data == null) return;

        // Only show special dragons
        // Only show classic dragons bar
       if ( !(_data is DragonDataSpecial) ) return;

        DragonDataSpecial specialData = _data as DragonDataSpecial;



        // Things to update only when target dragon has changed
        if (m_dragonData != specialData || _force)
        {

            // Dragon Name
            if (m_dragonNameText != null)
            {
                switch (_data.GetLockState())
                {
                    case DragonDataSpecial.LockState.SHADOW:
                    case DragonDataSpecial.LockState.REVEAL:
                        m_dragonNameText.Localize("TID_SELECT_DRAGON_UNKNOWN_NAME");
                        break;
                    default:
                        m_dragonNameText.Localize(_data.def.GetAsString("tidName"));
                        break;
                }
            }


            // Dragon Description
            if (m_dragonDescText != null)
            {
                // Description. Remove it when the player owns the dragon.
                m_dragonDescText.gameObject.SetActive(!specialData.isOwned);

                m_dragonDescText.Localize(_data.def.GetAsString("tidDesc"));
            }

            // Owned group. This items will be shown when the player owns the dragon
            {
                // XPBar
                if (m_specialDragonLevelBar != null)
                    if (specialData.isOwned)
                    {
                        m_specialDragonLevelBar.GetComponent<ShowHideAnimator>().RestartShow();
                        m_specialDragonLevelBar.BuildFromDragonData(specialData);

                    }
                    else
                    {
                        m_specialDragonLevelBar.GetComponent<ShowHideAnimator>().Hide();
                    }

                // Upgrade buttons
                for (int i = 0; i < m_stats.Length; ++i)
                {
                    if (specialData.isOwned)
                    {
                        m_stats[i].InitFromData(specialData);
                        m_stats[i].Refresh(true);
                    }
                    else
                    {
                        m_stats[i].GetComponent<ShowHideAnimator>().Hide(false);
                    }

                }


                // Upgrade powerup button
                if (specialData.isOwned)
                {
                    m_powerUpgrade.InitFromData(specialData);
                    m_powerUpgrade.Refresh(true);
                }
                else
                {
                    m_powerUpgrade.ForceGetComponent<ShowHideAnimator>().Hide(false);
                }
            }

            // Not owned group 
            {

                // Unlock buttons and message
                if (m_dragonUnlock != null)
                {
                    if (!specialData.isOwned)
                    {
                        m_dragonUnlock.Refresh(_data, true);
                    }
                    else
                    {
                        m_dragonUnlock.Refresh(_data, false);
                    }
                }
            }
        }

        // Store new dragon data
        m_dragonData = specialData;
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
        string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupSpecialDragonInfo.PATH);
        HDTrackingManager.Instance.Notify_InfoPopup(popupName, "info_button");

        // Open the dragon info popup and initialize it with the current dragon's data
        PopupSpecialDragonInfo popup = PopupManager.OpenPopupInstant(PopupSpecialDragonInfo.PATH).GetComponent<PopupSpecialDragonInfo>();
        popup.Init(m_dragonData);
    }


}