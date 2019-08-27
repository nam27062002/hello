// SeasonListAttribute.cs
// 
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom attribute to select a definition from a list containing all the definitions of the target type.
/// Simplified version of the SkuListAttribute since we know exactly the type of definition
/// we're dealing with (DecorationDef).
/// Usage: [SeasonListAttribute]
/// Usage: [SeasonListAttribute(true)]
/// </summary>
public class SeasonListAttribute : PropertyAttribute {
	
}

