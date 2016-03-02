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
		// [AOC] TODO!! Go to menu "open egg" screen
		MenuScreensController screensController = InstanceManager.sceneController.GetComponent<MenuScreensController>();
		//screensController.GoToScreen(MenuScreensController.Screens.OPEN_EGG);

		// Directly collect the egg for now
		EggManager.incubatingEgg.Collect();
		PersistenceManager.Save();

		// Hide! - we may need to confirm that egg was collected, but in theory the flow can't be interrupted
		m_anim.Hide();
	}
}

