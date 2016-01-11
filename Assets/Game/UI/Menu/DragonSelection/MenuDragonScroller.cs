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
	private Dictionary<string, GameObject> m_dragons = new Dictionary<string, GameObject>();
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
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnSelectedDragonChanged);
		
		// Do a first refresh
		OnSelectedDragonChanged(InstanceManager.GetSceneController<MenuSceneController>().selectedDragon);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnSelectedDragonChanged);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Selected dragon has changed
	/// </summary>
	/// <param name="_sku">The sku of the selected dragon</param>
	public void OnSelectedDragonChanged(string _sku) {
		// Get dragon data from the dragon manager
		DragonData newDragonData = DragonManager.GetDragonData(_sku);

		// Get cached dragon preview, if not found create it
		GameObject dragonObj = null;
		if(!m_dragons.TryGetValue(newDragonData.def.sku, out dragonObj)) {
			// Load it from resources
			GameObject prefab = Resources.Load<GameObject>(newDragonData.def.menuPrefabPath);
			DebugUtils.Assert(prefab != null, "Menu prefab for dragon of type " + newDragonData.def.sku + " wasn't found (" + newDragonData.def.menuPrefabPath + ")");
			dragonObj = GameObject.Instantiate<GameObject>(prefab);
			dragonObj.transform.SetParent(this.transform, false);

			// Store it in local cache
			m_dragons.Add(newDragonData.def.sku, dragonObj);
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

