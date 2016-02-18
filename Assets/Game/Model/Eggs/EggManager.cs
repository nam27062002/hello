// EggManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global manager of eggs.
/// Has its own asset in the Resources/Singletons folder with all the required parameters.
/// </summary>
[CreateAssetMenu]
public class EggManager : SingletonScriptableObject<EggManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly int INVENTORY_SIZE = 3;

	public enum RewardType {
		SUIT_COMMON,	// [AOC] TODO!!	81,4	
		SUIT_RARE,		// [AOC] TODO!!	11,7
		SUIT_EPIC,		// [AOC] TODO!!	3,8
		PET_RARE,		// [AOC] TODO!!	1,7
		PET_EPIC,		// [AOC] TODO!!	0,9
		DRAGON_SPECIAL,	// [AOC] TODO!!	0,5

		COUNT
	};

	[Serializable]
	public class SaveData {

	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// To be defined from the inspector
	[SerializeField] private ProbabilitySet m_rewardDropRate = new ProbabilitySet(
		new ProbabilitySet.Element[] {
			new ProbabilitySet.Element("Common Suit"),
			new ProbabilitySet.Element("Rare Suit"),
			new ProbabilitySet.Element("Epic Suit"),
			new ProbabilitySet.Element("Rare Pet"),
			new ProbabilitySet.Element("Epic Pet"),
			new ProbabilitySet.Element("Special Dragon")
		}
	);



	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected void OnEnable() {
		
	}

	//------------------------------------------------------------------//
	// PUBLIC UTILS														//
	//------------------------------------------------------------------//

}