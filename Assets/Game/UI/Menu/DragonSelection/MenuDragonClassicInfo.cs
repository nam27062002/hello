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

		// Aux vars
		bool dragonChanged = m_dragonData != _data;

		// Things to update only when target dragon has changed
		if (dragonChanged || _force)
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
				bool show = !_data.isOwned;
				if(show) {
					m_dragonDescText.Localize(_data.def.GetAsString("tidDesc"));
					if(dragonChanged) {
						m_dragonDescText.GetComponent<ShowHideAnimator>().RestartShow();
					} else {
						m_dragonDescText.GetComponent<ShowHideAnimator>().ForceShow();
					}
				} else {
					m_dragonDescText.GetComponent<ShowHideAnimator>().ForceHide();
				}
            }

            // Owned group. This items will be shown when the player owns the dragon
            {
                // XPBar
                if (m_xpBar != null)
                {
                    if (_data.isOwned)
                    {
						// Show it only in owned dragons
                        m_xpBar.Refresh(_data as DragonDataClassic);
						if(dragonChanged) {
							m_xpBar.GetComponent<ShowHideAnimator>().RestartShow();
						} else {
							m_xpBar.GetComponent<ShowHideAnimator>().ForceShow();
						}
                    }
                    else
                    {
                        m_xpBar.GetComponent<ShowHideAnimator>().Hide();
                    }

                }
            }

            // Not owned group 
            {
                // Unlock buttons and message
                if (m_dragonUnlock != null)
                {
                    if (!_data.isOwned)
                    {
                        m_dragonUnlock.Refresh(_data, true);
                    }
                    else
                    {
                        m_dragonUnlock.Refresh(_data, false);
                    }
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
		PopupDragonInfo.OpenPopupForDragon(m_dragonData, "info_button");
    }
}
