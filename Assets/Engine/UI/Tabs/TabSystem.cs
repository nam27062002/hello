// TabSystem.cs
// 
// Created by Alger Ortín Castellví on 26/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using InControl;
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
	[SerializeField] public List<SelectableButton> m_tabButtons = new List<SelectableButton>();

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
		Tab t = null;
		for(int i = 0; i < m_screens.Count; i++) {
			// Aux vars
			t = GetTab(i);

			// Button is disabled if tab is not enabled
			if(!t.tabEnabled) {
				m_tabButtons[i].button.interactable = t.tabEnabled;
			} else {
				// Button is selected for the active screen
				m_tabButtons[i].SetSelected(t.gameObject.activeSelf);
			}

			// Link button to that screen
			int screenIdx = i;	// Issue with lambda expressions and iterations, see http://answers.unity3d.com/questions/791573/46-ui-how-to-apply-onclick-handler-for-button-gene.html
			m_tabButtons[i].button.onClick.AddListener(() => GoToScreen(screenIdx));	// Lambda expression, see https://msdn.microsoft.com/en-us/library/bb397687.aspx
		}
	}

    private void Update() {
        InputDevice device = InputManager.ActiveDevice;

        if (device != null) {
            if (device.RightBumper.WasPressed) {
                if (m_currentScreenIdx < 0) {
                    GoToScreen(0);
                } else {
                    GoToScreen((m_currentScreenIdx + 1) % m_tabButtons.Count);
                }
            } else if (device.LeftBumper.WasPressed) {
                if (m_currentScreenIdx < 0) {
                    GoToScreen(0);
                } else {
                    GoToScreen((m_currentScreenIdx + m_tabButtons.Count - 1) % m_tabButtons.Count);
                }
            }
        }
    }

    //------------------------------------------------------------------//
    // OTHER METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Casted GetScreen().
    /// </summary>
    /// <returns>The tab at the given index. Null if index out of bounds.</returns>
    /// <param name="_tabIdx">Target tab index.</param>
    public Tab GetTab(int _tabIdx) {
		return GetScreen(_tabIdx) as Tab;
	}

	/// <summary>
	/// Allow the tab system to navigate to the target tab or not.
	/// </summary>
	/// <param name="_tabIdx">Tab index.</param>
	/// <param name="_enabled">Whether to allow navigating to that tab or not.</param>
	public void SetTabEnabled(int _tabIdx, bool _enabled) {
		// Get target tab
		Tab t = GetTab(_tabIdx);
		if(t == null) return;

		// Set flag
		t.tabEnabled = _enabled;

		// Refresh associated button
		// Do nothing if button is selected
		if(!m_tabButtons[_tabIdx].selected) {
			m_tabButtons[_tabIdx].button.interactable = _enabled;
		}
	}

	/// <summary>
	/// Add a tab to the system.
	/// No checks will be performed, use at your own risk.
	/// </summary>
	/// <param name="_idx">New tab's index.</param>
	/// <param name="_btn">Button linked to this tab.</param>
	/// <param name="_tab">The tab to be added.</param>
	public void AddTab(int _idx, SelectableButton _btn, Tab _tab) {
		m_tabButtons.Insert(_idx, _btn);
		AddScreen(_idx, _tab);
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
		// Don't do it if target tab is not enabled
		if(_newScreen != SCREEN_NONE && _newScreen >= 0 && _newScreen < m_screens.Count) {
			if(!GetTab(_newScreen).tabEnabled) return;
		}

		// Unselect button for the current screen
		if(m_currentScreenIdx != SCREEN_NONE) {
			// Button is disabled if tab is not enabled
			m_tabButtons[m_currentScreenIdx].SetSelected(false, !GetTab(m_currentScreenIdx).tabEnabled);
		}

		// Let parent do the magic
		base.GoToScreen(_newScreen, _animType);

		// Select button for newly selected screen
		if(m_currentScreenIdx != SCREEN_NONE) {
			m_tabButtons[m_currentScreenIdx].SetSelected(true);
		}
	}
}