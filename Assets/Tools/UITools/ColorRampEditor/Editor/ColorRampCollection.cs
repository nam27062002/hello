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
using System.IO;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Persistence data for the Editor.
/// </summary>
[CreateAssetMenu(fileName = "NewColorRampCollection", menuName = "HungryDragon/Color Ramp Collection")]
public class ColorRampCollection : ScriptableObject {
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

		// Control vars
		[NonSerialized] public bool dirty = false;
		[NonSerialized] public Gradient gradientBackup = new Gradient();

		/// <summary>
		/// Default constructor.
		/// </summary>
		public ColorRampData() {
			// Backup gradient
			gradientBackup.SetKeys(gradient.colorKeys, gradient.alphaKeys);
			gradientBackup.mode = gradient.mode;
		}

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

			// Mark it as dirty
			dirty = true;
        }

		/// <summary>
		/// Save texture file to disk.
		/// </summary>
		public void SaveTexture() {
			// Is texture initialized?
			if(tex == null) return;

			// Do it!
			string path = AssetDatabase.GetAssetPath(tex);
			File.WriteAllBytes(path, tex.EncodeToPNG());
			AssetDatabase.SaveAssets();

			// Update gradient backup
			gradientBackup.SetKeys(gradient.colorKeys, gradient.alphaKeys);
			gradientBackup.mode = gradient.mode;

			// Not dirty anymore!
			dirty = false;
		}

		/// <summary>
		/// Discard current gradient to the backup, if backup properly initialized.
		/// Reloads texture from disk as well.
		/// </summary>
		public void DiscardGradient() {
			// Restore gradient
			if(gradientBackup != null) {
				gradient.SetKeys(gradientBackup.colorKeys, gradientBackup.alphaKeys);
				gradient.mode = gradientBackup.mode;

				// Gradient changed, refresh texture
				RefreshTexture();
			}
		}
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	public ColorRampData[] ramps = new ColorRampData[0];
}