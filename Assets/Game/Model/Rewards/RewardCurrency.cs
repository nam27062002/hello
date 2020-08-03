// RewardCurrency.cs
// Hungry Dragon
// 
// Created by Marc Saña Forrellach on 18/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace Metagame {
	/// <summary>
	/// Base abstract class for all currency rewards.
	/// </summary>
	public abstract class RewardCurrency : Reward {
		public HDTrackingManager.EEconomyGroup EconomyGroup { get; set; }

		public RewardCurrency(string _source) {			
			m_source = _source;
		}

		protected void Init(string _type, long _amount, Rarity _rarity, HDTrackingManager.EEconomyGroup _economyGroup) {
			base.Init(_type, _amount);			
			m_rarity = _rarity;
            EconomyGroup = _economyGroup;
		}

		override protected void DoCollect() {
			UsersManager.currentUser.EarnCurrency(m_currency, (ulong)amount, false, EconomyGroup);
		}

		override public SimpleJSON.JSONNode ToJson() {
			// Basic data
			SimpleJSON.JSONNode data = base.ToJson();

			// Custom data
			data.Add("economyGroup", HDTrackingManager.EconomyGroupToString(this.EconomyGroup));

			return data;
		}
	}
	
	/// <summary>
	/// Soft currency reward.
	/// </summary>
	public class RewardSoftCurrency : RewardCurrency {
		public const string TYPE_CODE = "sc";

		public RewardSoftCurrency(long _amount, Rarity _rarity, HDTrackingManager.EEconomyGroup _economyGroup, string _source) : base(_source) {
			base.Init(TYPE_CODE, _amount, _rarity, _economyGroup);
			m_currency = UserProfile.Currency.SOFT;
		}

		public override string GetTID(bool _plural) {
			string tid = "TID_SC_NAME";
			if(_plural) {
				tid += "_PLURAL";
			}
			return tid;
		}

		/// <summary>
		/// Scale the given SC amount using the biggest owned dragon of the current user as a reference.
		/// </summary>
		/// <returns>The scaled SC amount.</returns>
		/// <param name="_amount">Base SC amount to be scaled.</param>
		public static long ScaleByMaxDragonOwned(long _amount) {
			DefinitionNode rewardScaleFactorDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.MISSION_MODIFIERS, "dragonSku", DragonManager.biggestOwnedDragon.def.sku);
			if(rewardScaleFactorDef != null) {
				return Mathf.RoundToInt(((float)_amount) * rewardScaleFactorDef.GetAsFloat("missionSCRewardMultiplier"));
			}
			return _amount;
		}
	}

	/// <summary>
	/// Hard currency reward.
	/// </summary>
	public class RewardHardCurrency : RewardCurrency {		
		public const string TYPE_CODE = "pc";

		public RewardHardCurrency(long _amount, Rarity _rarity, HDTrackingManager.EEconomyGroup _economyGroup, string _source) : base(_source) {
			base.Init(TYPE_CODE, _amount, _rarity, _economyGroup);
			m_currency = UserProfile.Currency.HARD;
		}

		public override string GetTID(bool _plural) {
			string tid = "TID_PC_NAME";
			if(_plural) {
				tid += "_PLURAL";
			}
			return tid;
		}
	}

	/// <summary>
	/// Golden fragments reward.
	/// </summary>
	public class RewardGoldenFragments : RewardCurrency {
		public const string TYPE_CODE = "gf";

		public RewardGoldenFragments(long _amount, Rarity _rarity, HDTrackingManager.EEconomyGroup _economyGroup, string _source) : base(_source) {
            //[AOC] Mini-hack: if reward is gold fragments, tweak its rarity so displayed reward looks cooler
            if (_amount >= 5)       _rarity = Metagame.Reward.Rarity.EPIC;
            else if (_amount >= 3)  _rarity = Metagame.Reward.Rarity.RARE;
            else                    _rarity = Metagame.Reward.Rarity.COMMON;

            base.Init(TYPE_CODE, _amount, _rarity, _economyGroup);
			m_currency = UserProfile.Currency.GOLDEN_FRAGMENTS;
		}

		public override string GetTID(bool _plural) {
			return "TID_GOLDEN_FRAGMENTS_NAME";	// No singular version :/
		}
	}
}
