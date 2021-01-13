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
	protected bool m_initialized = false;
	public bool initialized {
        get { return m_initialized; }
	}

	public int m_eventId;
	public string m_name;
	public EventType m_type;
	public List<Modifier> m_dragonMods = new List<Modifier>();
	public List<Modifier> m_laterMods = new List<Modifier>();
	public List<Modifier> m_otherMods = new List<Modifier>();

	public DateTime m_teasingTimestamp = new DateTime();
	public DateTime m_startTimestamp = new DateTime();
	public DateTime m_endTimestamp = new DateTime();

	public bool m_refund;

	public TimeSpan timeToEnd {
		get { return m_endTimestamp - GameServerManager.GetEstimatedServerTime(); }
	}

	public TimeSpan timeToStart {
		get { return m_startTimestamp - GameServerManager.GetEstimatedServerTime(); }
	}

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
                if (m_typeDef == null)
                {
					Debug.LogError("Mission type '" + m_type + "' doesnt exist in the definitions");
                }
			}

			if ( _data.ContainsKey("params") ){
				JSONArray arr = _data["params"].AsArray;
				if ( arr != null ){
					for (int i = 0; i < arr.Count; i++) {
						m_params.Add( arr[i] );
					}
				}else{
					Debug.Log("No Params");
				}
			}
		}

		public virtual SimpleJSON.JSONClass ToJson ()
		{
			SimpleJSON.JSONClass ret = new SimpleJSON.JSONClass();
			ret.Add("tidDesc", m_desc );
			ret.Add("icon", m_icon );
			ret.Add("type", m_type );
			JSONArray arr =  new JSONArray();
			for (int i = 0; i < m_params.Count; i++) {
				arr.Add( m_params[i] );
			}
			ret.Add("params", arr);

			return ret;
		}
	}
	

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDLiveEventDefinition() {
		m_initialized = false;
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
		m_initialized = false;

		m_eventId = -1;
		m_name = "";
		m_type = EventType.NONE;
		m_dragonMods.Clear();
		m_laterMods.Clear();
		m_otherMods.Clear();

		m_teasingTimestamp = new DateTime(1970, 1, 1);
		m_startTimestamp = new DateTime(1970, 1, 1);
		m_endTimestamp = new DateTime(1970, 1, 1);
	}

	public virtual void ParseInfo( SimpleJSON.JSONNode _data )
	{
		Clean();

		m_initialized = true;

		// has this event bee cancelled?
		if(_data.ContainsKey("refund")) {
			m_refund = PersistenceUtils.SafeParse<bool>(_data["refund"]);
		}

		if ( _data.ContainsKey("code") )
		{
			m_eventId = PersistenceUtils.SafeParse<int>(_data["code"]);
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
				if ( modDef != null )
				{
					Modifier m = Modifier.CreateFromDefinition(modDef);
					if (m.isValid()) {
						if ( m.isLateModifier() ) {
							m_laterMods.Add(m);
						} else if (m is ModifierDragon) {
							m_dragonMods.Add(m);
						} else {
							m_otherMods.Add(m);
						}
					}
				}
			}
		}

        if (_data.ContainsKey("customMods")) {
            JSONArray _mods = _data["customMods"].AsArray;
            for (int i = 0; i < _mods.Count; ++i) {
                Modifier m = Modifier.CreateFromJson(_mods[i]);
				if (m != null && m.isValid()) {
					if ( m.isLateModifier() ) {
						m_laterMods.Add(m);
					}else if (m is ModifierDragon) {
						m_dragonMods.Add(m);
					} else {
						m_otherMods.Add(m);
					}
				}
            }
        }
        //

		// timestamps
		if ( _data.ContainsKey("teaserTimestamp") )
			m_teasingTimestamp = TimeUtils.TimestampToDate(PersistenceUtils.SafeParse<long>(_data["teaserTimestamp"]));

		if ( _data.ContainsKey("startTimestamp") )
			m_startTimestamp = TimeUtils.TimestampToDate(PersistenceUtils.SafeParse<long>(_data["startTimestamp"]));

		if ( _data.ContainsKey("endTimestamp") )
			m_endTimestamp = TimeUtils.TimestampToDate(PersistenceUtils.SafeParse<long>(_data["endTimestamp"]));
	}


	public virtual SimpleJSON.JSONClass ToJson ()
	{
		SimpleJSON.JSONClass ret = new SimpleJSON.JSONClass();

		ret.Add("code", PersistenceUtils.SafeToString(m_eventId));	
		ret.Add("name", m_name);

		// Type?    
		SimpleJSON.JSONArray mods = new JSONArray();
        SimpleJSON.JSONArray customMods = new JSONArray();

		toJsonMods( m_dragonMods, mods, customMods );
		toJsonMods( m_laterMods, mods, customMods );
		toJsonMods( m_otherMods, mods, customMods );
		/*
        for (int i = 0; i < m_dragonMods.Count; i++) {
			if(m_dragonMods[i] == null) continue;

            string sku = m_dragonMods[i].GetSku();

            if (sku.Equals(Modifier.SKU_CUSTOM)) {
                customMods.Add(m_dragonMods[i].ToJson());
            } else{
                mods.Add(sku);
            }
		}

		for (int i = 0; i < m_otherMods.Count; i++) {
			if(m_otherMods[i] == null) continue;

            string sku = m_otherMods[i].GetSku();

            if (sku.Equals(Modifier.SKU_CUSTOM)) {
                customMods.Add(m_otherMods[i].ToJson());
            } else {
                mods.Add(sku);
            }
        }
		*/
		ret.Add("mods", mods);
        ret.Add("customMods", customMods);

        // timestamps
        ret.Add("teaserTimestamp", PersistenceUtils.SafeToString(TimeUtils.DateToTimestamp( m_teasingTimestamp )));
		ret.Add("startTimestamp", PersistenceUtils.SafeToString(TimeUtils.DateToTimestamp( m_startTimestamp )));
		ret.Add("endTimestamp", PersistenceUtils.SafeToString(TimeUtils.DateToTimestamp( m_endTimestamp )));

		return ret;
	}

	private void toJsonMods( List<Modifier> toSaveMods, SimpleJSON.JSONArray mods, SimpleJSON.JSONArray customMods  )
	{
		for (int i = 0; i < toSaveMods.Count; i++) {
			if(toSaveMods[i] == null) continue;

            string sku = toSaveMods[i].GetSku();

            if (sku.Equals(Modifier.SKU_CUSTOM)) {
                customMods.Add(toSaveMods[i].ToJson());
            } else{
                mods.Add(sku);
            }
		}
	}

}