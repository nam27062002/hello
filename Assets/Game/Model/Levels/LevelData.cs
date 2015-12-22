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
	[Separator("General")]
	[SerializeField] private string m_tidName = "";
	public string tidName { get { return m_tidName; }}

	[SerializeField] private string m_tidDescription = "";
	public string tidDescription { get { return m_tidDescription; }}

	[SerializeField] private int m_dragonsToUnlock = 0;
	public int dragonsToUnlock { get { return m_dragonsToUnlock; }}
	public bool isUnlocked { get { return m_dragonsToUnlock <= DragonManager.GetDragonsByLockState(DragonData.LockState.OWNED).Count; }}

	[Separator("Scenes")]
	[FileList("Resources/Game/Levels/Spawners", StringUtils.PathFormat.FILENAME_WITHOUT_EXTENSION, "*.unity")]
	[SerializeField] private string m_spawnersScene = "";
	public string spawnersScene { get { return m_spawnersScene; }}

	[FileList("Resources/Game/Levels/Collision", StringUtils.PathFormat.FILENAME_WITHOUT_EXTENSION, "*.unity")]
	[SerializeField] private string m_collisionScene = "";
	public string collisionScene { get { return m_collisionScene; }}

	[FileList("Resources/Game/Levels/Art", StringUtils.PathFormat.FILENAME_WITHOUT_EXTENSION, "*.unity")]
	[SerializeField] private string m_artScene = "";
	public string artScene { get { return m_artScene; }}
}
