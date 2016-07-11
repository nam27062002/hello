// MenuDragonLoader.cs
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
/// Helper class to easily instantiate dragon models within the menu.
/// </summary>
public class MenuDragonLoader : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Mode {
		CURRENT_DRAGON,		// Automatically loads and updates CURRENT dragon (UserProfile.currentDragon)
		SELECTED_DRAGON,	// Automatically loads and updates SELECTED dragon (MenuSceneController.selectedDragon)
		MANUAL				// Manual control via the LoadDragon() method and the exposed m_dragonSku parameter
	}
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private Mode m_mode = Mode.CURRENT_DRAGON;
	public Mode mode {
		get { return m_mode; }
		set {
			m_mode = value; 
			RefreshDragon();
		}
	}

	[SkuList(DefinitionsCategory.DRAGONS, true)]
	[SerializeField] private string m_dragonSku = "";
	public string dragonSku {
		get { return m_dragonSku; }
		set {
			m_dragonSku = value;
			RefreshDragon();
		}
	}

	[Space]
	[List("idle", "fly_idle")]
	[SerializeField] private string m_initialAnim = "idle";
	[SerializeField] private bool m_resetDragonScale = true;

	// Internal
	private GameObject m_dragonInstance = null;
	public GameObject dragonInstance {
		get { return m_dragonInstance; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialiation.
	/// </summary>
	private void Awake() {
		// Try to find out already instantiated previews of the dragon
		MenuDragonPreview preview = this.GetComponentInChildren<MenuDragonPreview>();
		if(preview != null) {
			m_dragonInstance = preview.gameObject;
		}

		// If this object has any children, consider first one the dragon preview placeholder
		else if(transform.childCount > 0) {
			m_dragonInstance = transform.GetChild(0).gameObject;
		}
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Initialize loaded dragon (unless using MANUAL mode)
		if(m_mode != Mode.MANUAL) RefreshDragon();

		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_CONFIRMED, OnDragonConfirmed);
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	/// <summary>
	/// The component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_CONFIRMED, OnDragonConfirmed);
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load the dragon with the given sku.
	/// </summary>
	/// <param name="_sku">The sku of the dragon to be loaded</param>
	public void LoadDragon(string _sku) {
		// Unload current dragon if any
		UnloadDragon();

		// Load selected dragon
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, _sku);
		if(def != null) {
			// Instantiate the prefab and add it as child of this object
			GameObject dragonPrefab = Resources.Load<GameObject>(def.GetAsString("menuPrefab"));
			if(dragonPrefab != null) {
				m_dragonInstance = GameObject.Instantiate<GameObject>(dragonPrefab);
				m_dragonInstance.transform.SetParent(this.transform);
				m_dragonInstance.transform.localPosition = Vector3.zero;
				m_dragonInstance.transform.localRotation = Quaternion.identity;
				m_dragonInstance.SetLayerRecursively(this.gameObject.layer);

				// Launch the default animation
				m_dragonInstance.GetComponentInChildren<Animator>().SetTrigger(m_initialAnim);

				// Reset scale if required
				if(m_resetDragonScale) {
					m_dragonInstance.transform.localScale = Vector3.one;
				}
			}
		}

		// Update dragon sku
		m_dragonSku = _sku;
	}

	/// <summary>
	/// Reload dragon preview based on mode.
	/// </summary>
	public void RefreshDragon() {
		// Load different dragons based on mode
		switch(m_mode) {
			case Mode.CURRENT_DRAGON:	LoadDragon(UserProfile.currentDragon);	break;
			case Mode.SELECTED_DRAGON:	LoadDragon(InstanceManager.GetSceneController<MenuSceneController>().selectedDragon);	break;
			case Mode.MANUAL:			LoadDragon(m_dragonSku);	break;
		}
	}

	/// <summary>
	/// Destroy current loaded dragon, if any.
	/// </summary>
	public void UnloadDragon() {
		// Just make sure the object doesn't have anything attached
		foreach(Transform child in transform) {
			GameObject.DestroyImmediate(child.gameObject);	// Immediate so it can be called from the editor
			m_dragonInstance = null;
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The current dragon has changed.
	/// </summary>
	/// <param name="_sku">The sku of the new dragon.</param>
	public void OnDragonConfirmed(string _sku) {
		// Only care if we're in CURRENT mode
		if(m_mode != Mode.CURRENT_DRAGON) return;
		LoadDragon(_sku);
	}

	/// <summary>
	/// The selected dragon has changed.
	/// </summary>
	/// <param name="_sku">The sku of the new dragon.</param>
	public void OnDragonSelected(string _sku) {
		// Only care if we're in SELECTED mode
		if(m_mode != Mode.SELECTED_DRAGON) return;
		LoadDragon(_sku);
	}
}
