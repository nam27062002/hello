// MenuSelectedDragonLoader.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class MenuSelectedDragonLoader : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private GameObject m_dragonPreview = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialiation.
	/// </summary>
	private void Awake() {
		// If this object has any children, consider first one the dragon preview placeholder
		if(transform.childCount > 0) {
			m_dragonPreview = transform.GetChild(0).gameObject;
		}
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Load currently selected dragon
		LoadSelectedDragon();
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_CONFIRMED, OnDragonChanged);
	}

	/// <summary>
	/// The component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_CONFIRMED, OnDragonChanged);
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	private void LoadSelectedDragon() {
		// Unload current dragon if any
		if(m_dragonPreview != null) {
			GameObject.Destroy(m_dragonPreview);
			m_dragonPreview = null;
		}

		// Load selected dragon
		DragonDef def = DefinitionsManager.dragons.GetDef(UserProfile.currentDragon);
		if(def != null) {
			// Instantiate the prefab and add it as child of this object
			GameObject dragonPrefab = Resources.Load<GameObject>(def.menuPrefabPath);
			m_dragonPreview = GameObject.Instantiate<GameObject>(dragonPrefab);
			m_dragonPreview.transform.SetParent(this.transform);
			m_dragonPreview.transform.localPosition = Vector3.zero;
			m_dragonPreview.transform.localRotation = Quaternion.identity;
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The selected dragon has changed.
	/// </summary>
	/// <param name="_sku">The sku of the new dragon.</param>
	public void OnDragonChanged(string _sku) {
		LoadSelectedDragon();
	}
}