// PopupSettingsLanguagePill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Language pill for the settings popup.
/// </summary>
public class PopupSettingsLanguagePill : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Localizer m_nameText = null;
	[SerializeField] private Image m_flagImage = null;

	// Internal
	private DefinitionNode m_def = null;
	public DefinitionNode def {
		get { return m_def; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the pill using the data from the given definition.
	/// </summary>
	/// <param name="_languageDef">Language definition.</param>
	public void InitFromDef(DefinitionNode _languageDef) {
		// Save definition
		m_def = _languageDef;

		// Set language name
		m_nameText.Localize(m_def.Get("tidName"));  // [AOC] CHECK!! Each language in its own language

		// Initialize language image
		m_flagImage.sprite = Resources.Load<Sprite>(UIConstants.LANGUAGE_ICONS_PATH + m_def.Get("icon"));
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}