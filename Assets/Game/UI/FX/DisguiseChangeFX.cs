// DisguiseChangeFX.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/01/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple class to control the disguise change FX on the menus.
/// </summary>
public class DisguiseChangeFX : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private GameObject m_prefab = null;

	// Internal
	private GameObject m_fxInstance = null;

	// Internal references
	private MenuDragonScroller m_dragonScroller = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_DISGUISE_CHANGE, OnDisguiseChanged);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_DISGUISE_CHANGE, OnDisguiseChanged);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Start (or restart) the FX.
	/// </summary>
	public void StartFX() {
		// IF not already done, instantiate now
		if(m_fxInstance == null) {
			m_fxInstance = GameObject.Instantiate<GameObject>(m_prefab);
			m_fxInstance.transform.SetParent(this.transform, false);
		}

		// Restart FX
		m_fxInstance.SetActive(false);
		m_fxInstance.SetActive(true);
	}

	/// <summary>
	/// Stop the FX. No effect if already stopped.
	/// </summary>
	public void StopFX() {
		// Disable ourselves
		if(m_fxInstance != null) {
			m_fxInstance.SetActive(false);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The selected disguise has been changed in the menu.
	/// </summary>
	/// <param name="_dragonSku">The dragon whose disguise has been changed.</param>
	private void OnDisguiseChanged(string _dragonSku) {
		// Launch FX!
		StartFX();

		// Adjust scale based on dragon
		if(m_fxInstance != null) {
			// Reset scale
			m_fxInstance.transform.localScale = Vector3.one;

			// If dragon scroller hasn't yet been found, look for it
			if(m_dragonScroller == null) {
				MenuScreenScene scene3D = InstanceManager.menuSceneController.GetScreenScene(MenuScreens.DISGUISES);
				m_dragonScroller = scene3D.GetComponent<MenuDragonScroller>();
			}

			// Apply scale according to target dragon's preview
			if(m_dragonScroller != null) {
				MenuDragonPreview preview = m_dragonScroller.GetDragonPreview(_dragonSku);
				if(preview != null) {
					m_fxInstance.transform.localScale = preview.transform.localScale;
				}
			}
		}
	}
}