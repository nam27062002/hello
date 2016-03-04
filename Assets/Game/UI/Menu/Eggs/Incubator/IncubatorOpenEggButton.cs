// IncubatorTimerButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

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
/// Controls the "Open Egg" button in the incubator menu.
/// </summary>
[RequireComponent(typeof(ShowHideAnimator))]
public class IncubatorOpenEggButton : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Scene references
	private ShowHideAnimator m_anim = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external assets
		m_anim = GetComponent<ShowHideAnimator>();
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Subscribe to external events
		Messenger.AddListener<Egg>(GameEvents.EGG_INCUBATION_ENDED, OnEggIncubationEnded);
		Messenger.AddListener(GameEvents.EGG_INCUBATOR_CLEARED, OnIncubatorCleared);

		// Setup initial visibility
		m_anim.Set(EggManager.isReadyForCollection, false);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Egg>(GameEvents.EGG_INCUBATION_ENDED, OnEggIncubationEnded);
		Messenger.RemoveListener(GameEvents.EGG_INCUBATOR_CLEARED, OnIncubatorCleared);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// An egg has finished its incubation timer and is ready for collection.
	/// </summary>
	/// <param name="_egg">The egg.</param>
	private void OnEggIncubationEnded(Egg _egg) {
		// Show the button
		m_anim.Show();
	}

	/// <summary>
	/// The egg on the incubator has been collected.
	/// </summary>
	private void OnIncubatorCleared() {
		// Hide the button
		m_anim.Hide();
	}

	/// <summary>
	/// The button has been pressed.
	/// </summary>
	public void OnOpenButton() {
		// Just in case, shouldn't happen anything if there is no egg incubating or it is not ready
		if(!EggManager.isReadyForCollection) return;

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

		// Hide! - we may need to confirm that egg was collected, but in theory the flow can't be interrupted
		m_anim.Hide();
	}
}

