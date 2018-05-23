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
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDLiveEventData() {

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
		HDLiveEventDefinition def = GetEventDefinition();
		if (def != null)
			def.Clean();
	}

	public virtual SimpleJSON.JSONClass ToJson ()
	{
		SimpleJSON.JSONClass ret = new SimpleJSON.JSONClass();
		ret.Add("code", m_eventId);
		return ret;
	}

	public virtual void ParseState( SimpleJSON.JSONNode _data )
	{
		m_eventId = _data["code"];

	}

	public virtual void ParseDefinition( SimpleJSON.JSONNode _data )
	{
		HDLiveEventDefinition def = GetEventDefinition();
		if (def != null)
		{
			def.ParseInfo( _data );
		}

	}

	public virtual void ParseProgress( SimpleJSON.JSONNode _data )
	{

	}

	public virtual HDLiveEventDefinition GetEventDefinition()
	{
		return null;
	}

}