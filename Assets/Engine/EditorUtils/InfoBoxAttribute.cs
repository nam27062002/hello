// InfoBoxAttribute.cs
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
public class InfoBoxAttribute : PropertyAttribute {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public string m_text = "";

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_text">The text to be displayed in the infobox.</param>
	public InfoBoxAttribute(string _text) {
		m_text = _text;
	}
}

