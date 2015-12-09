// MissionDef.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Definition of a mission.
/// Temporary stuff until missions are procedurally generated.
/// </summary>
[System.Serializable]
public class MissionDef : Definition {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	[Header("Setup")]
	[SerializeField] [HideEnumValues(false, true)] private Mission.Difficulty m_difficulty;
	public Mission.Difficulty difficulty { get { return m_difficulty; }}

	[Header("Objective")]
	[SerializeField] private MissionObjective.Type m_type;
	public MissionObjective.Type type { get { return m_type; }}

	[SerializeField] private int m_targetValue = 1;
	public int targetValue { get { return m_targetValue; }}

	[SerializeField] private string[] m_parameters;
	public string[] parameters { get { return m_parameters; }}

	[SerializeField] private bool m_singleRun = true;
	public bool singleRun { get { return m_singleRun; }}

	// [AOC] TEMP!! This will be replaced by a formula by Hadrian
	[SerializeField] private int m_rewardCoins = 1;
	public int rewardCoins { get { return m_rewardCoins; }}

	[Header("Info")]
	[SerializeField] private string m_tidName = "";
	public string tidName { get { return m_tidName; }}

	[SerializeField] private string m_tidDesc = "";
	public string tidDesc { get { return m_tidDesc; }}
}