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
using System.Collections;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class HDTournamentManager : HDLiveEventManager, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS   															  //
	//------------------------------------------------------------------------//
	public const string TYPE_CODE = "tournament";
	public const int NUMERIC_TYPE_CODE = 2;

	private const float TIME_LIMIT_END_OF_GAME_DELAY = 2f;
	private const float TIME_ATTACK_END_OF_GAME_DELAY = 2f;

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

	protected string m_entranceSent = "";
	protected long m_entranceAmountSent = 0;
	protected bool m_doneChecking = false;

	protected HDTournamentData m_tournamentData;
	public HDTournamentData tournamentData {
		get { return m_tournamentData; }
	}

	protected HDTournamentDefinition m_tournamentDefinition;


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDTournamentManager() {
        m_type = TYPE_CODE;
		m_numericType = NUMERIC_TYPE_CODE;

        m_data = new HDTournamentData();
        m_tournamentData = m_data as HDTournamentData;
        m_tournamentDefinition = m_tournamentData.definition as HDTournamentDefinition;
        
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
        
		m_isLeaderboardReady = false;
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDTournamentManager() {
        m_data = null;
        m_tournamentData = null;
        m_tournamentDefinition = null;
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
	}
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.GAME_ENDED:
            {
                OnGameEnded();
            }break;
        }
    }
    

	public override void ParseDefinition(SimpleJSON.JSONNode _data)
    {
    	base.ParseDefinition( _data );
    	if ( m_tracker != null )
			m_tracker.Clear();
		
		if (m_tournamentDefinition.m_goal.m_typeDef != null )
		{
			m_tracker = TrackerBase.CreateTracker(m_tournamentDefinition.m_goal.m_typeDef.sku, m_tournamentDefinition.m_goal.m_params);
		}
		else
		{
			Debug.LogError("TOURNAMENT: No Tracker!!");
		}
    }

	override protected void OnEventIdChanged()
	{
		base.OnEventIdChanged();
		m_lastLeaderboardTimestamp = 0;
		m_isLeaderboardReady = false;
	}

    #region server_comunication

    public override void OnLiveDataResponse() { 
        if (EventExists() && ShouldRequestLeaderboard())
            RequestLeaderboard();
    }

    public bool RequestLeaderboard( bool _force = false )
    {
    	bool ret = false;
    	if ( _force || ShouldRequestLeaderboard() )
    	{
			m_isLeaderboardReady = false;
			//m_lastLeaderboardTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong();	// [AOC] If the leaderboard fails, we won't be requesting it again until the caching period has expired!

	        if ( HDLiveDataManager.TEST_CALLS )
	        {
				ApplicationManager.instance.StartCoroutine( DelayedCall("tournament_leaderboard.json", LeaderboardResponse));
	        }
	        else
	        {
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
		HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
		HDLiveDataManager.ResponseLog("Leaderboard", _error, _response);
		SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);
		if ( outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR )
		{
			if (m_tournamentData != null )
            {
            	if ( responseJson.ContainsKey("c") )// Is cheater
            	{
            		AntiCheatsManager.MarkUserAsCheater();
            	}
            	tournamentData.ParseLeaderboard( responseJson );
				m_isLeaderboardReady = true;

				// We have new leaderobard data! Reset cache timer
				m_lastLeaderboardTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong();
            }
			Messenger.Broadcast(MessengerEvents.TOURNAMENT_LEADERBOARD);
		}
    }


    public void SendEntrance( string _type, long _amount )
    {
		m_entranceSent = _type;
		m_entranceAmountSent = _amount;
		if ( HDLiveDataManager.TEST_CALLS )
        {
			ApplicationManager.instance.StartCoroutine( DelayedCall("tournament_entrance.json", EntranceResponse));
        }
        else
        {
			int playerProgress = GetCurrentMatchmakingValue();
			GameServerManager.SharedInstance.HDEvents_Tournament_EnterEvent(data.m_eventId, _type, _amount, playerProgress, EntranceResponse);    
        }
    }

	protected virtual void EntranceResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
		HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
		HDLiveDataManager.ResponseLog("Entrance", _error, _response);
		SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);
		if ( responseJson != null && responseJson.ContainsKey("lastFreeTournamentRun") )
		{
			m_tournamentData.lastFreeEntranceTimestamp = responseJson["lastFreeTournamentRun"].AsLong;
			// Save cache?
		}
		Messenger.Broadcast<HDLiveDataManager.ComunicationErrorCodes, string, long> (MessengerEvents.TOURNAMENT_ENTRANCE, outErr, m_entranceSent, m_entranceAmountSent);
    }


    public void SendScore(int _score)
    {
        if (HDLiveDataManager.TEST_CALLS)
        {
			ApplicationManager.instance.StartCoroutine( DelayedCall("tournament_set_score.json", SetScoreResponse));
        }
        else
        {
            	// Build
            SimpleJSON.JSONClass _build = new SimpleJSON.JSONClass();
            _build.Add("dragon", m_tournamentDefinition.dragonData.sku);
			_build.Add("skin", m_tournamentDefinition.dragonData.disguise);
			SimpleJSON.JSONArray arr = new SimpleJSON.JSONArray();
			List<string> pets = m_tournamentDefinition.dragonData.pets;
			int max = pets.Count;
			for (int i = 0; i < max; i++) {
				arr.Add( pets[i] );
			}
			_build.Add("pets", arr);

            GameServerManager.SharedInstance.HDEvents_Tournament_SetScore(data.m_eventId, _score, _build, SetScoreResponse);
        }
    }

    protected virtual void SetScoreResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
		HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
		HDLiveDataManager.ResponseLog("SetScore", _error, _response);
		SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);
		if ( outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR )
		{
			if (m_tournamentData.m_state == HDLiveEventData.State.NOT_JOINED)
			{
				m_tournamentData.m_matchmakingValue = GetCurrentMatchmakingValue();
				m_tournamentData.m_state = HDLiveEventData.State.JOINED;
			}
			m_tournamentData.ParseLeaderboard(responseJson);

			// We have new leaderobard data! Reset cache timer
			m_lastLeaderboardTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong();
		}

		Messenger.Broadcast<HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.TOURNAMENT_SCORE_SENT, outErr);
    }

	public override void RequestRewards() {
		if (!m_requestingRewards && m_data.m_state < HDLiveEventData.State.REWARD_COLLECTED)
		{
			m_requestingRewards = true;
			if ( HDLiveDataManager.TEST_CALLS )
	        {
				ApplicationManager.instance.StartCoroutine( DelayedCall(m_type + "_rewards.json", RequestRewardsResponse));
	        }
	        else
	        {
				GameServerManager.SharedInstance.HDEvents_Tournament_GetMyReward(data.m_eventId, RequestRewardsResponse);    
	        }
        }

	}
#endregion

	/// <summary>
	/// Times to next free in seconds.
	/// </summary>
	/// <returns>The to next free.</returns>
	public double TimeToNextFree()
	{
		long millis = 0;

		if ( m_tournamentDefinition.m_entrance.m_type != "free" )
			millis = (m_tournamentDefinition.m_entrance.m_dailyFree * 1000) - (GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() - m_tournamentData.lastFreeEntranceTimestamp);

		if ( millis < 0  )
			millis = 0;

		return millis / 1000.0;
	}

	public bool CanIUseFree()
	{
		HDTournamentData tData = data as HDTournamentData;
		HDTournamentDefinition tDef = data.definition as HDTournamentDefinition;
		bool ret = false;

		long t1 = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() - tData.lastFreeEntranceTimestamp;
		long t2 = tDef.m_entrance.m_dailyFree * 1000;
		if ( tDef.m_entrance.m_type == "free" || t1 > t2 )
		{
			ret = true;
		}
		ret = false;
		return ret;
	}

	public void OnGameStarted(){
		m_doneChecking = false;
		if ( m_tracker != null)
    	{
			// Check if we are in tournament mode
			m_tracker.enabled = isActive;
			m_tracker.InitValue(0);
			Debug.Log(Color.green.Tag("RESET TRACKER (ON GAME STARTED"));
    	}

		if (isActive)
		{
			m_runWasValid = false;
			Messenger.AddListener(MessengerEvents.GAME_UPDATED, OnGameUpdate);
			m_runningGoal = m_tournamentDefinition.m_goal;

			switch(m_runningGoal.m_mode )
			{
				case HDTournamentDefinition.TournamentGoal.TournamentMode.NORMAL:
				{
					m_runWasValid = true;	// Any run is valid
				}break;
				case HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_LIMIT:
				{
					m_runWasValid = true;	// Any run is valid
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
		if ( !m_doneChecking )
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
						// Tell hud to show "Time is Up!"
						Messenger.Broadcast(MessengerEvents.TIMES_UP);
						m_doneChecking = true;
						InstanceManager.gameSceneController.StartCoroutine( DelayedEnd(TIME_LIMIT_END_OF_GAME_DELAY) );
					}
					
				}
			}break;
			case HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_ATTACK:
			{
				if ( m_tracker != null && m_tracker.currentValue >= m_runningGoal.m_targetAmount )
				{
					m_runWasValid = true;
					// End Game!
					if (InstanceManager.gameSceneController != null && !InstanceManager.gameSceneController.isSwitchingArea )
					{
						// Tell hud to show "Target accomplished"
						Messenger.Broadcast(MessengerEvents.TARGET_REACHED);
						m_doneChecking = true;
						InstanceManager.gameSceneController.StartCoroutine( DelayedEnd(TIME_ATTACK_END_OF_GAME_DELAY) );
					}
				}
			}break;
		}

	}

	IEnumerator DelayedEnd(float _delay)
	{
		// InstanceManager.gameSceneController.PauseGame(true, true);
		InstanceManager.gameSceneController.freezeElapsedSeconds++;
		if ( m_tracker != null)
			m_tracker.enabled = false;
		m_timePlayed = InstanceManager.gameSceneController.elapsedSeconds;
		InstanceManager.player.playable = false;
		InstanceManager.player.dragonMotion.control.enabled = false;
		yield return new WaitForSecondsRealtime(_delay);

		while( InstanceManager.gameSceneController != null && InstanceManager.gameSceneController.state < GameSceneController.EStates.FINISHED)
		{
			if ( !InstanceManager.gameSceneController.isSwitchingArea )
			{
				InstanceManager.gameSceneController.EndGame(false);
			}	
			yield return null;
		} 
	}

    public void OnGameEnded(){
    	// Save tracker value?
		Messenger.RemoveListener(MessengerEvents.GAME_UPDATED, OnGameUpdate);
		if ( !m_runWasValid )	// For time attack. We set this on normal ending, but if the player dies we save this value here
			m_timePlayed = InstanceManager.gameSceneController.elapsedSeconds;
    }

	//------------------------------------------------------------------------//
	// BUILD METHODS														  //
	//------------------------------------------------------------------------//
    /*public bool UsingProgressionDragon()
    {
		HDTournamentData tournamentData = m_data as HDTournamentData;
    	HDTournamentDefinition def = tournamentData.definition as HDTournamentDefinition;
    	bool ret = false;
    	if (string.IsNullOrEmpty(def.m_build.dragon))
    	{
    		ret = true;
    	}
    	return ret;
    }

    public string GetToUseDragon()
    {
		string ret;
		// NOTE: Here we should check datas last dragon used to return as it will be the prefered dragon
    	HDTournamentData tournamentData = m_data as HDTournamentData;
    	HDTournamentDefinition def = tournamentData.definition as HDTournamentDefinition;
    	if ( !string.IsNullOrEmpty( def.m_build.dragon) ){
			ret = def.m_build.dragon;
		}else{
			ret = UsersManager.currentUser.currentClassicDragon;	// [AOC] TODO!! Allow special dragons?
		}
		return ret;
    }

    // TODO:
    // public string SetToUseDragon( string dragonSku )

    public string GetToUseSkin()
    {
		string ret;
    	HDTournamentData tournamentData = m_data as HDTournamentData;
    	HDTournamentDefinition def = tournamentData.definition as HDTournamentDefinition;
    	if ( !string.IsNullOrEmpty( def.m_build.skin) )
			ret = def.m_build.skin;
		else
			ret = UsersManager.currentUser.GetEquipedDisguise(GetToUseDragon());
		return ret;
    }

    public List<string> GetToUsePets()
    {
    	List<string> ret;
		HDTournamentData tournamentData = m_data as HDTournamentData;
    	HDTournamentDefinition def = tournamentData.definition as HDTournamentDefinition;
    	if ( def.m_build.pets.Count > 0 )
    	{
			ret = def.m_build.pets;
		}
		else
		{
			ret = UsersManager.currentUser.GetEquipedPets(GetToUseDragon());
		}
		return ret;
    }*/

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
		
		long ret = -1;
		switch ( m_tournamentDefinition.m_goal.m_mode )
		{
			case HDTournamentDefinition.TournamentGoal.TournamentMode.NORMAL:
			{
				if ( m_tracker != null )
					ret = m_tracker.currentValue;
			}break;
			case HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_ATTACK:
			{
				// Game duration!
				ret = (long)m_timePlayed;
			}break;
			case HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_LIMIT:
			{
				if ( m_tracker != null )
					ret = m_tracker.currentValue;
			}break;
		}

		return ret;
	}

	/// <summary>
	/// The overall best score (the one registered in the leaderboard)
	/// </summary>
	public long GetBestScore() {
		return m_tournamentData.m_score;
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
	// REWARDS METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// List of rewards for this player based on its final position in the leaderboard.
	/// Should've been previously requested to the server and waited for the response 
	/// to arrive (RequestMyRewards() and AreRewardsReady() methods).
	/// Doesn't actually check tournament state.
	/// </summary>
	/// <returns>The rewards to be given.</returns>
	public override List<HDLiveData.Reward> GetMyRewards() {
		// Create new list
		List<HDLiveData.Reward> rewards = new List<HDLiveData.Reward>();

		// We must have a valid data and definition
		if(data != null && data.definition != null) {
			// Check reward level
			if(m_rewardLevel > 0) {
				// In a tournament, the reward level tells us in which reward tier we have ended within the leaderboard
				// Only one reward is given
				HDTournamentDefinition def = data.definition as HDTournamentDefinition;
				rewards.Add(def.m_rewards[m_rewardLevel - 1]);	// Assuming rewards are properly sorted :)
			}
		}

        if (data.m_state == HDLiveEventData.State.REWARD_AVAILABLE) {
            data.m_state = HDLiveEventData.State.REWARD_COLLECTED;
        }

        // Done!
        return rewards;
	}


	public int GetCurrentMatchmakingValue()
	{
		int ret = 0;
		if ( m_data.m_state >= HDLiveEventData.State.JOINED && m_tournamentData.m_matchmakingValue >= 0)
		{
			ret = m_tournamentData.m_matchmakingValue;
		}
		else
		{
			ret = (int) UsersManager.currentUser.GetHighestDragon().tier;
		}
		return ret;
	}


	//------------------------------------------------------------------------//
	// UI HELPER METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Gets the description of this tournament localized and properly formatted.
	/// Override to customize text in specific objective types.
	/// </summary>
	/// <returns>The description properly formatted.</returns>
	public virtual string GetDescription() {
		// Depends on tournament mode
		switch(m_tournamentDefinition.m_goal.m_mode) {
			case HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_LIMIT: {
				// Add formatted time limit
				return LocalizationManager.SharedInstance.Localize(
					m_tournamentDefinition.m_goal.m_desc,
					TimeUtils.FormatTime(m_tournamentDefinition.m_goal.m_seconds, TimeUtils.EFormat.WORDS_WITHOUT_0_VALUES, 3)
				);
			} break;

			case HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_ATTACK: {
				// Add target amount
				return LocalizationManager.SharedInstance.Localize(
					m_tournamentDefinition.m_goal.m_desc,
					FormatScore(m_tournamentDefinition.m_goal.m_targetAmount)
				);
			} break;

			case HDTournamentDefinition.TournamentGoal.TournamentMode.NORMAL:
			default: {
				// Just translation as is
				return LocalizationManager.SharedInstance.Localize(m_tournamentDefinition.m_goal.m_desc);
			} break;
		}
	}

	/// <summary>
	/// Given a score, format it based on tournament type
	/// </summary>
	public string FormatScore(long _score) {
		// Seconds
        if ( IsTimeBasedScore() )
		{
			return TimeUtils.FormatTime((double)_score, TimeUtils.EFormat.DIGITS, 2, TimeUtils.EPrecision.MINUTES, true);	// MM:SS
		}

		return StringUtils.FormatNumber(_score);
	}

    public bool IsTimeBasedScore() {
        return m_tournamentDefinition.m_goal.m_mode == HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_ATTACK || m_tournamentDefinition.m_goal.m_type.Contains("time");
    }
}