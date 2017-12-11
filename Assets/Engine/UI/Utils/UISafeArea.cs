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
	/// <param name="_bottom">Bottom margin.</param>
	/// <param name="_left">Left margin.</param>
	/// <param name="_right">Right margin.</param>
	/// <param name="_top">Top margin.</param>
	public UISafeArea(float _bottom, float _left, float _right, float _top) {
		this.bottom = _bottom;
		this.left = _left;
		this.right = _right;
		this.top = _top;
	}
}