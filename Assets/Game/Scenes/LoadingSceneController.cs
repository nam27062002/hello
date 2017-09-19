// LoadingSceneController.cs
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

        public override void onAndroidPermissionPopupNeeded (CaletyConstants.PopupConfig kPopupConfig)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                LoadingSceneController.Log("onAndroidPermissionPopupNeeded: " + kPopupConfig.m_strMessage);

			PopupMessage.Config config = PopupMessage.GetConfig();
            config.TitleTid = kPopupConfig.m_strTitle;
			config.ShowTitle = !string.IsNullOrEmpty( kPopupConfig.m_strTitle);
			config.MessageTid = kPopupConfig.m_strMessage;
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

            // The user is not allowed to close this popup
            config.IsButtonCloseVisible = false;
			m_confirmPopup = PopupManager.PopupMessage_Open(config);			
        }

        void onConfirm()
        {
			AndroidPermissionsManager.AndroidPermissionsPopupButton button = (AndroidPermissionsManager.AndroidPermissionsPopupButton) m_popupConfig.m_kPopupButtons[0];
			button.m_pOnResponse();
			m_confirmPopup = null;
        }

        void onCancel()
        {
			AndroidPermissionsManager.AndroidPermissionsPopupButton button = (AndroidPermissionsManager.AndroidPermissionsPopupButton) m_popupConfig.m_kPopupButtons[1];
			button.m_pOnResponse();
			m_confirmPopup = null;
        }

        public override void onAndroidPermissionsFinished ()
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                LoadingSceneController.Log ("onAndroidPermissionsFinished");

			// Close popup and continue
            m_permissionsFinished = true;
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
    private bool m_loading = false;
    private bool m_loadingDone = false;


    private enum State
    {
    	NONE,
    	WAITING_SAVE_FACADE,
    	WAITING_SOCIAL_AUTH,
    	WAITING_ANDROID_PERMISSIONS,
    	COUNT
    }
    private State m_state = State.NONE;
	private AndroidPermissionsListener m_androidPermissionsListener = null;

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    override protected void Awake() {        		
        // Call parent
		base.Awake();
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
                    kNewAndroidDangerousPermission.m_strPermissionsMessage = text;

                    kAndroidPermissionsConfig.m_kAndroidDangerousPermissions.Add(kNewAndroidDangerousPermission);
                }
				kAndroidPermissionsConfig.m_strPermissionsInSettingsMessage = "TID_POPUP_ANDROID_PERMISSION_SETTINGS_TEXT";
				kAndroidPermissionsConfig.m_strPermissionsShutdownMessage   = "TID_POPUP_ANDROID_PERMISSION_EXIT";
				kAndroidPermissionsConfig.m_strPopupButtonYes               = "TID_POPUP_ANDROID_PERMISSION_ALLOW";
				kAndroidPermissionsConfig.m_strPopupButtonNo                = "TID_POPUP_ANDROID_PERMISSION_DENY";
				kAndroidPermissionsConfig.m_strPopupButtonSettings          = "TID_POPUP_ANDROID_PERMISSION_SETTINGS";
				kAndroidPermissionsConfig.m_strPopupButtonExit              = "TID_EXIT_GAME";

                AndroidPermissionsManager.SharedInstance.Initialise(ref kAndroidPermissionsConfig);

				if(!AndroidPermissionsManager.SharedInstance.CheckDangerousPermissions ()) {
                    // Application.targetFrameRate = 10;
					m_state = State.WAITING_ANDROID_PERMISSIONS;
				}else{
                    // Load persistence
                    SetState(State.WAITING_SAVE_FACADE);								        
			        // TEST
			        /*
					m_state = State.WAITING_ANDROID_PERMISSIONS;
					m_androidPermissionsListener.m_permissionsFinished = false;
					CaletyConstants.PopupConfig pConfig = new CaletyConstants.PopupConfig();
					pConfig.m_strTitle = "";
					pConfig.m_strMessage = "TID_POPUP_ANDROID_PERMISSION_SETTINGS_TEXT";
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
                SetState(State.WAITING_SAVE_FACADE);				
            }
        #else
			// Load persistence        
            SetState(State.WAITING_SAVE_FACADE);				
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
                    SetState(State.WAITING_SAVE_FACADE);                        
    			}
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
        m_state = state;

        switch (state)
        {
            case State.WAITING_SAVE_FACADE:
            {
				// [DGR] A single point to handle applications events (init, pause, resume, etc) in a high level.
        		// No parameter is passed because it has to be created only once in order to make sure that it's initialized only once
        		ApplicationManager.CreateInstance();

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
				ParticleManager.CreateInstance(true);
				FirePropagationManager.CreateInstance(true);
				SpawnerManager.CreateInstance(true);
				SpawnerAreaManager.CreateInstance(true);
				EntityManager.CreateInstance(true);
				InstanceManager.CreateInstance(true);

		        GameAds.CreateInstance(false);
		        GameAds.instance.Init();
                 
            	ControlPanel.CreateInstance();
                DragonManager.SetupUser(UsersManager.currentUser);
                MissionManager.SetupUser(UsersManager.currentUser);
                EggManager.SetupUser(UsersManager.currentUser);
                ChestManager.SetupUser(UsersManager.currentUser);
                GameStoreManager.SharedInstance.Initialize();
#if UNITY_ANDROID
				ApplicationManager.instance.GameCenter_Login();
#endif

                StartLoadFlow();
          	}break;
        }
    }
        
    private void StartLoadFlow()
    {
        if (m_startLoadFlow)
        {            
            m_startLoadFlow = false;
            m_loading = true;
            m_loadingDone = false;

            if (FeatureSettingsManager.IsDebugEnabled)
                Log("Started Loading Flow");

            Action onDone = delegate()
            {
                m_loadingDone = true;
                m_loading = false;

                HDTrackingManager.Instance.Notify_ApplicationStart();

                // Initialize managers needing data from the loaded profile
                GlobalEventManager.SetupUser(UsersManager.currentUser);
            };

            PersistenceFacade.instance.Sync_FromLaunchApplication(onDone);            			
        }
    }

    private const string LOG_CHANNEL = "[LOADING] ";
    public static void Log(string msg)
    {
        Debug.Log(LOG_CHANNEL + msg);
    }
}

