// MenuCameraAnimatorBySnapPoints.cs
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
/// 3D camera control to navigate through a list of camera snap points.
/// </summary>
public class MenuCameraAnimatorBySnapPoints : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[SerializeField] private List<CameraSnapPoint> m_snapPoints = new List<CameraSnapPoint>();
	public List<CameraSnapPoint> snapPoints { get { return m_snapPoints; }}

	// Animation Setup
	[Space()]
	[SerializeField] private float m_tweenDuration = 0.25f;
	[SerializeField] private Ease m_tweenEase = Ease.InOutQuad;

	// Logic
	[Space()]
	[SerializeField] private int m_snapPoint = 0;
	public int snapPoint {
		get { return m_snapPoint;  }
		set { SnapTo(m_snapPoint, false); }
	}

	// Target scene
	[Space(10)]
	[Tooltip("The animator will only work when the active 3D scene matches this one")]
	[SerializeField] private MenuScreenScene m_targetScene = null;

	// Internal references
	private MenuScreensController m_menuScreensController = null;
	private MenuScreensController menuScreensController {
		get {
			if(m_menuScreensController == null) {
				m_menuScreensController = InstanceManager.GetSceneController<MenuSceneController>().screensController;
			}
			return m_menuScreensController;
		}
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_snapPoints.Count > 0, "At least one snap point required");
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

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Animate to a specific point in the path.
	/// </summary>
	/// <param name="_snapPoint">Target control point.</param>
	/// <param name="_animate">Whether to animate or do an instant camera reposition.</param>
	public void SnapTo(int _snapPoint, bool _animate) {
		// Check params
		if(menuScreensController == null) return;

		// If we're not at one of the target screens, skip
		if(m_menuScreensController.currentScene != m_targetScene) return;

		// Get target camera
		Camera targetCam = InstanceManager.sceneController.mainCamera;
		if(targetCam == null) return;

		// Figure out target snap point
		if(m_snapPoints.Count == 0) return;
		m_snapPoint = Mathf.Clamp(_snapPoint, 0, m_snapPoints.Count - 1);
		CameraSnapPoint targetPoint = m_snapPoints[m_snapPoint];
		if(targetPoint == null) return;

		// Animate?
		// Camera snap point makes it easy for us! ^_^
		if(_animate) {
			TweenParams tweenParams = new TweenParams().SetEase(m_tweenEase);
			targetPoint.TweenTo(targetCam, m_tweenDuration, tweenParams, null);
		} else {
			targetPoint.Apply(targetCam);
		}

		// Update current scene's camera snap point!
		m_menuScreensController.SetCameraSnapPoint(m_menuScreensController.currentScreenIdx, targetPoint);
	}

	/// <summary>
	/// Animate to a specific point in the path.
	/// Single parameter version to be able to connect it via inspector.
	/// Always uses animation.
	/// </summary>
	/// <param name="_snapPoint">Snap point.</param>
	public void SnapTo(int _snapPoint) {
		SnapTo(_snapPoint, true);
	}

	//------------------------------------------------------------------//
	// DEBUG															//
	//------------------------------------------------------------------//
	private void OnDrawGizmos() {
		// Setup gizmos
		Gizmos.matrix = Matrix4x4.identity;

		// Draw data
		for(int i = 0; i < m_snapPoints.Count; i++) {
			// Draw line connecting to next point
			if(m_snapPoints.Count > 1) {
				Gizmos.color = Colors.WithAlpha(Colors.purple, 0.5f);
				int nextIdx = (i + 1)%m_snapPoints.Count;
				Gizmos.DrawLine(m_snapPoints[i].transform.position, m_snapPoints[nextIdx].transform.position);
			}

			// Draw point
			float size = 0.5f;
			if(i == m_snapPoint) {
				Gizmos.color = Colors.WithAlpha(Colors.magenta, 0.75f);
				size = 1f;
			} else {
				Gizmos.color = Colors.WithAlpha(Colors.pink, 0.75f);
			}
			Gizmos.DrawSphere(m_snapPoints[i].transform.position, size);
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//

}

