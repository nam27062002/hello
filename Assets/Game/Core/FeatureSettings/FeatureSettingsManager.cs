// FeatureSettingsManager.cs
// Hungry Dragon
// 
// Created by David Germade
// Copyright (c) 2017 Ubisoft. All rights reserved.

#define FREQFORMULA
//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using FGOL.Server;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// This class is used as a facade that returns the configuration for all features of the aookucatuib
/// This class uses <c>DeviceQualityManager</c> to retrieve the configuration of these features taking into consideration the quality of the device
/// </summary>
public class FeatureSettingsManager : UbiBCN.SingletonMonoBehaviour<FeatureSettingsManager>
{
    private static bool sm_initialized = false;

    /// <summary>
    /// Initializes static stuff. It's not done with a static constructor because UnityEngine.Debug.isDebugBuild can't be called from a constructor
    /// </summary>
    private static void Static_Initialize()
    {
        if (!sm_initialized)
        {
            // It's stored in a variable because UnityEngine.Debug.isDebugBuild must be called from the main thread and DownloadablesDownloader, which is executed in its own thread
            // needs to check this variable
            sm_isDebugBuild = UnityEngine.Debug.isDebugBuild;

			// Cheats
			sm_areCheatsEnabled = false;
			if(Application.isPlaying) {
				Calety.Server.ServerConfig kServerConfig = ServerManager.SharedInstance.GetServerConfig();
				if(kServerConfig != null) {
					sm_areCheatsEnabled = kServerConfig.m_eBuildEnvironment != CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION;
				}
            }
#if UNITY_EDITOR            
            else
            {
                sm_areCheatsEnabled = true;
            }
#endif
            sm_initialized = true;
        }
    }    

    private DeviceQualityManager m_deviceQualityManager;

    public static DeviceQualityManager deviceQualityManager
    {
        get
        {
            return instance.m_deviceQualityManager;
        }
    }

#if FREQFORMULA
    //    private static string m_qualityFormulaVersion = "2.0";
    //  After fix the mistake in freqformula the version changes to 2.5
    //    private static string m_qualityFormulaVersion = "2.5";
    private static string m_qualityFormulaVersion = "3.0";
#else
    private static string m_qualityFormulaVersion = "1.0";
#endif
    public static string QualityFormulaVersion
    {
        get
        {
            return m_qualityFormulaVersion;
        }
    }

    private bool m_isReady;


	private static Resolution m_nativeResolution = new Resolution {
		width = Screen.width,
		height = Screen.height,
		refreshRate = 0
	};

	public static Resolution NativeResolution {
		get {
			return m_nativeResolution;
		}	
	}

    public static int m_OriginalScreenWidth = Screen.width;
    public static int m_OriginalScreenHeight = Screen.height;
    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Inititialization.
    /// </summary>
    private void Awake()
    {
#if UNITY_EDITOR
        Device_Model = "UNITY_EDITOR";
#else
        Device_Model = SystemInfo.deviceModel;
#endif

        //        Device_Model = SystemInfo.deviceModel;    //Allows same behaviour as device in UNITY_EDITOR 

        m_OriginalScreenWidth = Screen.width;
        m_OriginalScreenHeight = Screen.height;

		m_nativeResolution.width = m_OriginalScreenWidth;
		m_nativeResolution.height = m_OriginalScreenHeight;
		m_nativeResolution.refreshRate = Screen.currentResolution.refreshRate;

        Server_Reset();

        CurrentQualityIndex = -1;
        Shaders_CurrentKey = null;
        State = EState.WaitingForRules;

        m_deviceQualityManager = new DeviceQualityManager();

        m_isReady = false;

        InitDefaultValues();
        UpdateRules();
    }

    private void InitDefaultValues()
    {
        // Exceptionally some features need an early access (before rules are loaded, typically features related to content such as whether or not content deltas should be used).
        // For those features we need to use the server settings cached if they exist
        Device_CurrentFeatureSettings = CreateFeatureSettings();

        // Some Quality settings received from server in a previous session might be stored in cache
        JSONNode serverQualitySettingsJSON = null;
        if (CacheServerManager.SharedInstance.HasKey(CACHE_SERVER_QUALITY_KEY))
        {
            string value = CacheServerManager.SharedInstance.GetVariable(CACHE_SERVER_QUALITY_KEY);
            serverQualitySettingsJSON = JSON.Parse(value);

            if (IsDebugEnabled)
            {
                Log("Quality settings from server cached : " + ((serverQualitySettingsJSON == null) ? "null" : serverQualitySettingsJSON.ToString()));
            }
        }

        // Some game settings received from server in a previous session might be stored in cache
        JSONNode serverGameSettingsJSON = null;
        if (CacheServerManager.SharedInstance.HasKey(CACHE_SERVER_GAME_KEY))
        {
            string value = CacheServerManager.SharedInstance.GetVariable(CACHE_SERVER_GAME_KEY);
            serverGameSettingsJSON = JSON.Parse(value);

            if (IsDebugEnabled)
            {
                Log("Game settings from server cached : " + ((serverGameSettingsJSON == null) ? "null" : serverGameSettingsJSON.ToString()));
            }
        }

        // The configuration received for the device from server has a bigger priority than the one retrieved from rules
        if (serverQualitySettingsJSON != null)
        {
            serverQualitySettingsJSON = FormatJSON(serverQualitySettingsJSON);
            Device_CurrentFeatureSettings.OverrideFromJSON(serverQualitySettingsJSON);
        }

        // The configuration received for the game from server has the biggest priority so it's the last one to be applied so it can override the values set by the other two configurations
        if (serverGameSettingsJSON != null)
        {
            serverGameSettingsJSON = FormatJSON(serverGameSettingsJSON);
            Device_CurrentFeatureSettings.OverrideFromJSON(serverGameSettingsJSON);
        }

        IsFogOnDemandEnabled = !IsDebugEnabled;

        //Starts fps count system
        StartFPS();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (m_deviceQualityManager != null)
        {
            m_deviceQualityManager.Clear();
        }
    }

    private void UpdateRules() {
        if (ContentManager.ready)
        {
            Rules_OnLoaded();
            State_Timer = 0f;

            State = EState.WaitingToCheckConnection;
        }
    }

    private void Update()
    {
        switch (State)
        {
            case EState.WaitingForRules:
                UpdateRules();
                break;

            case EState.WaitingToCheckConnection:
                State_Timer -= Time.deltaTime;
                if (State_Timer <= 0)
                {
                    State = EState.CheckingConnection;

                    // Check if there's connection
                    GameServerManager.SharedInstance.CheckConnection(delegate (Error connectionError)
                    {
                        if (connectionError == null)
                        {
                            // If there's connection then the request is sent to the server
                            Server_RequestSettings();
                        }
                        else
                        {
                            // Wait to check the connection again
                            State_Timer = SERVER_TIME_TO_WAIT_BETWEEN_CONNECTION_CHECKS;
                            State = EState.WaitingToCheckConnection;
                        }
                    });
                }
                break;

            case EState.WaitingForServerResponse:
                if (!Server_WaitingForQualitySettings && !Server_WaitingForGameSettings)
                {
                    State = EState.Done;
                }
                break;
        }

        // Checks if the client has been told to upload the settings calculated by the client. It's not been implemented as a state because we want this upload
        // to happen as soon as possible (when the user logs in), otherwise we would have to synchronize it with the other states
        if (Server_TimeToWaitToUploadSettings > 0f)
        {
            if (GameSessionManager.SharedInstance.IsLogged())
            {
                Server_UploadQualitySettings();
            }
            else
            {
                // If the time given to wait for the user to log in has expired then we just ignore that request from server
                Server_TimeToWaitToUploadSettings -= Time.deltaTime;
                if (Server_TimeToWaitToUploadSettings <= 0f)
                {
                    Server_ResetUploadQualitySettings();
                }
            }
        }

        //Update fps count system
        UpdateFPS();

    }

    public static bool IsReady()
    {
        return isInstanceCreated && instance.m_isReady;
    }

    #region state
    // This region is responsible for handling the different states of this singleton. This is the flow:
    // 1)The singleton has to wait for the rules to be loaded
    // 2)Once they're loaded the quality settings information defined in content is applied and the singleton has to check whether or not there's connection, when there's connection a request for the latest
    //   quality setting defined on server side is sent
    // 3)The singleton has to wait for the response from server
    // 4)Once the response is received the singleton has to wait for an appropriate moment (not ingame) to apply the new quality settings.
    // 5)Once the quality settings information from server are applied then no more changes will happen on quality settings so the flow is done

    private enum EState
    {
        WaitingForRules,                        // Waiting for rules to be loaded
        WaitingToCheckConnection,               // Time to wait before checking if there's connection
        CheckingConnection,                     // Checks whether or not there's connection        
        WaitingForServerResponse,               // Waiting for the response with the quality settings information from server               
        Done                                    // No more stuff has to be done once server quality settings have been applied
    }

    private EState State { get; set; }

    /// <summary>
    /// General purpose timer to use by the current state
    /// </summary>
    private float State_Timer { get; set; }
    #endregion

    #region server
    // This region is responsible for sending related to quality settings request to the server and for receiving the responses
    // The response for the quality settings request can be null if the server doesn't have that information. If so then the client has to provide the server with that information.
    // In order to be allowed to do so the user has to be logged. 

    /// <summary>
    /// Key for cache server manager
    /// </summary>
    private string CACHE_SERVER_QUALITY_KEY = "fSettings_data";
    private string CACHE_SERVER_GAME_KEY = "gameSettings_data";

    private float SERVER_TIME_TO_WAIT_BETWEEN_CONNECTION_CHECKS = 15f;

    /// <summary>
    /// Time in seconds to wait for the user to log in once the client has been told to upload its quality settings information to the server
    /// </summary>
    private float SERVER_TIME_TO_WAIT_FOR_LOGIN_WHEN_TOLD_TO_UPLOAD = 90f;

    private float Server_TimeToWaitToUploadSettings { get; set; }

    private bool Server_WaitingForQualitySettings { get; set; }

    private bool Server_WaitingForGameSettings { get; set; }

    private void Server_Reset()
    {
        Server_ResetUploadQualitySettings();
        Server_WaitingForQualitySettings = false;
        Server_WaitingForGameSettings = false;
    }

    private void Server_RequestSettings()
    {
        Server_Reset();

        // We need to wait for the response
        State = EState.WaitingForServerResponse;

        Server_RequestQualitySettings();
        Server_RequestGameSettings();
    }

    private void Server_RequestQualitySettings()
    {
        Server_WaitingForQualitySettings = true;

        GameServerManager.SharedInstance.GetQualitySettings(
        (Error error, GameServerManager.ServerResponse response) =>
        {
            if (error == null)
            {
                // Quality settings received from server is not applied during this run because it can be expensive or tool late to be applied. 
                // Instead it's stored in cache so it can be applied next time the user launches the application
                string qualitySettings = response["response"] as string;
                if (qualitySettings != null)
                {
                    CacheServerManager.SharedInstance.SetVariable(CACHE_SERVER_QUALITY_KEY, qualitySettings);
                }

                // If there's no data then we need to upload the profile calculated to the server when as long as the user logs in
                string key = "upLoadRequired";
                if (response.ContainsKey(key) && (bool)response[key])
                {
                    Server_PrepareUploadQualitySettings();
                }
            }
            else
            {
                DeviceQualityManager.LogError("get quality settings response error " + error.message);
            }

            Server_WaitingForQualitySettings = false;
        });
    }

    private void Server_RequestGameSettings()
    {
        Server_WaitingForGameSettings = true;

        GameServerManager.SharedInstance.GetGameSettings(
        (Error error, GameServerManager.ServerResponse response) =>
        {
            if (error == null)
            {
                // Game settings received from server is not applied during this run because it can be expensive or tool late to be applied. 
                // Instead it's stored in cache so it can be applied next time the user launches the application
                string gameSettings = response["response"] as string;
                if (gameSettings != null)
                {
                    CacheServerManager.SharedInstance.SetVariable(CACHE_SERVER_GAME_KEY, gameSettings);
                }
            }
            else
            {
                DeviceQualityManager.LogError("get game settings response error " + error.message);
            }

            Server_WaitingForGameSettings = false;
        });
    }

    private void Server_ResetUploadQualitySettings()
    {
        Server_TimeToWaitToUploadSettings = 0f;
    }

    private void Server_PrepareUploadQualitySettings()
    {
        // Sets max time to wait to upload settings
        Server_TimeToWaitToUploadSettings = SERVER_TIME_TO_WAIT_FOR_LOGIN_WHEN_TOLD_TO_UPLOAD;
    }

    private void Server_UploadQualitySettings()
    {
        string profileName = Device_CalculatedProfile;

        if (IsDebugEnabled)
            Log("Server_UploadQualitySettings profileName = " + profileName);

        if (!string.IsNullOrEmpty(profileName))
        {
            // The calculated profile is the one that is sent to the server since the current profile could be different to the calculated profile if the client has applied
            // some settings received from server that were stored in the device cache in a previous session
            JSONNode json = new JSONClass();
            //profileName = "high";
            json.Add("profile", profileName);
            GameServerManager.SharedInstance.SetQualitySettings(json.ToString(),
                (Error error, GameServerManager.ServerResponse response) => {
                    if (error == null) {
                        Log("Quality settings uploaded successfully");
                    } else {
                        Log("Error when uploading quality settings " + error.message);
                    }
                });
        }

        Server_ResetUploadQualitySettings();
    }

    #endregion

    #region device
    public int Device_GetSystemMemorySize()
    {
        return SystemInfo.systemMemorySize;
    }

    public int Device_GetGraphicsMemorySize()
    {
        return SystemInfo.graphicsMemorySize;
    }

    public int Device_GetProcessorFrequency()
    {
        return SystemInfo.processorFrequency;
    }

    public string Device_Model { get; set; }

    /// <summary>
    /// Returns whether or not the device is supported by the app. IMPORTANT: Rules have to be loaded to rely on this method
    /// </summary>
    /// <returns></returns>
    public bool Device_IsSupported()
    {
        int minMemory = m_deviceQualityManager.Profiles_GetMinMemoryRequired();

        // Makes sure that the device has enough memory to run the game (min memory required is read from rules so this method should be called after rules are loaded)
        return Device_GetSystemMemorySize() >= minMemory;
    }

    public bool Device_SupportedWarning()
    {
        bool ret = false;
#if UNITY_IOS
		ret = UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPad3Gen;
#endif
        return ret;
    }

    public float Device_CalculateRating()
    {
        //Average the devices RAM, CPU and GPU details to give a rating betwen 0 and 1
        float finalDeviceRating = 0.0f;

        int processorCount = SystemInfo.processorCount;
        int graphicsMemorySize = Device_GetGraphicsMemorySize();
        int cpuFreq = SystemInfo.processorFrequency;

        Dictionary<string, DefinitionNode> definitions = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.DEVICE_RATING_SETTINGS);
        List<DeviceSettings> cpuCoresData = new List<DeviceSettings>();
        List<DeviceSettings> gpuMemoryData = new List<DeviceSettings>();
        List<DeviceSettings> cpuFreqData = new List<DeviceSettings>();

        DeviceSettings data;
        string key;
        foreach (KeyValuePair<string, DefinitionNode> pair in definitions)
        {
            key = "cpuCoresRating";
            if (pair.Value.Has(key))
            {
                data = new DeviceSettings(pair.Value.GetAsFloat(key), pair.Value.GetAsFloat("cpuCoresBoundary"));
                cpuCoresData.Add(data);
            }

            key = "gpuMemoryRating";
            if (pair.Value.Has(key))
            {
                data = new DeviceSettings(pair.Value.GetAsFloat(key), pair.Value.GetAsFloat("gpuMemoryBoundary"));
                gpuMemoryData.Add(data);
            }

            key = "cpuFreqRating";
            if (pair.Value.Has(key))
            {
                data = new DeviceSettings(pair.Value.GetAsFloat(key), pair.Value.GetAsFloat("cpuFreqBoundary"));
                cpuFreqData.Add(data);
            }

        }

        // Lists are sorted in ascendent order of rating        
        cpuCoresData.Sort(DeviceSettings.Sort);
        gpuMemoryData.Sort(DeviceSettings.Sort);
        cpuFreqData.Sort(DeviceSettings.Sort);

        Device_MemoryRating = 1.0f;

        // cpu rating
        Device_CPUCoresRating = 1.0f;
        foreach (DeviceSettings ds in cpuCoresData)
        {
            if (processorCount <= ds.Boundary)
            {
                Device_CPUCoresRating = ds.Rating;
                break;
            }
        }

        Device_GfxMemoryRating = 1.0f;
        foreach (DeviceSettings ds in gpuMemoryData)
        {
            if (graphicsMemorySize <= ds.Boundary)
            {
                Device_GfxMemoryRating = ds.Rating;
                break;
            }
        }

        Device_CPUFreqRating = 0.0f;
        foreach (DeviceSettings ds in cpuFreqData)
        {
            if (cpuFreq >= ds.Boundary)
            {
                Device_CPUFreqRating = ds.Rating;
                //                break;
            }
        }


#if FREQFORMULA
        finalDeviceRating = Mathf.Clamp(Device_CPUFreqRating, 0.0f, 1.0f);
        m_deviceQualityManager.Device_CalculatedRatingExt = Mathf.Clamp(((Device_CPUCoresRating + Device_GfxMemoryRating) / 2), 0.0f, 1.0f);
        m_deviceQualityManager.Device_UsingRatingFormula = true;
#else
        finalDeviceRating = Mathf.Clamp(((Device_CPUCoresRating + Device_GfxMemoryRating) / 2), 0.0f, 1.0f);
        m_deviceQualityManager.Device_CalculatedRatingExt = Mathf.Clamp(Device_CPUFreqRating, 0.0f, 1.0f);
        m_deviceQualityManager.Device_UsingRatingFormula = false;
#endif

        finalDeviceRating = Mathf.Clamp(finalDeviceRating, 0.0f, 1.0f);

        Log("Graphics memory size = " + graphicsMemorySize + " gpuMemQualityLevel = " + Device_GfxMemoryRating +
           "Num cores = " + processorCount + " cpuQualityRating = " + Device_CPUCoresRating +
           "CPU Freq = " + cpuFreq + " cpuFreqRating = " + Device_CPUFreqRating +
           "Device rating = " + finalDeviceRating);

        return finalDeviceRating;
    }

    public float Device_CalculatedRating
    {
        get
        {
            return m_deviceQualityManager.Device_CalculatedRating;
        }
    }
    public float Device_CalculatedRatingExt
    {
        get
        {
            return m_deviceQualityManager.Device_CalculatedRatingExt;
        }
    }

    public bool Device_UsingRatingFormula
    {
        get
        {
            return m_deviceQualityManager.Device_UsingRatingFormula;
        }
    }

    public float Device_CPUCoresRating { get; private set; }
    public float Device_MemoryRating { get; private set; }
    public float Device_GfxMemoryRating { get; private set; }
    public float Device_CPUFreqRating { get; private set; }

    public float Device_CurrentRating
    {
        get
        {
            return Device_CurrentFeatureSettings.Rating;
        }
    }

    private string Device_CalculatedProfile { get; set; }

    public string Device_CurrentProfile
    {
        get
        {
            return Device_CurrentFeatureSettings.Profile;
        }

        set
        {
            if (Device_CurrentFeatureSettings.Profile != value)
            {
                JSONNode profileSettigns = m_deviceQualityManager.Profiles_GetDataAsJSON(value);
                if (profileSettigns == null)
                {
                    DeviceQualityManager.LogError("No feature settings found for profile " + value);
                }
                else
                {
                    // We need Device_CurrentFeatureSettings to be a separated object from the one that we have for the profile because the configuration for the device
                    // might be slightly different to the profile one                         
                    Device_CurrentFeatureSettings.FromJSON(profileSettigns);
                }
            }
        }
    }

    public FeatureSettings Device_CurrentFeatureSettings { get; set; }

    private string Device_GetInfo()
    {
        System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
        strBuilder.AppendLine("");
        if (instance != null)
        {
            strBuilder.AppendLine("MODEL : " + instance.Device_Model);
        }

        strBuilder.AppendLine("GPU ID : " + SystemInfo.graphicsDeviceID.ToString());
        strBuilder.AppendLine("GPU VENDOR : " + SystemInfo.graphicsDeviceVendor);
        strBuilder.AppendLine("GPU VENDOR ID : " + SystemInfo.graphicsDeviceVendorID.ToString());
        strBuilder.AppendLine("GPU VERSION : " + SystemInfo.graphicsDeviceVersion);
        strBuilder.AppendLine("GPU MEMORY : " + Device_GetGraphicsMemorySize().ToString());
        strBuilder.AppendLine("GPU SHADER LEVEL : " + SystemInfo.graphicsShaderLevel.ToString());
        strBuilder.AppendLine("MAX TEX SIZE : " + SystemInfo.maxTextureSize.ToString());
        strBuilder.AppendLine("OS : " + SystemInfo.operatingSystem);
        strBuilder.AppendLine("CPU COUNT : " + SystemInfo.processorCount.ToString());
        strBuilder.AppendLine("CPU TYPE : " + SystemInfo.processorType);
        strBuilder.AppendLine("SYSTEM MEMORY : " + Device_GetSystemMemorySize().ToString());
        return strBuilder.ToString();
    }
    #endregion

    #region rules
    //    private const string RULES_DEFAULT_SKU = "L0";
    private const string RULES_DEFAULT_SKU = "common";

    private const string KEY_PHYSICS_MAX_RATING = "physicsMaxRating";

    private class DeviceSettings
    {
        public float Rating { get; set; }
        public float Boundary { get; set; }

        public DeviceSettings(float rating, float boundary)
        {
            Rating = rating;
            Boundary = boundary;
        }

        public static int Sort(DeviceSettings d1, DeviceSettings d2)
        {
            int returnValue = 0;
            if (d1.Rating > d2.Rating)
            {
                returnValue = 1;
            }
            else if (d1.Rating < d2.Rating)
            {
                returnValue = -1;
            }

            return returnValue;
        }
    }

    private static float Rules_PhysicsMaxRating
    {
        get
        {
            string key = KEY_PHYSICS_MAX_RATING;
            DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DEVICE_RATING_SETTINGS, RULES_DEFAULT_SKU);
            return (def == null) ? 0f : def.GetAsFloat(key);
        }
    }

    private void Rules_OnLoaded()
    {
        m_isReady = true;

        m_deviceQualityManager.Profiles_Clear();

        DefinitionsManager defManager = DefinitionsManager.SharedInstance;

        // All profiles and configurationjs collections are loaded from rules                           
        FeatureSettings featureSettings = FeatureSettingsHelper;    // Helper
        JSONNode settingsJSON;
        Dictionary<string, DefinitionNode> definitions = defManager.GetDefinitions(DefinitionsCategory.FEATURE_PROFILE_SETTINGS);
        foreach (KeyValuePair<string, DefinitionNode> pair in definitions)
        {
            settingsJSON = pair.Value.ToJSON();
            settingsJSON = FormatJSON(settingsJSON);
            featureSettings.FromJSON(settingsJSON);

            // Checks if it's a configuration collection or a profile to store it in the right place
            if (string.IsNullOrEmpty(featureSettings.Profile))
            {
                Configs_AddConfig(pair.Key, settingsJSON);
            }
            else
            {
                m_deviceQualityManager.Profiles_AddData(featureSettings.Profile, featureSettings.Rating, featureSettings.MinMemory, featureSettings.GfxMemory, settingsJSON);
            }
        }

        // In Unity editor feature settings for all devices are parsed to identify errors
#if UNITY_EDITOR     
        string profileName;
        string validProfileNames = DebugUtils.ListToString(m_deviceQualityManager.Profiles_Names);
        definitions = defManager.GetDefinitions(DefinitionsCategory.FEATURE_DEVICE_SETTINGS);
        foreach (KeyValuePair<string, DefinitionNode> pair in definitions)
        {
            settingsJSON = pair.Value.ToJSON();
            settingsJSON = FormatJSON(settingsJSON);

            // Makes sure that profile is a valid value
            if (settingsJSON.ContainsKey(FeatureSettings.KEY_PROFILE))
            {
                profileName = settingsJSON[FeatureSettings.KEY_PROFILE];
                if (m_deviceQualityManager.Profiles_GetDataAsJSON(profileName) == null)
                {
                    FeatureSettings.LogError("Sku <" + settingsJSON[FeatureSettings.KEY_SKU] + ">: " + profileName + " is not among the valid values for profile name: " + validProfileNames);
                }
            }
        }
#endif       

        // Banned settings are stored in Configs_Catalog
        DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.FEATURE_DEVICE_SETTINGS, CONFIG_SKU_BANNED);
        if (def != null)
        {
            JSONNode json = def.ToJSON();
            if (json != null)
            {
                FormatJSON(json);
            }

            Configs_AddConfig(CONFIG_SKU_BANNED, json);
        }

        // The device rating is calculated
        float rating = Device_CalculateRating();
        m_deviceQualityManager.Device_CalculatedRating = rating;

        Device_CurrentFeatureSettings = CreateFeatureSettings();

        // Some Quality settings received from server in a previous session might be stored in cache
        JSONNode serverQualitySettingsJSON = null;
        if (CacheServerManager.SharedInstance.HasKey(CACHE_SERVER_QUALITY_KEY))
        {
            string value = CacheServerManager.SharedInstance.GetVariable(CACHE_SERVER_QUALITY_KEY);
            serverQualitySettingsJSON = JSON.Parse(value);

            if (IsDebugEnabled)
            {
                Log("Quality settings from server cached : " + ((serverQualitySettingsJSON == null) ? "null" : serverQualitySettingsJSON.ToString()));
            }
        }

        // Some game settings received from server in a previous session might be stored in cache
        JSONNode serverGameSettingsJSON = null;
        if (CacheServerManager.SharedInstance.HasKey(CACHE_SERVER_GAME_KEY))
        {
            string value = CacheServerManager.SharedInstance.GetVariable(CACHE_SERVER_GAME_KEY);
            serverGameSettingsJSON = JSON.Parse(value);

            if (IsDebugEnabled)
            {
                Log("Game settings from server cached : " + ((serverGameSettingsJSON == null) ? "null" : serverGameSettingsJSON.ToString()));
            }
        }

        SetupCurrentFeatureSettings(GetDeviceFeatureSettingsAsJSON(), serverQualitySettingsJSON, serverGameSettingsJSON);
    }
    #endregion

    #region shaders
    // List of shader variant collections to warm up. They have to be sorted in ascendent order (the lowest quality one first)
    public ShaderVariantCollection[] m_shadersVariantCollections;

    public const string SHADERS_KEY_HIGH = "HI_DETAIL_ON";
    private const string SHADERS_KEY_MID = "MEDIUM_DETAIL_ON";
    private const string SHADERS_KEY_LOW = "LOW_DETAIL_ON";

    private string Shaders_CurrentKey { get; set; }

    private string Shaders_QualityLevelToKey(FeatureSettings.ELevel3Values quality)
    {
        string returnValue = null;
        switch (quality)
        {
            case FeatureSettings.ELevel3Values.high:
                returnValue = SHADERS_KEY_HIGH;
                break;

            case FeatureSettings.ELevel3Values.mid:
                returnValue = SHADERS_KEY_MID;
                break;

            default:
                returnValue = SHADERS_KEY_LOW;
                break;
        }

        return returnValue;
    }

    private void Shaders_ApplyQuality(FeatureSettings.ELevel3Values quality)
    {
        string key = Shaders_QualityLevelToKey(quality);
        if (key != Shaders_CurrentKey)
        {
            Shaders_CurrentKey = key;

            Shader.DisableKeyword(SHADERS_KEY_HIGH);
            Shader.DisableKeyword(SHADERS_KEY_MID);
            Shader.DisableKeyword(SHADERS_KEY_LOW);

            Shader.EnableKeyword(Shaders_CurrentKey);

            Shaders_WarmUpVariantCollection(quality);
        }
    }

    private void Shaders_WarmUpVariantCollection(FeatureSettings.ELevel3Values quality)
    {
        ShaderVariantCollection collection = null;
        if (m_shadersVariantCollections != null)
        {
            int qualityAsInt = (int)quality;
            if (qualityAsInt < m_shadersVariantCollections.Length)
            {
                collection = m_shadersVariantCollections[qualityAsInt];
            }
        }
        if (collection == null)
        {
            DeviceQualityManager.LogError("No shader variant collection found for shader quality " + quality);
        }
        else
        {
            collection.WarmUp();
        }
    }

    private string Shaders_GetInfo()
    {
        System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
        strBuilder.AppendLine("");
        strBuilder.AppendLine(">> Shader.HI_DETAIL_ON=" + Shader.IsKeywordEnabled("HI_DETAIL_ON"));
        strBuilder.AppendLine(">> Shader.MEDIUM_DETAIL_ON=" + Shader.IsKeywordEnabled("MEDIUM_DETAIL_ON"));
        strBuilder.AppendLine(">> Shader.LOW_DETAIL_ON=" + Shader.IsKeywordEnabled("LOW_DETAIL_ON"));
        return strBuilder.ToString();
    }
    #endregion

    #region feature_settings
    private FeatureSettings m_featureSettingsHelper;
    private FeatureSettings FeatureSettingsHelper
    {
        get
        {
            if (m_featureSettingsHelper == null)
            {
                m_featureSettingsHelper = CreateFeatureSettings();
            }

            return m_featureSettingsHelper;
        }
    }

    public void SetupCurrentFeatureSettings(JSONNode deviceSettingsJSON, JSONNode serverQualitySettingsJSON, JSONNode serverGameSettingsJSON)
    {
        float rating = m_deviceQualityManager.Device_CalculatedRating;
        string profileName = null;

        string userProfileName = GetUserProfileName();
        bool hasUserProfileName = !string.IsNullOrEmpty(userProfileName);

        if (IsDebugEnabled)
        {
            string msg = "SetupCurrentFeatureSettings: deviceSettingsJSON = ";
            msg += (deviceSettingsJSON == null) ? "null" : deviceSettingsJSON.ToString();
            msg += " qualitySettingsJSON = " + ((serverQualitySettingsJSON == null) ? "null" : serverQualitySettingsJSON.ToString());
            msg += " gameSettingsJSON = " + ((serverGameSettingsJSON == null) ? "null" : serverGameSettingsJSON.ToString());
            msg += " userProfileName = " + userProfileName;
            Log(msg);
        }

        // If no device configuration is passed then try to get the device configuration from rules        
        if (deviceSettingsJSON == null)
        {
            deviceSettingsJSON = GetDeviceFeatureSettingsAsJSON();
        }

        if (deviceSettingsJSON != null)
        {
            // Checks if the rating has been overriden for this device
            if (deviceSettingsJSON.ContainsKey(FeatureSettings.KEY_RATING))
            {
                rating = deviceSettingsJSON[FeatureSettings.KEY_RATING].AsFloat;
            }

            if (deviceSettingsJSON.ContainsKey(FeatureSettings.KEY_PROFILE))
            {
                profileName = deviceSettingsJSON[FeatureSettings.KEY_PROFILE];
            }

            if (IsDebugEnabled)
                Log("deviceSettingsJSON.profileName = " + profileName);

            // This settings are ignored if this settings profile is not the same as the profile chosen by the user. If it's the same profile then we assume that this settings are more
            // specific than the ones defined for the whole profile
            if (hasUserProfileName && profileName != userProfileName)
            {
                deviceSettingsJSON = null;
            }
        }

        // If no profileName is available for the device in iOS then we assume that it's a new device so its rating has to be the maximum
#if UNITY_IOS
        if (string.IsNullOrEmpty(profileName))
        {
            rating = 1f;

			if (IsDebugEnabled)
				Log("rating forced to 1f because profileName is empty");        
        }        
#endif

        // Gets the FeatureSettings object of the profile that corresponds to the calculated rating
        if (string.IsNullOrEmpty(profileName))
        {
            int systemMemorySize = Device_GetSystemMemorySize();
            int gfxMemorySize = Device_GetGraphicsMemorySize();
            profileName = m_deviceQualityManager.Profiles_RatingToProfileName(rating, systemMemorySize, gfxMemorySize);

            if (IsDebugEnabled)
                Log("Based on systemMemorySize = " + systemMemorySize + " rating = " + rating + " profileName is " + profileName);
        }

        Device_CalculatedProfile = profileName;

        // The config received from server has to override the profile
        if (serverQualitySettingsJSON != null)
        {
            // Makes sure that it has the correct format
            serverQualitySettingsJSON = FormatJSON(serverQualitySettingsJSON);

            // Checks if the rating has been overriden for this device
            if (serverQualitySettingsJSON.ContainsKey(FeatureSettings.KEY_RATING))
            {
                rating = serverQualitySettingsJSON[FeatureSettings.KEY_RATING].AsFloat;
            }

            if (serverQualitySettingsJSON.ContainsKey(FeatureSettings.KEY_PROFILE))
            {
                profileName = serverQualitySettingsJSON[FeatureSettings.KEY_PROFILE];
            }

            if (IsDebugEnabled)
                Log("serverSettingsJSON profileName is " + profileName + " json = " + serverQualitySettingsJSON.ToString());

            // This settings are ignored if this settings profile is not the same as the profile chosen by the user. If it's the same profile then we assume that this settings are more
            // specific than the ones defined for the whole profile
            if (hasUserProfileName && profileName != userProfileName)
            {
                serverQualitySettingsJSON = null;
            }
        }

        // User's profile, if the user has set it, has the highest priority        
        if (hasUserProfileName)
        {
            profileName = userProfileName;
            rating = m_deviceQualityManager.Profiles_ProfileNameToRating(profileName);
        }

        Device_CurrentProfile = profileName;

        // In iOS the rating of the device has to be the rating of the profile since the calculated rating is used only as a reference.         
#if !UNITY_IOS             
        Device_CurrentFeatureSettings.Rating = rating;
#endif

        // Debug or release settings are applied 
        string configSku = (IsDebugEnabled) ? CONFIG_SKU_DEBUG : CONFIG_SKU_RELEASE;
        JSONNode configJSON = Configs_GetConfig(configSku);
        if (configJSON != null)
        {
            Device_CurrentFeatureSettings.OverrideFromJSON(configJSON);
        }

        // Banned settings are applied. These settings are used to override the profile settings and they can be override by the particular settings for this device.
        // Its purpose is to disable a feature that has been proven to be problematic on several devices. In this case the feature is banned by default for all devices and it has to be
        // enabled device by device (after testing it on the actual devices) on their corresponding entries in featureDeviceSettings.xml        
        JSONNode bannedJSON = Configs_GetConfig(CONFIG_SKU_BANNED);
        if (bannedJSON != null)
        {
            Device_CurrentFeatureSettings.OverrideFromJSON(bannedJSON);
        }

        // We need to override the default configuration of the profile with the particular configuration defined for the device, if there's one
        if (deviceSettingsJSON != null)
        {
            Device_CurrentFeatureSettings.OverrideFromJSON(deviceSettingsJSON);
        }

        // The configuration received for the device from server has a bigger priority than the one retrieved from rules
        if (serverQualitySettingsJSON != null)
        {
            Device_CurrentFeatureSettings.OverrideFromJSON(serverQualitySettingsJSON);
        }

        // The configuration received for the game from server has the biggest priority so it's the last one to be applied so it can override the values set by the other two configurations
        if (serverGameSettingsJSON != null)
        {
            serverGameSettingsJSON = FormatJSON(serverGameSettingsJSON);
            Device_CurrentFeatureSettings.OverrideFromJSON(serverGameSettingsJSON);
        }

        ApplyCurrentFeatureSetting(false);
    }

    public void RecalculateAndApplyProfile()
    {
        SetupCurrentFeatureSettings(GetDeviceFeatureSettingsAsJSON(), null, null);
        AdjustScreenResolution(Device_CurrentFeatureSettings);
    }

    public void RestoreCurrentFeatureSettingsToDevice()
    {
        if (IsDebugEnabled)
            Log("RestoreCurrentFeatureSettingsToDevice");

        RecalculateAndApplyProfile();
    }

    private FeatureSettings CreateFeatureSettings()
    {
        return new FeatureSettings();
    }

    public void ApplyCurrentFeatureSetting(bool resolution)
    {
        ApplyFeatureSetting(Device_CurrentFeatureSettings);

        if (resolution)
        {
            AdjustScreenResolution(Device_CurrentFeatureSettings);
        }

        //JSONNode json = Device_CurrentFeatureSettings.ToJSON();
        //Debug.Log(json);
    }

    private int CurrentQualityIndex { get; set; }

    public void AdjustScreenResolution(FeatureSettings settings)
    {
        float resolutionFactor = settings.GetValueAsFloat(FeatureSettings.KEY_RESOLUTION_FACTOR);

        if (resolutionFactor < (float)m_OriginalScreenHeight)
        {
            resolutionFactor /= (float)m_OriginalScreenHeight;
        }
        else
        {
            resolutionFactor = 1.0f;
        }

        int width = (int)((float)m_OriginalScreenWidth * resolutionFactor);
        int height = (int)((float)m_OriginalScreenHeight * resolutionFactor);

        if (Screen.width != width || Screen.height != height)
        {
#if UNITY_ANDROID
            // if bigger than oreo (8.0)
            // This is a tmp fix for HDK-1911
            if (PlatformUtilsAndroidImpl.GetSDKLevel() >= 26 && width == 1920 && height == 1080)
            {
                width--;
                height--;
            }
#endif
            Screen.SetResolution(width, height, true);
        }
    }
    private void ApplyFeatureSetting(FeatureSettings settings)
    {
        FeatureSettings.EQualityLevelValues quality = settings.GetValueAsQualityLevel(FeatureSettings.KEY_QUALITY_LEVEL);
#if !UNITY_EDITOR
        int qualityIndex = (int)quality;
        if (qualityIndex != CurrentQualityIndex)
        {
            CurrentQualityIndex = qualityIndex;
            QualitySettings.SetQualityLevel(CurrentQualityIndex);
            
            if (IsDebugEnabled)
                Log(">> qualityLevel:" + quality.ToString() + " index = " + qualityIndex);
        }
#endif
        if (quality <= FeatureSettings.EQualityLevelValues.low) {
            Time.fixedDeltaTime = 1f / 20f;
        } else {
            Time.fixedDeltaTime = 1f / 30f;
        }

        FeatureSettings.ELevel3Values shadersLevel = settings.GetValueAsLevel3(FeatureSettings.KEY_SHADERS_LEVEL);
        Shaders_ApplyQuality(shadersLevel);
        /*
                float resolutionFactor = settings.GetValueAsFloat(FeatureSettings.KEY_RESOLUTION_FACTOR);

                if (resolutionFactor < (float)m_OriginalScreenHeight)
                {
                    resolutionFactor /= (float)m_OriginalScreenHeight;
                }
                else
                {
                    resolutionFactor = 1.0f;
                }

                int width = (int)((float)m_OriginalScreenWidth * resolutionFactor);
                int height = (int)((float)m_OriginalScreenHeight * resolutionFactor);

                Screen.SetResolution(width, height, true);
        */
        //        AdjustScreenResolution(settings);

        Log("Device Rating:" + settings.Rating);
        Log("Profile:" + settings.Profile);
        Log(Device_GetInfo());
        Log(Shaders_GetInfo());
        Log(">> Time.fixedDeltaTime:" + Time.fixedDeltaTime);

        if (IsLightmapEnabled) {
            Shader.EnableKeyword("FORCE_LIGHTMAP");
        }
        else
        {
            Shader.DisableKeyword("FORCE_LIGHTMAP");
        }

        if (Camera.main != null)
            Camera.main.Render();

        m_featureSettingsApplied = true;
    }

    public bool IsFeatureSettingsApplied { get { return m_featureSettingsApplied; } }
    private bool m_featureSettingsApplied = false;


    private JSONNode FormatJSON(JSONNode json)
    {
        JSONNode returnValue = json;
        if (json != null)
        {
            JSONClass jsonClass = json as JSONClass;

            returnValue = new JSONClass();

            // Server might sent the "overriderProfile" parameter to force a profile so in that case we need to override "profile" parameter with this value
            string overrideProfileKey = "overriderProfile";
            if (json.ContainsKey(overrideProfileKey))
            {
                string overrideProfileValue = json[overrideProfileKey];
                if (!string.IsNullOrEmpty(overrideProfileValue))
                {
                    json[FeatureSettings.KEY_PROFILE] = overrideProfileValue;
                }

                json[overrideProfileKey] = "";
            }

            foreach (KeyValuePair<string, JSONNode> pair in jsonClass.m_Dict)
            {
                if (
                    pair.Key == "iOSGeneration" || // used for making easier to identify a device
                    pair.Key == "mngd" ||          // used on server side to identify devices that have been managed
                    pair.Key == "comments"         // used on server to store some comments about the settings managed for a device
                    )
                    continue;

                // Empty keys or keys with "as_profile" as a value need to be ignored
                if (!string.IsNullOrEmpty(pair.Value.Value) && pair.Value.Value != "as_content")
                {
                    returnValue.Add(pair.Key, pair.Value);
                }
            }

#if UNITY_EDITOR
            // Only in editor mode we want to detect any field or value that is not supported
            returnValue = FeatureSettingsHelper.ParseJSON(returnValue);
#endif
        }

        return returnValue;
    }

    /// <summary>
    /// Returns the feature settings information for the device, if available, in JSON format
    /// </summary>    
    private JSONNode GetDeviceFeatureSettingsAsJSON()
    {
        JSONNode returnValue = null;
        Dictionary<string, DefinitionNode> profilesData = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.FEATURE_DEVICE_SETTINGS);
        string deviceModel = Device_Model;
        foreach (KeyValuePair<string, DefinitionNode> pair in profilesData)
        {
            if (deviceModel.Contains(pair.Key))
            {
                returnValue = pair.Value.ToJSON();
            }
        }

        // We only need the keys that actually have a valid value
        if (returnValue != null)
        {
            returnValue = FormatJSON(returnValue);
        }

        return returnValue;
    }

    /// <summary>
    /// Returns the profile level currently active.
    /// </summary>
    /// <returns>A value in [0, NUM_PROFILES -1]. The bigger the value the better the profile</returns>
    public int GetCurrentProfileLevel()
    {
        return Device_CurrentFeatureSettings == null ? -1 : ProfileNameToLevel(Device_CurrentFeatureSettings.Profile);
    }

    /// <summary>
    /// Returns max profile level supported by the device
    /// </summary>
    /// <returns>A value in [0, NUM_PROFILES -1]. The bigger the value the better the profile</returns>
    public int GetMaxProfileLevelSupported()
    {
        int systemMemorySize = Device_GetSystemMemorySize();
        return m_deviceQualityManager.Profiles_GetMaxProfileLevel(systemMemorySize);
    }

    /// <summary>
    /// Returns the profile level chosen by the user.
    /// </summary>
    /// <returns>A value in [0, MAX_PROFILE_LEVEL] if the user has ever chosen a profile, otherwise <c>-1</c></returns>
    public int GetUserProfileLevel()
    {
        // Translates name to level
        string userProfileName = GetUserProfileName();
        return ProfileNameToLevel(userProfileName);
    }

    /// <summary>
    /// Sets a profile level as the user's profile. This method just changes the user's profile level. Call <c>RecalculateAndApplyProfile()</c> to get it applied.
    /// </summary>
    /// <param name="value">A value in [0, NUM_PROFILES -1] to set as user's profile. The bigger the value the better the profile.</param>    
    public void SetUserProfileLevel(int value)
    {
        SetUserProfileName(ProfileLevelToName(value));
    }

    /// <summary>
    /// Returns the profile name chosen by the user.
    /// </summary>
    /// <returns>A string corresponding to one of the values of <c>FeatureSettings.EQualityLevelValues</c> if the user has ever chosen a profile, otherwise <c>null</c></returns>
    private string GetUserProfileName()
    {
        string userProfileName = PersistencePrefs.GetUserProfileName();
        // If user's profile is higher than max profile supported (probably because this level was supported in an earlier version of the game but it's not supported anymore) then
        // max profile is returned
        int userProfileLevel = ProfileNameToLevel(userProfileName);
        if (userProfileLevel > GetMaxProfileLevelSupported())
        {
            userProfileLevel = GetMaxProfileLevelSupported();
            userProfileName = ProfileLevelToName(userProfileLevel);
        }

        return userProfileName;
    }

    /// <summary>
    /// Sets a profile name as the user's profile.
    /// </summary>
    /// <param name="value">A string corresponding to one of the values of <c>FeatureSettings.EQualityLevelValues</c></param>
    /// <param name="apply">When <c>true</c> this profile is applied</param>
    private void SetUserProfileName(string value)
    {
        PersistencePrefs.SetUserProfileName(value);
    }

    /// <summary>
    /// 
    /// Translates a profile name into its level
    /// </summary>
    /// <param name="value">A string corresponding to one of the values of <c>FeatureSettings.EQualityLevelValues</c></param>
    /// <returns>A value in [0, NUM_PROFILES - 1]</returns>
    private int ProfileNameToLevel(string value)
    {
        return FeatureSettings.EQualityLevelValuesNames.IndexOf(value);
    }

    /// <summary>
    /// Translates a profile level into its name
    /// </summary>
    /// <param name="value">A value in [0, NUM_PROFILES - 1]</param>
    /// <returns>A string corresponding to one of the values of <c>FeatureSettings.EQualityLevelValues</c></returns>
    private string ProfileLevelToName(int level)
    {
        return (level >= 0 && level < FeatureSettings.EQualityLevelValuesNames.Count) ? FeatureSettings.EQualityLevelValuesNames[level] : null;
    }
    #endregion

    #region configs
    // Region responsible for storing some special configurations such as "debug" or "release" settings

    /// <summary>
    /// Configuration with the settings banned for all devices by default.
    /// </summary>
    private const string CONFIG_SKU_BANNED = "banned";

    /// <summary>
    /// Configuration with the settings for debug mode
    /// </summary>
    private const string CONFIG_SKU_DEBUG = "debug";

    /// <summary>
    /// Configuration with the settings for release mode
    /// </summary>
    private const string CONFIG_SKU_RELEASE = "release";

    private Dictionary<string, JSONNode> Configs_Catalog;

    private void Configs_AddConfig(string sku, JSONNode json)
    {
        if (Configs_Catalog == null)
        {
            Configs_Catalog = new Dictionary<string, JSONNode>();
        }

        if (Configs_Catalog.ContainsKey(sku))
        {
            LogWarning("Configuration <" + sku + "> has already been added to the catalog of configurations");
            Configs_Catalog[sku] = json;
        }
        else
        {
            Configs_Catalog.Add(sku, json);
        }
    }

    private JSONNode Configs_GetConfig(string sku)
    {
        return (Configs_Catalog != null && Configs_Catalog.ContainsKey(sku)) ? Configs_Catalog[sku] : null;
    }
    #endregion

    #region facade
    public List<string> Profiles_Names
    {
        get
        {
            return m_deviceQualityManager.Profiles_Names;
        }
    }

    private static bool sm_isDebugBuild;

    /// <summary>
    /// Returns whether or not debug mode is enabled. 
    /// It's a static method to be sure that it will be available at all times since it's looked up by some MonoBehaviours when awaking
    /// </summary>
    public static bool IsDebugEnabled
    {
        get
        {
            if (!sm_initialized)
            {
                Static_Initialize();
            }

            return sm_isDebugBuild;
        }
    }

    private static bool sm_areCheatsEnabled;

    public static bool AreCheatsEnabled
    {
        get
        {
            if (!sm_initialized)
            {
                Static_Initialize();
            }

            return sm_areCheatsEnabled;
        }
    }

    /// <summary>
    /// Returns whether or not the profiler is enabled. It's used to enable some features required by the profiler
    /// It's a static method to be sure that it will be available at all times since it's looked up by some MonoBehaviours when awaking
    /// </summary>
    public static bool IsProfilerEnabled
    {
        get
        {
            return UnityEngine.Debug.isDebugBuild;
        }
    }

    public static bool IsVerticalOrientationEnabled
    {
        get
        {
            return false;
        }
    }

    /// <summary>
    /// Whether or not the game leve scenes under development should be loaded when loading the level.
    /// It's a static method because it doesn't depend on the device profile and we want it to be ready at any moment since it can be called
    /// when the level editor loads the game
    /// </summary>
    public static bool IsWIPScenesEnabled
    {
        get
        {
            // Makes sure that WIO scenes won't be loaded on PRODUCTION
            /*ServerManager.ServerConfig kServerConfig = ServerManager.SharedInstance.GetServerConfig();
            return (kServerConfig != null &&
                    kServerConfig.m_eBuildEnvironment != CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION &&
                    kServerConfig.m_eBuildEnvironment != CaletyConstants.eBuildEnvironments.BUILD_STAGE &&
                    kServerConfig.m_eBuildEnvironment != CaletyConstants.eBuildEnvironments.BUILD_STAGE_QC);         
                    */
            return false;
        }
    }

    public static bool IsControlPanelEnabled
    {
        get
        {
            // When the application is not alive that means that the application is being shutting down so ServerManager might have already been destroyed so we don't want
            // to create it again
            if (ApplicationManager.IsAlive)
            {
#if UNITY_EDITOR
                return true;
#else
	            Calety.Server.ServerConfig kServerConfig = ServerManager.SharedInstance.GetServerConfig();
	            return (kServerConfig != null && kServerConfig.m_eBuildEnvironment != CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION);               
#endif
            }
            else
            {
                return false;
            }
        }
    }

    public static bool AreAdsEnabled
    {
        get
        {
            return true;
        }
    }

    public static bool IsBackButtonEnabled()
    {
        // Back button is disabled in TEST mode to prevent the test from being paused because the back button is pressed
        return ApplicationManager.instance.appMode == ApplicationManager.Mode.PLAY;
    }

    public bool IsIncentivisedLoginEnabled()
    {
#if UNITY_EDITOR
        return true;
#elif UNITY_IOS
        return true;
#else
        return true;
#endif
    }

	public static bool IsDailyRewardsEnabled() {
		// Feel free to disable it
		return true;
	}

    /// <summary>
    /// When <c>true</c> tracking is enabled. When <c>false</c> no tracking stuff is done at all
    /// </summary>
    public bool IsTrackingEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_TRACKING);
        }
    }

    /// <summary>
    /// When <c>true</c> performance tracking is enabled. When <c>false</c> no tracking stuff is done at all
    /// </summary>
    public bool IsPerformanceTrackingEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_PERFORMANCE_TRACKING);
        }
    }

    /// <summary>
    /// Delay in seconds between every track event
    /// </summary>
    public int PerformanceTrackingDelay
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsInt(FeatureSettings.KEY_PERFORMANCE_TRACKING_DELAY);
        }
    }

    /// <summary>
    /// When <c>true</c> events that couldn't be sent over the network are stored in the device storage so they can be sent when network is working again
    /// </summary>
    public bool IsTrackingOfflineCachedEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_TRACKING_OFFLINE_CACHED);
        }
    }

    public bool IsSafeTrackingOfflineCachedEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings != null && Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_SAFE_TRACKING_OFFLINE_CACHED);
        }
    }

    public bool IsTrackingStoreUnsentOnSaveGameEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings != null && Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_TRACKING_STORE_UNSENT_ON_SAVE_GAME);
        }
    }

    public float TrackingStoreUnsentMinTime
    {
        get
        {
            return (Device_CurrentFeatureSettings == null) ? 5f : Device_CurrentFeatureSettings.GetValueAsFloat(FeatureSettings.KEY_TRACKING_STORE_UNSENT_MIN_TIME);
        }
    }

    public bool IsMiniTrackingEnabled
    {
        get
        {
            // Disabled since it's a temporary tracking that is used only for play test
            return false;
        }
    }

    public bool IsContentDeltasEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_CONTENT_DELTAS);
        }
    }

    public bool IsContentDeltasCachedEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_CONTENT_DELTAS_CACHED);
        }
    }

    public bool IsGlowEffectEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_GLOW_EFFECT);
        }
    }

    public bool IsDrunkEffectEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_DRUNK_EFFECT);
        }
    }
                    
    public bool IsFrameColorEffectEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_FRAME_COLOR_EFFECT);
        }
    }

    public bool IsLightmapEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_LIGHTMAP);
        }
    }
    
    public bool IsHelicopterDestroying
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_HELICOPTER_DESTROYS);
        }
    }

    public FeatureSettings.ELevel2Values EntitiesLOD
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsLevel2(FeatureSettings.KEY_ENTITIES_LOD);
        }
    }

    public FeatureSettings.ELevel4Values LevelsLOD
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsLevel4(FeatureSettings.KEY_LEVELS_LOD);
        }
    }

    public bool IsBossZoomOutEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_BOSS_ZOOM_OUT);
        }
    }

    public bool IsDecoSpawnersEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_DECO_SPAWNERS);
        }
    }

    public bool IsLockEffectEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_LOCK_EFFECT);
        }
    }

    public bool IsFogOnDemandEnabled { get; set; }    

	public FeatureSettings.ELevel5Values Particles
	{
		get
		{
			return Device_CurrentFeatureSettings.GetValueAsLevel5(FeatureSettings.KEY_PARTICLES);
		}
	}

    public bool IsParticlesFeedbackEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_PARTICLES_FEEDBACK);
        }
    }

    public bool IsBloodEnabled()
    {
        bool ret = false;
        if (!GDPRManager.SharedInstance.IsAgeRestrictionEnabled() && Prefs.GetBoolPlayer(GameSettings.BLOOD_ENABLED, true))
        {
            ret = true;
        }
        return ret;
    }

	public bool IfPetRigidbodyInterpolates
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_PET_INTERPOLATES);
        }
    }

    public int MaxZoomPerformanceCost
	{
		get
		{
			return Device_CurrentFeatureSettings.GetValueAsInt( FeatureSettings.MAX_ZOOM_COST );
		}
	}
    
    public bool IsAutomaticReloginEnabled()
    {        
        return (Device_CurrentFeatureSettings == null) ? false : Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_AUTOMATIC_RELOGIN);        
    }
    
    public int GetAutomaticReloginPeriod()
    {     
        return (Device_CurrentFeatureSettings == null) ? 0 : Device_CurrentFeatureSettings.GetValueAsInt(FeatureSettings.KEY_AUTOMATIC_RELOGIN_PERIOD);        
    }

	public int GetAdTimeout()
	{
		return (Device_CurrentFeatureSettings == null) ? 0 : Device_CurrentFeatureSettings.GetValueAsInt(FeatureSettings.KEY_AD_TIMEOUT);
	}

    public bool IsCP2Enabled()
    {
        return (Device_CurrentFeatureSettings == null) ? false : Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_CP2);
    }

    public bool IsCP2InterstitialEnabled()
    {
        return (Device_CurrentFeatureSettings == null) ? false : Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_CP2_INTERSTITIAL);
    }

    public int GetCP2InterstitialFrequency()
    {
        return (Device_CurrentFeatureSettings == null) ? 0 : Device_CurrentFeatureSettings.GetValueAsInt(FeatureSettings.KEY_CP2_INTERSTITIAL_FREQUENCY);
    }

    public int GetCP2InterstitialMinRounds()
    {
        return (Device_CurrentFeatureSettings == null) ? 0 : Device_CurrentFeatureSettings.GetValueAsInt(FeatureSettings.KEY_CP2_INTERSTITIAL_MIN_ROUNDS);
    }

    public bool IsCrashlyticsEnabled()
    {
        return (Device_CurrentFeatureSettings == null) ? false : Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_CRASHLYTICS);
    }


    public static bool MenuDragonsAsyncLoading
    {
        get
        {                  
            return true;
        }
    }

    public bool NeedPendingTransactionsServerConfirm()
    {        
        return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_PENDING_TRANSACTIONS_SERVER_CONFIRM);        
    }

    public float SocialPlatformLoginTimeout
    {
        get
        {
            return (Device_CurrentFeatureSettings == null) ? 5f : Device_CurrentFeatureSettings.GetValueAsFloat(FeatureSettings.KEY_SOCIAL_PLAFTORM_LOGIN_TIMEOUT);
        }
    }
    #endregion

    #region log
    private const string PREFIX = "FeatureSettingsManager:";

    public static void Log(string message)
    {        
        Debug.Log(PREFIX + message);
        //Debug.Log("<color=cyan>" + PREFIX + message + " </color>");
    }

    public static void LogWarning(string message)
    {
        Debug.LogWarning(PREFIX + message);
    }

    public static void LogError(string message)
    {
        Debug.LogError(PREFIX + message);
    }

    public void Debug_Test()
    {
        // 0: very_low
        // 0.3: low
        // 0.7: mid
        // 0.85: high
        // 1: very_high
        float rating = 0.75f;
        int memorySize = 2767;
        int gfxMemorySize = 1024;

        string profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating, memorySize, gfxMemorySize);
        Log("Rating: " + rating + " profile = " + profile + " memorySize = " + memorySize + " gfxMemorySize = " + gfxMemorySize);

        rating = 0.1f;
        gfxMemorySize = 128;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating, memorySize, gfxMemorySize);
        Log("Rating: " + rating + " profile = " + profile + " memorySize = " + memorySize + " gfxMemorySize = " + gfxMemorySize);

        rating = 0.3f;
        gfxMemorySize = 1024;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating, memorySize, gfxMemorySize);
        Log("Rating: " + rating + " profile = " + profile + " memorySize = " + memorySize + " gfxMemorySize = " + gfxMemorySize);

        rating = 0.5f;
        gfxMemorySize = 1024;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating, memorySize, gfxMemorySize);
        Log("Rating: " + rating + " profile = " + profile + " memorySize = " + memorySize + " gfxMemorySize = " + gfxMemorySize);

        rating = 0.7f;
        memorySize = 2048;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating, memorySize, gfxMemorySize);
        Log("Rating: " + rating + " profile = " + profile + " memorySize = " + memorySize + " gfxMemorySize = " + gfxMemorySize);

        rating = 0.8f;
        gfxMemorySize = 2048;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating, memorySize, gfxMemorySize);
        Log("Rating: " + rating + " profile = " + profile + " memorySize = " + memorySize + " gfxMemorySize = " + gfxMemorySize);

        rating = 0.85f;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating, memorySize, gfxMemorySize);
        Log("Rating: " + rating + " profile = " + profile + " memorySize = " + memorySize + " gfxMemorySize = " + gfxMemorySize);

        rating = 0.9f;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating, memorySize, gfxMemorySize);
        Log("Rating: " + rating + " profile = " + profile + " memorySize = " + memorySize + " gfxMemorySize = " + gfxMemorySize);

        rating = 1.0f;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating, memorySize, gfxMemorySize);
        Log("Rating: " + rating + " profile = " + profile + " memorySize = " + memorySize + " gfxMemorySize = " + gfxMemorySize);

        rating = 1.1f;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating, memorySize, gfxMemorySize);
        Log("Rating: " + rating + " profile = " + profile + " memorySize = " + memorySize + " gfxMemorySize = " + gfxMemorySize);
    }
    #endregion

    #region fps

    // Internal logic
    private int m_NumFPSTimes;
    float[] m_FPSTimes;
    float m_FPSSum;
    int m_FPSIndex;

    public float SystemFPS
    {
        get; private set;
    }
    public float AverageSystemFPS
    {
        get; private set;
    }

    private void StartFPS()
    {
        // FPS Initialization
        SetFPSAverageBuffer(20);    //Default average buffer
    }

    public void SetFPSAverageBuffer(int bufSize)
    {
        m_NumFPSTimes = bufSize;
        m_FPSSum = 0.0f;
        if (bufSize > 0)
        {
            m_FPSTimes = new float[m_NumFPSTimes];
            m_FPSIndex = 0;
            for (int i = 0; i < m_NumFPSTimes; i++)
                m_FPSSum += (m_FPSTimes[i] = Application.targetFrameRate);

            AverageSystemFPS = SystemFPS = Application.targetFrameRate;
        }
        else
        {
            m_FPSTimes = null;
        }
    }

    // Update FPS
    private void UpdateFPS()
    {
        float dTime = Time.unscaledDeltaTime;
        SystemFPS = (dTime > 0.0f) ? 1.0f / dTime : 0.0f;

        if (m_NumFPSTimes > 0)
        {
            float diff = SystemFPS - m_FPSTimes[m_FPSIndex];
            m_FPSTimes[m_FPSIndex++] += diff;
            if (m_FPSIndex >= m_NumFPSTimes)
                m_FPSIndex = 0;
/*
            AverageSystemFPS = 0;
            for (int i = 0; i < m_NumFPSTimes; i++)
            {
                AverageSystemFPS += m_FPSTimes[i];
            }
            AverageSystemFPS /= m_NumFPSTimes;
*/
            m_FPSSum += diff;
            AverageSystemFPS = m_FPSSum / m_NumFPSTimes;
        }
        else
        {
            AverageSystemFPS = SystemFPS;
        }


    }



    #endregion //fps
}