// ColorUtils.cs
// 
// Created by Alger Ortín Castellví on 29/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Since constants can't be added to a class using c# extensions, use this class to
/// add extra color definitions.
/// </summary>
public static class Colors {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// Grayscale
	public static readonly Color white = new Color(1f, 1f, 1f);
	public static readonly Color silver = new Color(0.75f, 0.75f, 0.75f);
	public static readonly Color gray = new Color(0.5f, 0.5f, 0.5f);
	public static readonly Color darkGray = new Color(0.25f, 0.25f, 0.25f);
	public static readonly Color black = new Color(0f, 0f, 0f);

	// Primary (base, lightened and darkened)
	public static readonly Color red = new Color(1f, 0f, 0f);
	public static readonly Color coral = new Color(1f, 0.5f, 0.5f);
	public static readonly Color maroon = new Color(0.5f, 0f, 0f);

	public static readonly Color lime = new Color(0f, 1f, 0f);
	public static readonly Color paleGreen = new Color(0.5f, 1f, 0.5f);
	public static readonly Color green = new Color(0f, 0.5f, 0f);

	public static readonly Color blue = new Color(0f, 0f, 1f);
	public static readonly Color slateBlue = new Color(0.5f, 0.5f, 1f);
	public static readonly Color navy = new Color(0f, 0f, 0.5f);

	// Secondary (base, ligthened and darkened)
	public static readonly Color yellow = new Color(1f, 1f, 0f);
	public static readonly Color paleYellow = new Color(1f, 1f, 0.5f);
	public static readonly Color olive = new Color(0.5f, 0.5f, 0f);
	
	public static readonly Color aqua = new Color(0f, 1f, 1f);
	public static readonly Color cyan = new Color(0.5f, 1f, 1f);
	public static readonly Color teal = new Color(0f, 0.5f, 0.5f);
	
	public static readonly Color magenta = new Color(1f, 0f, 1f);
	public static readonly Color violet = new Color(1f, 0.5f, 1f);
	public static readonly Color purple = new Color(0.5f, 0f, 0.5f);

	// Alternative names and extra colors
	public static readonly Color skyBlue = new Color(0.2f, 0.8f, 1f);
	public static readonly Color fuchsia = new Color(1f, 0f, 1f);
	public static readonly Color pink = new Color(1f, 0.6f, 0.8f);
	public static readonly Color orange = new Color(1f, 0.5f, 0f);
	public static readonly Color gold = new Color(1f, 0.8f, 0f);

	// Special
	public static readonly Color transparentBlack = new Color(0f, 0f, 0f, 0f);
	public static readonly Color transparentWhite = new Color(1f, 1f, 1f, 0f);

	//------------------------------------------------------------------//
	// STATIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Get a color applying a given alpha value in a single line, for example:
	/// <code>
	/// Color c = Colors.WithAlpha(Colors.red, 0.5f);
	/// </code>
	/// instead of
	/// <code>
	/// Color c = Colors.red;
	/// c.a = 0.5f;
	/// </code>
	/// </summary>
	/// <returns>The input color with the given alpha applied.</returns>
	/// <param name="_color">The base color.</param>
	/// <param name="_alpha">The alpha to be used.</param>
	public static Color WithAlpha(Color _color, float _alpha) {
		return new Color(_color.r, _color.g, _color.b, _alpha);
	}
}
