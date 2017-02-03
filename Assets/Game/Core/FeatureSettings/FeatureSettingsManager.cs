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
                if (Server_QualitySettingsJSON == null)
                {
                    LogError("JSON received from server can't be null");
                    State = EState.Done;
                }
                else if (ISApplyFeatureSettingsAllowed())
                {                    
                    SetupCurrentFeatureSettings(Server_QualitySettingsJSON);                    
                    State = EState.Done;
                }
                break;

            case EState.WaitingForTheUserToLogInToUpload:                
                if (GameSessionManager.SharedInstance.IsLogged())
                {
                    State = EState.Done;
                    Server_UploadQualitySettings();
                }                
                else
                {
                    // If the time given to wait for the user to log in has expired then we just ignore that request from server
                    State_Timer -= Time.deltaTime;
                    if (State_Timer <= 0f)
                    {
                        State = EState.Done;
                    }
                }
                break;
        }       
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
        WaitingForTheUserToLogInToUpload,       // Waiting for the user to log in once the client has been told to upload its quality settings information to the server
        Done                                    // No more stuff has to be done once server quality settings have been applied
    }

    private EState m_state;
    private EState State
    {
        get
        {
            return m_state;
        }

        set
        {
            m_state = value;

            switch(m_state)
            {
                case EState.WaitingForTheUserToLogInToUpload:
                    State_Timer = SERVER_TIME_TO_WAIT_FOR_LOGIN_WHEN_TOLD_TO_UPLOAD;
                    break;
            }
        }
    }

    /// <summary>
    /// General purpose timer to use by the current state
    /// </summary>
    private float State_Timer { get; set; }
    #endregion

    #region server
    // This region is responsible for sending related to quality settings request to the server and for receiving the responses
    // The response for the quality settings request can be null if the server doesn't have that information. If so then the client has to provide the server with that information.
    // In order to be allowed to do so the user has to be logged. 

    private float SERVER_TIME_TO_WAIT_BETWEEN_CONNECTION_CHECKS = 5f;

    /// <summary>
    /// Time to wat for the user to log in once the client has been told to upload its quality settings information to the server
    /// </summary>
    private float SERVER_TIME_TO_WAIT_FOR_LOGIN_WHEN_TOLD_TO_UPLOAD = 20f * 60f;

    private JSONNode Server_QualitySettingsJSON { get; set; }    

    private void Server_Reset()
    {
        Server_QualitySettingsJSON = null;        
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

                // If there's no data then we need to upload the profile calculated to the server when as long as the user logs in
                if (qualitySettings == null)
                {
                    State = EState.WaitingForTheUserToLogInToUpload;                    
                }
                else
                {
                    JSONNode json = JSONNode.Parse(qualitySettings);
                    Server_QualitySettingsJSON = FormatJSON(json);
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

    private void Server_UploadQualitySettings()
    {
        // Only the profile has to be sent to the server
        JSONNode json = new JSONClass();
        json.Add("profile", Device_CurrentProfile);
        string data = "{\"profile\":\"very_low\"}";

        GameServerManager.SharedInstance.SetQualitySettings(data, delegate (Error error, Dictionary<string, object> response)
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
    }
    #endregion

    #region device
    public string Device_Model { get; set; }

    private float Device_CalculateRating()
    {
        //Average the devices RAM, CPU and GPU details to give a rating betwen 0 and 1
        float finalDeviceRating = 0.0f;

        Dictionary<string, DefinitionNode> definitions = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.DEVICE_RATING_SETTINGS);
        List<DeviceSettings> memoryData = new List<DeviceSettings>();
        List<DeviceSettings> cpuCoresData = new List<DeviceSettings>();
        List<DeviceSettings> gpuMemoryData = new List<DeviceSettings>();
        List<DeviceSettings> textureSizeData = new List<DeviceSettings>();

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
                data = new DeviceSettings(pair.Value.GetAsFloat(key), pair.Value.GetAsFloat("cpuCoresBoundary"));
                gpuMemoryData.Add(data);
            }

            key = "textureSizeRating";
            if (pair.Value.Has(key))
            {
                data = new DeviceSettings(pair.Value.GetAsFloat(key), pair.Value.GetAsFloat("textureSizeBoundary"));
                textureSizeData.Add(data);
            }
        }

        // Lists are sorted in ascendent order of rating
        memoryData.Sort(DeviceSettings.Sort);
        cpuCoresData.Sort(DeviceSettings.Sort);
        gpuMemoryData.Sort(DeviceSettings.Sort);
        textureSizeData.Sort(DeviceSettings.Sort);

        // Memory rating
        float memQualityRating = 1.0f;
        int deviceCapacity = SystemInfo.systemMemorySize;
        foreach (DeviceSettings ds in memoryData)
        {
            if (deviceCapacity <= ds.Boundary)
            {
                memQualityRating = ds.Rating;
                break;
            }
        }

        // cpu rating
        float cpuQualityRating = 1.0f;
        deviceCapacity = SystemInfo.processorCount;
        foreach (DeviceSettings ds in cpuCoresData)
        {
            if (SystemInfo.processorCount <= ds.Boundary)
            {
                cpuQualityRating = ds.Rating;
                break;
            }
        }

        float gpuMemQualityLevel = 1.0f;
        deviceCapacity = SystemInfo.graphicsMemorySize;
        foreach (DeviceSettings ds in gpuMemoryData)
        {
            if (deviceCapacity <= ds.Boundary)
            {
                gpuMemQualityLevel = ds.Rating;
                break;
            }
        }

        float texSizeQualityLevel = 1.0f;
        deviceCapacity = SystemInfo.maxTextureSize;
        foreach (DeviceSettings ds in textureSizeData)
        {
            if (deviceCapacity <= ds.Boundary)
            {
                texSizeQualityLevel = ds.Rating;
                break;
            }
        }

        float shaderMultiplier = 1.0f;
        if (SystemInfo.graphicsShaderLevel == 20)
        {
            shaderMultiplier = 0.0f;
        }

        //UnityEngine.Debug.Log("DeviceQualitySettings(Graphics) - gpuMemQualityLevel: " + gpuMemQualityLevel + " texSizeQualityLevel: " + texSizeQualityLevel + " shaderMultiplier: " + shaderMultiplier);

        //float gpuQualityRating = ((gpuMemQualityLevel + texSizeQualityLevel) / 2);

        //UnityEngine.Debug.Log("DeviceQualitySettings - memQualityLevel: " + memQualityRating + " cpuQualityRating: " + cpuQualityRating + " gpuMemQualityLevel: " + gpuQualityRating);

        finalDeviceRating = ((memQualityRating + cpuQualityRating + gpuMemQualityLevel) / 3) * shaderMultiplier;
        finalDeviceRating = Mathf.Clamp(finalDeviceRating, 0.0f, 1.0f);

        //UnityEngine.Debug.Log( "DeviceQualitySettings - Final Value: " + finalDeviceRating );

        return finalDeviceRating;
    }

    public float Device_CalculatedRating
    {
        get
        {
            return m_deviceQualityManager.Device_CalculatedRating;
        }
    }

    public float Device_CurrentRating
    {
        get
        {
            return Device_CurrentFeatureSettings.Rating;
        }
    }

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

        // All profiles are loaded from rules           
        FeatureSettings featureSettings = FeatureSettingsHelper;    // Helper
        JSONNode settingsJSON;
        Dictionary<string, DefinitionNode> definitions = defManager.GetDefinitions(DefinitionsCategory.FEATURE_PROFILE_SETTINGS);
        foreach (KeyValuePair<string, DefinitionNode> pair in definitions)
        {
            settingsJSON = pair.Value.ToJSON();
            settingsJSON = FormatJSON(settingsJSON);
            settingsJSON = featureSettings.ParseJSON(settingsJSON);
            featureSettings.FromJSON(settingsJSON);
            m_deviceQualityManager.Profiles_AddData(featureSettings.Profile, featureSettings.Rating, settingsJSON);
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
            settingsJSON = featureSettings.ParseJSON(settingsJSON);            

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

        // The device rating is calculated
        float rating = Device_CalculateRating();
        m_deviceQualityManager.Device_CalculatedRating = rating;

        Device_CurrentFeatureSettings = CreateFeatureSettings();

        SetupCurrentFeatureSettings(null);
    }
    #endregion

    #region shaders
    // List of shader variant collections to warm up. They have to be sorted in ascendent order (the lowest quality one first)
    public ShaderVariantCollection[] m_shadersVariantCollections;

    private const string SHADERS_KEY_HIGH = "HI_DETAIL_ON";
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

    public void SetupCurrentFeatureSettings(JSONNode deviceSettingsJSON)
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

        Device_CurrentProfile = profileName;

        // In iOS the rating of the device has to be the rating of the profile since the calculated rating is used only as a reference.         
#if !UNITY_IOS             
        Device_CurrentFeatureSettings.Rating = rating;
#endif

        // We need to override the default configuration of the profile with the particular configuration defined for the device, if there's one
        if (deviceSettingsJSON != null)
        {
            Device_CurrentFeatureSettings.OverrideFromJSON(deviceSettingsJSON);
        }

        ApplyCurrentFeatureSetting();
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
        return true;
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
                if (pair.Key == "iOSGeneration")
                    continue;
                             
                // Empty keys or keys with "as_profile" as a value need to be ignored
                if (!string.IsNullOrEmpty(pair.Value.Value) && pair.Value.Value != "as_content")
                {
                    returnValue.Add(pair.Key, pair.Value);
                }
            }
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

    #region facade
    public List<string> Profiles_Names
    {
        get
        {
            return m_deviceQualityManager.Profiles_Names;
        }
    }

    public bool IsGlowEnabled
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsBool(FeatureSettings.KEY_GLOW);
        }
    }

    public FeatureSettings.ELevel2Values EntitiesLOD
    {
        get
        {
            return Device_CurrentFeatureSettings.GetValueAsLevel2(FeatureSettings.KEY_ENTITIES_LOD);
        }
    }
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
    #endregion
}