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
public class MenuDragonScroller : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private GameObject m_dragonSlotsContainer = null;
	[SerializeField] private MenuCameraAnimatorByCurves m_cameraAnimator = null;
	public MenuCameraAnimatorByCurves cameraAnimator {
		get { return m_cameraAnimator; }
	}

	[Space]
	[SerializeField] private GameObject m_dragonPurchasedFX = null;
	[SerializeField] private GameObject m_disguisePurchasedFX = null;

	[Space]
	[SerializeField] private float m_lerpSpeed = 10f;

	// Dragon previews
	private Dictionary<string, MenuDragonSlot> m_dragonSlots = new Dictionary<string, MenuDragonSlot>();

	// Internal refs
	private MenuTransitionManager m_menuTransitionManager = null;
	private Transform m_cameraTransform = null;
	private Transform m_cameraAnchor = null;

	// Internal logic
	private bool m_snapCamera = false;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Find and store dragon preview references
		MenuDragonSlot[] dragonSlots = m_dragonSlotsContainer.GetComponentsInChildren<MenuDragonSlot>();
		for(int i = 0; i < dragonSlots.Length; i++) {
			// Add it into the map
			m_dragonSlots[dragonSlots[i].dragonPreview.sku] = dragonSlots[i];
		}

		// Subscribe to external events
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenTransitionEnd);
	}

	/// <summary>
	/// First update call
	/// </summary>
	private void Start() {
		// Store reference to menu screens controller for faster access
		m_menuTransitionManager = InstanceManager.menuSceneController.transitionManager;
		m_cameraTransform = m_menuTransitionManager.camera.transform;
		m_cameraAnchor = cameraAnimator.cameraPath.target;

		// Find game object linked to currently selected dragon
		FocusDragon(InstanceManager.menuSceneController.selectedDragon, false);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenTransitionEnd);
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// If the camera is not animating, snap it to the curve
		if(m_snapCamera) {
			if(m_cameraAnimator.isTweening || (m_cameraAnchor.position - m_cameraTransform.position).sqrMagnitude < 0.0001f) {
				// If animating, just snap camera to the path
				m_cameraTransform.position = m_cameraAnchor.position;
			} else {
				// Otherwise lerp a bit so we don't see camera jumps
				float amount = m_lerpSpeed * Time.fixedDeltaTime;
				m_cameraTransform.position = Vector3.Lerp(m_cameraTransform.position, m_cameraAnchor.position, amount);
			}
		}
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
			// Instantly move camera anchor
			cameraAnimator.snapPoint = menuOrder;

			// Instntly apply position to camera
			if(m_snapCamera) {	// Only if allowed! (we're in the right screen)
				if(m_cameraTransform != null && m_cameraAnchor != null) {
					m_cameraTransform.position = m_cameraAnchor.position;
				}
			}
		}

		// Only show pets of the focused dragon
		bool showPets = false;
		bool allowAltAnimations = false;
		foreach(KeyValuePair<string, MenuDragonSlot> kvp in m_dragonSlots) {
			showPets = allowAltAnimations = (kvp.Key == _sku);
			if ( allowAltAnimations)
			{
				MenuDragonSlot slot = GetDragonSlot( kvp.Key);
				if ( slot.currentState < DragonData.LockState.LOCKED )
					allowAltAnimations = false;
			}
			if(kvp.Value.dragonPreview.equip.showPets != showPets) {
				kvp.Value.dragonPreview.equip.TogglePets(showPets, false);
			}
			kvp.Value.dragonPreview.allowAltAnimations = allowAltAnimations;
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
		MenuDragonSlot slot = GetDragonSlot(_sku);
		if(slot != null) ret = slot.dragonPreview;

		// If not found on the dictionary, try to find it in the hierarchy
		if(ret == null) {
			// We have need to check all the dragons anyway, so update them all
			MenuDragonSlot[] dragonSlots = m_dragonSlotsContainer.GetComponentsInChildren<MenuDragonSlot>();
			for(int i = 0; i < dragonSlots.Length; i++) {
				// Add it into the map
				m_dragonSlots[dragonSlots[i].dragonPreview.sku] = dragonSlots[i];

				// Is it the one we're looking for?
				if(dragonSlots[i].dragonPreview.sku == _sku) {
					ret = dragonSlots[i].dragonPreview;
				}
			}
		}
		return ret;
	}

	/// <summary>
	/// Get the slot corresponding to a specific dragon.
	/// </summary>
	/// <returns>The slot of the requested dragon.</returns>
	/// <param name="_sku">The sku of the dragon whose slot we want.</param>
	public MenuDragonSlot GetDragonSlot(string _sku) {
		// Just get it from the dictionary
		MenuDragonSlot slot = null;
		m_dragonSlots.TryGetValue(_sku, out slot);
		return slot;
	}

	/// <summary>
	/// Launches the dragon purchased FX on the selected dragon.
	/// </summary>
	public void LaunchDragonPurchasedFX() {
		// Check required stuff
		if(m_dragonPurchasedFX == null) return;

		// Find target slot
		MenuDragonSlot slot = GetDragonSlot(InstanceManager.menuSceneController.selectedDragon);
		if(slot == null) return;

		// Create a new instance of the FX and put it on the selected dragon slot
		GameObject newObj = Instantiate<GameObject>(m_dragonPurchasedFX, slot.transform, false);

		// Auto-destroy after the FX has finished
		DestroyInSeconds destructor = newObj.AddComponent<DestroyInSeconds>();
		destructor.lifeTime = 9f;	// Sync with FX duration!
	}

	/// <summary>
	/// Launches the disguise purchased FX on the selected dragon.
	/// </summary>
	public void LaunchDisguisePurchasedFX() {
		// Check required stuff
		if(m_disguisePurchasedFX == null) return;

		// Find target slot
		MenuDragonSlot slot = GetDragonSlot(InstanceManager.menuSceneController.selectedDragon);
		if(slot == null) return;

		// Create a new instance of the FX and put it on the selected dragon slot
		GameObject newObj = Instantiate<GameObject>(m_disguisePurchasedFX, slot.transform, false);

		// Auto-destroy after the FX has finished
		DestroyInSeconds destructor = newObj.AddComponent<DestroyInSeconds>();
		destructor.lifeTime = 9f;	// Sync with FX duration!

		// Trigger SFX
		AudioController.Play("hd_unlock_dragon");
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
		MenuScreenScene currentScene = m_menuTransitionManager.currentScreenData.scene3d;
		MenuScreenScene dragonSelectionScene = m_menuTransitionManager.GetScreenData(MenuScreen.DRAGON_SELECTION).scene3d;
		if(currentScene == dragonSelectionScene) {
			FocusDragon(_sku, true);
		} else {
			FocusDragon(_sku, false);
		}
	}

	/// <summary>
	/// The current menu screen has changed (animation starts now).
	/// </summary>
	/// <param name="_from">Source screen.</param>
	/// <param name="_to">Target screen.</param>
	private void OnMenuScreenTransitionStart(MenuScreen _from, MenuScreen _to) {
		// If the new screen is not the dragon selection screen, hide all dragons except the selected one
		// To prevent seeing the head/tail of the previous/next dragons in pets/disguises/photo screens.
		bool showAll = _to == MenuScreen.DRAGON_SELECTION;
		bool animate = _from != MenuScreen.PLAY;
		foreach(KeyValuePair<string, MenuDragonSlot> kvp in m_dragonSlots) {
			// Use slot's ShowHideAnimator
			// Show always if it's the selected dragon!
			DragonData data = DragonManager.GetDragonData(kvp.Key);
			kvp.Value.animator.Set((showAll && data.lockState != DragonData.LockState.HIDDEN) || kvp.Key == InstanceManager.menuSceneController.selectedDragon, animate);
		}

		// Don't snap the camera!
		m_snapCamera = false;
	}

	/// <summary>
	/// The current menu screen has changed (animation ends now).
	/// </summary>
	/// <param name="_from">Source screen.</param>
	/// <param name="_to">Target screen.</param>
	private void OnMenuScreenTransitionEnd(MenuScreen _from, MenuScreen _to) {
		// If entering dragon selection screen, start snapping the camera!
		m_snapCamera = (_to == MenuScreen.DRAGON_SELECTION);
	}
}

