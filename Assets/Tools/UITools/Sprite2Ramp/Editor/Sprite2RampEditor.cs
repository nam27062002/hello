// Sprite2RampEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/06/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Main window for the Sprite2Ramp tool.
/// </summary>
public class Sprite2RampEditor : EditorWindow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Layout constants
	private const float SPACING = 2f;
	private const float WINDOW_MARGIN = 10f;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Window instance
	private static Sprite2RampEditor s_instance = null;
	public static Sprite2RampEditor instance {
		get {
			if(s_instance == null) {
				s_instance = (Sprite2RampEditor)EditorWindow.GetWindow(typeof(Sprite2RampEditor));
				s_instance.titleContent.text = "Sprite2Ramp";
			}
			return s_instance;
		}
	}

	// Editor GUI
	private Vector2 m_scrollPos = Vector2.zero;
	private Sprite m_toReplaceSprite = null;
	private Sprite m_grayscaleBaseSprite = null;
	private Texture2D m_gradientTex = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Opens the window.
	/// </summary>
	[MenuItem("Tools/Sprite2Ramp", false, 301)]
	public static void OpenWindow() {
		instance.Show();
		//instance.ShowUtility();
		//instance.ShowTab();
		//instance.ShowPopup();
		//instance.ShowAuxWindow();
	}

	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
        
    }

	/// <summary>
	/// Called 100 times per second on all visible windows.
	/// </summary>
	public void Update() {
		
	}

	/// <summary>
	/// OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update.
	/// Called less times as if it was OnGUI/Update
	/// </summary>
	public void OnInspectorUpdate() {
		
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
		// Nothing to do in play mode!
		if(Application.isPlaying) {
			// Error message if missing parameters
			EditorGUILayout.HelpBox("Can't be used in Play mode!", MessageType.Error);
			return;
		}

		// Scroll Rect
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos); {
			// Left Margin
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(WINDOW_MARGIN);

			// Top margin
			EditorGUILayout.BeginVertical();
			GUILayout.Space(WINDOW_MARGIN);

			// DO STUFF!
			// Sprite to replace
			m_toReplaceSprite = EditorGUILayout.ObjectField("To Replace", m_toReplaceSprite, typeof(Sprite), true) as Sprite;

			// Replacements
			EditorGUILayout.Space();
			m_grayscaleBaseSprite = EditorGUILayout.ObjectField("Grayscale Base Sprite", m_grayscaleBaseSprite, typeof(Sprite), true) as Sprite;
			m_gradientTex = EditorGUILayout.ObjectField("Gradient Texture", m_gradientTex, typeof(Texture2D), true) as Texture2D;

			// Space
			EditorGUILayout.Space();

			// Error message if missing parameters
			bool parameterCheck = m_toReplaceSprite != null && m_grayscaleBaseSprite != null && m_gradientTex != null;
			if(!parameterCheck) {
				EditorGUILayout.HelpBox("Missing parameters!", MessageType.Error);
			}

			// Button - if all parameters are properly filled!
			EditorGUI.BeginDisabledGroup(!parameterCheck);
			if(GUILayout.Button("REPLACE!!", GUILayout.Height(30f))) {
				// Perform the replacement!
				DoReplace();
			}
			EditorGUI.EndDisabledGroup();

			// Bottom margin
			GUILayout.Space(WINDOW_MARGIN);
			EditorGUILayout.EndVertical();

			// Right margin
			GUILayout.Space(WINDOW_MARGIN);
			EditorGUILayout.EndHorizontal();
		} EditorGUILayout.EndScrollView();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Perform the replacement in all open sceen!
	/// </summary>
	private void DoReplace() {
		// Aux vars
		HashSet<GameObject> modifiedObjects = new HashSet<GameObject>();

		// Find all references to the target sprite in the opened scenes
		// For the purpose of this tool, assume the sprite will be in an Image component
		//Image[] allImages = GameObject.FindObjectsOfType<Image>();	// [AOC] This skips disabled objects, we want them all!
		Image[] allImages = Resources.FindObjectsOfTypeAll<Image>();

		// [AOC] Unfortunately Resources.FindObjectsOfTypeAll returns us not only 
		//		  the objects in the scene, but anything that is loaded in the editor
		//		  at this moment (Project view, Inspector view, etc)
		//		  Use EditorUtility.IsPersistent to filter the list
		//		  Use the same loop to filter only those images targeting the sprite to replace.
		Image[] filteredImages = allImages.Where((Image _img) => {
			// Is it in the scene?
			if(EditorUtility.IsPersistent(_img)) return false;

			// Does it target the sprite to replace? Several ways of figuring out, choose one
			if(_img.sprite != m_toReplaceSprite) return false;
			//if(imagesInScene[i].sprite.GetHashCode() == m_toReplaceSprite.GetHashCode()) return false;
			//if(spriteGUID == toReplaceGUID) return false;

			// All checks passed!
			return true;
		}).ToArray();

		// Iterate through all selected Images and perform the replacement
		bool canceled = false;
		for(int i = 0; i < filteredImages.Length; ++i) {
			// Show progress bar
			if(EditorUtility.DisplayCancelableProgressBar(
				"Replacing " + m_toReplaceSprite.name + " in active scenes",
				"Analyizing Image component " + (i + 1).ToString() + "/" + filteredImages.Length + "... (" + filteredImages[i].name + ")",
				(float)i / (float)filteredImages.Length
			)) {
				canceled = true;
				break;	// Canceled, break loop
			}


			// Perform the replacement!
			// Easier naming
			Image targetImage = filteredImages[i];

			// Gather prefab's root for this image
			GameObject rootObj = PrefabUtility.FindRootGameObjectWithSameParentPrefab(targetImage.gameObject);

			// Replace target sprite by the grayscale one
			targetImage.sprite = m_grayscaleBaseSprite;

			// Get the UIColorFX component from this object. Add one if the object doesn't have it.
			UIColorFX targetFX = targetImage.GetComponent<UIColorFX>();
			if(targetFX == null) {
				targetFX = targetImage.gameObject.AddComponent<UIColorFX>();
			}

			// Set up UIColorFX to use the target color ramp
			targetFX.colorRampEnabled = true;
			targetFX.colorRamp = m_gradientTex;

			// Update log
			modifiedObjects.Add(rootObj);
		}

		// Close progress bar
		EditorUtility.ClearProgressBar();

		// Process modified objects
		int processedPrefabsCount = 0;
		foreach(GameObject obj in modifiedObjects) {
			// Show progress bar
			EditorUtility.DisplayProgressBar(
				canceled ? "Reverting changes" : "Applying changes to prefabs",
				"Processing prefab " + (processedPrefabsCount + 1).ToString() + "/" + modifiedObjects.Count + "... (" + obj.name + ")",
				(float)processedPrefabsCount / (float)modifiedObjects.Count
			);

			// If canceled, just revert changes
			if(canceled) {
				PrefabUtility.RevertPrefabInstance(obj);
			} else {
				// Make sure root prefab object is active (we want to save the prefabs active)
				bool wasActive = obj.activeSelf;
				obj.SetActive(true);

				// Re-apply the prefab
				PrefabUtility.ReplacePrefab(
					obj,
					PrefabUtility.GetPrefabParent(obj),
					ReplacePrefabOptions.ConnectToPrefab
				);

				// Restore original activation state
				obj.SetActive(wasActive);
			}

			// Done!
			processedPrefabsCount++;
		}

		// Close progress bar
		EditorUtility.ClearProgressBar();

		// Clear modified objects list if canceled
		if(canceled) {
			modifiedObjects.Clear();
		}

		// Display summary dialog
		Sprite2RampEditorSummaryWindow.OpenWindow(modifiedObjects);
		GUIUtility.ExitGUI();
	}


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}