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
		public int m_amount;
		public int m_dailyFree;

		public void Clean()
		{
			m_type = "";
			m_amount = 0;
			m_dailyFree = 100000000;
		}
	}
	public TournamentEntrance m_entrance;

	public struct TournamentBuild
	{
		public string m_dragon;
		public string m_skin;
		public List<string> m_pets;

		public void Clean()
		{
			m_dragon = "";
			m_skin = "";
			if ( m_pets != null )
				m_pets.Clear();
		}
	}

	public TournamentBuild m_build = new TournamentBuild();

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
				string modeStr = _data["gameMode"];
				switch( modeStr )
				{
					case "time_attack":{
						m_mode = TournamentMode.TIME_ATTACK;
						if ( _data.ContainsKey("amount") )
							m_targetAmount = _data["amount"].AsLong;
					}break;
					case "time_limit":{
						m_mode = TournamentMode.TIME_LIMIT;
						if ( _data.ContainsKey("seconds") )
							m_seconds = _data["seconds"].AsLong;
					}break;
					case "race":{
						m_mode = TournamentMode.RACE;
						if ( _data.ContainsKey("loops") )
							m_loops = _data["loops"].AsInt;
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

	public TournamentGoal m_goal = new TournamentGoal();

	public List<HDLiveEventReward> m_rewards = new List<HDLiveEventReward>();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDTournamentDefinition() {
		m_build.m_pets = new List<string>();
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
				m_entrance.m_amount = _entrance["amount"].AsInt;
			}

			if ( _entrance.ContainsKey("dailyFreeTimer") ){
				m_entrance.m_dailyFree = _entrance["dailyFreeTimer"].AsInt;
			}
		}

		if ( _data.ContainsKey("leaderboard_size") )
		{
			
		}

		// Build
		if ( _data.ContainsKey("build") )
		{
			JSONClass _build = _data["build"].AsObject;
			if ( _build.ContainsKey("dragon") ){
				m_build.m_dragon = _build["dragon"];
			}

			if ( _build.ContainsKey("skin") ){
				m_build.m_skin = _build["skin"];
			}

			if ( _build.ContainsKey("pets") ){
				JSONArray _pets = _build["pets"].AsArray;
				for (int i = 0; i < _pets.Count; i++) {
					m_build.m_pets.Add( _pets[i] );
				}
			}
		}


		if ( _data.ContainsKey("rewards") )
		{
			JSONArray arr = _data["rewards"].AsArray;
			for (int i = 0; i < arr.Count; i++) {
				m_rewards.Add( new HDLiveEventReward( arr[i], m_name) );
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

		// Build
		SimpleJSON.JSONClass _build = new JSONClass();
		_build.Add("dragon", m_build.m_dragon);
		_build.Add("skin", m_build.m_skin);
		JSONArray arr = new JSONArray();
		for (int i = 0; i < m_build.m_pets.Count; i++) {
				arr.Add(m_build.m_pets[i]);
		}
		_build.Add("pets", arr);
		ret.Add("build", _build);


		return ret;
	}

}