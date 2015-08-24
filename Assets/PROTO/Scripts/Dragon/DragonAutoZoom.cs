// DragonAutoZoom.cs
// Furious Dragon
// 
// Created by Alger Ortín Castellví on 28/07/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Auto-adjust camera zoom based on dragon's state.
/// For this first version we will be based in dragon's state rather than distance towards enemies, 
/// which seems more complicated and less efficient.
/// </summary>
[RequireComponent(typeof(DragonPlayer))]
public class DragonAutoZoom : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed members
	public float maxIdleTime = 2f;
	public float idleZoom = -700f;	// World units in Z axis
	public float zoomSpeed = 350f;	// World units per second

	// References
	private DragonPlayer player = null;
	private CameraController mainCamera = null;

	// Internal
	float timer = 0f;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		// Check required members
		player = GetComponent<DragonPlayer>();
		DebugUtils.Assert(player != null, "Required member!");

		mainCamera = Camera.main.GetComponent<CameraController>();
		DebugUtils.Assert(mainCamera != null, "Required member!");
	}

	/// <summary>
	/// First update.
	/// </summary>
	void Start() {
		timer = maxIdleTime;
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<DragonPlayer.EState, DragonPlayer.EState>(GameEvents_OLD.PLAYER_STATE_CHANGED, OnPlayerStateChanged);
	}

	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {
		if(player.state == DragonPlayer.EState.IDLE) {
			// Only if not already zooming
			if(timer > 0) {
				// Update timer
				timer -= Time.deltaTime;
				if(timer <= 0) {
					// Zoom
					mainCamera.ZoomAtSpeed(idleZoom, zoomSpeed);
				}
			}
		}
	}

	/// <summary>
	/// The component has been disabled.
	/// </summary>
	void OnDisable() {
		// Subscribe to external events
		Messenger.RemoveListener<DragonPlayer.EState, DragonPlayer.EState>(GameEvents_OLD.PLAYER_STATE_CHANGED, OnPlayerStateChanged);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The logical state of the dragon has changed.
	/// </summary>
	/// <param name="_oldState">Previuos state of the dragon.</param>
	/// <param name="_newState">New state of the dragon.</param>
	private void OnPlayerStateChanged(DragonPlayer.EState _oldState, DragonPlayer.EState _newState) {
		// Going to idle? Reset timer
		if(_newState == DragonPlayer.EState.IDLE) {
			timer = maxIdleTime;
		}

		// Leaving idle? Go back to default zoom
		else if(_oldState == DragonPlayer.EState.IDLE) {
			mainCamera.ZoomAtSpeed(0f, zoomSpeed);
		}
	}
}
