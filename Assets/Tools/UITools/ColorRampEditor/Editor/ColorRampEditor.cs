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
	private static GUIStyle s_infoLabelStyle = null;
	private static GUIStyle INFO_LABEL_STYLE {
		get {
			if(s_infoLabelStyle == null) {
				s_infoLabelStyle = new GUIStyle(GUI.skin.label);
				s_infoLabelStyle.wordWrap = true;
			}
			return s_infoLabelStyle;
		}
	}

	private static GUIStyle s_warningLabelStyle = null;
	private static GUIStyle WARNING_LABEL_STYLE {
		get {
			if(s_warningLabelStyle == null) {
				s_warningLabelStyle = new GUIStyle(GUI.skin.label);
				s_warningLabelStyle.wordWrap = true;
				s_warningLabelStyle.normal.textColor = Color.yellow;
				s_warningLabelStyle.alignment = TextAnchor.UpperCenter;
			}
			return s_warningLabelStyle;
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
				// Element's foldout arrow is drawn OUTSIDE the rectangle, so try to compensate it here.
				// Do some hardcoded adjusting before drawing the property
				_rect.x += 10f;
				_rect.width -= 10f;
				_rect.y += MARGIN;

				// Get target data
				ColorRampEditorData.ColorRampData rampData = m_data.ramps[_idx];
				SerializedProperty rampDataProp = m_rampsList.serializedProperty.GetArrayElementAtIndex(_idx);
				SerializedProperty gradientProp = rampDataProp.FindPropertyRelative("gradient");
				SerializedProperty texProp = rampDataProp.FindPropertyRelative("tex");

				// Adjust prefix label size
				EditorGUIUtility.labelWidth = 80f;

				// Display texture
				_rect.height = EditorGUI.GetPropertyHeight(texProp);
				EditorGUI.PropertyField(_rect, texProp, new GUIContent("Texture File"), true);

				// Advance pos
				_rect.y += _rect.height + MARGIN;

				// Display gradient
				// Disabled if no texture is assigned
				if(rampData.tex == null) {
					// Draw warning text
					Rect warningRect = new Rect(_rect);
					warningRect.height *= 2f;
					GUI.Label(warningRect, "Assign a texture to edit", WARNING_LABEL_STYLE);
				} else { 
					// [AOC] EditorGUILayout.GradientField is not public before Unity 2018, 
					// so we're forced to use the serialized property.
					// We want to open the gradient editor through a button, to emphasize
					// the fact that the user will be modifying the texture as well.
					// Unfortunately, Unity doesn't give access to the gradient editor until
					// version 2018, so we will drop the idea for now.4

					// Detect changes
					EditorGUI.BeginChangeCheck();

					// Some constants
					float infoWidth = 90f;

					// Define height
					_rect.height = EditorGUI.GetPropertyHeight(gradientProp);

					// Get the rect without the prefix label
					Rect contentRect = EditorGUI.IndentedRect(_rect);
					contentRect.x += EditorGUIUtility.labelWidth;
					contentRect.width -= EditorGUIUtility.labelWidth;

					// Draw Gradient field
					Rect gradientRect = new Rect(_rect);
					gradientRect.width -= infoWidth - MARGIN;
					EditorGUI.PropertyField(gradientRect, gradientProp, new GUIContent("Preview"), true);

					// Draw info text
					Rect infoRect = new Rect(contentRect);
					infoRect.x = gradientRect.xMax + MARGIN;
					infoRect.width = infoWidth;
					infoRect.height *= 2f;
					GUI.Label(infoRect, "<-- Click to edit texture", INFO_LABEL_STYLE);

					//EditorGUI.PropertyField(_rect, gradientProp, true);
					//EditorGUI.LabelField(_rect, ".", "Edit Gradient", EditorStyles.miniButton);

					if(EditorGUI.EndChangeCheck()) {
						// Gradient has changed! Generate texture
						rampData.RefreshTexture();
					}
				}

				// Restore prefix label size
				EditorGUIUtility.labelWidth = 0f;
			};

			m_rampsList.elementHeightCallback = (int _idx) => {
				// Hardcoded xD
				return 50f;
			};

			m_rampsList.drawElementBackgroundCallback = (Rect _rect, int _idx, bool _active, bool _focused) => {
				// Using customized serialized properties
				float margin = MARGIN;
				_rect.x += margin;
				_rect.width -= margin * MARGIN;

				// Different colors based on state
				if(_active) {
					GUI.color = new Color(0.24f, 0.38f, 0.54f);     // Unity default (blue-ish)
				} else if(_focused) {
					GUI.color = new Color(0.36f, 0.36f, 0.36f);     // Unity default (brighter gray)
				} else {
					GUI.color = new Color(0.30f, 0.30f, 0.30f);     // Unity default (dark gray)
				}
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
        SaveTextures();
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
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, GUILayout.Height(Screen.height - 60f)); {
			m_rampsList.DoLayoutList();
		} EditorGUILayout.EndScrollView();

		// Buttons bar
		float buttonHeight = 30f;
		EditorGUILayout.BeginHorizontal(); {
			// Flexible space to align buttons to the right
			GUILayout.FlexibleSpace();

			// Space
			EditorGUILayout.Space();

			// Save assets button
			GUI.color = Colors.paleGreen;
			if(GUILayout.Button("Save Assets", GUILayout.Width(100f), GUILayout.Height(buttonHeight))) {
                SaveTextures();
            }
			GUI.color = Color.white;
		} EditorGUILayout.EndHorizontal();

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		m_serializedData.ApplyModifiedProperties();
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//

    private void SaveTextures() {
        int count = m_data.ramps.Length;

        for (int i = 0; i < count; ++i) {
            Texture2D texture = m_data.ramps[i].tex;
            if (texture != null) {
                string path = AssetDatabase.GetAssetPath(texture);
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
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