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
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller to update a textfield with a game countdown.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(Animator))]
public class HUDCountdown : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	private TextMeshProUGUI m_text = null;
	private Animator m_anim = null;
	private GameSceneController m_scene = null;

	// Internal logic
	private int m_lastValue = 0;
	private bool m_animFinished = true;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Get required references
		m_text = GetComponent<TextMeshProUGUI>();
		DebugUtils.Assert(m_text != null, "Required component!");

		m_anim = GetComponent<Animator>();
		DebugUtils.Assert(m_anim != null, "Required component!");

		m_scene = InstanceManager.gameSceneController;
		DebugUtils.Assert(m_scene != null, "Game scene controller could not be found!");

		// Start hidden
		m_text.enabled = false;
		m_animFinished = true;
	}
	
	/// <summary>
	/// Called every frame after the animators are updated.
	/// </summary>
	private void LateUpdate() {
		// Only while countdown is active and an extra second afterwards
		if(m_lastValue >= 0 || m_scene.state == GameSceneController.EStates.COUNTDOWN) {
			if(m_animFinished) {
				// Has the value changed?
				int value = Mathf.CeilToInt(m_scene.countdown);
				if(value != m_lastValue) {
					// Yes!! Update text and reset anim
					// [AOC] Special case for 0 value
					if(value == 0) {
						m_text.text = "GO!";	// [AOC] HARDCODED!!
					} else {
						m_text.text = StringUtils.FormatNumber(value);
					}

					m_anim.SetTrigger("start");
					m_animFinished = false;
					m_lastValue = value;
				}
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
		m_animFinished = true;

		// If we've reached 0, stop counting
		if (m_lastValue == 0) {
			gameObject.SetActive(false);
		}
	}
}

