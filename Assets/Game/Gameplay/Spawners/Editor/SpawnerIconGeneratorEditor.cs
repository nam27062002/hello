// SpawnerIconGeneratorEditor.cs
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
using System.IO;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[CustomEditor(typeof(SpawnerIconGenerator))]
public class SpawnerIconGeneratorEditor : Editor {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private SpawnerIconGenerator typedTarget {
		get { return target as SpawnerIconGenerator; }
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private GameObject m_entityPrefab = null;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// The inspector has been opened, initialize.
	/// </summary>
	public void OnEnable() {
		Spawner sp = typedTarget.GetComponent<Spawner>();
		if(sp != null) {
			m_entityPrefab = sp.m_entityPrefab;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI() {
        // Get the spawner
        /*Spawner sp = typedTarget.GetComponent<Spawner>();
		if(sp == null) {
			EditorGUILayout.HelpBox("No spawner component could be found.", MessageType.Error);
			m_entityPrefab = null;
			return;
		}

		// Horizontal distribution
		Color newColor = typedTarget.m_backgroundColor;
		EditorGUILayout.BeginHorizontal(); {
			// Show current icon preview
			GUILayout.Box(typedTarget.m_tex, GUILayout.Width(100), GUILayout.Height(100));

			// Background color selector
			EditorGUILayout.BeginVertical(GUILayout.Width(150), GUILayout.Height(100)); {
				GUILayout.FlexibleSpace();
				GUILayout.Label("Background Color:");
				newColor = EditorGUILayout.ColorField(typedTarget.m_backgroundColor);
				GUILayout.FlexibleSpace();
			} EditorGUILayoutExt.EndVerticalSafe();
		} EditorGUILayoutExt.EndHorizontalSafe();

		// Button to manually recreate object's icon
		GUILayout.Space(5);
		bool recreateIcon = GUILayout.Button("Recreate Icon");
		GUILayout.Space(5);

		// Recreate the icon?
		if(recreateIcon		// Button has been pressed
		|| EditorUtils.GetObjectIconEnum(sp.gameObject) != EditorUtils.ObjectIcon.CUSTOM	// Icon null or default
		|| m_entityPrefab != sp.m_entityPrefab		// Prefab has changed
		|| newColor != typedTarget.m_backgroundColor) {		// Background color has changed
			// Generate icon
			GenerateIcon(typedTarget, sp);
		}

		// Store latest values
		m_entityPrefab = sp.m_entityPrefab;
		typedTarget.m_backgroundColor = newColor;*/
        GUILayout.Box("Disabled while finding a fix");
	}
	
	/// <summary>
	/// 
	/// </summary>
	public void OnSceneGUI() {

	}

	//------------------------------------------------------------------//
	// TOOLS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Regenerate the icon for all the spawners in the scene.
	/// </summary>
	public static void GenerateSpawnerIconsInScene() {
		Spawner[] spawners = FindObjectsOfType<Spawner>();
		for(int i = 0; i < spawners.Length; i++) {
			// If the spawner doesn't have an icon generator, add it now
			SpawnerIconGenerator generator = spawners[i].GetComponent<SpawnerIconGenerator>();
			if(generator == null) {
				generator = spawners[i].gameObject.AddComponent<SpawnerIconGenerator>();
			}

            // Generate icon for this spawner
			GenerateIcon(generator, spawners[i]);
		}
	}

	/// <summary>
	/// Automatically genereates and assigns the icon for the given spawner's game object.
	/// The icon will be a preview of the spawner's entity prefab, if defined, or a default label otherwise.
	/// </summary>
	/// <param name="_gen">The generator to be used.</param>
	/// <param name="_sp">The spawner to be changed.</param>
	private static void GenerateIcon(SpawnerIconGenerator _gen, Spawner _sp) {
        /*Texture2D tex = null;
		if(_sp.m_entityPrefab != null) {
			// Generate a new texture
			tex = AssetPreview.GetAssetPreview(_sp.m_entityPrefab);
			if(tex != null) {
				// Create a copy, we don't want to modify the source
				tex = Instantiate<Texture2D>(tex);
				
				// Remove ugly grey background
				Color toReplace = tex.GetPixel(0, 0);	// [AOC] Assume first pixel will always be background - happy assumption
				Color replacement = _gen.m_backgroundColor;
				if(toReplace != replacement) {
					Color[] pixels = tex.GetPixels();
					for(int i = 0; i < pixels.Length; i++) {
						if(pixels[i] == toReplace) {
							pixels[i] = replacement;
						}
					}
					tex.SetPixels(pixels);
					tex.Apply();
				}
				
				// Save to file (replace any existing)
				// [AOC] We must save it to a file in order to persist between sessions, otherwise the icon will be reseted once the scene is closed
				byte[] bytes = tex.EncodeToPNG();
				string iconFilePath = Application.dataPath + "/Tools/EditorIcons/" + _sp.m_entityPrefab.name + ".png";
				System.IO.File.WriteAllBytes(iconFilePath, bytes);
				
				// Import newly created png to assets database (so we have a GUID and all)
				iconFilePath = iconFilePath.Replace(Application.dataPath, "Assets");
				AssetDatabase.ImportAsset(iconFilePath);
				_gen.m_tex = AssetDatabase.LoadAssetAtPath<Texture2D>(iconFilePath);
				EditorUtils.SetObjectIcon(_sp.gameObject, _gen.m_tex);
				tex = _gen.m_tex;
			}
		}
		
		// Use default icon if prefab was null or no icon could be generated
		if(tex == null) {
			// Use default icon
			EditorUtils.SetObjectIcon(_sp.gameObject, EditorUtils.ObjectIcon.LABEL_RED);
		}*/
        EditorUtils.SetObjectIcon(_sp.gameObject, EditorUtils.ObjectIcon.LABEL_ORANGE);
    }

	/// <summary>
	/// Create an icon for the given prefab and store it under the "Gizmos/Spawners" folder
	/// with the name of the prefab. If an icon with that name already exists, it will be
	/// overwritten.
	/// </summary>
	/// <param name="_entityPrefab">The prefab whose icon we want to create.</param>
	/// <param name="_backgroundColor">The background color of the icon (recommended to use full transparency).</param>
	public static void GenerateIcon(GameObject _entityPrefab, Color _backgroundColor) {
		// Generate a new texture
		Texture2D tex = AssetPreview.GetAssetPreview(_entityPrefab);
		if(tex != null) {
			// Create a copy, we don't want to modify the source
			tex = Instantiate<Texture2D>(tex);

			// Remove ugly grey background
			Color toReplace = tex.GetPixel(0, 0);	// [AOC] Assume first pixel will always be background - happy assumption
			Color replacement = _backgroundColor;
			if(toReplace != replacement) {
				Color[] pixels = tex.GetPixels();
				for(int j = 0; j < pixels.Length; j++) {
					if(pixels[j] == toReplace) {
						pixels[j] = replacement;
					}
				}
				tex.SetPixels(pixels);
				tex.Apply();
			}

			// Save to file (replace any existing)
			// [AOC] We must save it to a file in order to persist between sessions, otherwise the icon will be reseted once the scene is closed
			byte[] bytes = tex.EncodeToPNG();
			string iconFilePath = Application.dataPath + "/Gizmos/Spawners/" + _entityPrefab.name + ".png";
			System.IO.File.WriteAllBytes(iconFilePath, bytes);

			// Import newly created png to assets database (so we have a GUID and all)
			iconFilePath = iconFilePath.Replace(Application.dataPath, "Assets");
			AssetDatabase.ImportAsset(iconFilePath);

			// Setup import settings
			TextureImporter importer = TextureImporter.GetAtPath(iconFilePath) as TextureImporter;
			if(importer != null) {
				importer.alphaIsTransparency = true;
				AssetDatabase.ImportAsset(iconFilePath, ImportAssetOptions.ForceUpdate);
			}
		}
	}

	/// <summary>
	/// Creates icons for all the entity prefabs stored under the Resources/Game/Entities folder.
	/// New icons will be stored under the Gizmos/ folder (as per Unity rules).
	/// Existing icons will be overwritten.
	/// </summary>
	/// <param name="_backgroundColor">The background color of the icon (recommended to use full transparency).</param>
	public static void GenerateSpawnerIconsInResources(Color _backgroundColor) {
		// Load all prefabs under the Entites folder
		List<GameObject> entityPrefabs = ResourcesExt.LoadRecursively("Game/Entities");
		for(int i = 0; i < entityPrefabs.Count; i++) {
			// Just create an icon for each prefab
			GenerateIcon(entityPrefabs[i], _backgroundColor);
		}
	}
}