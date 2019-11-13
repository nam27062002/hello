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
    private int m_easyExtraMissions;     // Amount of cooldown skips
    private int m_mediumExtraMissions;
    private int m_hardExtraMissions;
    private float m_easyMissionCooldownMultiplier;      // Duration of the cooldown of the skip-mission-cooldown feature
    private float m_mediumMissionCooldownMultiplier;
    private float m_hardMissionCooldownMultiplier;

    private int m_mapRevealDurationSecs;
    private int m_mapRevealCooldownSecs;

    private int m_freeRevives;


    // Current player data
    private int m_easyExtraMissionsLeft;
    private int m_mediumExtraMissionsLeft;
    private int m_hardExtraMissionsLeft;
    private int m_revivesLeft;

    // Timers
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

    public int easyExtraMissionsLeft
    { get
        {
            return m_easyExtraMissionsLeft;
        }
        set
        {
            m_easyExtraMissionsLeft = value;
        }
    }

    public int mediumExtraMissionsLeft
    { get
        {
            return m_mediumExtraMissionsLeft;
        }
        set
        {
            m_mediumExtraMissionsLeft = value;
        }
    }

    public int hardExtraMissionsLeft
    { get
        {
            return m_hardExtraMissionsLeft;
        }
        set
        {
            m_hardExtraMissionsLeft = value;
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

        Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);

    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~RemoveAdsFeature()
    {
        Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
    }

    /// <summary>
    /// Check if some cooldown has finished and call the delegated refreshes if needed
    /// </summary>
    public void Update()
    {
        // Will be called every frame, so dont waste time
        if (!IsActive) return;

        // Check if the map reveal feature is ready
        if (m_mapRevealTimestamp != DateTime.MinValue && GameServerManager.SharedInstance.GetEstimatedServerTime() > m_mapRevealTimestamp )
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
        m_easyExtraMissions = _def.GetAsInt("easyExtraMissions");
        m_mediumExtraMissions = _def.GetAsInt("mediumExtraMissions");
        m_hardExtraMissions = _def.GetAsInt("hardExtraMissions");
        m_easyMissionCooldownMultiplier = _def.GetAsFloat("easyMissionCooldownMultiplier");
        m_mediumMissionCooldownMultiplier = _def.GetAsFloat("mediumMissionCooldownMultiplier");
        m_hardMissionCooldownMultiplier = _def.GetAsFloat("hardMissionCooldownMultiplier");

        m_mapRevealDurationSecs = _def.GetAsInt("mapRevealDurationSecs");
        m_mapRevealCooldownSecs = _def.GetAsInt("mapRevealCooldownSecs");

        m_freeRevives = _def.GetAsInt("freeRevives");

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

        // Set initial values
        m_easyExtraMissionsLeft = m_easyExtraMissions;
        m_mediumExtraMissionsLeft = m_mediumExtraMissions;
        m_hardExtraMissionsLeft = m_hardExtraMissions;

        // Reset cooldowns
        m_mapRevealTimestamp = DateTime.MinValue;


    }
    


    /// <summary>
    /// When the user owns the Remove Ads feature, has a limited amount of
    /// mission cooldown skips for each difficulty of missions.
    /// </summary>
    /// <param name="_difficulty">Difficulty of the mission</param>
    /// <returns>The amount of skips available for the current mission</returns>
    public int GetExtraMissionsLeft(Mission.Difficulty _difficulty)
    {

        switch (_difficulty)
        {
            case Mission.Difficulty.EASY:
                return m_easyExtraMissionsLeft;
            case Mission.Difficulty.MEDIUM:
                return m_mediumExtraMissionsLeft;
            case Mission.Difficulty.HARD:
                return m_hardExtraMissionsLeft;
            default:    // Shouldnt happen
                return 0;
        }
    }

    

    ///<summary>
    /// When the player completes a mission, he can still play some extra missions
    /// before the cooldown. This method decrements the extra missions available counter.
    ///</summary>
    /// <param name="_difficulty">Difficulty of the mission</param>
    /// <returns>Returns false if the player doesnt have any extra missions left</returns>
    public bool UseExtraMission (Mission.Difficulty _difficulty)
    {
        switch (_difficulty)
        {
            case Mission.Difficulty.EASY:
                if (m_easyExtraMissionsLeft > 0)
                {
                    m_easyExtraMissionsLeft --;
                    return true;
                }
                return false;
            case Mission.Difficulty.MEDIUM:
                if (m_mediumExtraMissionsLeft > 0)
                {
                    m_mediumExtraMissionsLeft--;
                    return true;
                }
                return false;
            case Mission.Difficulty.HARD:
                if (m_hardExtraMissionsLeft > 0)
                {
                    m_hardExtraMissionsLeft--;
                    return true;
                }
                return false;
            default:
                Debug.LogError("Wrong mission difficulty");
                return false; // Default case. Shouldnt happen.
                break;
        }

    }

    
    /// <summary>
    /// Get the cooldown multiplier associated for this mission difficulty
    /// </summary>
    /// <param name="_difficulty"></param>
    /// <returns></returns>
    public float GetMissionCooldownMultiplier(Mission.Difficulty _difficulty)
    {
        switch (_difficulty)
        {
            case Mission.Difficulty.EASY:
                return m_easyMissionCooldownMultiplier;
            case Mission.Difficulty.MEDIUM:
                return m_mediumMissionCooldownMultiplier;
            case Mission.Difficulty.HARD:
                return m_hardMissionCooldownMultiplier;
            default:    // Shouldnt happen
                return 1f;
        }
    }


    ///<summary>
    /// The extra missions counter is restored to its initial value
    /// defined in the content.
    ///</summary>
    /// <param name="_difficulty">Difficulty of the mission</param>
    public void RestoreExtraMissions(Mission.Difficulty _difficulty)
    {
        switch (_difficulty)
        {
            case Mission.Difficulty.EASY:
                m_easyExtraMissionsLeft = m_easyExtraMissions;
                break;
            case Mission.Difficulty.MEDIUM:
                m_mediumExtraMissionsLeft = m_mediumExtraMissions;
                break;
            case Mission.Difficulty.HARD:
                m_hardExtraMissionsLeft = m_hardExtraMissions;
                break;
            default:
                Debug.LogError("Wrong mission difficulty");
                break;
        }
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
            m_mapRevealTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTime() + new TimeSpan(0, 0, m_mapRevealCooldownSecs);
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
                m_mapRevealTimestamp < GameServerManager.SharedInstance.GetEstimatedServerTime());
    }


    /// <summary>
    /// Decrement the amount of free revives. 
    /// </summary>
    /// <returns>Returns false if there arent any left.</returns>
    public bool UseRevive()
    {
        if (m_revivesLeft > 0)
        {
            m_revivesLeft--;
            return true;
        }

        return false;
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
            m_easyExtraMissionsLeft = _data[key].AsInt;
        }

        key = "mediumMissionSkipsLeft";
        if (_data.ContainsKey(key))
        {
            m_mediumExtraMissionsLeft = _data[key].AsInt;
        }

        key = "hardMissionSkipsLeft";
        if (_data.ContainsKey(key))
        {
            m_hardExtraMissionsLeft = _data[key].AsInt;
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
        data.Add("easyMissionSkipsLeft", m_easyExtraMissionsLeft);
        data.Add("mediumMissionSkipsLeft", m_mediumExtraMissionsLeft);
        data.Add("hardMissionSkipsLeft", m_hardExtraMissionsLeft);

        data.Add("mapRevealTimestamp", m_mapRevealTimestamp.Ticks.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));

        return data;
    }

    /// <summary>
    /// When the game starts restore the revive counter
    /// </summary>
    private void OnGameStarted ()
    {
        m_revivesLeft = m_freeRevives;
    }
    
}