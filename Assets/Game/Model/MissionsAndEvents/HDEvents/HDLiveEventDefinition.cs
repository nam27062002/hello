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
	private bool m_initialized = false;
	public bool initialized {
		get { return m_initialized; }
	}

	public int m_eventId;
	public string m_name;
	public EventType m_type;
	public List<Modifier> m_dragonMods = new List<Modifier>();
	public List<Modifier> m_otherMods = new List<Modifier>();

	public DateTime m_teasingTimestamp = new DateTime();
	public DateTime m_startTimestamp = new DateTime();
	public DateTime m_endTimestamp = new DateTime();

	public bool m_refund;

	public TimeSpan timeToEnd {
		get { return m_endTimestamp - GameServerManager.SharedInstance.GetEstimatedServerTime(); }
	}

	public TimeSpan timeToStart {
		get { return m_startTimestamp - GameServerManager.SharedInstance.GetEstimatedServerTime(); }
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
	// Build?
	// Rewards?

	[Serializable]
	public class HDLiveEventReward {		
		//------------------------------------------------------------------------//
		// MEMBERS																  //
		//------------------------------------------------------------------------//
		public Metagame.Reward reward = null;
		public long target = 0;

		//------------------------------------------------------------------------//
		// METHODS																  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Default constructor.
		/// </summary>
		public HDLiveEventReward() {

		}

		/// <summary>
		/// Constructor from json data.
		/// </summary>
		/// <param name="_data">Data to be parsed.</param>
		public virtual void ParseJson(SimpleJSON.JSONNode _data, string _source) {
			// Reward data
			reward = Metagame.Reward.CreateFromJson(_data, HDTrackingManager.EEconomyGroup.REWARD_LIVE_EVENT, _source);

			// [AOC] Going to hell!
			// 		 Mini-hack: if reward is gold fragments, tweak its rarity so displayed reward looks cooler
			if(reward.type == Metagame.RewardGoldenFragments.TYPE_CODE) {
				if(reward.amount >= 5) {
					reward.rarity = Metagame.Reward.Rarity.EPIC;
				} else if(reward.amount >= 3) {
                    reward.rarity = Metagame.Reward.Rarity.RARE;
				} else {
					reward.rarity = Metagame.Reward.Rarity.COMMON;
				}
			}

			// Init target amount
			target = _data["target"].AsLong;
		}

		/// <summary>
		/// Serialize into json.
		/// </summary>
		/// <returns>The json.</returns>
		public virtual JSONClass ToJson() {
			JSONClass data = reward.ToJson() as JSONClass;
			data.Add("target", target);
			return data;
		}
	};

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
		m_refund = _data["refund"].AsBool;

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
				if ( modDef != null )
				{
					Modifier m = Modifier.CreateFromDefinition(modDef);
					if (m is ModifierDragon) {
						m_dragonMods.Add(m);
					} else {
						m_otherMods.Add(m);
					}
				}
			}
		}

		// timestamps
		if ( _data.ContainsKey("teaserTimestamp") )
			m_teasingTimestamp = TimeUtils.TimestampToDate(_data["teaserTimestamp"].AsLong);

		if ( _data.ContainsKey("startTimestamp") )
			m_startTimestamp = TimeUtils.TimestampToDate(_data["startTimestamp"].AsLong);

		if ( _data.ContainsKey("endTimestamp") )
			m_endTimestamp = TimeUtils.TimestampToDate(_data["endTimestamp"].AsLong);
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

		// timestamps
		ret.Add("teaserTimestamp", TimeUtils.DateToTimestamp( m_teasingTimestamp ));
		ret.Add("startTimestamp", TimeUtils.DateToTimestamp( m_startTimestamp ));
		ret.Add("endTimestamp", TimeUtils.DateToTimestamp( m_endTimestamp ));

		return ret;
	}

}