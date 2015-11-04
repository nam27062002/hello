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

	// Skill types
	public enum EType {
		BITE,
		SPEED,
		BOOST,
		FIRE,
		COUNT
	};

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// To be set on the inspector only
	// Generic
	[SerializeField] private EType m_type = EType.BITE;
	public EType type { get { return m_type; }}

	[SerializeField] private string m_tidName = "";
	public string tidName { get { return m_tidName; }}
	
	[SerializeField] private string m_tidDescription = "";
	public string tidDescription { get { return m_tidDescription; }}

	// Values
	[SerializeField] private Range m_valueRange = new Range(0f, 1f);
	private float m_valueOffset = 0;
	public float value { get { return GetValueAtLevel(level); }}
	public float nextLevelValue { get { return GetValueAtLevel(nextLevel); }}

	// Levels
	private int m_level = 0;
	public int level { get { return m_level; }}
	public int nextLevel { get { return Mathf.Min(m_level + 1, lastLevel); }}
	public int lastLevel { get { return NUM_LEVELS - 1; }}

	// Level unlock prices
	[SerializeField] private long[] m_unlockPrices = new long[NUM_LEVELS];
	public long nextLevelUnlockPrice { get { return m_unlockPrices[nextLevel]; }}

	// Progress
	public float progress { get { return Mathf.InverseLerp(0, lastLevel, level); }}

	// Internal
	// Only to be set once
	[NonSerialized] private DragonData m_owner = null;	// [AOC] Avoid recursive serialization!!
	public DragonData owner { get { return m_owner; }}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_owner">The dragon data this progression belongs to.</param>
	/// <param name="_type">The type of this skill.</param>
	public DragonSkill(DragonData _owner, EType _type) {
		m_owner = _owner;
		m_type = _type;

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
			// Subtract cost from user profile
			UserProfile.AddCoins(-nextLevelUnlockPrice);

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
	/// This includes not being the last level and have enough resources.
	/// </summary>
	/// <returns>Whether the next level can be unlocked or not.</returns>
	public bool CanUnlockNextLevel() {
		// Last level?
		if(level >= lastLevel) return false;

		// Enough resources?
		if(UserProfile.coins < nextLevelUnlockPrice) return false;

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
