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
using TMPro;
using System.Collections;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Generic controller for any feedback to be placed in relation to the 3D world.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class WorldFeedbackController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private float m_delay = 0f;

	// Required components
	private Animator m_anim = null;
	private TextMeshProUGUI m_text = null;
	private RectTransform m_rectTransform = null;

	// Internal vars
	private Vector3 m_targetWorldPos = Vector3.zero;
	private float m_delayTimer = -1f;
	private Camera m_camera = null;

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
		m_text = GetComponent<TextMeshProUGUI>();
		m_rectTransform = transform as RectTransform;
		DebugUtils.Assert(m_anim != null, "Required Component!!", this);
		DebugUtils.Assert(m_text != null, "Required Component!!", this);
	}

	/// <summary>
	/// Called once per frame.
	/// </summary>
	private void LateUpdate() {
		// Use late update to give time to the animation to apply the position
		// Wait until delay is over
		if(m_delayTimer > 0f) {
			// Update timer
			m_delayTimer -= Time.deltaTime;
			if(m_delayTimer <= 0f) {
				// Timer finished, move to initial position and start animation
				ApplyPosOffset();
				m_anim.SetTrigger( GameConstants.Animator.START );

				// Show on top
				transform.SetAsLastSibling();
			}
		} else {
			ApplyPosOffset();
		}
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Configures the feedback with the given data, but doesn't launch any animation.
	/// </summary>
	/// <param name="_text">The text to be displayed.</param>
	/// <param name="_worldPos">The reference world position to follow.</param>
	public void Init(string _text, Vector3 _worldPos) {
		// Init text
		m_text.text = _text;

		// Call base initializer
		Init(_worldPos);
	}

	/// <summary>
	/// Configures the feedback with the given data, but doesn't launch any animation.
	/// </summary>
	/// <param name="_worldPos">The reference world position to follow.</param>
	public void Init(Vector3 _worldPos) {
		// Store target world pos
		m_targetWorldPos = _worldPos;

		// Put it off-screen
		m_rectTransform.anchorMin = Vector2.one * -1f;	// This should keep it off-screen
		m_rectTransform.anchorMax = Vector2.one * -1f;

		// Disable until Spawn() method is called
		gameObject.SetActive(false);
	}

	/// <summary>
	/// Start the animation using previously set data (via the Init() methods).
	/// </summary>
	public void Spawn() {
        if (gameObject != null) {
            // Activate object (so update is called), but keep it off-screen until delay is over (Init() methods already did it)
            gameObject.SetActive(true);
        }

		// Reset delay timer
		m_delayTimer = m_delay;
	}

	/// <summary>
	/// Make sure the feedback is positioned relative to the reference world position.
	/// </summary>
	private void ApplyPosOffset() {
		if (m_camera == null) {
			m_camera = InstanceManager.sceneController.mainCamera;
		}

		// Animation has already applied its position to the number based on (0,0)
		// Apply 3D projection offset
		// From http://answers.unity3d.com/questions/799616/unity-46-beta-19-how-to-convert-from-world-space-t.html
		// We can do it that easily because we've adjusted the containers to match the camera viewport coords
		// Only if a world coordinate to follow was given
		Vector2 posScreen = Vector2.one/2f;	// Center of the screen
		if(m_camera != null && m_targetWorldPos != Vector3.zero) {
			posScreen = m_camera.WorldToViewportPoint(m_targetWorldPos);
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