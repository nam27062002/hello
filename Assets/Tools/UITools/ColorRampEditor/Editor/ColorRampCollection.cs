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
using System.Collections.Generic;

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
		//--------------------------------------------------------------//
		// CONSTANTS													//
		//--------------------------------------------------------------//
		public const int RAMP_WIDTH = 256;   // Matching 1 pixel per Grayscale index
		public const int RAMP_HEIGHT = 1;

		public enum GradientSequenceType {
			HORIZONTAL,
			VERTICAL
		}

		//--------------------------------------------------------------//
		// MEMBERS AND PROPERTIES										//
		//--------------------------------------------------------------//
		// Data
		public Texture2D tex = null;
		public GradientSequenceType type = GradientSequenceType.VERTICAL;
		public Gradient[] gradients = new Gradient[0];
		//public RangeInt[] indices = new RangeInt[0];	// Only used for horizontal sequences

		// Control vars
		[NonSerialized] public bool dirty = false;
		[NonSerialized] private ColorRampData m_editBackup = null;	// Temp object to backup original values while editing so changes can be discarded

		//--------------------------------------------------------------//
		// METHODS														//
		//--------------------------------------------------------------//
		/// <summary>
		/// Default constructor.
		/// </summary>
		public ColorRampData() {
			// Create backup
			Backup();
		}

		/// <summary>
		/// Parametrized constructor.
		/// </summary>
		public ColorRampData(bool _createBackup) {
			// Backup only if required
			if(_createBackup) Backup();
		}

		/// <summary>
		/// Updates texture content with data from the gradient.
		/// </summary>
		public void RefreshTexture() {
			// Is texture initialized?
			if(tex == null) return;

			// Aux vars
			Color[] pixels = null;
			int pixelIdx = 0;
			Vector2Int targetSize = new Vector2Int();

			// Different techniques depending on mode
			switch(type) {
				case GradientSequenceType.HORIZONTAL: {
					// One gradient after another
					// [AOC] TODO!! Add different weights!
					// Resize texture if needed and get pixels array
					targetSize.Set(RAMP_WIDTH, RAMP_HEIGHT);
					if(tex.width != targetSize.x || tex.height != targetSize.y) {
						tex.Resize(targetSize.x, targetSize.y);
					}
					pixels = tex.GetPixels();

					// Sort indices
					/*List<Vector2Int> sortedInidices = new List<Vector2Int>(indices.Length);
						for(int i = 0; i < indices.Length; ++i) {
							sortedInidices.Add(new Vector2Int(indices[i], ))
						}*/

					// Write pixels
					float pixelsPerGradient = Mathf.Max(Mathf.Floor((float)tex.width / (float)gradients.Length), 1f);	// At least 1px
					for(int i = 0; i < gradients.Length; ++i) {
						for(int y = 0; y < tex.height; ++y) {
								// Find out target indices for this 
							for(int x = 0; x < pixelsPerGradient; ++x) {
								pixelIdx = y * tex.width + (i * (int)pixelsPerGradient + x);
								if(pixelIdx < pixels.Length) {
									pixels[pixelIdx] = gradients[i].Evaluate((float)x / pixelsPerGradient);
								}
							}
						}
					}
				} break;

				case GradientSequenceType.VERTICAL: {
					// One gradient on top of another
					// Resize texture if needed and get pixels array
					targetSize.Set(RAMP_WIDTH, gradients.Length * RAMP_HEIGHT);
					if(tex.width != targetSize.x || tex.height != targetSize.y) {
						tex.Resize(targetSize.x, targetSize.y);
					}
					pixels = tex.GetPixels();

					// Write pixels
					for(int y = 0; y < tex.height; ++y) {
						int gradientIdx = gradients.Length - y - 1;	// Reverse order to match editor view
						for(int x = 0; x < tex.width; ++x) {
							pixelIdx = y * tex.width + x;
							if(pixelIdx < pixels.Length) {
								pixels[pixelIdx] = gradients[gradientIdx].Evaluate((float)x / (float)tex.width);
							}
						}
					}
				} break;
			}

			// Upload to texture
			if(pixels != null) {
				tex.SetPixels(pixels);
				tex.Apply();
			}

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

			// Update gradients backup
			Backup();

			// Not dirty anymore!
			dirty = false;
		}

		/// <summary>
		/// Discard current values to the backup, if backup properly initialized.
		/// Reloads texture from disk as well.
		/// </summary>
		public void Discard() {
			// Restore gradients from the backup copy
			if(m_editBackup != null) {
				// Restore values from the backup
				this.CopyValuesFrom(m_editBackup);

				// Gradient changed, refresh texture
				RefreshTexture();
			}
		}

		/// <summary>
		/// Store current gradients into the backup array.
		/// </summary>
		private void Backup() {
			// If backup object is not created, do it now
			if(m_editBackup == null) {
				m_editBackup = new ColorRampData(false);	// Dont create backup for the backup! :D
			}

			// Perform the backup
			m_editBackup.CopyValuesFrom(this);
		}

		/// <summary>
		/// Copy ramp data values from another ramp data object.
		/// </summary>
		/// <param name="_source">Source color ramp.</param>
		public void CopyValuesFrom(ColorRampData _source) {
			// Skip if source not valid
			if(_source == null) return;

			// Copy gradients
			// Make sure the gradients array has the same length as the backup
			if(this.gradients == null || this.gradients.Length != _source.gradients.Length) {
				this.gradients = new Gradient[_source.gradients.Length];
			}

			// Restore gradients one by one
			for(int i = 0; i < this.gradients.Length; ++i) {
				this.gradients[i] = new Gradient();
				this.gradients[i].SetKeys(_source.gradients[i].colorKeys, _source.gradients[i].alphaKeys);
				this.gradients[i].mode = _source.gradients[i].mode;
			}

			// Copy inidices
		}
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	public ColorRampData[] ramps = new ColorRampData[0];
}