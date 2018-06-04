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
using System.Collections;
using System.Collections.Generic;
using System.IO;

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
    public bool m_isActive = false;

	protected HDLiveEventData m_data;
	public HDLiveEventData data
	{	
		get { return m_data; }
	}

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    public HDLiveEventManager()
    {
    	BuildData();
    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~HDLiveEventManager()
    {
    	
    }

    public virtual void BuildData()
    {
    	m_data = new HDLiveEventData();
    }

    public virtual bool EventExists()
    {
        bool ret = false;
        HDLiveEventData data = GetEventData();
        if (data != null && data.m_eventId > 0)
        {
			ret = data.definition.m_eventId == data.m_eventId;
        }
        return ret;
    }

    public virtual void UpdateStateFromTimers()
    {
    	m_data.UpdateStateFromTimers();
    }

    /// <summary>
    /// Is this event active?
    /// </summary>
	/// <returns><c>true</c> if this event is active (not teasing, nor reward pending).</returns>
    public virtual bool IsRunning()
    {
		bool ret = false;
        HDLiveEventData data = GetEventData();
        if (data != null && data.m_eventId > 0 )
        {
            ret = data.m_state == HDLiveEventData.State.NOT_JOINED || data.m_state == HDLiveEventData.State.JOINED;
        }
        return ret;
	}

	public virtual bool IsTeasing()
	{
		bool ret = false;
        HDLiveEventData data = GetEventData();
        if (data != null && data.m_eventId > 0 )
        {
            ret = data.m_state == HDLiveEventData.State.TEASING;
        }
        return ret;
	}

	public virtual bool IsRewardPenging()
	{
		bool ret = false;
        HDLiveEventData data = GetEventData();
        if (data != null && data.m_eventId > 0 )
        {
            ret = data.m_state == HDLiveEventData.State.REWARD_AVAILABLE;
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
        if (data != null){
            data.Clean();
      	}
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
        return m_data;
    }

    public virtual void OnNewStateInfo(SimpleJSON.JSONNode _data)
    {
        ParseData(_data);
    }

    /// <summary>
    /// Parses the data. Parses the state of the event.
    /// </summary>
    /// <param name="_data">Data.</param>
    public virtual void ParseData(SimpleJSON.JSONNode _data)
    {
        HDLiveEventData data = GetEventData();
        if (data != null)
        {
        	int oldId = data.m_eventId;
            data.ParseState(_data);
            if ( data.m_eventId != oldId )
            {
            	OnEventIdChanged();
            }
        }
    }

    /// <summary>
    /// Clears the event. Removes the data and definition
    /// </summary>
	public void ClearEvent()
    {
    	CleanData();
    }

    /// <summary>
    /// Raises the event identifier changed event. 
    // Function called when we parse an state and it has a different event id
    /// </summary>
	protected virtual void OnEventIdChanged()
	{
		m_shouldRequestDefinition = true;
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
    // SERVER CALLS  														  //
    //------------------------------------------------------------------------//

#region server_comunication
	public bool ShouldRequestDefinition()
	{
		return m_shouldRequestDefinition;
	}
    public bool RequestDefinition(bool _force = false)
    {
    	bool ret = false;
    	if ( _force || ShouldRequestDefinition() )
    	{
	        m_shouldRequestDefinition = false;
	        if ( HDLiveEventsManager.TEST_CALLS )
	        {
				ApplicationManager.instance.StartCoroutine( DelayedCall(m_type + "_definition.json", RequestEventDefinitionResponse));
	        }
	        else
	        {
	            HDLiveEventData data = GetEventData();
	            GameServerManager.SharedInstance.HDEvents_GetDefinition(data.m_eventId, RequestEventDefinitionResponse);    
	        }
	        ret = true;
        }
        return ret;
    }

	protected delegate void TestResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response);
	protected IEnumerator DelayedCall( string _fileName, TestResponse _testResponse )
	{
		yield return new WaitForSeconds(0.1f);
		GameServerManager.ServerResponse response = HDLiveEventsManager.CreateTestResponse( _fileName );
		_testResponse(null, response);
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
            if ( responseJson != null )
            {
	            int eventId = responseJson["code"].AsInt;
	            HDLiveEventData data = GetEventData();
	            if (data != null && data.m_eventId == eventId)
	            {
	            	bool wasActive = m_isActive;
	            	if ( m_isActive ){
	            		Deactivate();
	            	}
	                ParseDefinition(responseJson);
	                if (wasActive){
	                	Activate();
	                }
	            }
				Messenger.Broadcast(MessengerEvents.LIVE_EVENT_NEW_DEFINITION);
			}
			else
			{
				TreatJsonParseError();
			}
        }
    }

	public void RequestRewards()
    {
		if ( HDLiveEventsManager.TEST_CALLS )
        {
			ApplicationManager.instance.StartCoroutine( DelayedCall(m_type + "_rewards.json", RequestRewardsResponse));
        }
        else
        {
            HDLiveEventData data = GetEventData();
			GameServerManager.SharedInstance.HDEvents_GetMyReward(data.m_eventId, RequestRewardsResponse);    
        }
    }

	protected virtual void RequestRewardsResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
		if (_error != null)
        {
            return;
        }

        if (_response != null && _response["response"] != null)
        {
            SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
            if ( responseJson != null )
            {
	            if (data != null)
	            {
	            	
	            }
				Messenger.Broadcast(MessengerEvents.LIVE_EVENT_REWARDS_REVIEVED);
			}
			else
			{
				TreatJsonParseError();
			}
        }
    }


	public void FinishEvent()
	{
		// Tell server
		if ( HDLiveEventsManager.TEST_CALLS )
        {
			ApplicationManager.instance.StartCoroutine( DelayedCall(m_type + "_finish.json", FinishEventResponse));
        }
        else
        {
            HDLiveEventData data = GetEventData();
			GameServerManager.SharedInstance.HDEvents_FinishMyEvent(data.m_eventId, FinishEventResponse);    
        }

	}

	protected virtual void FinishEventResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
		if (_error != null)
        {
            return;
        }

        if (_response != null && _response["response"] != null)
        {
            SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
            if ( responseJson != null )
            {
	            if (data != null)
	            {
	            	
	            }
				Messenger.Broadcast(MessengerEvents.LIVE_EVENT_FINISHED);
			}
			else
			{
				TreatJsonParseError();
			}
        }
    }

    /// <summary>
    /// Treats the errors. Returns true if the error validates the event
    /// </summary>
    /// <returns><c>true</c>, if errors was treated, <c>false</c> otherwise.</returns>
	protected virtual bool TreatErrors()
	{
		bool ret = false;

		return ret;
	}

	protected virtual void TreatJsonParseError()
	{
			
	}
#endregion

#region mods_activation

    public void Activate()
    {
    	if (!m_isActive)
    	{
    		m_isActive = true;
    		HDLiveEventData data = GetEventData();
    		if ( data != null && data.definition != null )
    		{
	    		List<Modifier> mods = data.definition.m_otherMods;
				for (int i = 0; i < mods.Count; i++) {
	    			mods[i].Apply();
				}
			}
    	}
    }

    public void Deactivate()
    {
		if (m_isActive)
    	{
    		m_isActive = false;
    		HDLiveEventData data = GetEventData();
    		List<Modifier> mods = data.definition.m_otherMods;
			for (int i = 0; i < mods.Count; i++) {
    			mods[i].Remove();
			}
    	}
    }

    public void ApplyDragonMods()
    {
		HDLiveEventData data = GetEventData();
		List<Modifier> mods = data.definition.m_dragonMods;
		for (int i = 0; i < mods.Count; i++) {
    		mods[ i ].Apply();
		}
    }
#endregion

}