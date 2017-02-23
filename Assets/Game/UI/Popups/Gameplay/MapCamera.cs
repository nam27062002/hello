// LevelMapData.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/06/2016.
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
/// Data class to hold unique info for each level related to the map.
/// Each level should have one of these instantiated together with the map camera
/// customized for that level.
/// </summary>
[RequireComponent(typeof(Camera))]
public class MapCamera : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Properties
	private Camera m_camera = null;
	public Camera camera {
		get {
			if(m_camera == null) {
				m_camera = GetComponent<Camera>();
			}
			return m_camera;
		}
	}

	// Internal
	int m_originalLayerMask = 0;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Store original layer mask and initialize layer mask according to current map level
		m_originalLayerMask = camera.cullingMask;
		SetLayerMask(UsersManager.currentUser.mapLevel);

		// Subscribe to external events
		Messenger.AddListener<int>(GameEvents.PROFILE_MAP_UPGRADED, OnMapUpgraded);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<int>(GameEvents.PROFILE_MAP_UPGRADED, OnMapUpgraded);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Apply the layer mask corresponding to a given map upgrade level.
	/// </summary>
	/// <param name="_mapLevel">Map level.</param>
	private void SetLayerMask(int _mapLevel) {
		// If map is locked, show nothing
		if(_mapLevel == 0) {
			camera.cullingMask = 0;
		} else {
			camera.cullingMask = m_originalLayerMask;
		}
	}

	#if UNITY_EDITOR
	/// <summary>
	/// Draw gizmos to help debugging.
	/// </summary>
	void OnDrawGizmos() {
		// Only if enabled
		if(!isActiveAndEnabled) return;

		// Only if we have a valid level loaded
		if(LevelManager.currentLevelData == null) return;

		// Draw bounds in the world space
		Rect bounds = LevelManager.currentLevelData.bounds;
		Vector3 tl = new Vector3(bounds.xMin, bounds.yMin, 0);
		Vector3 tr = new Vector3(bounds.xMax, bounds.yMin, 0);
		Vector3 bl = new Vector3(bounds.xMin, bounds.yMax, 0);
		Vector3 br = new Vector3(bounds.xMax, bounds.yMax, 0);

		Gizmos.color = Colors.paleYellow;
		Gizmos.DrawLine(tl, tr);
		Gizmos.DrawLine(bl, br);
		Gizmos.DrawLine(tl, bl);
		Gizmos.DrawLine(tr, br);

		// Draw camera bounds as well
		Vector3 camPos = camera.transform.position;
		Vector2 camHalfSize = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize);
		tl = new Vector3(camPos.x - camHalfSize.x, camPos.y + camHalfSize.y, 0);
		tr = new Vector3(camPos.x + camHalfSize.x, camPos.y + camHalfSize.y, 0);
		bl = new Vector3(camPos.x - camHalfSize.x, camPos.y - camHalfSize.y, 0);
		br = new Vector3(camPos.x + camHalfSize.x, camPos.y - camHalfSize.y, 0);

		Gizmos.color = Colors.purple;
		Gizmos.DrawLine(tl, tr);
		Gizmos.DrawLine(bl, br);
		Gizmos.DrawLine(tl, bl);
		Gizmos.DrawLine(tr, br);
	}
	#endif

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Minimap has been upgraded.
	/// </summary>
	/// <param name="_newLevel">New minimap level.</param>
	private void OnMapUpgraded(int _newLevel) {
		// Update rendering layers
		// Add some delay to give time for feedback to show off
		float delay = 0.25f;
		if(_newLevel == 1) delay = 1.5f;	// Unlock animation is a bit longer
		DOVirtual.DelayedCall(delay, () => SetLayerMask(_newLevel), true);
	}
}