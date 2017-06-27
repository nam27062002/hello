// PersistenceManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

using FGOL.Save;
using FGOL.Save.SaveStates;

using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;


//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Class responsible to save/load persistence of the game, either local or online.
/// Static class so it can easily be accessed from anywhere.
/// Saved games names should match persistence profile names, although PersistenceProfile logic
/// is completely independent from the PersistenceManager to allow more flexibility regarding
/// future needs.
/// </summary>
public class PersistenceManager : Singleton<PersistenceManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private const string TAG = "PersistenceManager";

	// Make sure persistence JSON is formatted equal in all systems!
	public static readonly CultureInfo JSON_FORMATTING_CULTURE = CultureInfo.InvariantCulture;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Path where the data files are stored
	public static string saveDir {
		// Hidden file directory of Unity, see https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
		get { return Application.persistentDataPath; }
	}

	// Default persistence profile - it's stored in the player preferences, that way can be set from the editor and read during gameplay
	public static string activeProfile {
		get { return PlayerPrefs.GetString("activeProfile", PersistenceProfile.DEFAULT_PROFILE); }
		set
        {
            PlayerPrefs.SetString("activeProfile", value);

            //[DGR] Local ID has to be the same as the active profile
            SaveGameManager.LocalSaveID = activeProfile;
        }
	}

	private bool m_loadCompleted = false;
	public static bool loadCompleted {
		get { return instance.m_loadCompleted; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization. Must be called upon starting the application.
	/// </summary>
	public static void Init() {
		// Forces a different code path in the BinaryFormatter that doesn't rely on run-time code generation (which would break on iOS).
		// From http://answers.unity3d.com/questions/30930/why-did-my-binaryserialzer-stop-working.html
		Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
		instance.m_loadCompleted = false;
        Popups_Init();
    }

	//------------------------------------------------------------------//
	// MAIN PUBLIC METHODS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load the game persistence for a specific profile into the game managers.
	/// </summary>
	/// <param name="_profileName">The name of the profile to be loaded.</param>
	public static void Load(string _profileName = "")
    {               
        DragonManager.SetupUser(UsersManager.currentUser);
		MissionManager.SetupUser(UsersManager.currentUser);
        EggManager.SetupUser(UsersManager.currentUser);
		ChestManager.SetupUser(UsersManager.currentUser);

        //[DGR] FGOL SaveFacade is used to load the persistence
        // Makes sure Local ID is pointing to the active profile
		// [AOC] Added OnLoadCompleted callback
        SaveGameManager.LocalSaveID = activeProfile;
        Debug.Log("Active profile id = " + activeProfile);
		instance.m_loadCompleted = false;
		SaveFacade.Instance.OnLoadComplete += instance.OnLoadCompleted;
        SaveFacade.Instance.Load();
	}   

    /// <summary>
    /// Load the game persistence for a specific profile into a new JSON object.
    /// </summary>
    /// <returns>The to JSON where the data is loaded, <c>null</c> if an error ocurred.</returns>
    /// <param name="_profileName">The name of the profile to be loaded.</param>	
    public static SimpleJSON.JSONClass LoadToObject(string _profileName = "") {
		// From https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
		SimpleJSON.JSONClass data = null;
		string path = GetPersistenceFilePath(_profileName);
		if(File.Exists(path)) 
		{
            //[DGR] Persistence with FGOL technology           
            SaveData _saveData = new SaveData(_profileName);
            _saveData.Load();
            string profileJSONStr = _saveData.ToString();
            data = SimpleJSON.JSON.Parse( profileJSONStr ) as SimpleJSON.JSONClass;
		} else {
			// No saved games found for the given profile, try to load the profile data
			CheckProfileName(ref _profileName);
			Debug.Log("No saved games were found, loading profile " + _profileName);

			// Load profile and use it as initial data
			data = GetDefaultDataFromProfile(_profileName);
			if(data == null) {
				Debug.Log("Profile " + _profileName + " couldn't be found, starting from 0");
			}
		}

		return data;
	}    

	/// <summary>
	/// Save the game state to persistence file. This method should be called by the game when playing to save the persistence.
	/// </summary>
	/// <param name="_upload">When <c>true</c> the persistence is sent to the server too if cloud save is enabled</param>
	public static void Save(bool _upload=false)
    {
        //[DGR] Persistence with FGOL technology
        SaveFacade.Instance.Save(null, _upload);        
	}

	/// <summary>
	/// Save a given data object to persistence file. This method should be called only by the editor.
	/// </summary>
	/// <param name="_profileName">The name of the profile to be saved.</param>
	/// <param name="_data">The data object to be saved.</param>
	public static void SaveFromObject(string _profileName, SimpleJSON.JSONClass _data) 
	{
        //[DGR] Persistence with FGOL technology
        // The file needs to be encrypted and compressed in order to make it work as it will work on production
        SaveData _saveData = new SaveData(_profileName);        
        _saveData.Merge(_data.ToString());
        _saveData.Version = SaveGameManager.Instance.Version;//HSXServer.GameSaveVersion;
        _saveData.Save();        
    }

    /// <summary>
    /// Deletes local persistence file.
    /// The game should be reloaded afterwards.
    /// </summary>
    /// <param name="_profileName">The name of the profile to be cleared.</param>
    public static void Clear(string _profileName=null) 
	{
        // If no profileName is provided then we assume that the active profile is the one that has to be deleted
        if (_profileName == null)
            _profileName = activeProfile;

        // Delete persistence file
        string path = GetPersistenceFilePath(_profileName);
		File.Delete(path);

		// Create a new save file with the default data from the profile
		SimpleJSON.JSONClass data = GetDefaultDataFromProfile(_profileName);
		if (data != null) {
			SaveFromObject(_profileName, data);
		}
	}

	//------------------------------------------------------------------//
	// OTHER AUXILIAR METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Given the name of a profile, obtain the full path of its associated persistence file.
	/// </summary>
	/// <param name="_profileName">The name of the profile whose path we want.</param>
	private static string GetPersistenceFilePath(string _profileName = "") {
		// If not defined, return active profile
		CheckProfileName(ref _profileName);
		return saveDir + "/" + _profileName + ".sav";
	}

	/// <summary>
	/// Obtain the list of current saved games.
	/// </summary>
	/// <returns>The list of saved games in the persistence dir.</returns>
	public static string[] GetSavedGamesList() {
		// C# makes it easy for us
		DirectoryInfo dirInfo = new DirectoryInfo(saveDir);
		FileInfo[] files = dirInfo.GetFiles();

		// Strip filename from full file path
		List<string> fileNames = new List<string>();
		for(int i = 0; i < files.Length; i++) 
		{
			if ( files[i].Name.EndsWith("sav") )
			{                
                fileNames.Add(Path.GetFileNameWithoutExtension(files[i].Name));
			}
		}

		return fileNames.ToArray();
	}

	/// <summary>
	/// Determines if a given profile name has a persistence file attached to it.
	/// </summary>
	/// <returns><c>true</c> if a saved game exists for the specified _profileName; otherwise, <c>false</c>.</returns>
	/// <param name="_profileName">The name of the profile we want to check.</param>
	public static bool HasSavedGame(string _profileName) {
		return File.Exists(GetPersistenceFilePath(_profileName));
	}


	/// <summary>
	/// Get the default data stored in the given profile prefab.
	/// </summary>
	/// <returns>The data from profile.</returns>
	/// <param name="_profileName">The name of the profile to be loaded.</param>
	public static SimpleJSON.JSONClass GetDefaultDataFromProfile(string _profileName = "") 
	{
        SimpleJSON.JSONClass _returnValue = null;

        // Load data from prefab
        CheckProfileName(ref _profileName);
        
        // The default profile is created from rules
        if (_profileName == PersistenceProfile.DEFAULT_PROFILE)
        {
            DefinitionNode _def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "initialSettings");
            if (_def != null)
            {
                string _sc = _def.Get("softCurrency");
                string _pc = _def.Get("hardCurrency");
                string _initialDragonSku = _def.Get("initialDragonSKU");                

                _returnValue = new SimpleJSON.JSONClass();

                // User Profile: sc, pc, currentDragon
                SimpleJSON.JSONClass _userProfile = new SimpleJSON.JSONClass();
                _userProfile.Add("sc", _sc);
                _userProfile.Add("pc", _pc);
                _userProfile.Add("currentDragon", _initialDragonSku);
                _userProfile.Add("currentLevel", "level_0");	// Only one level now
                _returnValue.Add("userProfile", _userProfile);

				// Dragons array
				SimpleJSON.JSONArray _dragons = new SimpleJSON.JSONArray();

				// Initial dragon
                SimpleJSON.JSONClass _dragon = new SimpleJSON.JSONClass();
                _dragon.Add("sku", _initialDragonSku);
                _dragon.Add("owned", "true");                
				_dragons.Add(_dragon);

                _returnValue.Add("dragons", _dragons);                
            }
        }
        else
        {
            TextAsset defaultProfile = Resources.Load<TextAsset>(PersistenceProfile.RESOURCES_FOLDER + _profileName);
            if (defaultProfile != null)
            {
                _returnValue =  SimpleJSON.JSON.Parse(defaultProfile.text) as SimpleJSON.JSONClass;
            }
        }

		return _returnValue;
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// If the given profile name is empty, replace it by the current active profile name.
	/// </summary>
	/// <param name="_profileName">The profile name to be checked.</param>
	private static void CheckProfileName(ref string _profileName) {
		if(_profileName == "") {
			_profileName = activeProfile;
		}
	}

    #region texts
    public const string TID_SOCIAL_FB_LOGIN_MAINMENU_INCENTIVIZED = "TID_SOCIAL_LOGIN_MAINMENU_INCENTIVIZED";

    public static void Texts_LocalizeIncentivizedSocial(Localizer text)
    {
        text.Localize(TID_SOCIAL_FB_LOGIN_MAINMENU_INCENTIVIZED, Rules_GetPCAmountToIncentivizeSocial() + "");
    } 
    #endregion

    #region popups    
    // This region is responsible for opening the related to persistence popups    
    private static bool Popups_IsInited { get; set; }

    private static void Popups_Init()
    {
        if (!Popups_IsInited)
        {
            Messenger.AddListener<PopupController>(EngineEvents.POPUP_CLOSED, Popups_OnPopupClosed);
            Popups_IsInited = true;
        }
    }

    private static void Popups_Destroy()
    {
        Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_CLOSED, Popups_OnPopupClosed);
    }

    private static PopupController Popups_LoadingPopup { get; set; }

    private static bool Popups_IsLoadingPopupOpen()
    {
        return Popups_LoadingPopup != null;
    }

    /// <summary>
    /// Opens a popup to make the user wait until the response of a request related to persistence is received
    /// </summary>
    public static void Popups_OpenLoadingPopup()
    {
        if (!Popups_IsLoadingPopupOpen())
        {            
            Popups_LoadingPopup = PopupManager.PopupLoading_Open();
        }
    }

    public static void Popups_CloseLoadingPopup()
    {
        if (Popups_IsLoadingPopupOpen())
        {
            Popups_LoadingPopup.Close(true);
        }
    }

    private static void Popups_OnPopupClosed(PopupController popup)
    {
        if (popup == Popups_LoadingPopup)
        {
            Popups_LoadingPopup = null;
        }
    }

    /// <summary>
    /// This popup is shown when an error happens when the user tries to enable the cloud save, but there's no connection
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/5%29Try+to+cloud+save+with+no+network    
    /// </summary>
    public static void Popups_OpenCloudEnableHasFailed(int errorCode, Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_CLOUD_OFFLINE_NAME";
        config.MessageTid = "TID_SAVE_ERROR_CLOUD_OFFLINE_DESC";
        config.MessageParams = new string[] { "" + errorCode };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// A not logged user tries to enable the cloud so she's prompted to login first and an error when loging in happens
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/6%29Cancel+login+when+clicking+on+cloud+save
    /// </summary>
    public static void Popups_OpenCloudLoginHasFailed(int errorCode, SocialFacade.Network network, Action onConfirm, Action onCancel)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_CLOUD_LOGIN_FAILED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_CLOUD_LOGIN_FAILED_DESC";
        config.MessageParams = new string[] { "" + errorCode, SocialFacade.GetLocalizedNetworkName(network) };
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when the user clicks on cloud sync icon on hud.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/9%29Sync+cloud+save
    /// </summary>
    public static void Popups_OpenCloudSync(Action onConfirm, Action onCancel)
	{
		PopupMessage.Config config = PopupMessage.GetConfig();
		config.TitleTid = "TID_SAVE_CLOUD_ACTIVE_NAME";

		int lastUploadTime = SaveFacade.Instance.lastUploadTime;        
		if (lastUploadTime > 0)
		{
			config.MessageTid = "TID_SAVE_CLOUD_ACTIVE_DESC";
			DateTime lastUpload = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			lastUpload = lastUpload.AddSeconds(lastUploadTime).ToLocalTime();            
			string lastUploadStr = lastUpload.ToString("F");
			config.MessageParams = new string[] { lastUploadStr };
		}
		else
		{
			config.MessageTid = "TID_SAVE_CLOUD_SAVE_ACTIVE_DESC";
		}

		config.ConfirmButtonTid = "TID_SAVE_CLOUD_SAVE_SYNC_NOW";
		config.CancelButtonTid = "TID_GEN_CONTINUE";
		config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
		config.OnConfirm = onConfirm;
		config.OnCancel = onCancel;
		PopupManager.PopupMessage_Open(config);        
	}

    /// <summary>
    /// This popup is shown when the user clicks on disable the cloud save on settings popup.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/10%29Disable+cloud+save
	/// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/15%29Switch+users
    /// </summary>
    public static void Popups_OpenCloudDisable(Action onConfirm, Action onCancel)
	{
		PopupMessage.Config config = PopupMessage.GetConfig();
		config.TitleTid = "TID_SAVE_WARN_CLOUD_DISABLE_NAME";
		config.MessageTid = "TID_SAVE_WARN_CLOUD_DISABLE_DESC";        
		config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
		config.OnConfirm = onConfirm;
		config.OnCancel = onCancel;
		PopupManager.PopupMessage_Open(config);        
	}

    /// <summary>
    /// This popup is shown when the user clicks on CLOUD SAVE on settings popup to enable the feature. It's used to explain how the feature works to the user
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/11%29Enable+cloud+save
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/13%29Recommend+cloud+save
    /// </summary>
    public static void Popups_OpenCloudEnable(Action onConfirm)
	{
		PopupMessage.Config config = PopupMessage.GetConfig();
		config.TitleTid = "TID_SAVE_CLOUD_ENABLED_NAME";

		string enablePlatformMessage = "UNKNOWN";

		switch (Globals.GetPlatform())
		{
		case Globals.Platform.iOS:
			enablePlatformMessage = "TID_SAVE_CLOUD_ENABLED_IOS_DESC";
			break;
		case Globals.Platform.Android:
			enablePlatformMessage = "TID_SAVE_CLOUD_ENABLED_ANDROID_DESC";
			break;          
		}

		config.MessageTid = enablePlatformMessage;
		config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(SocialManager.GetSelectedSocialNetwork()) };
		config.OnConfirm = onConfirm;        
		config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
		PopupManager.PopupEnableCloud_Open(config);
	}

    /// <summary>
    /// Opens a popup to ask the user whether or not she wants to enable the cloud save. This popup is shown after the user logs in if the server sends cloudSaveAvailable to false.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/13%29Recommend+cloud+save
    /// </summary>
    public static void Popups_RecommendCloudEnable(Action onConfirm, Action onCancel)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_PROMPT_CLOUD_ENABLE_NAME";
        config.MessageTid = "TID_SAVE_PROMPT_CLOUD_ENABLE_DESC";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(SocialManager.GetSelectedSocialNetwork()) };
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
	/// This popup is shown when the user tries to switch users: logout from an account A and tries to log in an account B
	/// https://mdc-web-tomcat17.ubisoft.org/confluence/pages/editpage.action?pageId=358118704
	/// [DGR] TO ASK FGOL
	/// </summary>
    public static void Popups_OpenCloudSwitchWarning(SocialFacade.Network network, Action onConfirm, Action onCancel)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_WARN_CLOUD_SWITCH_NAME";
        config.MessageTid = "TID_SAVE_WARN_CLOUD_SWITCH_DESC";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(network) };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when the persistence stored in cloud is corrupted. It's been taken from HSX but it shouldn't be possible because our cloud is managed by our server.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/16%29Cloud+save+corrupted
    /// </summary>    
    public static void Popups_OpenCloudSaveCorruptedError(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_CLOUD_CORRUPTED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_CLOUD_CORRUPTED_DESC";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }


    /// <summary>
    /// Popup shown when the user changed social networks. It's shown when the user is logged in a social platform and clicks on log in to a different social network.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/15%29Switch+users
    /// [DGR] FLOW: Not supported yet since only one social platform is shown simultaneously
    /// </summary>    
    public static void Popups_OpenSwitchingUserWithCloudSaveEnabled(SocialFacade.Network networkFrom, SocialFacade.Network networkTo, Action onConfirm, Action onCancel)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_WARN_CLOUD_SWITCH_NAME";
        config.MessageTid = "TID_SAVE_WARN_CLOUD_SWITCH_NETWORK_DESC";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(networkFrom), SocialFacade.GetLocalizedNetworkName(networkTo) };        
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        PopupManager.PopupMessage_Open(config);        
    }

    /// <summary>
    /// This popup is shown when the user clicks on cancel on the popup that is shown when the user switches platform social accounts (this popup lets the user know that she's about to
    /// change the progress)
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/15%29Switch+users
    /// </summary>    
    public static void Popups_OpenNoCloudSaveEnabledAnymore(SocialFacade.Network network, Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_CLOUD_DISABLED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_CLOUD_SAVE_DISABLED_DESC";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(network) };        
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);        
    }
		
	/// {DGR] TO ASK FGOL
    public static void Popups_OpenLoginErrorWrongSocialAccount(SocialFacade.Network network, Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_CLOUD_WRONG_USER_NAME";
        config.MessageTid = "TID_SAVE_ERROR_CLOUD_WRONG_USER_DESC";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(network) };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);        
    }

    /// <summary>
    /// This popup is shown when the user has logged in, but she hasn't provided us with the permission to retrieve her friends.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/14%29New+user+incomplete+login
    /// </summary>
    public static void Popups_OpenLoginIncomplete(SocialFacade.Network network, bool incentiveAlreadyGiven, int incentiveAmount, Action onConfirm, Action onCancel)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SOCIAL_LOGIN_INCOMPLETE_NAME";

        if (incentiveAlreadyGiven)
        {
            config.MessageTid = "TID_SOCIAL_LOGIN_INCOMPLETE_DESC2";
        }
        else
        {
            config.MessageTid = "TID_SOCIAL_LOGIN_INCOMPLETE_DESC";
            config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(network), "" + incentiveAmount };
        }
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when the user cancels the login process
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/7%29Cancel+login
    /// </summary>    
    public static void Popups_OpenLoginGenericError(SocialFacade.Network network, Action onConfirm)
    {        
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SOCIAL_LOGIN_FAILED_NAME";
        config.MessageTid = "TID_SOCIAL_LOGIN_FAILED_DESC";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(network) };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when the user clicks on logout button on settings popup
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/12%29Logout
    /// </summary>   
    public static void Popups_OpenLogoutWarning(SocialFacade.Network network, bool cloudSaveEnabled, Action onConfirm, Action onCancel)
	{        
		PopupMessage.Config config = PopupMessage.GetConfig();
		config.TitleTid = cloudSaveEnabled ? "TID_SAVE_WARN_CLOUD_LOGOUT_NAME" : "TID_SOCIAL_WARNING_LOGOUT_TITLE";
		config.MessageTid = cloudSaveEnabled ? "TID_SAVE_WARN_CLOUD_LOGOUT_DESC" : "TID_SOCIAL_WARNING_LOGOUT_DESC";
		config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(network) };
		config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
		config.OnConfirm = onConfirm;
		config.OnCancel = onCancel;
		PopupManager.PopupMessage_Open(config);
	}

    /// <summary>
    /// This popup has been taken from HSX but it's not supported by Dragon yet since only one network is offered to the user
    /// </summary>    
    public static void Popups_OpenLoginWhenAlreadyLoggedIn(SocialFacade.Network networkFrom, SocialFacade.Network networkTo, Action onConfirm, Action onCancel)
    {                     
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_WARN_CLOUD_SWITCH_NAME";
        config.MessageTid = "TID_SAVE_WARN_CLOUD_SWITCH_NETWORK_DESC";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(networkFrom), SocialFacade.GetLocalizedNetworkName(networkTo) };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when the access to the local save file is not authorized by the device when starting the game
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/18%29No+access+to+local+data
    /// </summary>    
    public static void Popups_OpenLocalSavePermissionErrorWhenStarting(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOAD_FAILED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_LOAD_FAILED_DESC";
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;        
        PopupManager.PopupMessage_Open(config);        
    }

    /// <summary>
    /// This popup is shown when the access to the local save file is not authorized by the device when syncing progress
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/19%29No+access+to+local+data+when+syncing
    /// </summary>        
    public static void Popups_OpenLocalSavePermissionErrorWhenSyncing(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOAD_FAILED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_LOAD_FAILED_DESC";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when the local save is corrupted when the game was going to continue locally
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/20%29Local+save+corrupted
    /// </summary>
    /// <param name="cloudEver">Whether or not the user has synced with server</param>    
    public static void Popups_OpenLoadSaveCorruptedError(bool cloudEver, Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOCAL_CORRUPTED_NAME";
        config.MessageTid = (cloudEver) ? "TID_SAVE_ERROR_LOCAL_CORRUPTED_OFFLINE_DESC" : "TID_SAVE_ERROR_LOCAL_CORRUPTED_DESC";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when the local save is corrupted but the cloud save is ok when syncing with the cloud
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/20%29Local+save+corrupted
    /// </summary>    
    public static void Popups_OpenLocalSaveCorruptedError(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOCAL_CORRUPTED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_LOCAL_CORRUPTED_CLOUD_SAVE_DESC";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when the version of cloud save is more recent than the one in local save and local save is corrupted and no game update is available.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/22%29Version+of+cloud+save+more+recent+than+the+one+in+local+save+and+local+save+corrupted
    /// </summary>    
    public static void Popups_OpenUpdateToSolveLocalSaveCorrupted(bool updateAvailable, Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOCAL_CORRUPTED_NAME";
        config.MessageTid = (updateAvailable) ? "TID_SAVE_ERROR_LOCAL_CORRUPTED_UPDATE_DESC1" : "TID_SAVE_ERROR_LOCAL_CORRUPTED_UPDATE_DESC2";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when the game doesn't have access to the cloud server. This is a legacy code from HSX since we'll always have access to the cloud server because it's our server
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/24%29No+access+to+cloud
    /// </summary>    
    public static void Popups_OpenLoadSaveInaccessibleError(Action onConfirm, Action onCancel, Action onExtra)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_CLOUD_INACCESSIBLE_NAME";
        config.MessageTid = "TID_SAVE_ERROR_CLOUD_INACCESSIBLE_DESC";
        config.ConfirmButtonTid = "TID_GEN_CONTINUE";
        config.CancelButtonTid = "TID_GEN_RETRY";
        config.ExtraButtonTid = "TID_GEN_UPLOAD";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndExtraAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        config.OnExtra = onExtra;
        PopupManager.PopupMessage_Open(config);       
    }

    /// <summary>
    /// This popup is shown when the game is not allowed to save locally.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/25%29Can%27t+save+locally
    /// </summary>    
    public static void Popups_OpenLocalSaveCantSaveError(Action onConfirm)
    {
        string platformErrorMessage = "UNKNOWN";

        switch (Globals.GetPlatform())
        {
            case Globals.Platform.iOS:
                platformErrorMessage = "TID_SAVE_ERROR_FAILED_DESC";
                break;
            case Globals.Platform.Android:
                platformErrorMessage = "TID_SAVE_ERROR_SAVE_FAILED_ANDROID";
                break;            
        }

        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_FAILED_NAME";                
        config.MessageTid = platformErrorMessage;
        config.ConfirmButtonTid = "TID_GEN_CONTINUE";
        config.CancelButtonTid = "TID_GEN_RETRY";
        config.ExtraButtonTid = "TID_GEN_UPLOAD";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;        
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when starting the game if there's no free disk space to store the local save
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/27%29No+disk+space
    /// </summary>    
    public static void Popups_OpenLocalSaveDiskOutOfSpaceError(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_DISABLED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_DISABLED_SPACE_DESC";
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when starting the game if there's no access to disk to store the local save.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/28%29No+disk+access
    /// </summary>    
    public static void Popups_OpenLocalSaveDiskNoAccessError(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_DISABLED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_DISABLED_ACCESS_DESC";
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when both local save and cloud save are corrupted when syncing
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/23%29Local+save+corrupted+and+cloud+save+corrupted
    /// </summary>    
    public static void Popups_OpenLoadSaveBothCorruptedError(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_BOTH_SAVE_CORRUPTED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_BOTH_SAVE_CORRUPTED_DESC";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when the version used in the cloud save is more recent than the one used in the local save, which means that the user should update the game if there's a new version
    /// available.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/21%29Version+of+cloud+save+more+recent+than+the+one+in+local+save
    /// </summary>
    /// <param name="updateAvailable">Whether or not a game update is available</param>    
    public static void Popups_OpenUpdateToSolveCloudSaveCorrupted(bool updateAvailable, Action onConfirm, Action onCancel)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_UPDATE_TITLE";
        config.MessageTid = (updateAvailable) ? "TID_SAVE_ERROR_UPDATE_DESC1" : "TID_SAVE_ERROR_UPDATE_DESC2";
        config.OnConfirm = onConfirm;
        if (updateAvailable)
        {
            config.ConfirmButtonTid = "TID_GEN_UPDATE";
            config.CancelButtonTid = "TID_GEN_CONTINUE";
            config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
            config.OnCancel = onCancel;
        }
        else
        {
            config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        }

        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when the game realizes that there's game update available in the app store after receiving the response for an auth request
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/26%29Game+update+available
    /// </summary>   
    public static void Popup_OpenPromptUpdate(Action onConfirm, Action onCancel)
    {
        string platformUpdateMessage = "UNKNOWN";

        switch (Globals.GetPlatform())
        {
            case Globals.Platform.iOS:
                platformUpdateMessage = "TID_SAVE_PROMPT_UPDATE_IOS_DESC";
                break;
            case Globals.Platform.Android:
                platformUpdateMessage = "TID_SAVE_PROMPT_UPDATE_ANDROID_DESC";
                break;
            case Globals.Platform.Amazon:
                platformUpdateMessage = "STRING_SAVE_POPUP_PROMPT_UPDATE_TEXT_AMAZON";
                break;
        }

        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_PROMPT_UPDATE_NAME";
        config.MessageTid = platformUpdateMessage;
        config.ConfirmButtonTid = "TID_GEN_UPDATE";
        config.CancelButtonTid = "TID_GEN_CONTINUE";
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        PopupManager.PopupMessage_Open(config);                                
    }    

    /// <summary>
    /// This popup is shown only after the first time the user logs in facebook with the friend list permission granted
    /// </summary>
    /// <param name="rewardAmount"></param>
    /// <param name="onConfirm"></param>
    public static void Popups_OpenLoginComplete(int rewardAmount, Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SOCIAL_LOGIN_COMPLETE_NAME";
        config.MessageTid = "TID_SOCIAL_LOGIN_COMPLETE_DESC";
        config.MessageParams = new string[] { rewardAmount + "" };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);        
    }    

    public static void Popups_OpenPublishPermissionFailed(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SOCIAL_ERROR_SHAREPERMISSION_NAME";
        config.MessageTid = "TID_SOCIAL_ERROR_SHAREPERMISSION_DESC";        
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);       
    }

    /// <summary>
    /// The user is prompted with this popup so she can choose the persistence to keep when there's a conflict between the progress stored in local and the one stored in the cloud
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/29%29Sync+Conflict
    /// </summary>
    public static void Popups_OpenMerge(ConflictState conflictState, ProgressComparatorSystem local, ProgressComparatorSystem cloud, bool dismissable, Action<ConflictResult> onResolve)
    {
        PopupController pc = PopupManager.OpenPopupInstant(PopupMerge.PATH);
        PopupMerge pm = pc.GetComponent<PopupMerge>();
        if (pm != null)
        {
            pm.Setup(conflictState, local, cloud, dismissable, onResolve);
        }
    }

    /// <summary>
    /// This popup is shown when the user doesn't choose the recommended option in sync conflict popup.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/29%29Sync+Conflict
    /// </summary>    
    public static void Popups_OpenMergeConfirmation(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_WARN_CLOUD_WRONG_CHOICE_NAME";
        config.MessageTid = "TID_SAVE_WARN_CLOUD_WRONG_CHOICE_DESC";                
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when there's no internet connection
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/29%29No+internet+connection
    /// </summary>    
    public static void Popups_OpenErrorConnection(SocialFacade.Network network, Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SOCIAL_ERROR_CONNECTION_NAME";
        config.MessageTid = "TID_SOCIAL_ERROR_CONNECTION_DESC";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(network) };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);             
    }  		    

    public static void Popups_OpenMessage(PopupMessage.Config config)
    {
        PopupManager.PopupMessage_Open(config);
    }   
    #endregion

    #region rules
    // This region is responsible for giving access to rules related to persistence/social networks

    /// <summary>
    /// Returns the amount of PC the user will receive if she logs in a social network (she actually has to grant the friends list permission to get the reward)
    /// </summary>
    /// <returns></returns>
    public static int Rules_GetPCAmountToIncentivizeSocial()
    {
        int returnValue = 0;
        DefinitionNode _def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
        if (_def != null)
        {
            returnValue = _def.GetAsInt("incentivizeFBGem");
        }

        return returnValue;
    }
#endregion

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// SaveFacade's load has been completed.
	/// </summary>
	public void OnLoadCompleted() {
		// Unsubscribe from the event
		SaveFacade.Instance.OnLoadComplete -= OnLoadCompleted;

		// Initialize managers needing data from the loaded profile
		GlobalEventManager.SetupUser(UsersManager.currentUser);

		// Change flag
		instance.m_loadCompleted = true;
	}
}

