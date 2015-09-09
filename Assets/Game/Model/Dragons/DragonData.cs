// DragonData.cs
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
		public float xp;
		public int level;
		public int[] skillLevels;
	}

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	[Header("Basic data")]
	[SerializeField] private DragonID m_id = DragonID.NONE;
	public DragonID id { get { return m_id; }}
	
	[SerializeField] private int m_tier = 0;
	public int tier { get { return m_tier; }}

	[SerializeField] private string m_tidName = "";
	public string tidName { get { return m_tidName; }}

	[SerializeField] private string m_tidDescription = "";
	public string tidDescription { get { return m_tidDescription; }}

	[SerializeField] private string m_prefabPath = "";
	public string prefabPath { get { return m_prefabPath; }}

	[Header("Evolution")]
	[SerializeField] private DragonProgression m_progression = new DragonProgression();	// Will be exposed via a custom editor
	public DragonProgression progression { get { return m_progression; }}

	[Header("Level-dependant stats")]
	[SerializeField] private Range m_healthRange = new Range(1, 100);
	public float health { get { return m_healthRange.Lerp(progression.progressByLevel); }}

	[SerializeField] private Range m_scaleRange = new Range(0.5f, 1.5f);
	public float scale { get { return m_scaleRange.Lerp(progression.progressByLevel); }}

	[Header("Skills")]
	[SerializeField] private DragonSkill[] m_skills = new DragonSkill[(int)DragonSkill.EType.COUNT];
	public DragonSkill[] skills { get { return m_skills; }}
	public DragonSkill bite { get { return GetSkill(DragonSkill.EType.BITE); }}
	public DragonSkill speed { get { return GetSkill(DragonSkill.EType.SPEED); }}
	public DragonSkill boost { get { return GetSkill(DragonSkill.EType.BOOST); }}
	public DragonSkill fire { get { return GetSkill(DragonSkill.EType.FIRE); }}

	//[Header("Items")]
	// [AOC] TODO!!

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor
	/// </summary>
	public DragonData() {
		// Link this dragon data to its sub-classes
		m_progression.owner = this;
		for(int i = 0; i < m_skills.Length; i++) {
			m_skills[i].owner = this;
		}
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
		for(int i = 0; i < m_skills.Length; i++) {
			if(m_skills[i].type == _type) {
				return m_skills[i];
			}
		}
		return null;
	}

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public void Load(SaveData _data) {
		// Just read values from persistence object
		progression.Load(_data.xp, _data.level);
	}
	
	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SaveData Save() {
		// Create new object, initialize and return it
		SaveData data = new SaveData();

		data.xp = progression.xp;
		data.level = progression.level;

		data.skillLevels = new int[m_skills.Length];
		for(int i = 0; i < m_skills.Length; i++) {
			data.skillLevels[i] = m_skills[i].level;
		}

		return data;
	}
}
