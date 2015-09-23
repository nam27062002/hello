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
	public float m_size = 40f;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor with default values.
	/// </summary>
	public SeparatorAttribute(string _title = "", float _size = 40f) {
		m_title = _title;
		m_size = _size;
	}
}

