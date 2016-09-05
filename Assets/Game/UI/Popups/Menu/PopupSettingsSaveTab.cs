// PopupSettingsSaveTab.cs
// Hungry Dragon
// 
// Created by David Germade on 30th August 2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for handling the save tab in the settings popup. This tab is used for three things:
///   -Social: Log in/Log out to social network
///   -Cloud: Enable/Disable the cloud save
///   -Resync: Force a sync with the cloud if it's enabled
/// </summary>
public class PopupSettingsSaveTab : MonoBehaviour
{
    void Awake()
    {
        Cloud_Init();
    }
    #region social
    // This region is responsible for handling social stuff

    [SerializeField]
    private GameObject m_socialEnableBtn;

    [SerializeField]
    private GameObject m_socialLogoutBtn;
    
    /// <summary>
    /// Label below the social button describing the current connection
    /// </summary>
    [SerializeField]    
    private Localizer m_socialMessageText;

    /// <summary>
    /// Callback called by the player when the user clicks on enable a social network
    /// </summary>
    public void Social_OnSelectNetwork()
    {

    }

    /// <summary>
    /// Callback called by the player when the user clicks on log out from the current social network
    /// </summary>
    public void Social_OnLogoutNetwork()
    {

    }
    #endregion

    #region cloud
    [SerializeField]
    private GameObject m_cloudEnabledButton;

    [SerializeField]
    private GameObject m_cloudDisabledButton;

    private bool m_cloudIsEnabled;
    private bool Cloud_IsEnabled
    {
        get
        {
            return m_cloudIsEnabled;
        }

        set
        {
            m_cloudIsEnabled = value;

            m_cloudEnabledButton.SetActive(!m_cloudIsEnabled);
            m_cloudDisabledButton.SetActive(m_cloudIsEnabled);            
        }
    }

    private void Cloud_Init()
    {
        Cloud_IsEnabled = false;
    }

    /// <summary>
    /// Callback called by the player when the user clicks on enable/disable the cloud save
    /// </summary>
    public void Cloud_OnChangeSaveEnable()
    {
        Cloud_IsEnabled = !Cloud_IsEnabled;
    }
    #endregion

    #region resync
    [SerializeField]
    private Button m_resyncButton;
    #endregion

    #region user_profile

    /// <summary>
    /// User profile GameObject to use when the user is logged in. It shows user's profile information
    /// </summary>
    [SerializeField]
    private GameObject m_userProfileRoot;

    [SerializeField]
    private GameObject m_userProfileAvatarImage;

    [SerializeField]
    private GameObject m_userProfileNameText;

    /// <summary>
    /// User profile GameObject to use when the user hasn't logged in. It encourages the user to log in
    /// </summary>
    [SerializeField]
    private GameObject m_userProfileLogRoot;

    [SerializeField]
    private Localizer m_userProfileLogMessage;
    #endregion
}