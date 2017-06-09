// PopupSettings.cs
// 
// Created by Alger Ortín Castellví on 11/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Pause popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupSettings : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Menu/PF_PopupSettings";

    public const string KEY_SETTINGS_LANGUAGE = "SETTINGS_LANGUAGE";

    [SerializeField]
    private GameObject m_saveTab;

    void Awake()
    {
        if (m_saveTab != null)
        {
#if CLOUD_SAVE && (WEIBO || FACEBOOK)
            m_saveTab.SetActive(true);
#else
            m_saveTab.SetActive(false);
#endif
        }
    }
}
