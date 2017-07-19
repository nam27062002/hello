using UnityEngine;

namespace Metagame {
	public class RewardPet : Reward {
		public const string Code = "pet";

		private Reward m_replacement;

		public RewardPet(string _sku) {
			DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, _sku);
			InitFrom(def);
		}

		public RewardPet(DefinitionNode _def) {
			InitFrom(_def);
		}

		private void InitFrom(DefinitionNode _def) {
			m_currency = UserProfile.Currency.NONE;
			m_value = _def.sku;
			m_rarity = Reward.SkuToRarity(_def.GetAsString("rarity"));

			bool duplicated = false;
			// If the pet is already owned, give special egg part or coins instead
			// Cheat support
			switch (CPGachaTest.duplicateMode) {
				case CPGachaTest.DuplicateMode.DEFAULT: duplicated = UsersManager.currentUser.petCollection.IsPetUnlocked(_def.sku); break;
				case CPGachaTest.DuplicateMode.ALWAYS: 	duplicated = true; 	break;
				case CPGachaTest.DuplicateMode.NEVER: 	duplicated = false; break;
				case CPGachaTest.DuplicateMode.RANDOM: 	duplicated = Random.value > 0.5f; break;
			}

			// If duplicated, give alternative rewards
			if (duplicated) {
				string petRewardSku = "pet_" + _def.GetAsString("rarity");
				DefinitionNode petRewardDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGG_REWARDS, petRewardSku);

				// Have all golden eggs been collected?
				if (EggManager.allGoldenEggsCollected) {
					// Yes! Give coins rather than golden egg fragments (based on rarity)
					m_replacement = CreateTypeSoftCurrency(petRewardDef.GetAsLong("duplicateCoinsGiven"));
				} else {
					// No! Give golden egg fragments based on rarity
					m_replacement = CreateTypeGoldenFragments(petRewardDef.GetAsInt("duplicateFragmentsGiven"), rarity);
				}
			} 
		}

		public bool WillBeReplaced() 						{ return m_replacement != null; }
		public UserProfile.Currency ReplacementCurrency() 	{ return m_replacement.currency; }
		public string ReplacementValue() 				  	{ return m_replacement.value; }

		public override void Collect() {
			if (m_replacement != null) {
				m_replacement.Collect();
			} else {
				UsersManager.currentUser.petCollection.UnlockPet(m_value);
			}
		}
	}
}
