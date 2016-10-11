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
/// Simple script to control the camera on the level selection screen.
/// </summary>
[RequireComponent(typeof(MenuCameraAnimatorBySnapPoints))]
public class LevelSelectionCameraController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[Tooltip("Should match the tab id's on the UI to link each tab to a snap point in the camera animator")]
	[SerializeField] private List<string> m_tabNames = new List<string>();

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
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.AddListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnTabChanged);
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

		// Animate camera to target snap point
		cameraAnimator.SnapTo(_data.toScreenIdx);
	}
}