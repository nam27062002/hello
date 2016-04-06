// PopupTutorialControls.cs
// 
// Created by Alger Ortín Castellví on 05/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Pause popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupTutorialControls : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public static readonly string PATH = "UI/Popups/Tutorial/PF_PopupTutorialControls";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Open animation is about to start.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Pause the game
		GameSceneController gameController = InstanceManager.GetSceneController<GameSceneController>();
		if(gameController != null) {
			gameController.PauseGame(true);
		}
	}

	/// <summary>
	/// Close animation has finished.
	/// </summary>
	public void OnClosePostAnimation() {
		// Restore the game
		GameSceneController gameController = InstanceManager.GetSceneController<GameSceneController>();
		if(gameController != null) {
			gameController.PauseGame(false);
		}
	}
}
