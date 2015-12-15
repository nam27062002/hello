﻿// DragonData.cs
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
/// Definition of a dragon, together with its current values.
/// Every dragon ID must be linked to one DragonData in the DragonManager prefab.
/// </summary>
[Serializable]
public class DragonData {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// All dragons have the same amount of levels
	public static readonly int NUM_LEVELS = 10;

	/// <summary>
	/// Auxiliar serializable class to save/load to persistence.
	/// </summary>
	[Serializable]
	public class SaveData {
		// Only dynamic data is relevant
		[HideEnumValues(true, true)] public DragonId id;	// We don't trust the array order however, so keep the unique dragon ID with each data pack
		public float xp = 0;
		public int level = 0;
		public int[] skillLevels = new int[(int)DragonSkill.EType.COUNT];
		public bool owned = false;
	}

	// Dragons can be unlocked with coins when the previous tier is completed (all dragons in it at max level), or directly with PC.
	public enum LockState {
		ANY = -1,	// Any of the below states
		LOCKED,		// Previous tier hasn't been completed
		AVAILABLE,	// Previous tier has been completed but the dragon hasn't been purchased
		OWNED		// Dragon has been purchased and can be used
	}

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// General Data
	[SerializeField] [HideEnumValues(true, true)] private DragonId m_id = DragonId.NONE;
	public DragonId id { get { return m_id; }}
	
	[SerializeField] [HideEnumValues(false, true)] private DragonTier m_tier = DragonTier.TIER_0;
	public DragonTier tier { get { return m_tier; }}

	[SerializeField] private string m_tidName = "";
	public string tidName { get { return m_tidName; }}

	[SerializeField] private string m_tidDescription = "";
	public string tidDescription { get { return m_tidDescription; }}

	[SerializeField] private string m_prefabPath = "";
	public string prefabPath { get { return m_prefabPath; }}

	[SerializeField] private string m_menuPrefabPath = "";
	public string menuPrefabPath { get { return m_menuPrefabPath; }}

	[SerializeField] private float m_cameraZoomOffset = 0f;
	public float cameraZoomOffset { get { return m_cameraZoomOffset; }}

	// Progression
	[SerializeField] private int m_unlockPriceCoins = 0;
	public int unlockPriceCoins { get { return m_unlockPriceCoins; }}

	[SerializeField] private int m_unlockPricePC = 0;
	public int unlockPricePC { get { return m_unlockPricePC; }}

	[SerializeField] private bool m_owned = false;
	public LockState lockState { get { return GetLockState(); }}
	public bool isLocked { get { return lockState == LockState.LOCKED; }}
	public bool isOwned { get { return m_owned; }}

	[SerializeField] private DragonProgression m_progression = null;	// Will be exposed via a custom editor
	public DragonProgression progression { get { return m_progression; }}

	// Level-dependant stats
	[SerializeField] private Range m_healthRange = new Range(1, 100);
	public float maxHealth { get { return GetMaxHealthAtLevel(progression.level); }}

	[SerializeField] private Range m_scaleRange = new Range(0.5f, 1.5f);
	private float m_scaleOffset = 0;
	public float scale { get { return GetScaleAtLevel(progression.level); }}

	// Constant stats
	[SerializeField] private float m_healthDrainPerSecond = 10f;
	public float healthDrainPerSecond { get { return m_healthDrainPerSecond; }}

	[SerializeField] private float m_maxEnergy = 160f;
	public float maxEnergy { get { return m_maxEnergy; }}

	[SerializeField] private float m_energyDrainPerSecond = 10f;
	public float energyDrainPerSecond { get { return m_energyDrainPerSecond; }}
	
	[SerializeField] private float m_energyRefillPerSecond = 25f;
	public float energyRefillPerSecond { get { return m_energyRefillPerSecond; }}

	[SerializeField] private float m_maxFury = 160f;
	public float maxFury { get { return m_maxFury; }}
	
	[SerializeField] private float m_furyDuration = 15f; //seconds
	public float furyDuration { get { return m_furyDuration; }}

	// Skills
	[SerializeField] private DragonSkill[] m_skills;
	public DragonSkill[] skills { get { return m_skills; }}
	public DragonSkill bite { get { return GetSkill(DragonSkill.EType.BITE); }}
	public DragonSkill speed { get { return GetSkill(DragonSkill.EType.SPEED); }}
	public DragonSkill boost { get { return GetSkill(DragonSkill.EType.BOOST); }}
	public DragonSkill fire { get { return GetSkill(DragonSkill.EType.FIRE); }}

	// Items
	// [AOC] TODO!!

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor
	/// </summary>
	public DragonData() {
		// Progression
		m_progression = new DragonProgression(this);
		
		// Skills
		m_skills = new DragonSkill[(int)DragonSkill.EType.COUNT];
		for(int i = 0; i < m_skills.Length; i++) {
			m_skills[i] = new DragonSkill(this, (DragonSkill.EType)i);
		}

		m_scaleOffset = 0;
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Gets the skill.
	/// </summary>
	/// <returns>The skill.</returns>
	/// <param name="_type">_type.</param>
	public DragonSkill GetSkill(DragonSkill.EType _type) {
		return m_skills[(int)_type];
	}

	/// <summary>
	/// Compute the max health at a specific level.
	/// </summary>
	/// <returns>The dragon max health at the given level.</returns>
	/// <param name="_level">The level at which we want to know the max health value.</param>
	public float GetMaxHealthAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.lastLevel, _level);
		return m_healthRange.Lerp(levelDelta);
	}

	/// <summary>
	/// Compute the scale at a specific level.
	/// </summary>
	/// <returns>The dragon scale at the given level.</returns>
	/// <param name="_level">The level at which we want to know the scale value.</param>
	public float GetScaleAtLevel(int _level) {
		float levelDelta = Mathf.InverseLerp(0, progression.lastLevel, _level);
		return m_scaleRange.Lerp(levelDelta) + m_scaleOffset;
	}

	/// <summary>
	/// Offsets speed value. Used for Debug purposes on Preproduction fase.
	/// </summary>
	public void OffsetSpeedValue(float _speed) {
 		GetSkill(DragonSkill.EType.SPEED).OffsetValue(_speed);
	}

	/// <summary>
	/// Offsets the scale value.
	/// </summary>
	public void OffsetScaleValue(float _scale) {
		m_scaleOffset += _scale;
	}

	/// <summary>
	/// Gets the current lock state of this dragon.
	/// </summary>
	/// <returns>The lock state for this dragon.</returns>
	public LockState GetLockState() {
		// a) Is dragon owned?
		if(m_owned) return LockState.OWNED;

		// b) Is tier unlocked?
		if(DragonManager.IsTierUnlocked(this.tier)) {
			return LockState.AVAILABLE;
		}

		// c) Dragon locked
		return LockState.LOCKED;
	}

	/// <summary>
	/// Unlock this dragon (will be OWNED from now on). Doesn't do any currency transaction.
	/// Triggers the DRAGON_ACQUIRED event.
	/// </summary>
	public void Acquire() {
		// Skip if already owned
		if(m_owned) return;

		// Just change owned status
		m_owned = true;

		// Dispatch global event
		Messenger.Broadcast<DragonData>(GameEvents.DRAGON_ACQUIRED, this);
	}

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public void Load(SaveData _data) {
		// Make sure the persistence object corresponds to this dragon
		if(!DebugUtils.Assert(_data.id == id, "Attempting to load persistence data corresponding to a different dragon ID, aborting")) {
			return;
		}

		// Just read values from persistence object
		m_owned = _data.owned;
		progression.Load(_data.xp, _data.level);

		for(int i = 0; i < _data.skillLevels.Length; i++) {
			m_skills[i].Load(_data.skillLevels[i]);
		}
	}
	
	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SaveData Save() {
		// Create new object, initialize and return it
		SaveData data = new SaveData();

		data.id = id;
		data.owned = m_owned;
		data.xp = progression.xp;
		data.level = progression.level;

		data.skillLevels = new int[m_skills.Length];
		for(int i = 0; i < m_skills.Length; i++) {
			data.skillLevels[i] = m_skills[i].level;
		}

		return data;
	}
}
