﻿// InstanceManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Static access point to non-static objects. All of the objects may be null, so use it carefully.
/// Singleton class, work with it via its static methods only.
/// <see cref="https://youtu.be/64uOVmQ5R1k?t=20m16s"/>
/// </summary>
public class InstanceManager : Singleton<InstanceManager> {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Current scene controller, cast it to the right one for the active scene
	private SceneController m_sceneController = null;
	public static SceneController sceneController { 
		get { return instance.m_sceneController; }
		set { if(instance != null) instance.m_sceneController = value; }
	}

	// Only during game scene, reference to the dragon
	private DragonPlayer m_player = null;
	public static DragonPlayer player {
		get {
			DebugUtils.Assert(instance.m_player != null, "Attempting to retrieve the player, but no player has been created yet.");
			return instance.m_player;
		}
		set { if(instance != null) instance.m_player = value; }
	}

	// Main pool of game objects, we use it to store all the objects that will be destroyed and spawned several times 
	private PoolController m_poolController = null;
	public static PoolController pools { 
		get { return instance.m_poolController; }
		set { if(instance != null) instance.m_poolController = value; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
		// Clear all references
		m_sceneController = null;
		m_player = null;

		// Call parent
		base.OnDestroy();
	}
}

