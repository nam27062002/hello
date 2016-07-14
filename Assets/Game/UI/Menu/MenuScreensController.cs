// MenuScreensController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Screens enumerator, make it global because it's used a lot and we want to keep a shorter notation.
/// </summary>
public enum MenuScreens {
	NONE = -1,
	PLAY,
	DRAGON_SELECTION,
	LEVEL_SELECTION,
	INCUBATOR,
	OPEN_EGG,
	EQUIPMENT,

	COUNT
};

/// <summary>
/// Adds some specific behaviour to the main menu screen navigator.
/// </summary>
public class MenuScreensController : NavigationScreenSystem {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[Space()]
	[Comment("There should always be one scene entry per screen, value can be null")]
	[SerializeField] private MenuScreenScene[] m_scenes = new MenuScreenScene[(int)MenuScreens.COUNT];
	public MenuScreenScene[] scenes {
		get { return m_scenes; }
	}

	[Space()]
	[SerializeField] private Camera m_camera = null;
	public Camera camera { 
		get { return m_camera; }
	}

	// Use it to track actual screen changes
	private bool m_tweening = false;
	public bool tweening {
		get { return m_tweening; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Just define initial screen on the navigation system before it actually starts
		if(GameVars.menuInitialScreen != MenuScreens.NONE) {
			// Forced initial screen
			SetInitialScreen((int)GameVars.menuInitialScreen);
			GameVars.menuInitialScreen = MenuScreens.NONE;
		} else if(GameVars.playScreenShown) {
			SetInitialScreen((int)MenuScreens.DRAGON_SELECTION);
		} else {
			SetInitialScreen((int)MenuScreens.PLAY);
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	override protected void Start() {
		// Let parend do its stuff
		base.Start();

		// Instantly move camera to initial screen snap point
		MenuScreenScene targetScene = GetScene(m_currentScreenIdx);
		if(targetScene != null
		&& targetScene.cameraSnapPoint != null
		&& m_camera != null) {
			targetScene.cameraSnapPoint.Apply(m_camera);
		}
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Get the 3D scene linked to a given screen index.
	/// </summary>
	/// <returns>The 3D scene object linked to the screen with index <paramref name="_screenIdx"/>.<c>null</c> if not defined or provided index not valid.</returns>
	/// <param name="_screenIdx">The index of the screen whose linked 3D scene we want.</param>
	public MenuScreenScene GetScene(int _screenIdx) {
		if(_screenIdx < 0 || _screenIdx >= (int)MenuScreens.COUNT) return null;
		return m_scenes[_screenIdx];
	}

	//------------------------------------------------------------------//
	// NavigationScreenSystem OVERRIDES									//
	//------------------------------------------------------------------//
	/// <summary>
	/// Navigate to the target screen. Use an int to be able to directly connect buttons to it.
	/// </summary>
	/// <param name="_newScreenIdx">The index of the new screen to go to. Use -1 for NONE.</param>
	/// <param name="_animType">Optionally force the direction of the animation.</param>
	override public void GoToScreen(int _newScreenIdx, NavigationScreen.AnimType _animType) {
		// Store current screen
		int oldScreenIdx = m_currentScreenIdx;

		// Let parent do its stuff
		base.GoToScreen(_newScreenIdx, _animType);

		// Perform camera transition (if snap point defined)
		MenuScreenScene targetScene = GetScene(_newScreenIdx);
		if(targetScene != null
		&& targetScene.cameraSnapPoint != null
		&& m_camera != null) {
			// Perform camera transition!
			// Camera snap point makes it easy for us! ^_^
			TweenParams tweenParams = new TweenParams().SetEase(Ease.InOutCubic);
			tweenParams.OnComplete(OnCameraTweenCompleted);
			targetScene.cameraSnapPoint.TweenTo(m_camera, 0.5f, tweenParams); 
			m_tweening = true;
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A camera transition animation has been completed.
	/// Use it to track actual screen changes.
	/// </summary>
	private void OnCameraTweenCompleted() {
		m_tweening = false;
	}
}