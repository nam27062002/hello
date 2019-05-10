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
	private const string DATA_PATH = "Assets/Resources/Editor/ColorRampEditorData.asset";
	private const float MARGIN = 2f;
	private const float INFO_BOX_MARGIN = 5f;

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
	private ColorRampEditorData m_data = null;
	private ColorRampEditorData data {
		get {
			LoadDataIfNeeded();
			return m_data;
		}
	}

	// GUI widgets
	private ReorderableList m_rampsList = null;

	// Internal
	private SerializedObject m_serializedData = null;
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
	}

	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Load data
		LoadDataIfNeeded();

		// Serialize loaded data
		m_serializedData = new SerializedObject(m_data);

		// Initialize list widget
		m_rampsList = new ReorderableList(m_serializedData, m_serializedData.FindProperty("ramps"));
		if(m_rampsList != null) {
			// Header
			m_rampsList.drawHeaderCallback = (Rect _rect) => {
				// Show title
				EditorGUI.LabelField(_rect, "Ramps List");
			};

			// Element
			m_rampsList.drawElementCallback = (Rect _rect, int _idx, bool _isActive, bool _isFocused) => {
				// Margins
				_rect.x += MARGIN;
				_rect.width -= MARGIN;
				_rect.y += MARGIN;

				// Aux vars
				float currentLineY = _rect.y;

				// Get target data
				ColorRampEditorData.ColorRampData rampData = m_data.ramps[_idx];
				SerializedProperty rampDataProp = m_rampsList.serializedProperty.GetArrayElementAtIndex(_idx);
				SerializedProperty gradientProp = rampDataProp.FindPropertyRelative("gradient");
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
				EditorGUI.PropertyField(texRect, texProp, new GUIContent("Texture File"), true);

				// Advance pos
				currentLineY += texRect.height + MARGIN;

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
					boxTextRect.xMin += INFO_BOX_MARGIN + 20f;	// Leave room for the info box icon
					boxTextRect.yMin += INFO_BOX_MARGIN;
					boxTextRect.xMax -= INFO_BOX_MARGIN;
					boxTextRect.yMax -= INFO_BOX_MARGIN;
					GUI.Label(boxTextRect, "Assign a texture to edit", HELP_BOX_TEXT_STYLE);

					// Advance pos
					currentLineY += warningRect.height + MARGIN;
				} else { 
					// [AOC] EditorGUILayout.GradientField is not public before Unity 2018, 
					// so we're forced to use the serialized property.
					// We want to open the gradient editor through a button, to emphasize
					// the fact that the user will be modifying the texture as well.
					// Unfortunately, Unity doesn't give access to the gradient editor until
					// version 2018, so we will drop the idea for now.4

					// Detect changes
					EditorGUI.BeginChangeCheck();

					// Draw Gradient field
					Rect gradientRect = new Rect(
						_rect.x,
						currentLineY,
						_rect.width,
						EditorGUI.GetPropertyHeight(gradientProp)
					);
					EditorGUI.PropertyField(gradientRect, gradientProp, new GUIContent("Click to edit -->"), true);

					// Advance pos
					currentLineY += gradientRect.height + MARGIN;

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
						boxContentRect.xMin += INFO_BOX_MARGIN + 20f;	// Leave room for the info box icon
						boxContentRect.yMin += INFO_BOX_MARGIN;
						boxContentRect.xMax -= INFO_BOX_MARGIN + buttonW + MARGIN + buttonW;
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
						boxContentRect.xMin = boxContentRect.xMax + MARGIN;
						boxContentRect.width = buttonW;
						GUI.color = Colors.coral;
						if(GUI.Button(boxContentRect, "Discard")) {
							// Reload texture from disk
							rampData.DiscardGradient();
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
						currentLineY += errorBoxRect.height + MARGIN;
					}
				}

				// Restore prefix label size
				EditorGUIUtility.labelWidth = 0f;
			};

			m_rampsList.elementHeightCallback = (int _idx) => {
				// Hardcoded xD
				ColorRampEditorData.ColorRampData rampData = m_data.ramps[_idx];
				if(rampData.tex == null) {
					return 60f; // Extra space for warning message
				} else if(rampData.dirty) {
					return 80f;	// Extra space for buttons
				} else {
					return 45f;
				}
			};

			m_rampsList.drawElementBackgroundCallback = (Rect _rect, int _idx, bool _active, bool _focused) => {
				// Using customized serialized properties
				float margin = MARGIN;
				_rect.x += margin;
				_rect.width -= margin * MARGIN;

				// Different colors based on GUI state
				ColorRampEditorData.ColorRampData rampData = m_data.ramps[_idx];
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
		}
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
		m_serializedData.Update();

		// Entries list
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos); {
			m_rampsList.DoLayoutList();
		} EditorGUILayout.EndScrollView();

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		m_serializedData.ApplyModifiedProperties();
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Save all textures to disk.
	/// </summary>
    private void SaveTextures() {
        int count = m_data.ramps.Length;
        for (int i = 0; i < count; ++i) {
			m_data.ramps[i].SaveTexture();
		}
        AssetDatabase.SaveAssets();
    }

	/// <summary>
	/// Loads persistence data if not already loaded.
	/// If no data object exists at the target path, creates a new one.
	/// </summary>
	private void LoadDataIfNeeded() {
		// Is data loaded?
		if(m_data != null) return;

		// Try to load existing data
		m_data = AssetDatabase.LoadAssetAtPath(DATA_PATH, typeof(ColorRampEditorData)) as ColorRampEditorData;

		// If no data object was found, create a new one
		if(m_data == null) {
			m_data = ScriptableObject.CreateInstance<ColorRampEditorData>();
			AssetDatabase.CreateAsset(m_data, DATA_PATH);
			AssetDatabase.SaveAssets();
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
}