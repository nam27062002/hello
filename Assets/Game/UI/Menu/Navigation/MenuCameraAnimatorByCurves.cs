// MenuCameraAnimator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/10/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 3D camera control to follow a path defined by 2 bézier curves (pos and lookat).
/// </summary>
public class MenuCameraAnimatorByCurves : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[SerializeField] private PathFollower m_cameraPath = null;
	public PathFollower cameraPath { get { return m_cameraPath; }}

	[SerializeField] private PathFollower m_lookAtPath = null;
	public PathFollower lookAtPath { get { return m_lookAtPath; }}

	// Setup
	[Space(10)]
	[Tooltip("Seconds to travel one snap point's distance (min) and all path's distance (max).\nFinal animation duration will be interpolated from that.")]
	[SerializeField] private Range m_animSpeed = new Range(0.25f, 1f);
	public Range animSpeed {
		get { return m_animSpeed; }
		set { m_animSpeed = value; }
	}

	[SerializeField] private Ease m_animEase = Ease.InOutQuad;
	public Ease animEase {
		get { return m_animEase; }
		set { m_animEase = value; }
	}

	// Target scene
	[Space(10)]
	[Tooltip("The animator will only work when the active 3D scene matches this one")]
	[SerializeField] private MenuScreenScene m_targetScene = null;

	// Solo-properties
	// Delta and snap point are stored in the camera path to avoid keeping control of multiple vars
	public float delta {
		get { 
			return m_cameraPath != null ? m_cameraPath.delta : 0f; 
		}
		set {
			m_cameraPath.delta = value;
			m_lookAtPath.delta = value;
		}
	}

	public int snapPoint {
		get { 
			return m_cameraPath != null ? m_cameraPath.snapPoint : 0; 
		}
		set {
			m_cameraPath.snapPoint = value;
			m_lookAtPath.snapPoint = value;
		}
	}

	// Internal references
	private MenuScreensController m_menuScreensController = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_cameraPath != null, "Required field");
		Debug.Assert(m_lookAtPath != null, "Required field");
	}

	/// <summary>
	/// First update call
	/// </summary>
	private void Start() {
		// Store reference to menu screens controller for faster access
		m_menuScreensController = InstanceManager.GetSceneController<MenuSceneController>().screensController;
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// If we're at one of the target screens, snap camera to curves
		// [AOC] A bit dirty, but best way I can think of right now (and gacha is waiting)
		if(m_menuScreensController.currentScene == m_targetScene) {
			// Only if camera is not already moving!
			if(!m_menuScreensController.tweening) {
				// Move camera! ^_^
				InstanceManager.sceneController.mainCamera.transform.position = m_cameraPath.position;
				InstanceManager.sceneController.mainCamera.transform.LookAt(m_lookAtPath.position);
			}
		}
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Animate to a specific delta in the path.
	/// </summary>
	/// <param name="_delta">Target delta.</param>
	public void GoTo(float _delta) {
		// Adapt duration to the distance to traverse
		int targetSnapPoint = m_cameraPath.path.GetPointAt(_delta);
		float dist = Mathf.Abs((float)(targetSnapPoint - m_cameraPath.snapPoint));
		float delta = Mathf.InverseLerp(0, m_cameraPath.path.pointCount, dist);
		float duration = m_animSpeed.Lerp(delta);

		// Just let the paths manage it
		m_cameraPath.GoTo(_delta, duration, m_animEase);
		m_lookAtPath.GoTo(_delta, duration, m_animEase);
	}


	/// <summary>
	/// Animate to a specific point in the path.
	/// </summary>
	/// <param name="_snapPoint">Target control point.</param>
	public void SnapTo(int _snapPoint) {
		SnapToAndGetDelta(_snapPoint);
	}

	/// <summary>
	/// Animate to a specific point in the path.
	/// </summary>
	/// <param name="_snapPoint">Target control point.</param>
	/// <returns>The delta corresponding to the target snap point</returns>
	public float SnapToAndGetDelta(int _snapPoint) {
		// Adapt duration to the distance to traverse
		float dist = Mathf.Abs((float)(_snapPoint - m_cameraPath.snapPoint));
		float delta = Mathf.InverseLerp(0, m_cameraPath.path.pointCount, dist);
		float duration = m_animSpeed.Lerp(delta);

		// Just let the paths manage it
		float targetDelta = 0f;
		targetDelta = m_cameraPath.SnapTo(_snapPoint, duration, m_animEase);
		m_lookAtPath.SnapTo(_snapPoint, duration, m_animEase);
		return targetDelta;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//

}

