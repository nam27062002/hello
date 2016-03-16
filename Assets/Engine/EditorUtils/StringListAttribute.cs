// StringListAttribute.cs
// 
// Created by Alger Ortín Castellví on 16/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

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
/// Usage: [StringList("option1", "option2", "option3")]
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class StringListAttribute : PropertyAttribute {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public string[] m_options = new string[0];

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_options">Valid values to be used.</param>
	public StringListAttribute(params string[] _options) {
		// Just store options array
		m_options = _options;
	}
}

