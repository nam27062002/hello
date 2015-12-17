// TextfieldLocalization.cs
// 
// Created by Alger Ortín Castellví on 17/07/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple behaviour to automatically localize the text set in the editor
/// on a textfield.
/// </summary>
[RequireComponent(typeof(Text))]
public class AutoLocalization : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed members
	public string[] replacements;	// To be filled in the editor

	// References
	private Text txt = null;

	// Internal
	private string originalTid = "";

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		// Check required stuff
		txt = GetComponent<Text>();
		DebugUtils.Assert(txt != null, "Required member!");
	}

	/// <summary>
	/// First update.
	/// </summary>
	public void Start() {
		// Store original tid
		originalTid = txt.text;

		// Do the first translation
		Localize();
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(EngineEvents.EVENT_LANGUAGE_CHANGED, OnLanguageChanged);
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(EngineEvents.EVENT_LANGUAGE_CHANGED, OnLanguageChanged);
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Do the localization.
	/// </summary>
	private void Localize() {
		// Just do it
		txt.text = Localization.Localize(originalTid, replacements);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Localization language has changed, update textfield.
	/// </summary>
	private void OnLanguageChanged() {
		Localize();
	}
}
