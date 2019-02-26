// PopupAdPlaceholder.cs
// 
// Created by Alger Ortín Castellví on 09/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Placeholder popup while ads are being integrated.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupAdPlaceholder : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/InGame/PF_PopupAdPlaceholder";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Localizer m_timerText = null;
	[SerializeField] private float m_adDuration = 10f;	// Simulate ad duration

	// Internal
	private bool m_adRunning = false;
	private DeltaTimer m_timer = new DeltaTimer();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		// Check required params
		Debug.Assert(m_timerText != null, "Required field");
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		if(m_adRunning) {
			// Refresh text
            m_timerText.Localize("TID_GAME_REVIVE_AD_ENDS_IN", StringUtils.FormatNumber(Mathf.CeilToInt((float)m_timer.GetTimeLeft() / 1000.0f)));

			// Once timer finished, auto-close the popup and stop refreshing
			if(m_timer.IsFinished()) {
				m_adRunning = false;
				GetComponent<PopupController>().Close(true);
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Popup is about to be opened.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Reset timer
		// Override if control panel says so
		float duration = DebugSettings.Prefs_GetFloatPlayer(DebugSettings.POPUP_AD_DURATION, m_adDuration);
		if(duration < 0) duration = m_adDuration;
		m_timer.Start(duration * 1000f);
		m_adRunning = true;
	}
}
