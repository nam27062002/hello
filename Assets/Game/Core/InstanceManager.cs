// InstanceManager.cs
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
		set { instance.m_sceneController = value; }
	}

	// Only during game scene, reference to the dragon
	public static DragonPlayer player {
		get {
			// Only in game scene!!
			DebugUtils.Assert(SceneManager.currentScene == GameSceneController.NAME, "Attemting to retrieve the player outside the game scene!!");
			return (sceneController as GameSceneController).player;
		}
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

		// Call parent
		base.OnDestroy();
	}
}

