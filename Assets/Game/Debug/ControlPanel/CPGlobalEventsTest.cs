// CPGlobalEventsTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Global class to manage global events testing features.
/// </summary>
public class CPGlobalEventsTest : MonoBehaviour {
	//------------------------------------------------------------------------//
	// GLOBAL																  //
	//------------------------------------------------------------------------//
	public const string TEST_ENABLED = "RESULTS_TEST_ENABLED";
	public static bool testEnabled {
		get { return Prefs.GetBoolPlayer(TEST_ENABLED, false); }
		set { Prefs.SetBoolPlayer(TEST_ENABLED, value); }
	}

	//------------------------------------------------------------------------//
	// EXPOSED MEMBERS														  //
	//------------------------------------------------------------------------//
	// Global
	[Space]
	[SerializeField] private Toggle m_testEnabledToggle = null;
	[SerializeField] private CanvasGroup m_canvasGroup = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to changed events
		m_testEnabledToggle.onValueChanged.AddListener(
			(bool _toggled) => {
				testEnabled = _toggled;
				m_canvasGroup.interactable = testEnabled;
			}
		);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure all values ar updated
		Refresh();
	}

	/// <summary>
	/// Make sure all fields have the right values.
	/// </summary>
	private void Refresh() {
		m_testEnabledToggle.isOn = testEnabled;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}