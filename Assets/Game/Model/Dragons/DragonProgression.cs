// DragonProgression.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/09/2015.
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
/// Auxiliar class to be able to manage a dragon's XP/Level progression in cool way.
/// XP values represent the required amount of XP to reach a given level.
/// For level 0 it should always be 0.
/// XP can't be directly set, except when loading from persistence. It can only be 
/// added via the AddXp() method.
/// </summary>
[Serializable]
public class DragonProgression : SerializableClass {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// XP
	private float m_xp = 0;
	public float xp { get { return m_xp; }}

	private float[] m_levelsXp = null;	// XP required to unlock each level. Includes redundant level 0 (xp required for level 0 is always 0).
	public float[] levelsXP { get { return m_levelsXp; }}

	public float xpToNextLevel { get { return levelsXP[nextLevel] - levelsXP[level]; }}	// Should be safe, nextLevel is protected and level should never be > lastLevel
	public Range xpRange { get { return GetXpRangeForLevel(level); }}
		
	// Level
	private int m_level = 0;
	public int level { get { return m_level; }}	// [0..maxLevel]

	public int maxLevel { get { return m_levelsXp.Length - 1; }}
	public int nextLevel { get { return Mathf.Min(m_level + 1, maxLevel); }}
	public bool isMaxLevel { get { return m_level == maxLevel; }}
	public bool isMaxed { get { return xp >= levelsXP[maxLevel]; }}

	// Progress [0..1]
	public float progressByXp { get { return Mathf.InverseLerp(0f, levelsXP[maxLevel], m_xp); }}
	public float progressByLevel { get { return Mathf.InverseLerp(0, maxLevel, m_level); }}
	public float progressCurrentLevel { 
		get { 
			// If we've already reached last level, progress is always 1
			if (level >= maxLevel) return 1f;
			return Mathf.InverseLerp(levelsXP[m_level], levelsXP[nextLevel], m_xp);
		}
	}

	// Internal
	// Only to be set once
	[NonSerialized] private DragonDataClassic m_owner = null;	// [AOC] Avoid recursive serialization!!
	public DragonDataClassic owner { get { return m_owner; }}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_owner">The dragon data this progression belongs to.</param>
	public DragonProgression(DragonDataClassic _owner) {
		// Store owner dragon
		m_owner = _owner;

		// Get definition based on dragon sku
		DefinitionNode progressionDef = null;
		if(_owner != null) {
			// progressionDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_PROGRESSION, _owner.def.sku);
			progressionDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.DRAGON_PROGRESSION, "dragonSku", _owner.def.sku);
		}

		// Init!
		InitFromDef(progressionDef);
	}

	/// <summary>
	/// Initialize the level's XP data with the given dragonDefinition.
	/// </summary>
	/// <param name="_def">The dragon definition to be used.</param>
	public void InitFromDef(DefinitionNode _def) {
		RefreshDefinition( _def );
	}

	// Refresh info from definition
	public void RefreshDefinition( DefinitionNode _def )
	{
		// Check params
		if(_def == null) {
			// Clear current data and return
			m_levelsXp = new float[1];
			m_levelsXp[0] = 0;
			return;
		}

		// Get relevant data from definition
		int maxLevel = _def.GetAsInt("maxLevel", 1);

		// Reset xp array
		m_levelsXp = new float[maxLevel];
		m_levelsXp[0] = 0;	// XP required for level 0 is always 0
		for(int i = 0; i < maxLevel; i++) {
			m_levelsXp[i] = _def.GetAsFloat("xpLevel" + i);
		}
	}


	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Add experience to this progression.
	/// Optionally check for level ups, otherwise can be manually done by calling the IsLevelUpReady() and LevelUp() methods.
	/// </summary>
	/// <param name="_xpToAdd">The amount of xp to be added.</param>
	/// <param name="_checkLevelUp">Whether to check for level ups or not.</param>
	public void AddXp(float _xpToAdd, bool _checkLevelUp = false) {
		// Experience can't be subtracted
		if(_xpToAdd <= 0) return;

		// Add xp, capping to max level's XP
		m_xp = Mathf.Min(m_xp + _xpToAdd, m_levelsXp[maxLevel]);

		// Check for level ups
		if(_checkLevelUp) {
			LevelUp();
		}
	}

	/// <summary>
	/// Sets the global xp to a target delta.
	/// </summary>
	/// <param name="_delta">Global xp delta.</param>
	public void SetXp(float _xp) {
		// Set XP
		m_xp = Mathf.Clamp(_xp, 0f, levelsXP[maxLevel]);

		// Update level
		int newLevel = GetLevelFromXp(m_xp);
		if(newLevel > m_level) {
			LevelUp();
		} else if(newLevel < m_level) {
			// "Level Down", which is not possible in normal gameplay so we have to do it manually
			while(m_level > newLevel) {
				m_level--;
				Messenger.Broadcast<IDragonData>(MessengerEvents.DRAGON_LEVEL_UP, m_owner);
			}
		}
	}

	/// <summary>
	/// Obtain the first level corresponding to a given XP value.
	/// </summary>
	/// <returns>The first level matching the given XP value.</returns>
	/// <param name="_xp">The XP value to be checked.</param>
	public int GetLevelFromXp(float _xp) {
		for(int i = 1; i < levelsXP.Length; i++) {
			if(_xp < levelsXP[i]) {
				return i - 1;
			}
		}
		return maxLevel;	// We're at the last level
	}

	/// <summary>
	/// Get the xp values of a specific level.
	/// </summary>
	/// <returns>The min and max absolute xp values for the given level.</returns>
	/// <param name="_level">The level whose data we want.</param>
	public Range GetXpRangeForLevel(int _level) {
		// Check level
		DebugUtils.Assert(_level >= 0 && _level <= maxLevel, "Level out of bounds!");

		// Special case for last level
		int nextLevel = Mathf.Min(_level + 1, maxLevel);
		return new Range(levelsXP[_level], levelsXP[nextLevel]);
	}

	/// <summary>
	/// Check whether the current amount of experience matches a level above our current one.
	/// </summary>
	/// <returns><c>true</c> if there is a pending level up.</returns>
	public bool IsLevelUpReady() {
		// For sure not if we're already in the last level
		if(m_level >= maxLevel) return false;

		// Check next level
		if(m_xp >= levelsXP[m_level + 1]) {
			return true;
		}

		// No level ups pending
		return false;
	}

	/// <summary>
	/// Level up this dragon as much levels as possible.
	/// </summary>
	/// <returns>The amount of levels progressed.</returns>
	public int LevelUp() {
		int levelUpCount = 0;
		while(IsLevelUpReady()) {
			// Everything ok! Do it
			m_level++;
			levelUpCount++;

			// Dispatch global event
			Messenger.Broadcast<IDragonData>(MessengerEvents.DRAGON_LEVEL_UP, m_owner);
		}

		return levelUpCount;
	}

	/// <summary>
	/// Force a specific level.
	/// Use only in specific cases where level needs to be forced! (i.e. Tournament).
	/// Won't trigger DRAGON_LEVEL_UP event.
	/// </summary>
	/// <param name="_level">Target level.</param>
	public void SetLevel(int _level) {
		// Clamp given level
		_level = Mathf.Clamp(_level, 0, maxLevel);

		// Store new level
		m_level = _level;

		// Make XP match the new level
		m_xp = levelsXP[m_level];
	}

	/// <summary>
	/// Set the dragon to max level.
	/// Use only in specific cases where level needs to be forced! (i.e. Tournament)
	/// Won't trigger DRAGON_LEVEL_UP event.
	/// </summary>
	public void SetToMaxLevel() {
		SetLevel(maxLevel);
	}

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize the progression with the given xp and level.
	/// </summary>
	/// <param name="_xp">The xp value to be used as current xp.</param>
	/// <param name="_level">The level to be used as current level.</param>
	public void Load(float _xp, int _level) {
		// Check and apply xp
		m_xp = Mathf.Max(0f, _xp);

		// Check and apply level
		m_level = Mathf.Clamp(_level, 0, maxLevel);
	}
}
