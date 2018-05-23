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
public class HDLiveEventManager {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDLiveEventManager() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDLiveEventManager() {

	}

	public virtual bool IsActive()
	{
		bool ret = false;
		HDLiveEventData data = GetEventData();
		if (data != null && data.m_eventId > 0)
		{
			ret = true;
		}
		return true;
	}

	public virtual void CleanData()
	{
		HDLiveEventData data = GetEventData();
		if (data != null)
			data.Clean();
	}

	public virtual SimpleJSON.JSONClass ToJson ()
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

	public virtual void OnNewStateInfo( SimpleJSON.JSONNode _data )
	{
		ParseData( _data );
	}

	public virtual void ParseData(SimpleJSON.JSONNode _data)
	{
		HDLiveEventData data = GetEventData();
		if ( data != null)
		{
			data.ParseState( _data );
		}
	}

	public virtual void ParseDefinition(SimpleJSON.JSONNode _data)
	{
		HDLiveEventData data = GetEventData();
		if ( data != null )
		{
			data.ParseDefinition( _data );
		}
	}

	public virtual void ParseProgress(SimpleJSON.JSONNode _data)
	{
		HDLiveEventData data = GetEventData();
		if ( data != null )
		{
			data.ParseProgress( _data );
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	public void RequestDefinition()
	{
		HDLiveEventData data = GetEventData();
		GameServerManager.SharedInstance.HDEvents_GetDefinition( data.m_eventId, RequestEventDefinitionResponse);
	}

	protected virtual void RequestEventDefinitionResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) 
	{
		if ( _error != null ){
			// Messenger.Broadcast(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_ERROR);
			return;
		}

		if(_response != null && _response["response"] != null) 
		{
			SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
			int eventId = responseJson["code"].AsInt;
			HDLiveEventData data = GetEventData();
			if ( data != null && data.m_eventId == eventId )
			{
				ParseDefinition( responseJson );
			}
		}
	}


	public void RequestProgress()
	{
		HDLiveEventData data = GetEventData();
		GameServerManager.SharedInstance.HDEvents_GetMyProgess(data.m_eventId, RequestProgressResponse);
	}

	protected virtual void RequestProgressResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) 
	{
		if ( _error != null ){
			// Messenger.Broadcast(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_ERROR);
			return;
		}

		if(_response != null && _response["response"] != null) 
		{
			SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
			int eventId = responseJson["code"].AsInt;
			HDLiveEventData data = GetEventData();
			if ( data != null && data.m_eventId == eventId )
			{
				
			}
		}
	}

	public void SendProgress( int _score )
	{
		HDLiveEventData data = GetEventData();
		GameServerManager.SharedInstance.HDEvents_RegisterProgress(data.m_eventId, _score, RegisterProgressResponse);
	}

	protected virtual void RegisterProgressResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) 
	{
		if ( _error != null ){
			// Messenger.Broadcast(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_ERROR);
			return;
		}

		if(_response != null && _response["response"] != null) 
		{
			SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
			int eventId = responseJson["code"].AsInt;
			HDLiveEventData data = GetEventData();
			if ( data != null && data.m_eventId == eventId )
			{
				
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}