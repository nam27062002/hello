// DragonSkill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to be able to manage a dragon's skill upgrades.
/// </summary>
[Serializable]
public class DragonSkill : SerializableClass {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly int NUM_LEVELS = 6;	// Redundant level 0 counts as well!

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Def
	private DefinitionNode m_def = null;
	public DefinitionNode def { get { return m_def; }}

	private DefinitionNode m_progressionDef = null;
	public DefinitionNode progressionDef { get { return m_progressionDef; }}

	// Values
	private Range m_valueRange = new Range(0f, 1f);
	public float value { get { return GetValueAtLevel(level); }}
	public float nextLevelValue { get { return GetValueAtLevel(nextLevel); }}

	// Levels
	private int m_level = 0;
	public int level { get { return m_level; }}
	public int nextLevel { get { return Mathf.Min(m_level + 1, lastLevel); }}
	public int lastLevel { get { return NUM_LEVELS - 1; }}

	// Level unlock prices
	private long[] m_unlockPrices = new long[NUM_LEVELS];	// Redundant level 0
	public long nextLevelUnlockPrice { get { return m_unlockPrices[nextLevel]; }}

	// Progress
	public float progress { get { return Mathf.InverseLerp(0, lastLevel, level); }}

	// Internal
	// Only to be set once
	[NonSerialized] private DragonData m_owner = null;	// [AOC] Avoid recursive serialization!!
	public DragonData owner { get { return m_owner; }}

	// Debug
	private float m_valueOffset = 0;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_owner">The dragon data this skill belongs to.</param>
	/// <param name="_skillSku">The sku of the skill definition to initialize this skill data.</param>
	public DragonSkill(DragonData _owner, string _skillSku) {
		// Store references
		m_owner = _owner;
		Debug.Assert(m_owner != null, "A skill data object must always be linked to a DragonData instance.");

		m_def = Definitions.GetDefinition(Definitions.Category.DRAGON_SKILLS, _skillSku);
		Debug.Assert(m_def != null, "Skill " + _skillSku + " not recognized!");

		m_progressionDef = Definitions.GetDefinition(Definitions.Category.DRAGON_SKILLS, _owner.def.sku);	// Shares sku with the dragons
		Debug.Assert(m_progressionDef != null, "Skill progression def for dragon " + _owner.def.sku + " couldn't be found!");

		// [unlockPriceCoinsLevel1]	[unlockPriceCoinsLevel2]	[unlockPriceCoinsLevel3]	[unlockPriceCoinsLevel4]	[unlockPriceCoinsLevel5]	[fireMin]	[fireMax]	[speedMin]	[speedMax]	[boostMin]	[boostMax]
		// Init value range from def
		// [AOC] Tricky! Value range is stored in columns named after the skill sku!
		m_valueRange.min = m_progressionDef.GetAsFloat(m_def.sku + "Min");	// e.g. speedMin, fireMin, boostMin
		m_valueRange.max = m_progressionDef.GetAsFloat(m_def.sku + "Max");	// e.g. speedMax, fireMax, boostMax

		// Init unlock prices from def
		m_unlockPrices = new long[NUM_LEVELS];
		m_unlockPrices[0] = 0;	// Level 0 is always free! ^_^
		for(int i = 1; i < NUM_LEVELS; i++) {
			m_unlockPrices[i] = m_progressionDef.GetAsLong("unlockPriceCoinsLevel" + i);
		}

		// Init debug vars
		m_valueOffset = 0;
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Unlock next level for this skill. Ignored if last level or not enough resources.
	/// Performs the resources transaction.
	/// </summary>
	/// <returns>Whether the next level was successfully unlocked or not.</returns>
	public bool UnlockNextLevel() {
		// Can the unlock be performed?
		if(CanUnlockNextLevel()) {
			// Level up!
			m_level++;

			// Dispatch game event
			Messenger.Broadcast<DragonSkill>(GameEvents.DRAGON_SKILL_UPGRADED, this);

			return true;
		}

		return false;
	}

	/// <summary>
	/// Check whether all the conditions are met to unlock the next level of this skill.
	/// This includes not being the last level.
	/// Doesn't check for resources.
	/// </summary>
	/// <returns>Whether the next level can be unlocked or not.</returns>
	public bool CanUnlockNextLevel() {
		// Last level?
		if(level >= lastLevel) return false;

		// Everything ok!
		return true;
	}

	/// <summary>
	/// Compute the skill's value at a specific level.
	/// </summary>
	/// <returns>The skill's value at the given level.</returns>
	/// <param name="_level">The level at which we want to know the skill's value.</param>
	public float GetValueAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, lastLevel, _level);
		return m_valueRange.Lerp(levelDelta) + m_valueOffset;
	}

	/// <summary>
	/// Offsets the skill value. Used for Debug purposes on Preproduction fase.
	/// </summary>
	public void OffsetValue(float _value) {
		m_valueOffset += _value;
	}

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize the progression with the given level.
	/// </summary>
	/// <param name="_level">The level to be used as current level.</param>
	public void Load(int _level) {
		// Check and apply level
		m_level = Mathf.Clamp(_level, 0, lastLevel);
	}
}
