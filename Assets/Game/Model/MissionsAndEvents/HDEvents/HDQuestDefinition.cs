// HDQuestDefinition.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 24/05/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using SimpleJSON;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class HDQuestDefinition : HDLiveEventDefinition {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	public class QuestGoal : GoalCommon
	{
		public string m_bonusDragon = "";
		public long m_amount = 0;

		public override void Clear ()
		{
			base.Clear ();
			m_bonusDragon = "";
			m_amount = 0;
		}

		public override void ParseGoal (JSONNode _data)
		{
			base.ParseGoal (_data);
			if ( _data.ContainsKey("area") ){
				m_bonusDragon = _data["area"];
			}

			if ( _data.ContainsKey("amount") ){
				m_amount = _data["amount"].AsLong;
			}
		}
	}

	public QuestGoal m_goal;

	public List<GlobalEvent.RewardSlot> m_rewards = new List<GlobalEvent.RewardSlot>();	// <- te remove from GlobalEvents
		
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDQuestDefinition() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDQuestDefinition() {

	}

	public override void ParseInfo( SimpleJSON.JSONNode _data )
	{
		base.ParseInfo(_data);

		if ( _data.ContainsKey("rewards") )
		{
			JSONArray arr = _data["rewards"].AsArray;
			for (int i = 0; i < arr.Count; i++) {
				m_rewards.Add( new GlobalEvent.RewardSlot( arr[i]) );
			}
		}
	}
}