// DragonInfoTooltip.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specialized tooltip displaying dragon info.
/// </summary>
public class DragonInfoTooltip : UITooltip {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private Transform m_tierIconContainer = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Check required fields
		Debug.Assert(m_tierIconContainer != null, "Required field!");

		// Call parent
		base.Awake();
    }

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the tooltip with the given dragon's info.
	/// </summary>
	/// <param name="_dragonSku">The dragon to be used for initialization.</param>
	public void InitWithDragon(string _dragonSku) {
		// Get dragon's definition and use InitFromDefinition()
		DefinitionNode dragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, _dragonSku);
		InitFromDefinition(dragonDef);
	}

	/// <summary>
	/// Initialize the tooltip with the data from the given dragon definition.
	/// </summary>
	/// <param name="_dragonDef">Dragon definition.</param>
	public void InitFromDefinition(DefinitionNode _dragonDef) {
		// Ignore if given definition is not valid
		if(_dragonDef == null) return;

		// Load icon for the default skin of target dragon
		string defaultIconId = IDragonData.GetDefaultDisguise(_dragonDef.sku).Get("icon");
		Sprite dragonIcon = HDAddressablesManager.Instance.LoadAsset<Sprite>(defaultIconId);

		// Use default initializer for name, description and dragon icon
		Init(
			_dragonDef.GetLocalized("tidName"),
			_dragonDef.GetLocalized("tidDesc"),
			dragonIcon
		);

		// Tier icon
		if(m_tierIconContainer != null) {
			// Get tier definition
			string tierSku = _dragonDef.GetAsString("tier", "tier_6");  // If no tier is defined, assume it's a special dragon (tier_6)
			DragonTier tier = IDragonData.SkuToTier(tierSku);

			// Remove the placeholder
			m_tierIconContainer.DestroyAllChildren(true);

			// Add the proper tier icon
			GameObject tierIconPrefab = UIConstants.GetTierIcon(tier);
			Instantiate(tierIconPrefab, m_tierIconContainer, false);

		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}