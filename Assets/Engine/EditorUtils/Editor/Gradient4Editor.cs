// Gradient4Editor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/03/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the Gradient4 class.
/// </summary>
[CustomPropertyDrawer(typeof(Gradient4))]
public class Gradient4Editor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Important properties
	private SerializedProperty m_topLeftProp = null;
	private SerializedProperty m_topRightProp = null;
	private SerializedProperty m_bottomLeftProp = null;
	private SerializedProperty m_bottomRightProp = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PARENT IMPLEMENTATION											//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override protected void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Get important properties
		m_topLeftProp = m_rootProperty.FindPropertyRelative("topLeft");
		m_topRightProp = m_rootProperty.FindPropertyRelative("topRight");
		m_bottomLeftProp = m_rootProperty.FindPropertyRelative("bottomLeft");
		m_bottomRightProp = m_rootProperty.FindPropertyRelative("bottomRight");

		// 4 color fields in a 2 by 2 grid layout
		// Prefix label
		Rect contentRect = EditorGUI.PrefixLabel(m_pos, _label);	// This gives us the size of the content rect

		float colorFieldHeight = EditorStyles.colorField.lineHeight;
		float colorFieldWidth = Mathf.Max(contentRect.width/3f, 50f);	// Each side of the preview (1/3th of total width, with a min size)
		float previewSize = Mathf.Min(contentRect.width - 2 * colorFieldWidth, 50f);	// Squared, available space, max size
		colorFieldWidth = Mathf.Max(colorFieldWidth, (contentRect.width - previewSize)/2f);	// If we have some room left, maximize color fields

		m_pos.height = colorFieldHeight * 2 + previewSize;
		contentRect.height = m_pos.height;

		// Reset indentation
		int indentLevelBckp = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		// Top-Left
		Rect r = new Rect(contentRect);
		r.x = contentRect.x;
		r.y = contentRect.y;
		r.width = colorFieldWidth;
		r.height = colorFieldHeight;
		m_topLeftProp.colorValue = EditorGUI.ColorField(r, m_topLeftProp.colorValue);

		// Preview
		r.x = contentRect.x + colorFieldWidth;
		r.y = contentRect.y;
		r.width = previewSize;
		r.height = previewSize;

		Texture2D tex = CreatePreviewTexture();
		//EditorGUI.DrawTextureTransparent(r, tex);	// [AOC] This is bugged! Doing a workaround https://answers.unity.com/questions/377207/drawing-a-texture-in-a-custom-propertydrawer.html
		GUIStyle style = new GUIStyle();
		style.normal.background = tex;
		EditorGUI.LabelField(r, GUIContent.none, style);

		// Top-Right
		r.x = contentRect.x + colorFieldWidth + previewSize;
		r.y = contentRect.y;
		r.width = colorFieldWidth;
		r.height = colorFieldHeight;
		m_topRightProp.colorValue = EditorGUI.ColorField(r, m_topRightProp.colorValue);
		
		// Bottom-Left
		r.x = contentRect.x;
		r.y = contentRect.y + previewSize - colorFieldHeight;
		r.width = colorFieldWidth;
		r.height = colorFieldHeight;
		m_bottomLeftProp.colorValue = EditorGUI.ColorField(r, m_bottomLeftProp.colorValue);

		// Bottom-Right
		r.x = contentRect.x + colorFieldWidth + previewSize;
		r.y = contentRect.y + previewSize - colorFieldHeight;
		r.width = colorFieldWidth;
		r.height = colorFieldHeight;
		m_bottomRightProp.colorValue = EditorGUI.ColorField(r, m_bottomRightProp.colorValue);

		// Restore indentation and advance line
		EditorGUI.indentLevel = indentLevelBckp;
		AdvancePos();

	}

	/// <summary>
	/// Optionally override to give a custom label for this property field.
	/// </summary>
	/// <returns>The new label for this property.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_defaultLabel">The default label for this property.</param>
	override protected GUIContent GetLabel(SerializedProperty _property, GUIContent _defaultLabel) {
		return _defaultLabel;
	}

	/// <summary>
	/// Refresh the preview texture.
	/// </summary>
	private Texture2D CreatePreviewTexture() {
		// Create texture
		// [AOC] Extracted from https://github.com/MattRix/UnityDecompiled/blob/cc432a3de42b53920d5d5dae85968ff993f4ec0e/UnityEditor/UnityEditor/GradientEditor.cs
		// Color gradient will be renderd in a squared texture
		Texture2D tex = new Texture2D(8, 8, TextureFormat.ARGB32, false);
		tex.wrapMode = TextureWrapMode.Clamp;
		tex.hideFlags = HideFlags.HideAndDontSave;

		// Generate the texture by manually interpolating between keys
		Color[] pixels = new Color[tex.width * tex.height];
		Vector2 coord = Vector2.zero;
		Color ctop = Color.white;
		Color cbot = Color.white;
		int i = 0;
		for(int row = 0; row < tex.height; ++row) {
			// Compute new delta
			coord.y = (float)row/(float)tex.height;

			for(int col = 0; col < tex.width; ++col) {
				// Compute new delta
				coord.x = (float)col/(float)tex.width;

				// Compute new color
				ctop = Color.Lerp(m_topLeftProp.colorValue, m_topRightProp.colorValue, coord.x);
				cbot = Color.Lerp(m_bottomLeftProp.colorValue, m_bottomRightProp.colorValue, coord.x);
				pixels[i] = Color.Lerp(ctop, cbot, (1f - coord.y));	// Reverse delta since pixels are sorted bot to top

				// Next pixel
				++i;
			}
		}

		// Apply!
		tex.SetPixels(pixels);
		tex.Apply();
		return tex;
	}
}