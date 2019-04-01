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
		// Don't support multi-editing (all values would be overwritten!)
		if(_property.hasMultipleDifferentValues || _property.serializedObject.isEditingMultipleObjects) {
			GUIContent message = new GUIContent("Doesn't support multi-editing");
			m_pos.height = CustomEditorStyles.commentLabelLeft.CalcSize(message).y;
			EditorGUI.LabelField(m_pos, _label, message, CustomEditorStyles.commentLabelLeft);
			AdvancePos();
			return;
		}

		// Get important properties
		m_topLeftProp = m_rootProperty.FindPropertyRelative("topLeft");
		m_topRightProp = m_rootProperty.FindPropertyRelative("topRight");
		m_bottomLeftProp = m_rootProperty.FindPropertyRelative("bottomLeft");
		m_bottomRightProp = m_rootProperty.FindPropertyRelative("bottomRight");

		// Generate preview texture
		Texture2D tex = CreatePreviewTexture();
		//EditorGUI.DrawTextureTransparent(r, tex);	// [AOC] This is bugged! Doing a workaround https://answers.unity.com/questions/377207/drawing-a-texture-in-a-custom-propertydrawer.html
		GUIStyle previewStyle = new GUIStyle();
		previewStyle.normal.background = tex;

		// Prefix label
		Rect contentRect = EditorGUI.PrefixLabel(m_pos, _label);	// This gives us the size of the content rect
		m_pos.height = EditorStyles.largeLabel.lineHeight;

		// Reset indentation
		int indentLevelBckp = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		// Mini-preview
		Rect previewRect = new Rect(contentRect);
		previewRect.height = EditorStyles.largeLabel.lineHeight;
		previewRect.width = previewRect.height;	// Squared
		EditorGUI.LabelField(previewRect, GUIContent.none, previewStyle);

		// Compute foldable content rect
		float foldoutWidth = 5f;
		float foldoutMargin = 12f;
		Rect foldableContentRect = new Rect(contentRect);
		foldableContentRect.x += previewRect.width + foldoutMargin + foldoutWidth;
		foldableContentRect.width -= previewRect.width + foldoutMargin + foldoutWidth;

		// Foldable
		Rect r = previewRect;
		r.x += r.width + foldoutMargin;
		r.width = foldoutWidth;
		_property.isExpanded = EditorGUI.Foldout(r, _property.isExpanded, GUIContent.none);
		if(_property.isExpanded) {
			// 4 color fields in a 2 by 2 grid layout with a preview in between
			float colorFieldHeight = EditorStyles.colorField.lineHeight;
			float colorFieldWidth = Mathf.Max(foldableContentRect.width / 3f, 50f); // Each side of the preview (1/3th of total width, with a min size)
			float previewSize = Mathf.Min(foldableContentRect.width - 2 * colorFieldWidth, 50f);    // Squared, available space, max size
			colorFieldWidth = Mathf.Max(colorFieldWidth, (foldableContentRect.width - previewSize) / 2f);   // If we have some room left, maximize color fields

			m_pos.height = previewSize;
			foldableContentRect.height = m_pos.height;

			// Top-Left
			r.x = foldableContentRect.x;
			r.y = foldableContentRect.y;
			r.width = colorFieldWidth;
			r.height = colorFieldHeight;
			m_topLeftProp.colorValue = EditorGUI.ColorField(r, m_topLeftProp.colorValue);

			// Preview
			r.x = foldableContentRect.x + colorFieldWidth;
			r.y = foldableContentRect.y;
			r.width = previewSize;
			r.height = previewSize;
			EditorGUI.LabelField(r, GUIContent.none, previewStyle);

			// Top-Right
			r.x = foldableContentRect.x + colorFieldWidth + previewSize;
			r.y = foldableContentRect.y;
			r.width = colorFieldWidth;
			r.height = colorFieldHeight;
			m_topRightProp.colorValue = EditorGUI.ColorField(r, m_topRightProp.colorValue);

			// Bottom-Left
			r.x = foldableContentRect.x;
			r.y = foldableContentRect.y + previewSize - colorFieldHeight;
			r.width = colorFieldWidth;
			r.height = colorFieldHeight;
			m_bottomLeftProp.colorValue = EditorGUI.ColorField(r, m_bottomLeftProp.colorValue);

			// Bottom-Right
			r.x = foldableContentRect.x + colorFieldWidth + previewSize;
			r.y = foldableContentRect.y + previewSize - colorFieldHeight;
			r.width = colorFieldWidth;
			r.height = colorFieldHeight;
			m_bottomRightProp.colorValue = EditorGUI.ColorField(r, m_bottomRightProp.colorValue);

			// Toolbox (align bottom-left of the foldout content)
			int numButtons = 2;
			float buttonSize = 20f;
			Rect toolBoxRect = new Rect(
				previewRect.xMax - numButtons * buttonSize,
				foldableContentRect.yMax - buttonSize,
				numButtons * buttonSize,
				buttonSize
			);

			// Horizontal swap button
			r = toolBoxRect;
			r.width = buttonSize;
			r.height = buttonSize;
			if(GUI.Button(r, new GUIContent("⇆", "Swap left and right colors"))) {
				SwapHorizontal();
			}

			// Vertical swap button
			r.x += r.width;
			if(GUI.Button(r, new GUIContent("⇅", "Swap top and bottom colors"))) {
				SwapVertical();
			}
		}

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

	/// <summary>
	/// Swap the left and right colors.
	/// </summary>
	private void SwapHorizontal() {
		// Top
		Color tmp = m_topLeftProp.colorValue;
		m_topLeftProp.colorValue = m_topRightProp.colorValue;
		m_topRightProp.colorValue = tmp;

		// Bot
		tmp = m_bottomLeftProp.colorValue;
		m_bottomLeftProp.colorValue = m_bottomRightProp.colorValue;
		m_bottomRightProp.colorValue = tmp;
	}

	/// <summary>
	/// Swap the top and bottom colors.
	/// </summary>
	private void SwapVertical() {
		// Left
		Color tmp = m_topLeftProp.colorValue;
		m_topLeftProp.colorValue = m_bottomLeftProp.colorValue;
		m_bottomLeftProp.colorValue = tmp;

		// Right
		tmp = m_bottomRightProp.colorValue;
		m_bottomRightProp.colorValue = m_topRightProp.colorValue;
		m_topRightProp.colorValue = tmp;
	}
}