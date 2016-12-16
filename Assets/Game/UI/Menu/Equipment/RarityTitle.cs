 // RarityTitle.cs
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
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for a reward rarity title
/// </summary>
public class RarityTitle : MonoBehaviour {
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
	private DefinitionNode m_itemDef = null;
	public DefinitionNode itemDef {
		get { return m_itemDef; }
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
	/// Manually initializes with a hardcoded rarity value.
	/// </summary>
	/// <param name="_rarity">Rarity value.</param>
	/// <param name="_text">Text to be displayed as name, already localized.</parm>
	public void InitFromRarity(string _rarity, string _text) {
		// Not using definition
		m_itemDef = null;

		// Choose right background
		for(int i = 0; i < m_backgroundsByRarity.Count; i++) {
			// Is it a match?
			bool match = (m_backgroundsByRarity[i].rarity == _rarity);

			// Only show target rarity background
			m_backgroundsByRarity[i].gameObject.SetActive(match);

			// Set text
			if(match) {
				TextMeshProUGUI text = m_backgroundsByRarity[i].gameObject.GetComponentInChildren<TextMeshProUGUI>();
				if(text != null) text.text = _text;
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}