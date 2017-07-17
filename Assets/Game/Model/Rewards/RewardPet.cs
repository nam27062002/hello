namespace Metagame {
	public class RewardPet : Reward {
		public const string Code = "pet";

		public RewardPet(string _sku) {			
			m_currency = UserProfile.Currency.NONE;
			m_value = _sku;
		}

		public override void Collect() {
			UsersManager.currentUser.petCollection.UnlockPet(m_value);
		}
	}
}
