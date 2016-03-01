// DragonDef.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/12/2015.
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
/// Definition of a dragon.
/// </summary>
[Serializable]
public class DragonDef : Definition {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	[Serializable]
	public class SkillData {
		[SkuList(typeof(DragonSkillDef), false)] public string m_sku = "";
		public Range m_valueRange = new Range(0f, 1f);
		public long[] m_unlockPrices = new long[DragonSkillDef.NUM_LEVELS];
	}

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	[SerializeField] [HideEnumValues(false, true)] private DragonTier m_tier = DragonTier.TIER_0;
	public DragonTier tier { get { return m_tier; }}
	public DragonTierDef tierDef { get { return DefinitionsManager.dragonTiers.GetDef(m_tier); }}

	[Separator("Info")]
	[SerializeField] private int m_menuOrder = 0;
	public int menuOrder { get { return m_menuOrder; }}

	[SerializeField] private string m_tidName = "";
	public string tidName { get { return m_tidName; }}

	[SerializeField] private string m_tidDescription = "";
	public string tidDescription { get { return m_tidDescription; }}

	[Separator("Prefabs")]
	[FileList("Resources/Game/Dragons", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] private string m_prefabPath = "";
	public string prefabPath { get { return m_prefabPath; }}

	[FileList("Resources/UI/Menu/Dragons", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] private string m_menuPrefabPath = "";
	public string menuPrefabPath { get { return m_menuPrefabPath; }}

	[Separator("Technical")]
	[SerializeField] private float m_cameraDefaultZoom = 10f;
	public float cameraDefaultZoom { get { return m_cameraDefaultZoom; }}
	[SerializeField] private float m_cameraFarZoom = 50f;
	public float cameraFarZoom { get { return m_cameraFarZoom; }}

	[Separator("Progression")]
	[SerializeField] private int m_unlockPriceCoins = 0;
	public int unlockPriceCoins { get { return m_unlockPriceCoins; }}

	[SerializeField] private int m_unlockPricePC = 0;
	public int unlockPricePC { get { return m_unlockPricePC; }}

	[SerializeField] private float[] m_levelsXp = new float[10];	// [AOC] Each dragon may have a different amount of levels, use an arbitrary initial value
	public float[] levelsXP { get { return m_levelsXp; }}
	public int numLevels { get { return m_levelsXp.Length; }}

	[Separator("Level-dependant stats")]
	[SerializeField] private Range m_healthRange = new Range(1, 100);
	public Range healthRange { get { return m_healthRange; }}
		
	[SerializeField] private Range m_scaleRange = new Range(0.5f, 1.5f);
	public Range scaleRange { get { return m_scaleRange; }}

	[Separator("Constant stats")]
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
	
	[SerializeField] private float m_furyDuration = 15f; // seconds
	public float furyDuration { get { return m_furyDuration; }}

	[Separator("Skills")]
	[SerializeField] private SkillData m_biteSkill = new SkillData();
	public SkillData biteSkill { get { return m_biteSkill; }}

	[SerializeField] private SkillData m_speedSkill = new SkillData();
	public SkillData speedSkill { get { return m_speedSkill; }}

	[SerializeField] private SkillData m_boostSkill = new SkillData();
	public SkillData boostSkill { get { return m_boostSkill; }}

	[SerializeField] private SkillData m_fireSkill = new SkillData();
	public SkillData fireSkill { get { return m_fireSkill; }}
}
