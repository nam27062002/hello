// RewardMultiEgg.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/06/2018.
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
	/// Special reward to give multiple eggs of the same type.
	/// </summary>
	public class RewardMultiEgg : RewardMulti {
		//------------------------------------------------------------------------//
		// CONSTANTS															  //
		//------------------------------------------------------------------------//
		public const string TYPE_CODE = "multi_egg";

		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//

		//------------------------------------------------------------------------//
		// GENERIC METHODS														  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Parametrized constructor.
		/// </summary>
		public RewardMultiEgg(long _amount, string _sku, string _source, HDTrackingManager.EEconomyGroup _economyGroup = HDTrackingManager.EEconomyGroup.UNKNOWN) {
			// Internal initializer
            base.Init(TYPE_CODE, _amount);

			// Common stuff
			m_source = _source;
			m_sku = _sku;
			m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, _sku);

			// Create individual reward for each given data
			for(int i = 0; i < _amount; ++i) {
				RewardEgg r = Reward.CreateTypeEgg(_sku, _source);
				m_rewards.Add(r);
			}
		}

		/// <summary>
		/// Destructor
		/// </summary>
		~RewardMultiEgg() {

		}

		//------------------------------------------------------------------------//
		// OTHER METHODS														  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Implementation of the abstract Collect() method.
		/// </summary>
		override protected void DoCollect() {
			// Push all rewards (eggs) to the rewards stack
			for(int i = 0; i < m_rewards.Count; ++i) {
				UsersManager.currentUser.PushReward(m_rewards[i]);
			}
		}

        public override void LoadCustomJsonData(SimpleJSON.JSONNode _data)
        {
            // We don't need to parse the rewards contained in this item because individual eggs have already been created in the constructor
        }

        //------------------------------------------------------------------------//
        // CALLBACKS															  //
        //------------------------------------------------------------------------//
    }
}