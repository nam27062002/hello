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
using System.Collections;

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
	[InfoBox("- Will only be used if the OnNavigationButton listener is added to the button's OnClick event.\n" +
		"- Passing enum values as parameters for events is not possible in Unity, so we must do it this way.\n" +
		"- For the OnBackButton callback, this is the screen to default if there is no previous screen in the navigation history.")]
	[SerializeField] protected MenuScreen m_targetScreen = MenuScreen.NONE;

	// Internal References
	protected MenuTransitionManager m_transitionManager = null;

    // Multitouch avoidment
    private static bool m_buttonMultitouchProtector = false;

    public static bool checkMultitouchAvailability()
    {
        if (m_buttonMultitouchProtector) return false;
        m_buttonMultitouchProtector = true;
        CoroutineManager.Instance.StartCoroutine(WaitAMoment(0.75f));
        return true;
    }

    static IEnumerator WaitAMoment(float time)
    {
        // suspend execution for 5 seconds
        yield return new WaitForSeconds(time);
        m_buttonMultitouchProtector = false;
    }
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
        if (!checkMultitouchAvailability()) return;
		// Just go to target screen
		m_transitionManager.GoToScreen(m_targetScreen, true);
	}

	/// <summary>
	/// Go to the previous screen, if any.
	/// </summary>
	public void OnBackButton() {
        if (!checkMultitouchAvailability()) return;
        // If history is empty, go to default screen
        if (m_transitionManager.screenHistory.Count == 0) {
			OnNavigationButton();
		} else {
			m_transitionManager.Back(true);
		}
    }

    /// <summary>
    /// Special callback for the final play button.
    /// </summary>
    public void OnStartGameButton() {
        if (!checkMultitouchAvailability()) return;
        // To be used only on the menu
        // Let the scene controller manage it
        InstanceManager.menuSceneController.OnPlayButton();
    }
}