namespace Metagame {
	public class RewardEgg : Reward {
		public const string Code = "egg";

		public RewardEgg(string _sku) {			
			m_currency = UserProfile.Currency.NONE;
			m_value = _sku;
		}

		public override void Collect() {
			//(Egg.CreateFromSku(m_value)).Collect();
			EggManager.AddEggToInventory(Egg.CreateFromSku(m_value));
		}
	}
}
