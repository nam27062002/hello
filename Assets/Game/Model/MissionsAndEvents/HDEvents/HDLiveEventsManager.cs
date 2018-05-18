// LiveEventManager.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 15/05/2018.
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
/// Global singleton manager for global events.
/// </summary>
public class HDLiveEventsManager : Singleton<HDLiveEventsManager> {


	public enum ErrorCode {
		NONE = 0,

		OFFLINE,
		NOT_INITIALIZED,
		NOT_LOGGED_IN,
		NO_VALID_EVENT,
		EVENT_NOT_ACTIVE
	}

	HDTournamentManager m_tournament = new HDTournamentManager();
	HDLiveEventManager m_quest = new HDLiveEventManager();
	HDLiveEventManager m_passive = new HDLiveEventManager();


	public void RequestMyEvents() 
	{
		// GameServerManager.SharedInstance.HDEvents_GetMyEvents(instance.MyEventsResponse);
	}

	private void MyEventsResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) 
	{
		if ( _error != null ){
			// Messenger.Broadcast(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_ERROR);
			return;
		}

		if(_response != null && _response["response"] != null) 
		{
			SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);

			m_tournament.CleanData();
			if ( responseJson != null && responseJson.ContainsKey("tournament")){
				m_tournament.OnNewStateInfo(responseJson["tournament"]);	
			}

			m_quest.CleanData();
			if ( responseJson != null && responseJson.ContainsKey("quest")){
				m_quest.OnNewStateInfo(responseJson["tournament"]);	
			}

			m_passive.CleanData();
			if ( responseJson != null && responseJson.ContainsKey("passive")){
				m_quest.OnNewStateInfo(responseJson["passive"]);	
			}
		}
	}

	public bool Connected() {
		bool ret = false;	
		if ((CPGlobalEventsTest.networkCheck && Application.internetReachability != NetworkReachability.NotReachable) &&
			(CPGlobalEventsTest.loginCheck   && GameSessionManager.SharedInstance.IsLogged())) {
			ret = true;
		}
		return ret;
	}
}