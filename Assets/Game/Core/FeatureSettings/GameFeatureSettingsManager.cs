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

        SetupFeatureSettings();        
    }   

    public void SetupFeatureSettings()
    {
        // The device rating is calculated
        float rating = CalculateRating();
        m_deviceQualityManager.Device_CalculatedRating = rating;

        string profileName = null;

        // Checks if there's a definition for the device in content
        JSONNode deviceSettingsJSON = GetDeviceFeatureSettingsAsJSON();
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

        Device_CurrentFeatureSettings = CreateFeatureSettings();

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

    private float CalculateRating()
    {
        return 1f;
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