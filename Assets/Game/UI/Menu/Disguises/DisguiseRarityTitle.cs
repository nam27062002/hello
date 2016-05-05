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
		public Image image = null;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private Localizer m_nameText = null;
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
		// Check required fields
		Debug.Assert(m_nameText != null, "Required field!");
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

		// Name
		if(_disguiseDef == null) {
			m_nameText.Localize("TID_DISGUISE_DEFAULT_NAME");
		} else {
			m_nameText.Localize(_disguiseDef.Get("tidName"));
		}

		// Choose right background
		string rarity = m_disguiseDef == null ? "common" : m_disguiseDef.GetAsString("rarity");
		for(int i = 0; i < m_backgroundsByRarity.Count; i++) {
			// Only show target rarity background
			m_backgroundsByRarity[i].image.gameObject.SetActive(m_backgroundsByRarity[i].rarity == rarity);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}