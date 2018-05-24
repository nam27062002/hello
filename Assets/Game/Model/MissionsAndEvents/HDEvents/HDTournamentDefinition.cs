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
			m_pets.Clear();
		}
	}

	public TournamentBuild m_build;

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

	public override void ParseInfo( SimpleJSON.JSONNode _data )
	{
		base.ParseInfo(_data);

			// Entrance
		m_entrance.Clean();
		if ( _data.ContainsKey("entrance") )
		{
			JSONClass _entrance = _data["entrance"].AsObject;
			if ( _entrance.ContainsKey("type") ){
				m_entrance.m_type = _entrance["type"];
			}

			if ( _entrance.ContainsKey("amount") ){
				m_entrance.m_amount = _entrance["amount"].AsInt;
			}

			if ( _entrance.ContainsKey("daily_free") ){
				m_entrance.m_dailyFree = _entrance["daily_free"].AsInt;
			}
		}

		if ( _data.ContainsKey("leaderboard_size") )
		{
			
		}

		// Build
		m_build.Clean();
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
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}