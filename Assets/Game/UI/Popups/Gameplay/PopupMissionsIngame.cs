// PopupMissionsIngame.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

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
/// Temp popup to show active missions during the game.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupMissionsIngame : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string PATH = "UI/Popups/Menu/PF_PopupMissionsIngame";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled
	/// </summary>
	private void OnEnable() {
		// Add delegates
		PopupController popup = GetComponent<PopupController>();
		popup.OnClosePostAnimation += OnClosePostAnimation;
	}

	/// <summary>
	/// Component has been disabled
	/// </summary>
	private void OnDisable() {
		// Add delegates
		PopupController popup = GetComponent<PopupController>();
		popup.OnClosePostAnimation -= OnClosePostAnimation;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Close animation finished.
	/// </summary>
	private void OnClosePostAnimation() {
		// Keep playing - only if we're in the game scene
		GameSceneController gameScene = InstanceManager.GetSceneController<GameSceneController>();
		if(gameScene != null) {
			gameScene.PauseGame(false);
		}
	}
}
