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
	public TournamentEntrance m_entrance;

	public long m_leaderboardSegmentation = -1;

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
					case "time_attack":{
						m_mode = TournamentMode.TIME_ATTACK;
						if ( gameMode.ContainsKey("amount") )
							m_targetAmount = gameMode["amount"].AsLong;
					}break;
					case "time_limit":{
						m_mode = TournamentMode.TIME_LIMIT;
						if ( gameMode.ContainsKey("seconds") )
							m_seconds = gameMode["seconds"].AsLong;
					}break;
					case "race":{
						m_mode = TournamentMode.RACE;
						if ( gameMode.ContainsKey("loops") )
							m_loops = gameMode["loops"].AsInt;
					}break;
					case "boss":{
						m_mode = TournamentMode.BOSS;
						// boss config?
					}break;
					case "normal":
					default:
					{
						m_mode = TournamentMode.NORMAL;
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
			switch(m_mode)
			{
				case TournamentMode.TIME_ATTACK:
				{
					ret.Add("gameMode", "time_attack");
					ret.Add("amount", m_targetAmount);

				}break;
				case TournamentMode.TIME_LIMIT:
				{
					ret.Add("gameMode", "time_limit");
					ret.Add("seconds", m_seconds);
				}break;
				case TournamentMode.RACE:
				{
					ret.Add("gameMode", "race");
					ret.Add("loops", m_loops);
				}break;
				case TournamentMode.BOSS:
				{
					ret.Add("gameMode", "boss");
				}break;
				default:
				{
					ret.Add("gameMode", "normal");
				}break;
			}
			ret.Add("area", m_area);
			return ret;
		}
	}

	public class TournamentReward : HDLiveEventReward {
		public RangeInt ranks = new RangeInt(0, 100);
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
		m_leaderboardSegmentation = -1;
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

		if ( _data.ContainsKey("leaderboardSegmentation") )
		{
			m_leaderboardSegmentation = _data["leaderboardSegmentation"].AsLong;
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
				r.ParseJson(arr[i], m_name);

				// Compute ranks. Min can only be computed based on previous reward.
				// Since we can't assume rewards are received sorted, we'll do it afterwards in a separate loop.
				r.ranks.min = 0;
				r.ranks.max = Mathf.Max(0, Mathf.FloorToInt(r.targetPercentage * (float)m_leaderboardSegmentation) - 1);	// 0-99

				m_rewards.Add(r);
			}

			// Sort by target percentage
			m_rewards.Sort(
				(TournamentReward _reward1, TournamentReward _reward2) => {
					return _reward1.targetPercentage.CompareTo(_reward2.targetPercentage);
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

		// Entrance
		SimpleJSON.JSONClass _entrance = new JSONClass();
		_entrance.Add("type", m_entrance.m_type);
		_entrance.Add("amount", m_entrance.m_amount);
		_entrance.Add("dailyFreeTimer", m_entrance.m_dailyFree);
		ret.Add("entrance", _entrance);

		ret.Add("leaderboardSegmentation", m_leaderboardSegmentation);

		// Build
		SimpleJSON.JSONClass _build = m_build.ToJson();
		ret.Add("build", _build);

		return ret;
	}

}