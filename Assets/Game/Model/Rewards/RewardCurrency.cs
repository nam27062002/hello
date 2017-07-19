namespace Metagame {
	//------------------------------------------//
	public abstract class RewardCurrency : Reward {
		private long m_amount;

		public RewardCurrency(long _amount, Rarity _rarity) {			
			m_value = _amount.ToString();
			m_amount = _amount;
			m_rarity = _rarity;
		}

		public override void Collect() {
			UsersManager.currentUser.AddCurrency(m_currency, m_amount);
		}		
	}
	//------------------------------------------//

	public class RewardSoftCurrency : RewardCurrency {
		public const string Code = "sc";

		public RewardSoftCurrency(long _amount, Rarity _rarity) : base(_amount, _rarity) {
			m_currency = UserProfile.Currency.SOFT;
		}
	}

	public class RewardHardCurrency : RewardCurrency {		
		public const string Code = "pc";

		public RewardHardCurrency(long _amount, Rarity _rarity) : base(_amount, _rarity) {
			m_currency = UserProfile.Currency.HARD;
		}
	}

	public class RewardGoldenFragments : RewardCurrency {
		public const string Code = "gf";

		public RewardGoldenFragments(long _amount, Rarity _rarity) : base(_amount, _rarity) {
			m_currency = UserProfile.Currency.GOLDEN_FRAGMENTS;
		}

		public override void Collect() {
			base.Collect();

			if (EggManager.goldenEggCompleted) {
				Reward reward = Reward.CreateTypeEgg(Egg.SKU_GOLDEN_EGG);
				UsersManager.currentUser.rewardStack.Push(reward);
			}
		}
	}
}
