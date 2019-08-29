// MenuTransitionManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar class representing a transition between 2 menu screens.
/// </summary>
[System.Serializable]
public class Transition {
	[HideEnumValues(true, true)] public MenuScreen destination = MenuScreen.NONE;

	[Tooltip("Will override any other setup")]
	public bool showOverlay = false;

	public BezierCurve path = null;
	public string initialPathPoint = "";
	public string finalPathPoint = "";

	public bool overrideDuration = false;
	public float duration = 0.5f;

	public bool overrideEase = false;
	public Ease ease = Ease.InOutCubic;
}

/// <summary>
/// 
/// </summary>
[System.Serializable]
public class ScreenData {
	[HideInInspector] public MenuScreen screenId = MenuScreen.NONE;
	public NavigationScreen ui = null;
	public MenuScreenScene scene3d = null;
	public CameraSnapPoint cameraSetup = null;
	public Transition[] transitions = new Transition[0];
}

/// <summary>
/// All transitions between menu screens must go through this class.
/// Specialization of the NavigationScreenSystem class, adapted to the Hungry Dragon menu needs.
/// </summary>
public class MenuTransitionManager : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Safety time period between transitions (to avoid breaking the UI if tapping 2 buttons for example)
	private const float TRANSITION_SAFETY_PERIOD = 0.5f;
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Separator("Scene References")]
	[SerializeField] private Camera m_camera = null;
	new public Camera camera {
		get { return m_camera; }
	}

	[SerializeField] private BezierCurve m_dynamicPath = null;
	public BezierCurve dynamicPath {
		get { return m_dynamicPath; }
	}

	[SerializeField] private MenuTransitionOverlay m_overlay = null;

	[Separator("Default Transition Setup")]
	[SerializeField] private float m_defaultTransitionDuration = 0.5f;
	[SerializeField] private Ease m_defaultTransitionEase = Ease.InOutCubic;
	[SerializeField] 
	[Range(0f, 1f)] 
	[Tooltip("How much the dynamic path will respect the original curve shape " +
		"when the initial camera position is not in the path.\n" +
		"The more strength, the less the original curve is respected.")]
	private float m_dynamicPathStrength = 1f;

	[Separator("Transition Definitions")]
	[SerializeField] private ScreenData[] m_screens = new ScreenData[(int)MenuScreen.COUNT];
	public ScreenData[] screens {
		get { return m_screens; }
	}

	// Screen navigation history
	private List<MenuScreen> m_screenHistory = new List<MenuScreen>();  // Used to implement the Back() functionality
	public List<MenuScreen> screenHistory {
		get { return m_screenHistory; }
	}

	private MenuScreen m_prevScreen = MenuScreen.NONE;
	public MenuScreen prevScreen {
		get { return m_prevScreen; }
	}
	private MenuScreen m_currentScreen = MenuScreen.NONE;
	public MenuScreen currentScreen {
		get { return m_currentScreen; }
	}

	public ScreenData currentScreenData {
		get { return GetScreenData(m_currentScreen); }
	}

	// Camera animation control
	private Tweener m_cameraTween = null;
	private float m_cameraTweenDelta = 0f;

	// Transition protection
	private bool m_transitionAllowed = true;
	public bool transitionAllowed {
		get { return m_transitionAllowed; }
	}

	private bool m_isTransitioning = false;
	public bool isTransitioning {
		get { return m_isTransitioning; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Activate all screens during one single frame to make sure everything is properly initialized
		// Target screen will be selected on the Start() call
		for(int i = 0; i < m_screens.Length; ++i) {
			// Main screen
			m_screens[i].ui.gameObject.SetActive(true);
		}

		// Activate transition overlay, usually disabled for editing
		if(m_overlay != null) {
			m_overlay.gameObject.SetActive(true);
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Hide all screens
		for(int i = 0; i < m_screens.Length; ++i) {
			if ( (int)m_currentScreen != i )
				m_screens[i].ui.Hide(NavigationScreen.AnimType.NONE);
		}

		// Set initial screen
		if ( m_currentScreen == MenuScreen.NONE ){
			if(GameVars.menuInitialScreen != MenuScreen.NONE) {
				GoToScreen(GameVars.menuInitialScreen, false);	// Forced initial screen
				GameVars.menuInitialScreen = MenuScreen.NONE;
			} else if(GameVars.playScreenShown) {
				GoToScreen(MenuScreen.DRAGON_SELECTION, false);
			} else {
				GoToScreen(MenuScreen.PLAY, false);
			}
		}

		// Make sure overlay is hidden as well
		m_overlay.Stop();
	}

	//------------------------------------------------------------------------//
	// NAVIGATION METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Sets the current screen.
	/// </summary>
	/// <param name="_targetScreen">Target screen.</param>
	/// <param name="_animate">Whether to animate or do it instantly.</param>
	/// <param name="_forceTransition">Top priority, allows to interrupt an ongoing transition.</param>
	/// <param name="_allowBack">If set to <c>false</c>, overrides the current screen's setup and doesn't store it to the navigation history.</param>
	public void GoToScreen(MenuScreen _targetScreen, bool _animate, bool _forceTransition = false, bool _allowBack = true) {
		Debug.Log("Changing screen from " + Colors.coral.Tag(m_currentScreen.ToString()) + " to " + Colors.aqua.Tag(_targetScreen.ToString()));

		// Block if transitions are not allowed at this moment
        if(!m_transitionAllowed && !_forceTransition) {
			Debug.Log(Color.red.Tag("BLOCKED"));
			return;
		}

		// Ignore if screen is already active
		if(_targetScreen == m_currentScreen) return;

		// Notify game a screen transition is about to happen
		Messenger.Broadcast<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_REQUESTED, m_currentScreen, _targetScreen);

		// Aux vars
		ScreenData fromScreenData = GetScreenData(m_currentScreen);
		ScreenData toScreenData = GetScreenData(_targetScreen);

		// Update screen history
		if(m_currentScreen != MenuScreen.NONE && _targetScreen != MenuScreen.NONE) {
			MenuScreen lastScreen = MenuScreen.NONE;
			if(m_screenHistory.Count > 0) lastScreen = m_screenHistory.Last();
			if(lastScreen == _targetScreen) {
				// Going back to previous screen!
				m_screenHistory.RemoveAt(m_screenHistory.Count - 1);
			} else {
				// Moving forward to a new screen!
				// Don't add to history if going back to this screen is not allowed
				// Don't add also if the caller had explicitely requested so
				if(fromScreenData != null && fromScreenData.ui.allowBackToThisScreen && _allowBack) {
					m_screenHistory.Add(m_currentScreen);
				}
			} 
		}

		// Store new screen and previous one
		m_prevScreen = m_currentScreen;
		m_currentScreen = _targetScreen;

		// Notify game a screen transition has just happen and animation is about to start
		Messenger.Broadcast<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, m_prevScreen, m_currentScreen);
		m_isTransitioning = true;

		// Prevent any transition during a safety period (to avoid breaking the UI if tapping 2 buttons for example)
		m_transitionAllowed = false;
		UbiBCN.CoroutineManager.DelayedCall(OnTransitionSafetyPeriodFinished, TRANSITION_SAFETY_PERIOD);

		// Perform transition
		// Do we have a valid transition data from current screen to target screen?
		Transition t = FindTransition(m_prevScreen, _targetScreen);
		if(t == null) Debug.Log(Color.red.Tag("Screen transition not defined! " + m_prevScreen + " -> " + m_currentScreen));
		if(t != null && _animate) {
			// Yes! Use it
			// Get some aux vars first
			float duration = t.overrideDuration ? t.duration : m_defaultTransitionDuration;
			Ease ease = t.overrideEase ? t.ease : m_defaultTransitionEase;

			// If using the overlay, override duration
			if(t.showOverlay) duration = m_overlay.transitionDuration;

			// UI
			PerformUITransition(fromScreenData, toScreenData, duration);

			// Camera
			PerformCameraTransition(fromScreenData, toScreenData, t, duration, ease);

			// Overlay
			if(t.showOverlay) {
				m_overlay.Play();
			}

			// Lock input to prevent weird flow cases when interrupting a screen transition
			// See https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-620
			InputLocker.Lock();

			// Program transition end callback
			UbiBCN.CoroutineManager.DelayedCall(OnTransitionFinished, duration, false);
		} else {
			// UI
			PerformUITransition(fromScreenData, toScreenData);

			// Camera
			PerformCameraTransition(fromScreenData, toScreenData);

			// No animation, instantly notify game the screen transition has been completed
			m_isTransitioning = false;
			Messenger.Broadcast<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, m_prevScreen, m_currentScreen);
		}
	}

	/// <summary>
	/// Navigate to the previous screen (if any).
	/// </summary>
	/// <param name="_animate">Whether to animate or do it instantly.</param>
	public void Back(bool _animate) {
		if(m_screenHistory.Count == 0) return;
		GoToScreen(m_screenHistory.Last(), _animate);
	}

	/// <summary>
	/// Get the setup for a specific scrreen.
	/// </summary>
	/// <returns>The screen data corresponding to the requested screen.</returns>
	/// <param name="_scr">Screen whose data we want.</param>
	public ScreenData GetScreenData(MenuScreen _scr) {
		// Check index (for MenuScreen.NONE and MenuScreen.COUNT)
		int idx = (int)_scr;
		if(idx < 0 || idx > m_screens.Length) return null;

		return m_screens[(int)_scr];
	}

	//------------------------------------------------------------------------//
	// GETTERS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Find a transition between two screens.
	/// </summary>
	/// <returns>The transition. <c>null</c> if no transition defined between both screens.</returns>
	/// <param name="_from">Source screen.</param>
	/// <param name="_to">Destination screen.</param>
	public Transition FindTransition(MenuScreen _from, MenuScreen _to) {
		// Get screen data
		ScreenData fromData = GetScreenData(_from);
		if(fromData == null) return null;

		// Iterate existing transitions looking for the one going to the target screen
		for(int i = 0; i < fromData.transitions.Length; ++i) {
			if(fromData.transitions[i].destination == _to) {
				return fromData.transitions[i];
			}
		}

		// No valid transition was found!
		return null;
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Performs a camera transition with the given parameters.
	/// </summary>
	/// <param name="_fromScreenData">Data of the screen we're moving from. Can be null.</param>
	/// <param name="_toScreenData">Data of the screen we're moving to.</param>
	/// <param name="_duration">Duration, negative for no animation.</param>
	private void PerformUITransition(ScreenData _fromScreenData, ScreenData _toScreenData, float _duration = -1f) {
		// [AOC] TODO!! Use duration

		// Animated transition?
		NavigationScreen.AnimType animType = (_duration > 0f) ? NavigationScreen.AnimType.AUTO : NavigationScreen.AnimType.NONE;

		// Hide current screen
		if(_fromScreenData != null) {
			_fromScreenData.ui.Hide(animType);
		}

		// Show target screen
		if(_toScreenData != null) {
			_toScreenData.ui.Show(animType);
		}
	}

	/// <summary>
	/// Performs a camera transition with the given parameters.
	/// </summary>
	/// <param name="_fromScreenData">Data of the screen we're moving from. Can be null.</param>
	/// <param name="_toScreenData">Data of the screen we're moving to.</param>
	/// <param name="_t">Transition data, can be null (transition will be instant).</param>
	/// <param name="_duration">Duration, negative for no animation.</param>
	/// <param name="_ease">Ease function.</param>
	private void PerformCameraTransition(ScreenData _fromScreenData, ScreenData _toScreenData, Transition _t = null, float _duration = -1f, Ease _ease = Ease.InOutCubic) {
		// A) Using overlay: instant camera swap after some delay
		if(_t != null && _t.showOverlay) {
			UbiBCN.CoroutineManager.DelayedCall(
				() => {
					_toScreenData.cameraSetup.changePosition = true;
					_toScreenData.cameraSetup.Apply(m_camera);
				}, _duration / 2f	// Right at the middle of the overlay transition so we don't see the swap!
			);
		}

		// B) Duration <= 0 or no transition defined: Instant camera change
		else if(_t == null || _duration <= 0f) {
			// No! Go straight to the new screen
			_toScreenData.cameraSetup.changePosition = true;
			_toScreenData.cameraSetup.Apply(m_camera);
		}

		// C) Normal, animated transition
		else {
			// Lerp path (if defined)
			bool usePath = _t.path != null;
			if(usePath) {
				// Dynamic path will copy the original transition path and then 
				// lerp it with the current camera position replacing the first point

				// Aux vars
				BezierPoint refP = null;
				BezierPoint newP = null;
				int initialPoint = _t.path.GetPointIdx(_t.initialPathPoint);
				int finalPoint = _t.path.GetPointIdx(_t.finalPathPoint);

				// Prevent snap point to change the position as well (the dynamic path will control it instead)
				_toScreenData.cameraSetup.changePosition = false;

				// If animating, kill tween
				if(m_cameraTween != null) {
					m_cameraTween.Kill();
				}

				// 1. Clear path
				m_dynamicPath.transform.position = _t.path.transform.position;	// Points are cloned, so we need parent path to be at the same position!
				m_dynamicPath.Clear();

				// 2. Compute curve offset to be able to lerp the curve points, 
				//	  respecting curve shape but using current camera position as start point
				refP = _t.path.GetPoint(initialPoint);
				Vector3 offset = m_camera.transform.position - refP.globalPosition;
				Vector3 correctedOffset = offset;

				// Compute deltas to be able to properly do the interpolation
				float delta = 0f;
				float initialDelta = _t.path.GetDelta(initialPoint);
				float finalDelta = _t.path.GetDelta(finalPoint);

				// 3. Clone points from the transition curve, respecting the order
				//	  Use the loop to compute the interpolation as well
				int pointsToProcess = Mathf.Abs(initialPoint - finalPoint) + 1;
				int i = initialPoint;
				for(int processedPoints = 0; processedPoints < pointsToProcess; ++processedPoints) {
					// Create new point
					refP = _t.path.GetPoint(i);
					newP = new BezierPoint(refP);
					m_dynamicPath.AddPoint(newP);

					// Interpolate using initial point offset and delta
					delta = Mathf.InverseLerp(initialDelta, finalDelta, _t.path.GetDelta(i));
					correctedOffset = offset * (1f - delta);	// The closer we get to the final point, the less offset we apply
					if(i != initialPoint && i != finalPoint) {
						correctedOffset *= m_dynamicPathStrength;	// How much do we respect original curve?
					}
					newP.globalPosition = refP.globalPosition + correctedOffset;

					// Going forwards or backwards?
					if(finalPoint > initialPoint) {
						++i;
					} else {
						--i;
					}
				}

				// 4. Make sure curve is updated
				m_dynamicPath.ForceUpdate();

				// 5. Launch camera animation!
				m_cameraTweenDelta = 0f;
				m_cameraTween = DOTween.To(OnCameraTweenGetValue, OnCameraTweenSetValue, 1f, _duration)
					.SetEase(_ease)
					.SetAutoKill(true)
					.SetTarget(m_camera)
					.OnComplete(OnCameraTweenCompleted);
			}

			// Path not defined, lerp position
			else {
				_toScreenData.cameraSetup.changePosition = true;
			}

			// Camera rotation and properties will just be lerped using the snap points
			TweenParams tweenParams = new TweenParams().SetEase(_ease);
			_toScreenData.cameraSetup.TweenTo(m_camera, _duration, tweenParams);

			// Notify game a camera transition is about to start!
			Messenger.Broadcast<MenuScreen, MenuScreen, bool>(MessengerEvents.MENU_CAMERA_TRANSITION_START, m_prevScreen, m_currentScreen, usePath);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Camera tween getter.
	/// </summary>
	private float OnCameraTweenGetValue() {
		return m_cameraTweenDelta;
	}

	/// <summary>
	/// Camera tween setter.
	/// </summary>
	/// <param name="_newValue">New value coming from the tween.</param>
	private void OnCameraTweenSetValue(float _newValue) {
		m_camera.transform.position = m_dynamicPath.GetValue(_newValue);
		m_cameraTweenDelta = _newValue;
	}

	/// <summary>
	/// A camera transition animation has been completed.
	/// Use it to track actual screen changes.
	/// </summary>
	private void OnCameraTweenCompleted() {
		// Clear tween reference
		m_cameraTween = null;
	}

	/// <summary>
	/// Animated transition has finished.
	/// </summary>
	private void OnTransitionFinished() {
		// Transition finished, unlock input!
		InputLocker.Unlock();

		// [AOC] From time to time, the leaving screen remains active (although invisible), blocking the input from the actual current screen
		//		 Try to fix it the hardcore way
		ScreenData prevScreenData = GetScreenData(m_prevScreen);
		if(prevScreenData != null && prevScreenData.ui != null) {
			prevScreenData.ui.gameObject.SetActive(false);
		}

		// Notify game the screen transition has been completed
		m_isTransitioning = false;
		Messenger.Broadcast<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, m_prevScreen, m_currentScreen);
	}

	/// <summary>
	/// The safety period before allowing more transitions has finished.
	/// </summary>
	private void OnTransitionSafetyPeriodFinished() {
		// Transitions allowed again
		m_transitionAllowed = true;
	}
}