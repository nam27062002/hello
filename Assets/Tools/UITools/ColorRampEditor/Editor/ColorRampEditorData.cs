// ColorRampEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/05/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Persistence data for the Editor.
/// </summary>
public class ColorRampEditorData : ScriptableObject {
	//------------------------------------------------------------------//
	// NESTED CLASSES													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Auxiliar class to store serialized data of a single color ramp.
	/// </summary>
	[Serializable]
	public class ColorRampData {
		// Data
		public Gradient gradient = new Gradient();
		public Texture2D tex = null;

		/// <summary>
		/// Updates texture content with data from the gradient.
		/// </summary>
		public void RefreshTexture() {
			// Is texture initialized?
			if(tex == null) return;

			// Do it :)
			float fWidth = (float)tex.width;
			Color[] pixels = tex.GetPixels();
			for(int y = 0; y < tex.height; ++y) {
				for(int x = 0; x < tex.width; ++x) {
					pixels[y * tex.width + x] = gradient.Evaluate((float)x / fWidth);
				}
			}
			tex.SetPixels(pixels);
			tex.Apply();
        }
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	public ColorRampData[] ramps = new ColorRampData[0];
}