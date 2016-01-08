// SkuListAttribute.cs
// 
// Created by Alger Ortín Castellví on 14/12/2015.
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
/// Usage: [SkuList(typeof(EntityDef))]
/// Usage: [SkuList(typeof(EntityDef), false)]
/// </summary>
public class SkuListAttribute : PropertyAttribute {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public Type m_defType = null;
	public bool m_allowNullValue = true;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_defType">The type of definition to be parsed.</param>
	/// <param name="_allowNullValue">If set to <c>true</c>, the "NONE" option will be available.</param>
	public SkuListAttribute(Type _defType, bool _allowNullValue = true) {
		// Check type validity
		if(_defType == null || !_defType.IsSubclassOf(typeof(Definition))) {
			Debug.LogError("Failed to load Def Type for the SkuListAttribute (either null or doesn't inherit from Definition)");
		}

		m_defType = _defType;
		m_allowNullValue = _allowNullValue;
	}
}

