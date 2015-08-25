// GameFlow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Class responsible for making all the transitions in the game, such as go to the menu, start a game, etc.
/// This transitions may involve loading a new scene, sending some metrics, communication with the server to let it know
/// that the user is changing its state, etc.
/// Singleton class, work with it via its static methods only.
/// <see cref="https://youtu.be/64uOVmQ5R1k?t=20m16s"/>
/// </summary>
public class GameFlow : Singleton<GameFlow> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

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
	/// Called every frame.
	/// </summary>
	override protected void Update() {
		// Call parent
		base.Update();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
		// Call parent
		base.OnDestroy();
	}

	//------------------------------------------------------------------//
	// SCENE NAVIGATION													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Navigate to the menu scene.
	/// </summary>
	public static void GoToMenu() {
		// Skip if next scene is already game
		if(SceneManager.nextScene == MenuSceneController.NAME) return;

		// Change scene
		SceneManager.SwitchScene(MenuSceneController.NAME);
	}

	/// <summary>
	/// Navigate to the game scene.
	/// </summary>
	public static void GoToGame() {
		// Skip if next scene is already game
		if(SceneManager.nextScene == GameSceneController.NAME) return;

		// Change scene
		SceneManager.SwitchScene(GameSceneController.NAME);
	}
}

