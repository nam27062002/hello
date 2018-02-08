﻿// LoadingSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Text.RegularExpressions;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Main controller for the loading scene.
/// </summary>
public class LoadingSceneController : SceneController {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string NAME = "SC_Loading";    

	//------------------------------------------------------------------//
	// CLASS															//
	//------------------------------------------------------------------//
	private class AndroidPermissionsListener : AndroidPermissionsManager.AndroidPermissionsListenerBase
    {
    	public bool m_permissionsFinished = false;
        private PopupController m_confirmPopup = null;
		private CaletyConstants.PopupConfig m_popupConfig;
		private bool m_actionsAllowed = true;

        public override void onAndroidPermissionPopupNeeded (CaletyConstants.PopupConfig kPopupConfig)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                LoadingSceneController.Log("onAndroidPermissionPopupNeeded: " + kPopupConfig.m_strMessage);

			PopupMessage.Config config = PopupMessage.GetConfig();
            config.TitleText = kPopupConfig.m_strTitle;
			config.ShowTitle = !string.IsNullOrEmpty( kPopupConfig.m_strTitle);
			config.MessageText = kPopupConfig.m_strMessage;
            // This popup ignores back button and stays open so the user makes a decision
            config.BackButtonStrategy = PopupMessage.Config.EBackButtonStratety.None;

			m_popupConfig = kPopupConfig;
            if (kPopupConfig.m_kPopupButtons.Count == 2)
            {
				AndroidPermissionsManager.AndroidPermissionsPopupButton confirmButtonConfig = (AndroidPermissionsManager.AndroidPermissionsPopupButton) kPopupConfig.m_kPopupButtons [0];
				config.ConfirmButtonTid = confirmButtonConfig.m_strText;
				config.OnConfirm = onConfirm;

				AndroidPermissionsManager.AndroidPermissionsPopupButton cancelButtonConfig = (AndroidPermissionsManager.AndroidPermissionsPopupButton) kPopupConfig.m_kPopupButtons [1];
				config.CancelButtonTid = cancelButtonConfig.m_strText;
				config.OnCancel = onCancel;

	            config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
            }
            else if (kPopupConfig.m_kPopupButtons.Count == 1)
            {
				AndroidPermissionsManager.AndroidPermissionsPopupButton kAndroidPermissionButton = (AndroidPermissionsManager.AndroidPermissionsPopupButton) kPopupConfig.m_kPopupButtons [0];
				config.ConfirmButtonTid = kAndroidPermissionButton.m_strText;
	            config.OnConfirm = onConfirm;
	            config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
            }

			// Allow actions
			m_actionsAllowed = true;

            // The user is not allowed to close this popup
            config.IsButtonCloseVisible = false;
			m_confirmPopup = PopupManager.PopupMessage_Open(config);			
        }

        void onConfirm()
        {
			// Ignore if actions are not allowed
			if(!m_actionsAllowed) return;

			// Ignore further actions (prevent spamming)
			m_actionsAllowed = false;

			// Add some delay to give enough time for SFX to be played before losing focus
			UbiBCN.CoroutineManager.DelayedCall(
				() => {
					AndroidPermissionsManager.AndroidPermissionsPopupButton button = (AndroidPermissionsManager.AndroidPermissionsPopupButton) m_popupConfig.m_kPopupButtons[0];
					button.m_pOnResponse();
					m_confirmPopup = null;
					m_actionsAllowed = true;
				}, 0.15f
			);
        }

        void onCancel()
        {
			// Ignore if actions are not allowed
			if(!m_actionsAllowed) return;

			// Ignore further actions (prevent spamming)
			m_actionsAllowed = false;

			// Add some delay to give enough time for SFX to be played before losing focus
			UbiBCN.CoroutineManager.DelayedCall(
				() => {
					AndroidPermissionsManager.AndroidPermissionsPopupButton button = (AndroidPermissionsManager.AndroidPermissionsPopupButton) m_popupConfig.m_kPopupButtons[1];
					button.m_pOnResponse();
					m_confirmPopup = null;
					m_actionsAllowed = true;
				}, 0.15f
			);
        }

        public override void onAndroidPermissionsFinished ()
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                LoadingSceneController.Log ("onAndroidPermissionsFinished");

			// Close popup and continue
            m_permissionsFinished = true;
			m_actionsAllowed = true;
        }
    }



    //------------------------------------------------------------------//
    // MEMBERS															//
    //------------------------------------------------------------------//
    // References
	[SerializeField] private TextMeshProUGUI m_loadingTxt = null;
	[SerializeField] private Slider m_loadingBar = null;
	public InitRefObject m_savingReferences;	// this will point to an asset that points to lozalication and rules to make sure they get into the apk and not in the obb file
	public GameObject m_messagePopup;

	// Internal
	private float timer = 0;

    private bool m_startLoadFlow = true;    
    private bool m_loadingDone = false;

    private float m_stateDuration = 0;

    private enum State
    {
    	NONE,        
        WAITING_SAVE_FACADE,
    	WAITING_SOCIAL_AUTH,
    	WAITING_ANDROID_PERMISSIONS,
        WAITING_FOR_RULES,
        LOADING_RULES,
        SHOWING_UPGRADE_POPUP,        
        COUNT
    }
    private State m_state = State.NONE;
	private AndroidPermissionsListener m_androidPermissionsListener = null;
	private string m_buildVersion;

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    override protected void Awake() {        		
        // Call parent
		base.Awake();
		CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
		if ( settingsInstance )
		{
			m_buildVersion = settingsInstance.GetClientBuildVersion();
		}
		else
		{
			m_buildVersion = Application.version;
		}
		CacheServerManager.SharedInstance.Init(m_buildVersion);
		ContentManager.InitContent();
		// Check required references
		DebugUtils.Assert(m_loadingTxt != null, "Required component!"); 

		// Used for android permissions
		PopupManager.CreateInstance(true);
        // Initialize localization
        SetSavedLanguage();      
    }    

	/// <summary>
	/// First update.
	/// </summary>
	void Start() {                        
        // Load menu scene
        //GameFlow.GoToMenu();
        // [AOC] TEMP!! Simulate loading time with a timer for now
        timer = 0;
        m_stateDuration = 0;

        // [AOC] This is a safe place to instantiate all the singletons
        //		 Do it now so we have it under control
        //		 Add all the new created singletons
        // Content and persistence
        //DefinitionsManager.CreateInstance(true);	// Moved to Awake() so content is the very first thing loaded (a lot of things depend on it)


#if UNITY_ANDROID
            CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
            if (settingsInstance != null)
            {
                m_androidPermissionsListener = new AndroidPermissionsListener ();
				AndroidPermissionsManager.SharedInstance.SetListener (m_androidPermissionsListener);

                AndroidPermissionsManager.AndroidPermissionsConfig kAndroidPermissionsConfig = new AndroidPermissionsManager.AndroidPermissionsConfig ();
                for (int i = 0; i < settingsInstance.m_kAndroidDangerousPermissions.Count; ++i)
                {
                    AndroidPermissionsManager.AndroidDangerousPermission kNewAndroidDangerousPermission = new AndroidPermissionsManager.AndroidDangerousPermission();
                    kNewAndroidDangerousPermission.m_strPermission = settingsInstance.m_kAndroidDangerousPermissions[i];

                    string text = settingsInstance.m_kAndroidDangerousPermissions[i];   // Get the text
                    text = text.Replace (".", "_").ToUpper ();                          // Replace . per _ and convert it to mayus
                    text = "TID_" + text;                                               // Add TID_
					kNewAndroidDangerousPermission.m_strPermissionsMessage = LocalizationManager.SharedInstance.Localize(text);

                    kAndroidPermissionsConfig.m_kAndroidDangerousPermissions.Add(kNewAndroidDangerousPermission);
                }
				kAndroidPermissionsConfig.m_strPermissionsInSettingsMessage = LocalizationManager.SharedInstance.Localize( "TID_POPUP_ANDROID_PERMISSION_SETTINGS_TEXT" );
				kAndroidPermissionsConfig.m_strPermissionsShutdownMessage   = LocalizationManager.SharedInstance.Localize( "TID_POPUP_ANDROID_PERMISSION_EXIT" );
				kAndroidPermissionsConfig.m_strPopupButtonYes               = "TID_POPUP_ANDROID_PERMISSION_ALLOW";
				kAndroidPermissionsConfig.m_strPopupButtonNo                = "TID_POPUP_ANDROID_PERMISSION_DENY";
				kAndroidPermissionsConfig.m_strPopupButtonSettings          = "TID_POPUP_ANDROID_PERMISSION_SETTINGS";
				kAndroidPermissionsConfig.m_strPopupButtonExit              = "TID_EXIT_GAME";

                AndroidPermissionsManager.SharedInstance.Initialise(ref kAndroidPermissionsConfig);
                
				if(!AndroidPermissionsManager.SharedInstance.CheckDangerousPermissions ()) {
                    // Application.targetFrameRate = 10;
					SetState(State.WAITING_ANDROID_PERMISSIONS);
				}else{
                    // Load persistence
                    if ( CacheServerManager.SharedInstance.GameNeedsUpdate() )
                    {
						SetState(State.SHOWING_UPGRADE_POPUP);
                    }
                    else
                    {
                    	SetState(State.WAITING_FOR_RULES);
                    }
			        // TEST
			        /*
					m_state = State.WAITING_ANDROID_PERMISSIONS;
					m_androidPermissionsListener.m_permissionsFinished = false;
					CaletyConstants.PopupConfig pConfig = new CaletyConstants.PopupConfig();
					pConfig.m_strTitle = "";
					pConfig.m_strMessage = LocalizationManager.SharedInstance.Localize("TID_POPUP_ANDROID_PERMISSION_SETTINGS_TEXT");
					pConfig.m_strIconURL = "";
					AndroidPermissionsManager.AndroidPermissionsPopupButton button = new AndroidPermissionsManager.AndroidPermissionsPopupButton();
					button.m_strText = "TID_EXIT_GAME";
					button.m_pOnResponse = m_androidPermissionsListener.onAndroidPermissionsFinished;
					pConfig.m_kPopupButtons.Add(button);
					m_androidPermissionsListener.onAndroidPermissionPopupNeeded( pConfig );
					*/
				}
            }
            else
            {
				// Load persistence
                if ( CacheServerManager.SharedInstance.GameNeedsUpdate() )
                {
					SetState(State.SHOWING_UPGRADE_POPUP);
                }
                else
                {
                	SetState(State.WAITING_FOR_RULES);
                }
            }
        #else
			// Load persistence
            if ( CacheServerManager.SharedInstance.GameNeedsUpdate() )
            {
				SetState(State.SHOWING_UPGRADE_POPUP);
            }
            else
            {
            	SetState(State.WAITING_FOR_RULES);
            }			
        #endif
    }

    public static void SetSavedLanguage()
    {
		// Load set language from preferences
        string strLanguageSku = PlayerPrefs.GetString(PopupSettings.KEY_SETTINGS_LANGUAGE);
        if (string.IsNullOrEmpty(strLanguageSku))
        {
			// No language was defined, load default system language
			strLanguageSku = LocalizationManager.SharedInstance.GetDefaultSystemLanguage();
        }

        LocalizationManager.SharedInstance.SetLanguage(strLanguageSku);

		// [AOC] If the setting is enabled, replace missing TIDs for english ones
		if(!Prefs.GetBoolPlayer(DebugSettings.SHOW_MISSING_TIDS, false)) {
			LocalizationManager.SharedInstance.FillEmptyTids("lang_english");
		}
    }    

    /// <summary>
    /// Called every frame.
    /// </summary>
    void Update() {       

    	switch( m_state )
    	{
    		case State.NONE:
    		{

    		}break;
    		case State.WAITING_ANDROID_PERMISSIONS:
    		{
    			if ( m_androidPermissionsListener.m_permissionsFinished )
    			{  
                    // Load persistence        
                    if ( CacheServerManager.SharedInstance.GameNeedsUpdate() )
                    {
						SetState(State.SHOWING_UPGRADE_POPUP);
                    }
                    else
                    {
						SetState(State.WAITING_FOR_RULES);
                    }

    			}
    		}break;
            case State.WAITING_FOR_RULES:
            {
                if (ContentManager.ready)
                {
                    // The state is not changed to WAITING_SAVE_FACADE yet because we want to be sure that other scripts checking for ContentManager.ready
                    // in their Update() (for example FeatureSettingsManager, which has to load game settings according to rules and server) have time to do 
                    // their stuff before the flow goes on
                    SetState(State.LOADING_RULES);                        
                }
            }break;
            case State.LOADING_RULES:
            {
                // A tick is enought to do this state stuff as we just want to wait a tick so all scripts have the chance to realize content is ready
                SetState(State.WAITING_SAVE_FACADE);                    
            }break;
            default:
    		{
				// Update load progress
				//m_loadingTxt.text = System.String.Format("LOADING {0}%", StringUtils.FormatNumber(SceneManager.loadProgress * 100f, 0));

				// [AOC] TODO!! Fake timer for now
				timer += Time.deltaTime;
				float loadProgress = Mathf.Min(timer/1f, 1f);	// Divide by the amount of seconds to simulate
				//m_loadingTxt.text = System.String.Format("LOADING {0}%", StringUtils.FormatNumber(loadProgress * 100f, 0));
				m_loadingTxt.text = "Loading";	// Don't show percentage (too techy), don't localize (language data not yet loaded)

				if (m_loadingBar != null)
					m_loadingBar.normalizedValue = loadProgress;                    		        	        

		        // Once load is finished, navigate to the menu scene
		        if (loadProgress >= 1f && !GameSceneManager.isLoading && m_loadingDone) {
		            FlowManager.GoToMenu();
		        }
    		}break;
    	}		
    }

	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
		base.OnDestroy();
	}

    private void SetState(State state)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            float deltaTime = Time.timeSinceLevelLoad - m_stateDuration;
            m_stateDuration = Time.timeSinceLevelLoad;
            Log(m_state + " -> " + state + " time = " + deltaTime);
        }

        m_state = state;

        switch (state)
        {            
        	case State.SHOWING_UPGRADE_POPUP:
        	{
        		PopupManager.OpenPopupInstant( PopupUpgrade.PATH );
        	}break;
            case State.WAITING_SAVE_FACADE:
            {
				// [DGR] A single point to handle applications events (init, pause, resume, etc) in a high level.
        		// No parameter is passed because it has to be created only once in order to make sure that it's initialized only once
        		ApplicationManager.CreateInstance();

        		AntiCheatsManager.CreateInstance();

				// The stuff that this manager handles has to be done only once, regardless the game reboots
				FeatureSettingsManager.CreateInstance(false);

				if (FeatureSettingsManager.instance.IsMiniTrackingEnabled) {
		            // Initialize local mini-tracking session!
		            // [AOC] Generate a unique ID with the device's identifier and the number of progress resets
		            MiniTrackingEngine.InitSession(SystemInfo.deviceUniqueIdentifier + "_" + PlayerPrefs.GetInt("RESET_PROGRESS_COUNT", 0).ToString());
		        }

				HDTrackingManager.Instance.Init();
				UsersManager.CreateInstance();

		        // Game		        
                PersistenceFacade.instance.Reset();

				// Meta
				SeasonManager.CreateInstance(true);
		        DragonManager.CreateInstance(true);
				LevelManager.CreateInstance(true);
				MissionManager.CreateInstance(true);
				ChestManager.CreateInstance(true);
				RewardManager.CreateInstance(true);
				EggManager.CreateInstance(true);
				EggManager.InitFromDefinitions();

				// Settings and setup
				GameSettings.CreateInstance(false);

				// Tech
				GameSceneManager.CreateInstance(true);
				FlowManager.CreateInstance(true);
				PoolManager.CreateInstance(true);
				ActionPointManager.CreateInstance(true);
				ParticleManager.CreateInstance(true);
				FirePropagationManager.CreateInstance(true);
				SpawnerManager.CreateInstance(true);
				SpawnerAreaManager.CreateInstance(true);
				EntityManager.CreateInstance(true);
				ViewManager.CreateInstance(true);
				InstanceManager.CreateInstance(true);

		        GameAds.CreateInstance(false);
		        GameAds.instance.Init();
                 
            	ControlPanel.CreateInstance();
                DragonManager.SetupUser(UsersManager.currentUser);
                MissionManager.SetupUser(UsersManager.currentUser);
                EggManager.SetupUser(UsersManager.currentUser);
                ChestManager.SetupUser(UsersManager.currentUser);
                GameStoreManager.SharedInstance.Initialize();

				if ( ApplicationManager.instance.GameCenter_LoginOnStart() )
				{
					ApplicationManager.instance.GameCenter_Login();
				}

                HDNotificationsManager.CreateInstance();
                HDNotificationsManager.instance.Initialise();
				
				StartLoadFlow();	            	                                
          	}break;
        }
    }


        
    private void StartLoadFlow()
    {
        if (m_startLoadFlow)
        {            
            m_startLoadFlow = false;            
            m_loadingDone = false;

            if (FeatureSettingsManager.IsDebugEnabled)
                Log("Started Loading Flow");

            Action onDone = delegate()
            {                
                HDTrackingManager.Instance.Notify_ApplicationStart();

                // Initialize managers needing data from the loaded profile
                GlobalEventManager.SetupUser(UsersManager.currentUser);

                // Automatic connection check is enabled once the loading is over
                GameServerManager.SharedInstance.Connection_SetIsCheckEnabled(true);

                // Game will be loaded only if the device is supported, otherwise a popup is shown suggesting the user download HSE. 
                // We need to wait until this point to open this popup because we need to read local persistence to have access to
                // tracking id since the user's interaction with this popup has to be tracked
                if (FeatureSettingsManager.instance.Device_IsSupported())
                {
                    m_loadingDone = true;
                }
                else
                {
                    Popup_ShowUnsupportedDevice();
                }
            };

            // Automatic connection check disabled during loading because network is already being used
            GameServerManager.SharedInstance.Connection_SetIsCheckEnabled(false);
            PersistenceFacade.instance.Sync_FromLaunchApplication(onDone);

            HDTrackingManager.Instance.Notify_Razolytics_Funnel_Load(FunnelData_LoadRazolytics.Steps._00_start);        			
        }
    }

    #region unsupported_device
    private void Popup_ShowUnsupportedDevice()
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_TITLE_UNSUPPORTED_DEVICE";
        config.MessageTid = "TID_BODY_UNSUPPORTED_DEVICE";
        config.IsButtonCloseVisible = false;

        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;        
        config.OnConfirm = UnsupportedDevice_OnGoToLink;
        config.ConfirmButtonTid = "TID_BUTTON_UNSUPPORTED_DEVICE";
        config.OnCancel = UnsupportedDevice_OnQuit;
        config.CancelButtonTid = "TID_PAUSE_TAB_OPTIONS_QUIT";

        // Back button is disabled in order to make sure that the user is aware when making such an important decision
        config.BackButtonStrategy = PopupMessage.Config.EBackButtonStratety.None;
        PopupManager.PopupMessage_Open(config);

        HDTrackingManager.Instance.Notify_PopupUnsupportedDeviceAction(HDTrackingManager.EPopupUnsupportedDeviceAction.Shown);
    }

    private void UnsupportedDevice_OnGoToLink()
    {        
        HDTrackingManager.Instance.Notify_PopupUnsupportedDeviceAction(HDTrackingManager.EPopupUnsupportedDeviceAction.Leave2HSE);

        // HSE is opened in store        
        ApplicationManager.Apps_OpenAppInStore(ApplicationManager.EApp.HungrySharkEvo);
    }  

    private void UnsupportedDevice_OnQuit()
    {
        HDTrackingManager.Instance.Notify_PopupUnsupportedDeviceAction(HDTrackingManager.EPopupUnsupportedDeviceAction.Quit);        

        // The user quits the application
        Application.Quit();
    }
    #endregion

    #region log
    private const string LOG_CHANNEL = "[LOADING] ";
    public static void Log(string msg)
    {
        Debug.Log(LOG_CHANNEL + msg);
    }
    #endregion
}

