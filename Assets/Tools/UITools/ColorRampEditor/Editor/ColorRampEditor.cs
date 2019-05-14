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
using UnityEditorInternal;
using System;
using System.IO;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor window for the color ramp editor tool.
/// </summary>
public class ColorRampEditor : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// Data constants
	public const string DATA_PATH = "Assets/Resources/Editor/ColorRampCollections/";

	// Layout constants
	private const float SPACING = 2f;
	private const float WINDOW_MARGIN = 10f;
	private const float INFO_BOX_MARGIN = 5f;

	// Prefs constants
	private const string SELECTION_COLLECTION_NAME_KEY = "ColorRampEditor.SELECTION_COLLECTION_NAME";
	private string selectedCollectionName {
		get { return EditorPrefs.GetString(SELECTION_COLLECTION_NAME_KEY, ""); }
		set { EditorPrefs.SetString(SELECTION_COLLECTION_NAME_KEY, value); }
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Window instance
	private static ColorRampEditor s_instance = null;
	public static ColorRampEditor instance {
		get {
			if(s_instance == null) {
				s_instance = (ColorRampEditor)EditorWindow.GetWindow(typeof(ColorRampEditor));
				s_instance.titleContent.text = "Color Ramp Editor";
			}
			return s_instance;
		}
	}

	// Custom styles
	private static GUIStyle s_helpBoxTextStyle = null;
	private static GUIStyle HELP_BOX_TEXT_STYLE {
		get {
			if(s_helpBoxTextStyle == null) {
				s_helpBoxTextStyle = new GUIStyle(GUI.skin.label);
				s_helpBoxTextStyle.alignment = TextAnchor.MiddleLeft;
				s_helpBoxTextStyle.richText = true;
			}
			return s_helpBoxTextStyle;
		}
	}

	// Data
	private ColorRampCollection m_currentCollection = null;
	private ColorRampCollection currentCollection {
		get {
			LoadCollectionIfNeeded();
			return m_currentCollection;
		}
	}

	// GUI widgets
	private ReorderableList m_rampsList = null;
	private float[] m_elementHeights = new float[0];

	// Internal
	private SerializedObject m_serializedCollection = null;
	private Vector2 m_scrollPos = Vector2.zero;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Opens the window.
	/// </summary>
	//[MenuItem("Hungry Dragon/Tools/ColorRampEditor")]	// UNCOMMENT TO ADD MENU ENTRY!!!
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
		// Load data
		LoadCollectionIfNeeded();
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
        // Make sure textures are properly saved
        //SaveTextures();
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
		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		if(m_serializedCollection != null) m_serializedCollection.Update();

		// Scroll Rect
		bool reloadCollection = false;
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos); {
			// Left Margin
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(WINDOW_MARGIN);

			// Top margin
			EditorGUILayout.BeginVertical();
			GUILayout.Space(WINDOW_MARGIN);

			// Window Content!
			// Collection Chooser
			EditorGUI.BeginChangeCheck();

			// Refresh files list
			{
				// [AOC] Would be nice to not having to do this always (performance), but so far is the only way to make sure the list is accurate
				DirectoryInfo dirInfo = new DirectoryInfo(DATA_PATH);
				FileInfo[] files = dirInfo.GetFiles();

				// Strip filename from full file path
				List<string> fileNames = new List<string>();
				for(int i = 0; i < files.Length; i++) {
					if(files[i].Name.EndsWith("asset", true, System.Globalization.CultureInfo.InvariantCulture)) {
						fileNames.Add(Path.GetFileNameWithoutExtension(files[i].Name));
					}
				}

				// Find index of our current collection
				int selectedIdx = fileNames.IndexOf(selectedCollectionName);

				// Show dropdown field
				selectedIdx = EditorGUILayout.Popup("Select Collection ", selectedIdx, fileNames.ToArray());

				if(EditorGUI.EndChangeCheck()) {
					// Store new collection
					selectedCollectionName = selectedIdx >= 0 ? fileNames[selectedIdx] : "";

					// Reload collection if required
					reloadCollection = true;
				}
			}

			// Color Ramps List
			if(m_rampsList != null) {
				m_rampsList.DoLayoutList();
			}

			// Bottom margin
			GUILayout.Space(WINDOW_MARGIN);
			EditorGUILayout.EndVertical();

			// Right margin
			GUILayout.Space(WINDOW_MARGIN);
			EditorGUILayout.EndHorizontal();
		} EditorGUILayout.EndScrollView();

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		if(m_serializedCollection != null) m_serializedCollection.ApplyModifiedProperties();

		// Did selected collection change?
		if(reloadCollection) LoadCollectionIfNeeded();
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Loads persistence data for the selected collection if not already loaded.
	/// If no data object exists at the target path, creates a new one.
	/// </summary>
	private void LoadCollectionIfNeeded() {
		// Is there a loaded collection?
		if(m_currentCollection != null) {
			// Is it the current collection?
			if(m_currentCollection.name == selectedCollectionName) {
				// Nothing to do ^^
				return;
			}
		}

		// Try to load existing data
		string path = DATA_PATH + selectedCollectionName + ".asset";
		m_currentCollection = AssetDatabase.LoadAssetAtPath(path, typeof(ColorRampCollection)) as ColorRampCollection;

		// Initialize reorderable list
		InitWithCurrentCollection();
	}

	/// <summary>
	/// Initialize reorderable list with collection.
	/// </summary>
	private void InitWithCurrentCollection() {
		// If no collection is loaded, unload everything
		if(m_currentCollection == null) {
			m_serializedCollection = null;
			m_rampsList = null;
			return;
		}

		// Serialize loaded data
		m_serializedCollection = new SerializedObject(m_currentCollection);

		// Reset scroll pos
		m_scrollPos = Vector2.zero;

		// Initialize list widget
		m_rampsList = new ReorderableList(m_serializedCollection, m_serializedCollection.FindProperty("ramps"));
		if(m_rampsList != null) {
			// Aux vars
			m_elementHeights = new float[m_currentCollection.ramps.Length];

			// Header
			m_rampsList.drawHeaderCallback = (Rect _rect) => {
				// Show title
				EditorGUI.LabelField(_rect, "Ramps List");
			};

			// Element
			m_rampsList.drawElementCallback = (Rect _rect, int _idx, bool _isActive, bool _isFocused) => {
				// Margins
				_rect.x += SPACING;
				_rect.width -= SPACING;
				_rect.y += SPACING;

				// Aux vars
				float currentLineY = _rect.y;

				// Get target data
				ColorRampCollection.ColorRampData rampData = m_currentCollection.ramps[_idx];
				SerializedProperty rampDataProp = m_rampsList.serializedProperty.GetArrayElementAtIndex(_idx);
				SerializedProperty gradientsProp = rampDataProp.FindPropertyRelative("gradients");
				SerializedProperty texProp = rampDataProp.FindPropertyRelative("tex");

				// Adjust prefix label size
				EditorGUIUtility.labelWidth = 90f;

				// Display texture
				Rect texRect = new Rect(
					_rect.x,
					currentLineY,
					_rect.width,
					EditorGUI.GetPropertyHeight(texProp)
				);
				EditorGUI.PropertyField(texRect, texProp, new GUIContent("Ramp Texture"), true);

				// Advance pos
				currentLineY += texRect.height + SPACING;

				// Display gradient
				// Disabled if no texture is assigned
				if(rampData.tex == null) {
					// Draw warning text
					Rect warningRect = new Rect(
						_rect.x,
						currentLineY,
						_rect.width,
						30f
					);
					EditorGUI.HelpBox(warningRect, "", MessageType.Info);

					Rect boxTextRect = new Rect(warningRect);
					boxTextRect.xMin += INFO_BOX_MARGIN + 20f;  // Leave room for the info box icon
					boxTextRect.yMin += INFO_BOX_MARGIN;
					boxTextRect.xMax -= INFO_BOX_MARGIN;
					boxTextRect.yMax -= INFO_BOX_MARGIN;
					GUI.Label(boxTextRect, "Assign a texture to edit", HELP_BOX_TEXT_STYLE);

					// Advance pos
					currentLineY += warningRect.height + SPACING;
				} else {
					// [AOC] EditorGUILayout.GradientField is not public before Unity 2018, 
					// so we're forced to use the serialized property.
					// We want to open the gradient editor through a button, to emphasize
					// the fact that the user will be modifying the texture as well.
					// Unfortunately, Unity doesn't give access to the gradient editor either, 
					// so we will drop the idea for now.

					// Detect changes
					EditorGUI.BeginChangeCheck();

					// Texture Preview
					Rect previewRect = new Rect(
						_rect.x,
						currentLineY,
						_rect.width,
						// EditorGUIUtility.singleLineHeight
						rampData.tex.height * 10
					);
					GUI.DrawTexture(previewRect, rampData.tex);

					// Advance pos
					currentLineY += previewRect.height + SPACING;

					// Sequence type
					SerializedProperty sequenceTypeProp = rampDataProp.FindPropertyRelative("type");
					Rect sequenceTypeRect = new Rect(
						_rect.x,
						currentLineY,
						_rect.width,
						EditorGUI.GetPropertyHeight(sequenceTypeProp)
					);
					EditorGUI.PropertyField(sequenceTypeRect, sequenceTypeProp, true);

					// Advance pos
					currentLineY += sequenceTypeRect.height + SPACING;

					// Draw Gradient field
					Rect gradientRect = new Rect(
						_rect.x,
						currentLineY,
						_rect.width,
						EditorGUI.GetPropertyHeight(gradientsProp)
					);
					EditorGUI.PropertyField(gradientRect, gradientsProp, true);

					// Advance pos
					currentLineY += gradientRect.height + SPACING;

					// Did the gradient change?
					if(EditorGUI.EndChangeCheck()) {
						// Yes! Generate texture
						rampData.RefreshTexture();
					}

					// If texture has been modified, show Save and Discard buttons
					if(rampData.dirty) {
						// Aux vars
						float buttonW = 80f;

						// Box
						Rect errorBoxRect = new Rect(
							_rect.x,
							currentLineY,
							_rect.width,
							30f
						);
						EditorGUI.HelpBox(errorBoxRect, "", MessageType.Warning);

						// Message
						Rect boxContentRect = new Rect(errorBoxRect);
						boxContentRect.xMin += INFO_BOX_MARGIN + 20f;   // Leave room for the info box icon
						boxContentRect.yMin += INFO_BOX_MARGIN;
						boxContentRect.xMax -= INFO_BOX_MARGIN + buttonW + SPACING + buttonW;
						boxContentRect.yMax -= INFO_BOX_MARGIN;
						GUI.Label(boxContentRect, Colors.yellow.Tag("Texture not saved!"), HELP_BOX_TEXT_STYLE);

						// Save button
						boxContentRect.xMin = boxContentRect.xMax;
						boxContentRect.width = buttonW;
						GUI.color = Colors.paleGreen;
						if(GUI.Button(boxContentRect, "Save")) {
							// Save to disk
							rampData.SaveTexture();
						}

						// Discard button
						boxContentRect.xMin = boxContentRect.xMax + SPACING;
						boxContentRect.width = buttonW;
						GUI.color = Colors.coral;
						if(GUI.Button(boxContentRect, "Discard")) {
							// Reload texture from disk
							rampData.Discard();
							rampData.SaveTexture();

							// [AOC] TODO!! Somehow the gradient preview doesn't get refreshed when restoring the gradient by script -___-
							// 		 Figure out a solution for it.
							/*
							EditorUtility.SetDirty(m_data);
							m_serializedData.Update();  // Otherwise the gradient preview will not refresh properly
							m_serializedData.ApplyModifiedProperties();
							Repaint();
							*/
						}
						GUI.color = Color.white;

						// Advance pos
						currentLineY += errorBoxRect.height + SPACING;
					}
				}

				// Restore prefix label size
				EditorGUIUtility.labelWidth = 0f;

				// Store element height
				m_elementHeights[_idx] = currentLineY - _rect.y + 3 * SPACING;    // Extra spacing
			};

			m_rampsList.elementHeightCallback = (int _idx) => {
				if(_idx >= m_elementHeights.Length) {
					m_elementHeights = new float[m_currentCollection.ramps.Length];
				}
				return m_elementHeights[_idx];
			};

			m_rampsList.drawElementBackgroundCallback = (Rect _rect, int _idx, bool _active, bool _focused) => {
				// [AOC] Empty lists give -1
				if(_idx < 0) return;

				// Using customized serialized properties
				float margin = SPACING;
				_rect.x += margin;
				_rect.width -= margin * SPACING;
				_rect.height -= SPACING;	// Leave some space without painting

				// Different colors based on GUI state
				ColorRampCollection.ColorRampData rampData = m_currentCollection.ramps[_idx];
				if(_active) {
					GUI.color = new Color(0.35f, 0.35f, 0.35f);     // Bright gray
				} else if(_focused) {
					GUI.color = new Color(0.32f, 0.32f, 0.32f);    // Medium gray
				} else {
					GUI.color = new Color(0.25f, 0.25f, 0.25f);     // Dark gray
				}

				// Blend with color by error state
				if(rampData.dirty) {
					GUI.color = Color.Lerp(GUI.color, Colors.gold, 0.25f);
				} else if(rampData.tex == null) {
					GUI.color = Color.Lerp(GUI.color, Colors.maroon, 0.25f);
				}

				// Draw background
				GUI.DrawTexture(_rect, Texture2D.whiteTexture);
				GUI.color = Color.white;
			};

			m_rampsList.onChangedCallback = OnListSizeChanged;
		}
	}

	/// <summary>
	/// Save all textures to disk.
	/// </summary>
	private void SaveTextures() {
		if(m_currentCollection == null) return;

		int count = m_currentCollection.ramps.Length;
		for(int i = 0; i < count; ++i) {
			m_currentCollection.ramps[i].SaveTexture();
		}
		AssetDatabase.SaveAssets();
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Reorderable list size has changed.
	/// </summary>
	/// <param name="_list">The list that triggered the event.</param>
	private void OnListSizeChanged(ReorderableList _list) {
		// Re-generate heights array
		m_elementHeights = new float[m_currentCollection.ramps.Length];
	}
}