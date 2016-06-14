// LavaFXController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/06/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for the lava FX in the UI
/// </summary>
public class LavaFXController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private Range m_delayRandomInterval = new Range(2f, 5f);

	// Internal
	private Animator m_anim = null;
	private float m_timer = 0f;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_anim = GetComponent<Animator>();
		Debug.Assert(m_anim != null, "Required component missing!");
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Launch an initial delay
		m_timer = m_delayRandomInterval.GetRandom();
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Update timer
		if(m_timer > 0f) {
			m_timer -= Time.deltaTime;
			if(m_timer <= 0f) {
				// Timer finished! Launch animation and program next one
				m_anim.SetTrigger("start");

				// Program next anim trigger
				// Take in account anim's duration
				float animLength = m_anim.GetCurrentAnimatorStateInfo(0).length;
				m_timer = animLength + m_delayRandomInterval.GetRandom();
			}
		}
	}
}