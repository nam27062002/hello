// EntityDefinitions.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/12/2015.
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
public class EntityDefinitions : DefinitionSet<EntityDef> {
	//------------------------------------------------------------------//
	// EXTRA PROPERTIES													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Obtain a dictionary of all the definitions sorted by category.
	/// Key is category sku, value is list of entity definitions belonging to that category.
	/// </summary>
	/// <value>The defs sorted by category.</value>
	public Dictionary<string, List<EntityDef>> defsByCategory {
		get {
			// Create and populate a new dictionary
			Dictionary<string, List<EntityDef>> dict = new Dictionary<string, List<EntityDef>>();
			foreach(KeyValuePair<string, EntityDef> kvp in m_defsDict) {
				// IF category was not yet in the dictionary, add a slot for it
				if(!dict.ContainsKey(kvp.Value.categorySku)) {
					dict.Add(kvp.Value.categorySku, new List<EntityDef>());
				}

				// Add this definition to the dictionary
				dict[kvp.Value.categorySku].Add(kvp.Value);
			}
			return dict;
		}
	}
}