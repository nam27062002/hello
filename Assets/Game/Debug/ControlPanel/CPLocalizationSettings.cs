// CPLanguageSettings.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/11/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Helper methods to work with localization settings.
/// </summary>
public class CPLocalizationSettings : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		Messenger.AddListener<string, bool>(GameEvents.CP_BOOL_CHANGED, OnBoolChanged);
	}

	/// <summary>
	/// Destruction.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe to external events
		Messenger.RemoveListener<string, bool>(GameEvents.CP_BOOL_CHANGED, OnBoolChanged);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A bool value has been changed on the control panel.
	/// </summary>
	public void OnBoolChanged(string _prefId, bool _newValue) {
		// Check pref ID
		if(_prefId == DebugSettings.SHOW_MISSING_TIDS) {
			// Either fill missing tids or reload current language
			if(_newValue) {
				// Reload language (empty TIDs will remain empty)
				LocalizationManager.SharedInstance.ReloadLanguage();
			} else {
				// Fill empty entries and notify game
				LocalizationManager.SharedInstance.FillEmptyTids("lang_english");
				Messenger.Broadcast(EngineEvents.LANGUAGE_CHANGED);
			}
		}
	}
}