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
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public Gradient4() {
		// Nothing to do
	}

	/// <summary>
	/// Copy constructor.
	/// </summary>
	/// <param name="_g">Source gradient.</param>
	public Gradient4(Gradient4 _g) {
		if(_g == null) return;
		Set(_g);
	}

	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_topLeft">Top left color.</param>
	/// <param name="_topRight">Top right color.</param>
	/// <param name="_bottomLeft">Bottom left color.</param>
	/// <param name="_bottomRight">Bottom right color.</param>
	public Gradient4(Color _topLeft, Color _topRight, Color _bottomLeft, Color _bottomRight) {
		Set(_topLeft, _topRight, _bottomLeft, _bottomRight);
	}

	/// <summary>
	/// Setter.
	/// </summary>
	/// <param name="_g">Source gradeitn.</param>
	public void Set(Gradient4 _g) {
		if(_g == null) return;
		Set(_g.topLeft, _g.topRight, _g.bottomLeft, _g.bottomRight);
	}

	/// <summary>
	/// Setter.
	/// </summary>
	/// <param name="_topLeft">Top left color.</param>
	/// <param name="_topRight">Top right color.</param>
	/// <param name="_bottomLeft">Bottom left color.</param>
	/// <param name="_bottomRight">Bottom right color.</param>
	public void Set(Color _topLeft, Color _topRight, Color _bottomLeft, Color _bottomRight) {
		topLeft = _topLeft;
		topRight = _topRight;
		bottomLeft = _bottomLeft;
		bottomRight = _bottomRight;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Evaluate the color at a given coordinate.
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