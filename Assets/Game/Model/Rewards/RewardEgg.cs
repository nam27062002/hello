using UnityEngine;
using System.Collections.Generic;

namespace Metagame {
	public class RewardEgg : Reward {
		public const string Code = "egg";

		private Egg m_egg;
		public Egg egg { 
			get { 
				if (m_egg == null) {
					m_egg = Egg.CreateFromSku(m_value);
					m_egg.SetReward(this);
				}
				return m_egg; 
			} 
			set {
				m_egg = value;
				m_egg.SetReward(this);
			}
		}

		private Reward m_reward;
		public string eggRewardSku { 
			get { 
				if (m_reward != null) 
					return m_reward.value;
				return "";
			}
		}

		public RewardEgg(string _sku) {
			Build(_sku, "");
		}

		public RewardEgg(string _sku, string _rewardSku) {
			Build(_sku, _rewardSku);
		}

		public override void Collect() {
			if (m_reward != null) {
				UsersManager.currentUser.rewardStack.Push(m_reward);
			}
		}

		private void Build(string _sku, string _rewardSku) {
			m_currency = UserProfile.Currency.NONE;
			m_value = _sku;

			BuildReward(_rewardSku);

			if (m_reward != null) {
				m_rarity = m_reward.rarity;
			} else {
				m_rarity = Rarity.COMMON;
			}
		}

		private void BuildReward(string _rewardSku) {
			DefinitionNode rewardDef = null;
			if (!string.IsNullOrEmpty(_rewardSku)) {
				rewardDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGG_REWARDS, _rewardSku);
			} else if (m_value.Equals(Egg.SKU_GOLDEN_EGG)) {
				rewardDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGG_REWARDS, "pet_special");
			} else {
				rewardDef = EggManager.GenerateReward();
			}
		
			if (rewardDef != null) {
				string type = rewardDef.GetAsString("type");
				string raritySku = rewardDef.GetAsString("rarity");
				Rarity rarity = Reward.SkuToRarity(raritySku);

				// right now, eggs only reward pets
				if (type.Equals("pet")) {
					List<DefinitionNode> petDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.PETS, "rarity", raritySku);
					DefinitionNode petDef = null;

					if (CPGachaTest.rewardChanceMode == CPGachaTest.RewardChanceMode.FORCED_PET_SKU) {
						petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, CPGachaTest.forcedPetSku);
					} else if (rarity == Rarity.SPECIAL) {
						if (!EggManager.allGoldenEggsCollected) {
							// Still add a safeguard just in case
							int maxTries = 100;
							int tryCount = 0;
							do { 
								petDef = petDefs.GetRandomValue();
								tryCount++;
							} while (UsersManager.currentUser.petCollection.IsPetUnlocked(petDef.sku) && tryCount < maxTries);
						}
					} else {
						if (!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.EGG_REWARD)) {
							// If tutorial is not completed, choose from a limited pool
							do {
								petDef = petDefs.GetRandomValue();
							} while (!petDef.GetAsBool("startingPool"));
						} else {
							// Default behaviour
							petDef = petDefs.GetRandomValue();
						}
					}

					if (petDef != null) {
						m_reward = CreateTypePet(petDef);
					}
				}
			}
		}
	}
}
