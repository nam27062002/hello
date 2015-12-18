// CPCameraFOV.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Several debug options to control the game camera.
/// </summary>
public class CPCameraFOV : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private Camera m_cam = null;
	private Slider m_slider = null;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		m_slider = GetComponent<Slider>();
	}

	/// <summary>
	/// Component was enabled.
	/// </summary>
	void OnEnable() {
		// If camera ref wasn't initialized, try to do it now
		if(m_cam == null) OnLevelWasLoaded();
	}

	/// <summary>
	/// Sets the FOV of the camera.
	/// </summary>
	/// <param name="_value">The new fov of the camera.</param>
	public void SetFOV(float _value) {
		if(m_cam != null) {
			m_cam.fieldOfView = _value;
		}
	}

	/// <summary>
	/// A new scene has been loaded.
	/// </summary>
	private void OnLevelWasLoaded() {
		// Reset camera references to the camera in the new scene
		// We want the camera attached to the game camera controller
		m_cam = null;
		GameCameraController camController = GameObject.FindObjectOfType<GameCameraController>();
		if(camController != null) {
			m_cam = camController.GetComponent<Camera>();
		}

		// Initialize slider with new scene's camera values
		m_slider.interactable = (m_cam != null);
		if(m_cam != null) {
			m_slider.value = m_cam.fieldOfView;	// Initial value
		}
	}
}
