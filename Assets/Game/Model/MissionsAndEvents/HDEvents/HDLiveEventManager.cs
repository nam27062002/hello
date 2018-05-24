// HDLiveEventManager.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 17/05/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class HDLiveEventManager
{
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    public string m_type = "";
    public bool m_shouldRequestDefinition = false;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    public HDLiveEventManager()
    {

    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~HDLiveEventManager()
    {
    	
    }

    public virtual bool IsActive()
    {
        bool ret = false;
        HDLiveEventData data = GetEventData();
        if (data != null && data.m_eventId > 0)
        {
            ret = true;
        }
        return ret;
    }

    public virtual bool HasValidDefinition()
    {
        bool ret = false;
        HDLiveEventData data = GetEventData();
        if (data != null && data.m_eventId > 0)
        {
            
            ret = data.definition.m_eventId == data.m_eventId;
        }
        return ret;
    }

    public virtual void CleanData()
    {
        HDLiveEventData data = GetEventData();
        if (data != null)
            data.Clean();
    }

    public virtual SimpleJSON.JSONClass ToJson()
    {
        // Create new object, initialize and return it
        SimpleJSON.JSONClass ret = null;
        HDLiveEventData data = GetEventData();
        if (data != null)
        {
            ret = data.ToJson();
        }
        return ret;
    }



    public virtual HDLiveEventData GetEventData()
    {
        return null;
    }

    public virtual void OnNewStateInfo(SimpleJSON.JSONNode _data)
    {
        ParseData(_data);
    }

    public virtual void ParseData(SimpleJSON.JSONNode _data)
    {
        HDLiveEventData data = GetEventData();
        if (data != null)
        {
            data.ParseState(_data);
            m_shouldRequestDefinition = true;
        }
    }

    public virtual void ParseDefinition(SimpleJSON.JSONNode _data)
    {
        HDLiveEventData data = GetEventData();
        if (data != null)
        {
            data.ParseDefinition(_data);
        }
    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    public void RequestDefinition()
    {
        m_shouldRequestDefinition = false;
        if ( HDLiveEventsManager.TEST_CALLS )
        {
            GameServerManager.ServerResponse response = HDLiveEventsManager.CreateTestResponse(m_type + "_definition.json");
            RequestEventDefinitionResponse(null, response);    
        }
        else
        {
            HDLiveEventData data = GetEventData();
            GameServerManager.SharedInstance.HDEvents_GetDefinition(data.m_eventId, RequestEventDefinitionResponse);    
        }
    }

    protected virtual void RequestEventDefinitionResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
        if (_error != null)
        {
            // Messenger.Broadcast(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_ERROR);
            return;
        }

        if (_response != null && _response["response"] != null)
        {
            SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
            int eventId = responseJson["code"].AsInt;
            HDLiveEventData data = GetEventData();
            if (data != null && data.m_eventId == eventId)
            {
                ParseDefinition(responseJson);
            }
        }
    }


}