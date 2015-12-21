// FlowManager.cs
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
public class FlowManager : SingletonMonoBehaviour<FlowManager> {
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

	//------------------------------------------------------------------//
	// SCENE NAVIGATION													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Navigate to the menu scene.
	/// </summary>
	public static void GoToMenu() {
		// Skip if next scene is already menu
		if(GameSceneManager.nextScene == MenuSceneController.NAME) return;

		// Change scene
		GameSceneManager.SwitchScene(MenuSceneController.NAME);
	}

	/// <summary>
	/// Navigate to the game scene.
	/// </summary>
	public static void GoToGame() {
		// Skip if next scene is already game
		if(GameSceneManager.nextScene == GameSceneController.NAME) return;

		// Change scene
		GameSceneManager.SwitchScene(GameSceneController.NAME);
	}

	/// <summary>
	/// Interrupts current flow and restarts the application.
	/// </summary>
	public static void Restart() {
		// Delete key singletons that must be reloaded
		DragonManager.DestroyInstance();
		UserProfile.DestroyInstance();
		InstanceManager.DestroyInstance();
		PoolManager.Clear(true);
		ParticleManager.Clear();

		// Change to the loading scene
		GameSceneManager.SwitchScene(LoadingSceneController.NAME);
	}
}

