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
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string DEFAULT_TEXT = "";
	public static readonly float DEFAULT_SIZE = 20f;
	public static readonly Color DEFAULT_COLOR = new Color(0.65f, 0.65f, 0.65f);	// Silver-ish
	
	public enum Orientation {
		HORIZONTAL,
		VERTICAL
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public string m_text = DEFAULT_TEXT;
	public float m_size = DEFAULT_SIZE;
	public Color m_color = DEFAULT_COLOR;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Empty constructor.
	/// </summary>
	public SeparatorAttribute() {
		// Nothing to do, default values will be used.
	}

	/// <summary>
	/// Single param constructors.
	/// </summary>
	public SeparatorAttribute(string _text) {
		Init(_text, DEFAULT_SIZE, DEFAULT_COLOR);
	}
	
	public SeparatorAttribute(float _size) {
		Init(DEFAULT_TEXT, _size, DEFAULT_COLOR);
	}
	
	public SeparatorAttribute(Color _color) {
		Init(DEFAULT_TEXT, DEFAULT_SIZE, _color);
	}

	/// <summary>
	/// 2 param constructors.
	/// </summary>
	public SeparatorAttribute(string _text, float _size) {
		Init(_text, _size, DEFAULT_COLOR);
	}

	public SeparatorAttribute(string _text, Color _color) {
		Init(_text, DEFAULT_SIZE, _color);
	}

	public SeparatorAttribute(float _size, Color _color) {
		Init(DEFAULT_TEXT, _size, _color);
	}

	/// <summary>
	/// Full parametrized constructor.
	/// </summary>
	public SeparatorAttribute(string _text, float _size, Color _color) {
		Init(_text, _size, _color);
	}

	//------------------------------------------------------------------//
	// INTERNAL															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Init the separator with the given title, size and color.
	/// </summary>
	/// <param name="_text">The text to be displayed. Empty for single line.</param>
	/// <param name="_size">Size of the separator. Won't affect the thickness of the line, just the spacing around it.</param>
	/// <param name="_color">The color of the separator line.</param>
	private void Init(string _text, float _size, Color _color) {
		m_text = _text;
		m_size = _size;
		m_color = _color;
	}
}

