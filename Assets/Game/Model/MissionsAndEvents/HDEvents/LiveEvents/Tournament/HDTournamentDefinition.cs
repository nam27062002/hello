// HDTournamentDefinition.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 24/05/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;
using SimpleJSON;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class HDTournamentDefinition : HDLiveEventDefinition{
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	public struct TournamentEntrance
	{
		public string m_type;
		public long m_amount;
		public int m_dailyFree;

		public void Clean()
		{
			m_type = "";
			m_amount = 0;
			m_dailyFree = 100000000;
		}
	}
	public TournamentEntrance m_entrance = new TournamentEntrance();

	public LeaderboardData m_leaderboard = new LeaderboardData();

	public HDTournamentBuild m_build = new HDTournamentBuild();

	public class TournamentGoal : GoalCommon
	{
		public enum TournamentMode{
			NORMAL,
			TIME_ATTACK,
			TIME_LIMIT,
			RACE,
			BOSS
		}
		public TournamentMode m_mode;
		public long m_seconds;
		public long m_targetAmount;
		public int m_loops;
		public string m_area = "";

		public override void Clear ()
		{
			base.Clear ();
			m_mode = TournamentMode.NORMAL;
			m_seconds = -1;
			m_targetAmount = -1;
			m_loops = -1;
			m_area = "";
		}

		public override void ParseGoal (JSONNode _data)
		{
			base.ParseGoal (_data);

			if ( _data.ContainsKey("gameMode") )
			{
				JSONNode gameMode = _data["gameMode"];
				string modeStr = gameMode["type"];

				switch( modeStr )
				{
					case "0":
					case "normal":
					default:
					{
						m_mode = TournamentMode.NORMAL;
					}break;

					case "1":
					case "time_attack":{
						m_mode = TournamentMode.TIME_ATTACK;
						if ( gameMode.ContainsKey("amount") )
							m_targetAmount = gameMode["amount"].AsLong;
					}break;
					case "2":
					case "time_limit":{
						m_mode = TournamentMode.TIME_LIMIT;
						if ( gameMode.ContainsKey("seconds") )
							m_seconds = gameMode["seconds"].AsLong;
					}break;
					case "3":
					case "race":{
						m_mode = TournamentMode.RACE;
						if ( gameMode.ContainsKey("loops") )
							m_loops = gameMode["loops"].AsInt;
					}break;
					case "4":
					case "boss":{
						m_mode = TournamentMode.BOSS;
						// boss config?
					}break;
				}
			}

			if ( _data.ContainsKey("area") ){
				m_area = _data["area"];
			}
		}

		public override SimpleJSON.JSONClass ToJson ()
		{
			SimpleJSON.JSONClass ret = base.ToJson();

            SimpleJSON.JSONClass gameMode = new SimpleJSON.JSONClass();

			switch(m_mode)
			{
				case TournamentMode.TIME_ATTACK:
				{
                    gameMode.Add("type", "time_attack");
                    gameMode.Add("amount", m_targetAmount);

				}break;
				case TournamentMode.TIME_LIMIT:
				{
                    gameMode.Add("type", "time_limit");
                    gameMode.Add("seconds", m_seconds);
				}break;
				case TournamentMode.RACE:
				{
                    gameMode.Add("type", "race");
                    gameMode.Add("loops", m_loops);
				}break;
				case TournamentMode.BOSS:
				{
                    gameMode.Add("type", "boss");
				}break;
				default:
				{
                    gameMode.Add("type", "normal");
				}break;
			}
            ret.Add("gameMode", gameMode);
			ret.Add("area", m_area);
			return ret;
		}
	}

	public class TournamentReward : HDLiveData.Reward {
		public RangeInt ranks = new RangeInt(0, 100);
	}

	public class LeaderboardData {
		public int type = -1;
		public int segmentation = -1;
		public int matchmakerType = -1;

		public void Clean() {
			type = -1;
			segmentation = -1;
			matchmakerType = -1;
		}

		public void ParseJson(JSONNode _data) {
			if(_data.ContainsKey("type")) {
				type = _data["type"].AsInt;
			}

			if(_data.ContainsKey("segmentation")) {
				segmentation = _data["segmentation"].AsInt;
			}

			if(_data.ContainsKey("matchmaker")) {
				JSONClass matchmakerData = _data["matchmaker"] as JSONClass;
				if(matchmakerData.ContainsKey("type")) {
					matchmakerType = matchmakerData["type"].AsInt;
				}
			}
		}

		public JSONClass ToJson() {
			JSONClass data = new JSONClass();
			data.Add("type", type);
			data.Add("segmentation", segmentation);
			{
				JSONClass _matchmaker = new JSONClass();
				_matchmaker.Add("type", matchmakerType);
				data.Add("matchmaker", _matchmaker);
			}
			return data;
		}
	}

    public TournamentGoal m_goal = new TournamentGoal();

	public List<TournamentReward> m_rewards = new List<TournamentReward>();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDTournamentDefinition() {
		
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDTournamentDefinition() {

	}

	public override void Clean()
	{
		base.Clean();
		m_entrance.Clean();
		m_leaderboard.Clean();
		m_build.Clean();
		m_rewards.Clear();
	}

	public override void ParseInfo( SimpleJSON.JSONNode _data )
	{
		base.ParseInfo(_data);

		if ( _data.ContainsKey("goal") ){
			m_goal.ParseGoal( _data["goal"] );
		}

			// Entrance
		if ( _data.ContainsKey("entrance") )
		{
			JSONClass _entrance = _data["entrance"].AsObject;
			if ( _entrance.ContainsKey("type") ){
				m_entrance.m_type = _entrance["type"];
			}

			if ( _entrance.ContainsKey("amount") ){
				m_entrance.m_amount = _entrance["amount"].AsLong;
			}

			if ( _entrance.ContainsKey("dailyFreeTimer") ){
				m_entrance.m_dailyFree = _entrance["dailyFreeTimer"].AsInt;
			}
		}

		// Leaderboard
		if ( _data.ContainsKey("leaderboard") )
		{
			m_leaderboard.ParseJson(_data["leaderboard"]);
		}

		// Build
		if ( _data.ContainsKey("build") )
		{
			m_build.ParseBuild( _data["build"] );
		}

		if ( _data.ContainsKey("rewards") )
		{
			JSONArray arr = _data["rewards"].AsArray;
			for (int i = 0; i < arr.Count; i++) {
				TournamentReward r = new TournamentReward();
				r.ParseJson(arr[i], HDTrackingManager.EEconomyGroup.REWARD_LIVE_EVENT, m_name);

				// Compute ranks. Min can only be computed based on previous reward.
				// Since we can't assume rewards are received sorted, we'll do it afterwards in a separate loop.
				r.ranks.min = 0;
				r.ranks.max = Mathf.Max(0, (int)(r.target) - 1);	// 0-99

				m_rewards.Add(r);
			}

			// Sort by target percentage
			m_rewards.Sort(
				(TournamentReward _reward1, TournamentReward _reward2) => {
					return _reward1.target.CompareTo(_reward2.target);
				}
			);

			// Compute min rank based on previous reward
			for(int i = 1; i < m_rewards.Count; ++i) {	// Skip first reward (min is always 0)
				m_rewards[i].ranks.min = Mathf.Min(m_rewards[i - 1].ranks.max + 1, m_rewards[i].ranks.max);	// Starts where previous rank ends, but never bigger than our rank end
			}
		}
	}


	public override SimpleJSON.JSONClass ToJson ()
	{
		SimpleJSON.JSONClass ret = base.ToJson();

		// Goal
		ret.Add("goal", m_goal.ToJson());

		// Entrance
		SimpleJSON.JSONClass _entrance = new JSONClass();
		_entrance.Add("type", m_entrance.m_type);
		_entrance.Add("amount", m_entrance.m_amount);
		_entrance.Add("dailyFreeTimer", m_entrance.m_dailyFree);
		ret.Add("entrance", _entrance);

		// Leaderboard
		ret.Add("leaderboard", m_leaderboard.ToJson());

		// Build
		ret.Add("build", m_build.ToJson());

		return ret;
	}

}