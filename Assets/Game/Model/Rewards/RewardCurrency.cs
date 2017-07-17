namespace Metagame {
	//------------------------------------------//
	public abstract class RewardCurrency : Reward {
		private long m_amount;

		public RewardCurrency(long _amount) {			
			m_value = _amount.ToString();
			m_amount = _amount;
		}

		public override void Collect() {
			UsersManager.currentUser.AddCurrency(m_currency, m_amount);
		}		
	}
	//------------------------------------------//

	public class RewardSoftCurrency : RewardCurrency {
		public const string Code = "sc";

		public RewardSoftCurrency(long _amount) : base(_amount) {			
			m_currency = UserProfile.Currency.SOFT;
		}
	}

	public class RewardHardCurrency : RewardCurrency {		
		public const string Code = "pc";

		public RewardHardCurrency(long _amount) : base(_amount) {			
			m_currency = UserProfile.Currency.HARD;
		}
	}

	public class RewardGoldenFragments : RewardCurrency {
		public const string Code = "gf";

		public RewardGoldenFragments(long _amount) : base(_amount) {			
			m_currency = UserProfile.Currency.GOLDEN_FRAGMENTS;
		}

		public override void Collect() {
			base.Collect();

			// we got enough golden fragments to create a golden egg!
			// new RewardGoldenEgg
			// push it to the profile
		}
	}
}
