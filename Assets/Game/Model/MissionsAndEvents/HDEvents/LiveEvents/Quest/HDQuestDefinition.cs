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

		public override JSONClass ToJson() {
			JSONClass data = base.ToJson();
			data.Add("amount", m_amount);
			return data;
		}
	}

	public QuestGoal m_goal = new QuestGoal();

	public List<HDLiveData.Reward> m_rewards = new List<HDLiveData.Reward>();
		
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
                HDLiveData.Reward reward = new HDLiveData.Reward();
				long defaultValue = (long)(((float)(i + 1) / (float)arr.Count) * m_goal.m_amount);  // [AOC] Just in case data is corrupt, initialize with nice default values (i.e. 25%, 50%, 75%, 100% for 4 rewards)
				reward.target = defaultValue;
				reward.LoadData(arr[i], HDTrackingManager.EEconomyGroup.REWARD_LIVE_EVENT, m_name);
				if(reward.target <= 0) reward.target = defaultValue;

				m_rewards.Add( reward );
			}
		}
	}

	public override JSONClass ToJson() {
		JSONClass data = base.ToJson();

		// Add goal?
		data.Add("goal", m_goal.ToJson());

		// Add rewards
		// [AOC] TODO!! Restoring caching rewards cause a null pointer exception. Investigate why, don't cache them meanwhile
		/*JSONArray rewardsData = new JSONArray();
		for(int i = 0; i < m_rewards.Count; ++i) {
			rewardsData.Add(m_rewards[i].ToJson());
		}
		data.Add("rewards", rewardsData);*/

		return data;
	}
}