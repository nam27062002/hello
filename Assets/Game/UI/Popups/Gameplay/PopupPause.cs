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
[RequireComponent(typeof(PopupController))]
public class PopupPause : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string PATH = "UI/Popups/Pause/PF_PopupPause";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private bool m_endGame = false;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Popup has been enabled.
	/// </summary>
	private void OnEnable() {
		PopupController popup = GetComponent<PopupController>();
		popup.OnClosePostAnimation.AddListener(OnClosePostAnimation);
	}

	//------------------------------------------------------------------//
	// DELEGATES														//
	//------------------------------------------------------------------//
	/// <summary>
	/// End game button has been pressed
	/// </summary>
	public void OnEndGameButton() {
		// Activate flag and close popup
		m_endGame = true;
		GetComponent<PopupController>().Close(true);
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
