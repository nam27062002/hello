// RemoveAdsOffer.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 16/09/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// By buying this offer the player wont see any ads and will have also special benefits.
/// This offer doesnt expire, once the player accquires it is forever.
/// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/%5BHD%5D+8.+Pay+To+Remove+Ads
/// </summary>
/// 
[Serializable]
public class RemoveAdsFeature {


    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    private DefinitionNode m_def;

    private bool m_isActive = false; // Whether the player has bought this offer or not

    // Cached values from the definition node
    private int m_easyMissionCooldownSkips;
    private int m_mediumMissionCooldownSkips;
    private int m_hardMissionCooldownSkips;
    private int m_easyMissionCooldownTimeSecs;
    private int m_mediumMissionCooldownTimeSecs;
    private int m_hardMissionCooldownTimeSecs;

    private int m_mapRevealDurationSecs;
    private int m_mapRevealCooldownSecs;

    private int m_freeRevives;


    // Current offer data
    private int m_easyMissionCooldownLeft;
    private int m_mediumMissionCooldownLeft;
    private int m_hardMissionCooldownLeft;
    private int m_revivesLeft;

    // Timers
    private DateTime m_easyMissionCooldownTimestamp;    // Timestamp when the mission skips will be restored
    private DateTime m_mediumMissionCooldownTimestamp;
    private DateTime m_hardMissionCooldownTimestamp;
    private DateTime m_mapRevealTimestamp;      // Timestamp when the map reveal power will be restored


    // Getters/Setters
    #region GettersSetters
    
    public bool IsActive
    { get
        {
            return m_isActive;
        }
        set
        {
            m_isActive = value;
        }
    }

    public int easyMissionCooldownsLeft
    { get
        {
            return m_easyMissionCooldownLeft;
        }
        set
        {
            m_easyMissionCooldownLeft = value;
        }
    }

    public int mediumMissionCooldownsLeft
    { get
        {
            return m_mediumMissionCooldownLeft;
        }
        set
        {
            m_mediumMissionCooldownLeft = value;
        }
    }

    public int hardMissionCooldownsLeft
    { get
        {
            return m_hardMissionCooldownLeft;
        }
        set
        {
            m_hardMissionCooldownLeft = value;
        }
    }

    public int revivesLeft
    { get
        {
            return m_revivesLeft;
        }
        set
        {
            m_revivesLeft = value;
        }
    }

    public DateTime easyMissionCooldownTimestamp
    { get
        {
            return m_easyMissionCooldownTimestamp;
        }
        set
        {
            m_easyMissionCooldownTimestamp = value;
        }
    }

    public DateTime mediumMissionCooldownTimestamp
    { get
        {
            return m_mediumMissionCooldownTimestamp;
        }
        set
        {
            m_mediumMissionCooldownTimestamp = value;
        }
    }

    public DateTime hardMissionCooldownTimestamp
    { get
        {
            return m_hardMissionCooldownTimestamp;
        }
        set
        {
            m_hardMissionCooldownTimestamp = value;
        }
    }

    public DateTime mapRevealTimestamp
    { get
        {
            return m_mapRevealTimestamp;
        }
        set
        {
            m_mapRevealTimestamp = value;
        }
    }
    #endregion


    //------------------------------------------------------------------------//
    // STATIC   															  //
    //------------------------------------------------------------------------//


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    public RemoveAdsFeature()
    {
    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~RemoveAdsFeature()
    {
    }




    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    
    /// <summary>
    /// Initialize the happy hour with the values in the definition node. 
    /// </summary>
    public void InitializeFromDefinition()
    {
        DefinitionNode def;
        List<DefinitionNode> definitions = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.REMOVE_ADS_OFFER);

        // No definition found
        if (definitions.Count == 0)
        {
            Debug.LogError("Couldn't find any definition of Remove Ads offer in the content");
            return;
        }

        // Happy hours definitions only should contain one row
        def = definitions[0];

        InitializeFromDefinition(def);
    }

    /// <summary>
    /// Initialize the happy hour with the values in the definition node. 
    /// </summary>
    public void InitializeFromDefinition(DefinitionNode _def)
    {

        m_def = _def;

        // Initialize definition values from definition
        m_isActive = _def.GetAsBool("isActive");
        m_easyMissionCooldownSkips = _def.GetAsInt("easyMissionCooldownSkips");
        m_mediumMissionCooldownSkips = _def.GetAsInt("mediumMissionCooldownSkips");
        m_hardMissionCooldownSkips = _def.GetAsInt("hardMissionCooldownSkips");
        m_easyMissionCooldownTimeSecs = _def.GetAsInt("easyMissionCooldownTimeSecs");
        m_mediumMissionCooldownTimeSecs = _def.GetAsInt("mediumMissionCooldownTimeSecs"); ;
        m_hardMissionCooldownTimeSecs = _def.GetAsInt("hardMissionCooldownTimeSecs"); ;

        m_mapRevealDurationSecs = _def.GetAsInt("mapRevealDurationSecs"); ;
        m_mapRevealCooldownSecs = _def.GetAsInt("mapRevealCooldownSecs"); ;

        m_freeRevives = _def.GetAsInt("freeRevives"); ;

    }


    /// <summary>
    /// Enables/disables the remove ads feature
    /// </summary>
    public void SetActive(bool active)
    {
        m_isActive = active;
    }



    //------------------------------------------------------------------//
    // PERSISTENCE														//
    //------------------------------------------------------------------//
    /// <summary>
    /// Load state from a json object.
    /// </summary>
    /// <param name="_data">The data object loaded from persistence.</param>
    public void Load(SimpleJSON.JSONNode _data)
    {
        string key = "isActive";
        if (_data.ContainsKey(key))
        {
            m_isActive = _data[key].AsBool;
        }

        key = "easyMissionCooldownLeft";
        if (_data.ContainsKey(key))
        {
            m_easyMissionCooldownLeft = _data[key].AsInt;
        }

        key = "mediumMissionCooldownLeft";
        if (_data.ContainsKey(key))
        {
            m_mediumMissionCooldownLeft = _data[key].AsInt;
        }

        key = "hardMissionCooldownLeft";
        if (_data.ContainsKey(key))
        {
            m_hardMissionCooldownLeft = _data[key].AsInt;
        }

        key = "easyMissionCooldownTimestamp";
        if (_data.ContainsKey(key))
        {
            m_easyMissionCooldownTimestamp = new DateTime(_data[key].AsLong);
        }
        else
        {
            m_easyMissionCooldownTimestamp = DateTime.MinValue;
        }

        key = "mediumMissionCooldownTimestamp";
        if (_data.ContainsKey(key))
        {
            m_mediumMissionCooldownTimestamp = new DateTime(_data[key].AsLong);
        }
        else
        {
            m_mediumMissionCooldownTimestamp = DateTime.MinValue;
        }

        key = "hardMissionCooldownTimestamp";
        if (_data.ContainsKey(key))
        {
            m_hardMissionCooldownTimestamp = new DateTime(_data[key].AsLong);
        }
        else
        {
            m_hardMissionCooldownTimestamp = DateTime.MinValue;
        }

        key = "mapRevealTimestamp";
        if (_data.ContainsKey(key))
        {
            m_mapRevealTimestamp = new DateTime(_data[key].AsLong);
        }
        else
        {
            m_mapRevealTimestamp = DateTime.MinValue;
        }
    }

    /// <summary>
    /// Create and return a persistence save data object initialized with the data.
    /// </summary>
    /// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
    public SimpleJSON.JSONNode Save()
    {
        SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
        data.Add("isActive", m_isActive);
        data.Add("easyMissionCooldownLeft", m_easyMissionCooldownLeft);
        data.Add("mediumMissionCooldownLeft", m_mediumMissionCooldownLeft);
        data.Add("hardMissionCooldownLeft", m_hardMissionCooldownLeft);

        data.Add("easyMissionCooldownTimestamp", m_easyMissionCooldownTimestamp.Ticks.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
        data.Add("mediumMissionCooldownTimestamp", m_mediumMissionCooldownTimestamp.Ticks.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
        data.Add("hardMissionCooldownTimestamp", m_hardMissionCooldownTimestamp.Ticks.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
        data.Add("mapRevealTimestamp", m_mapRevealTimestamp.Ticks.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));

        return data;
    }
    
}