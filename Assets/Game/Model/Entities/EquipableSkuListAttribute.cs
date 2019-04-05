// EquipableSkuListAttribute.cs
// 
// Copyright (c) 2019 Ubisoft. All rights reserved.

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
/// we're dealing with (EquipableDef).
/// Usage: [EquipableSkuListAttribute]
/// Usage: [EquipableSkuListAttribute(true)]
/// </summary>
public class EquipableSkuListAttribute : PropertyAttribute {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public bool m_allowNullValue = false;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_allowNullValue">If set to <c>true</c>, the "NONE" option will be available.</param>
	public EquipableSkuListAttribute(bool _allowNullValue = false) {
		m_allowNullValue = _allowNullValue;
	}
}

