// LevelSelectionCameraController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Main script to control several aspects of the Goals screen 3D scene.
/// </summary>
[RequireComponent(typeof(MenuCameraAnimatorBySnapPoints))]
public class GoalsSceneController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[Tooltip("Should match the tab id's on the UI to link each tab to a snap point in the camera animator")]
	[SerializeField] private List<string> m_tabNames = new List<string>();

	// Info panel
	[Space]
	[SerializeField] private Transform m_infoPanelUIAnchor = null;
	public Transform infoPanelUIAnchor {
		get { return m_infoPanelUIAnchor; }
	}

	// Chest references
	[Space]
	[Tooltip("Always 5 slots, please!")]
	[SerializeField] private ChestsScreenSlot[] m_chestSlots = new ChestsScreenSlot[5];
	public ChestsScreenSlot[] chestSlots {
		get { return m_chestSlots; }
	}

	// Internal
	private MenuCameraAnimatorBySnapPoints m_cameraAnimator = null;
	public MenuCameraAnimatorBySnapPoints cameraAnimator {
		get {
			if(m_cameraAnimator == null) {
				m_cameraAnimator = GetComponent<MenuCameraAnimatorBySnapPoints>();
			}
			return m_cameraAnimator;
		}
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		Messenger.AddListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnTabChanged);
		Messenger.AddListener(GameEvents.CHESTS_RESET, OnChestsReset);
		Messenger.AddListener(GameEvents.CHESTS_PROCESSED, OnChestsProcessed);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Initialize chests
		RefreshChests();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnTabChanged);
		Messenger.RemoveListener(GameEvents.CHESTS_RESET, OnChestsReset);
		Messenger.RemoveListener(GameEvents.CHESTS_PROCESSED, OnChestsProcessed);
	}

	/// <summary>
	/// A change has occurred on the inspector. Validate its values.
	/// </summary>
	private void OnValidate() {
		// There must be exactly 5 chest slots
		if(m_chestSlots.Length != 5) {
			// Create a new array with exactly 5 slots and copy as many values as we can
			ChestsScreenSlot[] chestSlots = new ChestsScreenSlot[5];
			for(int i = 0; i < m_chestSlots.Length && i < chestSlots.Length; i++) {
				chestSlots[i] = m_chestSlots[i];
			}
			m_chestSlots = chestSlots;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refreshs the chests.
	/// </summary>
	private void RefreshChests() {
		// Chest by chest
		Chest.RewardData rewardData;
		for(int i = 0; i < m_chestSlots.Length; i++) {
			// Skip if chest not initialized
			if(m_chestSlots[i] == null) continue;

			// Initialize with the state of that chest
			m_chestSlots[i].Init(ChestManager.collectedChests > i, ChestManager.GetRewardData(i + 1));
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A tab has been changed.
	/// </summary>
	/// <param name="_data">The event data.</param>
	private void OnTabChanged(NavigationScreenSystem.ScreenChangedEventData _data) {
		// Find whether the target screen is interesting for us
		if(_data.toScreen == null) return;
		if(!m_tabNames.Contains(_data.toScreen.screenName)) return;

		// If the goals screen is active, animate camera to the target snap point
		// Otherwise just mark target snap point as the current one
		MenuScreensController screensController = InstanceManager.menuSceneController.screensController;
		if(screensController.currentScreenIdx == (int)MenuScreens.GOALS) {
			// Animate camera to target snap point
			cameraAnimator.SnapTo(_data.toScreenIdx);
		} else {
			// Set target snap point as the current one for the goals screen
			screensController.SetCameraSnapPoint((int)MenuScreens.GOALS, cameraAnimator.snapPoints[_data.toScreenIdx]);
		}
	}

	/// <summary>
	/// Chest Manager has reset the chests timer.
	/// </summary>
	private void OnChestsReset() {
		RefreshChests();
	}

	/// <summary>
	/// Chest Manager has processed the chests and given the reward.
	/// </summary>
	private void OnChestsProcessed() {
		RefreshChests();
	}
}