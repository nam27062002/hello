// CameraShake.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/09/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple component to make a camera shake.
/// Attach it to a camera and broadcast the GameEvents.CAMERA_SHAKE event.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraShake : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private Camera m_camera = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_camera = GetComponent<Camera>();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<float, float>(GameEvents.CAMERA_SHAKE, OnCameraShake);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe to external events
		Messenger.RemoveListener<float, float>(GameEvents.CAMERA_SHAKE, OnCameraShake);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Shake for a given duration and intensity.
	/// </summary>
	/// <param name="_duration">Duration of the shake effect in seconds.</param>
	/// <param name="_intensity">Intensity of the shake effect.</param>
	public void Shake(float _duration, float _intensity) {
		// Kill any existing camera shake tween on this camera
		DOTween.Kill(this.name + ".CameraShake", true);
		m_camera.DOShakePosition(_duration, _intensity).SetRecyclable(true).SetId(this.name + ".CameraShake");
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A canera shake has been requested.
	/// </summary>
	/// <param name="_duration">Duration of the shake effect in seconds.</param>
	/// <param name="_intensity">Intensity of the shake effect.</param>
	private void OnCameraShake(float _duration, float _intensity) {
		// Just do it
		Shake(_duration, _intensity);
	}
}