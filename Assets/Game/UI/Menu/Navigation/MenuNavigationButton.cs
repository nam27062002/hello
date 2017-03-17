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
	[SerializeField] protected MenuScreens m_targetScreen = MenuScreens.NONE;

	// Internal References
	protected MenuScreensController m_navigationSystem = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	protected void Start() {
		// Get a reference to the navigation system, which in this particular case should be a component in the menu scene controller
		m_navigationSystem = InstanceManager.sceneController.GetComponent<MenuScreensController>();
		Debug.Assert(m_navigationSystem != null, "Required component missing!");
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Go to the target screen.
	/// </summary>
	public void OnNavigationButton() {
		// Just go to target screen
		m_navigationSystem.GoToScreen((int)m_targetScreen);
	}

	/// <summary>
	/// Go to the previous screen, if any.
	/// </summary>
	public void OnBackButton() {
		// If history is empty, go to default screen
		if(m_navigationSystem.screenHistory.Count == 0) {
			OnNavigationButton();
		} else {
			m_navigationSystem.Back();
		}
	}

	/// <summary>
	/// Special callback for the final play button.
	/// </summary>
	public void OnStartGameButton() {
		// To be used only on the menu
		// Let the scene controller manage it
		InstanceManager.menuSceneController.OnPlayButton();
	}
}