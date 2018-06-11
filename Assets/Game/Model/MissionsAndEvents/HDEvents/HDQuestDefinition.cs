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
		// public string m_bonusDragon = "";
		public long m_amount = 0;

		public override void Clear ()
		{
			base.Clear ();
			// m_bonusDragon = "";
			m_amount = 0;
		}

		public override void ParseGoal (JSONNode _data)
		{
			base.ParseGoal (_data);
			//if ( _data.ContainsKey("bonusDragon") ){
			//	m_bonusDragon = _data["bonusDragon"];
			// }

			if ( _data.ContainsKey("amount") ){
				m_amount = _data["amount"].AsLong;
			}
		}
	}

	public class QuestReward : HDLiveEventReward {
		public float targetAmount = 0f;
	}

	public QuestGoal m_goal = new QuestGoal();

	public List<QuestReward> m_rewards = new List<QuestReward>();
		
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

		if ( _data.ContainsKey("goal") ){
			m_goal.ParseGoal( _data["goal"] );
		}

		if ( _data.ContainsKey("rewards") )
		{
			JSONArray arr = _data["rewards"].AsArray;
			for (int i = 0; i < arr.Count; i++) {
				QuestReward reward = new QuestReward();
				reward.ParseJson(arr[i], m_name);

				if(m_goal != null) {
					reward.targetAmount = reward.targetPercentage * m_goal.m_amount;
				}

				m_rewards.Add( reward );
			}
		}
	}
}