// MenuCameraController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace MenuCameraTest {
	/// <summary>
	/// 
	/// </summary>
	[System.Serializable]
	public class Transition {
		[HideEnumValues(true, true)] public Screen destination = Screen.NONE;

		public bool overrideDuration = false;
		public float duration = 0.5f;

		public bool overrideEase = false;
		public Ease ease = Ease.InOutCubic;
		[Space]
		public BezierCurve path = null;
		public string initialPathPoint = "";
		public string finalPathPoint = "";
	}

	/// <summary>
	/// 
	/// </summary>
	[System.Serializable]
	public class ScreenData {
		[HideInInspector] public Screen screenId = Screen.NONE;
		public CameraSnapPoint snapPoint = null;
		public Transition[] transitions = new Transition[0];
	}

	/// <summary>
	/// 
	/// </summary>
	public class MenuCameraController : MonoBehaviour {
		//------------------------------------------------------------------------//
		// CONSTANTS															  //
		//------------------------------------------------------------------------//
		
		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//
		[SerializeField] private Camera m_camera = null;
		[SerializeField] private BezierCurve m_dynamicPath = null;
		[Space]
		[SerializeField] private float m_defaultTransitionDuration = 0.5f;
		[SerializeField] private Ease m_defaultTransitionEase = Ease.InOutCubic;
		[SerializeField] 
		[Range(0f, 1f)] 
		[Tooltip("How much the dynamic path will respect the original curve shapes")]
		private float m_dynamicPathStrength = 1f;

		[Space]
		[SerializeField] private ScreenData[] m_screens = new ScreenData[(int)Screen.COUNT];

		[Separator]
		[SerializeField] private GameObject m_buttonTemplate = null;

		// Internal
		private Screen m_currentScreen = Screen.NONE;

		// Animation control
		private Tweener m_cameraTween = null;
		private float m_cameraTweenDelta = 0f;

		// Internal test stuff
		private Button[] m_buttons = null;
		
		//------------------------------------------------------------------------//
		// GENERIC METHODS														  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Initialization.
		/// </summary>
		private void Awake() {
			// Instantiate one button for each screen
			m_buttons = new Button[(int)Screen.COUNT];
			for(int i = 0; i < (int)Screen.COUNT; ++i) {
				// Create new instance
				GameObject newInstance = GameObject.Instantiate<GameObject>(m_buttonTemplate, m_buttonTemplate.transform.parent, false);

				// Set label
				newInstance.GetComponentInChildren<TextMeshProUGUI>().text = ((Screen)i).ToString();

				// Store button and connect event
				int screenIdx = i;	// Delegates and loops suck -_-
				m_buttons[i] = newInstance.GetComponentInChildren<Button>();
				m_buttons[i].onClick.AddListener(delegate { OnScreenButton(screenIdx); });
			}

			// Destroy template
			GameObject.Destroy(m_buttonTemplate);
			m_buttonTemplate = null;
		}

		/// <summary>
		/// First update call.
		/// </summary>
		private void Start() {
			// Set initial screen
			GoToScreen(Screen.PLAY, false);
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
		/// Sets the current screen.
		/// </summary>
		/// <param name="_scr">Target screen.</param>
		/// <param name="_animate">Whether to animate or do it instantly.</param>
		public void GoToScreen(Screen _scr, bool _animate) {
			Debug.Log("Changing screen from " + m_currentScreen + " to " + _scr);

			// Do we have a valid transition from current screen to target screen?
			Transition t = FindTransition(m_currentScreen, _scr);
			ScreenData targetScreenData = GetScreenData(_scr);
			if(t != null && _animate) {
				// Yes! Use it
				// Get some aux vars first
				float duration = t.overrideDuration ? t.duration : m_defaultTransitionDuration;
				Ease ease = t.overrideEase ? t.ease : m_defaultTransitionEase;

				// Lerp path (if defined)
				if(t.path != null) {
					// Dynamic path will copy the original transition path and then 
					// lerp it with the current camera position replacing the first point
				
					// Aux vars
					BezierPoint refP = null;
					BezierPoint newP = null;
					int initialPoint = t.path.GetPointIdx(t.initialPathPoint);
					int finalPoint = t.path.GetPointIdx(t.finalPathPoint);

					// If animating, kill tween
					if(m_cameraTween != null) {
						m_cameraTween.Kill();
					}

					// 1. Clear path
					m_dynamicPath.transform.position = t.path.transform.position;	// Points are cloned, so we need parent path to be at the same position!
					m_dynamicPath.Clear();

					// 2. Compute curve offset to be able to lerp the curve points, 
					//	  respecting curve shape but using current camera position as start point
					refP = t.path.GetPoint(initialPoint);
					Vector3 offset = m_camera.transform.position - refP.globalPosition;
					Vector3 correctedOffset = offset;

					// Compute deltas to be able to properly do the interpolation
					float delta = 0f;
					float initialDelta = t.path.GetDelta(initialPoint);
					float finalDelta = t.path.GetDelta(finalPoint);

					// 3. Clone points from the transition curve, respecting the order
					//	  Use the loop to compute the interpolation as well
					int pointsToProcess = Mathf.Abs(initialPoint - finalPoint) + 1;
					int i = initialPoint;
					for(int processedPoints = 0; processedPoints < pointsToProcess; ++processedPoints) {
						// Create new point
						refP = t.path.GetPoint(i);
						newP = new BezierPoint(refP);
						m_dynamicPath.AddPoint(newP);

						// Interpolate using initial point offset and delta
						delta = Mathf.InverseLerp(initialDelta, finalDelta, t.path.GetDelta(i));
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
					m_cameraTween = DOTween.To(
						() => { 
							return m_cameraTweenDelta; 
						}, 
						_newValue => {
							m_camera.transform.position = m_dynamicPath.GetValue(m_cameraTweenDelta);
							m_cameraTweenDelta = _newValue;
						}, 
						1f,
						duration)
						.SetEase(ease)
						.SetAutoKill(true)
						.SetTarget(m_camera)
						.OnComplete(() => {
							m_cameraTween = null; 
						});
				}

				// Camera rotation and properties will just be lerped using the snap points
				TweenParams tweenParams = new TweenParams().SetEase(ease);
				targetScreenData.snapPoint.changePosition = false;
				targetScreenData.snapPoint.TweenTo(m_camera, duration, tweenParams);
			} else {
				// Not! Go straight to the new screen
				targetScreenData.snapPoint.changePosition = true;
				targetScreenData.snapPoint.Apply(m_camera);
			}

			// Store new screen
			m_currentScreen = _scr;

			// Refresh buttons list
			RefreshScreenButtons();
		}

		/// <summary>
		/// Get the setup for a specific scrreen.
		/// </summary>
		/// <returns>The screen data corresponding to the requested screen.</returns>
		/// <param name="_scr">Screen whose data we want.</param>
		public ScreenData GetScreenData(Screen _scr) {
			// Check index (for Screen.NONE and Screen.COUNR)
			int idx = (int)_scr;
			if(idx < 0 || idx > m_screens.Length) return null;

			return m_screens[(int)_scr];
		}

		/// <summary>
		/// Find a transition between two screens.
		/// </summary>
		/// <returns>The transition. <c>null</c> if no transition defined between both screens.</returns>
		/// <param name="_from">Source screen.</param>
		/// <param name="_to">Destination screen.</param>
		public Transition FindTransition(Screen _from, Screen _to) {
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
		/// Enables only buttons corresponding to valid transitions.
		/// </summary>
		private void RefreshScreenButtons() {
			// Rather than enabling/disabling the buttons, change color text to valid transitions
			ScreenData currentScreenData = GetScreenData(m_currentScreen);
			for(int i = 0; i < m_buttons.Length; ++i) {
				m_buttons[i].GetComponentInChildren<TextMeshProUGUI>().color = (i == (int)m_currentScreen) ? Colors.white : Colors.gray;
				m_buttons[i].GetComponentInChildren<Image>().color = (i == (int)m_currentScreen) ? Colors.darkGreen : Colors.white;
			}

			for(int i = 0; i < currentScreenData.transitions.Length; ++i) {
				m_buttons[(int)currentScreenData.transitions[i].destination].GetComponentInChildren<TextMeshProUGUI>().color = Colors.darkGreen;
			}
		}

		//------------------------------------------------------------------------//
		// CALLBACKS															  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// 
		/// </summary>
		/// <param name="_screenIdx">Screen to go to.</param>
		public void OnScreenButton(int _screenIdx) {
			// Just go to that screen
			GoToScreen((Screen)_screenIdx, true);
		}
	}
}