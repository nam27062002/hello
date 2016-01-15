// HUDChestFound.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/01/2016.
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
/// Simple controller for a mission completed feedback in the hud.
/// </summary>
[RequireComponent(typeof(Animator))]
public class HUDChestFound : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private Animator m_anim;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_anim = GetComponent<Animator>();
	}

	/// <summary>
	/// The spawner has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<Chest>(GameEvents.CHEST_COLLECTED, OnChestCollected);
	}
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Chest>(GameEvents.CHEST_COLLECTED, OnChestCollected);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A mission has been completed.
	/// </summary>
	/// <param name="_chest">The chest that has been collected.</param>
	private void OnChestCollected(Chest _chest) {
		// Just play the anim!
		m_anim.SetTrigger("start");
	}
}
