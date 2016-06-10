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
	[SerializeField] private Range m_delayRandomInterval = new Range(0f, 1f);

	// Internal
	private Animator m_anim = null;
	
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
		StartCoroutine(TriggerAfterDelay(m_delayRandomInterval.GetRandom(), "start"));
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Enables this game object after a given delay.
	/// </summary>
	/// <returns>Coroutine standard return.</returns>
	/// <param name="_delay">The delay, in seconds.</param>
	/// <param name="_trigger">The trigger to be sent to the animator.</param>
	private IEnumerator TriggerAfterDelay(float _delay, string _trigger) {
		// Wait for delay
		yield return new WaitForSeconds(_delay);

		// Launch animation and program next one
		if(m_anim != null) {
			// Launch trigger
			m_anim.SetTrigger(_trigger);

			// Program next anim
			// Take in account anim's duration
			float animLength = m_anim.GetCurrentAnimatorStateInfo(0).length;
			Debug.Log(animLength);
			StartCoroutine(TriggerAfterDelay(animLength + m_delayRandomInterval.GetRandom(), _trigger));
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}