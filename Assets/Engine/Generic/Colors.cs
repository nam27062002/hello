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

	public static readonly Color green = new Color(0f, 1f, 0f);
	public static readonly Color paleGreen = new Color(0.5f, 1f, 0.5f);
	public static readonly Color darkGreen = new Color(0f, 0.5f, 0f);

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
	public static readonly Color lime = new Color(0f, 1f, 0f);
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

	//------------------------------------------------------------------//
	// EXTENSIONS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Convert this Color32 into a Color.
	/// </summary>
	/// <returns>The color.</returns>
	/// <param name="_source">Source Color32.</param>
	public static Color ToColor(this Color32 _source) {
		return new Color(
			_source.r/255f,
			_source.g/255f,
			_source.b/255f,
			_source.a/255f
		);
	}

	/// <summary>
	/// Convert this Color into a Color32.
	/// </summary>
	/// <returns>The color 32.</returns>
	/// <param name="_source">Source Color.</param>
	public static Color32 ToColor32(this Color _source) {
		// Clamp to prevent unexpected values
		return new Color32(
			(byte)(Mathf.Clamp01(_source.r) * 255f),
			(byte)(Mathf.Clamp01(_source.g) * 255f),
			(byte)(Mathf.Clamp01(_source.b) * 255f),
			(byte)(Mathf.Clamp01(_source.a) * 255f)
		);
	}

	/// <summary>
	/// Initialize color from rgba components.
	/// </summary>
	/// <param name="_source">Color being modified.</param>
	/// <param name="_r">R.</param>
	/// <param name="_g">G.</param>
	/// <param name="_b">B.</param>
	/// <param name="_a">A.</param>
	public static void Set(this Color _source, float _r, float _g, float _b, float _a) {
		_source.r = _r;
		_source.g = _g;
		_source.b = _b;
		_source.a = _a;
	}

	/// <summary>
	/// Initialize color from rgb components.
	/// </summary>
	/// <param name="_source">Color being modified.</param>
	/// <param name="_r">R.</param>
	/// <param name="_g">G.</param>
	/// <param name="_b">B.</param>
	public static void Set(this Color _source, float _r, float _g, float _b) {
		_source.r = _r;
		_source.g = _g;
		_source.b = _b;
	}

	/// <summary>
	/// Initialize color from a vector.
	/// </summary>
	/// <param name="_source">Color being modified.</param>
	/// <param name="_v">Vector with the new values.</param>
	public static void Set(this Color _source, Vector3 _v) {
		_source.r = _v[0];
		_source.g = _v[1];
		_source.b = _v[2];
	}

	/// <summary>
	/// Get the rgb components of the color.
	/// </summary>
	/// <param name="_source">Source color.</param>
	public static Vector3 RGB(this Color _source) {
		return new Vector3(_source.r, _source.g, _source.b);
	}
}
