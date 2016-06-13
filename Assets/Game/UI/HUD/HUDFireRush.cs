// HUDFireRush.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/01/2015.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller for the fire rush feedback in the game HUD.
/// </summary>
public class HUDFireRush : MonoBehaviour {
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

	/// <summary>
	/// The spawner has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryRushToggled);
	}
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryRushToggled);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Fury rush has been toggled.
	/// </summary>
	/// <param name="_active">Whether the fury rush has been activated or not.</param>
	private void OnFuryRushToggled(bool _active, DragonBreathBehaviour.Type _type) {
		// Just launch the text animation when activated
		if(_active) {
			DOTween.Restart("fireRushIn");
		}
	}

	public void TEST_FIRE_RUSH_FX() {
		DOTween.Restart("fireRushIn");
	}
}
