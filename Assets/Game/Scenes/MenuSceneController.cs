// MenuSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Main controller for the menu scene.
/// </summary>
public class MenuSceneController : SceneController {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string NAME = "SC_Menu";

	// Order is relevant! Will define animations
	public enum Screen {
		NONE = -1,

		MAIN,
		DRAGON_SELECTION,
		LEVEL_SELECTION,

		COUNT
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Screen references to be set from the inspector
	[Space(10)]
	[Comment("Required")]
	[SerializeField] private MenuScreen m_mainScreen = null;
	[SerializeField] private MenuScreen m_dragonSelectionScreen = null;
	[SerializeField] private MenuScreen m_levelSelectionScreen = null;

	[Space(10)]
	[HideEnumValues(false, true)]
	[SerializeField] private Screen m_initialScreen = Screen.MAIN;

	// Screen navigation helpers
	private MenuScreen[] m_screens = null;
	private Screen m_currentScreen = Screen.NONE;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Call parent
		base.Awake();

		// Check required members
		DebugUtils.Assert(m_mainScreen != null, "Required field!");
		DebugUtils.Assert(m_dragonSelectionScreen != null, "Required field!");
		DebugUtils.Assert(m_levelSelectionScreen != null, "Required field!");

		// Init screens array
		m_screens = new MenuScreen[] {
			m_mainScreen, 
			m_dragonSelectionScreen, 
			m_levelSelectionScreen
		};

		// Initial screen is visible from the beginning
		m_currentScreen = m_initialScreen;
		for(int i = 0; i < m_screens.Length; i++) {
			bool initialScreen = (i == (int)m_currentScreen);
			m_screens[i].gameObject.SetActive(initialScreen);
		}
	}

	/// <summary>
	/// First update.
	/// </summary>
	void Start() {

	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
		// Call parent
		base.OnDestroy();
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
		if(_newScreen == (int)m_currentScreen) return;

		// Translate int to Screen
		Screen newScreen = Screen.NONE;
		if(_newScreen >= 0 && _newScreen < (int)Screen.COUNT) {
			newScreen = (Screen)_newScreen;
		}

		// Figure out animation direction based on current and new screen indexes
		MenuScreen.AnimDir dir = MenuScreen.AnimDir.NEUTRAL;
		if(m_currentScreen != Screen.NONE && newScreen != Screen.NONE) {
			if(m_currentScreen > newScreen) {
				dir = MenuScreen.AnimDir.BACK;
			} else {
				dir = MenuScreen.AnimDir.FORWARD;
			}
		}

		// Hide current screen (if any)
		if(m_currentScreen != Screen.NONE) {
			m_screens[(int)m_currentScreen].Hide(dir);
		}

		// Show new screen (if any)
		if(newScreen != Screen.NONE) {
			m_screens[(int)newScreen].Show(dir);
		}

		// Update screen tracking
		m_currentScreen = newScreen;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Play button has been pressed.
	/// </summary>
	public void OnPlayButton() {
		// Hide current screen
		GoToScreen((int)Screen.NONE);

		// Go to game!
		// [AOC] No need to block the button, the GameFlow already controls spamming
		FlowManager.GoToGame();
	}
}

