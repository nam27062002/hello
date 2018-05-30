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

	private DateTime m_lastFreeEntranceTimestamp = new DateTime();

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
		m_lastFreeEntranceTimestamp = new DateTime(1970, 1, 1);
	}

	public override SimpleJSON.JSONClass ToJson ()
	{
		JSONClass ret = base.ToJson();

		ret.Add("Timestamp", TimeUtils.DateToTimestamp( m_lastFreeEntranceTimestamp ));

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
		ret.Add("leaderboardTimestamp", TimeUtils.DateToTimestamp( m_teasingTimestamp ));

		return ret;
	}

	public override void ParseState( SimpleJSON.JSONNode _data )
	{
		base.ParseState( _data );

		if ( _data.ContainsKey("Timestamp") )
		{
			m_lastFreeEntranceTimestamp = TimeUtils.TimestampToDate( _data["Timestamp"].AsLong );
		}

		if ( _data.ContainsKey("leaderboard") )	// This comes from a saves data
		{
			ParseLeaderboard( _data["leaderboard"] );
			if ( _data.ContainsKey("leaderboardTimestamp") )
			{
				m_leaderboardCheckTimestamp = TimeUtils.TimestampToDate( _data["leaderboardTimestamp"].AsLong );
			}
		}
	}


	public void ParseLeaderboard( SimpleJSON.JSONNode _data )
	{
		m_leaderboardCheckTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTime();

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