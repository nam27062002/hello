// DragonDefinitions.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom definition set to be able to create the asset.
/// </summary>
//[CreateAssetMenu]
public class DragonDefinitions : DefinitionSet<DragonDef> {
	//------------------------------------------------------------------//
	// CUSTOM PROPERTIES												//
	//------------------------------------------------------------------//
	public List<DragonDef> defsListByMenuOrder {
		get {
			// Linq makes it easy for us
			List<DragonDef> sortedDefs = defsList;
			sortedDefs.OrderBy(_def => _def.menuOrder);
			return sortedDefs;
		}
	}
}