// PopupPause.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Pause popup.
/// </summary>
public class PopupPauseOptionsTab : MonoBehaviour {

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private bool m_endGame = false;
	private Tab m_tabScreen;

	//------------------------------------------------------------------//
	// DELEGATES														//
	//------------------------------------------------------------------//
	/// <summary>
	/// End game button has been pressed
	/// </summary>
	public void OnEndGameButton() {
		// Activate flag and close popup
		m_endGame = true;
		GetComponentInParent<PopupController>().Close(true);
	}

	/// <summary>
	/// Open animation is about to start.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Pause the game
		GameSceneController gameController = InstanceManager.GetSceneController<GameSceneController>();
		if(gameController != null) {
			//gameController.PauseGame(true);
		}
	}

	/// <summary>
	/// Close animation has finished.
	/// </summary>
	public void OnClosePostAnimation() {
		// Keep playing or end the game?
		GameSceneController gameScene = InstanceManager.GetSceneController<GameSceneController>();
		if(gameScene != null) {
			if(m_endGame) {
				// Instantly finish the game for now
				InstanceManager.GetSceneController<GameSceneController>().EndGame();
			} else {
				// Keep playing
				gameScene.PauseGame(false);
			}
		}
	}
}
