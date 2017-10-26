﻿// RewardPet.cs
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
			if(duplicated) {
				string petRewardSku = "pet_" + _def.GetAsString("rarity");
				DefinitionNode petRewardDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGG_REWARDS, petRewardSku);

				// Have all golden eggs been collected?
				if (EggManager.allGoldenEggsCollected) {
					// Yes! Give coins rather than golden egg fragments (based on rarity)
					m_replacement = Metagame.Reward.CreateTypeSoftCurrency(petRewardDef.GetAsLong("duplicateCoinsGiven"), HDTrackingManager.EEconomyGroup.PET_DUPLICATED, m_source);
				} else {
					// No! Give golden egg fragments based on rarity
					m_replacement = Metagame.Reward.CreateTypeGoldenFragments(petRewardDef.GetAsInt("duplicateFragmentsGiven"), rarity, HDTrackingManager.EEconomyGroup.PET_DUPLICATED, m_source);
				}
			} 
		}

		/// <summary>
		/// Implementation of the abstract Collect() method.
		/// </summary>
		override protected void DoCollect() {
			UsersManager.currentUser.petCollection.UnlockPet(m_sku);

			HDTrackingManager.Instance.Notify_Pet(m_sku, m_source);
		}
	}
}
