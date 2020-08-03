// FontGroup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/03/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple representation of a font group. Stores data of a single definition
/// from the fontGroupsDefinitions table.
/// </summary>
[Serializable]
public class FontGroup {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public string sku = "";
	public string[] fontAssets = null;
	public string defaultFont = "";
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	public FontGroup(DefinitionNode _def) {
		// Initialize from def
		// Sku
		sku = _def.sku;

		// Font assets
		fontAssets = _def.GetAsArray<string>("fonts", ";");

		// Default font
		// If not defined, grab first one in the font assets array
		defaultFont = _def.GetAsString("defaultFont", string.Empty);
		if(string.IsNullOrEmpty(defaultFont) && fontAssets.Length > 0) {
			defaultFont = fontAssets[0];
		}
	}
}