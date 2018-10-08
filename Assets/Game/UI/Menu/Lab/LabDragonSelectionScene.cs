// LabDragonSelectionScene.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Main controller for the Lab Dragon Selection 3D scene.
/// </summary>
public class LabDragonSelectionScene : MenuScreenScene {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private MenuDragonLoader m_dragonLoader = null;
	public MenuDragonLoader dragonLoader {
		get { return m_dragonLoader; }
	}

	// Internal references
	private GameObject m_loadingUI = null;
	public GameObject loadingUI {
		set { m_loadingUI = value; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		Messenger.AddListener<SceneController.Mode, SceneController.Mode>(MessengerEvents.GAME_MODE_CHANGED, OnGameModeChanged);
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		m_dragonLoader.onDragonLoaded += OnDragonPreviewLoaded;

		// Destroy any loaded dragon preview
		UnloadDragonPreview();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<SceneController.Mode, SceneController.Mode>(MessengerEvents.GAME_MODE_CHANGED, OnGameModeChanged);
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		m_dragonLoader.onDragonLoaded -= OnDragonPreviewLoaded;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Load preview of a given dragon.
	/// Will unload previously loaded dragon preview.
	/// </summary>
	/// <param name="_sku">Dragon's sku.</param>
	private void LoadDragonPreview(string _sku) {
		// Toggle loading UI on (will be disabled once the dragon preview is loaded)
		if(m_loadingUI != null) m_loadingUI.gameObject.SetActive(true);

		// Load the new dragon with its current skin!
		IDragonData dragonData = DragonManager.GetDragonData(_sku);
		m_dragonLoader.LoadDragon(_sku, dragonData.persistentDisguise);
	}

	/// <summary>
	/// Unload current dragon's preview.
	/// </summary>
	private void UnloadDragonPreview() {
		// Toggle loading UI on
		if(m_loadingUI != null) m_loadingUI.gameObject.SetActive(false);

		// Unload dragon preview
		m_dragonLoader.UnloadDragon();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Active game mode has changed.
	/// </summary>
	/// <param name="_oldMode">Previous game mode.</param>
	/// <param name="_newMode">The new active game mode.</param>
	private void OnGameModeChanged(SceneController.Mode _oldMode, SceneController.Mode _newMode) {
		// Entering lab mode?
		if(_newMode == SceneController.Mode.SPECIAL_DRAGONS) {
			// Load current special dragon
			LoadDragonPreview(InstanceManager.menuSceneController.selectedDragon);
		}

		// Leaving lab mode?
		if(_oldMode == SceneController.Mode.SPECIAL_DRAGONS) {
			// Unload special dragon preview
			UnloadDragonPreview();
		}

	}

	/// <summary>
	/// A new dragon has been selected.
	/// </summary>
	/// <param name="_sku">SKU of the newly selected dragon.</param>
	private void OnDragonSelected(string _sku) {
		// Ignore if not in special dragon mode
		if(SceneController.mode != SceneController.Mode.SPECIAL_DRAGONS) return;

		// Load newly selected special dragon
		LoadDragonPreview(_sku);
	}

	/// <summary>
	/// Dragon preview has finished loading.
	/// </summary>
	/// <param name="_loader">The loader that triggered the event.</param>
	private void OnDragonPreviewLoaded(MenuDragonLoader _loader) {
		// Toggle loading UI off
		if(m_loadingUI != null) m_loadingUI.gameObject.SetActive(false);
	}
}