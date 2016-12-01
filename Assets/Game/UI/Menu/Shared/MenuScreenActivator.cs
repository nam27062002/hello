// MenuScreenActivator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/12/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Activate object only on specific screens.
/// </summary>
public class MenuScreenActivator : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private MenuScreens m_screen = MenuScreens.NONE;

	// Internal
	private MenuScreensController m_screensController = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialiation.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Get required references
		m_screensController = InstanceManager.GetSceneController<MenuSceneController>().screensController;

		// Subscribe to external events
		Messenger.AddListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnScreenChanged);

		// Apply initial state
		Apply();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Subscribe to external events
		Messenger.RemoveListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnScreenChanged);
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	private void Apply() {
		// Check whether active screen is the target one
		if(m_screensController == null) return;

		// Special case if NONE
		if(m_screen == MenuScreens.NONE) {
			gameObject.SetActive(true);
		} else {
			gameObject.SetActive(m_screensController.currentScene == m_screensController.GetScene((int)m_screen));
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A navigation screen has changed.
	/// </summary>
	/// <param name="_data">The event's data.</param>
	public void OnScreenChanged(NavigationScreenSystem.ScreenChangedEventData _data) {
		// Refresh
		Apply();
	}
}
