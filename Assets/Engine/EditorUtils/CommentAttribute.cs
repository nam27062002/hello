// CommentAttribute.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple custom attribute to draw a single text line before a property.
/// </summary>
public class CommentAttribute : PropertyAttribute {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public string m_text = "";
	public float m_spaceAbove = 0f;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_text">The text to be displayed in the infobox.</param>
	/// <param name="_spaceAbove">The space to be left as separation above the comment.</param>
	public CommentAttribute(string _text, float _spaceAbove = 0f) {
		m_text = _text;
		m_spaceAbove = _spaceAbove;
	}
}

