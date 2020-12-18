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
public class HDLiveQuestDefinition : HDLiveEventDefinition {
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
				m_amount = PersistenceUtils.SafeParse<long>(_data["amount"]);
			}
		}

		public override JSONClass ToJson() {
			JSONClass data = base.ToJson();
			data.Add("amount", PersistenceUtils.SafeToString(m_amount));
			return data;
		}
	}

	public QuestGoal m_goal = new QuestGoal();

	public List<HDLiveData.Reward> m_rewards = new List<HDLiveData.Reward>();

	// Duration of the quest in mins (for solo quests)
	public long duration;
		
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDLiveQuestDefinition() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDLiveQuestDefinition() {

	}

	public override void Clean()
    {
		base.Clean();

		m_goal = new QuestGoal();

	    m_rewards = new List<HDLiveData.Reward>();
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
		JSONArray rewardsData = new JSONArray();
		for(int i = 0; i < m_rewards.Count; ++i)
		{
			rewardsData.Add(m_rewards[i].SaveData());
		}
		data.Add("rewards", rewardsData);

		return data;
	}

	/// <summary>
	/// Initialize this quest definition from the data in the content
	/// We use this method to load a quest from the soloQuestDefinitions
	/// </summary>
	/// <param name="_def">The soloQuest definition node</param>
	public void InitFromDefinition(DefinitionNode _def)
	{
		string sku = _def.GetAsString("sku");
		
		// Initialize Goal
		m_goal = new QuestGoal();
		m_goal.m_amount = _def.GetAsLong("amount");
		m_goal.m_desc = _def.GetAsString("description");
		m_goal.m_icon = _def.GetAsString("icon");
		m_goal.m_type = _def.GetAsString("type");
		m_goal.m_typeDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_TYPES, m_goal.m_type);

		// Initialize rewards
		List<DefinitionNode> rewardsDef = DefinitionsManager.SharedInstance.
			GetDefinitionsByVariable(DefinitionsCategory.SOLO_QUESTS_REWARDS, "questSku", sku);

		foreach (DefinitionNode rewardDef in rewardsDef)
		{
			HDLiveData.Reward reward = new HDLiveData.Reward();
			reward.target = rewardDef.GetAsLong("target");

			Metagame.Reward.Data data = new Metagame.Reward.Data();
			data.amount = rewardDef.GetAsInt("amount");
			data.sku = rewardDef.GetAsString("rewardSku");
			data.typeCode = rewardDef.GetAsString("type");
			reward.reward = Metagame.Reward.CreateFromData(data, HDTrackingManager.EEconomyGroup.SOLO_QUEST, "");

			if (reward.reward == null)
			{
				Debug.LogError("The reward defined for the Solo Quest is not valid");
				continue;
			}
			
			m_rewards.Add(reward);
		}

		// The UI only allows 4 rewards
		if (m_rewards.Count != 4)
		{
			Debug.LogError("The expected amount of rewards for a Solo Quest is 4. Please fix the content.");
		}
		
		// Duration of the quest
		duration = _def.GetAsLong("durationMinutes");
	}
}