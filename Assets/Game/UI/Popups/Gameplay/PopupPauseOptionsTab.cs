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
	/// Close animation has finished.
	/// </summary>
	public void OnClosePostAnimation() {
		// End the game?
		if(m_endGame) {
			GameSceneController gameController = InstanceManager.gameSceneController;
			if(gameController != null) {
				gameController.EndGame();
			}
		}
	}
}
