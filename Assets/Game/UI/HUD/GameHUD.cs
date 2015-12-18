// GameHUD.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Root controller for the in-game HUD prefab.
/// </summary>
public class GameHUD : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Callback for the pause button.
	/// </summary>
	public void OnPauseButton() {
		// Instantly finish the game for now
		InstanceManager.GetSceneController<GameSceneController>().EndGame();

		/*
		GameSceneController scene = InstanceManager.GetSceneController<GameSceneController>();
		scene.PauseGame(!scene.paused);
		*/
	}

	/// <summary>
	/// Callback for the missions button.
	/// </summary>
	public void OnMissionsButton() {
		// Pause game
		InstanceManager.GetSceneController<GameSceneController>().PauseGame(true);

		// Open missions popup
		PopupManager.OpenPopupInstant(PopupMissionsIngame.PATH);
	}
}
