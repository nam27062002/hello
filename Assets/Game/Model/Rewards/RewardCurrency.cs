﻿// RewardCurrency.cs
// Hungry Dragon
// 
// Created by Marc Saña Forrellach on 18/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace Metagame {
	/// <summary>
	/// Base abstract class for all currency rewards.
	/// </summary>
	public abstract class RewardCurrency : Reward {
        private HDTrackingManager.EEconomyGroup EconomyGroup { get; set; }

		public RewardCurrency() {			
			
		}

		protected void Init(string _type, long _amount, Rarity _rarity, HDTrackingManager.EEconomyGroup _economyGroup) {
			base.Init(_type);
			m_amount = _amount;
			m_rarity = _rarity;
            EconomyGroup = _economyGroup;
		}

		override protected void DoCollect() {
			UsersManager.currentUser.EarnCurrency(m_currency, (ulong)m_amount, false, EconomyGroup);
		}		
	}
	
	/// <summary>
	/// Soft currency reward.
	/// </summary>
	public class RewardSoftCurrency : RewardCurrency {
		public const string TYPE_CODE = "sc";

		public RewardSoftCurrency(long _amount, Rarity _rarity, HDTrackingManager.EEconomyGroup _economyGroup) : base() {
			base.Init(TYPE_CODE, _amount, _rarity, _economyGroup);
			m_currency = UserProfile.Currency.SOFT;
		}
	}

	/// <summary>
	/// Hard currency reward.
	/// </summary>
	public class RewardHardCurrency : RewardCurrency {		
		public const string TYPE_CODE = "pc";

		public RewardHardCurrency(long _amount, Rarity _rarity, HDTrackingManager.EEconomyGroup _economyGroup) : base() {
			base.Init(TYPE_CODE, _amount, _rarity, _economyGroup);
			m_currency = UserProfile.Currency.HARD;
		}
	}

	/// <summary>
	/// Golden fragments reward.
	/// </summary>
	public class RewardGoldenFragments : RewardCurrency {
		public const string TYPE_CODE = "gf";

		public RewardGoldenFragments(long _amount, Rarity _rarity, HDTrackingManager.EEconomyGroup _economyGroup) : base() {
			base.Init(TYPE_CODE, _amount, _rarity, _economyGroup);
			m_currency = UserProfile.Currency.GOLDEN_FRAGMENTS;
		}

		override protected void DoCollect() {
			base.DoCollect();

			if (EggManager.goldenEggCompleted) {
				Reward reward = Reward.CreateTypeEgg(Egg.SKU_GOLDEN_EGG);
				UsersManager.currentUser.rewardStack.Push(reward);
			}
		}
	}
}
