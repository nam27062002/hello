// GameCameraController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controller for the camera in the game scene.
/// In this particular game, the camera will always be looking at Z-0 and never rotate, 
/// making everything much easier.
/// </summary>
public class GameCameraController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed members
	[Separator("Movement")]
	[SerializeField] private float m_smoothFactor = 0.15f;
	[SerializeField] private float m_forwardOffset = 300f;	// The distance looking ahead
	[SerializeField] private Range m_limitX = new Range(-100, 100);

	[Separator("Zoom")]
	[InfoBox("All zoom related values are in relative terms [0..1] to Zoom Range")]
	[SerializeField] [Range(0, 1)] private float m_defaultZoom = 0;
	[SerializeField] private Range m_zoomRange = new Range(10f, 300f);	// Zoom factor, distance from Z-0 in world units

	[Separator("Shaking")]
	[SerializeField] private Vector3 m_shakeDefaultAmount = new Vector3(0.5f, 0.5f, 0f);
	[SerializeField] private float m_shakeDefaultDuration = 0.15f;
	[SerializeField] private bool m_shakeDecayOverTime = true;

	// References
	private Transform m_player = null;
	private Transform m_danger = null;

	// Positioning
	private Vector3 m_currentPos = Vector3.zero;
	private Vector3 m_targetPos = Vector3.zero;
	private Vector3 m_playerDir = Vector3.right;
	private Vector3 m_forward = Vector3.right;

	// Zoom
	private Interpolator m_zInterpolator = new Interpolator();

	// Shake
	private Vector3 m_shakeAmount = Vector3.one;
	private float m_shakeDuration = 0f;
	private float m_shakeTimer = 0f;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Current zoom level [0..1]
	public float zoom {
		//get { return transform.position.z - m_defaultZ; }
		get { return m_zoomRange.InverseLerp(transform.position.z); }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {

	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}
}

