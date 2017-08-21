// BackButtonTrigger.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/08/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Individual handler for the back button
/// </summary>
public class BackButtonHandler : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Actions to be performed upon pressing back
	// [AOC] WARNING! Don't change the enum order, otherwise all serialized instances will point to wrong actions!
	public enum Action {
		IGNORE,		// The handler will be pushed to the stack, but will do nothing when pressing the button (not even remove itself from the stack!)

		POPUP_CLOSE,				// Requires a PopupController component
		POPUP_CLOSE_AND_DESTROY,	// Requires a PopupController component

		MENU_NAVIGATION_BACK,			// Requires a NavigationScreen component
		MENU_NAVIGATION_MAIN_SCREEN,	// Requires a NavigationScreen component

		GAME_PAUSE,		// Requires a GameSceneController component
		GAME_QUIT,		// Requires a GameSceneController component

		APPLICATION_EXIT,

		CUSTOM			// Subscribe to the OnBackButton event to perform a custom action
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed to the inspector
	[SerializeField] private Action m_action = Action.IGNORE;
	public Action action {
		get { return m_action; }
	}

	// Events
	public UnityEvent OnBackButton = new UnityEvent();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Do some initialization based on type
		switch(m_action) {
			case Action.POPUP_CLOSE:
			case Action.POPUP_CLOSE_AND_DESTROY: {
				PopupController popup = GetComponent<PopupController>();
				Debug.Assert(popup != null, m_action + " requires a PopupController component!");
				popup.OnOpenPreAnimation.AddListener(Register);
				popup.OnClosePostAnimation.AddListener(Unregister);
			} break;

			case Action.MENU_NAVIGATION_BACK:
			case Action.MENU_NAVIGATION_MAIN_SCREEN: {
				NavigationScreen screen = GetComponent<NavigationScreen>();
				Debug.Assert(screen != null, m_action + " requires a NavigationScreen component!");
				screen.OnShow.AddListener(Register);
				screen.OnHide.AddListener(Unregister);
			} break;
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Register to the manager, on top of the stack.
	/// </summary>
	public void Register() {
		
	}

	/// <summary>
	/// Unregister from the manager, regardless of the position it's stacked.
	/// </summary>
	public void Unregister() {

	}

	/// <summary>
	/// Perform the defined action on to this handler.
	/// To be called by the manager.
	/// </summary>
	public void Trigger() {

	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}