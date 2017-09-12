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
        Messenger.AddListener<bool>(GameEvents.SOCIAL_LOGGED, OnSocialLogged);
        Messenger.AddListener(GameEvents.PERSISTENCE_SYNC_DONE, OnPersistenceSyncDone);

        Model_Init();
        Social_Init();
        Resync_Init();
        User_Init();
        IsShown = false;
    }

    void OnEnable()
    {
        IsShown = true;
        RefreshView();
    }    

    void OnDestroy()
    {
        IsShown = false;
        Messenger.RemoveListener<bool>(GameEvents.SOCIAL_LOGGED, OnSocialLogged);
        Messenger.RemoveListener(GameEvents.PERSISTENCE_SYNC_DONE, OnPersistenceSyncDone);        
    }    

    /// <summary>
    /// Returns whether or not this tab is being shown
    /// </summary>
    private bool IsShown { get; set; }
    
    private void RefreshView()
    {
        Model_Refresh();
        User_Refresh();
        Social_Refresh();
        Resync_Refresh();
        Cloud_Refresh();
    }

    private bool IsLoadingPopupOpen { get; set; }

    private void OpenLoadingPopup()
    {
        IsLoadingPopupOpen = true;
        PersistenceFacade.Popups_OpenLoadingPopup();
    }

    private void CloseLoadingPopup()
    {
        IsLoadingPopupOpen = false;
        PersistenceFacade.Popups_CloseLoadingPopup();
    }

    private void OnSocialLogged(bool logged)
    {
        RefreshView();

        if (IsLoadingPopupOpen)
        {
            CloseLoadingPopup();
        }
    }

    private void OnPersistenceSyncDone()
    {
        RefreshView();
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

    private void Social_Refresh()
    {        
        bool isLoggedIn = Model_SocialIsLoggedIn();
        bool isCloudSaveEnabled = Model_SaveIsCloudSaveEnabled();        
        if (isLoggedIn)
        {            
            m_socialEnableBtn.gameObject.SetActive(false);         
            m_socialEnableBtn.interactable = isCloudSaveEnabled;                        
            m_socialLogoutBtn.SetActive(true);
        }
        else
        {
            // not logged in to any network            
            m_socialEnableBtn.gameObject.SetActive(true);
            m_socialEnableBtn.interactable = true;
            m_socialLogoutBtn.SetActive(false);
        }

        string localizedName = SocialPlatformManager.SharedInstance.GetPlatformName();
        if (localizedName != null)
        {
            m_userNotLoggedInMessageText.Localize(TID_OPTIONS_USERPROFILE_LOG_RECEIVE, localizedName);
            m_userPreviouslyLoggedInMessageText.Localize(TID_OPTIONS_USERPROFILE_LOG_NETWORK, localizedName);

            m_socialMessageText.gameObject.SetActive(true);

            // if logged in and the cloud save is enabled...
            if (Model_SocialIsLoggedIn() && Model_SaveIsCloudSaveEnabled())
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

    /// <summary>
    /// Callback called by the player when the user clicks on log in the social network
    /// </summary>
    public void Social_Login()
    {
        // [DGR] ANALYTICS: Not supported yet
        // HSXAnalyticsManager.Instance.loginContext = "OptionsLogin";

        OpenLoadingPopup();
        Action onDone = delegate()
        {
            CloseLoadingPopup();         
            if (SocialPlatformManager.SharedInstance.IsLoggedIn())
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                    Log("LOGIN SUCCESSFUL");

                User_LoadProfileInfo();                
            }
            else
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                    Log("LOGIN FAILED");                
            }

            RefreshView();
        };

        PersistenceFacade.instance.Sync_FromSettings(onDone);        
    }
   

    /// <summary>
    /// Callback called by the player when the user clicks on log out from the social network
    /// </summary>
    public void Social_Logout()
    {        
        if (Model_SocialIsLoggedIn())
        {
            PersistenceFacade.Popups_OpenLogoutWarning(Model_SaveIsCloudSaveEnabled(),
                delegate ()
                {
                    if (FeatureSettingsManager.IsDebugEnabled)
                        Log("LOGGING OUT... ");

                    OpenLoadingPopup();
                    SocialPlatformManager.SharedInstance.Logout();

                        /*
                        delegate ()
                        {
                            if (FeatureSettingsManager.IsDebugEnabled)
                                Debug.Log("LOGGING OUT COMPLETED");

                            // on logout
                            PersistenceManager.Popups_CloseLoadingPopup();
                            User_LoadProfileInfo();
                            Cloud_DisableCloudSave();                            

                            //  Reenable log in button
                            m_socialLogoutBtn.SetActive(false);
                            m_socialEnableBtn.gameObject.SetActive(true);                            
                        }
                        );*/
                },
                null
            );                                
        }
        else if (FeatureSettingsManager.IsDebugEnabled)
        {
            LogError("LOGIN: Not logged in to " ); 
        }        
    }
    #endregion

    #region cloud
    [SerializeField]
    private Slider m_cloudEnableSlider;
    /*private GameObject m_cloudEnabledButton;

    [SerializeField]
    private GameObject m_cloudDisabledButton;        
    */

    /// <summary>
    /// Callback called by the player when the user clicks on enable/disable the cloud save
    /// </summary>
    public void Cloud_OnChangeSaveEnable()
    {        
        bool isSaveEnabled = Model_SaveIsCloudSaveEnabled();
        PersistenceFacade.instance.CloudDriver.Upload_IsEnabled = !isSaveEnabled;
        Cloud_Refresh();
        Resync_Refresh();

        /*
        if (PersistenceFacade.instance.CloudDriver.Upload_IsEnabled)
        {            
            Cloud_Refresh();
            Resync_Refresh();            
        }
        else
        {            
            Resync_OnCloudSaveSync();
        }
        */
        
        /*
        if (!Cloud_IsResyncing)
        {
            Cloud_IsResyncing = true;
            Cloud_Resync();
        }*/
        
        /*if (!Cloud_IsStateChanging)
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
        }  */             
    }   
    
    private void Cloud_Refresh()
    {
        bool isSaveEnabled = Model_SaveIsCloudSaveEnabled();
        m_cloudEnableSlider.value = (isSaveEnabled) ? 1 : 0;
        m_cloudEnableSlider.interactable = Model_SocialIsLoggedIn();                                    
    }    
    #endregion

    #region resync
    [SerializeField]
    private Button m_resyncButton;

    private bool Resync_IsRunning { get; set; }

    private void Resync_Init()
    {
        Resync_IsRunning = false;
    }

    private void Cloud_Reset()
    {
        Resync_IsRunning = false;
    }

    private void Resync_Refresh()
    {
        m_resyncButton.interactable = Model_SaveIsCloudSaveEnabled() && Model_SocialIsLoggedIn();        
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
        if (!Resync_IsRunning)
        {
            Action onDone = delegate ()
            {
                Resync_IsRunning = false;
                Cloud_Refresh();
                RefreshView();                
            };

            Resync_IsRunning = true;
            PersistenceFacade.instance.Sync_FromSettings(onDone);
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

    private bool User_IsAvatarLoaded { get; set; }

    private EState User_LastState { get; set; }

    private void User_Init()
    {        
        User_IsAvatarLoaded = false;

        // Nothing is shown
        m_userNotLoggedInRoot.SetActive(false);
        m_userLoggedInRoot.SetActive(false);
        m_userPreviouslyLoggedInRoot.SetActive(false);
        User_LastState = EState.None;
    }

    private void User_Reset()
    {
        User_Init();                
    }

    /*private void User_LoadProfileInfo()
    {
        m_userLoggedInRoot.SetActive(false);
        m_userPreviouslyLoggedInRoot.SetActive(false);

        User_IsAvatarLoaded = false;

        if (Model_SocialIsLoggedIn())
        {
            User_IsLoggedIn = true;

            m_userNotLoggedInRoot.SetActive(false);
            m_userLoggedInRoot.SetActive(true);
            m_userAvatarImage.gameObject.SetActive(false);
            //m_profileSpinner.SetActive(true);

            m_userNameText.text = LocalizationManager.SharedInstance.Get(TID_LOADING);

            SocialPlatformManager.SharedInstance.GetProfileInfo(
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

            m_userPreviouslyLoggedInRoot.SetActive(false);
            m_userNotLoggedInRewardText.gameObject.SetActive(false);
            m_userNotLoggedInMessageText.gameObject.SetActive(false);            
            
            switch (Model_State)
            {
                //case EState.LoggedIn:
                   // m_userNotLoggedInRewardText.gameObject.SetActive(true);

                    //if (!Model_SocialWasLoginIncentivised(SocialFacade.Network.Default))
                    //{
                       // PersistenceFacade.Texts_LocalizeIncentivizedSocial(m_userNotLoggedInRewardText);
                    //}
                    //else
                    //{
                    //    m_userNotLoggedInRewardText.Localize(TID_SOCIAL_PERM_MAINMENU);
                    //}

                    //break;

                case EState.NeverLoggedIn:
                    m_userNotLoggedInRewardText.gameObject.SetActive(true);
                    PersistenceFacade.Texts_LocalizeIncentivizedSocial(m_userNotLoggedInRewardText);                    
                    m_userNotLoggedInMessageText.gameObject.SetActive(true);                    
                    m_userNotLoggedInMessageText.Localize(TID_OPTIONS_USERPROFILE_LOG_RECEIVE, SocialPlatformManager.SharedInstance.GetPlatformName());                                        
                    //m_spriteBGReceive.SetActive(true);
                    break;
                
                case EState.PreviouslyLoggedIn:
                    //Activate & Deactivate correspondant widgets for this state
                    m_userNotLoggedInRewardText.gameObject.SetActive(false);
                    m_userNotLoggedInMessageText.gameObject.SetActive(false);
                    //m_spriteBGReceive.SetActive(false);
                    m_userPreviouslyLoggedInRoot.SetActive(true);

                    string platformName = SocialPlatformManager.SharedInstance.GetPlatformName();
                    m_userPreviouslyLoggedInMessageText.Localize(TID_OPTIONS_USERPROFILE_LOG_NETWORK, platformName);
                    m_socialMessageText.Localize(TID_CLOUD_DESC, platformName);                    

                    break;
            }

            m_userNotLoggedInRoot.SetActive(true);
        }
    }    
    */
    
    private void User_Refresh()
    {
        if (User_LastState != Model_State || true)
        {
            bool needsToLoadProfile = Model_HasBeenLoaded(Model_State);// && !Model_HasBeenLoaded(User_LastState);            

            User_LastState = Model_State;

            m_userLoggedInRoot.SetActive(false);
            m_userNotLoggedInRoot.SetActive(false);
            m_userPreviouslyLoggedInRoot.SetActive(false);            

            if (needsToLoadProfile)
            {
                User_LoadProfileInfo();
            }

            switch (Model_State)
            {
                case EState.LoggedIn:
                case EState.LoggedInAndIncentivised:                                    
                    m_userLoggedInRoot.SetActive(true);
                    m_userAvatarImage.gameObject.SetActive(false);
                    //m_profileSpinner.SetActive(true);                    
                    break;
                
                case EState.NeverLoggedIn:
                    m_userNotLoggedInRoot.SetActive(true);                    
                    m_userNotLoggedInRewardText.gameObject.SetActive(true);
                    PersistenceFacade.Texts_LocalizeIncentivizedSocial(m_userNotLoggedInRewardText);
                    m_userNotLoggedInMessageText.gameObject.SetActive(true);
                    m_userNotLoggedInMessageText.Localize(TID_OPTIONS_USERPROFILE_LOG_RECEIVE, SocialPlatformManager.SharedInstance.GetPlatformName());
                    break;

                case EState.PreviouslyLoggedIn:
                    m_userPreviouslyLoggedInRoot.SetActive(true);                                        
                    string platformName = SocialPlatformManager.SharedInstance.GetPlatformName();
                    m_userPreviouslyLoggedInMessageText.Localize(TID_OPTIONS_USERPROFILE_LOG_NETWORK, platformName);
                    m_socialMessageText.Localize(TID_CLOUD_DESC, platformName);
                    break;                
            }
        }
    }    

    private void User_LoadProfileInfo()
    {
        m_userNameText.text = LocalizationManager.SharedInstance.Get(TID_LOADING);
        
        /*
        SocialPlatformManager.SharedInstance.GetProfileInfo(
            delegate (string userName)
            {
                //if (!string.IsNullOrEmpty(userName) && User_IsLoggedIn && m_userNameText != null)
                if (!string.IsNullOrEmpty(userName) && m_userNameText != null)
                {
                    m_userNameText.text = userName;
                }
            },
            delegate (Texture2D profileImage)
            {
                if (IsShown)
                {
                    //if (User_IsLoggedIn)
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
            */              
    }
    #endregion

    #region model
    private enum EState
    {        
        None,
        NeverLoggedIn,
        PreviouslyLoggedIn,
        LoggedIn,
        LoggedInAndIncentivised
    }

    private EState Model_State { get; set; }

    private bool Model_HasBeenLoaded(EState state)
    {
        return state == EState.PreviouslyLoggedIn || state == EState.LoggedIn || state == EState.LoggedInAndIncentivised;
    }

    private void Model_Init()
    {
        Model_Refresh();
    }                

    private void Model_Refresh()
    {
        EState state = EState.NeverLoggedIn;
        bool isLoggedIn = SocialPlatformManager.SharedInstance.IsLoggedIn();
        UserProfile userProfile = UsersManager.currentUser;
        if (userProfile != null)
        {
            switch (userProfile.SocialState)
            {
                case UserProfile.ESocialState.NeverLoggedIn:
                    state = EState.NeverLoggedIn;
                    break;

                case UserProfile.ESocialState.LoggedIn:
                    state = (isLoggedIn) ? EState.LoggedIn : EState.PreviouslyLoggedIn;
                    break;

                case UserProfile.ESocialState.LoggedInAndInventivised:
                    state = (isLoggedIn) ? EState.LoggedInAndIncentivised : EState.PreviouslyLoggedIn;
                    break;                
            }
        }

        Model_State = state;
    }

    private bool Model_SocialIsLoggedIn()
    {
        bool returnValue = false;
        switch (Model_State)
        {            
            case EState.LoggedIn:
            case EState.LoggedInAndIncentivised:
            {
                returnValue = SocialPlatformManager.SharedInstance.IsLoggedIn();
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

    private bool Model_SaveIsCloudSaveEnabled()
    {
        return PersistenceFacade.instance.CloudDriver.Upload_IsEnabled;
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


    #region customersupport
    // This region is responsible for handling customer support stuff

   

   

    public void OpenCustomerSupport()
    {
        //CSTSManager.SharedInstance.OpenView(TranslationsManager.Instance.ISO.ToString(), PersistenceManager.Instance.IsPayer);
		CSTSManager.SharedInstance.OpenView(LocalizationManager.SharedInstance.Culture.Name, false);	// Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.;
        HDTrackingManager.Instance.Notify_CustomerSupportRequested();
    }

    #endregion    

	#region notifications_settings
	[SerializeField]
    private Slider m_notificationsSlider;

    public void Notifications_Init(){
		m_notificationsSlider.normalizedValue = PlayerPrefs.GetInt( PopupSettings.KEY_SETTINGS_NOTIFICATIONS, 1);
    }

    public void OnNotificationsSettingChanged(){
		int v = Mathf.RoundToInt( m_notificationsSlider.normalizedValue);
		PlayerPrefs.SetInt( PopupSettings.KEY_SETTINGS_NOTIFICATIONS, v );
    }
    #endregion
}