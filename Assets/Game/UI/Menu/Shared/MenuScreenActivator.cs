// MenuScreenActivator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/12/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

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
	public enum Mode {
		SHOW_ON_TARGET_SCREENS,
		HIDE_ON_TARGET_SCREENS
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private Mode m_mode = Mode.SHOW_ON_TARGET_SCREENS;
	[SerializeField] private List<MenuScreens> m_screens = new List<MenuScreens>();

	// Internal
	private MenuScreensController m_screensController = null;
	private ShowHideAnimator m_animator = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialiation.
	/// </summary>
	private void Awake() {
		// If we have an animator, store reference to it
		m_animator = GetComponent<ShowHideAnimator>();
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

		// Is the current screen in the list?
		MenuScreens currentScreen = (MenuScreens)m_screensController.currentScreenIdx;
		bool isScreenOnTheList = m_screens.IndexOf(currentScreen) >= 0;

		// Determine visibility
		bool show = true;
		switch(m_mode) {
			case Mode.SHOW_ON_TARGET_SCREENS: {
				show = isScreenOnTheList;
			} break;

			case Mode.HIDE_ON_TARGET_SCREENS: {
				show = !isScreenOnTheList;
			} break;
		}

		// Apply visibility
		if(m_animator != null) {
			m_animator.ForceSet(show);
		} else {
			gameObject.SetActive(show);
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
