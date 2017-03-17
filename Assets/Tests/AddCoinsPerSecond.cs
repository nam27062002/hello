// AddCoinsPerSecond.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple test script to periodically add coins to the user profile in order to test persistence, etc.
/// </summary>
public class AddCoinsPerSecond : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public Range m_coinsRange = new Range(1f, 10f);		// How many coins?
	public Range m_intervalRange = new Range(0.5f, 5f);	// How often?
	private float m_timer = -1f;
	private GameSceneController m_scene = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {

	}

	/// <summary>
	/// First update call.
	/// </summary>
	void Start() {
		// Get external references
		m_scene = InstanceManager.gameSceneController;
		DebugUtils.Assert(m_scene != null, "Required component!");

		// Initialize the timer
		m_timer = m_intervalRange.GetRandom();
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {
		// Only do it while the game is running
		if(m_scene.state == GameSceneController.EStates.RUNNING) {
			m_timer -= Time.deltaTime;
			if(m_timer <= 0) {
				// Add a random amount of coins
				UsersManager.currentUser.AddCoins((long)m_coinsRange.GetRandom());
				
				// Reset timer
				m_timer = m_intervalRange.GetRandom();
			}
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	void OnDestroy() {

	}
}