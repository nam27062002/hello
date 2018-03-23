// ColorGradient.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/03/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simplified version of a 4-corner gradient.
/// </summary>
[Serializable]
public class Gradient4 {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public Color topLeft = Color.white;
	public Color topRight = Color.white;
	public Color bottomLeft = Color.black;
	public Color bottomRight = Color.black;
	
	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// EXPERIMENTAL: Evaluate the color at a given coordinate.
	/// </summary>
	/// <param name="_coord">Coordinate to be evaluated (0..1).</param>
	public Color Evaluate(Vector2 _coord) {
		Color ctop = Color.Lerp(topLeft, topRight, _coord.x);
		Color cbot = Color.Lerp(bottomLeft, bottomRight, _coord.x);
		return Color.Lerp(ctop, cbot, (1f - _coord.y));	// Reverse delta since pixels are sorted bot to top
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}