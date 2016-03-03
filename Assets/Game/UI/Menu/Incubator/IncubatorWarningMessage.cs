// IncubatorWarningMessage.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple auto-hide warning message for the incubator menu.
/// </summary>
[RequireComponent(typeof(ShowHideAnimator))]
public class IncubatorWarningMessage : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private float m_hideDelay = 0.5f;

	// Scene references
	private ShowHideAnimator m_anim = null;

	// Internal logic
	private float m_timer = -1f;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external assets
		m_anim = GetComponent<ShowHideAnimator>();
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Start hidden
		m_anim.Set(false, false);
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Wait until the timer runs out to hide the message
		if(m_anim.visible && m_timer >= 0f) {
			m_timer -= Time.deltaTime;
			if(m_timer < 0f) {
				m_anim.Hide();
			}
		}
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Show/hide message!
	/// </summary>
	/// <param name="_show">Whether to show or hide the message.</param>
	public void Show(bool _show) {
		// If showing, reset timer to auto-hide it in a while
		if(_show) {
			m_anim.Show();
			m_timer = m_hideDelay;
		} else {
			m_anim.Hide();
			m_timer = -1f;	// Clear timer
		}
	}
}

