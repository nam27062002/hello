// TextfieldLocalization.cs
// 
// Created by Alger Ortín Castellví on 17/07/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple behaviour to automatically localize the text set in the editor
/// on a textfield.
/// Use this when possible rather than directly setting the text's value.
/// </summary>
[RequireComponent(typeof(Text))]
public class Localizer : MonoBehaviour {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[Comment("Will be overwritten if the Localize(_tid, _params) method is invoked")]
	[SerializeField] private string m_tid = "";
	public string tid {
		get { return m_tid; }
	}

	[SerializeField] string[] m_replacements;
	public string[] replacements {
		get { return m_replacements; }
	}

	// References
	private Text m_text = null;
	public Text text {
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
		m_text = GetComponent<Text>();
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
	// INTERNAL UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Update the text with the current tid, replacements and language.
	/// </summary>
	private void Localize() {
		// Just do it
		if(m_text == null) return;
		m_text.text = Localization.Localize(m_tid, m_replacements);
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
