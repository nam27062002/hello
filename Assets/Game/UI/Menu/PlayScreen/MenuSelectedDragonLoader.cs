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
	[List("idle", "fly_idle")]
	[SerializeField] private string m_initialAnim = "idle";
	[SerializeField] private bool m_resetDragonScale = false;

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
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Load currently selected dragon
		LoadSelectedDragon();

		// Subscribe to external events
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_CONFIRMED, OnDragonChanged);
	}

	/// <summary>
	/// The component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_CONFIRMED, OnDragonChanged);
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
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, UsersManager.currentUser.CurrentDragon);
		if(def != null) {
            // Instantiate the prefab and add it as child of this object
            GameObject dragonPrefab = HDAddressablesManager.Instance.LoadAsset<GameObject>(def.GetAsString("menuPrefab"));
			m_dragonPreview = GameObject.Instantiate<GameObject>(dragonPrefab);
			m_dragonPreview.transform.SetParent(this.transform);
			m_dragonPreview.transform.localPosition = Vector3.zero;
			m_dragonPreview.transform.localRotation = Quaternion.identity;

			// Launch the default animation
			m_dragonPreview.GetComponentInChildren<Animator>().SetTrigger(m_initialAnim);

			if (m_resetDragonScale) {
				m_dragonPreview.transform.localScale = Vector3.one;
			}
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