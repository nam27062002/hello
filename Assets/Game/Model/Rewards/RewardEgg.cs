﻿// RewardEgg.cs
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
		private const string RANDOM_STATE_PREFS_KEY = "RewardEgg.RandomState";

		//------------------------------------------------------------------------//
		// CLASS MEMBERS AND METHODS											  //
		//------------------------------------------------------------------------//
		private static List<float> sm_petWeights = new List<float>();
		private static Dictionary<string, float> sm_petOverrideProbs = new Dictionary<string, float>();
		public static void OverridePetProb(string _sku, float _weight) {			
			sm_petOverrideProbs.Add(_sku, _weight);
		}

		public static void RemoveOverridePetProb(string _sku) {
			sm_petOverrideProbs.Remove(_sku);
		}


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
		public RewardEgg(string _sku, string _source, bool _buildReward = true) {
			m_source = _source;
			Build(_sku, _buildReward);	// This will generate a random reward following the gacha rules
		}

		/// <summary>
		/// Internal builder with egg sku and reward sku.
		/// </summary>
		/// <param name="_sku">Egg sku.</param>
		private void Build(string _sku, bool _buildReward) {
			//Debug.Log("<color=purple>Building egg with sku " + _sku + "</color>");

			// Internal initializer
			base.Init(TYPE_CODE);

			// Override some fields
			m_sku = _sku;
			m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, _sku);

			if(_buildReward) BuildReward();

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
			//Debug.Log("<color=purple>Building egg reward!</color>");

			// Get the reward definition
			DefinitionNode rewardTypeDef = null;
			if(m_sku.Equals(Egg.SKU_GOLDEN_EGG)) {
				rewardTypeDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGG_REWARDS, "pet_special");
			} else {
				rewardTypeDef = EggManager.GenerateReward();
			}

			// Nothing else to do if def is null
			if(rewardTypeDef == null) {
				Debug.Log("<color=red>COULDN'T BUILD EGG REWARD!</color>");
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

					// Remove all hidden and disabled pets
					petDefs.RemoveAll(
						(DefinitionNode _petDef) => {
							// Several conditions:
							// a) Hidden pets (usually WIP pets or pets meant to be revealed in future updates)
							if(!DebugSettings.showHiddenPets) {		// Check cheats
								if(_petDef.GetAsBool("hidden", false)) return true;
							}

							// b) Not-in-gatcha pets (pets unlocked by other means: global event reward, etc.)
							if(_petDef.GetAsBool("notInGatcha", false)) return true;

							// c) Pets linked to a specific season different than the current one
							string targetSeason = _petDef.GetAsString("associatedSeason", SeasonManager.NO_SEASON_SKU);
							if(targetSeason != SeasonManager.NO_SEASON_SKU && targetSeason != SeasonManager.activeSeason) return true;

							// Pet is valid! Don't remove it from the list
							return false;
						}
					);

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
						if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.EGG_REWARD)) 
						{
							Debug.Log(Colors.pink.Tag("TUTORIAL NOT COMPLETED, USING REDUCED POOL"));
							List<DefinitionNode> newPetDefs = new List<DefinitionNode>();
							for (int i = 0; i < petDefs.Count; i++) {
								DefinitionNode newPetDef = petDefs[i];
								if (newPetDef.GetAsBool("startingPool"))
								{
									newPetDefs.Add( newPetDef );
								}
							}

							if ( newPetDefs.Count > 0 )
							{
								petDef = newPetDefs.GetRandomValue();
							}
							else
							{
								petDef = petDefs.GetRandomValue();	
							}
						} else {
							// Default behaviour
							float totalWeight = 0f;
							if (sm_petWeights.Count < petDefs.Count) {
								sm_petWeights.Resize(petDefs.Count);
							}

							// [AOC] Creating a new probability set every time will reset the random seed, causing the random to always return the same value
							//		 Restore the random seed every time we generate a reward so we keep the probability uniform.
							ProbabilitySet petProb = new ProbabilitySet();
							if(PlayerPrefs.HasKey(RANDOM_STATE_PREFS_KEY)) {
								UnityEngine.Random.State s = petProb.randomState;
								petProb.randomState = s.Deserialize(PlayerPrefs.GetString(RANDOM_STATE_PREFS_KEY));
							}

							// Add pets to the probability set
							for (int i = 0; i < petDefs.Count; ++i) {
								float value = 1f;
								if (sm_petOverrideProbs.ContainsKey(petDefs[i].sku)) {
									value = sm_petOverrideProbs[petDefs[i].sku];
								}

								sm_petWeights[i] = value;
								totalWeight += value;

								petProb.AddElement(petDefs[i].sku, 1);
							}

							// Set weights for each pet
							for (int i = 0; i < petDefs.Count; ++i) {
								petProb.SetProbability(i, sm_petWeights[i] / totalWeight);
							}

							// Get a random one!
							int idx = petProb.GetWeightedRandomElementIdx();
							petDef = petDefs[idx];

							// Store random state to be used on the next Egg reward
							PlayerPrefs.SetString(RANDOM_STATE_PREFS_KEY, petProb.randomState.Serialize());
						}
					}

					// Create the egg reward!
					if(petDef != null) {
						m_reward = CreateTypePet(petDef, m_sku);
						#if UNITY_EDITOR
						Color[] colorTags = {
							UIConstants.GetRarityColor(Rarity.COMMON),
							UIConstants.GetRarityColor(Rarity.RARE),
							UIConstants.GetRarityColor(Rarity.EPIC),
							UIConstants.GetRarityColor(Rarity.SPECIAL)
						};
						Debug.Log(Colors.purple.Tag("EGG REWARD GENERATED FOR EGG " + m_sku + ":\n") + colorTags[(int)m_reward.rarity].Tag(m_reward.ToString()));
						#endif
					} else {
						Debug.LogError(Color.red.Tag("COULDN'T GENERATE EGG REWARD FOR EGG " + m_sku + " and rarity " + m_rarity + "!"));
					}
				} break;
			}
		}

		/// <summary>
		/// Implementation of the abstract Collect() method.
		/// </summary>
		override protected void DoCollect() {
            HDTrackingManager.Instance.Notify_EggOpened();

            // Push the egg's reward to the stack
            if (m_reward != null) {
				// Check again whether the reward needs to be replaced or not
				// (i.e. we just opened an egg that has given us the same reward)
				m_reward.CheckReplacement();
				UsersManager.currentUser.PushReward(m_reward);
			}
		}

		/// <summary>
		/// Return a visual representation of the reward.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Metagame.RewardEgg"/>.</returns>
		override public string ToString() {
			return m_reward.sku + (m_reward.WillBeReplaced() ? " (d)" : "");
		}
	}
}
