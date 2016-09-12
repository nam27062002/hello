// PersistenceManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using FGOL.Save;
using FGOL.Save.SaveStates;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;


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
public static class PersistenceManager {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	const string TAG = "PersistenceManager";

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
        EggManager.SetupUser(UsersManager.currentUser);
        MissionManager.SetupUser(UsersManager.currentUser);

        //[DGR] FGOL SaveFacade is used to load the persistence
        // Makes sure Local ID is pointing to the active profile
        SaveGameManager.LocalSaveID = activeProfile;
        Debug.Log("Active profile id = " + activeProfile);
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
    public static void Clear(string _profileName = "") 
	{
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
		// Load data from prefab
		CheckProfileName(ref _profileName);
		TextAsset defaultProfile = Resources.Load<TextAsset>(PersistenceProfile.RESOURCES_FOLDER + _profileName);
		if(defaultProfile != null) 
		{
			return SimpleJSON.JSON.Parse( defaultProfile.text ) as SimpleJSON.JSONClass;
		}

		return null;
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
    public const string TID_SOCIAL_FB_LOGIN_MAINMENU_INCENTIVIZED = "STRING_SOCIAL_FB_LOGIN_MAINMENU_INCENTIVIZED";

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
            PopupMessage.Config config = PopupMessage.GetConfig();
            config.TitleTid = "STRING_CLOUD_LOADING_TITLE";
            config.MessageTid = "STRING_CLOUD_LOADING_WAIT";
            Popups_LoadingPopup = PopupManager.PopupMessage_Open(config);
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
    /// Opens a popup to ask the user whether or not she wants to enable the cloud save
    /// </summary>
    public static void Popups_OpenEnableCloudSavePopup(Action onConfirm, Action onCancel)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_PROMPT_CLOUD_ENABLE_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_PROMPT_CLOUD_ENABLE_TEXT_SOCIAL_SN";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(SocialManager.GetSelectedSocialNetwork()) };
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popups_OpenDisableCloudSavePopup(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_CLOUD_ENABLED_TITLE";

        string enablePlatformMessage = "UNKNOWN";

        switch (Globals.GetPlatform())
        {
            case Globals.Platform.iOS:
                enablePlatformMessage = "STRING_SAVE_POPUP_CLOUD_ENABLED_TEXT_IOS";
                break;
            case Globals.Platform.Android:
                enablePlatformMessage = "STRING_SAVE_POPUP_CLOUD_ENABLED_TEXT_ANDROID";
                break;
            case Globals.Platform.Amazon:
                enablePlatformMessage = "STRING_SAVE_POPUP_CLOUD_ENABLED_TEXT_AMAZON";
                break;
        }

        config.MessageTid = enablePlatformMessage;
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(SocialManager.GetSelectedSocialNetwork()) };
        config.OnConfirm = onConfirm;        
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        PopupManager.PopupMessage_Open(config);
    }
    
    public static void Popups_OpenSwitchingUserWithCloudSaveEnabled(SocialFacade.Network networkFrom, SocialFacade.Network networkTo, Action onConfirm, Action onCancel)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_WARN_CLOUD_SWITCH_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_WARN_CLOUD_SWITCH_NETWORK_TEXT";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(networkFrom), SocialFacade.GetLocalizedNetworkName(networkTo) };        
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnConfirm = onCancel;
        PopupManager.PopupMessage_Open(config);        
    }

    public static void Popups_OpenNoCloudSaveEnabledAnymore(SocialFacade.Network network, Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_ERROR_CLOUD_SAVE_DISABLED_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_ERROR_CLOUD_SAVE_DISABLED_TEXT_SN";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(network) };        
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);        
    }

    public static void Popups_OpenLoginErrorWrongSocialAccount(SocialFacade.Network network, Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SOCIAL_ERROR_WRONG_ACCOUNT";
        config.MessageTid = "STRING_SOCIAL_ERROR_WRONG_ACCOUNT_FB_SN";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(network) };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);        
    }

    /// <summary>
    /// This popup is shown when the user has logged in, but she hasn't provided us with the permission to retrieve her friends.
    /// </summary>
    public static void Popups_OpenLoginIncomplete(SocialFacade.Network network, bool incentiveAlreadyGiven, int incentiveAmount, Action onConfirm, Action onCancel)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SOCIAL_LOGIN_INCOMPLETE_TITLE";

        if (incentiveAlreadyGiven)
        {
            config.MessageTid = "STRING_SOCIAL_LOGIN_INCOMPLETE_FB2";
        }
        else
        {
            config.MessageTid = "STRING_SOCIAL_LOGIN_INCOMPLETE_FB_SN";
            config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(network), "" + incentiveAmount };
        }
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popups_OpenLoginGenericError(SocialFacade.Network network, Action onConfirm)
    {        
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SOCIAL_LOGIN_FAILED";
        config.MessageTid = "STRING_SOCIAL_LOGIN_FAILED_FB_SN";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(network) };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popups_OpenLoginWhenAlreadyLoggedIn(SocialFacade.Network networkFrom, SocialFacade.Network networkTo, Action onConfirm, Action onCancel)
    {                     
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_WARN_CLOUD_SWITCH_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_WARN_CLOUD_SWITCH_NETWORK_TEXT";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(networkFrom), SocialFacade.GetLocalizedNetworkName(networkTo) };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popups_OpenLogoutWarning(SocialFacade.Network network, bool cloudSaveEnabled, Action onConfirm, Action onCancel)
    {        
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = cloudSaveEnabled ? "STRING_SAVE_POPUP_WARN_CLOUD_LOGOUT_TITLE" : "STRING_SOCIAL_WARNING_LOGOUT";
        config.MessageTid = cloudSaveEnabled ? "STRING_SAVE_POPUP_WARN_CLOUD_LOGOUT_TEXT_SN" : "STRING_SOCIAL_WARNING_LOGOUT_FB_SN";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(network) };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popups_OpenCloudSwitchWarning(SocialFacade.Network network, Action onConfirm, Action onCancel)
    {        
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_WARN_CLOUD_SWITCH_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_WARN_CLOUD_SWITCH_TEXT_SN";        
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(network) };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when the user clicks to disable the cloud save on settings popup.
    /// </summary>
    public static void Popups_OpenCloudDisable(Action onConfirm, Action onCancel)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_WARN_CLOUD_DISABLE_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_WARN_CLOUD_DISABLE_TEXT";        
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        PopupManager.PopupMessage_Open(config);        
    }

    public static void Popups_OpenErrorLoadFailed(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_ERROR_LOAD_FAILED_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_ERROR_LOAD_FAILED_TEXT";
        config.ConfirmButtonTid = "STRING_BUTTON_RETRY";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;        
        PopupManager.PopupMessage_Open(config);        
    }

    public static void Popups_OpenLoadSavePermissionError(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_ERROR_LOAD_FAILED_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_ERROR_LOAD_FAILED_TEXT";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popups_OpenLoadSaveBothCorruptedError(Action onConfirm)
    {        
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_ERROR_BOTH_SAVE_CORRUPTED_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_ERROR_BOTH_SAVE_CORRUPTED_TEXT";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popups_OpenLoadSaveCorruptedError(bool cloudEver, Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_ERROR_LOCAL_SAVE_CORRUPTED_TITLE";
        config.MessageTid = (cloudEver) ? "STRING_SAVE_POPUP_ERROR_LOCAL_SAVE_CORRUPTED_TEXT_OFFLINE" : "STRING_SAVE_POPUP_ERROR_LOCAL_SAVE_CORRUPTED_TEXT";        
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popups_OpenCloudSaveCorruptedError(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_ERROR_CLOUD_SAVE_CORRUPTED_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_ERROR_CLOUD_SAVE_CORRUPTED_TEXT";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popups_OpenLocalSaveCorruptedError(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_ERROR_LOCAL_SAVE_CORRUPTED_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_ERROR_LOCAL_SAVE_CORRUPTED_TEXT_CLOUD";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);        
    }

    public static void Popups_OpenLoadSaveInaccessibleError(Action onConfirm, Action onCancel, Action onExtra)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_ERROR_CLOUD_INACCESSIBLE_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_ERROR_CLOUD_INACCESSIBLE_TEXT";
        config.ConfirmButtonTid = "STRING_BUTTON_CONTINUE";
        config.CancelButtonTid = "STRING_BUTTON_RETRY";
        config.ExtraButtonTid = "STRING_BUTTON_UPLOAD";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndExtraAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        config.OnExtra = onExtra;
        PopupManager.PopupMessage_Open(config);       
    }

    public static void Popups_OpenLocalSaveGenericError(Action onConfirm)
    {
        string platformErrorMessage = "UNKNOWN";

        switch (Globals.GetPlatform())
        {
            case Globals.Platform.iOS:
                platformErrorMessage = "STRING_SAVE_POPUP_ERROR_SAVE_FAILED_TEXT_IOS";
                break;
            case Globals.Platform.Android:
                platformErrorMessage = "STRING_SAVE_POPUP_ERROR_SAVE_FAILED_TEXT_ANDROID";
                break;            
        }

        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_ERROR_SAVE_FAILED_TITLE";                
        config.MessageTid = platformErrorMessage;
        config.ConfirmButtonTid = "STRING_BUTTON_CONTINUE";
        config.CancelButtonTid = "STRING_BUTTON_RETRY";
        config.ExtraButtonTid = "STRING_BUTTON_UPLOAD";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;        
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popups_OpenSaveDiskOutOfSpaceError(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_ERROR_SAVE_DISABLED_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_ERROR_SAVE_DISABLED_TEXT_SPACE";
        config.ConfirmButtonTid = "STRING_BUTTON_RETRY";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popups_OpenSaveDiskNoAccessError(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_ERROR_SAVE_DISABLED_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_ERROR_SAVE_DISABLED_TEXT_ACCESS";
        config.ConfirmButtonTid = "STRING_BUTTON_RETRY";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popups_OpenLoginComplete(int rewardAmount, Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SOCIAL_LOGIN_COMPLETE_TITLE";
        config.MessageTid = "STRING_SOCIAL_LOGIN_COMPLETE_FB";
        config.MessageParams = new string[] { rewardAmount + "" };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);        
    }    

    public static void Popups_OpenPublishPermissionFailed(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_FACEBOOKPOST_ERROR_SHAREPERMISSION_TITLE";
        config.MessageTid = "STRING_FACEBOOKPOST_ERROR_SHAREPERMISSION";        
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);       
    }

    /// <summary>
    /// This popup is shown when an error happens when the user tries to enable the cloud save.
    /// </summary>
    public static void Popups_OpenEnableCloudFailed(int errorCode, Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_ERROR_CLOUD_OFFLINE_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_ERROR_CLOUD_OFFLINE_TEXT";
        config.MessageParams = new string[] { "" + errorCode };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when an error happens when the user tries to enable the cloud save.
    /// </summary>
    public static void Popups_OpenLoginToCloudFailed(int errorCode, SocialFacade.Network network, Action onConfirm, Action onCancel)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_ERROR_CLOUD_OFFLINE_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_ERROR_CLOUD_OFFLINE_TEXT";
        config.MessageParams = new string[] { "" + errorCode, SocialFacade.GetLocalizedNetworkName(network) };
        config.ConfirmButtonTid = "STRING_BUTTON_RETRY";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// The user is prompted with this popup so she can choose the persistence to keep when there's a conflict between the progress stored in local and the one stored in the cloud
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

    public static void Popups_OpenMergeConfirmation(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_WARN_CLOUD_WRONG_CHOICE_TITLE";
        config.MessageTid = "STRING_SAVE_POPUP_WARN_CLOUD_WRONG_CHOICE_TEXT";                
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }  
    
    public static void Popups_OpenErrorConnection(SocialFacade.Network network, Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SOCIAL_ERROR_CONNECTION_TITLE";
        config.MessageTid = "STRING_SOCIAL_ERROR_CONNECTION_INFO_SN";
        config.MessageParams = new string[] { SocialFacade.GetLocalizedNetworkName(network) };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);             
    }  

    public static void Popups_OpenCloudSync(Action onConfirm, Action onCancel)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "STRING_SAVE_POPUP_CLOUD_SAVE_ACTIVE_TITLE";
        
        int lastUploadTime = SaveFacade.Instance.lastUploadTime;        
        if (lastUploadTime > 0)
        {
            config.MessageTid = "STRING_SAVE_POPUP_CLOUD_SAVE_ACTIVE_TEXT1";
            DateTime lastUpload = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            lastUpload = lastUpload.AddSeconds(lastUploadTime).ToLocalTime();            
            string lastUploadStr = lastUpload.ToString("F");
            config.MessageParams = new string[] { lastUploadStr };
        }
        else
        {
            config.MessageTid = "STRING_SAVE_POPUP_CLOUD_SAVE_ACTIVE_TEXT2";
        }

        config.ConfirmButtonTid = "STRING_SAVE_POPUP_CLOUD_SAVE_SYNC_NOW";
        config.CancelButtonTid = "STRING_BUTTON_CONTINUE";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnConfirm = onCancel;
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
        // [DGR] RULES: To read from content
        return 25;
    }
#endregion
}

