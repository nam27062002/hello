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
public abstract class HDLiveEventManager : HDLiveDataController {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    public int m_numericType = -1;

    public bool m_shouldRequestDefinition = false;
    protected bool m_requestingRewards = false;

	protected HDLiveEventData m_data;
	public HDLiveEventData data
	{	
		get { return m_data; }
	}

    private bool m_waitingForNewDefinition = true;
    public bool isWaitingForNewDefinition
    {
        get { return m_waitingForNewDefinition; }
    }

	// Internal
	protected int m_rewardLevel = -1;	// Response to the get_my_rewards request

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    public virtual bool EventExists()
    {
        bool ret = false;
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
		if (data != null && data.m_eventId > 0)
        {
            ret = data.m_state == HDLiveEventData.State.NOT_JOINED || data.m_state == HDLiveEventData.State.JOINED;
        }
        return ret;
	}

	public virtual bool IsTeasing()
	{
		bool ret = false;
        if (data != null && data.m_eventId > 0 )
        {
            ret = data.m_state == HDLiveEventData.State.TEASING;
        }
        return ret;
	}

	public virtual bool IsRewardPending()
	{
		bool ret = false;
        if (data != null && data.m_eventId > 0 )
        {
            ret = data.m_state == HDLiveEventData.State.REWARD_AVAILABLE;
        }
        return ret;
	}

    public virtual bool HasValidDefinition()
    {
        bool ret = false;
        if (data != null && data.m_eventId > 0)
        {
            ret = data.definition.m_eventId == data.m_eventId;
        }
        return ret;
    }

    public virtual bool RequiresUpdate()
    {
        bool ret = false;
        if (data != null && data.m_eventId > 0 )
        {
            ret = data.m_state == HDLiveEventData.State.REQUIRES_UPDATE;
        }
        return ret;
    }


    //------------------------------------------------------------------------//
    // FROM LIVE DATA CONTROLLER                                              //
    //------------------------------------------------------------------------//

    public override void CleanData()
    {
        if (data != null){
            data.Clean();
      	}

        m_dataLoadedFromCache = false;
    }

    public override bool ShouldSaveData() {
        return EventExists() && data.m_state != HDLiveEventData.State.FINALIZED && data.m_state != HDLiveEventData.State.REQUIRES_UPDATE;
    }

    public override SimpleJSON.JSONNode SaveData() {
        return ToJson();
    }

    public override bool IsFinishPending() {
        bool isFinishPending = m_isFinishPending;

        if (isFinishPending
        &&  Application.internetReachability != NetworkReachability.NotReachable
        &&  GameSessionManager.SharedInstance.IsLogged()) {
            FinishEvent();
            HDLiveDataManager.instance.ForceRequestMyEventType(m_numericType);
            m_isFinishPending = false;
        }

        return isFinishPending;
    }

    public override void LoadDataFromCache() {
        CleanData();
        if (CacheServerManager.SharedInstance.HasKey(m_type)) {
            SimpleJSON.JSONNode json = SimpleJSON.JSONNode.Parse(CacheServerManager.SharedInstance.GetVariable(m_type));
            OnNewStateInfo(json);
            UpdateStateFromTimers();
            if (data.m_state == HDLiveEventData.State.REWARD_COLLECTED) {
                m_isFinishPending = true;
            }
        }
        m_dataLoadedFromCache = true;
    }

    public override void LoadData(SimpleJSON.JSONNode _data) {
        bool dataWasLoadedFromCache = m_dataLoadedFromCache;

        CleanData();
        OnNewStateInfo(_data);
        if (data.m_eventId > 0 && (!HasValidDefinition() || dataWasLoadedFromCache)) {
            RequestDefinition();
        }

        m_dataLoadedFromCache = false;
    }

    //------------------------------------------------------------------------//

    public virtual SimpleJSON.JSONClass ToJson()
    {
        // Create new object, initialize and return it
        SimpleJSON.JSONClass ret = null;
        if (data != null)
        {
            ret = data.ToJson();
        }
        return ret;
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
        if (data != null)
        {
        	int oldId = data.m_eventId;
            data.ParseState(_data);

			if (data.m_state == HDLiveEventData.State.REFUND) {
				GetRefund();
			} else {
	            if ( data.m_eventId != oldId )
	            {
	            	OnEventIdChanged();
	            }
			}
        }
    }

    /// <summary>
    /// Clears the event. Removes the data and definition
    /// </summary>
	public void ClearEvent()
    {
		Deactivate();
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
            m_waitingForNewDefinition = true;
	        m_shouldRequestDefinition = false;
	        if ( HDLiveDataManager.TEST_CALLS )
	        {
				ApplicationManager.instance.StartCoroutine( DelayedCall(m_type + "_definition.json", RequestEventDefinitionResponse));
	        }
	        else
	        {
	            GameServerManager.SharedInstance.HDEvents_GetDefinition(data.m_eventId, RequestEventDefinitionResponse);    
	        }
	        ret = true;
        }
        return ret;
    }

	protected delegate void TestResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response);
    protected IEnumerator DelayedCall( string _fileName, TestResponse _testResponse)
	{
		yield return new WaitForSeconds(0.5f);
		GameServerManager.ServerResponse response = HDLiveDataManager.CreateTestResponse( _fileName );
		_testResponse(null, response);
	}

    protected virtual void RequestEventDefinitionResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
		HDLiveDataManager.ResponseLog("Definition", _error, _response);
		HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
		SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);

		if ( outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR )
		{
			int eventId = responseJson["code"].AsInt;
            if (data != null && data.m_eventId == eventId)
            {
                bool wasActive = m_active;
            	if (m_active) {
            		Deactivate();
            	}
                ParseDefinition(responseJson);

				if (data.definition.m_refund) {
					GetRefund();
				} else {
	                if (wasActive){
	                	Activate();
	                }
				}
            }
		} else {
            m_shouldRequestDefinition = true;
        }
        m_waitingForNewDefinition = false;
		Messenger.Broadcast<int, HDLiveDataManager.ComunicationErrorCodes> (MessengerEvents.LIVE_EVENT_NEW_DEFINITION, data.m_eventId, outErr);
    }

	public virtual List<HDLiveData.Reward> GetMyRewards() {
		// To be implemented by heirs!
		return new List<HDLiveData.Reward>();
	}

	public abstract void RequestRewards();    

	protected virtual void RequestRewardsResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
		HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
		HDLiveDataManager.ResponseLog("Rewards", _error, _response);
		SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);
		if ( outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR )
		{
			if (data != null)
            {
				if ( responseJson.ContainsKey("rewardLevel") ){
            		m_rewardLevel = responseJson["rewardLevel"].AsInt;
            	} else {
					m_rewardLevel = 0;
            	}
            }
		}
		m_requestingRewards = false;

		Messenger.Broadcast<int,HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_REWARDS_RECEIVED, data.m_eventId, outErr);
    }


	public virtual void FinishEvent()
	{
		// Tell server
		if ( HDLiveDataManager.TEST_CALLS )
        {
			ApplicationManager.instance.StartCoroutine( DelayedCall(m_type + "_finish.json", FinishEventResponse));
        }
        else
        {
			GameServerManager.SharedInstance.HDEvents_FinishMyEvent(data.m_eventId, FinishEventResponse);    
        }

	}

	protected virtual void FinishEventResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
		HDLiveDataManager.ResponseLog("Finish", _error, _response);
		HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
		SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);
		if ( outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR )
		{
			if ( responseJson.ContainsKey("code") )
			{
                if (responseJson["code"].AsInt == m_data.m_eventId) {
                    data.m_state = HDLiveEventData.State.FINALIZED;
                    HDLiveDataManager.instance.SaveEventsToCache();
                }
			}
		}
		Messenger.Broadcast<int,HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, data.m_eventId, outErr);
    }

	public void GetRefund()
	{
		if ( HDLiveDataManager.TEST_CALLS )
		{
			
		}
		else
		{
			GameServerManager.SharedInstance.HDEvents_Tournament_GetRefund(data.m_eventId, GetRefundResponse);    
		}
	}

	private void GetRefundResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
	{
		HDLiveDataManager.ResponseLog("Refund", _error, _response);
		HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
		SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);

        switch (outErr) {
            case HDLiveDataManager.ComunicationErrorCodes.NO_ERROR:
            Metagame.Reward r = Metagame.Reward.CreateFromJson(responseJson);
            UsersManager.currentUser.PushReward(r);
            FinishEvent();
            ClearEvent();
            break;

            case HDLiveDataManager.ComunicationErrorCodes.NOTHING_PENDING:            
            FinishEvent();
            ClearEvent();
            break;
        }
        // Get My Events
        // Request new event data
        if(!HDLiveDataManager.TEST_CALLS) {       // Would read the event again from the json xD
            HDLiveDataManager.instance.RequestMyLiveData(true);
        }
	}

    // If there is an error we should clear we will call this
    public void ForceFinishByError()
    {
        FinishEvent();
        ClearEvent();
        // Get My Events
        // Request new event data
        if(!HDLiveDataManager.TEST_CALLS) {       // Would read the event again from the json xD
            HDLiveDataManager.instance.RequestMyLiveData(true);
        }   
    }
    
#endregion

#region mods_activation

    public override void Activate()
    {
        if (!m_active)
    	{    		
			if (data.m_state < HDLiveEventData.State.FINALIZED) {
                m_active = true;
				if (data != null && data.definition != null)
	    		{
		    		List<Modifier> mods = data.definition.m_otherMods;
					for (int i = 0; i < mods.Count; i++) {
                        if (mods[i] != null) {
                            mods[i].Apply();
                        }
					}
				}
			}
    	}
    }

    public override  void Deactivate()
    {
		if (m_active)
    	{
            m_active = false;
    		List<Modifier> mods = data.definition.m_otherMods;
			for (int i = 0; i < mods.Count; i++) {
                if (mods[i] != null) {
                    mods[i].Remove();
                }
			}
    	}
    }

    public override void ApplyDragonMods()
    {
		List<Modifier> mods = data.definition.m_dragonMods;
		for (int i = 0; i < mods.Count; i++) {
    		mods[ i ].Apply();
		}
    }

    public bool HasModOfType(Type _type)
    {
        List<Modifier> mods = data.definition.m_otherMods;
        for (int i = 0; i < mods.Count; i++) {
            if (mods[i].GetType() == _type) {
                return true;
            }
        }

        mods = data.definition.m_dragonMods;
        for (int i = 0; i < mods.Count; i++) {
            if (mods[i].GetType() == _type) {
                return true;
            }
        }

        return false;
    }
#endregion

}