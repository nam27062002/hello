// Texture2DExtensions.cs
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
/// Custom extensions to the Texture2D class.
/// </summary>
public static class Texture2DExtensions {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// STATIC EXTENSION METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Fill the texture with the given color.
	/// </summary>
	/// <param name="_texture">The texture to be filled.</param>
	/// <param name="_color">The color to be applied.</param>
	public static void Fill(this Texture2D _texture, Color _color) {
		// Nothing fancy: iterate through all the pixels in the texture and perform the color multiplication
		Color[] pixels = _texture.GetPixels();
		for(int i = 0; i < pixels.Length; i++) {
			pixels[i] = _color;
		}
		_texture.SetPixels(pixels);
		_texture.Apply();
	}

	/// <summary>
	/// Tint the texture with the given color.
	/// Tinting is done by multiplying all pixels in the texture by the giving color, RGBA component by component.
	/// </summary>
	/// <param name="_texture">The texture to be tinted.</param>
	/// <param name="_color">The color to be applied.</param>
	public static void Tint(this Texture2D _texture, Color _color) {
		// Nothing fancy: iterate through all the pixels in the texture and perform the color multiplication
		Color[] pixels = _texture.GetPixels();
		for(int i = 0; i < pixels.Length; i++) {
			pixels[i] *= _color;
		}
		_texture.SetPixels(pixels);
		_texture.Apply();
	}

	//------------------------------------------------------------------//
	// OTHER STATIC TOOLS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Create a new texture with the given parameters.
	/// The texture will be width by height size, with an ARGB32 TextureFormat, with mipmaps and in sRGB color space.
	/// </summary>
	/// <param name="_width">The width of the texture in pixels.</param>
	/// <param name="_height">The height of the texture in pixels.</param>
	/// <param name="_color">The color used to initialize all the pixels in the textures.</param>
	public static Texture2D Create(int _width, int _height, Color _color) {
		// Easy thanks to the other extension methods
		Texture2D tex = new Texture2D(_width, _height);
		tex.Fill(_color);
		return tex;
	}
}
