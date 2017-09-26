// RewardEgg.cs
// Hungry Dragon
// 
// Created by Marc Saña Forrellach on 18/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace Metagame {
	/// <summary>
	/// Egg as reward.
	/// </summary>
	public class RewardEgg : Reward {
		//------------------------------------------------------------------------//
		// CONSTANTS															  //
		//------------------------------------------------------------------------//
		public const string TYPE_CODE = "egg";

		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//
		// Egg linked to this reward
		private Egg m_egg;
		public Egg egg { 
			get { 
				if (m_egg == null) {
					m_egg = Egg.CreateFromSku(m_sku);
					m_egg.SetReward(this);
				}
				return m_egg; 
			} 
			set {
				m_egg = value;
				m_egg.SetReward(this);
			}
		}

		// The reward given by the egg
		private Reward m_reward = null;
		public Reward reward { 
			get { return m_reward; }
		}

		//------------------------------------------------------------------------//
		// METHODS																  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Constructor from egg sku.
		/// </summary>
		/// <param name="_sku">Egg sku.</param>
		public RewardEgg(string _sku) {
			Build(_sku);	// This will generate a random reward following the gacha rules
		}

		/// <summary>
		/// Internal builder with egg sku and reward sku.
		/// </summary>
		/// <param name="_sku">Egg sku.</param>
		private void Build(string _sku) {
			Debug.Log("<color=purple>Building egg with sku " + _sku + "</color>");

			// Internal initializer
			base.Init(TYPE_CODE);

			// Override some fields
			m_sku = _sku;
			m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, _sku);

			BuildReward();

			if (m_reward != null) {
				m_rarity = m_reward.rarity;
			} else {
				m_rarity = Rarity.COMMON;
			}
		}

		/// <summary>
		/// Initializes the reward given by this egg.
		/// </summary>
		/// <param name="_rewardTypeSku">Reward sku (from EGG_REWARDS definitions category). Leave empty to generate a random reward following the gacha rules.</param>
		private void BuildReward() {
			Debug.Log("<color=purple>Building egg reward!</color>");

			// Get the reward definition
			DefinitionNode rewardTypeDef = null;
			if(m_sku.Equals(Egg.SKU_GOLDEN_EGG)) {
				rewardTypeDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGG_REWARDS, "pet_special");
			} else {
				rewardTypeDef = EggManager.GenerateReward();
			}

			// Nothing else to do if def is null
			if(rewardTypeDef == null) {
				Debug.Log("<color=red>COULDN'T DO IT!</color>");
				return;
			}

			// Initialize the reward data based on type
			string rewardType = rewardTypeDef.GetAsString("type");
			switch(rewardType) {
				case RewardPet.TYPE_CODE: {
					// Find out reward rarity
					string raritySku = rewardTypeDef.Get("rarity");
					m_rarity = SkuToRarity(raritySku);

					// Select a pet to be rewarded!
					List<DefinitionNode> petDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.PETS, "rarity", raritySku);
					DefinitionNode petDef = null;

					// Remove all hidden pets
					for( int i = petDefs.Count - 1; i >= 0; --i )
					{
						if ( petDefs[i].GetAsBool("hidden") )
						{
							petDefs.RemoveAt(i);
						}
					}

					// a) Forcing a specific sku from cheats?
					if(CPGachaTest.rewardChanceMode == CPGachaTest.RewardChanceMode.FORCED_PET_SKU) {
						// Get that specific pet!
						petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, CPGachaTest.forcedPetSku);
					}

					// b) Special case if golden egg
					else if (rarity == Rarity.SPECIAL) {
						// We should never be opening a special egg when all of them are already collected, but check just in case!
						if(!EggManager.allGoldenEggsCollected) {
							// Get a random special pet, but make sure it's one we don't have. If we have it, just reroll the dice.
							// Still add a safeguard just in case
							int maxTries = 100;
							int tryCount = 0;
							do { 
								petDef = petDefs.GetRandomValue();
								tryCount++;
							} while (UsersManager.currentUser.petCollection.IsPetUnlocked(petDef.sku) && tryCount < maxTries);
						} else {
							// This should never happen!
							// We should never be opening a golden egg when all golden eggs had been collected
							Debug.LogError("We should never be opening a golden egg when all golden eggs had been collected");
						}
					}

					// c) Normal case: random pet of the target rarity
					else {
						// If tutorial is not completed, choose from a limited pool
						if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.EGG_REWARD)) {
							do {
								petDef = petDefs.GetRandomValue();
							} while(!petDef.GetAsBool("startingPool"));
						} else {
							// Default behaviour
							petDef = petDefs.GetRandomValue();
						}
					}

					// Create the egg reward!
					if(petDef != null) {
						m_reward = CreateTypePet(petDef);
						Debug.Log("<color=purple>EGG REWARD GENERATED FOR EGG " + m_sku + ":\n" + m_reward.ToString() + "</color>");
					} else {
						Debug.LogError("<color=red>COULDN'T GENERATE EGG REWARD FOR EGG " + m_sku + "!" + "</color>");
					}
				} break;
			}
		}

		/// <summary>
		/// Implementation of the abstract Collect() method.
		/// </summary>
		override protected void DoCollect() {
			// Push the egg's reward to the stack
			if (m_reward != null) {
				UsersManager.currentUser.rewardStack.Push(m_reward);
			}
		}
	}
}
