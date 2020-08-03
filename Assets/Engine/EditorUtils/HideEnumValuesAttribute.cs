// HideEnumValuesAttribute.cs
// 
// Created by Alger Ortín Castellví on 22/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple custom attribute to allow hiding first and/or last element of an enum
/// field when exposing it in the inspector.
/// Useful for default values (i.e. INIT = -1) or last element used as count (i.e. COUNT).
/// Usage: [HideEnumValues(true, true)]
/// </summary>
public class HideEnumValuesAttribute : PropertyAttribute {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public bool m_excludeFirstElement = false;
	public bool m_excludeLastElement = false;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_excludeFirstElement">If set to <c>true</c> _exclude first element of the enum.</param>
	/// <param name="_excludeLastElement">If set to <c>true</c> _exclude last element of the enum.</param>
	public HideEnumValuesAttribute(bool _excludeFirstElement, bool _excludeLastElement) {
		m_excludeFirstElement = _excludeFirstElement;
		m_excludeLastElement = _excludeLastElement;
	}
}

