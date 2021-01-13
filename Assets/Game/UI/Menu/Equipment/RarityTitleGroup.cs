// RarityTitleGroup.cs
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
/// Simple controller for a reward rarity title. Automatically selects and initializes
/// the proper title object based on reward's rarity.
/// </summary>
public class RarityTitleGroup : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Comment("There should be one title for each rarity (as defined in the content)", 10)]
	[SerializeField] private List<RarityTitle> m_titles = new List<RarityTitle>();

	// Internal
	private RarityTitle m_activeTitle = null;
	public RarityTitle activeTitle {
		get { return m_activeTitle; }
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
		// Clear active background
		m_activeTitle = null;

		// Choose right background
		for(int i = 0; i < m_titles.Count; i++) {
			// Is it a match?
			bool match = (m_titles[i].rarity == _rarityDef.sku);

			// Only show target rarity background
			m_titles[i].gameObject.SetActive(match);

			// Stuff to do when matching
			if(match) {
				// Set text
				if(m_titles[i].text != null) m_titles[i].text.text = _text;

				// Update active object
				m_activeTitle = m_titles[i];
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}