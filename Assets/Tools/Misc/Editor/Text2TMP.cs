// Text2TMP.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
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
/// 
/// </summary>
public class Text2TMP : EditorWindow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Windows instance
	private static Text2TMP m_instance = null;
	public static Text2TMP instance {
		get {
			if(m_instance == null) {
				m_instance = (Text2TMP)EditorWindow.GetWindow(typeof(Text2TMP));
			}
			return m_instance;
		}
	}

	// Auxiliar classes
	[System.Serializable]
	private class FontReplacement {
		public Font sourceFont = null;
		public TMP_FontAsset targetFont = null;
	}

	// Since we can't have the Unity's Text and the TMP Text components alive at the same time, store all the required data into an aux temp struct in order to do the conversion
	private struct ConversionData {
		// Vars
		public TMP_FontAsset font;
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
		public void Init(Text _text) {
			font = Text2TMP.GetTMPFont(_text);
			fontSize = _text.fontSize;
			fontSizeMax = _text.resizeTextMaxSize;
			enableAutoSizing = _text.resizeTextForBestFit;
			alignment = Text2TMP.GetAlignment(_text);
			color = _text.color;
			enableWordWrapping = (_text.horizontalOverflow == HorizontalWrapMode.Wrap);
			overflowMode = Text2TMP.GetOverflowMode(_text);
			text = _text.text;
			richText = _text.supportRichText;
		}

		/// <summary>
		/// Apply the data to the target TMP Text object.
		/// </summary>
		public void Apply(TextMeshProUGUI _tmpText) {
			_tmpText.font = font;
			_tmpText.fontSize = fontSize;
			_tmpText.fontSizeMax = fontSizeMax;
			_tmpText.enableAutoSizing = enableAutoSizing;
			_tmpText.alignment = alignment;
			_tmpText.color = color;
			_tmpText.enableWordWrapping = enableWordWrapping;
			_tmpText.OverflowMode = overflowMode;
			_tmpText.text = text;
			_tmpText.richText = richText;
		}
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed font list
	private List<FontReplacement> m_fontReplacements = new List<FontReplacement>();

	// Editor
	private ReorderableList m_fontReplacementsEditor = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Shows the window.
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/Text 2 TMP", false)]
	public static void ShowWindow() {
		// Setup window
		instance.titleContent = new GUIContent("Text 2 TMP");
		instance.minSize = new Vector2(400, 200);	// Arbitrary
		instance.maxSize = new Vector2(1000, 1000);	// Arbitrary

		// Show it
		instance.ShowUtility();
	}

	/// <summary>
	/// The window has been enabled - similar to the constructor.
	/// </summary>
	public void OnEnable() {
		// Initialize replacement fonts list editor
		// [AOC] Using the obscure UnityEditorInternal ReorderableList!
		m_fontReplacementsEditor = new ReorderableList(m_fontReplacements, typeof(FontReplacement));

		// Header
		m_fontReplacementsEditor.drawHeaderCallback = (Rect _rect) => { 
			EditorGUI.LabelField(_rect, "Replacement Fonts"); 
		};

		// Element
		m_fontReplacementsEditor.drawElementCallback = (Rect _rect, int _idx, bool _isActive, bool _isFocused) => {
			FontReplacement item = (FontReplacement)m_fontReplacementsEditor.list[_idx];

			float separatorWidth = 50f;
			float itemWidth = (_rect.width - separatorWidth)/2f;
			_rect.y += 2;	// Some padding
			_rect.height = EditorGUIUtility.singleLineHeight;

			// Source font
			_rect.width = itemWidth;
			item.sourceFont = (Font)EditorGUI.ObjectField(_rect, GUIContent.none, item.sourceFont, typeof(Font), false);

			// Separator
			float labelWidth = 20f;
			_rect.x += _rect.width;
			_rect.x += (separatorWidth - labelWidth)/2f;	// "Center" label within the separator space
			_rect.width = labelWidth;
			EditorGUI.LabelField(_rect, "->");

			// Replacement font
			_rect.x += _rect.width + (separatorWidth - labelWidth)/2f;
			_rect.width = itemWidth;
			item.targetFont = (TMP_FontAsset)EditorGUI.ObjectField(_rect, GUIContent.none, item.targetFont, typeof(TMP_FontAsset), false);
		};
	}

	/// <summary>
	/// The window has been disabled - similar to the destructor.
	/// </summary>
	public void OnDisable() {
		// Clear instance reference
		m_instance = null;
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
		Repaint();
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
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

		// Check required conditions
		bool error = true;
		if(m_fontReplacements.Count == 0) {
			EditorGUILayout.HelpBox("Initialize the replacement fonts array first!", MessageType.Error);
		} else if(Selection.activeGameObject == null) {
			EditorGUILayout.HelpBox("No object selected!", MessageType.Error);
		} else if(Selection.activeGameObject.GetComponent<TextMeshProUGUI>() != null) {
			EditorGUILayout.HelpBox("Selected object already has a TMP component!", MessageType.Error);
		} else if(Selection.activeGameObject.GetComponent<Text>() == null) {
			EditorGUILayout.HelpBox("Selected object doesn't have a Text component", MessageType.Error);
		} else {
			error = false;
		}

		// Show button
		GUI.enabled = !error;
		if(GUILayout.Button("Convert to TMP", GUILayout.Height(50f))) {
			ConvertToTMP(Selection.activeGameObject);
		}
		GUI.enabled = true;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Convert selected object from a Unity Text to a TMP.
	/// </summary>
	private void ConvertToTMP(GameObject _obj) {
		// Valid object?
		if(_obj == null) return;

		// Object has a Text component?
		Text targetText = _obj.GetComponent<Text>();
		if(targetText == null) return;

		// Start the conversion!
		// Since we can't have both a Unity Text and a TMP Text component simultaneously, use an auxiliar struct to store all the data
		ConversionData data = new ConversionData();
		data.Init(targetText);

		// Validate some data
		if(data.font == null) {
			Debug.LogError("Couldn't find a suitable font to replace " + targetText.font.name + ", aborting conversion");
			return;
		}

		// Register undo action
		Undo.RegisterFullObjectHierarchyUndo(_obj, "Text2TMP");

		// Remove original text component
		DestroyImmediate(targetText);

		// Add a new TMP object
		TextMeshProUGUI tmpText = _obj.AddComponent<TextMeshProUGUI>();
		if(tmpText == null) {
			Debug.LogError("Couldn't create a new TextMeshProUGUI component");
			return;
		}

		// Finish the conversion!
		data.Apply(tmpText);

		// Try to find a matching material based on TextFX
		TextFX fx = _obj.GetComponent<TextFX>();
		if(fx != null) {
			// Find all the material presets for the target font
			Material[] materials = TMP_EditorUtility.FindMaterialReferences(data.font);

			// [AOC] TODO!!

			// Remove TextFX component
			//DestroyImmediate(fx);
		}
	}

	//------------------------------------------------------------------------//
	// UTILS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finds the replacement TMP font matching a specific Unity textfield.
	/// </summary>
	public static TMP_FontAsset GetTMPFont(Text _textObject) {
		foreach(FontReplacement replaceFont in instance.m_fontReplacements) {
			if(replaceFont.sourceFont == _textObject.font)
				return replaceFont.targetFont;
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