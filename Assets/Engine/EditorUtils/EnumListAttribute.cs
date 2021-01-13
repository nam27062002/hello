// EnumListAttribute.cs
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
/// Custom attribute to show the values from an enum to choose from and store them into a string field.
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class EnumListAttribute : ListAttribute {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public Type m_enumType = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_enumType">The type of the enum to be used for this field.</param>
	public EnumListAttribute(Type _enumType) {
		// Make sure it's an enum type
		Debug.Assert(_enumType.IsEnum, "EnumList Attribute only works with Enum types.");
		m_enumType = _enumType;
		ValidateOptions();	// Initialize options array
	}

	/// <summary>
	/// Make sure the options array is updated.
	/// To be implemented by heirs if needed.
	/// </summary>
	public virtual void ValidateOptions() {
		// Just fill in the options array with the values from the enum
		m_options = System.Enum.GetNames(m_enumType);
	}
}

