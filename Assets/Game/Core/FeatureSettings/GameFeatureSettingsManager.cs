// GameFeatureSettingsManager.cs
// Hungry Dragon
// 
// Created by David Germade
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// This class is used as a facade that returns the configuration for all features in the game. 
/// This class uses <c>DeviceQualityManager</c> to retrieve the configuration of these features taking into consideration the quality of the device
/// </summary>
public class GameFeatureSettingsManager : UbiBCN.SingletonMonoBehaviour<GameFeatureSettingsManager>
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

        m_deviceQualityManager = new DeviceQualityManager();

        if (ContentManager.ready)
        {
            OnDefinitionsLoaded();
        }

        Messenger.AddListener(EngineEvents.DEFINITIONS_LOADED, OnDefinitionsLoaded);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (m_deviceQualityManager != null)
        {
            m_deviceQualityManager.Clear();
        }

        Messenger.RemoveListener(EngineEvents.DEFINITIONS_LOADED, OnDefinitionsLoaded);
    }    

    public string Device_Model { get; set; }    

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

    public GameFeatureSettings Device_CurrentFeatureSettings { get; set; }

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
            return Device_CurrentFeatureSettings.GetValueAsBool(GameFeatureSettings.KEY_GLOW);
        }
    }

    private void OnDefinitionsLoaded()
    {
        m_deviceQualityManager.Profiles_Clear();

        DefinitionsManager defManager = DefinitionsManager.SharedInstance;

        // All profiles are loaded from rules           
        GameFeatureSettings featureSettings = CreateFeatureSettings();    // Helper
        JSONNode settingsJSON;
        Dictionary<string, DefinitionNode> definitions = defManager.GetDefinitions(DefinitionsCategory.FEATURE_PROFILE_SETTINGS);
        foreach (KeyValuePair<string, DefinitionNode> pair in definitions)
        {
            settingsJSON = pair.Value.ToJSON();            
            settingsJSON = FormatJSON(settingsJSON);
            featureSettings.FromJSON(settingsJSON);
            m_deviceQualityManager.Profiles_AddData(featureSettings.Profile, featureSettings.Rating, settingsJSON);
        }

        // The device rating is calculated
        float rating = CalculateRating();
        m_deviceQualityManager.Device_CalculatedRating = rating;

        Device_CurrentFeatureSettings = CreateFeatureSettings();

        SetupCurrentFeatureSettings(null);        
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

        // Gets the FeatureSettings object of the profile that corresponds to the calculated rating
        if (string.IsNullOrEmpty(profileName))
        {
            profileName = m_deviceQualityManager.Profiles_RatingToProfileName(rating);
        }

        Device_CurrentProfile = profileName;
        Device_CurrentFeatureSettings.Rating = rating;

        // We need to override the default configuration of the profile with the particular configuration defined for the device, if there's one
        if (deviceSettingsJSON != null)
        {
            Device_CurrentFeatureSettings.OverrideFromJSON(deviceSettingsJSON);
        }
    }    

    private GameFeatureSettings CreateFeatureSettings()
    {
        return new GameFeatureSettings();
    }

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

    private float CalculateRating()
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

    public virtual JSONNode FormatJSON(JSONNode json)
    {
        JSONNode returnValue = json;        
        if (json != null)
        {
            JSONClass jsonClass = json as JSONClass;

            returnValue = new JSONClass();            
            foreach (KeyValuePair<string, JSONNode> pair in jsonClass.m_Dict)
            {
                // Empty keys or keys with "as_profile" as a value need to be ignored
                if (!string.IsNullOrEmpty(pair.Value.Value) && pair.Value.Value != "as_profile")
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
}