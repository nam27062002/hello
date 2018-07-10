// PopupInfoEggDropChance.cs
// 
// Created by Alger Ortín Castellví on 06/07/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Drop chance of a specific egg.
/// </summary>
public class PopupInfoEggDropChance : PopupInfoDropChance {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Tutorial/PF_PopupInfoEggDropChance";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private Localizer m_title = null;
	[SerializeField] private MenuEggLoader m_eggPreview = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Do nothing, this popup requires manual initialization
	}

	/// <summary>
	/// Initialize with a given egg sku.
	/// </summary>
	/// <param name="_eggSku">Egg sku.</param>
	public void Init(string _eggSku) {
		// Get definition of the target egg
		DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, _eggSku);
		Debug.Assert(eggDef != null, "Unknown egg " + _eggSku, this);

		// Create an array with the probabilities for each rarity
		float[] probabilities = EggManager.ComputeProbabilities(eggDef);

		// Initialize rarity info objects
		InitInfos(m_rarityInfos, probabilities);

		// Initialize egg preview and title
		if(m_eggPreview != null) m_eggPreview.Load(eggDef.sku);
		if(m_title != null) m_title.Localize(eggDef.GetAsString("tidName"));
	}
}
