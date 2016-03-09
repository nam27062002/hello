// NavigationScreenSystem.cs
// 
// Created by Alger Ortín Castellví on 03/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Class to implement a screen navigation system to move between different ui screens.
/// </summary>
public class NavigationScreenSystem : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly int SCREEN_NONE = -1;

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Screen references to be set from the inspector
	[SerializeField] protected NavigationScreen m_initialScreen = null;
	public NavigationScreen initialScreen {
		get { return m_initialScreen; }
	}

	[SerializeField] protected List<NavigationScreen> m_screens = new List<NavigationScreen>();
	public List<NavigationScreen> screens {
		get { return m_screens; }
	}

	protected int m_currentScreenIdx = SCREEN_NONE;
	public int currentScreenIdx {
		get { return m_currentScreenIdx; }
	}

	public NavigationScreen currentScreen {
		get { return GetScreen(m_currentScreenIdx); }
	}

	private List<int> m_screenHistory = new List<int>();	// Used to decide the AUTO animation direction as well as to implement the Back() functionality
	public List<int> screenHistory {
		get { return m_screenHistory; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	virtual protected void Start() {
		// Initial screen is visible from the beginning
		for(int i = 0; i < m_screens.Count; i++) {
			if(m_screens[i] == m_initialScreen) {
				m_currentScreenIdx = i;
				m_screens[i].Show(NavigationScreen.AnimType.NONE);
			} else {
				m_screens[i].Hide(NavigationScreen.AnimType.NONE);
			}
		}
	}

	//------------------------------------------------------------------//
	// SCREEN GETTER													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Obtain the screen at the given index.
	/// </summary>
	/// <param name="_screenIdx">Index of the desired screen.</param>
	/// <returns>The screen with the given index. <c>null</c> if not found.</returns>
	public NavigationScreen GetScreen(int _screenIdx) {
		if(_screenIdx < 0 || _screenIdx >= m_screens.Count) return null;
		return m_screens[_screenIdx];
	}

	/// <summary>
	/// Obtain the screen with the given name.
	/// </summary>
	/// <param name="_screenName">Name of the desired screen.</param>
	/// <returns>The screen with the given name. <c>null</c> if not found.</returns>
	public NavigationScreen GetScreen(string _screenName) {
		// [AOC] Use native C# find function with lambda expression to define the match algorithm
		return m_screens.Find(_screen => _screen.name.Equals(_screenName));
	}

	/// <summary>
	/// Find the index within the system of the given screen.
	/// </summary>
	/// <param name="_screen">The screen whose index we want.</param>
	/// <returns>The index of the screen within the system. <c>-1</c> if the screen is not on the navigation system.</returns>
	public int GetScreenIndex(NavigationScreen _screen) {
		return m_screens.IndexOf(_screen);
	}

	//------------------------------------------------------------------//
	// SCREEN NAVIGATION												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Navigate to the target screen. Use an int to be able to directly connect buttons to it.
	/// </summary>
	/// <param name="_newScreenIdx">The index of the new screen to go to. Use -1 for NONE.</param>
	virtual public void GoToScreen(int _newScreenIdx) {
		GoToScreen(_newScreenIdx, NavigationScreen.AnimType.AUTO);
	}

	/// <summary>
	/// Navigate to the target screen. Use an int to be able to directly connect buttons to it.
	/// </summary>
	/// <param name="_newScreenIdx">The index of the new screen to go to. Use -1 for NONE.</param>
	/// <param name="_animType">Optionally force the direction of the animation.</param>
	virtual public void GoToScreen(int _newScreenIdx, NavigationScreen.AnimType _animType) {
		// Ignore if screen is already active
		if(_newScreenIdx == m_currentScreenIdx) return;
		
		// If out of bounds, go to none
		if(_newScreenIdx < 0 || _newScreenIdx >= m_screens.Count) {
			_newScreenIdx = SCREEN_NONE;
		}

		// Get last screen
		int lastScreenIdx = SCREEN_NONE;
		if(m_screenHistory.Count > 0) lastScreenIdx = m_screenHistory.Last();
		
		// Figure out animation direction based on current and new screen indexes
		if(_animType == NavigationScreen.AnimType.AUTO) {
			_animType = NavigationScreen.AnimType.NEUTRAL;
			if(m_currentScreenIdx != SCREEN_NONE && _newScreenIdx != SCREEN_NONE) {
				//if(m_currentScreenIdx > _newScreenIdx) {
				if(lastScreenIdx == _newScreenIdx) {
					// Going back to previous screen!
					_animType = NavigationScreen.AnimType.BACK;
				} else {
					// Moving forward to a new screen!
					_animType = NavigationScreen.AnimType.FORWARD;
				}
			}
		}

		// Update screen history
		if(m_currentScreenIdx != SCREEN_NONE && _newScreenIdx != SCREEN_NONE) {
			if(lastScreenIdx == _newScreenIdx) {
				// Going back to previous screen!
				m_screenHistory.RemoveAt(m_screenHistory.Count - 1);
			} else {
				// Moving forward to a new screen!
				m_screenHistory.Add(m_currentScreenIdx);
			}
		}

		// Hide current screen (if any)
		NavigationScreen currentScreen = null;
		if(m_currentScreenIdx != SCREEN_NONE) {
			currentScreen = m_screens[m_currentScreenIdx];
			currentScreen.Hide(_animType);
		}
		
		// Show new screen (if any)
		NavigationScreen newScreen = null;
		if(_newScreenIdx != SCREEN_NONE) {
			newScreen = m_screens[_newScreenIdx];
			newScreen.Show(_animType);
		}

		// Update screen tracking
		int oldScreenIdx = m_currentScreenIdx;
		m_currentScreenIdx = _newScreenIdx;

		// Notify game!
		Messenger.Broadcast<NavigationScreen, NavigationScreen, bool>(EngineEvents.NAVIGATION_SCREEN_CHANGED, currentScreen, newScreen, _animType != NavigationScreen.AnimType.NONE);
		Messenger.Broadcast<int, int, bool>(EngineEvents.NAVIGATION_SCREEN_CHANGED_INT, oldScreenIdx, _newScreenIdx, _animType != NavigationScreen.AnimType.NONE);
	}

	/// <summary>
	/// Navigate to the target screen. Screen must be added to the screens array, otherwise nothing will happen.
	/// </summary>
	/// <param name="_screen">The screen to go to.</param>
	public void GoToScreen(NavigationScreen _screen) {
		GoToScreen(_screen, NavigationScreen.AnimType.AUTO);
	}

	/// <summary>
	/// Navigate to the target screen. Screen must be added to the screens array, otherwise nothing will happen.
	/// </summary>
	/// <param name="_screen">The screen to go to.</param>
	/// <param name="_animType">Optionally force the direction of the animation.</param>
	public void GoToScreen(NavigationScreen _screen, NavigationScreen.AnimType _animType) {
		int idx = GetScreenIndex(_screen);
		if(idx >= 0) GoToScreen(idx, _animType);
	}

	/// <summary>
	/// Navigate to screen defined by the given name.
	/// If no screen with the given name is found, nothing will happen.
	/// </summary>
	/// <param name="_screenName">The name of the screen we want to go to.</param>
	public void GoToScreen(string _screenName) {
		GoToScreen(_screenName, NavigationScreen.AnimType.AUTO);
	}

	/// <summary>
	/// Navigate to screen defined by the given name.
	/// If no screen with the given name is found, nothing will happen.
	/// </summary>
	/// <param name="_screenName">The name of the screen we want to go to.</param>
	/// <param name="_animType">Optionally force the direction of the animation.</param>
	public void GoToScreen(string _screenName, NavigationScreen.AnimType _animType) {
		GoToScreen(GetScreen(_screenName), _animType);
	}

	/// <summary>
	/// Navigate to the previous screen (if any).
	/// </summary>
	/// <param name="_animType">Optionally force the direction of the animation.</param>
	public void Back(NavigationScreen.AnimType _animType = NavigationScreen.AnimType.BACK) {
		if(m_screenHistory.Count == 0) return;
		GoToScreen(m_screenHistory.Last(), _animType);
	}

	//------------------------------------------------------------------//
	// INITIAL SCREEN SETTER											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Define initial screen. Should be called before the start method is invoked.
	/// </summary>
	/// <param name="_screen">New initial screen. Nothing will change if screen is not on the system. </param>
	public void SetInitialScreen(NavigationScreen _screen) {
		// Make sure given screen is in the system
		if(GetScreenIndex(_screen) < 0) return;
		m_initialScreen = _screen;
	}

	/// <summary>
	/// Define initial screen. Should be called before the start method is invoked.
	/// </summary>
	/// <param name="_screenIdx">Index of the new initial screen. Nothing will change if not valid.</param>
	public void SetInitialScreen(int _screenIdx) {
		SetInitialScreen(GetScreen(_screenIdx));
	}

	/// <summary>
	/// Define initial screen. Should be called before the start method is invoked.
	/// </summary>
	/// <param name="_screenName">Name of the new initial screen. Nothing will change if the screen is not on the system.</param>
	public void SetInitialScreen(string _screenName) {
		SetInitialScreen(GetScreen(_screenName));
	}
}