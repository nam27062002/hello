// FeatureSettingsManager.cs
// Hungry Dragon
// 
// Created by David Germade
// Copyright (c) 2017 Ubisoft. All rights reserved.

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
    private DeviceQualityManager m_deviceQualityManager;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Inititalization.
    /// </summary>
    private void Awake()
    {
#if UNITY_EDITOR
        Device_Model = "UNITY_EDITOR";
#else
        Device_Model = SystemInfo.deviceModel;
#endif

        Server_Reset();

        CurrentQualityIndex = -1;
        Shaders_CurrentKey = null;
        State = EState.WaitingForRules;        

        m_deviceQualityManager = new DeviceQualityManager();

        InitDefaultValues();    
    }

    private void InitDefaultValues()
    {
        IsFogOnDemandEnabled = !IsDebugEnabled;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (m_deviceQualityManager != null)
        {
            m_deviceQualityManager.Clear();
        }        
    }

    private void Update()
    {
        switch (State)
        {
            case EState.WaitingForRules:
                if (ContentManager.ready)
                {
                    Rules_OnLoaded();
                    State_Timer = 0f;
                    State = EState.WaitingToCheckConnection;                    
                }                
                break;

            case EState.WaitingToCheckConnection:
                State_Timer -= Time.deltaTime;
                if (State_Timer <= 0)
                {
                    State = EState.CheckingConnection;

                    // Check if there's connection
                    Authenticator.Instance.CheckConnection(delegate (Error connectionError)
                    {
                        if (connectionError == null)
                        {
                            // If there's connection then the request is sent to the server
                            Server_RequestQualitySettings();
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

            case EState.WaitingToApplyServerResponse:
                if (Server_QualitySettingsJSONToApply == null)
                {
                    LogError("JSON received from server can't be null");
                    State = EState.Done;
                }
                else if (ISApplyFeatureSettingsAllowed())
                {                    
                    // Content configuration for that device and server configuration
                    SetupCurrentFeatureSettings(GetDeviceFeatureSettingsAsJSON(), Server_QualitySettingsJSONToApply);                    
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
    }

    public bool IsReady()
    {
        return Device_CurrentFeatureSettings != null;
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
        WaitingToApplyServerResponse,           // Waiting for an appropriate moment to apply the quality settings received from server        
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
    /// Key for the latest settings received from server
    /// </summary>
    private string SERVER_KEY_SETTINGS_DATA_CACHED = "fSettings_data";

    /// <summary>
    /// Key for the version of the game when the latest settings were stored.
    /// </summary>
    private string SERVER_KEY_SETTINGS_VERSION_CACHED = "fSettings_version";

    private float SERVER_TIME_TO_WAIT_BETWEEN_CONNECTION_CHECKS = 5f;

    /// <summary>
    /// Time to wat for the user to log in once the client has been told to upload its quality settings information to the server
    /// </summary>
    private float SERVER_TIME_TO_WAIT_FOR_LOGIN_WHEN_TOLD_TO_UPLOAD = 20f * 60f;    

    private float Server_TimeToWaitToUploadSettings { get; set; }

    /// <summary>
    /// JSON with the quality settings received from the server to be applied
    /// </summary>
    private JSONNode Server_QualitySettingsJSONToApply { get; set; }
   
    private void Server_Reset()
    {
        Server_QualitySettingsJSONToApply = null;
        Server_ResetUploadQualitySettings();
    }

    private void Server_RequestQualitySettings()
    {
        Server_Reset();

        // We need to wait for the response
        State = EState.WaitingForServerResponse;

        GameServerManager.SharedInstance.GetQualitySettings(delegate (Error error, Dictionary<string, object> response)
        {
            if (error == null)
            {
                // If there's no error then we need to wait for the right moment to apply the quality settings received from server
                string qualitySettings = response["response"] as string;                
                if (qualitySettings != null)
                {                    
                    JSONNode json = JSONNode.Parse(qualitySettings);
                    Server_QualitySettingsJSONToApply = FormatJSON(json);                    
                }

                // If there's no data then we need to upload the profile calculated to the server when as long as the user logs in
                string key = "upLoadRequired";
                if (response.ContainsKey(key) && (bool)response[key])
                {
                    Server_PrepareUploadQualitySettings();
                }

                if (Server_QualitySettingsJSONToApply == null)
                {
                    State = EState.Done;
                    Server_ClearCache();                                                                
                }
                else
                {
                    // If needs to be stored in the mobile cache so it can be applied next time the user starts the game just in case she's playing offline
                    string currentVersion = ServerManager.SharedInstance.GetServerConfig().m_strClientVersion;
                    Server_CacheData(currentVersion, Server_QualitySettingsJSONToApply.ToString());

                    State = EState.WaitingToApplyServerResponse;                    
                }
            }
            else
            {
                DeviceQualityManager.LogError("get quality settings response error " + error.message);

                // No quality settings information got from server so nothing has to be applied and the game continues with the information defined in content
                State = EState.Done;
            }
        });
    }

    private void Server_ResetUploadQualitySettings()
    {
        Server_TimeToWaitToUploadSettings = 0f ;       
    }

    private void Server_PrepareUploadQualitySettings()
    {        
        // Sets max time to wait to upload settings
        Server_TimeToWaitToUploadSettings = SERVER_TIME_TO_WAIT_FOR_LOGIN_WHEN_TOLD_TO_UPLOAD;        
    }

    private void Server_UploadQualitySettings()
    {
        // The calculated profile is the one that is sent to the server since the current profile could be different to the calculated profile if the client has applied
        // some settings received from server that were stored in the device cache in a previous session
        JSONNode json = new JSONClass();
        string profileName = Device_CalculatedProfile;
        //profileName = "high";
        json.Add("profile", profileName);             
        GameServerManager.SharedInstance.SetQualitySettings(json.ToString(), delegate (Error error, Dictionary<string, object> response)
        {
            if (error == null)
            {
                Log("Quality settings uploaded successfully");
            }
            else
            {
                Log("Error when uploading quality settings " + error.message);
            }
        });                    

        Server_ResetUploadQualitySettings();
    }   
    
    /// <summary>
    /// Returns the JSON received from server that is stored in the device cache
    /// </summary>
    /// <returns></returns>
    private JSONNode Server_GetCachedJSON()
    {
        JSONNode returnValue = null;
        
        string raw = PlayerPrefs.GetString(SERVER_KEY_SETTINGS_DATA_CACHED);
        if (!string.IsNullOrEmpty(raw))
        {            
            returnValue = JSON.Parse(raw);
        }

        return returnValue;
    }     

    private string Server_GetVersionCachedJSON()
    {
        return PlayerPrefs.GetString(SERVER_KEY_SETTINGS_VERSION_CACHED);
    }

    private void Server_ClearCache()
    {
        PlayerPrefs.DeleteKey(SERVER_KEY_SETTINGS_VERSION_CACHED);
        PlayerPrefs.DeleteKey(SERVER_KEY_SETTINGS_DATA_CACHED);
    }

    private void Server_CacheData(string version, string data)
    {
        PlayerPrefs.SetString(SERVER_KEY_SETTINGS_VERSION_CACHED, version);
        PlayerPrefs.SetString(SERVER_KEY_SETTINGS_DATA_CACHED, data);
    }
    #endregion

    #region device
    public string Device_Model { get; set; }

    private float Device_CalculateRating()
    {
        //Average the devices RAM, CPU and GPU details to give a rating betwen 0 and 1
        float finalDeviceRating = 0.0f;

        int processorCount = SystemInfo.processorCount;
        int systemMemorySize = SystemInfo.systemMemorySize;
        int graphicsMemorySize = SystemInfo.graphicsMemorySize;

        Dictionary<string, DefinitionNode> definitions = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.DEVICE_RATING_SETTINGS);
        List<DeviceSettings> memoryData = new List<DeviceSettings>();
        List<DeviceSettings> cpuCoresData = new List<DeviceSettings>();
        List<DeviceSettings> gpuMemoryData = new List<DeviceSettings>();        

        DeviceSettings data;
        string key;
        foreach (KeyValuePair<string, DefinitionNode> pair in definitions)
        {
            key = "memoryRating";
            if (pair.Value.Has(key))
            {
                data = new DeviceSettings(pair.Value.GetAsFloat(key), pair.Value.GetAsFloat("memoryBoundary"));
                memoryData.Add(data);
            }

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
        }

        // Lists are sorted in ascendent order of rating
        memoryData.Sort(DeviceSettings.Sort);
        cpuCoresData.Sort(DeviceSettings.Sort);
        gpuMemoryData.Sort(DeviceSettings.Sort);        

        // Memory rating
        Device_MemoryRating = 1.0f;        
        foreach (DeviceSettings ds in memoryData)
        {
            if (systemMemorySize <= ds.Boundary)
            {
                Device_MemoryRating = ds.Rating;
                break;
            }
        }

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

        float shaderMultiplier = 1.0f;
        if (SystemInfo.graphicsShaderLevel == 20)
        {
            shaderMultiplier = 0.0f;
        }              

        finalDeviceRating = ((Device_MemoryRating + Device_CPUCoresRating + Device_GfxMemoryRating) / 3) * shaderMultiplier;
        finalDeviceRating = Mathf.Clamp(finalDeviceRating, 0.0f, 1.0f);

        Log("Memory size = " + systemMemorySize + " memQualityRating = " + Device_MemoryRating +
           "Graphics memory size = " + graphicsMemorySize + " gpuMemQualityLevel = " + Device_GfxMemoryRating +
           "Num cores = " + processorCount + " cpuQualityRating = " + Device_CPUCoresRating +
           "Shader multiplier = " + shaderMultiplier + 
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

    public float Device_CPUCoresRating { get; private set; }
    public float Device_MemoryRating { get; private set; }
    public float Device_GfxMemoryRating { get; private set; }

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
        strBuilder.AppendLine("MODEL : " + SystemInfo.deviceModel);
        strBuilder.AppendLine("GPU ID : " + SystemInfo.graphicsDeviceID.ToString());
        strBuilder.AppendLine("GPU VENDOR : " + SystemInfo.graphicsDeviceVendor);
        strBuilder.AppendLine("GPU VENDOR ID : " + SystemInfo.graphicsDeviceVendorID.ToString());
        strBuilder.AppendLine("GPU VERSION : " + SystemInfo.graphicsDeviceVersion);
        strBuilder.AppendLine("GPU MEMORY : " + SystemInfo.graphicsMemorySize.ToString());
        strBuilder.AppendLine("GPU SHADER LEVEL : " + SystemInfo.graphicsShaderLevel.ToString());
        strBuilder.AppendLine("MAX TEX SIZE : " + SystemInfo.maxTextureSize.ToString());
        strBuilder.AppendLine("OS : " + SystemInfo.operatingSystem);
        strBuilder.AppendLine("CPU COUNT : " + SystemInfo.processorCount.ToString());
        strBuilder.AppendLine("CPU TYPE : " + SystemInfo.processorType);
        strBuilder.AppendLine("SYSTEM MEMORY : " + SystemInfo.systemMemorySize);
        return strBuilder.ToString();
    }
    #endregion    

    #region rules
    private const string RULES_DEFAULT_SKU = "L0";

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
                m_deviceQualityManager.Profiles_AddData(featureSettings.Profile, featureSettings.Rating, settingsJSON);
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

        // Checks if the latest server settings cached are still valid
        string serverLastVersion = Server_GetVersionCachedJSON();
        if (!string.IsNullOrEmpty(serverLastVersion))
        {
            string currentVersion = ServerManager.SharedInstance.GetServerConfig().m_strClientVersion;
            if (!currentVersion.Equals(serverLastVersion))
            {
                Server_ClearCache();
            }
        }

        // Retrieves the latest configuration received from server to apply it since the user might be playing offline
        JSONNode serverJSON = Server_GetCachedJSON();        
        SetupCurrentFeatureSettings(GetDeviceFeatureSettingsAsJSON(), serverJSON);
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

    public void SetupCurrentFeatureSettings(JSONNode deviceSettingsJSON, JSONNode serverSettingsJSON)
    {
        float rating = m_deviceQualityManager.Device_CalculatedRating;
        string profileName = null;

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
        }        

        // If no profileName is available for the device in iOS then we assume that it's a new device so its rating has to be the maximum
#if UNITY_IOS        
        if (string.IsNullOrEmpty(profileName))
        {
            rating = 1f;
        }        
#endif

        // Gets the FeatureSettings object of the profile that corresponds to the calculated rating
        if (string.IsNullOrEmpty(profileName))
        {
            profileName = m_deviceQualityManager.Profiles_RatingToProfileName(rating);
        }
        
        Device_CalculatedProfile = profileName;        

        // The config received from server has to override the profile
        if (serverSettingsJSON != null)
        {
            // Checks if the rating has been overriden for this device
            if (serverSettingsJSON.ContainsKey(FeatureSettings.KEY_RATING))
            {
                rating = serverSettingsJSON[FeatureSettings.KEY_RATING].AsFloat;
            }

            if (serverSettingsJSON.ContainsKey(FeatureSettings.KEY_PROFILE))
            {
                profileName = serverSettingsJSON[FeatureSettings.KEY_PROFILE];
            }
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

        // The configuration received from server has the maximum priority, so it overrides everything
        if (serverSettingsJSON != null)
        {
            Device_CurrentFeatureSettings.OverrideFromJSON(serverSettingsJSON);
        }

        ApplyCurrentFeatureSetting();
    }

    public void RestoreCurrentFeatureSettingsToDevice()
    {
        SetupCurrentFeatureSettings(GetDeviceFeatureSettingsAsJSON(), null);
    }

    private FeatureSettings CreateFeatureSettings()
    {
        return new FeatureSettings();
    }

    public void ApplyCurrentFeatureSetting()
    {
        ApplyFeatureSetting(Device_CurrentFeatureSettings);

        //JSONNode json = Device_CurrentFeatureSettings.ToJSON();
        //Debug.Log(json);
    }

    private int CurrentQualityIndex { get; set; }

    /// <summary>
    /// Returns whether or not this is a good moment to apply feature settings as it can change quality settings, which migh have an impact on performance. Typically we don't want to apply 
    /// them when ingame.
    /// </summary>
    /// <returns></returns>
    private bool ISApplyFeatureSettingsAllowed()
    {
        // It's not allowed to changing feature settings ingame because it might cause a halt
        return !FlowManager.IsInGameScene();
    }

    private void ApplyFeatureSetting(FeatureSettings settings)
    {
#if !UNITY_EDITOR
        FeatureSettings.EQualityLevelValues quality = settings.GetValueAsQualityLevel(FeatureSettings.KEY_QUALITY_LEVEL);
        int qualityIndex = (int)quality;
        if (qualityIndex != CurrentQualityIndex)
        {
            CurrentQualityIndex = qualityIndex;
            QualitySettings.SetQualityLevel(CurrentQualityIndex);
            Log(">> qualityLevel:" + quality.ToString() + " index = " + qualityIndex);
        }

        ApplyPhysicQuality(settings.Rating);
#endif

        FeatureSettings.ELevel3Values shadersLevel = settings.GetValueAsLevel3(FeatureSettings.KEY_SHADERS_LEVEL);
        Shaders_ApplyQuality(shadersLevel);

        Log("Device Rating:" + settings.Rating);
        Log("Profile:" + settings.Profile);
        Log(Device_GetInfo());
        Log(Shaders_GetInfo());
        Log(">> Time.fixedDeltaTime:" + Time.fixedDeltaTime);
    }

    private void ApplyPhysicQuality(float deviceRating)
    {
        float startValue = Rules_PhysicsMaxRating;
        if (deviceRating < startValue)
        {
            float perc = Mathf.Clamp01(deviceRating / startValue);
            float fixedTimeStep = Mathf.Lerp(0.025f, 0.01666666f, perc);
            Time.fixedDeltaTime = fixedTimeStep;
        }
    }

    private JSONNode FormatJSON(JSONNode json)
    {
        JSONNode returnValue = json;
        if (json != null)
        {
            JSONClass jsonClass = json as JSONClass;

            returnValue = new JSONClass();
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

    /// <summary>
    /// Returns whether or not debug mode is enabled. 
    /// It's a static method to be sure that it will be available at all times since it's looked up by some MonoBehaviours when awaking
    /// </summary>
    public static bool IsDebugEnabled
    {
        get
        {
            return UnityEngine.Debug.isDebugBuild;
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
            ServerManager.ServerConfig kServerConfig = ServerManager.SharedInstance.GetServerConfig();
            return (kServerConfig != null && kServerConfig.m_eBuildEnvironment != CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION);            
        }
    }

    public static bool IsControlPanelEnabled
    {
        get
        {
#if UNITY_EDITOR || true
            return true;
#else
            ServerManager.ServerConfig kServerConfig = ServerManager.SharedInstance.GetServerConfig();
            return (kServerConfig != null && kServerConfig.m_eBuildEnvironment != CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION);               
#endif
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

    public FeatureSettings.ELevel2Values EntitiesLOD
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsLevel2(FeatureSettings.KEY_ENTITIES_LOD);
        }
    }

    public FeatureSettings.ELevel3Values LevelsLOD
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsLevel3(FeatureSettings.KEY_LEVELS_LOD);
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

    public bool IsFogOnDemandEnabled { get; set; }    
    #endregion

    #region log
    private const string PREFIX = "FeatureSettingsManager:";

    public static void Log(string message)
    {
        Debug.Log(PREFIX + message);
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
        float rating = 0f;
        string profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating);
        Log("Rating: " + rating + " profile = " + profile);

        rating = 0.1f;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating);
        Log("Rating: " + rating + " profile = " + profile);

        rating = 0.3f;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating);
        Log("Rating: " + rating + " profile = " + profile);

        rating = 0.5f;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating);
        Log("Rating: " + rating + " profile = " + profile);

        rating = 0.7f;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating);
        Log("Rating: " + rating + " profile = " + profile);

        rating = 0.8f;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating);
        Log("Rating: " + rating + " profile = " + profile);

        rating = 0.85f;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating);
        Log("Rating: " + rating + " profile = " + profile);

        rating = 0.9f;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating);
        Log("Rating: " + rating + " profile = " + profile);

        rating = 1.0f;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating);
        Log("Rating: " + rating + " profile = " + profile);

        rating = 1.1f;
        profile = m_deviceQualityManager.Profiles_RatingToProfileName(rating);
        Log("Rating: " + rating + " profile = " + profile);
    }
    #endregion
}