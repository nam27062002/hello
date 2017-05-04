// EggRewardData.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/01/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple class to store all data related to an egg reward.
/// </summary>
public class EggReward {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Rarity {
		UNKNOWN = -1,

		COMMON,
		RARE,
		EPIC,
		SPECIAL,

		COUNT
	}

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Given a rarity sku, return the equivalent enum value.
	/// </summary>
	/// <returns>The rarity enum value.</returns>
	/// <param name="_raritySku">Rarity sku to be checked.</param>
	public static Rarity SkuToRarity(string _raritySku) {
		// We could double-check with content, but it's much faster if we hardcode it (and these skus are not supposed to change)
		//DefinitionNode rarityDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.RARITIES, _raritySku);
		//return (Rarity)rarityDef.GetAsInt("order");
		switch(_raritySku) {
			case "common":	return Rarity.COMMON; 	break;
			case "rare":	return Rarity.RARE;		break;
			case "epic":	return Rarity.EPIC;		break;
			case "special":	return Rarity.SPECIAL;	break;
			default:		return Rarity.UNKNOWN;	break;
		}
		return Rarity.UNKNOWN;
	}

	/// <summary>
	/// Given a rarity, return its sku.
	/// </summary>
	/// <returns>The sku of the given rarity. Empty string if unknown.</returns>
	/// <param name="_rarity">Rarity whose sku we want.</param>
	public static string RarityToSku(Rarity _rarity) {
		// We could double-check with content, but it's much faster if we hardcode it (and these skus are not supposed to change)
		switch(_rarity) {
			case Rarity.COMMON:		return "common";		break;
			case Rarity.RARE:		return "rare";			break;
			case Rarity.EPIC:		return "epic";			break;
			case Rarity.SPECIAL:	return "special";		break;
			default:				return string.Empty;	break;
		}
		return string.Empty;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Reward type, matches rewardDefinitions "type" property.
	private string m_type = "";
	public string type { 
		get { return m_type; }
	}

	// The reward definition, to access some extra info
	private DefinitionNode m_def = null;
	public DefinitionNode def { 
		get { return m_def; }
	}

	// The definition of the rewarded item. Typically a Pet definition.
	private DefinitionNode m_itemDef = null;
	public DefinitionNode itemDef { 
		get { return m_itemDef; }
	}

	// Shortcut to the reward's rarity. Initialized from the definition.
	private Rarity m_rarity = Rarity.UNKNOWN;
	public Rarity rarity {
		get { return m_rarity; }
	}

	// Is it a duplicated?
	private bool m_duplicated = false;
	public bool duplicated { 
		get { return m_duplicated; }
	}

	// Golden egg fragments given instead of the reward. Only if bigger than 0.
	// Happens when it's a duplicated and we still haven't opened all the golden eggs.
	private int m_fragments = 0;
	public int fragments {
		get { return m_fragments; }
	}

	// Coins to be given instead of the reward. Only if bigger than 0.
	// Happens when it's a duplicated and all golden eggs have already been collected.
	private long m_coins = 0;
	public long coins { 
		get { return m_coins; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this data object from a reward definition.
	/// </summary>
	/// <param name="_rewardDef">Reward def.</param>
	public void InitFromDef(DefinitionNode _rewardDef) {
		// Store def
		m_def = _rewardDef;

		// Reset all variables
		m_type = "";
		m_itemDef = null;
		m_rarity = Rarity.UNKNOWN;
		m_duplicated = false;
		m_fragments = 0;
		m_coins = 0;

		// Nothing else to do if def is null
		if(_rewardDef == null) return;

		// Initialize the reward data based on type
		m_type = _rewardDef.GetAsString("type");
		switch(m_type) {
			case "pet": {
				// Find out reward rarity
				string raritySku = _rewardDef.Get("rarity");
				m_rarity = SkuToRarity(raritySku);

				// Special case if golden egg
				List<DefinitionNode> petDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.PETS, "rarity", raritySku);
				if(m_rarity == Rarity.SPECIAL) {
					// Get a random special pet, but make sure it's one we don't have. If we have it, just reroll the dice.
					// [AOC] Unless forcing a specific sku from cheats!
					if(CPGachaTest.rewardChanceMode == CPGachaTest.RewardChanceMode.FORCED_PET_SKU) {
						// Get that specific pet!
						m_itemDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, CPGachaTest.forcedPetSku);
					} else {
						// Default behaviour
						m_itemDef = petDefs.GetRandomValue();
					}

					// We should never open a special egg when all of them are already collected, but check just in case!
					if(!EggManager.allGoldenEggsCollected) {
						// Still add a safeguard just in case
						int maxTries = 100;
						int tryCount = 0;
						while(UsersManager.currentUser.petCollection.IsPetUnlocked(m_itemDef.sku) && tryCount < maxTries) {
							m_itemDef = petDefs.GetRandomValue();
							tryCount++;
						}
					} else {
						// This should never happen!
						// We should never be opening a golden egg when all golden eggs had been collected
						// Put fake values to easily detect if it happens
						m_duplicated = true;
						m_fragments = 9999;
						m_coins = 9999;
					}
				} else {
					// Select a pet from the given rarity
					// Forcing a specific sku from cheats!
					if(CPGachaTest.rewardChanceMode == CPGachaTest.RewardChanceMode.FORCED_PET_SKU) {
						// Get that specific pet!
						m_itemDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, CPGachaTest.forcedPetSku);
					} 

					// If tutorial is not completed, choose from a limited pool
					else if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.EGG_REWARD)) {
						do {
							m_itemDef = petDefs.GetRandomValue();
						} while(!m_itemDef.GetAsBool("inFirstGatcha"));
					} 

					// Default behaviour: random pet of the target rarity
					else {
						// Default behaviour
						m_itemDef = petDefs.GetRandomValue();
					}

					// If the pet is already owned, give special egg part or coins instead
					// Cheat support
					switch(CPGachaTest.duplicateMode) {
						case CPGachaTest.DuplicateMode.DEFAULT: {
							m_duplicated = UsersManager.currentUser.petCollection.IsPetUnlocked(m_itemDef.sku);
						} break;
						
						case CPGachaTest.DuplicateMode.ALWAYS: {
							m_duplicated = true;
						} break;
						
						case CPGachaTest.DuplicateMode.NEVER: {
							m_duplicated = false;
						} break;
						
						case CPGachaTest.DuplicateMode.RANDOM: {
							m_duplicated = Random.value > 0.5f;
						} break;
					}

					// If duplicated, give alternative rewards
					if(m_duplicated) {
						// Have all golden eggs been collected?
						if(EggManager.allGoldenEggsCollected) {
							// Yes! Give coins rather than golden egg fragments (based on rarity)
							m_coins = _rewardDef.GetAsLong("duplicateCoinsGiven");
						} else {
							// No! Give golden egg fragments based on rarity
							m_fragments = _rewardDef.GetAsInt("duplicateFragmentsGiven");
						}
					}
				}

				Debug.Log("EGG REWARD GENERATED:\n" + this.ToString());
			} break;
		}
	}

	/// <summary>
	/// For debug purposes.
	/// </summary>
	public string ToString() {
		return "Type: " + m_type + "\n" +
			"Def: " + (m_def == null ? "NULL" : m_def.sku) + "\n" +
			"Item Def: " + (m_itemDef == null ? "NULL" : m_itemDef.sku) + "\n" +
			"Rarity: " + m_rarity + "\n" +
			"Duplicated: " + m_duplicated + "\n" +
			"Fragments: " + m_fragments + "\n" +
			"Coins: " + m_coins;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}