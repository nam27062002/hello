// CameraSnapPoint.cs
// 
// Created by Alger Ortín Castellví on 29/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Define a camera setup.
/// </summary>
[RequireComponent(typeof(LookAt))]
public class CameraSnapPoint : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private const string TWEEN_ID = "CameraSnapPointTween";
	private const string DARK_SCREEN_PREFAB_PATH = "UI/Common/PF_CameraDarkScreen";
	private const string DARK_SCREEN_NAME = "PF_CameraDarkScreen";

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed parameters
	// Position/Rotation toggles
	public bool changePosition = true;
	public bool changeRotation = true;

	// Optional Parameters
	public bool changeFov = true;
	public float fov = 60;

	public bool changeNear = true;
	public float near = 0.1f;

	public bool changeFar = true;
	public float far = 1000f;

	// Optional fog setup
	public bool changeFogColor = true;
	public Color fogColor = Colors.silver;

	public bool changeFogStart = true;
	public float fogStart = 20f;

	public bool changeFogEnd = true;
	public float fogEnd = 100f;

	// Optional screen darkening setup
	public bool darkenScreen = false;
	public float darkScreenDistance = 50f;
	public Color darkScreenColor = Colors.WithAlpha(Color.black, 0.8f);

	// Editor Settings
	public bool livePreview = true;
	public bool drawGizmos = true;
	public Color gizmoColor = new Color(0f, 1f, 1f, 0.25f);

	// Internal references
	private LookAt m_lookAt = null;
	public LookAt lookAtData {
		get {
			if(m_lookAt == null) {
				m_lookAt = GetComponent<LookAt>();
			}
			return m_lookAt;
		}
	}

	//------------------------------------------------------------------//
	// STATIC MEMBERS													//
	//------------------------------------------------------------------//
	// A single dark screen shared among all cameras/snap points
	private static SpriteRenderer m_darkScreen = null;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Get references.
	/// </summary>
	void Awake() {
		
	}

	/// <summary>
	/// Draw scene gizmos.
	/// </summary>
	private void OnDrawGizmos() {
		// Ignore if gizmos disabled
		if(!drawGizmos) return;

		// LookAt line
		Gizmos.color = Colors.WithAlpha(Colors.cyan, 0.75f);
		Gizmos.DrawLine(lookAtData.transform.position, lookAtData.lookAtPointGlobal);

		// Position and lookAt points
		Gizmos.color = Colors.WithAlpha(Colors.red, 0.75f);
		Gizmos.DrawSphere(lookAtData.transform.position, 0.5f);
		Gizmos.DrawSphere(lookAtData.lookAtPointGlobal, 0.5f);

		// Camera frustum
		// If not defined, use main camera values in a different color
		// If there is no main camera, use default values in a different color
		Gizmos.color = gizmoColor;
		Camera refCamera = Camera.main;

		// Fov
		float targetFov = fov;
		if(!changeFov) {
			targetFov = (refCamera != null) ? refCamera.fieldOfView : 60f;
		}

		// Near
		float targetNear = near;
		if(!changeNear) {
			targetNear = (refCamera != null) ? refCamera.nearClipPlane : 0.3f;
		}

		// Far
		float targetFar = far;
		if(!changeFar) {
			targetFar = (refCamera != null) ? refCamera.farClipPlane : 1000f;
		}

		// Draw camera frustum
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawFrustum(Vector3.zero, targetFov, targetFar, targetNear, (refCamera != null) ? refCamera.aspect : 4f/3f);
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Apply current values to given camera
	/// </summary>
	/// <param name="_cam">The camera to be modified.</param>
	public void Apply(Camera _cam) {
		// Check params
		if(_cam == null) return;

		// Camera position and orientation
		if(changePosition) _cam.transform.position = lookAtData.transform.position;
		if(changeRotation) _cam.transform.LookAt(lookAtData.lookAtPointGlobal);

		// Camera params
		if(changeFov) _cam.fieldOfView = fov;
		if(changeNear) _cam.nearClipPlane = near;
		if(changeFar) _cam.farClipPlane = far;

		// Fog params
		if(changeFogColor) RenderSettings.fogColor = fogColor;
		if(changeFogStart) RenderSettings.fogStartDistance = fogStart;
		if(changeFogEnd) RenderSettings.fogEndDistance = fogEnd;

		// Dark screen
		// Apply values
		SpriteRenderer screen = ApplyDarkScreen(_cam);
		if(screen != null) {
			if(darkenScreen) {
				screen.gameObject.SetActive(true);
				screen.transform.localPosition = Vector3.forward * darkScreenDistance;
				screen.color = this.darkScreenColor;
			} else {
				screen.color = Colors.WithAlpha(screen.color, 0f);
				screen.gameObject.SetActive(false);
			}
		}
	}

	/// <summary>
	/// Tween the given camera from its current position to this snap point.
	/// Tween also current render settings.
	/// Respects the change flags.
	/// </summary>
	/// <returns>The generated tween sequence.</returns>
	/// <param name="_cam">The camera to be tweened.</param>la
	/// <param name="_duration">Tween duration, same considerations as in any DGTween.</param>
	/// <param name="_params">Animation parameters.</param>
	/// <param name="_onComplete">Optional function to be called upon completing the tween.</param>
	public Sequence TweenTo(Camera _cam, float _duration, TweenParams _params, TweenCallback _onComplete = null) {
		// Check params
		if(_cam == null) return null;
		if(_params == null) return null;

		// [AOC] Make sure tweens are autokilled and recyclable, so we do a "pooling" of this tween type, avoiding creating memory garbage
		string tweenId = GetTweenId(_cam);
		_params.SetAutoKill(true);
		_params.SetRecyclable(true);
		_params.SetId(tweenId);

		// Stop any other camera snap point animation
		// [AOC] This obviously won't work when animating multiple cameras with the same name at the same time, but it's a rare case which we don't care about right now
		DOTween.Kill(tweenId);

		// Create a whole sequence to keep all the tweens together
		Sequence seq = DOTween.Sequence()
			.SetRecyclable(true)
			.SetAutoKill(true)
			.SetId(tweenId)
			.SetTarget(_cam);
		
		// Camera position and orientation
		if(changePosition){
			seq.Join(_cam.transform.DOMove(lookAtData.transform.position, _duration).SetAs(_params));
		}
		if(changeRotation) {
			seq.Join(_cam.transform.DORotateQuaternion(lookAtData.transform.rotation, _duration).SetAs(_params));
		}

		// Camera params
		if(changeFov) {
			seq.Join(DOTween.To(
				() => { return _cam.fieldOfView; },
				(_newValue) => { _cam.fieldOfView = _newValue; },
				fov, _duration
			).SetAs(_params));
		}

		if(changeNear) {
			seq.Join(DOTween.To(
				() => { return _cam.nearClipPlane; },
				(_newValue) => { _cam.nearClipPlane = _newValue; },
				near, _duration
			).SetAs(_params));
		}

		if(changeFar) {
			seq.Join(DOTween.To(
				() => { return _cam.farClipPlane; },
				(_newValue) => { _cam.farClipPlane = _newValue; },
				far, _duration
			).SetAs(_params));
		}

		// Fog params
		if(changeFogColor) {
			seq.Join(DOTween.To(
				() => { return RenderSettings.fogColor; },
				(_newValue) => { RenderSettings.fogColor = _newValue; },
				fogColor, _duration
			).SetAs(_params));
		}

		if(changeFogStart) {
			seq.Join(DOTween.To(
				() => { return RenderSettings.fogStartDistance; },
				(_newValue) => { RenderSettings.fogStartDistance = _newValue; },
				fogStart, _duration
			).SetAs(_params));
		}

		if(changeFogEnd) {
			seq.Join(DOTween.To(
				() => { return RenderSettings.fogEndDistance; },
				(_newValue) => { RenderSettings.fogEndDistance = _newValue; },
				fogEnd, _duration
			).SetAs(_params));
		}

		// Dark screen
		SpriteRenderer screen = ApplyDarkScreen(_cam);
		if(screen != null) {
			// Make sure the screen is active (if we have the darken screen toggled)
			if(darkenScreen) screen.gameObject.SetActive(true);

			// Tween position
			screen.transform.DOLocalMove(Vector3.forward * darkScreenDistance, _duration).SetAs(_params);

			// Tween color
			// If disabling the darken screen, tween to transparent
			Color targetColor = darkenScreen ? this.darkScreenColor : Colors.WithAlpha(screen.color, 0f);
			seq.Join(DOTween.To(
				() => { return screen.color; },
				(_newValue) => { screen.color = _newValue; },
				targetColor, _duration
			).SetAs(_params));
		}

		// Attach custom OnComplete callback
		seq.OnComplete(() => {
			// If we were turning off the dark screen, disable it now
			if(!darkenScreen) screen.gameObject.SetActive(false);

			// If defined, call the OnComplete() callback
			if(_onComplete != null) _onComplete();
		});

		seq.Restart(true);
		return seq;
	}

	//------------------------------------------------------------------//
	// STATIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Generate the name to identify tweens associated to the given camera.
	/// Generated name is an association of a constant string + the camera object name.
	/// </summary>
	/// <returns>The name used on tweens generated by TweenTo method for the given camera.</returns>
	/// <param name="_cam">Target camera.</param>
	public static string GetTweenId(Camera _cam) {
		return TWEEN_ID + _cam.name;
	}

	/// <summary>
	/// Apply the dark screen instance to a specific camera.
	/// The dark screen instance is not created, the prefab will be instantiated.
	/// </summary>
	/// <returns>The dark screen linked to the given camera.</returns>
	/// <param name="_cam">Camera to be checked.</param>
	private static SpriteRenderer ApplyDarkScreen(Camera _cam) {
		// Is dark screen instance created?
		if(m_darkScreen == null) {
			// No! Do it now
			GameObject screenPrefab = Resources.Load<GameObject>(DARK_SCREEN_PREFAB_PATH);
			GameObject screenInstance = GameObject.Instantiate<GameObject>(screenPrefab);
			screenInstance.hideFlags = HideFlags.DontSave;
			m_darkScreen = screenInstance.GetComponent<SpriteRenderer>();
		}

		// Move dark screen to target camera's hierarchy
		m_darkScreen.transform.SetParent(_cam.transform, false);
		return m_darkScreen;
	}
}
