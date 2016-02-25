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
/// Adds some specific behaviour to the main menu screen navigator.
/// </summary>
public class MenuScreensController : NavigationScreenSystem {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Screens {
		NONE = -1,
		PLAY,
		DRAGON_SELECTION,
		LEVEL_SELECTION,
		INCUBATOR,

		COUNT
	};

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[Space()]
	[Comment("There should always be one camera snap point per screen, value can be null")]
	[SerializeField] private CameraSnapPoint[] m_screensCameraSnapPoints = new CameraSnapPoint[(int)Screens.COUNT];

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
		/*if(GameVars.playScreenShown) {
			SetInitialScreen((int)Screens.DRAGON_SELECTION);
		} else {
			SetInitialScreen((int)Screens.PLAY);
		}*/
	}

	/// <summary>
	/// First update call.
	/// </summary>
	override protected void Start() {
		// Let parend do its stuff
		base.Start();

		// Instantly move camera to initial screen snap point
		if(m_camera != null
		&& m_currentScreenIdx != SCREEN_NONE
		&& m_screensCameraSnapPoints[m_currentScreenIdx] != null) {
			m_screensCameraSnapPoints[m_currentScreenIdx].Apply(m_camera);
		}
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
		if(m_camera != null
		&& m_currentScreenIdx != SCREEN_NONE
		&& m_screensCameraSnapPoints[m_currentScreenIdx] != null) {
			// Perform camera transition!
			// Camera snap point makes it easy for us! ^_^
			TweenParams tweenParams = new TweenParams().SetEase(Ease.InOutCirc);
			tweenParams.OnComplete(OnCameraTweenCompleted);
			m_screensCameraSnapPoints[m_currentScreenIdx].TweenTo(m_camera, 0.5f, tweenParams); 
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