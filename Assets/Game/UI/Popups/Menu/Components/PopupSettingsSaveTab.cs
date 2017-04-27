// PopupSettingsSaveTab.cs
// Hungry Dragon
// 
// Created by David Germade on 30th August 2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

using FGOL.Authentication;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// This class is responsible for handling the save tab in the settings popup. This tab is used for three things:
///   -Social: Log in/Log out to social network
///   -Cloud: Enable/Disable the cloud save
///   -Resync: Force a sync with the cloud if it's enabled
/// </summary>
public class PopupSettingsSaveTab : MonoBehaviour
{
    private const string TID_LOADING = "TID_GEN_LOADING";
    private const string TID_CLOUD_DESC = "TID_SAVE_CLOUD_DESC";
    private const string TID_CLOUD_DESC_LOGGED = "TID_SAVE_CLOUD_LOGGED_DESC";
    private const string TID_SOCIAL_LOGIN_MAINMENU_INCENTIVIZED = "TID_SOCIAL_LOGIN_MAINMENU_INCENTIVIZED";
    private const string TID_SOCIAL_PERM_MAINMENU_INCENTIVIZED = "TID_SOCIAL_PERM_MAINMENU_INCENTIVIZED";
    private const string TID_SOCIAL_PERM_MAINMENU = "TID_SOCIAL_PERM_MAINMENU";
    private const string TID_OPTIONS_USERPROFILE_LOG_RECEIVE = "TID_SOCIAL_USERPROFILE_LOG_RECEIVE";
    private const string TID_OPTIONS_USERPROFILE_LOG_NETWORK = "TID_SOCIAL_USERPROFILE_LOG_NETWORK";

    void Awake()
    {            
        Init();        
    }

    private void Init()
    {
        Model_Init();
        Social_Init();
        Cloud_Init();
        User_Init();
        IsShown = false;
    }

    void OnEnable()
    {
        IsShown = true;
        Reset();
    }

    void OnDestroy()
    {
        IsShown = false;
    }

    private void Reset()
    {
        Cloud_Reset();
        Social_Reset();
        User_Reset();
        Resync_Refresh();        
    }

    /// <summary>
    /// Returns whether or not this tab is being shown
    /// </summary>
    private bool IsShown { get; set; }

    public string GetCenteredNetworkLocalized()
    {
        SocialFacade.Network network = Model_SocialGetCenteredNetwork();
        return SocialFacade.GetLocalizedNetworkName(network);        
    }

    private void OnNetworkSelectCenter(SocialFacade.Network network)
    {
        string localizedName = SocialFacade.GetLocalizedNetworkName(network);
        if (localizedName != null)
        {
            User_OnNetworkSelectCenter(network, localizedName);
            Social_OnNetworkSelectCenter(network, localizedName);            
        }
    }

    private void RefreshView()
    {
        Social_Refresh();
        Resync_Refresh();
    }

    #region social
    // This region is responsible for handling social stuff

    [SerializeField]
    private Button m_socialEnableBtn;

    [SerializeField]
    private GameObject m_socialLogoutBtn;
    
    /// <summary>
    /// Label below the social button describing the current connection
    /// </summary>
    [SerializeField]    
    private Localizer m_socialMessageText;

    private void Social_Init()
    {        
        m_socialMessageText.gameObject.SetActive(false);
        m_socialEnableBtn.gameObject.SetActive(true);
        m_socialEnableBtn.interactable = false;
        m_socialLogoutBtn.SetActive(false);
    }

    private void Social_Reset()
    {
        Social_Init();

        // It simulates the automatic scroll of the default social network
        StartCoroutine(DelayedCall(0.1f, Social_Refresh));
    }

    private void Social_Refresh()
    {
        SocialFacade.Network centeredNetwork = Model_SocialGetCenteredNetwork();

        bool isLoggedInToCenteredNetwork = Model_SocialIsLoggedIn(centeredNetwork);
        bool isCloudSaveEnabled = Model_SaveIsCloudSaveEnabled();

        User.LoginType[] authenticatedNetworks = Model_AuthGetAuthenticatedNetworks();
        bool isLoggedInToAnyNetwork = authenticatedNetworks != null && authenticatedNetworks.Length > 0;
        if (isLoggedInToAnyNetwork)
        {
            // logged in to one of the networks... button states depend on what the centered network state is like            
            m_socialEnableBtn.gameObject.SetActive(!isLoggedInToCenteredNetwork);
            // note: Can't switch networks if cloud save is disabled
            m_socialEnableBtn.interactable = isCloudSaveEnabled;                        
            m_socialLogoutBtn.SetActive(isLoggedInToCenteredNetwork);
        }
        else
        {
            // not logged in to any network            
            m_socialEnableBtn.gameObject.SetActive(true);
            m_socialEnableBtn.interactable = true;
            m_socialLogoutBtn.SetActive(false);
        }

        OnNetworkSelectCenter(centeredNetwork);
    }

    private void Social_OnNetworkSelectCenter(SocialFacade.Network network, string localizedName)
    {
        if (localizedName != null)
        {
            m_socialMessageText.gameObject.SetActive(true);

            // if logged in and the cloud save is enabled...
            if (Model_SocialIsLoggedIn(network) && Model_SaveIsCloudSaveEnabled())
            {
                // ... show an advice about cloud save.
                m_socialMessageText.Localize(TID_CLOUD_DESC_LOGGED, localizedName);
            }
            else
            {
                // else use a generic string.
                m_socialMessageText.Localize(TID_CLOUD_DESC, localizedName);
            }
        }
    }

    private void Social_LoginToCenteredNetwork()
    {
        // log in to selected network        
        SocialFacade.Network network = Model_SocialGetCenteredNetwork();

        // [DGR] ANALYTICS: Not supported yet
        // HSXAnalyticsManager.Instance.loginContext = "OptionsLogin";
                
        SocialManager.Instance.Login(network, (success) =>
        {         
            PersistenceManager.Popups_CloseLoadingPopup();

            if (success)
            {
                Log("LOGIN to " + network.ToString() + " SUCCESSFUL");                
                User_LoadProfileInfo();
                Social_Refresh();
            }
            else
            {
                Log("LOGIN to " + network.ToString() + " FAILED");                
                Social_Refresh();

                //	Claim cloud save disabled
                Cloud_DisableCloudSave();
            }
        });
    }

    /// <summary>
    /// Callback called by the player when the user clicks on enable a social network
    /// </summary>
    public void Social_OnSelectNetwork()
    {
        SocialFacade.Network currentNetwork = Model_SocialGetSelectedSocialNetwork();
        SocialFacade.Network centeredNetwork = Model_SocialGetCenteredNetwork();

        bool sameNetwork = currentNetwork == centeredNetwork;
        bool loggedInToCurrentNetwork = SocialManager.Instance.IsLoggedIn(currentNetwork);
        if (sameNetwork && loggedInToCurrentNetwork)
        {
            Log("Shouldn't be able to click Select when it's already the current network!");
            return;
        }

        if (currentNetwork != SocialFacade.Network.Default)
        {
            // you've got a preferred network, see if logged in or not
            if (Model_SocialIsLoggedIn(currentNetwork))
            {                
                // they're logged in to a network already...ask them if they want to log out of the current network
                PersistenceManager.Popups_OpenLoginWhenAlreadyLoggedIn(currentNetwork, centeredNetwork,
                    delegate ()
                    {
                        //Debug.Log("WEIBOWEIBO: LOGGING OUT of " + currentNetwork.ToString());
                        PersistenceManager.Popups_OpenLoadingPopup();                        
                        SocialManager.Instance.Logout(currentNetwork, delegate ()
                        {
                            //Debug.Log("WEIBOWEIBO: LOG OUT from " + currentNetwork.ToString() + " COMPLETE!");
                            // on logout
                            // [DGR] ANALYTICS: Not supported yet
                            // HSXAnalyticsManager.Instance.SocialLogout(currentNetwork.ToString());

                            // Log in to new network
                            Social_LoginToCenteredNetwork();
                        });
                    },
                    null
                );                                
            }
            else
            {             
                // Not logged in to current network
                Social_LoginToCenteredNetwork();
            }
        }
    }

    /// <summary>
    /// Callback called by the player when the user clicks on log out from the current social network
    /// </summary>
    public void Social_OnLogoutNetwork()
    {
        SocialFacade.Network currentNetwork = Model_SocialGetSelectedSocialNetwork();
        if (currentNetwork != SocialFacade.Network.Default)
        {
            if (Model_SocialIsLoggedIn(currentNetwork))
            {
                PersistenceManager.Popups_OpenLogoutWarning(currentNetwork, Model_SaveIsCloudSaveEnabled(),
                    delegate ()
                    {
                        Log("LOGGING OUT of " + currentNetwork.ToString());
                        PersistenceManager.Popups_OpenLoadingPopup();
                        SocialManager.Instance.Logout(currentNetwork, 
                            delegate ()
                            {
                                Debug.Log("LOGGING OUT of " + currentNetwork.ToString() + " COMPLETED");
                                // on logout
                                PersistenceManager.Popups_CloseLoadingPopup();
                                User_LoadProfileInfo();
                                Cloud_DisableCloudSave();

                                // [DGR] ANALYTICS Not supported yet
                                //HSXAnalyticsManager.Instance.SocialLogout(currentNetwork.ToString());

                                //  Reenable log in button
                                m_socialLogoutBtn.SetActive(false);
                                m_socialEnableBtn.gameObject.SetActive(true);                            
                            }
                         );
                    },
                    null
                );                                
            }
            else            
            {
                LogError("LOGIN: Not logged in to " + currentNetwork); 
            }
        }
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

            m_cloudEnabledButton.SetActive(m_cloudIsEnabled);
            m_cloudDisabledButton.SetActive(!m_cloudIsEnabled);            
        }
    }

    private bool Cloud_IsStateChanging { get; set; }        

    private void Cloud_Init()
    {
        Cloud_IsEnabled = false;
        Cloud_IsStateChanging = false;
    }

    private void Cloud_Reset()
    {
        Cloud_IsEnabled = SaveFacade.Instance.cloudSaveEnabled;
        Cloud_IsStateChanging = false;
    }

    private void Cloud_DisableCloudSave()
    {
        SaveFacade.Instance.ClearError();

        // Report cloud save disabled to analytics only once! The actual disable logic may be called multiple times
        if (SaveFacade.Instance.cloudSaveEnabled)
        {
            // [DGR] ANALYTICS Not supported yet
            //HSXAnalyticsManager.Instance.CloudSaveDisabledResult("Disabled", SystemInfo.deviceModel);
        }

        SaveFacade.Instance.cloudSaveEnabled = false;
        Cloud_IsEnabled = false;
        
        RefreshView();
    }

    /// <summary>
    /// Callback called by the player when the user clicks on enable/disable the cloud save
    /// </summary>
    public void Cloud_OnChangeSaveEnable()
    {                     
        if (!Cloud_IsStateChanging)
        {
#if CLOUD_SAVE && (FACEBOOK || WEIBO)
            bool newvalue = !Cloud_IsEnabled;
            if (newvalue != SaveFacade.Instance.cloudSaveEnabled)
            {
                Cloud_IsStateChanging = true;                
                
                if (!newvalue)
                {
                    PersistenceManager.Popups_OpenCloudDisable(
                        delegate
                        {
                            Cloud_IsEnabled = false;                            
                            Cloud_DisableCloudSave();
                            Cloud_IsStateChanging = false;
                        },                        
                        delegate ()
                        {
                            Cloud_IsEnabled = true;
                            Resync_IsEnabled = true;
                            Cloud_IsStateChanging = false;
                        }
                    );                    
                }
                else
                {
                    //TODO show confirmation popup if facebook not logged in?                    
                    // [DGR] ANALYTICS Not supported yet
                    //HSXAnalyticsManager.Instance.loginContext = "OptionsCloudSave";

                    SaveFacade.Instance.Enable(User.LoginType.Default, 
                        delegate ()
                        {
                            Cloud_IsEnabled = true;
                            Resync_IsEnabled = true;
                            Cloud_IsStateChanging = false;
                        },
                        delegate ()
                        {
                            Cloud_IsEnabled = false;
                            Resync_IsEnabled = false;                        
                            Cloud_IsStateChanging = false;
                        }
                    );
                }
            }
#endif
        }               
    }    
    #endregion

    #region resync
    [SerializeField]
    private Button m_resyncButton;

    private void Resync_Refresh()
    {
        m_resyncButton.interactable = Model_SaveIsCloudSaveEnabled();        
    }

    private bool Resync_IsEnabled
    {
        get
        {
            return m_resyncButton.interactable;
        }

        set
        {
            m_resyncButton.interactable = value;
        }
    }

    /// <summary>
    /// Callback called by the player when the user clicks on the Sync save data button
    /// </summary>
    public void Resync_OnCloudSaveSync()
    {
        if (SaveFacade.Instance.cloudSaveEnabled)
        {
            SaveFacade.Instance.GoToSaveLoaderState();
        }
    }
    #endregion

    #region user

    /// <summary>
    /// User profile GameObject to use when the user has never logged in. It encourages the user to log in
    /// </summary>
    [SerializeField]
    private GameObject m_userNotLoggedInRoot;

    /// <summary>
    /// User profile GameObject to use when the user is logged in. It shows user's profile information
    /// </summary>
    [SerializeField]
    private GameObject m_userLoggedInRoot;

    /// <summary>
    /// User profile GameObject to use when the user is not logged in but she logged in previously
    /// </summary>
    [SerializeField]
    private GameObject m_userPreviouslyLoggedInRoot;

    [SerializeField]
    private Image m_userAvatarImage;

    [SerializeField]
    private TextMeshProUGUI m_userNameText;
    
    [SerializeField]
    private Localizer m_userNotLoggedInMessageText;

    [SerializeField]
    private Localizer m_userNotLoggedInRewardText;

    [SerializeField]
    private Localizer m_userPreviouslyLoggedInMessageText;

    private bool User_IsLoggedIn { get; set; }

    private bool User_IsAvatarLoaded { get; set; }

    private void User_Init()
    {
        User_IsLoggedIn = false;
        User_IsAvatarLoaded = false;

        // Nothing is shown
        m_userNotLoggedInRoot.SetActive(false);
        m_userLoggedInRoot.SetActive(false);
        m_userPreviouslyLoggedInRoot.SetActive(false);
    }

    private void User_Reset()
    {
        User_Init();
        User_LoadProfileInfo();
    }

    private void User_LoadProfileInfo()
    {
        m_userLoggedInRoot.SetActive(false);
        m_userPreviouslyLoggedInRoot.SetActive(false);

        User_IsAvatarLoaded = false;

        if (Model_SocialIsLoggedIn(SocialFacade.Network.Default))
        {
            User_IsLoggedIn = true;

            m_userNotLoggedInRoot.SetActive(false);
            m_userLoggedInRoot.SetActive(true);
            m_userAvatarImage.gameObject.SetActive(false);
            //m_profileSpinner.SetActive(true);

            m_userNameText.text = LocalizationManager.SharedInstance.Get(TID_LOADING);

            Model_SocialGetProfileInfo(SocialFacade.Network.Default, 
            delegate (string userName) 
            {
                if (!string.IsNullOrEmpty(userName) && User_IsLoggedIn && m_userNameText != null)
                {
                    m_userNameText.text = userName;
                }
            }, 
            delegate (Texture2D profileImage)
            {
                if (IsShown)
                {
                    if (User_IsLoggedIn)
                    {
                        if (profileImage != null)
                        {
                            User_IsAvatarLoaded = true;

                            Sprite sprite = Sprite.Create(profileImage, new Rect(0, 0, profileImage.width, profileImage.height), new Vector2(0.5f, 0.0f), 1.0f);                            
                            m_userAvatarImage.sprite = sprite;                            
                            m_userAvatarImage.gameObject.SetActive(true);
                            // m_profileSpinner.SetActive(false);
                        }
                        else if (!User_IsAvatarLoaded)
                        {
                            //m_profileSpinner.SetActive(false);
                            m_userAvatarImage.gameObject.SetActive(true);
                        }
                    }
                }
            });
        }
        else
        {
            User_IsLoggedIn = false;
            
            AuthManager.LoginState loginState = Model_AuthGetLoginState(SocialFacade.Network.Default);            

            m_userPreviouslyLoggedInRoot.SetActive(false);
            m_userNotLoggedInRewardText.gameObject.SetActive(false);
            m_userNotLoggedInMessageText.gameObject.SetActive(false);

            string centeredNetwork = GetCenteredNetworkLocalized();
            
            switch (loginState)
            {
                case AuthManager.LoginState.LoggedInFriendsPermissionNeeded:
                    m_userNotLoggedInRewardText.gameObject.SetActive(true);

                    if (!Model_SocialWasLoginIncentivised(SocialFacade.Network.Default))
                    {
                        PersistenceManager.Texts_LocalizeIncentivizedSocial(m_userNotLoggedInRewardText);
                    }
                    else
                    {
                        m_userNotLoggedInRewardText.Localize(TID_SOCIAL_PERM_MAINMENU);
                    }

                    break;
                case AuthManager.LoginState.NeverLoggedIn:
                    m_userNotLoggedInRewardText.gameObject.SetActive(true);
                    PersistenceManager.Texts_LocalizeIncentivizedSocial(m_userNotLoggedInRewardText);                    
                    m_userNotLoggedInMessageText.gameObject.SetActive(true);

                    if (!string.IsNullOrEmpty(centeredNetwork))
                    {
                        m_userNotLoggedInMessageText.Localize(TID_OPTIONS_USERPROFILE_LOG_RECEIVE, centeredNetwork);
                    }
                    //m_spriteBGReceive.SetActive(true);

                    break;
                case AuthManager.LoginState.PreviouslyLoggedIn:
                    //Activate & Deactivate correspondant widgets for this state
                    m_userNotLoggedInRewardText.gameObject.SetActive(false);
                    m_userNotLoggedInMessageText.gameObject.SetActive(false);
                    //m_spriteBGReceive.SetActive(false);
                    m_userPreviouslyLoggedInRoot.SetActive(true);                    

                    if (!string.IsNullOrEmpty(centeredNetwork))
                    {
                        m_userPreviouslyLoggedInMessageText.Localize(TID_OPTIONS_USERPROFILE_LOG_NETWORK, centeredNetwork);
                        m_socialMessageText.Localize(TID_CLOUD_DESC, centeredNetwork);
                    }

                    break;
            }

            m_userNotLoggedInRoot.SetActive(true);
        }
    }    

    private void User_OnNetworkSelectCenter(SocialFacade.Network network, string localizedName)
    {
        if (localizedName != null)
        {
            m_userNotLoggedInMessageText.Localize(TID_OPTIONS_USERPROFILE_LOG_RECEIVE, localizedName);
            m_userPreviouslyLoggedInMessageText.Localize(TID_OPTIONS_USERPROFILE_LOG_NETWORK, localizedName);            
        }
    }
    #endregion

    #region model
    private enum EState
    {
        Normal, // Delegates to the model
        NeverLoggedIn,
        PreviouslyLoggedIn,
        LoggedInFriendsPermissionNeeded
    }

    private EState state = EState.Normal;

    private void Model_Init()
    {
#if UNITY_EDITOR && false
        SocialFacade.Instance.Init();
        GameServicesFacade.Instance.Init();
        SocialManager.Instance.Init();

        state = EState.Normal;
#endif
    }    

    private AuthManager.LoginState Model_AuthGetLoginState(SocialFacade.Network network)
    {
        AuthManager.LoginState returnValue = AuthManager.LoginState.NeverLoggedIn;       
        switch (state)
        {
            case EState.Normal:
            {
                returnValue = AuthManager.Instance.GetNetworkLoginState(SocialManagerUtilities.GetLoginTypeFromSocialNetwork(network));
            }
            break;

            case EState.NeverLoggedIn:
            {
                returnValue = AuthManager.LoginState.NeverLoggedIn;
            }
            break;

            case EState.PreviouslyLoggedIn:
            {
                returnValue = AuthManager.LoginState.PreviouslyLoggedIn;
            }
            break;

            case EState.LoggedInFriendsPermissionNeeded:
            {
                returnValue = AuthManager.LoginState.LoggedInFriendsPermissionNeeded;
            }
            break;
        }

        return returnValue;
    }

    private User.LoginType[] Model_AuthGetAuthenticatedNetworks()
    {
        User.LoginType[] returnValue = null;
        switch (state)
        {
            case EState.Normal:
            {
                returnValue = AuthManager.Instance.GetAuthenticatedNetworks();
            }
            break;

            case EState.NeverLoggedIn:            
            {
                returnValue = null;
            }
            break;
        }

        return returnValue;
    }

    private SocialFacade.Network Model_SocialGetSelectedSocialNetwork()
    {        
        return SocialManager.GetSelectedSocialNetwork();
    }

    private SocialFacade.Network Model_SocialGetCenteredNetwork()
    {
        // So far we assume that the nework is Facebook
        return SocialFacade.Network.Facebook;
    }

    private bool Model_SocialIsLoggedIn(SocialFacade.Network network)
    {
        bool returnValue = false;
        switch (state)
        {
            case EState.Normal:
                {
                    returnValue = SocialManager.Instance.IsLoggedIn(network);
                }
                break;

            case EState.NeverLoggedIn:
            case EState.PreviouslyLoggedIn:
            {
                returnValue = false;
            }
            break;
        }

        return returnValue;
    }

    private void Model_SocialGetProfileInfo(SocialFacade.Network network, Action<string> onGetName, Action<Texture2D> onGetImage)
    {
        switch (state)
        {
            case EState.Normal:
            {
                SocialManager.Instance.GetProfileInfo(network, onGetName, onGetImage);
            }
            break;            
        }
    }

    private bool Model_SocialWasLoginIncentivised(SocialFacade.Network network)
    {
        bool returnValue = false;
        switch (state)
        {
            case EState.Normal:
            {
                returnValue = SocialManager.Instance.WasLoginIncentivised(network);
            }
            break;

            case EState.NeverLoggedIn:
            {
                returnValue = false;
            }
            break;            
        }

        return returnValue;
    }    

    private bool Model_SaveIsCloudSaveEnabled()
    {
        bool returnValue = false;
        switch (state)
        {
            case EState.Normal:
                {
                    returnValue = SaveFacade.Instance.cloudSaveEnabled;
                }
                break;

            case EState.NeverLoggedIn:
                {
                    returnValue = false;
                }
                break;
        }

        return returnValue;
    }
    #endregion

    #region utils
    System.Collections.IEnumerator DelayedCall(float waitTime, Action callback)
    {
        yield return new WaitForSecondsRealtime(waitTime);
        callback();
    }
    #endregion

    #region log
    private static string LOG_CHANNEL = "[SAVE_TAB]";
    private void Log(string message)
    {
        Debug.Log(LOG_CHANNEL + message);        
    }

    private void LogError(string message)
    {
        Debug.LogError(LOG_CHANNEL + message);
    }
    #endregion
}