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
	[Comment("Will only be used if the OnNavigationButtonClick listener is added to the button's OnClick event. Passing enum values as parameters for events is not possible in Unity, so we must do it this way.")]
	[SerializeField] private MenuScreensController.Screens m_targetScreen = MenuScreensController.Screens.NONE;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Go to the target screen.
	/// </summary>
	public void OnNavigationButtonClick() {
		// Find the menu screen navigation system - assume it's in a parent object for faster search
		NavigationScreenSystem navigationSystem = GetComponentInParent<NavigationScreenSystem>();
		if(navigationSystem == null) return;

		// Just go to target screen
		navigationSystem.GoToScreen((int)m_targetScreen);
	}

	/// <summary>
	/// Special callback for the final play button.
	/// </summary>
	public void OnStartGameClick() {
		// To be used only on the menu
		// Let the scene controller manage it
		InstanceManager.GetSceneController<MenuSceneController>().OnPlayButton();
	}
}