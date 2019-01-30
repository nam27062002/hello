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
public abstract class HDLiveEventData {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public int m_eventId;
	public enum State
	{
		NONE,
		TEASING,
		NOT_JOINED,
		JOINED,
		REWARD_AVAILABLE,
        REWARD_COLLECTED,
		FINALIZED,
		REFUND,
        REQUIRES_UPDATE
	};
	public State m_state = State.NONE;

	protected HDLiveEventDefinition m_definition;
	public HDLiveEventDefinition definition
	{	
		get { return m_definition; }
	}

	public TimeSpan remainingTime {	// Dynamic, depending on current state
		get { 
			DateTime now = GameServerManager.SharedInstance.GetEstimatedServerTime();
			switch(m_state) {
				case State.TEASING:	return m_definition.m_startTimestamp - now;		break;
                case State.REQUIRES_UPDATE:
				case State.NOT_JOINED:
				case State.JOINED:	return m_definition.m_endTimestamp - now;		break;
				default:			return new TimeSpan();				break;
			}
		}
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDLiveEventData() {
		m_definition = new HDLiveEventDefinition();
	}

	/// <summary>
	/// Clean this instance. Remove all the information so no event is here
	/// </summary>
	public virtual void Clean()
	{
		m_eventId = -1;
		m_state = State.NONE;
		// m_definition.Clean();
	}

	/// <summary>
	/// Check for state changes based on timestamps.
	/// </summary>
	public virtual void UpdateStateFromTimers()
	{
		if ( m_eventId > 0 && definition.m_eventId == m_eventId)
		{
			if (m_definition.m_refund) {
				m_state = State.REFUND;
			} else {
				DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
				if ( serverTime < m_definition.m_startTimestamp )
				{
					m_state = State.TEASING;
				}
				else if ( m_state < State.REWARD_AVAILABLE )
				{
					if ( serverTime > m_definition.m_endTimestamp )
					{
						// if I was playing this event I need to check the reward, otherwise I can finalize it direclty
						if ( m_state == State.JOINED )	
						{
							m_state = State.REWARD_AVAILABLE;
						}
						else
						{
							m_state = State.FINALIZED;
						}
					}
					else if ( m_state == State.TEASING )
					{
						m_state = State.NOT_JOINED;
					}
				}
				else if ( m_state == State.NONE )
				{
					m_state = State.NOT_JOINED;
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
			case State.NOT_JOINED:
			{
				stateStr = "not_joined";
			}break;
			case State.FINALIZED:
			{
				stateStr = "finalized";
			}break;
			case State.REWARD_AVAILABLE:
			{
				stateStr = "pending_rewards";
			}break;
            case State.REWARD_COLLECTED: 
            {
                stateStr = "collected_rewards";
            }break;
            case State.JOINED:
			{
				stateStr = "joined";
			}break;
            case State.REQUIRES_UPDATE:
            {
                stateStr = "requires_update";
            }break;
		}
		ret.Add("status", stateStr);

		if ( m_definition.m_eventId == m_eventId )
		{
			ret.Add("definition", m_definition.ToJson());
		}

		return ret;
	}

	/// <summary>
	/// Parses the state of this event. If it comes from cache it contains the definition.
	/// </summary>
	/// <param name="_data">Data.</param>
	public virtual void ParseState( SimpleJSON.JSONNode _data )
	{
		Clean();

		if (_data.ContainsKey("code"))
			m_eventId = _data["code"];
        if ( m_definition.m_eventId != m_eventId )
			m_definition.Clean();

		m_state = State.NONE;
		if ( _data.ContainsKey("status") )
		{
			string stateStr = _data["status"];
			switch( stateStr )
			{
				case "0":
				case "not_joined":
				{
					m_state = State.NOT_JOINED;
				}break;
				case "1":
				case "joined":
				{
					m_state = State.JOINED;
				}break;
				case "2":
				case "pending_rewards":
				{
					m_state = State.REWARD_AVAILABLE;
				}break;
                case "collected_rewards":
                {
                    m_state = State.REWARD_COLLECTED;
                }break;
                case "finalized":
				{
					m_state = State.FINALIZED;
				}break;
				case "5":
				case "pending_refund":
				{
					m_state = State.REFUND;
				}break;
                case "6":
                case "requires_update":
                {
                    m_state = State.REQUIRES_UPDATE;
                }break;
			}
		}

		if ( _data.ContainsKey("definition") )
		{
			ParseDefinition( _data["definition"] );
		}
        
	}

	public virtual void ParseDefinition( SimpleJSON.JSONNode _data )
	{
		m_definition.ParseInfo( _data );
		UpdateStateFromTimers();
	}

}