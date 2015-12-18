// LevelData.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

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
/// Definition of a game level.
/// </summary>
[Serializable]
public class LevelData {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	[SerializeField] private string m_tidName = "";
	public string tidName { get { return m_tidName; }}

	[SerializeField] private string m_tidDescription = "";
	public string tidDescription { get { return m_tidDescription; }}

	[SerializeField] private string m_sceneName = "";
	public string sceneName { get { return m_sceneName; }}

	[SerializeField] private int m_dragonsToUnlock = 0;
	public int dragonsToUnlock { get { return m_dragonsToUnlock; }}
	public bool isUnlocked { get { return m_dragonsToUnlock <= DragonManager.GetDragonsByLockState(DragonData.LockState.OWNED).Count; }}
}
