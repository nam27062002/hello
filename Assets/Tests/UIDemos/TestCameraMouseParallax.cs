// TestCameraMouseParallax.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(Camera))]
public class TestCameraMouseParallax : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private Camera m_camera = null;
	private Quaternion m_initialRotation = Quaternion.identity;
	public float m_offset = 10f;	// Degrees offset
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_camera = GetComponent<Camera>();
		m_initialRotation = m_camera.transform.rotation;
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

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
	/// Called every frame
	/// </summary>
	private void Update() {
		// Compute offset delta with mouse position relative to the viewport
		Vector2 mouseOffset = new Vector2(
			Mathf.Clamp01(Input.mousePosition.x/Screen.width), 
			Mathf.Clamp01(Input.mousePosition.y/Screen.height)
		);

		// Translate offset towards center [0..1] -> [-1..1]
		mouseOffset = (mouseOffset * 2f) - Vector2.one;

		// Compute rotation offset
		Quaternion rotationOffset = Quaternion.Euler(-mouseOffset.y * m_offset, mouseOffset.x * m_offset, 0f);
		m_camera.transform.rotation = m_initialRotation * rotationOffset;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
}