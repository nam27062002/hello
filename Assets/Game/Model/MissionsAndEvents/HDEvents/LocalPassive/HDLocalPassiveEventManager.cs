// HDLocalPassiveEventManager.cs
// Hungry Dragon
// 
// Created by Jose M. Olea on 16/10/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using SimpleJSON;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Represents a Passive Event that is independent from the server. It is defined in the
/// content files and is persisted in the user profile. If there is a live passive event
/// active, the local one has preference over that.
/// </summary>
[Serializable]
public class HDLocalPassiveEventManager: HDPassiveEventManager {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
    // Manager will try to find this def in the server response. Wont be there as this passive event is local.
    // For the sake of consistency only. 
    public new const string TYPE_CODE = "localPassive";
    public new const int NUMERIC_TYPE_CODE = 4;

    // Use a static event id for the local passive event (any number > 0 will do)
    public const int EVENT_ID = 1;
    
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	
    
    private int m_passiveDurationDays;
    
    public override HDLiveEventData data
    {	
        get { return m_passiveEventData; }
    }

    
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDLocalPassiveEventManager()
    {
        // Redefine this values (they differ from the live passive event object)
        m_type = TYPE_CODE;
        m_numericType = NUMERIC_TYPE_CODE;
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDLocalPassiveEventManager() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

    /// <summary>
    /// Create the passive event from the definition and initialize the timers
    /// so the event is activated at this moment
    /// </summary>
    public void StartPassiveEvent()
    {
        
        // Initialize the quest from content
        InitFromDefinition();

        if (EventExists())
        {
            // Calculate the ending timestamp
            m_passiveEventDefinition.m_startTimestamp = GameServerManager.GetEstimatedServerTime();
            m_passiveEventDefinition.m_endTimestamp =
                m_passiveEventDefinition.m_startTimestamp.AddDays(m_passiveDurationDays);
            
            // Set the state to JOINED 
            m_data.m_state = HDLiveEventData.State.JOINED;

            // Activate passive
            Activate();
            
            // Anounce the new passive via broadcast
            Messenger.Broadcast<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, EVENT_ID,
                HDLiveDataManager.ComunicationErrorCodes.NO_ERROR);
        }
    }

    /// <summary>
    /// End this event
    /// </summary>
    public void DestroyPassiveEvent()
    {
        m_passiveEventData.Clean();
        m_passiveEventDefinition.Clean();

        m_passiveEventDefinition = m_passiveEventData.definition as HDPassiveEventDefinition;
        
        // Anounce it via broadcast
        Messenger.Broadcast<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, EVENT_ID,
            HDLiveDataManager.ComunicationErrorCodes.NO_ERROR);
    }
    
    /// <summary>
    /// Create the passive event definition from the data defined in the welcome back content
    /// </summary>
    public void InitFromDefinition()
    {
        // Set the fake event id
        m_passiveEventData.m_eventId = EVENT_ID;
        
        // Find welcome back definition
        DefinitionNode configDef = WelcomeBackManager.instance.def;

        m_passiveDurationDays = configDef.GetAsInt("passiveDurationDays");
        string passiveModSku = configDef.GetAsString("passiveModSku");
        
        if (string.IsNullOrEmpty(passiveModSku))
        {
            // Make sure there is valid mod in the xml
            Debug.LogError("No passive mod was defined in the content");

            return;
        }
        


        // Initialized definition object
        
        m_passiveEventDefinition = m_passiveEventData.definition as HDPassiveEventDefinition;
        m_passiveEventDefinition.InitFromDefinition(configDef);
        m_passiveEventDefinition.m_eventId = m_passiveEventData.m_eventId;


    }
    
	//------------------------------------------------------------------------//
	// PARENT OVERRIDING
	//------------------------------------------------------------------------//
    
    public override void OnLiveDataResponse()
    {
        // Nothing. This is a local event, not live.
    }
    
    public override void FinishEvent()
    {
        // Just change the state. For local events is as simple as this
        m_data.m_state = HDLiveEventData.State.FINALIZED;

        // Notify everyone via broadcast
        Messenger.Broadcast<int,HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, m_data.m_eventId, 
            HDLiveDataManager.ComunicationErrorCodes.NO_ERROR);

    }

    public override void LoadDataFromCache()
    {
        // Nothing. Local events are stored in the persistence.
    }

    /// <summary>
    /// This method determines if the event should be saved in the cache
    /// </summary>
    /// <returns></returns>
    public override bool ShouldSaveData()
    {
        // Nope. Local events are stored in the persistence, not in cache.
        return false;
    }

    //------------------------------------------------------------------------//
    // PERSISTENCE															  //
    //------------------------------------------------------------------------//
	
    /// <summary>
    /// Constructor from json data.
    /// </summary>
    /// <param name="_data">Data to be parsed.</param>
    public void ParseJson(SimpleJSON.JSONNode _data)
    {
        m_passiveEventData.Clean();
        m_passiveEventData.ParseState(_data["data"]);
		
        m_passiveEventDefinition = m_passiveEventData.definition as HDPassiveEventDefinition;
    }

    /// <summary>
    /// Serialize into json.
    /// </summary>
    /// <returns>The json.</returns>
    public JSONClass ToJson()
    {
        JSONClass jsonData = new JSONClass();

        // Add data
		
        jsonData.Add("data", m_data.ToJson());

        return jsonData;
    }
    
    
}