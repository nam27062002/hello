// HUDCountdown.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/08/2015.
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
/// Simple controller to update a textfield with a game countdown.
/// </summary>
[RequireComponent(typeof(Text))]
[RequireComponent(typeof(Animator))]
public class HUDCountdown : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	private Text m_text = null;
	private Animator m_anim = null;
	private GameSceneController m_scene = null;

	// Internal logic
	private int m_lastValue = 0;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Get required references
		m_text = GetComponent<Text>();
		DebugUtils.Assert(m_text != null, "Required component!");

		m_anim = GetComponent<Animator>();
		DebugUtils.Assert(m_anim != null, "Required component!");

		m_scene = InstanceManager.sceneController as GameSceneController;
		DebugUtils.Assert(m_scene != null, "Game scene controller could not be found!");

		// Start hidden
		m_text.enabled = false;
	}
	
	/// <summary>
	/// Called every frame after the animators are updated.
	/// </summary>
	private void LateUpdate() {
		// Only while countdown is active and an extra second afterwards
		if(m_lastValue >= 0 || m_scene.state == GameSceneController.EStates.COUNTDOWN) {
			// Has the value changed?
			int value = Mathf.CeilToInt(m_scene.countdown);
			if(value != m_lastValue) {
				// Yes!! Update text and reset anim
				// [AOC] Special case for 0 value
				if(value == 0) {
					m_text.text = "GO!";	// [AOC] HARDCODED!!
					m_anim.SetBool("exit", true);
				} else {
					m_text.text = StringUtils.FormatNumber(value);
					m_anim.SetBool("exit", false);
				}
				m_anim.SetTrigger("start");
				m_lastValue = value;
			}
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The animation has finished.
	/// </summary>
	public void OnAnimationFinished() {
		// Disable textfield - will be reenabled when launching the animation again
		m_text.enabled = false;

		// If we've reached 0, stop counting
		if(m_lastValue == 0) m_lastValue = -1;
	}
}

