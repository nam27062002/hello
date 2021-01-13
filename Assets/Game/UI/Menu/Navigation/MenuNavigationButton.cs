// MenuNavigationButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

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
/// Simple behaviour to be attached to buttons in the main menu.
/// Use it to define navigation between different screen prefabs.
/// Use this instead of the button event directly to avoid losing the references
/// when reverting prefabs (each menu screen and the navigation system are different prefabs).
/// </summary>
[RequireComponent(typeof(Button))]
public class MenuNavigationButton : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed
	[InfoBox("USAGE:\n" +
		"1. Define target screen\n" +
		"2. Add the OnNavigationButton or OnBackButton listeners to the button's OnClick event.\n" +
		"(Passing enum values as parameters for events is not possible in Unity, so we must do it this way.)\n\n" +
		"Target Screen Parameter:\n" +
		"- For the OnNavigationButton callback, this is the screen to go to.\n" +
		"- For the OnBackButton callback, this is the screen to go if there is no previous screen in the navigation history or the last screen is in the excluded list.\n\n")]
	[SerializeField] protected MenuScreen m_targetScreen = MenuScreen.NONE;
	[SerializeField] protected List<MenuScreen> m_excludedScreens = new List<MenuScreen>();

	// Internal References
	protected MenuTransitionManager m_transitionManager = null;

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//

    /// <summary>
    /// First update call.
    /// </summary>
    protected void Start() {
		// Get a reference to the navigation system, which in this particular case should be a component in the menu scene controller
		m_transitionManager = InstanceManager.menuSceneController.transitionManager;
		Debug.Assert(m_transitionManager != null, "Required component missing!");
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Go to the target screen.
	/// </summary>
	public void OnNavigationButton() {
		if (!ButtonExtended.checkMultitouchAvailability ())
			return;
		// Just go to target screen
		m_transitionManager.GoToScreen(m_targetScreen, true);
	}

	/// <summary>
	/// Go to the previous screen, if any.
	/// </summary>
	public void OnBackButton() {
		if (!ButtonExtended.checkMultitouchAvailability ())
			return;

        if (m_transitionManager != null) {
            // If history is empty, go to default screen
            if (m_transitionManager.screenHistory.Count == 0) {
				m_transitionManager.GoToScreen(m_targetScreen, true);
			} else {
				// Make sure that the last screen is not in the excluded list
				MenuScreen lastScreen = m_transitionManager.screenHistory.Last();
				if(m_excludedScreens.Contains(lastScreen)) {
					// Default to target scren
					m_transitionManager.GoToScreen(m_targetScreen, true);
				} else {
					// Good to go!
					m_transitionManager.Back(true);
				}
            }
        }
    }

    /// <summary>
    /// Special callback for the final play button.
    /// </summary>
    public void OnStartGameButton() {
		if (!ButtonExtended.checkMultitouchAvailability ())
			return;
        // To be used only on the menu
        // Let the scene controller manage it
        InstanceManager.menuSceneController.GoToGame();
    }
}