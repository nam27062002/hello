// SceneController.cs
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
/// Base class for all scene controllers.
/// Each scene should have an object containing one of these, usually a custom
/// implementation of this class.
/// </summary>
public class SceneController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	[InfoBox("Mark it only if the scene doesn't have any dependence with previous scenes.")]
	[SerializeField] private bool m_standaloneScene = false;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// If it's the first scene being loaded and it can't run standalone, restart the game flow
		if((SceneManager.prevScene == "") && !m_standaloneScene) {
			FlowManager.Restart();
		}

		// Register ourselves to the instance manager
		InstanceManager.sceneController = this;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected virtual void OnDestroy() {
		// Unregister ourselves from the instance manager
		// If the instance manager was not created (or already destroyed) we would 
		// be creating a new object while destroying current scene, which is quite 
		// problematic for Unity.
		if(InstanceManager.isInstanceCreated) InstanceManager.sceneController = null;
	}
}

