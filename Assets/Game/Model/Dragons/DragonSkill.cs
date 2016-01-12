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
	public static readonly int NUM_LEVELS = 6;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Def
	private DragonSkillDef m_def = null;
	public DragonSkillDef def { get { return m_def; }}

	private DragonDef.SkillData m_data = null;
	public DragonDef.SkillData skillData { get { return m_data; }}

	// Values
	public float value { get { return GetValueAtLevel(level); }}
	public float nextLevelValue { get { return GetValueAtLevel(nextLevel); }}

	// Levels
	private int m_level = 0;
	public int level { get { return m_level; }}
	public int nextLevel { get { return Mathf.Min(m_level + 1, lastLevel); }}
	public int lastLevel { get { return NUM_LEVELS - 1; }}

	// Level unlock prices
	public long nextLevelUnlockPrice { get { return skillData.m_unlockPrices[nextLevel]; }}

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
	/// <param name="_data">The initialization data of this skill.</param>
	public DragonSkill(DragonData _owner, DragonDef.SkillData _data) {
		m_owner = _owner;
		m_data = _data;
		m_def = DefinitionsManager.dragonSkills.GetDef(_data.m_sku);

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
		return m_data.m_valueRange.Lerp(levelDelta) + m_valueOffset;
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
