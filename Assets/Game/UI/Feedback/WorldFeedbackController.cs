// WorldFeedbackController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/10/2015.
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
/// Generic controller for any feedback to be placed in relation to the 3D world.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Text))]
public class WorldFeedbackController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Required components
	private Animator m_anim = null;
	private Text m_text = null;
	private RectTransform m_rectTransform = null;

	// Internal vars
	private Vector3 m_targetWorldPos = Vector3.zero;

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
		m_anim = GetComponent<Animator>();
		m_text = GetComponent<Text>();
		m_rectTransform = transform as RectTransform;
		DebugUtils.Assert(m_anim != null, "Required Component!!");
		DebugUtils.Assert(m_text != null, "Required Component!!");
	}

	/// <summary>
	/// Called once per frame.
	/// </summary>
	private void LateUpdate() {
		// Use late update to give time to the animation to apply the position
		ApplyPosOffset();
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Start the animation of this feedback with the given data.
	/// </summary>
	/// <param name="_text">The text to be displayed.</param>
	/// <param name="_worldPos">The reference world position to follow.</param>
	public void Spawn(string _text, Vector3 _worldPos) {
		// Store params
		m_targetWorldPos = _worldPos;

		// Init text
		m_text.text = _text;

		// Move to initial position, activate object and start animation
		ApplyPosOffset();
		gameObject.SetActive(true);
		m_anim.SetTrigger("start");
	}


	/// <summary>
	/// Make sure the feedback is positioned relative to the reference world position.
	/// </summary>
	private void ApplyPosOffset() {
		// Animation has already applied its position to the number based on (0,0)
		// Apply 3D projection offset
		// From http://answers.unity3d.com/questions/799616/unity-46-beta-19-how-to-convert-from-world-space-t.html
		// We can do it that easily because we've adjusted the containers to match the camera viewport coords
		// Only if a world coordinate to follow was given
		Vector2 posScreen = Vector2.one/2f;	// Center of the screen
		if(m_targetWorldPos != Vector3.zero) {
			posScreen = Camera.main.WorldToViewportPoint(m_targetWorldPos);
		}
		m_rectTransform.anchorMin = posScreen;
		m_rectTransform.anchorMax = posScreen;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Animation callback to be called once the animation has finished.
	/// </summary>
	private void OnAnimFinished() {
		gameObject.SetActive(false);
	}
}