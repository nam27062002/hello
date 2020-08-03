// Gravity.cs
// ArmyTapper
// 
// Created by Alger Ortín Castellví on 23/06/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple script to make gravity affect a rigidbody.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Gravity : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public float gravityMultiplier = 1f;
	private Rigidbody body = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		// Get references
		body = GetComponent<Rigidbody>();
		DebugUtils.Assert(body != null, "Required component!!");
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	void FixedUpdate() {
		// Just apply gravity force
		body.AddForce(Physics.gravity * body.mass * gravityMultiplier, ForceMode.Acceleration);
	}
}
