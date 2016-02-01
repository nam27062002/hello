// MenuScreensController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple class to add some specific behaviour to the main menu screen navigator.
/// </summary>
[RequireComponent(typeof(NavigationScreenSystem))]
public class MenuScreensController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Screens {
		NONE = -1,
		PLAY,
		DRAGON_SELECTION,
		LEVEL_SELECTION
	};
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Just define initial screen on the navigation system before it actually starts
		if(GameVars.playScreenShown) {
			GetComponent<NavigationScreenSystem>().SetInitialScreen((int)Screens.DRAGON_SELECTION);
		} else {
			GetComponent<NavigationScreenSystem>().SetInitialScreen((int)Screens.PLAY);
		}
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
}