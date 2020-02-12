// PopupShopTooltip.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Temp popup to "purchase" currencies.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupShopTooltip : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Economy/Shop2020/PF_PopupShopTooltip";

	private enum State {
		IDLE,
		SAFE_FRAMES,
		WAITING_FOR_TOUCH,
		CLOSING
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed References
    [SerializeField] private UITooltip m_tooltip = null;
	public UITooltip tooltip {
		get { return m_tooltip; }
	}

	// Exposed Setup
	[Space]
	[SerializeField] private int m_safeFrames = 5;

	// Internal references
	private PopupController m_popup = null;

	// Internal parameters
	private RectTransform m_anchor = null;
	private Vector2 m_offset = GameConstants.Vector2.zero;

	// Internal logic
	private State m_state = State.SAFE_FRAMES;
	private float m_stateTimer = -1f;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Gather internal references
		m_popup = GetComponent<PopupController>();

		// Subscribe to external events
		m_popup.OnOpenPreAnimation.AddListener(OnOpenPreAnimation);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		m_popup.OnOpenPreAnimation.RemoveListener(OnOpenPreAnimation);
	}

	/// <summary>
	/// Initialize the popup with the given data. Call before Open.
	/// </summary>
	/// <param name="_anchor">Anchor used as position reference to place the tooltip.</param>
	/// <param name="_offset">Optional offset from the anchor.</param>
	public void Init(RectTransform _anchor, Vector2 _offset) {
		// Just store parameters, they will be applied when opening the popup
		m_anchor = _anchor;
		m_offset = _offset;
	}

    /// <summary>
    /// Component has been enabled
    /// </summary>
    private void OnEnable() {
		// Go to initial state
		ChangeState(State.SAFE_FRAMES);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
    public void Update() {
		// Depending on state
		switch(m_state) {
			case State.IDLE: {
				// Nothing to do
			} break;

			case State.SAFE_FRAMES: {
				// Update frame counter
				if(m_stateTimer > 0f) {
					m_stateTimer -= 1f;	// Counting frames
					if(m_stateTimer <= 0f) {
						// Go to IDLE
						ChangeState(State.WAITING_FOR_TOUCH);
					}
				}
			} break;

			case State.WAITING_FOR_TOUCH: {
				// If any input on the screen is detected, close the tooltip (and the popup)
#if((UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR)
				if(Input.touchCount > 0) {
#else
				if(Input.GetMouseButtonDown(0)) {
#endif
					ChangeState(State.CLOSING);
				}
			} break;

			case State.CLOSING: {
				// Update close delay timer
				if(m_stateTimer > 0f) {
					m_stateTimer -= Time.deltaTime;
					if(m_stateTimer <= 0f) {
						// Close the popup and go to idle
						m_popup.Close(true);
						ChangeState(State.IDLE);
					}
				}
			} break;
		}
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Change the logic state of the popup.
	/// </summary>
	/// <param name="_newState"></param>
	private void ChangeState(State _newState) {
		// Do stuff upon entering a new state
		switch(_newState) {
			case State.IDLE: {
				// Nothing to do
			} break;

			case State.SAFE_FRAMES: {
				// Reset timer
				m_stateTimer = (float)m_safeFrames;
			} break;

			case State.WAITING_FOR_TOUCH: {
				// Nothing to do
			} break;

			case State.CLOSING: {
				// Close tooltip first
				m_tooltip.animator.ForceHide(true);

				// Close the popup after some delay
				m_stateTimer = m_tooltip.animator.tweenDuration;

			} break;
		}

		// Store new state
		m_state = _newState;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to open.
	/// </summary>
	private void OnOpenPreAnimation() {
		// Place and open tooltip
		// UITooltip does all the hard math for us!
		UITooltip.PlaceAndShowTooltip(
			m_tooltip,
			m_anchor,
			m_offset,
			false,
			false
		);
	}
}
