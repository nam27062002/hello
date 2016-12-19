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
		[SkuList(DefinitionsCategory.RARITIES, false)]
		public string rarity = "";
		public GameObject gameObject = null;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Comment("There should be one background for each rarity (as defined in the content)", 10)]
	[SerializeField] private List<RarityBackground> m_backgroundsByRarity = new List<RarityBackground>();
	
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
	/// Manually initializes with a given rarity sku.
	/// </summary>
	/// <param name="_raritySku">Rarity sku.</param>
	/// <param name="_text">Text to be displayed, already localized.</parm>
	public void InitFromRarity(string _raritySku, string _text) {
		// Get rarity definition
		DefinitionNode rarityDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.RARITIES, _raritySku);

		// Use alternative initializer
		InitFromRarity(rarityDef, _text);
	}

	/// <summary>
	/// Manually initializes with a given rarity definition.
	/// </summary>
	/// <param name="_rarity">Rarity value.</param>
	/// <param name="_text">Text to be displayed, already localized.</parm>
	public void InitFromRarity(DefinitionNode _rarityDef, string _text) {
		// Choose right background
		for(int i = 0; i < m_backgroundsByRarity.Count; i++) {
			// Is it a match?
			bool match = (m_backgroundsByRarity[i].rarity == _rarityDef.sku);

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