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
		public string m_name = "";
		public string m_pic = "";
		public int m_score = -1;
		public int m_rank = -1;
		public HDLiveData.DragonBuild m_build = new HDLiveData.DragonBuild();
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public List<LeaderboardLine> m_leaderboard = new List<LeaderboardLine>();

	public long m_rank = -1;
	public long m_score = -1;
	public long m_tournamentSize = -1;
	public int m_matchmakingValue = -1;	// Value used to know in what league you are and to scale rewards and entrance

	private long m_lastFreeEntranceTimestamp = 0;
	public long lastFreeEntranceTimestamp
	{	
		get{ return m_lastFreeEntranceTimestamp; }
		set{ m_lastFreeEntranceTimestamp = value; }
	}
		// Default tournament config
	protected HDLiveData.DragonBuild m_defaultBuild = new HDLiveData.DragonBuild();


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDTournamentData() {
        m_definition = new HDTournamentDefinition();
	}	

	public override void Clean()
	{
		base.Clean();

		m_leaderboard.Clear();
		m_rank = -1;
		m_score = -1;
		m_tournamentSize = -1;
		m_matchmakingValue = -1;
		m_lastFreeEntranceTimestamp = 0;

		m_defaultBuild.Clean();

	}

	public override SimpleJSON.JSONClass ToJson ()
	{
		JSONClass ret = base.ToJson();

		ret.Add("lastFreeTournamentRun", m_lastFreeEntranceTimestamp);
		ret.Add("elo", m_matchmakingValue);

		// Leadeboard
		JSONClass leaderboard = new JSONClass();
		leaderboard.Add("n", m_tournamentSize);

		JSONArray arr = new JSONArray();
		int max = m_leaderboard.Count;
		for (int i = 0; i < max; i++) {
			JSONClass line = new JSONClass();
			line.Add("name");
			line.Add("pic");
			line.Add("score");
			arr.Add(line);
		}
		leaderboard.Add("l", arr);

		JSONClass u = new JSONClass();
		u.Add("rank", m_rank);
		u.Add("score", m_score);
		leaderboard.Add("u", u);

		ret.Add("leaderboard", leaderboard);

		return ret;
	}

	public override void ParseState( SimpleJSON.JSONNode _data )
	{
		base.ParseState( _data );

		if ( _data.ContainsKey("lastFreeTournamentRun") )
		{
			m_lastFreeEntranceTimestamp = _data["lastFreeTournamentRun"].AsLong;
		}

		if ( _data.ContainsKey("elo") )
		{
			m_matchmakingValue = _data["elo"].AsInt;
		}

		if ( _data.ContainsKey("leaderboard") )	// This comes from a saves data
		{
			ParseLeaderboard( _data["leaderboard"] );
		}
	}


	public void ParseLeaderboard( SimpleJSON.JSONNode _data )
	{
		if ( _data.ContainsKey("u") )
		{
			// user info
			m_rank = _data["u"]["rank"].AsLong;
			m_score = _data["u"]["score"].AsLong;
			m_defaultBuild.FromJson(_data["u"]["build"]);
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
				if (arr[i].ContainsKey("build"))
					l.m_build.FromJson(arr[i]["build"]);
				else
					l.m_build.Clean();
				l.m_rank = i;
			}
		}

		if ( _data.ContainsKey("n") )
		{
			m_tournamentSize = _data["n"].AsLong;
		}
	}

}