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


    [SerializeField] private LabDragonBar m_specialDragonLevelBar;

    [SerializeField] private LabStatUpgrader[] m_stats = new LabStatUpgrader[0];
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

            // XPBar
            if (m_specialDragonLevelBar != null)
            {
                if (specialData.isOwned)
                {
                    // Show it only in owned dragons
                    m_specialDragonLevelBar.gameObject.SetActive(true);

                    // Wait 1 frame for the Awake method to finish
                    UbiBCN.CoroutineManager.DelayedCallByFrames(
                        () =>
                        {
                            m_specialDragonLevelBar.BuildFromDragonData(specialData);
                        }, 1);

                }
                else
                {
                    m_specialDragonLevelBar.gameObject.SetActive(false);
                }

            }


            // Upgrade buttons
            for (int i = 0; i < m_stats.Length; ++i)
            {
                m_stats[i].InitFromData(specialData);
            }

            // Upgrade powerup button
            m_powerUpgrade.InitFromData(specialData);

            // Store new dragon data
            m_dragonData = specialData;

        }
    }


    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//

    // Implemented in parent class

}