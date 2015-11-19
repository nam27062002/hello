// MenuDragonScroller.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Show the currently selected dragon in the menu screen.
/// TODO!! Nice animation
/// </summary>
public class MenuDragonScroller : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[SerializeField] private Camera m_camera = null;

	// Internal
	private Dictionary<DragonId, GameObject> m_dragons = new Dictionary<DragonId, GameObject>();
	private GameObject m_currentDragon = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		DebugUtils.Assert(m_camera != null, "Required field!");
	}

	/// <summary>
	/// First update
	/// </summary>
	private void Start() {
		// Subscribe to external events
		Messenger.AddListener(MenuDragonSelector.EVENT_DRAGON_CHANGED, OnSelectedDragonChanged);
		
		// Do a first refresh
		OnSelectedDragonChanged();
	}
	
	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MenuDragonSelector.EVENT_DRAGON_CHANGED, OnSelectedDragonChanged);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Selected dragon has changed
	/// </summary>
	private void OnSelectedDragonChanged() {
		// Get dragon data from the dragon manager
		DragonData newDragonData = DragonManager.currentDragonData;

		// Get cached dragon preview, if not found create it
		GameObject dragonObj = null;
		if(!m_dragons.TryGetValue(newDragonData.id, out dragonObj)) {
			// Load it from resources
			GameObject prefab = Resources.Load<GameObject>(newDragonData.menuPrefabPath);
			DebugUtils.Assert(prefab != null, "Menu prefab for dragon of type " + newDragonData.id + " wasn't found (" + newDragonData.menuPrefabPath + ")");
			dragonObj = GameObject.Instantiate<GameObject>(prefab);
			dragonObj.transform.SetParent(this.transform, false);

			// Store it in local cache
			m_dragons.Add(newDragonData.id, dragonObj);
		}

		// If we have an active dragon, make it disappear
		if(m_currentDragon != null) {
			// [AOC] TODO!! Animation!
			m_currentDragon.SetActive(false);
		}

		// Show new dragon
		if(dragonObj != null) {
			// Show new dragon!
			// Do it before the rest, otherwise GetComponentInChildren doesn't work! D:
			dragonObj.SetActive(true);
			m_currentDragon = dragonObj;

			// Make sure the dragon has the scale according to its level
			dragonObj.transform.localScale = Vector3.one * newDragonData.scale;

			// Make camera centered to the dragon
			SkinnedMeshRenderer dragonMesh = dragonObj.GetComponentInChildren<SkinnedMeshRenderer>();
			if(dragonMesh != null) {
				m_camera.transform.LookAt(dragonMesh.bounds.center);
			}
		}
	}
}

