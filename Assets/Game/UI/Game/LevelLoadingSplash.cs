// LevelLoadingSplash.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Splash screen to show while the level is loading.
/// </summary>
public class LevelLoadingSplash : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Slider m_progressBar = null;
	[Space]
	[SerializeField] private Image m_dragonIcon = null;
	[SerializeField] private PowerIcon[] m_powerIcons = null;

	// Internal references
	private GameSceneController m_sceneController = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required references
		DebugUtils.Assert(m_progressBar != null, "Required param!");

		// Initialize with current dragon setup
		Initialize();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		m_sceneController = InstanceManager.gameSceneController;

		// Show!
		GetComponent<ShowHideAnimator>().ForceShow(false);
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnGameLevelLoaded);
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	private void Update() {
		// Update progress
		m_progressBar.normalizedValue = m_sceneController.levelLoadingProgress;
	}

	/// <summary>
	/// The component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe to external events
		Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, OnGameLevelLoaded);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize with current dragon setup.
	/// </summary>
	private void Initialize() {
		// Aux vars
		DragonData currentDragon = DragonManager.GetDragonData(UsersManager.currentUser.currentDragon);
		DefinitionNode skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, currentDragon.diguise);

		// Dragon image
		m_dragonIcon.sprite = Resources.Load<Sprite>(UIConstants.DISGUISE_ICONS_PATH + currentDragon.def.sku + "/" + skinDef.Get("icon"));

		// Powers: skin + pets
		List<DefinitionNode> powerDefs = new List<DefinitionNode>();

		// Skin
		if(skinDef == null) {
			powerDefs.Add(null);
		} else {
			powerDefs.Add(DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, skinDef.Get("powerup")));	// Can be null
		}

		// Pets
		for(int i = 0; i < currentDragon.pets.Count; i++) {
			DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, currentDragon.pets[i]);
			if(petDef == null) {
				powerDefs.Add(null);
			} else {
				powerDefs.Add(DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, petDef.Get("powerup")));
			}
		}

		// Initialize power icons
		for(int i = 0; i < m_powerIcons.Length; i++) {
			// Hide if there are not enough powers defined
			if(i >= powerDefs.Count) {
				m_powerIcons[i].gameObject.SetActive(false);
				continue;
			}

			// Hide if there is no power associated
			if(powerDefs[i] == null) {
				m_powerIcons[i].gameObject.SetActive(false);
				continue;
			}

			// Everything ok! Initialize
			m_powerIcons[i].gameObject.SetActive(true);
			m_powerIcons[i].InitFromDefinition(powerDefs[i], false, false);
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The game level has been loaded.
	/// </summary>
	private void OnGameLevelLoaded() {
		// Hide!
		GetComponent<ShowHideAnimator>().ForceHide();
	}
}