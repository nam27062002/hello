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
	public List<Modifier> m_dragonMods = new List<Modifier>();
	public List<Modifier> m_otherMods = new List<Modifier>();

	public long m_teasingTimestamp = 0;
	public long m_startTimestamp = 0;
	public long m_endTimestamp = 0;

	// Goal?
	public class GoalCommon
	{
		public string m_desc = "";
		public string m_icon = "";
		public string m_type = "";
		public DefinitionNode m_typeDef;

		public List<string> m_params = new List<string>();


		public virtual void Clear()
		{
			m_desc = "";
			m_icon = "";
			m_type = "";
			m_params.Clear();
		}

		public virtual void ParseGoal(SimpleJSON.JSONNode _data)
		{
			Clear();
			if (_data.ContainsKey("tidDesc")){
				m_desc = _data["tidDesc"];
			}
			if ( _data.ContainsKey("icon") ){
				m_icon = _data["icon"];
			}
			if ( _data.ContainsKey("type") ){
				m_type = _data["type"];
				m_typeDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_TYPES, m_type);
			}

			if ( _data.ContainsKey("params") ){
				JSONArray arr = _data["params"].AsArray;
				for (int i = 0; i < arr.Count; i++) {
					m_params.Add( arr[i] );
				}
			}

		}
	}
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
		m_dragonMods.Clear();
		m_otherMods.Clear();

		m_teasingTimestamp = -1;
		m_startTimestamp = -1;
		m_endTimestamp = -1;
	}

	public virtual void ParseInfo( SimpleJSON.JSONNode _data )
	{
		Clean();
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
		if (_data.ContainsKey("mods"))
		{
			JSONArray _mods = _data["mods"].AsArray;
			for (int i = 0; i < _mods.Count; ++i)
			{
				DefinitionNode modDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LIVE_EVENTS_MODIFIERS, _mods[i]);
				Modifier m = Modifier.CreateFromDefinition(modDef);
				if (m is ModifierDragon) {
					m_dragonMods.Add(m);
				} else {
					m_otherMods.Add(m);
				}
			}
		}

		// timestamps
	}


	public virtual SimpleJSON.JSONClass ToJson ()
	{
		SimpleJSON.JSONClass ret = new SimpleJSON.JSONClass();

		ret.Add("code", m_eventId);	
		ret.Add("name", m_name);

		// Type?

		SimpleJSON.JSONArray arr = new JSONArray();
		for (int i = 0; i < m_dragonMods.Count; i++) {
			arr.Add( m_dragonMods[i].def.sku );
		}
		for (int i = 0; i < m_otherMods.Count; i++) {
			arr.Add( m_otherMods[i].def.sku );
		}
		ret.Add("mods", arr);

		// timestamps?


		return ret;
	}

}