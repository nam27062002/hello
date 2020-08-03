// UISafeArea.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/12/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Utility class to define a safe area.
/// Safe area is defined in Canvas reference resolution.
/// </summary>
[System.Serializable]
public class UISafeArea {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public float bottom = 0f;
	public float left = 0f;
	public float right = 0f;
	public float top = 0f;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor
	/// </summary>
	public UISafeArea() {

	}

	/// <summary>
	/// Parametrized constructor
	/// </summary>
	/// <param name="_left">Left margin.</param>
	/// <param name="_top">Top margin.</param>
	/// <param name="_right">Right margin.</param>
	/// <param name="_bottom">Bottom margin.</param>
	public UISafeArea(float _left, float _top, float _right, float _bottom) {
		this.left = _left;
		this.top = _top;
		this.right = _right;
		this.bottom = _bottom;
	}

	/// <summary>
	/// Get a string representation of this safe area.
	/// </summary>
	/// <returns>A <see cref="T:System.String"/> that represents this <see cref="T:UISafeArea"/>.</returns>
	public string ToString() {
		return left + ", " + top + ", " + right + ", " + bottom;
	}
}