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
		public string m_area = "";
		public override void Clear ()
		{
			base.Clear ();
			m_area = "";
		}

		public override void ParseGoal (JSONNode _data)
		{
			base.ParseGoal (_data);
			if ( _data.ContainsKey("area") ){
				m_area = _data["area"];
			}
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