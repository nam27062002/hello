// PopupSettingsSaveTab.cs
// Hungry Dragon
// 
// Created by David Germade on 30th August 2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

using System;
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
        Resync_Init();
        User_Init();        
    }

    void OnEnable()
    {        
        RefreshView();
    }            
    
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
            if (Model_SocialIsLoggedIn())
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                    Log("LOGIN SUCCESSFUL");

                // Invalidades User_LastState in order to make sure user's information will be recalculated
                User_LastState = EState.None;                                
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

                    //OpenLoadingPopup();                    
                    PersistenceFacade.instance.CloudDriver.Logout();
                    RefreshView();                        
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

    [SerializeField]
    private Button m_cloudEnableButton;

    /// <summary>
    /// Callback called by the player when the user clicks on enable/disable the cloud save
    /// </summary>
    public void Cloud_OnChangeSaveEnable()
    {        
        PersistenceFacade.instance.IsCloudSaveEnabled = m_cloudEnableSlider.value == 1;
        Resync_Refresh();    
    }   
    
    private void Cloud_Refresh()
    {        
        bool isSaveEnabled = Model_SaveIsCloudSaveEnabled();
        m_cloudEnableSlider.value = (isSaveEnabled) ? 1 : 0;

        bool isOn = Model_SocialIsLoggedIn();
        m_cloudEnableSlider.interactable = isOn;
        m_cloudEnableButton.interactable = isOn;
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
            OpenLoadingPopup();
            Action onDone = delegate()
            {
                CloseLoadingPopup();
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
    
    private void User_Refresh()
    {
        if (User_LastState != Model_State)
        {
            bool needsToLoadProfile = (Model_HasBeenLoaded(Model_State) && !Model_HasBeenLoaded(User_LastState)) ||
                                       SocialPlatformManager.SharedInstance.NeedsProfileInfoToBeUpdated();

            bool needsSocialIdToBeUpdated = SocialPlatformManager.SharedInstance.NeedsSocialIdToBeUpdated();

            User_LastState = Model_State;

            m_userLoggedInRoot.SetActive(false);
            m_userNotLoggedInRoot.SetActive(false);
            m_userPreviouslyLoggedInRoot.SetActive(false);            

            if (needsToLoadProfile)
            {
                User_LoadProfileInfo(needsSocialIdToBeUpdated);
            }

            switch (Model_State)
            {
                case EState.LoggedIn:
                case EState.LoggedInAndIncentivised:
                case EState.PreviouslyLoggedIn:
                    m_userLoggedInRoot.SetActive(true);

                    if (needsSocialIdToBeUpdated)
                    {
                        m_userAvatarImage.gameObject.SetActive(false);
                    }
                    //m_profileSpinner.SetActive(true);                    
                    break;
                
                case EState.NeverLoggedIn:
                    m_userNotLoggedInRoot.SetActive(true);                    
                    m_userNotLoggedInRewardText.gameObject.SetActive(true);
                    PersistenceFacade.Texts_LocalizeIncentivizedSocial(m_userNotLoggedInRewardText);
                    m_userNotLoggedInMessageText.gameObject.SetActive(true);
                    m_userNotLoggedInMessageText.Localize(TID_OPTIONS_USERPROFILE_LOG_RECEIVE, SocialPlatformManager.SharedInstance.GetPlatformName());
                    break;

                    /*
                case EState.PreviouslyLoggedIn:
                    m_userPreviouslyLoggedInRoot.SetActive(true);                                        
                    string platformName = SocialPlatformManager.SharedInstance.GetPlatformName();
                    m_userPreviouslyLoggedInMessageText.Localize(TID_OPTIONS_USERPROFILE_LOG_NETWORK, platformName);
                    m_socialMessageText.Localize(TID_CLOUD_DESC, platformName);
                    break;     
                    */           
            }
        }
    }    

    private void User_LoadProfileInfo(bool needsToUpdateSocialId)
    {
        if (needsToUpdateSocialId)
        {
            m_userNameText.text = LocalizationManager.SharedInstance.Get(TID_LOADING);
        }

        Action<string, Texture2D> onDone = delegate (string userName, Texture2D profileImage)
        {            
            if (!string.IsNullOrEmpty(userName) && m_userNameText != null)
            {
                m_userNameText.text = userName;
            }
            
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
        };

        SocialPlatformManager.SharedInstance.GetSimpleProfileInfo(onDone);
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
        bool isLoggedIn = PersistenceFacade.instance.CloudDriver.IsLoggedIn;
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
                returnValue = PersistenceFacade.instance.CloudDriver.IsLoggedIn;
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
        return PersistenceFacade.instance.IsCloudSaveEnabled;
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