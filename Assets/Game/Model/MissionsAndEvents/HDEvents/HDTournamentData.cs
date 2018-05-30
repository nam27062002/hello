// HDTournament.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 16/05/2018.
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
public class HDTournamentData : HDLiveEventData {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	public class LeaderboardLine
	{
		public string m_name;
		public string m_pic;
		public int m_score;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public List<LeaderboardLine> m_leaderboard = new List<LeaderboardLine>();
	private DateTime m_leaderboardCheckTimestamp = new DateTime();

	public long m_rank = -1;
	public long m_score = -1;
	public long m_tournamentSize = -1;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDTournamentData() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDTournamentData() {

	}

	protected override void BuildDefinition()
	{
		m_definition = new HDTournamentDefinition();
	}


	public override void Clean()
	{
		m_leaderboard.Clear();
		m_rank = -1;
		m_score = -1;
		m_tournamentSize = -1;
	}


	public override void ParseState( SimpleJSON.JSONNode _data )
	{
		base.ParseState( _data );
	}


	public void ParseLeaderboard( SimpleJSON.JSONNode _data )
	{
		if ( _data.ContainsKey("u") )
		{
			// user info
			m_rank = _data["u"]["rank"].AsLong;
			m_score = _data["u"]["score"].AsLong;
		}

		if ( _data.ContainsKey("l") )
		{
			JSONArray arr = _data["l"].AsArray;
			int max = arr.Count;

			if ( m_leaderboard.Count > max )
			{
				m_leaderboard.RemoveRange( max, m_leaderboard.Count - max);
			}
			else if ( m_leaderboard.Count < max )
			{
				while( m_leaderboard.Count < max )
				{
					m_leaderboard.Add( new LeaderboardLine() );
				}
			}

			for (int i = 0; i < max; i++) {
				LeaderboardLine l = m_leaderboard[i];
				l.m_name = arr[i]["name"];
				l.m_pic = arr[i]["pic"];
				l.m_score = arr[i]["score"];
			}
		}

		if ( _data.ContainsKey("n") )
		{
			m_tournamentSize = _data["n"].AsLong;
		}
	}

}