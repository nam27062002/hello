// LevelMapData.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/06/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Data class to hold unique info for each level related to the map.
/// Each level should have one of these instantiated together with the map camera
/// customized for that level.
/// </summary>
[RequireComponent(typeof(Camera))]
public class LevelMapData : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[Comment("The camera prefab must have a Camera and a LevelMapData components, and it should be unique for each level")]
	[SerializeField] private Rect m_mapCameraBounds = new Rect();
	public Rect mapCameraBounds {
		get { return m_mapCameraBounds; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
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

		// Draw bounds in the world space
		Vector3 tl = new Vector3(m_mapCameraBounds.xMin, m_mapCameraBounds.yMin, 0);
		Vector3 tr = new Vector3(m_mapCameraBounds.xMax, m_mapCameraBounds.yMin, 0);
		Vector3 bl = new Vector3(m_mapCameraBounds.xMin, m_mapCameraBounds.yMax, 0);
		Vector3 br = new Vector3(m_mapCameraBounds.xMax, m_mapCameraBounds.yMax, 0);

		Gizmos.color = Colors.purple;
		Gizmos.DrawLine(tl, tr);
		Gizmos.DrawLine(bl, br);
		Gizmos.DrawLine(tl, bl);
		Gizmos.DrawLine(tr, br);

		// Draw camera bounds as well
		Camera cam = GetComponent<Camera>();
		if(cam != null) {
			Vector3 camPos = cam.transform.position;
			Vector2 camHalfSize = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
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
	}
	#endif

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}