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

    public const string ENTRANCE_TYPE_OPEN = "open";

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

	public HDLiveData.DragonBuild m_build = new HDLiveData.DragonBuild();

    protected IDragonData m_dragonData = null;
	public IDragonData dragonData {
		get { return m_dragonData; }
	}

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
		public string m_spawnPoint = "";
        public string m_spawnPointTID = "";
		public float m_progressionSeconds = 0f;
		public int m_progressionXP = 0;

		public override void Clear ()
		{
			base.Clear ();
			m_mode = TournamentMode.NORMAL;
			m_seconds = -1;
			m_targetAmount = -1;
			m_loops = -1;
			
			m_area = "";
			m_spawnPoint = "";
			m_progressionSeconds = 0f;
			m_progressionXP = 0;
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
							m_targetAmount = PersistenceUtils.SafeParse<long>(gameMode["amount"]);
					}break;
					case "2":
					case "time_limit":{
						m_mode = TournamentMode.TIME_LIMIT;
						if ( gameMode.ContainsKey("seconds") )
							m_seconds = PersistenceUtils.SafeParse<long>(gameMode["seconds"]);
					}break;
					case "3":
					case "race":{
						m_mode = TournamentMode.RACE;
						if ( gameMode.ContainsKey("loops") )
							m_loops = PersistenceUtils.SafeParse<int>(gameMode["loops"]);
					}break;
					case "4":
					case "boss":{
						m_mode = TournamentMode.BOSS;
						// boss config?
					}break;
				}
			}

			if (_data.ContainsKey("area")) {
				JSONNode area = _data["area"];

				m_spawnPoint = area.GetSafe("spawnPoint", "");

				m_progressionXP = PersistenceUtils.SafeParse<int>(area.GetSafe("xp", "0"));
				m_progressionSeconds = PersistenceUtils.SafeParse<float>(area.GetSafe("time", "0"));

				DefinitionNode spawnPointDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LEVEL_SPAWN_POINTS, m_spawnPoint);
				if (spawnPointDef != null) {
					m_area = spawnPointDef.Get("area");
                    m_spawnPointTID = spawnPointDef.Get("tidName");
                } else {
                    Debug.Log("Spawning point with sku=" + m_spawnPoint + " not defined in xml");
					m_area = "";
                    m_spawnPointTID = "";

                }
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
                    gameMode.Add("amount", PersistenceUtils.SafeToString(m_targetAmount));

				}break;
				case TournamentMode.TIME_LIMIT:
				{
                    gameMode.Add("type", "time_limit");
                    gameMode.Add("seconds", PersistenceUtils.SafeToString(m_seconds));
				}break;
				case TournamentMode.RACE:
				{
                    gameMode.Add("type", "race");
                    gameMode.Add("loops", PersistenceUtils.SafeToString(m_loops));
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

			SimpleJSON.JSONClass area = new JSONClass();
			{				
				area.Add("spawnPoint", m_spawnPoint);
				area.Add("xp", PersistenceUtils.SafeToString(m_progressionXP));
				area.Add("time", PersistenceUtils.SafeToString(m_progressionSeconds));
			}
			ret.Add("area", area);
			
			return ret;
		}
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
				type = PersistenceUtils.SafeParse<int>(_data["type"]);
			}

			if(_data.ContainsKey("segmentation")) {
				segmentation = PersistenceUtils.SafeParse<int>(_data["segmentation"]);
			}

			if(_data.ContainsKey("matchmaker")) {
				JSONClass matchmakerData = _data["matchmaker"] as JSONClass;
				if(matchmakerData.ContainsKey("type")) {
					matchmakerType = PersistenceUtils.SafeParse<int>(matchmakerData["type"]);
				}
			}
		}

		public JSONClass ToJson() {
			JSONClass data = new JSONClass();
			data.Add("type", PersistenceUtils.SafeToString(type));
			data.Add("segmentation", PersistenceUtils.SafeToString(segmentation));
			{
				JSONClass _matchmaker = new JSONClass();
				_matchmaker.Add("type", PersistenceUtils.SafeToString(matchmakerType));
				data.Add("matchmaker", _matchmaker);
			}
			return data;
		}
	}

    public TournamentGoal m_goal = new TournamentGoal();

	public List<HDLiveData.RankedReward> m_rewards = new List<HDLiveData.RankedReward>();

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
        m_dragonData = null;
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
				m_entrance.m_amount = PersistenceUtils.SafeParse<long>(_entrance["amount"]);
			}

			if ( _entrance.ContainsKey("dailyFreeTimer") ){
				m_entrance.m_dailyFree = PersistenceUtils.SafeParse<int>(_entrance["dailyFreeTimer"]);
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
			m_build.LoadData( _data["build"] );

			// Create and initialize a new dragon data object with given build
			m_dragonData = IDragonData.CreateFromBuild(m_build);
			m_dragonData.Acquire(false);
		}

		if ( _data.ContainsKey("rewards") )
		{
			JSONArray arr = _data["rewards"].AsArray;
			for (int i = 0; i < arr.Count; i++) {
				HDLiveData.RankedReward r = new HDLiveData.RankedReward();
				r.LoadData(arr[i], HDTrackingManager.EEconomyGroup.REWARD_LIVE_EVENT, m_name);
				m_rewards.Add(r);
			}

			// Since we can't assume rewards are received sorted, do it now
			m_rewards.Sort(HDLiveData.Reward.SortByTarget);   // Will be sorted by target percentage

			// Compute min rank based on previous reward
			for(int i = 1; i < m_rewards.Count; ++i) {  // Skip first reward (min is always 0)
				m_rewards[i].InitMinRankFromPreviousReward(m_rewards[i - 1]);
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
		_entrance.Add("amount", PersistenceUtils.SafeToString(m_entrance.m_amount));
		_entrance.Add("dailyFreeTimer", PersistenceUtils.SafeToString(m_entrance.m_dailyFree));
		ret.Add("entrance", _entrance);

		// Leaderboard
		ret.Add("leaderboard", m_leaderboard.ToJson());

		// Build
		ret.Add("build", m_build.SaveData());

		return ret;
	}

}