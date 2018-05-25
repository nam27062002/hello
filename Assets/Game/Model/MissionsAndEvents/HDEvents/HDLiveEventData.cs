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
		m_definition.Clean();
	}

	public virtual SimpleJSON.JSONClass ToJson ()
	{
		SimpleJSON.JSONClass ret = new SimpleJSON.JSONClass();
		ret.Add("code", m_eventId);

		if ( m_definition.m_eventId == m_eventId )
		{
			ret.Add("definition", m_definition.ToJson());
		}

		return ret;
	}

	public virtual void ParseState( SimpleJSON.JSONNode _data )
	{
		m_eventId = _data["code"];
        if ( m_definition.m_eventId != m_eventId )
			m_definition.Clean();

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