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

    public GameFeatureSettings Device_CurrentFeatureSettings
    {
        get
        {
            return (m_deviceQualityManager == null) ? null : m_deviceQualityManager.Device_CurrentFeatureSettings as GameFeatureSettings;
        }
    }

    public string Device_Model { get; set; }

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
        GameFeatureSettings featureSettings;
        Dictionary<string, DefinitionNode> definitions = defManager.GetDefinitions(DefinitionsCategory.FEATURE_PROFILE_SETTINGS);
        foreach (KeyValuePair<string, DefinitionNode> pair in definitions)
        {
            featureSettings = CreateFeatureSettings();
            SimpleJSON.JSONNode json = pair.Value.ToJSON();
            featureSettings.FromJSON(json);
            m_deviceQualityManager.Profiles_AddFeatureSettings(pair.Key, featureSettings);
        }

        // The device rating is calculated
        float rating = CalculateRating();
        m_deviceQualityManager.Device_CalculatedRating = rating;

        // Checks if there's a definition for the device in content
        JSONNode deviceSettingsJSON = GetDeviceFeatureSettingsAsJSON();        
        if (deviceSettingsJSON != null)
        {
            // Checks if the rating has been overriden for this device
            if (deviceSettingsJSON.ContainsKey(FeatureSettings.KEY_RATING))
            {
                rating = deviceSettingsJSON[FeatureSettings.KEY_RATING].AsFloat;
            }
        }

        // Gets the FeatureSettings object of the profile that corresponds to the calculated rating
        string profileName = m_deviceQualityManager.Profiles_RatingToProfileName(rating);
        Device_SetProfileFeatureSettings(profileName);
        
        // We need to override the default configuration of the profile with the particular configuration defined for the device, if there's one
        if (deviceSettingsJSON != null)
        {
            m_deviceQualityManager.Device_CurrentFeatureSettings.OverrideFromJSON(deviceSettingsJSON);
        }

        Debug.Log(m_deviceQualityManager.Device_CurrentFeatureSettings.ToJSON());      
    }

    public void Device_SetProfileFeatureSettings(string profileName)
    {
        FeatureSettings profileSettigns = m_deviceQualityManager.Profiles_GetFeatureSettings(profileName);
        if (profileSettigns == null)
        {
            DeviceQualityManager.LogError("No feature settings found for profile " + profileName);
        }
        else
        {
            // We need Device_CurrentFeatureSettings to be a separated object from the one that we have for the profile because the configuration for the device
            // might be slightly different to the profile one 
            if (m_deviceQualityManager.Device_CurrentFeatureSettings == null)
            {
                m_deviceQualityManager.Device_CurrentFeatureSettings = CreateFeatureSettings();
            }
            else
            {
                m_deviceQualityManager.Device_CurrentFeatureSettings.Reset();
            }

            FeatureSettings settings = m_deviceQualityManager.Device_CurrentFeatureSettings;
            settings.FromJSON(profileSettigns.ToJSON());            
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

        return returnValue;
    }
}