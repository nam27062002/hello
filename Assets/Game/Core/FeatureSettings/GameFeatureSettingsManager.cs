// GameFeatureSettingsManager.cs
// Hungry Dragon
// 
// Created by David Germade
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections.Generic;

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

        // All profiles are loaded from rules        
        GameFeatureSettings featureSettings;
        Dictionary<string, DefinitionNode> definitions = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.FEATURE_PROFILE_SETTINGS);
        foreach (KeyValuePair<string, DefinitionNode> pair in definitions)
        {
            featureSettings = CreateFeatureSettings();
            SimpleJSON.JSONNode json = pair.Value.ToJSON();
            featureSettings.FromJSON(json);
            m_deviceQualityManager.Profiles_AddFeatureSettings(pair.Key, featureSettings);
        }

        m_deviceQualityManager.Device_CalculatedRating = 1f;
        FeatureSettings profileSettings = m_deviceQualityManager.Profiles_GetFeatureSettingsPerRating(m_deviceQualityManager.Device_CalculatedRating);

        // A new HDFeatureSettings object is created instead of using the profile feature settings one because it might be changed and we want the profile one to stay the same
        GameFeatureSettings deviceSettings = CreateFeatureSettings();
        deviceSettings.FromJSON(profileSettings.ToJSON());
        m_deviceQualityManager.Device_CurrentFeatureSettings = deviceSettings;
    }

    private GameFeatureSettings CreateFeatureSettings()
    {
        return new GameFeatureSettings();
    }
}