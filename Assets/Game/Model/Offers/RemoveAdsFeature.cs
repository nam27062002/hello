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
    private int m_easyMissionCooldownSkips;     // Amount of cooldown skips
    private int m_mediumMissionCooldownSkips;
    private int m_hardMissionCooldownSkips;
    private int m_easyMissionCooldownTimeSecs;      // Duration of the cooldown of the skip-mission-cooldown feature
    private int m_mediumMissionCooldownTimeSecs;
    private int m_hardMissionCooldownTimeSecs;

    private int m_mapRevealDurationSecs;
    private int m_mapRevealCooldownSecs;

    private int m_freeRevives;


    // Current offer data
    private int m_easyMissionSkipsLeft;
    private int m_mediumMissionSkipsLeft;
    private int m_hardMissionSkipsLeft;
    private int m_revivesLeft;

    // Timers
    private DateTime m_easyMissionCooldownTimestamp = DateTime.MinValue;    // Timestamp when the mission skips will be restored
    private DateTime m_mediumMissionCooldownTimestamp = DateTime.MinValue;
    private DateTime m_hardMissionCooldownTimestamp = DateTime.MinValue;
    private DateTime m_mapRevealTimestamp = DateTime.MinValue;      // Timestamp when the map reveal power will be restored

    // Delegates
    public delegate void RefreshPill(bool animate);
    public RefreshPill refreshMapPill;


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

    public int easyMissionSkipsLeft
    { get
        {
            return m_easyMissionSkipsLeft;
        }
        set
        {
            m_easyMissionSkipsLeft = value;
        }
    }

    public int mediumMissionSkipsLeft
    { get
        {
            return m_mediumMissionSkipsLeft;
        }
        set
        {
            m_mediumMissionSkipsLeft = value;
        }
    }

    public int hardMissionSkipsLeft
    { get
        {
            return m_hardMissionSkipsLeft;
        }
        set
        {
            m_hardMissionSkipsLeft = value;
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

    public int mapRevealDurationSecs
    {
        get
        {
            return m_mapRevealDurationSecs;
        }

        set
        {
            m_mapRevealDurationSecs = value;
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

    /// <summary>
    /// Check if some cooldown has finished and call the delegated refreshes if needed
    /// </summary>
    public void Update()
    {
        // Will be called every frame, so dont waste time
        if (!IsActive) return;

        // Check if the map reveal feature is ready
        if (m_mapRevealTimestamp != DateTime.MinValue && DateTime.Now > m_mapRevealTimestamp )
        {
            m_mapRevealTimestamp = DateTime.MinValue;

            if (refreshMapPill != null)
            {
                // Call the map pill refresh
                refreshMapPill(true);
            }
        }
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
        // If already active, do nothing.
        if (active && m_isActive)
            return;

        if (active)
        {
            // The player just bought the offer, initialize the values
            ResetValues();
        }

        m_isActive = active;
    }

    /// <summary>
    /// Reset all the values of the feature
    /// </summary>
    private void ResetValues() {
     
        // Initialize values 
        InitializeFromDefinition();

        // Reset cooldowns
        m_mapRevealTimestamp = DateTime.MinValue;
    }
    


    /// <summary>
    /// When the user owns the Remove Ads feature, has a limited amount of
    /// mission cooldown skips for each difficulty of missions.
    /// </summary>
    /// <param name="difficulty">Difficulty of the mission</param>
    /// <returns>The amount of skips available for the current mission</returns>
    public int GetMissionCooldownSkipsLeft(Mission.Difficulty difficulty)
    {

        switch (difficulty)
        {
            case Mission.Difficulty.EASY:
                return UsersManager.currentUser.removeAds.m_easyMissionSkipsLeft;
            case Mission.Difficulty.MEDIUM:
                return UsersManager.currentUser.removeAds.m_mediumMissionSkipsLeft;
            case Mission.Difficulty.HARD:
                return UsersManager.currentUser.removeAds.m_hardMissionSkipsLeft;
            default:    // Shouldnt happen
                return 0;
        }
    }


    /// <summary>
    /// When the user owns the Remove Ads feature, has a limited amount of
    /// mission cooldown skips for each difficulty of missions.
    /// </summary>
    /// <param name="difficulty">Difficulty of the mission</param>
    /// <returns>The time left in seconds to restore the skips</returns>
    public double GetMissionCooldownSkipsTimeLeft(Mission.Difficulty difficulty)
    {

        DateTime cooldownTimestamp;
        switch (difficulty)
        {
            case Mission.Difficulty.EASY:
                cooldownTimestamp = UsersManager.currentUser.removeAds.m_easyMissionCooldownTimestamp;
                break;
            case Mission.Difficulty.MEDIUM:
                cooldownTimestamp = UsersManager.currentUser.removeAds.m_mediumMissionCooldownTimestamp;
                break;
            case Mission.Difficulty.HARD:
                cooldownTimestamp = UsersManager.currentUser.removeAds.m_hardMissionCooldownTimestamp;
                break;
            default:    // Shouldnt happen
                return 0;
        }

        // Remaining time to restore the skip buttons
        TimeSpan diff = cooldownTimestamp - DateTime.Now ;

        return diff.TotalSeconds;
    }


    ///<summary>
    /// Used when the player skips the cooldown of a mission
    ///</summary>
    /// <param name="difficulty">Difficulty of the mission</param>
    /// <returns>Returns false if the player doesnt have enough skips left</returns>
    public bool UseSkip (Mission.Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Mission.Difficulty.EASY:
                if (m_easyMissionSkipsLeft > 0)
                {
                    m_easyMissionSkipsLeft --;
                    if (m_easyMissionSkipsLeft == 0)
                    {
                        // No more skips left. Initialize the cooldown timer.
                        m_easyMissionCooldownTimestamp = DateTime.Now + new TimeSpan(0, 0, m_easyMissionCooldownTimeSecs);
                    }
                    return true;
                }
                return false;
            case Mission.Difficulty.MEDIUM:
                if (m_mediumMissionSkipsLeft > 0)
                {
                    m_mediumMissionSkipsLeft--;
                    if (m_mediumMissionSkipsLeft == 0)
                    {
                        // No more skips left. Initialize the cooldown timer.
                        m_mediumMissionCooldownTimestamp = DateTime.Now + new TimeSpan(0, 0, m_mediumMissionCooldownTimeSecs);
                    }
                    return true;
                }
                return false;
            case Mission.Difficulty.HARD:
                if (m_hardMissionSkipsLeft > 0)
                {
                    m_hardMissionSkipsLeft--;
                    if (m_hardMissionSkipsLeft == 0)
                    {
                        // No more skips left. Initialize the cooldown timer.
                        m_hardMissionCooldownTimestamp = DateTime.Now + new TimeSpan(0, 0, m_hardMissionCooldownTimeSecs);
                    }
                    return true;
                }
                return false;
        }

        return false; // Default case. Shouldnt happen.

    }

    ///<summary>
    /// Used when the player reveals the map
    ///</summary>
    /// <returns>Returns false if map reveal is not ready yet</returns>
    public bool UseMapReveal()
    {

        if (IsMapRevealAvailable ())
        {
            // Initialize the map reveal cooldown
            m_mapRevealTimestamp = DateTime.Now + new TimeSpan(0, 0, m_mapRevealCooldownSecs);
            return true;
        }

        // Map reveal wasnt ready yet
        return false;
     
    }

    /// <summary>
    /// Whether the map reveal feature is ready or not.
    /// </summary>
    /// <returns>True if ready, false if the cooldown has not finished yet</returns>
    public bool IsMapRevealAvailable ()
    {
        return (m_mapRevealTimestamp == DateTime.MinValue ||
                m_mapRevealTimestamp < DateTime.Now);
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

        key = "easyMissionSkipsLeft";
        if (_data.ContainsKey(key))
        {
            m_easyMissionSkipsLeft = _data[key].AsInt;
        }

        key = "mediumMissionSkipsLeft";
        if (_data.ContainsKey(key))
        {
            m_mediumMissionSkipsLeft = _data[key].AsInt;
        }

        key = "hardMissionSkipsLeft";
        if (_data.ContainsKey(key))
        {
            m_hardMissionSkipsLeft = _data[key].AsInt;
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
        data.Add("easyMissionSkipsLeft", m_easyMissionSkipsLeft);
        data.Add("mediumMissionSkipsLeft", m_mediumMissionSkipsLeft);
        data.Add("hardMissionSkipsLeft", m_hardMissionSkipsLeft);

        data.Add("easyMissionCooldownTimestamp", m_easyMissionCooldownTimestamp.Ticks.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
        data.Add("mediumMissionCooldownTimestamp", m_mediumMissionCooldownTimestamp.Ticks.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
        data.Add("hardMissionCooldownTimestamp", m_hardMissionCooldownTimestamp.Ticks.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
        data.Add("mapRevealTimestamp", m_mapRevealTimestamp.Ticks.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));

        return data;
    }
    
}