// DragonTierDefinitions.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom definition set to be able to create the asset.
/// </summary>
//[CreateAssetMenu]
public class DragonTierDefinitions : DefinitionSet<DragonTierDef> {
	//----------------------------------------------------------------------//
	// CUSTOM METHODS														//
	//----------------------------------------------------------------------//
	/// <summary>
	/// Alternative definition getter using <see cref="DragonTier"/> enum instead of sku.
	/// Slightly less efficient, use consciously.
	/// </summary>
	/// <returns>The definition matchin the requeste tier.</returns>
	/// <param name="_tier">The tier whose definition we want.</param>
	public DragonTierDef GetDef(DragonTier _tier) {
		foreach(KeyValuePair<string, DragonTierDef> kvp in m_defsDict) {
			if(kvp.Value.tier == _tier) return kvp.Value;
		}
		return null;
	}
}