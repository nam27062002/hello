// DisguiseRarityTitle.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for a disguise rarity title
/// </summary>
public class DisguiseRarityTitle : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[Serializable]
	private class RarityBackground {
		public string rarity = "";
		public GameObject gameObject = null;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Comment("There should be one background for each rarity (as defined in the content)", 10)]
	[SerializeField] private List<RarityBackground> m_backgroundsByRarity = new List<RarityBackground>();

	// Data
	private DefinitionNode m_disguiseDef = null;
	public DefinitionNode disguiseDef {
		get { return m_disguiseDef; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this button with the data from the given definition.
	/// </summary>
	/// <param name="_disguiseDef">Disguise definition. <c>null</c> for default disguise.</param>
	public void InitFromDefinition(DefinitionNode _disguiseDef) {
		// Save definition
		m_disguiseDef = _disguiseDef;

		// Choose right background
		string rarity = m_disguiseDef == null ? "common" : m_disguiseDef.GetAsString("rarity");
		for(int i = 0; i < m_backgroundsByRarity.Count; i++) {
			// Is it a match?
			bool match = (m_backgroundsByRarity[i].rarity == rarity);

			// Only show target rarity background
			m_backgroundsByRarity[i].gameObject.SetActive(match);

			// Set text
			if(match) {
				Text text = m_backgroundsByRarity[i].gameObject.GetComponentInChildren<Text>();
				if(text != null) {
					if(_disguiseDef == null) {
						text.text = LocalizationManager.SharedInstance.Localize("TID_DISGUISE_DEFAULT_NAME");
					} else {
						text.text = _disguiseDef.GetLocalized("tidName");
					}
				}
			}
		}
	}

	/// <summary>
	/// Manually initializes with a hardcoded rarity value.
	/// </summary>
	/// <param name="_rarity">Rarity value.</param>
	/// <param name="_text">Text to be displayed as name, already localized.</parm>
	public void InitFromRarity(string _rarity, string _text) {
		// Not using definition
		m_disguiseDef = null;

		// Choose right background
		for(int i = 0; i < m_backgroundsByRarity.Count; i++) {
			// Is it a match?
			bool match = (m_backgroundsByRarity[i].rarity == _rarity);

			// Only show target rarity background
			m_backgroundsByRarity[i].gameObject.SetActive(match);

			// Set text
			if(match) {
				Text text = m_backgroundsByRarity[i].gameObject.GetComponentInChildren<Text>();
				if(text != null) text.text = _text;
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}