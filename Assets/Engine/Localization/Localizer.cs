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
public class Localizer : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Case {
		DEFAULT,
		UPPER_CASE,
		LOWER_CASE,
		TITLE_CASE
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[Comment("Will be overwritten if the Localize(_tid, _params) method is invoked")]
	[SerializeField] protected string m_tid = "";
	public string tid {
		get { return m_tid; }
	}

	[SerializeField] private Case m_caseType = Case.DEFAULT;
	public Case caseType {
		get { return m_caseType; }
		set { m_caseType = value; }
	}

	[SerializeField] private string[] m_replacements;
	public string[] replacements {
		get { return m_replacements; }
	}

	// References
	private TextMeshProUGUI m_text = null;
	public TextMeshProUGUI text {
		get { 
			if(m_text == null) m_text = GetComponent<TextMeshProUGUI>();
			return m_text; 
		}
	}

	// Internal logic
	private bool m_ignoreLanguageChange = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public virtual void Awake() {
		// Check required stuff
		m_text = GetComponent<TextMeshProUGUI>();
		DebugUtils.Assert(m_text != null, "Required member!");
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
		Broadcaster.AddListener(BroadcastEventType.LANGUAGE_CHANGED, this);

		// Make sure text is properly localized (in case language changed while disabled)
		Localize();
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnDisable() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.LANGUAGE_CHANGED, this);
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
				processedText = processedText.Replace(formattingTag, formattingTag.ToLower(LocalizationManager.SharedInstance.Culture));

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

		// Listen to future language changes
		m_ignoreLanguageChange = false;
	}

	/// <summary>
	/// Directly set an already localized text.
	/// Use for texts that don't require localization or texts localized from outside.
	/// </summary>
	/// <param name="_text">Text to be applied to the textfield.</param>
	public void Set(string _text) {
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

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// An event has been broadcasted.
	/// </summary>
	/// <param name="_eventType">Event type.</param>
	/// <param name="_broadcastEventInfo">Event data.</param>
	public void OnBroadcastSignal(BroadcastEventType _eventType, BroadcastEventInfo _broadcastEventInfo) {
		switch(_eventType) {
			// Language has been changed!
			case BroadcastEventType.LANGUAGE_CHANGED: {
				// Re-localize (if allowed)
				if(!m_ignoreLanguageChange) {
					Localize();
				}
			} break;
		}
	}
}
