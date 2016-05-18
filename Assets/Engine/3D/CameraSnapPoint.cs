// CameraSnapPoint.cs
// 
// Created by Alger Ortín Castellví on 29/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;

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
	private static readonly string TWEEN_ID = "CameraSnapPointTween";

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed parameters
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

	// Editor Settings
	public bool livePreview = true;
	public Color gizmoColor = new Color(0f, 1f, 1f, 0.25f);

	// Internal references
	private LookAt m_lookAtPoint = null;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Get references.
	/// </summary>
	void Awake() {
		if(m_lookAtPoint == null) m_lookAtPoint = GetComponent<LookAt>();
	}

	/// <summary>
	/// Draw scene gizmos.
	/// </summary>
	private void OnDrawGizmos() {
		// Make sure lookAtPoint reference is valid
		if(m_lookAtPoint == null) m_lookAtPoint = GetComponent<LookAt>();

		// LookAt line
		Gizmos.color = Colors.WithAlpha(Colors.cyan, 0.75f);
		Gizmos.DrawLine(m_lookAtPoint.transform.position, m_lookAtPoint.lookAtPointGlobal);

		// Position and lookAt points
		Gizmos.color = Colors.WithAlpha(Colors.red, 0.75f);
		Gizmos.DrawSphere(m_lookAtPoint.transform.position, 1f);
		Gizmos.DrawSphere(m_lookAtPoint.lookAtPointGlobal, 1f);

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
		if(m_lookAtPoint == null) m_lookAtPoint = GetComponent<LookAt>();

		// Camera position
		_cam.transform.position = m_lookAtPoint.transform.position;
		_cam.transform.LookAt(m_lookAtPoint.lookAtPointGlobal);

		// Camera params
		if(changeFov) _cam.fieldOfView = fov;
		if(changeNear) _cam.nearClipPlane = near;
		if(changeFar) _cam.farClipPlane = far;

		// Fog params
		if(changeFogColor) RenderSettings.fogColor = fogColor;
		if(changeFogStart) RenderSettings.fogStartDistance = fogStart;
		if(changeFogEnd) RenderSettings.fogEndDistance = fogEnd;
	}

	/// <summary>
	/// Tween the given camera from its current position to this snap point.
	/// Tween also current render settings.
	/// Respects the change flags.
	/// </summary>
	/// <param name="_cam">The camera to be tweened.</param>
	/// <param name="_duration">Tween duration, same considerations as in any DGTween.</param>
	/// <param name="_params">Animation parameters.</param>
	public void TweenTo(Camera _cam, float _duration, TweenParams _params) {
		// Check params
		if(_cam == null) return;
		if(_params == null) return;
		if(m_lookAtPoint == null) m_lookAtPoint = GetComponent<LookAt>();

		// [AOC] Make sure tweens are autokilled and recyclable, so we do a "pooling" of this tween type, avoiding creating memory garbage
		string tweenId = GetTweenId(_cam);
		_params.SetAutoKill(true);
		_params.SetRecyclable(true);
		_params.SetId(tweenId);

		// Stop any other camera snap point animation
		// [AOC] This obviously won't work when animating multiple cameras with the same name at the same time, but it's a rare case which we don't care about right now
		DOTween.Kill(tweenId);

		// Camera position
		_cam.transform.DOMove(m_lookAtPoint.transform.position, _duration).SetAs(_params);
		_cam.transform.DORotateQuaternion(m_lookAtPoint.transform.rotation, _duration).SetAs(_params);

		// Camera params
		if(changeFov) {
			DOTween.To(
				() => { return _cam.fieldOfView; },
				(_newValue) => { _cam.fieldOfView = _newValue; },
				fov, _duration
			).SetAs(_params);
		}

		if(changeNear) {
			DOTween.To(
				() => { return _cam.nearClipPlane; },
				(_newValue) => { _cam.nearClipPlane = _newValue; },
				near, _duration
			).SetAs(_params);
		}

		if(changeFar) {
			DOTween.To(
				() => { return _cam.farClipPlane; },
				(_newValue) => { _cam.farClipPlane = _newValue; },
				far, _duration
			).SetAs(_params);
		}

		// Fog params
		if(changeFogColor) {
			DOTween.To(
				() => { return RenderSettings.fogColor; },
				(_newValue) => { RenderSettings.fogColor = _newValue; },
				fogColor, _duration
			).SetAs(_params);
		}

		if(changeFogStart) {
			DOTween.To(
				() => { return RenderSettings.fogStartDistance; },
				(_newValue) => { RenderSettings.fogStartDistance = _newValue; },
				fogStart, _duration
			).SetAs(_params);
		}

		if(changeFogEnd) {
			DOTween.To(
				() => { return RenderSettings.fogEndDistance; },
				(_newValue) => { RenderSettings.fogEndDistance = _newValue; },
				fogEnd, _duration
			).SetAs(_params);
		}
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
}
