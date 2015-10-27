// SpawnerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI() {
		// Aux vars
		Spawner sp = target as Spawner;
		GameObject oldEntityPrefab = sp.m_entityPrefab;

		// Default
		DrawDefaultInspector();

		// Button to recreate object's icon
		GUILayout.Space(15);
		bool recreateIcon = GUILayout.Button("Recreate Icon");
		GUILayout.Space(5);

		// Check game object's icon: if not defined assign the entity prefab's preview as icon
		// Do it also if the prefab has changed
		// Also add a button to manually do it
		if(recreateIcon
		|| EditorUtils.GetObjectIcon(sp.gameObject) == null
		|| oldEntityPrefab != sp.m_entityPrefab) {
			GenerateIcon(sp);
		}
	}
	
	/// <summary>
	/// 
	/// </summary>
	public void OnSceneGUI() {

	}

	/// <summary>
	/// Automatically genereates and assigns the icon for the given spawner's game object.
	/// The icon will be a preview of the spawner's entity prefab, if defined, or a default yellow label otherwise.
	/// </summary>
	/// <param name="_sp">The spawner to be changed.</param>
	private void GenerateIcon(Spawner _sp) {
		if(_sp.m_entityPrefab != null) {
			// Generate a new texture
			Texture2D tex = AssetPreview.GetAssetPreview(_sp.m_entityPrefab);
			
			// Remove grey background
			if(tex != null) {
				Color toReplace = tex.GetPixel(0, 0);	// [AOC] Assume first pixel will always be background - happy assumption
				Color replacement = new Color(0f, 0f, 0f, 0.25f);	// Make it quite transparent - full transparent is confusing, looks like a 3D object
				Color[] pixels = tex.GetPixels();
				for(int i = 0; i < pixels.Length; i++) {
					if(pixels[i] == toReplace) {
						pixels[i] = replacement;
					}
				}
				tex.SetPixels(pixels);
				tex.Apply();
			}
			
			// Use the customized texture as icon
			EditorUtils.SetObjectIcon(_sp.gameObject, tex);
		} else {
			// Use default icon
			EditorUtils.SetObjectIcon(_sp.gameObject, EditorUtils.ObjectIcon.LABEL_YELLOW);
		}
	}
}