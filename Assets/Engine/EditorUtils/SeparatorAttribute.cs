// SeparatorAttribute.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple custom attribute to draw a separator line between different sections 
/// of your script.
/// Usage 1: [Separator]
/// Usage 2: [Separator("title")]
/// </summary>
public class SeparatorAttribute : PropertyAttribute {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public string m_title = "";

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default empty constructor.
	/// </summary>
	public SeparatorAttribute() {
		m_title = "";
	}

	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	public SeparatorAttribute(string _title) {
		m_title = _title;
	}
}

