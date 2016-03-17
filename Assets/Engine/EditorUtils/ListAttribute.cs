// ListAttribute.cs
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
/// Custom attribute to select a value from a limited list of options.
/// Only works with basic types, as stated by C# documentation:
/// <list type="bullet">
/// <item><description>One of the following types: bool, byte, char,  double, float, int, long, short, string.</description></item>
/// <item><description>The type object.</description></item>
/// <item><description>The type System.Type.</description></item>
/// <item><description>An enum type, provided it has public accessibility and the types in which it is nested (if any) also have public accessibility.</description></item>
/// <item><description>Single-dimensional arrays of the above types.</description></item>
/// </list>
/// http://stackoverflow.com/questions/1235617/how-to-pass-objects-into-an-attribute-constructor
/// Usage: 
/// <code>
/// [List("option1", "option2", "option3")]
/// public string m_testString;
/// 
/// [List(0, 5, 10, 15, 20)]
/// public int m_testInt;
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ListAttribute : PropertyAttribute {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public object[] m_options = new object[0];

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_options">Valid values to be used. Should be of the same type of the field (or castable to).</param>
	public ListAttribute(params object[] _options) {
		// Just store options array
		m_options = _options;
	}
}

