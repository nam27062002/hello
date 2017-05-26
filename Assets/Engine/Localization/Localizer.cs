// TextfieldLocalization.cs
// 
// Created by Alger Ortín Castellví on 17/07/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple behaviour to automatically localize the text set in the editor
/// on a textfield.
/// Use this when possible rather than directly setting the text's value.
/// </summary>
//[RequireComponent(typeof(TextMeshProUGUI))]
public class Localizer : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Case {
		DEFAULT,
		UPPER_CASE,
		LOWER_CASE,
		REPLACEMENTS_UPPER_CASE,
		REPLACEMENTS_LOWER_CASE,
		TITLE_CASE
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[Comment("Will be overwritten if the Localize(_tid, _params) method is invoked")]
	[SerializeField] private string m_tid = "";
	public string tid {
		get { return m_tid; }
	}

	[SerializeField] private Case m_caseType = Case.DEFAULT;
	public Case caseType {
		get { return m_caseType; }
		set { m_caseType = value; }
	}

	[SerializeField] string[] m_replacements;
	public string[] replacements {
		get { return m_replacements; }
	}

	// References
	private TextMeshProUGUI m_text = null;
	public TextMeshProUGUI text {
		get { return m_text; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		// Check required stuff
		m_text = GetComponent<TextMeshProUGUI>();
		DebugUtils.Assert(m_text != null, "Required member!");

		// If tid is not defined, grab text as tid
		if(string.IsNullOrEmpty(m_tid)) {
			m_tid = m_text.text;
		}
	}

	/// <summary>
	/// First update.
	/// </summary>
	public void Start() {
		// Do the first translation
		Localize();
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(EngineEvents.LANGUAGE_CHANGED, OnLanguageChanged);

		// Make sure text is properly localized (in case language changed while disabled)
		Localize();
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(EngineEvents.LANGUAGE_CHANGED, OnLanguageChanged);
	}

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Auxiliar method to apply a specific casing to a given string.
	/// </summary>
	/// <returns>The processed string.</returns>
	/// <param name="_case">Case type to be applied.</param>
	/// <param name="_text">Text to be processed.</param>
	public static string ApplyCase(Case _case, string _text) {
		// Do it!
		string processedText = _text;
		switch(_case) {
			case Case.LOWER_CASE: 	processedText = _text.ToLower(LocalizationManager.SharedInstance.Culture);	break;
			case Case.UPPER_CASE: 	processedText = _text.ToUpper(LocalizationManager.SharedInstance.Culture);	break;
			case Case.TITLE_CASE:	processedText = LocalizationManager.SharedInstance.Culture.TextInfo.ToTitleCase(_text);	break;	// From http://stackoverflow.com/questions/1206019/converting-string-to-title-case
		}

		// Reverse casing in formatting tags, where mixed-casing is problematic
		int startIdx = processedText.IndexOf("<");
		while(startIdx > -1) {
			int endIdx = processedText.IndexOf(">", startIdx);
			if(endIdx == -1) {
				// Error, tag unclosed
				Debug.LogWarning("Sentence error in '" + processedText + "' . The symbol > wasn't found.");
				startIdx = -1;	// Break loop
			} else {
				// Replace formatting tag with the same tag in lower case
				string formattingTag = processedText.Substring(startIdx, endIdx - startIdx);
				processedText = processedText.Replace(formattingTag, formattingTag.ToLowerInvariant());

				// Find next one
				startIdx = processedText.IndexOf("<", endIdx);
			}
		}

		// Done!
		return processedText;
	}

	//------------------------------------------------------------------------//
	// INTERNAL UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Update the text with the current tid, replacements and language.
	/// </summary>
	private void Localize() {
		// Just do it
		if(m_text == null) return;

		if(m_caseType == Case.REPLACEMENTS_LOWER_CASE || m_caseType == Case.REPLACEMENTS_UPPER_CASE) {
			Debug.Log("<color=red>!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!</color>");
		}

		// Process casing first
		string[] processedReplacements = new string[m_replacements.Length];
		for(int i = 0; i < m_replacements.Length; i++) {
			switch(m_caseType) {
                case Case.REPLACEMENTS_LOWER_CASE: 	processedReplacements[i] = m_replacements[i].ToLower(LocalizationManager.SharedInstance.Culture);	break;
                case Case.REPLACEMENTS_UPPER_CASE: 	processedReplacements[i] = m_replacements[i].ToUpper(LocalizationManager.SharedInstance.Culture);	break;
				default: 							processedReplacements[i] = m_replacements[i];								break;
			}
		}

		// Perform the localization
        string localizedString = LocalizationManager.SharedInstance.Localize(m_tid, replacements);

		// Apply casing to full string
		localizedString = ApplyCase(m_caseType, localizedString);

		// Apply to textfield
		m_text.text = localizedString;
	}

	/// <summary>
	/// Update the text with given tid and replacements, using the current
	/// localization language.
	/// </summary>
	/// <param name="_tid">The text ID to be translated.</param>
	/// <param name="_params">The parameters used to replace the %U0, %U1, etc vars within the translated text.</param>
	public void Localize(string _tid, params string[] _params) {
		// Store new tid and params
		m_tid = _tid;
		m_replacements = _params;

		// Refresh text
		Localize();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Localization language has changed, update textfield.
	/// </summary>
	private void OnLanguageChanged() {
		Localize();
	}
}
