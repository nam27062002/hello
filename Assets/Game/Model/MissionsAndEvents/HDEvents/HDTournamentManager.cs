// HDTournamentManager.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 17/05/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

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
/// 
/// </summary>
[Serializable]
public class HDTournamentManager : HDLiveEventManager {
	//------------------------------------------------------------------------//
	// CLASSES   															  //
	//------------------------------------------------------------------------//
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public TrackerBase m_tracker = new TrackerBase();

	private bool m_isLeaderboardReady;
	private bool m_runWasValid = false;
	private long m_lastLeaderboardTimestamp = 0;
	private long m_laderboardRequestMinTim = 1000 * 60 * 5;


		// Control vars
	protected HDTournamentDefinition.TournamentGoal m_runningGoal;
	protected float m_timePlayed = -1;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDTournamentManager() {
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(MessengerEvents.GAME_ENDED, OnGameEnded);

		m_isLeaderboardReady = false;
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDTournamentManager() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(MessengerEvents.GAME_ENDED, OnGameEnded);
	}

	public override void BuildData()
    {
		m_data = new HDTournamentData();
    }

	public override void ParseDefinition(SimpleJSON.JSONNode _data)
    {
    	base.ParseDefinition( _data );
		m_tracker.Clear();
		HDTournamentDefinition def = m_data.definition as HDTournamentDefinition;
		m_tracker = TrackerBase.CreateTracker( def.m_goal.m_typeDef.sku, def.m_goal.m_params);
    }

	override protected void OnEventIdChanged()
	{
		base.OnEventIdChanged();
		m_lastLeaderboardTimestamp = 0;
	}


    public bool RequestLeaderboard( bool _force = false )
    {
    	bool ret = false;
    	if ( _force || ShouldRequestLeaderboard() )
    	{
			m_isLeaderboardReady = false;
			m_lastLeaderboardTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong();

	        if ( HDLiveEventsManager.TEST_CALLS )
	        {
				ApplicationManager.instance.StartCoroutine( DelayedCall("tournament_leaderboard.json", LeaderboardResponse));
	        }
	        else
	        {
	            HDLiveEventData data = GetEventData();
	            GameServerManager.SharedInstance.HDEvents_GetLeaderboard(data.m_eventId, LeaderboardResponse);    
	        }
			ret = true;
        }
        return ret;
    }

	public bool ShouldRequestLeaderboard()
	{
		long diff = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() - m_lastLeaderboardTimestamp;
		return diff > m_laderboardRequestMinTim;	// 5 min timeout
	}

	public bool IsLeaderboardReady() {
		return m_isLeaderboardReady;
	}

    protected virtual void LeaderboardResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
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
			HDTournamentData data = m_data as HDTournamentData;
            if (data != null /*&& data.m_eventId == eventId*/ )
            {
            	if ( responseJson.ContainsKey("c") )// Is cheater
            	{
            		AntiCheatsManager.MarkUserAsCheater();
            	}
            	data.ParseLeaderboard( responseJson );

				m_isLeaderboardReady = true;
            }
			Messenger.Broadcast(MessengerEvents.TOURNAMENT_LEADERBOARD);
        }
    }

    public void SendScore(int _score)
    {
        if (HDLiveEventsManager.TEST_CALLS)
        {
			ApplicationManager.instance.StartCoroutine( DelayedCall("tournament_set_score.json", SetScoreResponse));
        }
        else
        {
            HDLiveEventData data = GetEventData();
            GameServerManager.SharedInstance.HDEvents_SetScore(data.m_eventId, _score, SetScoreResponse);
        }
    }

    protected virtual void SetScoreResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
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
            if (data != null/* && data.m_eventId == eventId*/)
            {
            	
            }

			Messenger.Broadcast(MessengerEvents.TOURNAMENT_SCORE_SENT);
        }
    }

	public void OnGameStarted(){

		if ( m_tracker != null)
    	{
			m_tracker.SetValue(0, false);
			// Check if we are in tournament mode
			m_tracker.enabled = m_isActive;
    	}

		if ( m_isActive )
		{
			m_runWasValid = false;
			Messenger.AddListener(MessengerEvents.GAME_UPDATED, OnGameUpdate);
			HDTournamentData data = HDLiveEventsManager.instance.m_tournament.GetEventData() as HDTournamentData;
			HDTournamentDefinition def = data.definition as HDTournamentDefinition;
			m_runningGoal = def.m_goal;

			switch( def.m_goal.m_mode )
			{
				case HDTournamentDefinition.TournamentGoal.TournamentMode.NORMAL:
				{
					m_runWasValid = true;	// Any run was valid
				}break;
				case HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_LIMIT:
				{
					m_runWasValid = true;	// Any run is valis
				}break;
				case HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_ATTACK:
				{
					m_runWasValid = false;// for a run to be valid it has to meet the target
				}break;
			}
			
		}

    }


	public void OnGameUpdate(){
		// Check time limit?
		switch( m_runningGoal.m_mode )
		{
			case HDTournamentDefinition.TournamentGoal.TournamentMode.NORMAL:
			{
			}break;
			case HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_LIMIT:
			{
				if ( m_runningGoal.m_seconds <= InstanceManager.gameSceneController.elapsedSeconds )
				{
					// End game!
					if (InstanceManager.gameSceneController != null && !InstanceManager.gameSceneController.isSwitchingArea )
					{
						InstanceManager.gameSceneController.EndGame(false);
					}
					
				}
			}break;
			case HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_ATTACK:
			{
				if ( m_tracker.currentValue >= m_runningGoal.m_targetAmount )
				{
					m_runWasValid = true;
					// End Game!
					if (InstanceManager.gameSceneController != null && !InstanceManager.gameSceneController.isSwitchingArea )
					{
						InstanceManager.gameSceneController.EndGame(true);
					}
					
				}
			}break;
		}

	}

    public void OnGameEnded(){
    	// Save tracker value?
		Messenger.RemoveListener(MessengerEvents.GAME_UPDATED, OnGameUpdate);
		m_timePlayed = InstanceManager.gameSceneController.elapsedSeconds;
    }

	//------------------------------------------------------------------------//
	// BUILD METHODS														  //
	//------------------------------------------------------------------------//
    public bool UsingProgressionDragon()
    {
		HDTournamentData data = m_data as HDTournamentData;
    	HDTournamentDefinition def = data.definition as HDTournamentDefinition;
    	bool ret = false;
    	if (string.IsNullOrEmpty( def.m_build.m_dragon))
    	{
    		ret = true;
    	}
    	return ret;
    }

    public string GetToUseDragon()
    {
		string ret;
    	HDTournamentData data = m_data as HDTournamentData;
    	HDTournamentDefinition def = data.definition as HDTournamentDefinition;
    	if ( !string.IsNullOrEmpty( def.m_build.m_dragon) )
			ret = def.m_build.m_dragon;
		else
			ret = UsersManager.currentUser.currentDragon;
		return ret;
    }

    public string GetToUseSkin()
    {
		string ret;
    	HDTournamentData data = m_data as HDTournamentData;
    	HDTournamentDefinition def = data.definition as HDTournamentDefinition;
    	if ( !string.IsNullOrEmpty( def.m_build.m_skin) )
			ret = def.m_build.m_skin;
		else
			ret = UsersManager.currentUser.GetEquipedDisguise(GetToUseDragon());
		return ret;
    }

    public List<string> GetToUsePets()
    {
    	List<string> ret;
		HDTournamentData data = m_data as HDTournamentData;
    	HDTournamentDefinition def = data.definition as HDTournamentDefinition;
    	if ( def.m_build.m_pets.Count > 0 )
    	{
			ret = def.m_build.m_pets;
		}
		else
		{
			ret = UsersManager.currentUser.GetEquipedPets(GetToUseDragon());
		}
		return ret;
    }

	//------------------------------------------------------------------------//
	// SCORE METHODS														  //
	//------------------------------------------------------------------------//
    /// <summary>
    /// Tells if the last played run was valid
    /// </summary>
    /// <returns><c>true</c>, if last run valid was wased, <c>false</c> otherwise.</returns>
    public bool WasLastRunValid()
    {
    	return m_runWasValid;
    }

	/// <summary>
	/// The score obtained in the last run (tracker)
	/// </summary>
	public long GetRunScore() {
		
		HDTournamentData data = m_data as HDTournamentData;
		HDTournamentDefinition def = data.definition as HDTournamentDefinition;
		long ret = -1;
		switch ( def.m_goal.m_mode )
		{
			case HDTournamentDefinition.TournamentGoal.TournamentMode.NORMAL:
			{
				ret = m_tracker.currentValue;
			}break;
			case HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_ATTACK:
			{
				// Game duration!
				ret = (long)m_timePlayed;
			}break;
			case HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_LIMIT:
			{
				ret = m_tracker.currentValue;
			}break;
		}

		return ret;
	}

	/// <summary>
	/// The overall best score (the one registered in the leaderboard)
	/// </summary>
	public long GetBestScore() {
		HDTournamentData data = m_data as HDTournamentData;
		return data.m_score;
	}

	/// <summary>
	/// Has the last run been a high score? This functions is useful before sending the new score
	/// </summary>
	public bool IsNewBestScore() {
		long runScore = GetRunScore();
		long bestScore = GetBestScore();
		bool ret = false;
		HDTournamentDefinition def = data.definition as HDTournamentDefinition;
		switch ( def.m_goal.m_mode )
		{
			case HDTournamentDefinition.TournamentGoal.TournamentMode.NORMAL:
			case HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_LIMIT:
			{
				ret = runScore > bestScore;
			}break;
			case HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_ATTACK:
			{
				ret = runScore < bestScore;
			}break;
		}

		return ret;
	}

	//------------------------------------------------------------------------//
	// UI HELPER METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Given a score, format it based on tournament type
	/// </summary>
	public string FormatScore(long _score) {
		// TODO!!
		// Depends on tournament type, not all scores are numbers! (time)
		//return TimeUtils.FormatTime((double)_score, TimeUtils.EFormat.DIGITS, 2, TimeUtils.EPrecision.MINUTES, true);	// MM:SS
		return StringUtils.FormatNumber(_score);
	}
}