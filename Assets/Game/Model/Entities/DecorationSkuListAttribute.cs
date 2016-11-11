﻿// DecorationSkuListAttribute.cs
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
/// Usage: [DecorationSkuListAttribute]
/// Usage: [DecorationSkuListAttribute(true)]
/// </summary>
public class DecorationSkuListAttribute : PropertyAttribute {
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
	public DecorationSkuListAttribute(bool _allowNullValue = false) {
		m_allowNullValue = _allowNullValue;
	}
}

