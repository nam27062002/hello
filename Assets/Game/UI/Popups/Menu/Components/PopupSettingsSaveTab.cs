// PopupSettingsSaveTab.cs
// Hungry Dragon
// 
// Created by David Germade on 30th August 2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif


using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using TMPro;
using FGOL.Server;

/// <summary>
/// This class is responsible for handling the save tab in the settings popup. This tab is used for three things:
///   -Social: Log in/Log out to social network
///   -Cloud: Enable/Disable the cloud save
///   -Resync: Force a sync with the cloud if it's enabled
/// </summary>
public class PopupSettingsSaveTab : MonoBehaviour
{
    private const string TID_LOADING = "TID_GEN_LOADING";

#if UNITY_ANDROID
	private const string TID_LOGIN_ERROR = "TID_GOOGLE_PLAY_AUTH_ERROR";
#elif UNITY_IPHONE
	private const string TID_LOGIN_ERROR = "TID_GAME_CENTER_AUTH_ERROR";
#else
    private const string TID_LOGIN_ERROR = "";
#endif

    private bool Shown { get; set; }

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	void Awake()
    {            
		Model_Init();
		Social_Init();
		Resync_Init();
        IAP_restore_Init();        
		GameCenter_Init();
        Shown = false;
    }

	/// <summary>
	/// 
	/// </summary>
	public void OnShow(){
        if (!Shown) {
            Shown = true;

#if UNITY_ANDROID
		    RefreshGooglePlayView();
		    Messenger.AddListener(MessengerEvents.GOOGLE_PLAY_AUTH_CANCELLED, GooglePlayAuthCancelled);
		    Messenger.AddListener(MessengerEvents.GOOGLE_PLAY_AUTH_FAILED, GooglePlayAuthFailed);
#endif
            Messenger.AddListener(MessengerEvents.GOOGLE_PLAY_STATE_UPDATE, RefreshGooglePlayView);
            Messenger.AddListener<bool>(MessengerEvents.SOCIAL_LOGGED, Social_OnLogInUpdated);
        }
	}

	/// <summary>
	/// 
	/// </summary>
    public void OnHide(){
        if (Shown){
            Shown = false;

#if UNITY_ANDROID
            Messenger.RemoveListener(MessengerEvents.GOOGLE_PLAY_AUTH_CANCELLED, GooglePlayAuthCancelled);
            Messenger.RemoveListener(MessengerEvents.GOOGLE_PLAY_AUTH_FAILED, GooglePlayAuthFailed);
#endif
            Messenger.RemoveListener(MessengerEvents.GOOGLE_PLAY_STATE_UPDATE, RefreshGooglePlayView);
			Messenger.RemoveListener<bool>(MessengerEvents.SOCIAL_LOGGED, Social_OnLogInUpdated);

        }
    }    

	/// <summary>
	/// 
	/// </summary>
    void OnDestroy() {
        if (Shown) {
            OnHide();
        }
    }

	/// <summary>
	/// 
	/// </summary>
    void OnEnable()
    {        
        RefreshView();
    }            

	/// <summary>
	/// 
	/// </summary>
    private void RefreshView()
    {
        Model_Refresh();
        Social_Refresh();
        Resync_Refresh();
    }

	/// <summary>
	/// 
	/// </summary>
    private bool IsLoadingPopupOpen { get; set; }

	/// <summary>
	/// 
	/// </summary>
    private void OpenLoadingPopup()
    {
        IsLoadingPopupOpen = true;
        PersistenceFacade.Popups_OpenLoadingPopup();
    }

	/// <summary>
	/// 
	/// </summary>
    private void CloseLoadingPopup()
    {
        IsLoadingPopupOpen = false;
        PersistenceFacade.Popups_CloseLoadingPopup();
    }

	//------------------------------------------------------------------------//
	// GAME PLATFORM SECTION												  //
	//------------------------------------------------------------------------//
	#region game_platform
	[Separator("Game Platform")]
	[SerializeField] private GameObject m_googlePlayGroup = null;
	[SerializeField] private GameObject m_googlePlayLoginButton = null;
	[SerializeField] private GameObject m_googlePlayLogoutButton = null;
	[SerializeField] private Button m_googlePlayAchievementsButton = null;
	[Space]
	[SerializeField] private GameObject m_gameCenterGroup = null;

	private PopupController m_loadingPopupController = null;

	/// <summary>
	/// 
	/// </summary>
	private void GameCenter_Init() {
		// Disable google play group if not available
		#if UNITY_ANDROID
		m_googlePlayGroup.SetActive(true);
		m_gameCenterGroup.SetActive(false);	
		#elif UNITY_IOS
		m_googlePlayGroup.SetActive(false);
		m_gameCenterGroup.SetActive(true);
		#else
		m_googlePlayGroup.SetActive(false);
		m_gameCenterGroup.SetActive(false);
		#endif
	}	

	/// <summary>
	/// 
	/// </summary>
	public void RefreshGooglePlayView(){

		#if UNITY_ANDROID
		if ( m_loadingPopupController != null ){
			m_loadingPopupController.Close(true);
			m_loadingPopupController = null;
		}

		if ( ApplicationManager.instance.GameCenter_IsAuthenticated() ){
			m_googlePlayLoginButton.SetActive(false);
			m_googlePlayLogoutButton.SetActive(true);
			m_googlePlayAchievementsButton.interactable = true;
		}else{
			m_googlePlayLoginButton.SetActive(true);
			m_googlePlayLogoutButton.SetActive(false);
			m_googlePlayAchievementsButton.interactable = false;
		}
		#endif

	}

	/// <summary>
	/// 
	/// </summary>
	public void GooglePlayAuthCancelled(){
		RefreshGooglePlayView();
	}

	/// <summary>
	/// 
	/// </summary>
	public void GooglePlayAuthFailed(){
		RefreshGooglePlayView();

		// Show generic message there was an error!
		UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize(TID_LOGIN_ERROR), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnGooglePlayLogIn(){
		if (!ApplicationManager.instance.GameCenter_IsAuthenticated()){

            if (DeviceUtilsManager.SharedInstance.internetReachability == NetworkReachability.NotReachable)
            {                
                UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_GEN_NO_CONNECTION"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
            }
            else
            {
                // Show curtain and wait for game center response
                if (!GameCenterManager.SharedInstance.GetAuthenticatingState()) // if not authenticating
                {
                    ApplicationManager.instance.GameCenter_Login();
                }

                if (GameCenterManager.SharedInstance.GetAuthenticatingState())
                {
                    m_loadingPopupController = PopupManager.PopupLoading_Open();
                }
                else
                {
                    // No curatin -> something failed, we are not authenticating -> tell the player there was an error
                    UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize(TID_LOGIN_ERROR), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
                }
            }
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnGooglePlayLogOut()
	{
		if (ApplicationManager.instance.GameCenter_IsAuthenticated())
		{
			// Show popup message
			IPopupMessage.Config config = IPopupMessage.GetConfig();
			config.ShowTitle = false;
			config.MessageTid = "TID_GEN_CONFIRM_LOGOUT";
			config.BackButtonStrategy = IPopupMessage.Config.EBackButtonStratety.None;
			config.ConfirmButtonTid = "TID_GEN_YES";
			config.OnConfirm = OnLogOutGooglePlay;
			config.CancelButtonTid = "TID_GEN_NO";
			config.OnCancel = null;
            config.ButtonMode = IPopupMessage.Config.EButtonsMode.ConfirmAndCancel;
			config.IsButtonCloseVisible = false;
			config.BackButtonStrategy = IPopupMessage.Config.EBackButtonStratety.PerformCancel;
			PopupManager.PopupMessage_Open(config);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnLogOutGooglePlay()
	{
		if (ApplicationManager.instance.GameCenter_IsAuthenticated())
		{
			ApplicationManager.instance.GameCenter_LogOut();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnGooglePlayAchievements(){
		if (ApplicationManager.instance.GameCenter_IsAuthenticated()){
			// Add some delay to give enough time for SFX to be played before losing focus
			UbiBCN.CoroutineManager.DelayedCall(
				() => {
					ApplicationManager.instance.GameCenter_ShowAchievements();
				}, 0.15f
			);
		}
	}
	#endregion

	//------------------------------------------------------------------------//
	// SOCIAL LOGIN AND CLOUD SAVE SECTION									  //
	//------------------------------------------------------------------------//
	#region social
	// This region is responsible for handling social stuff
	[Separator("Social")]
	[SerializeField] private GameObject m_socialNotLoggedInRoot = null;
    [SerializeField] private GameObject m_socialLoggedInRoot = null;
	[Space]
	// User profile GameObject to use when the user has never logged in. It encourages the user to log in
	[FormerlySerializedAs("m_userNotLoggedInRoot")]
	[SerializeField] private GameObject m_loginRewardRoot = null;
	[SerializeField] private Localizer m_loginRewardtext = null;
	[SerializeField] private UISocialSetup m_logoutIcon = null;

	/// <summary>
	/// 
	/// </summary>
	private void Social_Init()
    {
		m_socialNotLoggedInRoot.SetActive(true);
		m_socialLoggedInRoot.SetActive(false);
        m_loginRewardRoot.SetActive(false);
    }

	private void Social_OnLogInUpdated(bool logged)
	{
		// Do not refresh the view until the sync is completed
		if (!PersistenceFacade.instance.CloudDriver.IsInSync)
		{
			RefreshView();
		}
	}

	/// <summary>
	/// 
	/// </summary>
    private void Social_Refresh()
    {        
		// If the user is EXPLICITELY logged in into a social platform, show the logout layout instead
        bool isExplicitelyLoggedIn = Model_SocialIsLoggedIn();
		isExplicitelyLoggedIn &= !SocialPlatformManager.SharedInstance.CurrentPlatform_IsImplicit();

		// Show target layout
		m_socialNotLoggedInRoot.SetActive(!isExplicitelyLoggedIn);
		m_socialLoggedInRoot.SetActive(isExplicitelyLoggedIn);

		if(m_logoutIcon != null) {
			m_logoutIcon.Refresh();
		}

        if (Model_State == EState.NeverLoggedIn && FeatureSettingsManager.instance.IsIncentivisedLoginEnabled())
        {
            m_loginRewardRoot.SetActive(true);
            m_loginRewardtext.gameObject.SetActive(true);
            PersistenceFacade.Texts_LocalizeIncentivizedSocial(m_loginRewardtext);
        }
        else
        {
            m_loginRewardRoot.SetActive(false);
        }      
   }

	/// <summary>
	/// Callback called by the player when the user clicks on log in the social network.
	/// Because Unity doesn't allow button callbacks to have Enum as parameter, create
	/// a wrapper method for each platform.
	/// </summary>
	public void Social_OnLoginButton_Facebook() {
		Social_OnLoginButton(SocialUtils.EPlatform.Facebook);
	}

	public void Social_OnLoginButton_Weibo() {
		Social_OnLoginButton(SocialUtils.EPlatform.Weibo);
	}

	public void Social_OnLoginButton_Apple() {
        Social_OnLoginButton(SocialUtils.EPlatform.SIWA);
	}

	public void Social_OnLoginButton(SocialUtils.EPlatform _platform)
    {
        // [DGR] ANALYTICS: Not supported yet
        // HSXAnalyticsManager.Instance.loginContext = "OptionsLogin";

        OpenLoadingPopup();
        Action onDone = delegate()
        {
            CloseLoadingPopup();         
            if (Model_SocialIsLoggedIn())
            {                
                Log("LOGIN SUCCESSFUL");                 
            }
            else
            {                
                Log("LOGIN FAILED");                
            }

            RefreshView();
        };

        PersistenceFacade.instance.Sync_FromSettings(_platform, onDone);        
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
                    Log("LOGGING OUT... ");                    

                    //OpenLoadingPopup();                    
                    PersistenceFacade.instance.CloudDriver.Logout();
                    RefreshView();                        
                },
                null
            );                                
        }
        else 
        {
            LogError("LOGIN: Not logged in to " ); 
        }        
    }
	#endregion	

	//------------------------------------------------------------------------//
	// CLOUD SAVE RESYNC LOGIC												  //
	//------------------------------------------------------------------------//
	#region resync
	[Separator("Cloud Save")]
	[SerializeField] private Button m_resyncButton = null;	// [AOC] Could be removed since CloudSave can no longer be disabled once logged in

    private bool Resync_IsRunning { get; set; }

	/// <summary>
	/// 
	/// </summary>
    private void Resync_Init()
    {
        Resync_IsRunning = false;
    }

	/// <summary>
	/// 
	/// </summary>
    private void Cloud_Reset()
    {
        Resync_IsRunning = false;
    }

	/// <summary>
	/// 
	/// </summary>
    private void Resync_Refresh()
    {
        m_resyncButton.interactable = Model_SaveIsCloudSaveEnabled() && Model_SocialIsLoggedIn();        
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
                RefreshView();                
            };

            Resync_IsRunning = true;

			// Uses the same social platform that is currently in usage since the user can not change social platforms
			// by clicking on save sync
			PersistenceFacade.instance.Sync_FromSettings(SocialPlatformManager.SharedInstance.CurrentPlatform_GetId(), onDone);
        }
    }
	#endregion

	//------------------------------------------------------------------------//
	// IAP RESTORE LOGIC													  //
	//------------------------------------------------------------------------//
	#region IAP_restore
	[Separator("IAP Restore")]
	[SerializeField] private GameObject m_restoreIAP_IOSLayout = null;
	[SerializeField] private GameObject m_restoreIAP_AndroidLayout = null;

	/// <summary>
	/// 
	/// </summary>
    private void IAP_restore_Init()
    {
		// Different layouts based on platform
#if UNITY_IOS || UNITY_EDITOR
		m_restoreIAP_IOSLayout.SetActive(true);
		m_restoreIAP_AndroidLayout.SetActive(false);
#else
		m_restoreIAP_IOSLayout.SetActive(false);
		m_restoreIAP_AndroidLayout.SetActive(true);
#endif
    }
    #endregion

    //------------------------------------------------------------------------//
    // MODEL																  //
    //------------------------------------------------------------------------//
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

	/// <summary>
	/// 
	/// </summary>
	/// <param name="state"></param>
	/// <returns></returns>
    private bool Model_HasBeenLoaded(EState state)
    {
        return state == EState.PreviouslyLoggedIn || state == EState.LoggedIn || state == EState.LoggedInAndIncentivised;
    }

	/// <summary>
	/// 
	/// </summary>
    private void Model_Init()
    {
        Model_Refresh();
    }                

	/// <summary>
	/// 
	/// </summary>
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

                case UserProfile.ESocialState.LoggedInAndIncentivised:
                    state = (isLoggedIn) ? EState.LoggedInAndIncentivised : EState.PreviouslyLoggedIn;
                    break;                
            }
        }

        Model_State = state;
    }

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
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

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
    private bool Model_SaveIsCloudSaveEnabled()
    {
        return PersistenceFacade.instance.IsCloudSaveEnabled;
    }
	#endregion

	//------------------------------------------------------------------------//
	// UTILS																  //
	//------------------------------------------------------------------------//
	#region utils
	/// <summary>
	/// 
	/// </summary>
	/// <param name="waitTime"></param>
	/// <param name="callback"></param>
	/// <returns></returns>
	System.Collections.IEnumerator DelayedCall(float waitTime, Action callback)
    {
        yield return new WaitForSecondsRealtime(waitTime);
        callback();
    }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_layout"></param>
	public void ForceLayoutRefresh(HorizontalOrVerticalLayoutGroup _layout) {
		// [AOC] Enabling/disabling objects while the layout is inactive makes the layout to not update properly
		//		 Luckily for us Unity provides us with the right tools to rebuild it
		//		 Fixes issue https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-690
		if(_layout != null) LayoutRebuilder.ForceRebuildLayoutImmediate(_layout.transform as RectTransform);
	}
	#endregion

	//------------------------------------------------------------------------//
	// DEBUG																  //
	//------------------------------------------------------------------------//
	#region log
	private static string LOG_CHANNEL = "[SAVE_TAB]";

	/// <summary>
	/// 
	/// </summary>
	/// <param name="message"></param>
#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void Log(string message)
    {
        Debug.Log(LOG_CHANNEL + message);        
    }

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
	/// <summary>
	/// 
	/// </summary>
	/// <param name="message"></param>
	public static void LogError(string message)
    {
        Debug.LogError(LOG_CHANNEL + message);
    }
#endregion

	
}