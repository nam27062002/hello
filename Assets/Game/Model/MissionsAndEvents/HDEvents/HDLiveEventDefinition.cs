// HDEventDefinition.cs
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
public class HDLiveEventDefinition {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum EventType{
		NONE,
		TOURNAMENT,
		QUEST,
		PASSIVE
	};
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public int m_eventId;
	public string m_name;
	public EventType m_type;
	public List<string> m_mods;

	public long m_teasingTimestamp = 0;
	public long m_startTimestamp = 0;
	public long m_endTimestamp = 0;

	// Goal?
	// Build?
	// Rewards?

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDLiveEventDefinition() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDLiveEventDefinition() {
	}

	/// <summary>
	/// Clean this instance. Remove all information so this definition is not valid
	/// </summary>
	public virtual void Clean()
	{
		m_eventId = -1;
		m_name = "";
		m_type = EventType.NONE;
		m_mods.Clear();

		m_teasingTimestamp = -1;
		m_startTimestamp = -1;
		m_endTimestamp = -1;
	}

	public virtual void ParseInfo( SimpleJSON.JSONNode _data )
	{
		if ( _data.ContainsKey("code") )
		{
			m_eventId = _data["code"].AsInt;
		}

		if ( _data.ContainsKey("name") )
		{
			m_name = _data["name"];
		}
		// type?

		// Mods
		m_mods.Clear();
		if (_data.ContainsKey("mods"))
		{
			JSONArray _mods = _data["mods"].AsArray;

			for (int i = 0; i < _mods.Count; ++i)
			{
				m_mods.Add(_mods[i]);
			}
		}

		// timestamps
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}