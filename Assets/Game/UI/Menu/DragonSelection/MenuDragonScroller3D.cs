// MenuDragonScroller.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Show the currently selected dragon in the menu screen.
/// </summary>
[RequireComponent(typeof(MenuCameraAnimatorByCurves))]
public class MenuDragonScroller3D : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	private MenuCameraAnimatorByCurves m_cameraAnimator = null;
	public MenuCameraAnimatorByCurves cameraAnimator {
		get {
			if(m_cameraAnimator == null) {
				m_cameraAnimator = GetComponent<MenuCameraAnimatorByCurves>();
			}
			return m_cameraAnimator;
		}
	}

	// Dragon previews
	private Dictionary<string, MenuDragonPreview> m_dragonPreviews = new Dictionary<string, MenuDragonPreview>();

	// Internal refs
	private MenuScreensController m_menuScreensController = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Find and store dragon preview references
		MenuDragonPreview[] dragonPreviews = GetComponentsInChildren<MenuDragonPreview>();
		for(int i = 0; i < dragonPreviews.Length; i++) {
			// Add it into the map
			m_dragonPreviews[dragonPreviews[i].sku] = dragonPreviews[i];
		}

		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	/// <summary>
	/// First update call
	/// </summary>
	private void Start() {
		// Store reference to menu screens controller for faster access
		m_menuScreensController = InstanceManager.GetSceneController<MenuSceneController>().screensController;

		// Find game object linked to currently selected dragon
		FocusDragon(InstanceManager.GetSceneController<MenuSceneController>().selectedDragon, false);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Focus a specific dragon.
	/// </summary>
	/// <param name="_sku">The dragon identifier.</param>
	/// <param name="_animate">Whether to animate or do an instant camera swap.</param>
	public void FocusDragon(string _sku, bool _animate) {
		// Trust that snap points are placed based on dragons' menuOrder value
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, _sku);
		if(def == null) return;
		int menuOrder = def.GetAsInt("order");
		if(_animate) {
			cameraAnimator.SnapTo(menuOrder);
		} else {
			cameraAnimator.snapPoint = menuOrder;
		}
	}

	/// <summary>
	/// Get the 3D preview of a specific dragon.
	/// </summary>
	/// <returns>The dragon preview object.</returns>
	/// <param name="_sku">The sku of the dragon whose preview we want.</param>
	public MenuDragonPreview GetDragonPreview(string _sku) {
		// Try to get it from the dictionary
		MenuDragonPreview ret = null;
		m_dragonPreviews.TryGetValue(_sku, out ret);

		// If not found on the dictionary, try to find it in the hierarchy
		if(ret == null) {
			// We have need to check all the dragons anyway, so update them all
			MenuDragonPreview[] dragonPreviews = GetComponentsInChildren<MenuDragonPreview>();
			for(int i = 0; i < dragonPreviews.Length; i++) {
				// Add it into the map
				m_dragonPreviews[dragonPreviews[i].sku] = dragonPreviews[i];

				// Is it the one we're looking for?
				if(dragonPreviews[i].sku == _sku) {
					ret = dragonPreviews[i];
				}
			}
		}
		return ret;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The selected dragon has been changed.
	/// </summary>
	/// <param name="_sku">The sku of the dragon we want to be the current one.</param>
	private void OnDragonSelected(string _sku) {
		// Move camera to the newly selected dragon
		// If the current menu screen is not using the dragon selection 3D scene, skip animation
		MenuScreenScene currentScene = m_menuScreensController.currentScene;
		MenuScreenScene dragonSelectionScene = m_menuScreensController.GetScene((int)MenuScreens.DRAGON_SELECTION);
		if(currentScene == dragonSelectionScene) {
			FocusDragon(_sku, true);
		} else {
			FocusDragon(_sku, false);
		}
	}
}

