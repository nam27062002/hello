// BusyScreen.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/07/2017.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
// #define DISABLE_BUSY_SCREEN

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Splash screen to show while the game is busy.
/// </summary>
public class BusyScreen : UbiBCN.SingletonMonoBehaviour<BusyScreen> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private enum State {
		HIDDEN = 0,
		FADE_IN,
		VISIBLE,
		FADE_OUT
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private float m_fadeTime = 0.25f;

	[SerializeField] private CanvasGroup m_canvasGroup = null;
	public CanvasGroup canvasGroup {
		get { return m_canvasGroup; }
	}

	[SerializeField] private TextMeshProUGUI m_text = null;
	public TextMeshProUGUI text {
		get { return m_text; }
	}

	[SerializeField] private GameObject m_spinner = null;
	public GameObject spinner {
		get { return m_spinner; }
	}

	// Internal
	private HashSet<Object> m_owners = new HashSet<Object>();	// HashSet ~= List without duplicates

	private float m_fromAlpha;
	private float m_timer;
	private State m_state;



	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Default setup
		SetupInternal(true, string.Empty);

		// Start hidden
		ChangeState(State.HIDDEN);
	}

	private void Update() {
		switch (m_state) {
		case State.FADE_IN: {
				m_timer -= Time.deltaTime;
				if (m_timer <= 0f) {
					ChangeState(State.VISIBLE);
				} else {
					m_canvasGroup.alpha = Mathf.Lerp(m_fromAlpha, 1f, 1f - (m_timer / m_fadeTime));
				}
			} break;

		case State.FADE_OUT: {
				m_timer -= Time.deltaTime;
				if (m_timer <= 0f) {
					ChangeState(State.HIDDEN);
				} else {
					m_canvasGroup.alpha = Mathf.Lerp(m_fromAlpha, 0f, 1f - (m_timer / m_fadeTime));
				}
			} break;
		}
	}


	//------------------------------------------------------------------//
	// SINGLETON STATIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Setup the busy screen.
	/// </summary>
	/// <param name="_showSpinner">Show the spinner?.</param>
	/// <param name="_text">Text to be displayed. string.Empty for none.</param>
	public static void Setup(bool _showSpinner, string _text) {
		instance.SetupInternal(_showSpinner, _text);
	}

	/// <summary>
	/// Toggle the loading screen on/off.
	/// </summary>
	/// <param name="_show">Whether to show or hide the screen.</param>
	/// <param name="_owner">The object performing the request.</param>
	/// <param name="_animate">Use fade animation?</param>
	public static void Toggle(bool _show, Object _owner, bool _animate = true) {
		#if !DISABLE_BUSY_SCREEN
		instance.__Toggle(_show, _owner, _animate);
		#endif
	}

	/// <summary>
	/// Toggle the loading screen on.
	/// </summary>
	/// <param name="_owner">The object performing the request.</param>
	/// <param name="_animate">Fade animation?</param>
	public static void Show(Object _owner, bool _animate = true) {
		Toggle(true, _owner, _animate);
	}

	/// <summary>
	/// Toggle the loading screen off.
	/// </summary>
	/// <param name="_owner">The object performing the request.</param>
	/// <param name="_animate">Fade animation?</param>
	public static void Hide(Object _owner, bool _animate = true) {
		Toggle(false, _owner, _animate);
	}

	/// <summary>
	/// Toggle the loading screen off, clearing the owners stack.
	/// </summary>
	/// <param name="_animate">Fade animation?</param>
	public static void ForceHide(bool _animate = true) {
		// Clear owners stack
		instance.m_owners.Clear();

		// Hide!
		Hide(null, _animate);
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Actually do the setup.
	/// </summary>
	/// <param name="_showSpinner">Show the spinner?.</param>
	/// <param name="_text">Text to be displayed. string.Empty for none.</param>
	private void SetupInternal(bool _showSpinner, string _text) {
		// Spinner
		m_spinner.SetActive(_showSpinner);

		// Text
		m_text.gameObject.SetActive(!string.IsNullOrEmpty(_text));
		m_text.text = _text;
	}

	private void __Toggle(bool _show, Object _owner, bool _animate) {
		// Only hide when there are no owners retaining the screen
		if(_show) {
			if(_owner != null) instance.m_owners.Add(_owner);
			ChangeState((_animate)? State.FADE_IN : State.VISIBLE);
		} else {
			instance.m_owners.Remove(_owner);
			if(instance.m_owners.Count == 0) {
				ChangeState((_animate)? State.FADE_OUT : State.HIDDEN);
			}
		}
	}

	private void ChangeState(State _state) {
		switch (_state) {
		case State.HIDDEN:
			m_canvasGroup.alpha = 0f;
			gameObject.SetActive(false);
			break;

		case State.FADE_IN:
			m_fromAlpha = m_canvasGroup.alpha;
			m_timer = m_fadeTime * (1f - m_fromAlpha);
			gameObject.SetActive(true);
			break;

		case State.VISIBLE:
			m_canvasGroup.alpha = 1f;
			gameObject.SetActive(true);
			break;

		case State.FADE_OUT:
			m_fromAlpha = m_canvasGroup.alpha;
			m_timer = m_fadeTime * m_fromAlpha;
			gameObject.SetActive(true);
			break;
		}

		m_state = _state;
	}
}