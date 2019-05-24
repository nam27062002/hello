// ColorRampEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/05/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																		  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.IO;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																			  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor window for the color ramp editor tool.
/// </summary>
public class ColorRampEditor : EditorWindow {
	//-------------------------------------------------------------------------//
	// CONSTANTS																	  //
	//-------------------------------------------------------------------------//
	// Data constants
	public const string DATA_PATH = "Assets/Resources/Editor/ColorRampCollections/";

	// Layout constants
	private const float SPACING = 2f;
	private const float WINDOW_MARGIN = 10f;
	private const float INFO_BOX_MARGIN = 5f;
	private const float FOLDOUT_INDENT_SIZE = 15f;
	private const float SPACE_BETWEEN_ITEMS = 10f;
	private const float DIVISION_SIZE = 0f;

	// Prefs constants
	private const string SELECTION_COLLECTION_NAME_KEY = "ColorRampEditor.SELECTION_COLLECTION_NAME";
	private static string s_selectedCollectionName {
		get { return EditorPrefs.GetString(SELECTION_COLLECTION_NAME_KEY, ""); }
		set { EditorPrefs.SetString(SELECTION_COLLECTION_NAME_KEY, value); }
	}

	private const string LAST_NEW_RAMP_PATH_KEY = "ColorRampEditor.LAST_NEW_RAMP_KEY";
	private static string s_lastNewRampPath {
		get { return EditorPrefs.GetString(LAST_NEW_RAMP_PATH_KEY, Application.dataPath); }
		set { EditorPrefs.SetString(LAST_NEW_RAMP_PATH_KEY, value); }
	}

	//-------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES														  //
	//-------------------------------------------------------------------------//
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

	//-------------------------------------------------------------------------//
	// METHODS																		  //
	//-------------------------------------------------------------------------//
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

		// Refresh collection files list
		// [AOC] Would be nice to not having to do this always (performance), but so far is the only way to make sure the list is accurate
		DirectoryInfo collectionsDirInfo = new DirectoryInfo(DATA_PATH);
		FileInfo[] collectionFiles = collectionsDirInfo.GetFiles();

		// Strip filename from full file path
		List<string> collectionFilenames = new List<string>();
		for(int i = 0; i < collectionFiles.Length; i++) {
			if(collectionFiles[i].Name.EndsWith("asset", true, System.Globalization.CultureInfo.InvariantCulture)) {
				collectionFilenames.Add(Path.GetFileNameWithoutExtension(collectionFiles[i].Name));
			}
		}

		// Toolbar
		EditorGUILayout.BeginHorizontal();
		{
			// New collection
			if(GUILayout.Button("New Collection")) {
				OnNewCollection();
			}

			// Delete selected collection - disabled if there are no collections
			EditorGUI.BeginDisabledGroup(collectionFiles.Length == 0);
			if(GUILayout.Button("Delete Collection")) {
				OnDeleteCollection();
			}
			EditorGUI.EndDisabledGroup();

			// Show collections folder
			if(GUILayout.Button("Show Collections Folder")) {
				OnShowCollectionsFolder();
			}
		}
		EditorGUILayout.EndHorizontal();

		// Collection Chooser
		bool reloadCollection = false;
		{
			EditorGUI.BeginChangeCheck();

			// Find index of our current collection
			int selectedIdx = collectionFilenames.IndexOf(s_selectedCollectionName);

			// Show dropdown field
			selectedIdx = EditorGUILayout.Popup("Select Collection ", selectedIdx, collectionFilenames.ToArray());

			if(EditorGUI.EndChangeCheck()) {
				// Store new collection
				s_selectedCollectionName = selectedIdx >= 0 ? collectionFilenames[selectedIdx] : "";

				// Reload collection if required
				reloadCollection = true;
			}
		}

		// Scroll Rect
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos); {
			// Left Margin
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(WINDOW_MARGIN);

			// Top margin
			EditorGUILayout.BeginVertical();
			GUILayout.Space(WINDOW_MARGIN);

			// Scroll Content!
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
			if(m_currentCollection.name == s_selectedCollectionName) {
				// Nothing to do ^^
				return;
			}
		}

		// Try to load existing data
		string path = DATA_PATH + s_selectedCollectionName + ".asset";
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

				// Adjust prefix label size
				EditorGUIUtility.labelWidth = 90f;

				// Texture Preview
				currentLineY += SPACING;	// Add some extra spacing for breathing
				Rect previewRect = new Rect(
					_rect.x,
					currentLineY,
					_rect.width,
					10f
				);
				if(rampData.tex != null) {
					// Actual texture
					previewRect.height = rampData.tex.height * 10f; // 10 units per gradient
					GUI.DrawTexture(previewRect, rampData.tex);
				} else {
					// Placeholder texture
					GUI.color = Colors.WithAlpha(Color.white, 0.1f);
					GUI.DrawTexture(previewRect, Texture2D.whiteTexture);
					GUI.color = Color.white;
				}

				// Advance pos
				currentLineY += previewRect.height + SPACING;

				// Texture field
				// Add a "new" button to the right
				float newButtonWidth = 50f;
				SerializedProperty texProp = rampDataProp.FindPropertyRelative("tex");
				Rect texRect = new Rect(
					_rect.x,
					currentLineY,
					_rect.width - newButtonWidth,
					EditorGUI.GetPropertyHeight(texProp)
				);
				EditorGUI.PropertyField(texRect, texProp, new GUIContent("Ramp Texture"), true);

				// "New" Button
				Rect newButtonRect = new Rect(
					texRect.xMax + SPACING,
					currentLineY,
					newButtonWidth - SPACING,
					texRect.height
				);
				GUI.color = Colors.paleGreen;
				if(GUI.Button(newButtonRect, "NEW")) {
					OnCreateRampTexture(ref rampData);
				}
				GUI.color = Colors.white;

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

					// Draw Gradient fields
					SerializedProperty gradientsProp = rampDataProp.FindPropertyRelative("gradients");
					Rect gradientRect = new Rect(
						_rect.x,
						currentLineY,
						_rect.width,
						EditorGUI.GetPropertyHeight(gradientsProp)
					);
					EditorGUI.PropertyField(gradientRect, gradientsProp, true);

					// Advance pos
					currentLineY += gradientRect.height + SPACING;

					// Draw Indices - only for horizontal gradients
					if(rampData.type == ColorRampCollection.ColorRampData.GradientSequenceType.HORIZONTAL) {
						// Get Data
						SerializedProperty indicesProp = rampDataProp.FindPropertyRelative("indices");
						Rect indicesRect = new Rect(
							_rect.x,
							currentLineY,
							_rect.width,
							EditorGUIUtility.singleLineHeight
						);

						// Fixed length array matching the amount of gradients
						if(indicesProp.arraySize != gradientsProp.arraySize) {
							indicesProp.arraySize = gradientsProp.arraySize;
						}

						// Foldable
						indicesProp.isExpanded = EditorGUI.Foldout(indicesRect, indicesProp.isExpanded, "Indices", true);
						currentLineY += indicesRect.height + SPACING;   // Advance pos
						if(indicesProp.isExpanded) {
							// Do elements!
							// Aux vars
							SerializedProperty indexProp = null;
							SerializedProperty minProp = null;
							SerializedProperty maxProp = null;
							float min = 0f;
							float max = 0f;
							float previousMax = 0f;
							float labelWidth = 60f; // Fixed width

							// Aux rects
							Rect prefixLabelRect = new Rect(
								_rect.x + FOLDOUT_INDENT_SIZE,  // Extra indent space to align with foldout
								currentLineY,
								_rect.width,
								EditorGUIUtility.singleLineHeight
							);

							// Draw all elements
							for(int i = 0; i < indicesProp.arraySize; ++i) {
								// Prefix label
								Rect indentedRect = EditorGUI.PrefixLabel(
									prefixLabelRect,
									new GUIContent(gradientsProp.GetArrayElementAtIndex(i).displayName) // Show same label as Gradients array
								);

								// Get data
								indexProp = indicesProp.GetArrayElementAtIndex(i);

								minProp = indexProp.FindPropertyRelative("min");
								min = (float)minProp.intValue;

								maxProp = indexProp.FindPropertyRelative("max");
								max = (float)maxProp.intValue;

								// Slider
								// Disable for last item, since its value is fixed between previous max and 255f
								EditorGUI.BeginDisabledGroup(i == indicesProp.arraySize - 1);

								// Use a min-max slider where the min will be fixed to the previous index end
								indentedRect.x -= FOLDOUT_INDENT_SIZE;
								indentedRect.xMax = _rect.xMax - labelWidth - SPACING * 4;
								EditorGUI.MinMaxSlider(indentedRect, ref min, ref max, 0f, 255f);

								// End disabled group
								EditorGUI.EndDisabledGroup();

								// Force min to previous item's max
								min = previousMax;

								// Force max to 255 for last item
								if(i == indicesProp.arraySize - 1) {
									max = 255f;
								}

								// Cache max
								previousMax = max;

								// Store values to property (only if changed to avoid refreshing the texture all the time
								minProp.intValue = (int)min;
								maxProp.intValue = (int)max;

								// Draw label - give it a textfield style, but we don't want to make it editable so it's a label
								indentedRect.xMin = indentedRect.xMax + SPACING * 4;
								indentedRect.width = labelWidth;
								EditorGUI.SelectableLabel(indentedRect, minProp.intValue + " - " + maxProp.intValue, EditorStyles.textField);

								// Advance pos
								currentLineY += indentedRect.height + SPACING;
								prefixLabelRect.y = currentLineY;
							}
						}
					}

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
				m_elementHeights[_idx] = (currentLineY - _rect.y) + 3 * SPACING + SPACE_BETWEEN_ITEMS;    // Extra spacing
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
				_rect.height -= SPACE_BETWEEN_ITEMS;	// Leave some space without painting

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

				// Draw division - after the background to render on top
				Rect divisionRect = new Rect(
					_rect.x,
					_rect.y - DIVISION_SIZE,
					_rect.width,
					DIVISION_SIZE
				);
				GUI.DrawTexture(divisionRect, Texture2D.whiteTexture);
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

	//-------------------------------------------------------------------------//
	// CALLBACKS																	  //
	//-------------------------------------------------------------------------//
	/// <summary>
	/// Reorderable list size has changed.
	/// </summary>
	/// <param name="_list">The list that triggered the event.</param>
	private void OnListSizeChanged(ReorderableList _list) {
		// Re-generate heights array
		m_elementHeights = new float[m_currentCollection.ramps.Length];
	}

	/// <summary>
	/// Create a new ramp texture and assign it to the given ramp data object.
	/// </summary>
	/// <param name="_rampData">Ramp data where the new texture will be assigned.</param>
	private void OnCreateRampTexture(ref ColorRampCollection.ColorRampData _rampData) {
		// Open file browser
		string path = EditorUtility.SaveFilePanelInProject("New Color Ramp Texture", "new_color_ramp", "png", "", s_lastNewRampPath);
		if(string.IsNullOrEmpty(path)) return;

		// Create the new texture
		Texture2D newTex = new Texture2D(256, 1, TextureFormat.RGB24, false);
		newTex.alphaIsTransparency = false;
		newTex.filterMode = FilterMode.Point;
		newTex.wrapMode = TextureWrapMode.Clamp;

		// Save in disk
		File.WriteAllBytes(path, newTex.EncodeToPNG());
		AssetDatabase.ImportAsset(path);

		// Destroy texture data
		DestroyImmediate(newTex);
		newTex = null;

		// Change import properties
		TextureImporter texImporter = TextureImporter.GetAtPath(path) as TextureImporter;
		ColorRampCollection.ColorRampData.ApplyTextureImportSettings(ref texImporter);
		texImporter.SaveAndReimport();

		// Reload texture and assign it to the color ramp
		newTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
		_rampData.tex = newTex;

		// Store directory path
		s_lastNewRampPath = Path.GetDirectoryName(path);
	}

	/// <summary>
	/// "New Collection" button has been pressed.
	/// </summary>
	private void OnNewCollection() {
		this.ShowNotification(new GUIContent("Coming Soon!"));
	}

	/// <summary>
	/// "Delete Collection" button has been pressed.
	/// </summary>
	private void OnDeleteCollection() {
		// Nothing to do if no collection is selected
		if(m_currentCollection == null) return;

		// Show confirmation dialog
		if(EditorUtility.DisplayDialog(
			"Delete " + m_currentCollection.name + "?",
			"The collection " + m_currentCollection.name + " will be permanently deleted. Are you sure?",
			"Yes", "No"
			)) {
			// Do it!
			if(AssetDatabase.DeleteAsset(DATA_PATH + s_selectedCollectionName + ".asset")) {
				// Clear selection
				s_selectedCollectionName = string.Empty;
				m_currentCollection = null;
				InitWithCurrentCollection();
			}
		}
	}

	/// <summary>
	/// "Show Collections Folder" button has been pressed.
	/// </summary>
	private void OnShowCollectionsFolder() {
		// Aux vars
		UnityEngine.Object toSelect = null;

		// Do we have a valid collection loaded?
		if(m_currentCollection != null) {
			// Yes! Select it
			toSelect = m_currentCollection;
		} else {
			// No! Select the folder object instead
			// Check the path has no '/' at the end, if it does remove it
			string path = Path.GetDirectoryName(DATA_PATH);

			// Load object
			toSelect = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
		}

		// Select the object in the project folder
		Selection.activeObject = toSelect;

		// Also flash the folder yellow to highlight it
		EditorGUIUtility.PingObject(toSelect);
	}
}