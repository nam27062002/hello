// NavigationScreenSystem.cs
// 
// Created by Alger Ortín Castellví on 03/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Class to implement a screen navigation system to move between different ui screens.
/// </summary>
public class NavigationScreenSystem : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly int SCREEN_NONE = -1;

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Screen references to be set from the inspector
	[SerializeField] private List<NavigationScreen> m_screens = new List<NavigationScreen>();
	[SerializeField] private NavigationScreen m_initialScreen = null;
	private int m_currentScreenIdx = SCREEN_NONE;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		// Initial screen is visible from the beginning
		for(int i = 0; i < m_screens.Count; i++) {
			if(m_screens[i] == m_initialScreen) {
				m_currentScreenIdx = i;
				m_screens[i].gameObject.SetActive(true);
			} else {
				m_screens[i].gameObject.SetActive(false);
			}
		}
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Navigate to the target screen. Use an int to be able to directly connect buttons to it.
	/// </summary>
	/// <param name="_newScreen">The index of the new screen to go to. Use -1 for NONE.</param>
	public void GoToScreen(int _newScreen) {
		// Ignore if screen is already active
		if(_newScreen == m_currentScreenIdx) return;
		
		// If out of bounds, go to none
		if(_newScreen < 0 || _newScreen >= m_screens.Count) {
			_newScreen = SCREEN_NONE;
		}
		
		// Figure out animation direction based on current and new screen indexes
		NavigationScreen.AnimDir dir = NavigationScreen.AnimDir.NEUTRAL;
		if(m_currentScreenIdx != SCREEN_NONE && _newScreen != SCREEN_NONE) {
			if(m_currentScreenIdx > _newScreen) {
				dir = NavigationScreen.AnimDir.BACK;
			} else {
				dir = NavigationScreen.AnimDir.FORWARD;
			}
		}
		
		// Hide current screen (if any)
		if(m_currentScreenIdx != SCREEN_NONE) {
			m_screens[m_currentScreenIdx].Hide(dir);
		}
		
		// Show new screen (if any)
		if(_newScreen != SCREEN_NONE) {
			m_screens[_newScreen].Show(dir);
		}
		
		// Update screen tracking
		m_currentScreenIdx = _newScreen;
	}
	
	/// <summary>
	/// Navigate to the target screen. Screen must be added to the screens array, otherwise nothing will happen.
	/// </summary>
	/// <param name="_screen">The screen to go to.</param>
	public void GoToScreen(NavigationScreen _screen) {
		for(int i = 0; i < m_screens.Count; i++) {
			if(m_screens[i] == _screen) {
				GoToScreen(i);
				return;
			}
		}
	}
}