// OrbitalCameraController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/07/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for a camera orbiting around a point.
/// </summary>
public class OrbitalCameraController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum MouseButton {
		NONE = -1,
		LEFT = 0,
		RIGHT = 1,
		MIDDLE = 2
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	public Transform m_lookAt = null;
	public float m_rotationSpeed = 0.5f;
	public float m_zoomSpeed = 0.5f;

	[Space]
	public MouseButton m_holdMouseButtonToRotate = MouseButton.NONE;

	// Internal
	private Vector3 m_mouseDelta = Vector3.zero;
	private Vector3 m_lastMouse = Vector3.zero;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Init internal vars
		m_lastMouse = Input.mousePosition;
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Only if active!
		if(!isActiveAndEnabled) return;

		// Track mouse offset
		m_mouseDelta = m_lastMouse - Input.mousePosition;
		m_lastMouse = Input.mousePosition;
	}

	/// <summary>
	/// Update performed at the end of the frame.
	/// </summary>
	private void LateUpdate() {
		// Only if active!
		if(!isActiveAndEnabled) return;

		// Change zoom?
		// If mouse wheel has been scrolled
		if(Input.mouseScrollDelta.sqrMagnitude > Mathf.Epsilon) {
			// Change value size based on mouse wheel
			float zoomOffset = Input.mouseScrollDelta.y * -m_zoomSpeed;

			// Do it!
			Vector3 dir = transform.position - m_lookAt.position;
			float newDist = dir.magnitude + zoomOffset;
			if(newDist > Mathf.Epsilon) {   // Don't go inside the object!
				transform.position = m_lookAt.position + dir.normalized * newDist;
			}
		}

		// Apply rotation?
		if(m_holdMouseButtonToRotate == MouseButton.NONE || Input.GetMouseButton((int)m_holdMouseButtonToRotate)) {
			if(m_mouseDelta.sqrMagnitude > Mathf.Epsilon) {
				transform.RotateAround(m_lookAt.position, Vector3.up, m_mouseDelta.x * -m_rotationSpeed);
				transform.RotateAround(m_lookAt.position, Vector3.right, m_mouseDelta.y * m_rotationSpeed);
			}
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}