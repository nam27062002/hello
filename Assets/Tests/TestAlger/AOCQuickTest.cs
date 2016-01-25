// AOCQuickTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class AOCQuickTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private float m_multiplier = 1f;
	private float m_timer = 0f;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

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
		
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {
		if(m_timer > 0f) {
			m_timer -= Time.deltaTime;
			if(m_timer <= 0f) {
				ChangeMultiplier(1f, false);
			}
		}
	}

	/// <summary>
	/// Changes the multiplier.
	/// </summary>
	/// <param name="_newMultiplier">New multiplier.</param>
	/// <param name="_resetTimer">If set to <c>true</c> reset timer.</param>
	public void ChangeMultiplier(float _newMultiplier, bool _resetTimer = true) {
		// Check params
		if(_newMultiplier == m_multiplier) return;

		// Do it
		ScoreMultiplier oldMultiplier = new ScoreMultiplier();
		oldMultiplier.multiplier = m_multiplier;

		m_multiplier = _newMultiplier;

		ScoreMultiplier newMultiplier = new ScoreMultiplier();
		newMultiplier.multiplier = m_multiplier;

		// Reset timer
		if(_resetTimer) {
			m_timer = 5f;
		}

		// Simulate event
		Messenger.Broadcast<ScoreMultiplier, ScoreMultiplier>(GameEvents.SCORE_MULTIPLIER_CHANGED, oldMultiplier, newMultiplier);

	}

	public void OnClick() {
		ChangeMultiplier(m_multiplier * 2f);
	}
}