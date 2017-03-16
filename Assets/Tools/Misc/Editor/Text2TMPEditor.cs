// Text2TMP.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/10/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditorInternal;
using TMPro;
using TMPro.EditorUtilities;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tool to help migrating from Unity Text to Text Mesh Pro.
/// </summary>
[CustomEditor(typeof(Text2TMP))]
public class Text2TMPEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Windows instance
	public Text2TMP instance {
		get { return (Text2TMP)target; }
	}

	private enum ErrorCode {
		NONE = -1,
		FONT_REPLACEMENTS_NOT_INITIALIZED,
		NO_OBJECT_SELECTED,
		SELECTED_OBJECT_HAS_TMP,
		SELECTED_OBJECT_NOT_TEXT
	};

	// Since we can't have the Unity's Text and the TMP Text components alive at the same time, store all the required data into an aux temp struct in order to do the conversion
	private struct ConversionData {
		// Vars
		public Text2TMP.FontReplacement fontReplacement;
		public float fontSize;
		public float fontSizeMax;
		public bool enableAutoSizing;
		public TextAlignmentOptions alignment;
		public Color color;
		public bool enableWordWrapping;
		public TextOverflowModes overflowMode;
		public string text;
		public bool richText;

		/// <summary>
		/// Initialize the data with a given Unity Text object.
		/// </summary>
		public void Init(Text _text, List<Text2TMP.FontReplacement> _fontReplacements) {
			fontReplacement = Text2TMPEditor.GetFontReplacement(_text, _fontReplacements);
			fontSize = _text.fontSize;
			fontSizeMax = _text.resizeTextMaxSize;
			enableAutoSizing = _text.resizeTextForBestFit;
			alignment = Text2TMPEditor.GetAlignment(_text);
			color = _text.color;
			enableWordWrapping = (_text.horizontalOverflow == HorizontalWrapMode.Wrap);
			overflowMode = Text2TMPEditor.GetOverflowMode(_text);
			text = _text.text;
			richText = _text.supportRichText;
		}

		/// <summary>
		/// Apply the data to the target TMP Text object.
		/// </summary>
		public void Apply(TextMeshProUGUI _tmpText) {
			_tmpText.font = fontReplacement.targetFont;
			_tmpText.fontSize = fontSize;
			_tmpText.fontSizeMax = fontSizeMax;
			_tmpText.enableAutoSizing = enableAutoSizing;
			_tmpText.alignment = alignment;
			_tmpText.color = color;
			_tmpText.enableWordWrapping = enableWordWrapping;
			_tmpText.overflowMode = overflowMode;
			_tmpText.text = text;
			_tmpText.richText = richText;
		}
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Editor
	private ReorderableList m_fontReplacementsEditor = null;
	private List<float> m_fontItemsHeights = new List<float>();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The inspector has been enabled - similar to the constructor.
	/// </summary>
	public void OnEnable() {
		// Initialize replacement fonts list editor
		// [AOC] Using the obscure UnityEditorInternal ReorderableList!
		//m_fontReplacementsEditor = new ReorderableList(instance.m_fontReplacements, typeof(Text2TMP.FontReplacement));
		m_fontReplacementsEditor = new ReorderableList(serializedObject, serializedObject.FindProperty("m_fontReplacements"));

		// Header
		m_fontReplacementsEditor.drawHeaderCallback = (Rect _rect) => { 
			EditorGUI.LabelField(_rect, "Replacement Fonts"); 
		};

		// Element
		m_fontReplacementsEditor.drawElementCallback = (Rect _rect, int _idx, bool _isActive, bool _isFocused) => {
			// Aux vars
			Rect sourceRect = _rect;	// Backup
			float separatorWidth = 50f;
			float itemWidth = (_rect.width - separatorWidth)/2f;
			_rect.y += 2;	// Some padding
			_rect.height = EditorGUIUtility.singleLineHeight;
			float itemHeight = _rect.height;

			// Get item
			SerializedProperty p = m_fontReplacementsEditor.serializedProperty.GetArrayElementAtIndex(_idx);

			// Source font
			_rect.width = itemWidth;
			EditorGUI.PropertyField(_rect, p.FindPropertyRelative("sourceFont"), GUIContent.none);

			// Separator
			float labelWidth = 20f;
			_rect.x += _rect.width;
			_rect.x += (separatorWidth - labelWidth)/2f;	// "Center" label within the separator space
			_rect.width = labelWidth;
			EditorGUI.LabelField(_rect, "->");

			// Replacement font
			_rect.x += _rect.width + (separatorWidth - labelWidth)/2f;
			_rect.width = itemWidth;
			EditorGUI.PropertyField(_rect, p.FindPropertyRelative("targetFont"), GUIContent.none);

			// Sizes
			SerializedProperty sizesProp = p.FindPropertyRelative("sizes");
			string displayName = sizesProp.displayName + " (";
			for(int i = 0; i < sizesProp.arraySize; i++) {
				if(i > 0) displayName += ", ";
				displayName += sizesProp.GetArrayElementAtIndex(i).intValue.ToString();
			}
			displayName += ")";
			_rect.y += _rect.height;
			_rect.x = sourceRect.x + 10f;	// A bit indented
			_rect.width = sourceRect.width - 10f;
			_rect.height = EditorGUI.GetPropertyHeight(sizesProp, new GUIContent(displayName), true);
			itemHeight += _rect.height;
			EditorGUI.PropertyField(_rect, sizesProp, new GUIContent(displayName), true);

			// Init item height
			itemHeight += 5f;	// Add some extra margin between items

			// Make sure heights list matches items list
			int targetSize = m_fontReplacementsEditor.serializedProperty.arraySize;
			if(m_fontItemsHeights.Count > targetSize) {
				while(m_fontItemsHeights.Count > targetSize) {
					m_fontItemsHeights.RemoveAt(m_fontItemsHeights.Count - 1);
				}
			} else if(m_fontItemsHeights.Count < targetSize) {
				while(m_fontItemsHeights.Count < targetSize) {
					m_fontItemsHeights.Add(0);
				}
			}
			if(m_fontItemsHeights[_idx] != itemHeight) {
				// Force repaing
				m_fontItemsHeights[_idx] = itemHeight;
				Repaint();
			}
		};

		m_fontReplacementsEditor.elementHeightCallback = (int _idx) => {
			// Using serialized properties without customization
			/*SerializedProperty p = m_fontReplacementsEditor.serializedProperty.GetArrayElementAtIndex(_idx);
			return EditorGUI.GetPropertyHeight(p, new GUIContent(p.displayName), true);*/

			// Using customized serialized properties
			if(_idx < m_fontItemsHeights.Count && _idx >= 0) {
				return m_fontItemsHeights[_idx];
			}
			return EditorGUIUtility.singleLineHeight;
		};

		m_fontReplacementsEditor.drawElementBackgroundCallback = (Rect _rect, int _idx, bool _active, bool _focused) => {
			// Using customized serialized properties
			// Get previously computed height
			if(_idx < m_fontItemsHeights.Count && _idx >= 0) {
				_rect.height = m_fontItemsHeights[_idx];
			}
			float margin = 2f;
			_rect.x += margin;
			_rect.width -= margin * 2f;

			// Create texture (different colors based on state)
			Texture2D tex = new Texture2D (1, 1);
			if(_active) {
				tex.SetPixel(0, 0, new Color(0.24f, 0.38f, 0.54f));		// Unity default (blue-ish)
			} else if(_focused) {
				tex.SetPixel(0, 0, new Color(0.36f, 0.36f, 0.36f));		// Unity default (brighter gray)
			} else {
				tex.SetPixel(0, 0, new Color(0.30f, 0.30f, 0.30f));		// Unity default (dark gray)
			}
			tex.Apply();
			GUI.DrawTexture(_rect, tex as Texture);
		};
	}

	/// <summary>
	/// The window has been disabled - similar to the destructor.
	/// </summary>
	public void OnDisable() {
		
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public override void OnInspectorGUI() {
		// Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update ();

		// Replacements fonts list
		// Add some padding
		GUILayout.Space(5f);
		EditorGUILayout.BeginHorizontal(); {
			GUILayout.Space(5f);

			EditorGUILayout.BeginVertical(); {
				m_fontReplacementsEditor.DoLayoutList();
			} EditorGUILayout.EndVertical();

			GUILayout.Space(5f);
		} EditorGUILayout.EndHorizontal();

		// Error check
		ErrorCode error = ErrorCode.NONE;
		if(instance.m_fontReplacements.Count == 0) {
			error = ErrorCode.FONT_REPLACEMENTS_NOT_INITIALIZED;

		} else if(Selection.activeGameObject == null) {
			error = ErrorCode.NO_OBJECT_SELECTED;
		} else {
			if(Selection.gameObjects.Length == 1) {
				// Single object selected, check component type
				if(Selection.activeGameObject.GetComponent<TextMeshProUGUI>() != null) {
					error = ErrorCode.SELECTED_OBJECT_HAS_TMP;
				} else if(Selection.activeGameObject.GetComponent<Text>() == null) {
					error = ErrorCode.SELECTED_OBJECT_NOT_TEXT;
				}
			} else {
				error = ErrorCode.NONE;	// Multiple objects, skip component check
			}
		}

		// Show buttons
		GUILayout.Space(20f);
		EditorGUILayout.BeginVertical(); {
			// BUTTON 1: Do all texts in the selected objects (non-recursive)
			GUI.enabled = (error == ErrorCode.NONE);	// All errors are blockers
			if(GUILayout.Button("Convert selected Texts to TMP", GUILayout.Height(50f))) {
				int totalObjects = Selection.gameObjects.Length;
				int processedObjects = 0;
				int successCount = 0;
				foreach(GameObject obj in Selection.gameObjects) {
					// Show progress bar
					processedObjects++;
					if(EditorUtility.DisplayCancelableProgressBar("Text2TMP", "Processing " + processedObjects + "/" + totalObjects + " text objects (" + obj.name + ")", (float)processedObjects/(float)totalObjects)) {
						break;
					}

					// Do it! And track success
					if(ConvertToTMP(obj)) {
						successCount++;
					}
				}

				// Clear progress bar and show summary
				EditorUtility.ClearProgressBar();
				EditorUtility.DisplayDialog("Text2TMP Done!", "Successfully converted " + successCount + "/" + processedObjects + " objects!", "Ok");
			}
			GUI.enabled = true;

			// BUTTON 2: Do all texts within the selected object hierarchy
			GUI.enabled = (error != ErrorCode.FONT_REPLACEMENTS_NOT_INITIALIZED
						&& error != ErrorCode.NO_OBJECT_SELECTED);
			if(GUILayout.Button("Traverse selected objects hierarchy and convert all Texts to TMP", GUILayout.Height(50f))) {
				int totalObjects = 0;
				int processedObjects = 0;
				int successCount = 0;
				foreach(GameObject obj in Selection.gameObjects) {
					// Get all Text components in the hierarchy
					Text[] textsInHierarchy = obj.GetComponentsInChildren<Text>(true);
					totalObjects += textsInHierarchy.Length;
					foreach(Text t in textsInHierarchy) {
						// Show progress bar
						processedObjects++;
						if(EditorUtility.DisplayCancelableProgressBar("Text2TMP", "Processing " + processedObjects + "/" + totalObjects + " text objects (" + t.name + ")", (float)processedObjects/(float)totalObjects)) {
							break;
						}

						// Do it! And track success
						if(ConvertToTMP(t.gameObject)) {
							successCount++;
						}
					}
				}

				// Clear progress bar and show summary
				EditorUtility.ClearProgressBar();
				EditorUtility.DisplayDialog("Text2TMP Done!", "Successfully converted " + successCount + "/" + processedObjects + " objects!", "Ok");
			}
			GUI.enabled = true;
		} EditorGUILayout.EndVertical();

		// Show errors
		GUILayout.Space(20f);
		switch(error) {
			case ErrorCode.FONT_REPLACEMENTS_NOT_INITIALIZED: {
				EditorGUILayout.HelpBox("Initialize the replacement fonts array first!", MessageType.Error);
			} break;

			case ErrorCode.NO_OBJECT_SELECTED: {
				EditorGUILayout.HelpBox("No object selected!", MessageType.Error);
			} break;

			case ErrorCode.SELECTED_OBJECT_HAS_TMP: {
				EditorGUILayout.HelpBox("Selected object already has a TMP component!", MessageType.Warning);
			} break;

			case ErrorCode.SELECTED_OBJECT_NOT_TEXT: {
				EditorGUILayout.HelpBox("Selected object doesn't have a Text component", MessageType.Warning);
			} break;
		}

		// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties ();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Convert selected object from a Unity Text to a TMP.
	/// </summary>
	private bool ConvertToTMP(GameObject _obj) {
		// Valid object?
		if(_obj == null) {
			Debug.LogError("Selected object is null!");
			return false;
		}

		// Object has a Text component?
		Text targetText = _obj.GetComponent<Text>();
		if(targetText == null) {
			Debug.LogError("Selected object " + _obj.name + " doesn't have a Text component to be converted.", _obj);
			return false;
		}

		// Start the conversion!
		// Since we can't have both a Unity Text and a TMP Text component simultaneously, use an auxiliar struct to store all the data
		ConversionData data = new ConversionData();
		data.Init(targetText, instance.m_fontReplacements);

		// Validate some data
		if(data.fontReplacement == null) {
			Debug.LogError("Couldn't find a suitable font to replace " + targetText.font.name + ", aborting conversion", _obj);
			return false;
		}

		// Register undo action
		Undo.RegisterFullObjectHierarchyUndo(_obj, "Text2TMP");

		// Remove original text component
		DestroyImmediate(targetText);

		// Add a new TMP object
		TextMeshProUGUI tmpText = _obj.AddComponent<TextMeshProUGUI>();
		if(tmpText == null) {
			Debug.LogError("Couldn't create a new TextMeshProUGUI component", _obj);
			return false;
		}

		// Finish the conversion!
		data.Apply(tmpText);

		// Round size to the nearest fixed one
		bool fontSizeCorrected = false;
		for(int i = 1; i < data.fontReplacement.sizes.Count; i++) {
			if(data.fontSize < data.fontReplacement.sizes[i]) {
				float d0 = Mathf.Abs(data.fontSize - data.fontReplacement.sizes[i-1]);
				float d1 = Mathf.Abs(data.fontSize - data.fontReplacement.sizes[i]);
				if(d0 < d1) {
					tmpText.fontSize = data.fontReplacement.sizes[i-1];
				} else {
					tmpText.fontSize = data.fontReplacement.sizes[i];
				}
				fontSizeCorrected = true;
				break;
			}
		}

		// Use biggest size if not found
		if(!fontSizeCorrected && data.fontReplacement.sizes.Count > 0) {
			tmpText.fontSize = data.fontReplacement.sizes.Last();
		}

		// Try to find a matching material based on TextFX
		TextFX fx = _obj.GetComponent<TextFX>();
		if(fx != null) {
			// Find all the material presets for the target font
			Material[] materials = TMP_EditorUtility.FindMaterialReferences(tmpText.font);
			Dictionary<string, Material> matDict = new Dictionary<string, Material>(materials.Length);
			for(int i = 0; i < materials.Length; i++) {
				matDict[materials[i].name] = materials[i];
			}

			// Select one based on Text FX setup
			// Simple selection for now
			string matName = "";
			if(tmpText.font.name == "FNT_Default") {
				if(fx.m_outline) {
					matName = "FNT_Default_Stroke";
				}
			} else {
				if(fx.m_outline) {
					// Special ones with shadow
					if(fx.m_shadow) {
						// By color
						if(fx.m_shadowColor == Colors.ParseHexString("FB846CFF")) {
							matName = tmpText.font.name + "_Stroke_Shadow_PC";
						} else if(fx.m_shadowColor == Colors.ParseHexString("FFB60FFF")) {
							matName = tmpText.font.name + "_Stroke_Shadow_SC";
						} else if(fx.m_shadowColor == Colors.ParseHexString("000000FF")) {
							matName = tmpText.font.name + "_Stroke_Shadow";
						} else {
							matName = tmpText.font.name + "_Stroke";
						}
					}

					// Stroke only
					else {
						matName = tmpText.font.name + "_Stroke";
					}
				}
			}

			// Apply material (if valid)
			if(matDict.ContainsKey(matName)) {
				tmpText.fontSharedMaterial = matDict[matName];
			}

			// Remove TextFX component
			DestroyImmediate(fx);
		}

		return true;
	}

	//------------------------------------------------------------------------//
	// UTILS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finds the replacement TMP font matching a specific Unity textfield.
	/// </summary>
	public static Text2TMP.FontReplacement GetFontReplacement(Text _textObject, List<Text2TMP.FontReplacement> _fontReplacements) {
		foreach(Text2TMP.FontReplacement replaceFont in _fontReplacements) {
			if(replaceFont.sourceFont == _textObject.font)
				return replaceFont;
		}
		return null;
	}

	/// <summary>
	/// Converts from Unity's overflow enum to TMP's overflow enum.
	/// </summary>
	public static TextOverflowModes GetOverflowMode(Text _textObject) {
		if(_textObject.verticalOverflow == VerticalWrapMode.Truncate) {
			return TextOverflowModes.Truncate;
		}

		return TextOverflowModes.Overflow;
	}

	/// <summary>
	/// Convert from Unity's alignment enum to TMP's alignment enum.
	/// </summary>
	public static TextAlignmentOptions GetAlignment(Text _textObject) {
		switch(_textObject.alignment) {
			case TextAnchor.LowerCenter:	return TextAlignmentOptions.Bottom;			break;
			case TextAnchor.LowerLeft:		return TextAlignmentOptions.BottomLeft;		break;
			case TextAnchor.LowerRight:		return TextAlignmentOptions.BottomRight;	break;
			case TextAnchor.MiddleCenter:	return TextAlignmentOptions.Midline;		break;
			case TextAnchor.MiddleLeft:		return TextAlignmentOptions.MidlineLeft;	break;
			case TextAnchor.MiddleRight:	return TextAlignmentOptions.MidlineRight;	break;
			case TextAnchor.UpperCenter:	return TextAlignmentOptions.Top;			break;
			case TextAnchor.UpperLeft:		return TextAlignmentOptions.TopLeft;		break;
			case TextAnchor.UpperRight:		return TextAlignmentOptions.TopRight;		break;
			default:						return TextAlignmentOptions.Center;			break;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}