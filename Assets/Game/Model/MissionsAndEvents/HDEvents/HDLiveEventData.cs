// HDEvent.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 16/05/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class HDLiveEventData {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public int m_eventId;
	public string m_name;
	public enum State
	{
		NONE,
		AVAILABLE,
		RUNNING,
		REWARD_AVAILABLE,
		FINALIZED
	};
	public State m_state = State.NONE;

	public DateTime m_teasingTimestamp = new DateTime();
	public DateTime m_startTimestamp = new DateTime();
	public DateTime m_endTimestamp = new DateTime();

	protected HDLiveEventDefinition m_definition;
	public HDLiveEventDefinition definition
	{	
		get { return m_definition; }
	}
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDLiveEventData() {
		BuildDefinition();
	}

	protected virtual void BuildDefinition()
	{
		m_definition = new HDLiveEventDefinition();
	}


	/// <summary>
	/// Destructor
	/// </summary>
	~HDLiveEventData() {

	}

	/// <summary>
	/// Clean this instance. Remove all the information so no event is here
	/// </summary>
	public virtual void Clean()
	{
		m_eventId = -1;
		m_name = "";
		m_state = State.NONE;
		m_teasingTimestamp = new DateTime(1970, 1, 1);
		m_startTimestamp = new DateTime(1970, 1, 1);
		m_endTimestamp = new DateTime(1970, 1, 1);
		m_definition.Clean();
	}

	public virtual void UpdateStateFromTimers()
	{
		if ( m_eventId > 0 )
		{
			DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
			if ( m_state < State.REWARD_AVAILABLE )
			{
				if ( serverTime > m_endTimestamp )
				{
					// if I was playing this event I need to check the reward, otherwise I can finalize it direclty
					if ( m_state == State.RUNNING )	
					{
						m_state = State.REWARD_AVAILABLE;
					}
					else
					{
						m_state = State.FINALIZED;
					}
				}
			}
		}
	}

	public virtual SimpleJSON.JSONClass ToJson ()
	{
		SimpleJSON.JSONClass ret = new SimpleJSON.JSONClass();
		ret.Add("code", m_eventId);

		string stateStr = "none";
		switch( m_state )
		{
			case State.AVAILABLE:
			{
				stateStr = "not_joined";
			}break;
			case State.FINALIZED:
			{
				stateStr = "finalized";
			}break;
			case State.REWARD_AVAILABLE:
			{
				stateStr = "penging_rewards";
			}break;
			case State.RUNNING:
			{
				stateStr = "joined";
			}break;
		}
		ret.Add("state", stateStr);

		ret.Add("teaserTimestamp", TimeUtils.DateToTimestamp( m_teasingTimestamp ));
		ret.Add("startTimestamp", TimeUtils.DateToTimestamp( m_startTimestamp ));
		ret.Add("endTimestamp", TimeUtils.DateToTimestamp( m_endTimestamp ));

		if ( m_definition.m_eventId == m_eventId )
		{
			ret.Add("definition", m_definition.ToJson());
		}

		return ret;
	}

	public virtual void ParseState( SimpleJSON.JSONNode _data )
	{
		Clean();

		m_eventId = _data["code"];
        if ( m_definition.m_eventId != m_eventId )
			m_definition.Clean();

		m_state = State.NONE;
		if ( _data.ContainsKey("state") )
		{
			string stateStr = _data["state"];
			switch( stateStr )
			{
				case "not_joined":
				{
					m_state = State.AVAILABLE;
				}break;
				case "finalized":
				{
					m_state = State.FINALIZED;
				}break;
				case "penging_rewards":
				{
					m_state = State.REWARD_AVAILABLE;
				}break;
				case "joined":
				{
					m_state = State.RUNNING;
				}break;
			}
		}

		if ( _data.ContainsKey("teaserTimestamp") )
			m_teasingTimestamp = TimeUtils.TimestampToDate(_data["teaserTimestamp"].AsLong);

		if ( _data.ContainsKey("startTimestamp") )
			m_startTimestamp = TimeUtils.TimestampToDate(_data["startTimestamp"].AsLong);

		if ( _data.ContainsKey("endTimestamp") )
			m_endTimestamp = TimeUtils.TimestampToDate(_data["endTimestamp"].AsLong);

		if ( _data.ContainsKey("definition") )
		{
			m_definition.ParseInfo( _data["definition"] );
		}
        
	}

	public virtual void ParseDefinition( SimpleJSON.JSONNode _data )
	{
		m_definition.ParseInfo( _data );
	}

}