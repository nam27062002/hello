// NavigationScreenSystem.cs
// 
// Created by Alger Ortín Castellví on 03/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using System;
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

	// Auxiliar class to send multiple data with the event
	public class ScreenChangedEventData {
		public NavigationScreenSystem dispatcher = null;

		public NavigationScreen fromScreen = null;
		public int fromScreenIdx = SCREEN_NONE;

		public NavigationScreen toScreen = null;
		public int toScreenIdx = SCREEN_NONE;

		public bool animated = false;
	};

	// Events, subscribe as needed via inspector or code
	[Serializable] public class ScreenChangedEvent : UnityEvent<ScreenChangedEventData> { }
	[Serializable] public class ScreenIndexChangedEvent : UnityEvent<int> { }

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Screen references to be set from the inspector
	[SerializeField] protected NavigationScreen m_initialScreen = null;
	public NavigationScreen initialScreen {
		get { return m_initialScreen; }
		set { m_initialScreen = value; }
	}

	[SerializeField] protected List<NavigationScreen> m_screens = new List<NavigationScreen>();
	public List<NavigationScreen> screens {
		get { return m_screens; }
	}

	// Exposed events
	public ScreenChangedEvent OnScreenChanged = new ScreenChangedEvent();
	public ScreenIndexChangedEvent OnScreenIndexChanged = new ScreenIndexChangedEvent();

	// Navigation logic
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
	/// First update call.
	/// </summary>
	virtual protected void Start() {
		// Initial screen is visible from the beginning
		// Override if a screen change was demanded before the Start() method.
		NavigationScreen targetInitialScreen = m_initialScreen;
		if(currentScreen != null) {
			targetInitialScreen = currentScreen;
		}

		// Show initial screen, hide the rest
		bool initialScreenSet = false;
		for(int i = 0; i < m_screens.Count; i++) {
			if(m_screens[i] == targetInitialScreen && !initialScreenSet) {
				m_currentScreenIdx = i;
				m_screens[i].Show(NavigationScreen.AnimType.NONE);
				initialScreenSet = true;

				// Notify game!
				ScreenChangedEventData evt = new ScreenChangedEventData();
				evt.dispatcher = this;
				evt.fromScreen = null;
				evt.fromScreenIdx = SCREEN_NONE;
				evt.toScreen = currentScreen;
				evt.toScreenIdx = i;
				evt.animated = false;
				Messenger.Broadcast<ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, evt);

				OnScreenChanged.Invoke(evt);
				OnScreenIndexChanged.Invoke(i);
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
	/// Navigate to the target screen without animation.
	/// Use an int to be able to directly connect buttons to it.
	/// </summary>
	/// <param name="_newScreenIdx">The index of the new screen to go to. Use -1 for NONE.</param>
	virtual public void GoToScreenInstant(int _newScreenIdx) {
		GoToScreen(_newScreenIdx, NavigationScreen.AnimType.NONE);
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

		// Aux vars
		NavigationScreen currentScreen = (m_currentScreenIdx == SCREEN_NONE) ? null : m_screens[m_currentScreenIdx];
		NavigationScreen newScreen = (_newScreenIdx == SCREEN_NONE) ? null : m_screens[_newScreenIdx];
		
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
				// Don't add to history if going back to this screen is not allowed
				if(currentScreen != null && currentScreen.allowBackToThisScreen) {
					m_screenHistory.Add(m_currentScreenIdx);
				}
			}
		}

		// Hide current screen (if any)
		if(currentScreen != null) {
			currentScreen.Hide(_animType);
		}
		
		// Show new screen (if any)
		if(newScreen != null) {
			newScreen.Show(_animType);
		}

		// Update screen tracking
		int oldScreenIdx = m_currentScreenIdx;
		m_currentScreenIdx = _newScreenIdx;

		// Notify game!
		ScreenChangedEventData evt = new ScreenChangedEventData();
		evt.dispatcher = this;
		evt.fromScreen = currentScreen;
		evt.fromScreenIdx = oldScreenIdx;
		evt.toScreen = newScreen;
		evt.toScreenIdx = _newScreenIdx;
		evt.animated = _animType != NavigationScreen.AnimType.NONE;
		Messenger.Broadcast<ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, evt);

		OnScreenChanged.Invoke(evt);
		OnScreenIndexChanged.Invoke(_newScreenIdx);
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