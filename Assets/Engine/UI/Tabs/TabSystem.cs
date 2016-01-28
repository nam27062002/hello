// TabSystem.cs
// 
// Created by Alger Ortín Castellví on 26/11/2015.
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
/// Specialization of the navigation screen system for a tab system.
/// </summary>
public class TabSystem : NavigationScreenSystem {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Buttons list to switch between tabs
	// The custom editor will make sure that the size of this list matches the list of tabs
	[SerializeField] public List<Button> m_tabButtons = new List<Button>();

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Start() {
		// Make sure each screen has a button assigned
		DebugUtils.Assert(m_tabButtons.Count == m_screens.Count, "The amount of buttons and screens doesn't match");

		// Initialize parent so only initial tab is active
		base.Start();

		// Initialize buttons according to screen status
		for(int i = 0; i < m_screens.Count; i++) {
			// Button is disabled for the active screen
			m_tabButtons[i].interactable = !m_screens[i].gameObject.activeSelf;

			// Link button to that screen
			int screenIdx = i;	// Issue with lambda expressions and iterations, see http://answers.unity3d.com/questions/791573/46-ui-how-to-apply-onclick-handler-for-button-gene.html
			m_tabButtons[i].onClick.AddListener(() => GoToScreen(screenIdx));	// Lambda expression, see https://msdn.microsoft.com/en-us/library/bb397687.aspx
		}
	}

	//------------------------------------------------------------------//
	// OVERRIDES														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Navigate to the target screen. Use an int to be able to directly connect buttons to it.
	/// </summary>
	/// <param name="_newScreen">The index of the new screen to go to. Use -1 for NONE.</param>
	/// <param name="_animType">Optionally force the direction of the animation.</param>
	override public void GoToScreen(int _newScreen, NavigationScreen.AnimType _animType) {
		// Enable button for the current screen
		if(m_currentScreenIdx != SCREEN_NONE) {
			m_tabButtons[m_currentScreenIdx].interactable = true;
		}

		// Let parent do the magic
		base.GoToScreen(_newScreen, _animType);

		// Disable button for newly selected screen
		if(m_currentScreenIdx != SCREEN_NONE) {
			m_tabButtons[m_currentScreenIdx].interactable = false;
		}
	}
}