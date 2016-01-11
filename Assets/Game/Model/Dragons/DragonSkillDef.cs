// DragonSkillDef.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/12/2015.
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
/// Definition of a dragon skill.
/// </summary>
[Serializable]
public class DragonSkillDef : Definition {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// All skills have the same amount of levels
	public static readonly int NUM_LEVELS = 5;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	[Separator("Info")]
	[SerializeField] private string m_tidName = "";
	public string tidName { get { return m_tidName; }}

	[SerializeField] private string m_tidDescription = "";
	public string tidDescription { get { return m_tidDescription; }}

	[SerializeField] private Sprite m_icon = null;
	public Sprite icon { get { return m_icon; }}
}
