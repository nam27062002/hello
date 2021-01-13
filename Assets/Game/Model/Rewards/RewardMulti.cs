// RewardMulti.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace Metagame {
	/// <summary>
	/// A reward that actually contains multiple rewards.
	/// </summary>
	public class RewardMulti : Reward {
		//------------------------------------------------------------------------//
		// CONSTANTS															  //
		//------------------------------------------------------------------------//
		public const string TYPE_CODE = "multi";

		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//
		protected List<Reward> m_rewards = new List<Reward>();
		public List<Reward> rewards {
			get { return m_rewards; }
		}
		
		//------------------------------------------------------------------------//
		// GENERIC METHODS														  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Default constructor.
		/// </summary>
		protected RewardMulti() {

		}

		/// <summary>
		/// Parametrized constructor.
		/// </summary>
		public RewardMulti(List<Reward.Data> _datas, string _source, HDTrackingManager.EEconomyGroup _economyGroup = HDTrackingManager.EEconomyGroup.UNKNOWN) {
			// Internal initializer
            base.Init(TYPE_CODE, _datas.Count);

			// Common stuff
			m_source = _source;

			// Create individual reward for each given data
			for(int i = 0; i < _datas.Count; ++i) {
				Reward r = Reward.CreateFromData(_datas[i], _economyGroup, _source);
				m_rewards.Add(r);
			}
		}

		/// <summary>
		/// Destructor
		/// </summary>
		~RewardMulti() {

		}

		//------------------------------------------------------------------------//
		// OTHER METHODS														  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Implementation of the abstract Collect() method.
		/// </summary>
		override protected void DoCollect() {
			// Collect all rewards internally
			for(int i = 0; i< m_rewards.Count; ++i) {
				m_rewards[i].Collect();
			}
		}

		/// <summary>
		/// Create and return a persistence save data json initialized with this reward's data.
		/// </summary>
		/// <returns>A new data json to be stored to persistence.</returns>
		override public SimpleJSON.JSONNode ToJson() {
			// Basic data
			SimpleJSON.JSONNode data = base.ToJson();

			// Add array with child rewards
			SimpleJSON.JSONArray rewardsData = new SimpleJSON.JSONArray();
			for(int i = 0; i < m_rewards.Count; ++i) {
				rewardsData.Add(m_rewards[i].ToJson());
			}
			data.Add("rewards", rewardsData);

			// Done!
			return data;
		}

		/// <summary>
		/// For those types requiring it, parse extra data from a json node.
		/// </summary>
		/// <param name="_data">Json to be parsed.</param>
		override public void LoadCustomJsonData(SimpleJSON.JSONNode _data) {
			// Parse child rewards array and add create child rewards
			if(_data.ContainsKey("rewards")) {
				SimpleJSON.JSONArray rewardsData = _data["rewards"].AsArray;
				for(int i = 0; i < rewardsData.Count; ++i) {
					Reward r = Reward.CreateFromJson(rewardsData[i]);
					m_rewards.Add(r);
				}
			}
		}

		/// <summary>
		/// Obtain the generic TID to describe this reward type.
		/// </summary>
		/// <returns>TID describing this reward type.</returns>
		/// <param name="_plural">Singular or plural TID?</param>
		public override string GetTID(bool _plural) {
			// Shouldn't be used with Multi rewards
			return "TID_GEN_REWARD";
		}

		//------------------------------------------------------------------------//
		// CALLBACKS															  //
		//------------------------------------------------------------------------//
	}
}