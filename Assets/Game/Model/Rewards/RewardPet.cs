// RewardPet.cs
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
	/// Pet reward.
	/// </summary>
	public class RewardPet : Reward {
		//------------------------------------------------------------------------//
		// CONSTANTS															  //
		//------------------------------------------------------------------------//
		public const string TYPE_CODE = "pet";

		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//

		//------------------------------------------------------------------------//
		// METHODS																  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Constructor from pet sku.
		/// </summary>
		/// <param name="_sku">Pet sku.</param>
		public RewardPet(string _sku, string _source) {
			m_source = _source;
			DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, _sku);
			InitFrom(def);
		}

		/// <summary>
		/// Constructor from pet definition.
		/// </summary>
		/// <param name="_def">Pet definition.</param>
		public RewardPet(DefinitionNode _def, string _source) {
			m_source = _source;
			InitFrom(_def);
		}

		/// <summary>
		/// Internal initializer from pet definition.
		/// </summary>
		/// <param name="_def">Pet definition.</param>
		private void InitFrom(DefinitionNode _def) {
			base.Init(TYPE_CODE);

			m_sku = _def.sku;
			m_def = _def;

			m_rarity = Reward.SkuToRarity(_def.GetAsString("rarity"));

			CheckReplacement();
		}

		/// <summary>
		/// Checks whether this reward needs to be replaced and creates a replacement
		/// reward if needed.
		/// </summary>
		override public void CheckReplacement() {
			// If the pet is already owned, give special egg part or coins instead
			bool duplicated = false;

			// Cheat support
			switch (CPGachaTest.duplicateMode) {
				case CPGachaTest.DuplicateMode.DEFAULT: duplicated = UsersManager.currentUser.petCollection.IsPetUnlocked(m_sku); break;
				case CPGachaTest.DuplicateMode.ALWAYS: 	duplicated = true; 	break;
				case CPGachaTest.DuplicateMode.NEVER: 	duplicated = false; break;
				case CPGachaTest.DuplicateMode.RANDOM: 	duplicated = Random.value > 0.5f; break;
			}

			// If duplicated, give alternative rewards
			if(duplicated) {
				// Replacement reward depends on pet rarity
				string raritySku = m_def.GetAsString("rarity");
				string petRewardSku = "pet_" + raritySku;
				DefinitionNode petRewardDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGG_REWARDS, petRewardSku);

				// Which currency?
				// Try with SC
				long targetAmount = petRewardDef.GetAsLong("duplicateCoinsGiven", 0);
				UserProfile.Currency targetCurrency = UserProfile.Currency.SOFT;
				if(targetAmount <= 0) {
					// Try with HC
					targetAmount = petRewardDef.GetAsLong("duplicateGemsGiven", 0);
					targetCurrency = UserProfile.Currency.HARD;

					// Throw error if neither SC nor HC were defined
					Debug.Assert(amount > 0, "No replacement reward defined for " + petRewardSku);
				}

				// Create reward
				m_replacement = Metagame.Reward.CreateTypeCurrency(targetAmount, targetCurrency, SkuToRarity(raritySku), HDTrackingManager.EEconomyGroup.PET_DUPLICATED, m_source);
			} 
		}

		/// <summary>
		/// Implementation of the abstract Collect() method.
		/// </summary>
		override protected void DoCollect() {
			UsersManager.currentUser.petCollection.UnlockPet(m_sku);

			HDTrackingManager.Instance.Notify_Pet(m_sku, m_source);
		}

		/// <summary>
		/// Return a visual representation of the reward.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Metagame.RewardEgg"/>.</returns>
		override public string ToString() {
			if(def == null) {
				return "NULL";
			} else {
				return def.sku + (this.WillBeReplaced() ? " (d)" : "");
			}
		}

		/// <summary>
		/// Obtain the generic TID to describe this reward type.
		/// </summary>
		/// <returns>TID describing this reward type.</returns>
		/// <param name="_plural">Singular or plural TID?</param>
		public override string GetTID(bool _plural) {
			// Use definition to find a better tid
			string tid = "TID_PET";
			if(m_def != null) {
				tid = m_def.GetAsString("tidName");
			}

			// Add plural suffix if needed
			if(_plural) {
				tid += "_PLURAL";
			}
			return tid;
		}
	}
}
