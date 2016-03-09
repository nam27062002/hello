// IncubatorScreenController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global controller for the Incubator screen in the main menu.
/// </summary>
public class IncubatorScreenController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Start open flow on the current egg in the incubator, if possible.
	/// </summary>
	/// <returns>Whether the opening process was started or not.</returns>
	public bool OpenCurrentEgg() {
		// Just in case, shouldn't happen anything if there is no egg incubating or it is not ready
		if(!EggManager.isReadyForCollection) return false;

		// Try to reuse current egg view attached to the incubator
		MenuScreensController screensController = InstanceManager.sceneController.GetComponent<MenuScreensController>();
		MenuScreenScene incubatorScene = screensController.GetScene((int)MenuScreens.INCUBATOR);
		IncubatorEggAnchor eggAnchor = incubatorScene.FindComponentRecursive<IncubatorEggAnchor>();
		EggController eggView = eggAnchor.attachedEgg;
		eggAnchor.DeattachEgg(false);	// Don't destroy it!

		// Go to OPEN_EGG screen and start open flow
		OpenEggScreenController openEggScreen = screensController.GetScreen((int)MenuScreens.OPEN_EGG).GetComponent<OpenEggScreenController>();
		openEggScreen.StartFlow(EggManager.incubatingEgg, eggView);
		screensController.GoToScreen((int)MenuScreens.OPEN_EGG);

		return true;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
}