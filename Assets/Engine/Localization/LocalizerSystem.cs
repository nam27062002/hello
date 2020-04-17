// LocalizerSystem.cs
// 
// Created by Alger Ortín Castellví on 01/04/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Localizer version for Unity Textfields.
/// </summary>
public class LocalizerSystem : Localizer {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// References
	protected new Text m_text = null;
	public new Text text {
		get { 
			if(m_text == null) m_text = GetComponent<Text>();
			return m_text; 
		}
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public override void Awake() {
		// Check required stuff
		m_text = GetComponent<Text>();
		DebugUtils.Assert(m_text != null, "Required member!");
	}

	//------------------------------------------------------------------------//
	// INTERNAL UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Update the text with the current tid, replacements and language.
	/// </summary>
	protected override void Localize() {
		// Check params
		if(m_text == null) return;

		// Special Case: if tid is empty, skip localization, replacement and casing and put an empty string
		if(string.IsNullOrEmpty(m_tid)) {
			// Except if text was forced
			if(!m_ignoreLanguageChange) {
				m_text.text = string.Empty;
			}
			return;
		}

		// Perform the localization
        string localizedString = LocalizationManager.SharedInstance.Localize(m_tid, replacements);

		// Apply casing to full string
		localizedString = ApplyCase(m_caseType, localizedString);

		// Apply to textfield
		m_text.text = localizedString;
	}

	/// <summary>
	/// Directly set an already localized text.
	/// Use for texts that don't require localization or texts localized from outside.
	/// </summary>
	/// <param name="_text">Text to be applied to the textfield.</param>
	public override void Set(string _text) {
		// Clear tid and params
		m_tid = string.Empty;
		m_replacements = null;

		// Refresh text
		if(m_text != null) {
			m_text.text = _text;
		}

		// Ignore future language changes
		m_ignoreLanguageChange = true;
	}
}
