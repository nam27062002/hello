// SimulatorSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Main controller for the simulator scene.
/// </summary>
public class SimulatorSceneController : SceneController {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string NAME = "SC_Simulator";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//


	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Call parent
		base.Awake();
	}

	/// <summary>
	/// First update.
	/// </summary>
	void Start() {

	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
		// Call parent
		base.OnDestroy();
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Play button has been pressed.
	/// </summary>
	public void OnPlayButton() {
		// Go to game!
		// [AOC] No need to block the button, the GameFlow already controls spamming
		FlowManager.GoToGame();
	}

	/// <summary>
	/// Reset button has been pressed.
	/// </summary>
	public void OnResetButton() {
		// Clear persistence
		PersistenceManager.Clear();

		// Restart game
		FlowManager.Restart();
	}
}

