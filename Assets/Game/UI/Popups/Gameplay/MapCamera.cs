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
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected void OnDestroy() {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
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

}