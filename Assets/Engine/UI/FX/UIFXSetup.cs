// UIFXSetup.cs
// 
// Created by Alger Ortín Castellví on 24/07/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Customize basic parameters of a UI FX standard animation.
/// </summary>
[RequireComponent(typeof(Animator))]
public class UIFXSetup : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed members
	public float duration = 1f;
	public float delay = 0f;
	public bool loop = false;
	public bool loopDelay = false;

	// References
	private Animator animator = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		// Check required stuff
		animator = GetComponent<Animator>();
		DebugUtils.Assert(animator != null, "Required member!");
	}

	/// <summary>
	/// First update.
	/// </summary>
	public void Start() {
		// Just make sure that the animator has the overriden parameters
		animator.SetFloat("durationInverted", 1f/duration);
		animator.SetFloat("delayInverted", 1f/delay);
		animator.SetBool("loop", loop);
		animator.SetBool("loopDelay", loopDelay);
	}
}
