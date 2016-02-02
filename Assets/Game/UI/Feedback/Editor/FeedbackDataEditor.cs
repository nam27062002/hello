// FeedbackDataEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the FeedbackData class.
/// </summary>
[CustomPropertyDrawer(typeof(FeedbackData))]
public class FeedbackDataEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// MEMBERS & PROPERTIES												//
	//------------------------------------------------------------------//
	private static GUIStyle s_centeredCommentLabelStyle = null;
	private Rect m_areaRect = new Rect();
	
	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
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
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override protected void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Initialize custom styles
		if(s_centeredCommentLabelStyle == null) {
			s_centeredCommentLabelStyle = new GUIStyle(EditorStyles.label);
			s_centeredCommentLabelStyle.normal.textColor = Colors.gray;
			s_centeredCommentLabelStyle.fontStyle = FontStyle.Italic;
			s_centeredCommentLabelStyle.wordWrap = true;
			s_centeredCommentLabelStyle.alignment = TextAnchor.MiddleCenter;
		}

		// Aux vars
		int numTypes = (int)FeedbackData.Type.COUNT;
		m_pos.height = EditorGUIUtility.singleLineHeight + 1f;
		Rect contentRect = EditorGUI.PrefixLabel(m_pos, new GUIContent(" "));	// Remaining space for the content
		Rect pos = new Rect();

		// Group in a foldout
		_property.isExpanded = EditorGUI.Foldout(m_pos, _property.isExpanded, _label);
		AdvancePos(0f, 2f);
		if(_property.isExpanded) {
			// Indent in
			EditorGUI.indentLevel++;

			// Draw probability and strings for each type together
			SerializedProperty probabilitesProp = _property.FindPropertyRelative("m_probabilities");
			SerializedProperty feedbacksProp = _property.FindPropertyRelative("m_feedbacks");

			// Make sure both arrays have the size they must have
			if(probabilitesProp.arraySize != numTypes) {
				probabilitesProp.arraySize = numTypes;
			}

			if(feedbacksProp.arraySize != numTypes) {
				feedbacksProp.arraySize = numTypes;
			}

			// Draw editor for each type
			int indentLevel = EditorGUI.indentLevel;
			for(int i = 0; i < numTypes; i++) {
				// Restore indent level
				EditorGUI.indentLevel = indentLevel;

				// Make it foldable
				// Reuse the isExpanded property of the Probability field to store the folded status
				SerializedProperty probabilityProp = probabilitesProp.GetArrayElementAtIndex(i);

				// Foldout
				pos.Set(m_pos.x, m_pos.y, contentRect.x - m_pos.x, m_pos.height);
				probabilityProp.isExpanded = EditorGUI.Foldout(pos, probabilityProp.isExpanded, ((FeedbackData.Type)i).ToString());

				// Probability slider - same line as the foldout
				// For some unknown reason, slider saves up some space for the label even if it's empty
				// Correct it by some manual offset on the x position
				float offset = 45f;
				pos.Set(contentRect.x - offset, m_pos.y, contentRect.width + offset, m_pos.height);
				probabilityProp.floatValue = EditorGUI.Slider(pos, probabilityProp.floatValue, 0f, 1f);
				AdvancePos(0f, 2f);

				// Foldable content
				if(probabilityProp.isExpanded) {
					// Reset indentation - we're already starting at the content rect
					EditorGUI.indentLevel = 0;

					// Feedback list
					// Since multidimensional arrays are not serializable, we had to create a custom class FeedbackList
					// http://answers.unity3d.com/questions/411696/how-to-initialize-array-via-serializedproperty.html
					// Display it beautifully though
					SerializedProperty feedbacksListProp = feedbacksProp.GetArrayElementAtIndex(i);
					SerializedProperty actualListProp = feedbacksListProp.FindPropertyRelative("data");

					// Special message if list is empty
					if(actualListProp.arraySize == 0) {
						// Comment-like label
						GUIContent labelContent = new GUIContent("Feedbacks list for " + ((FeedbackData.Type)i).ToString() + " is empty. Add some with the \"+\" button.");
						pos.Set(contentRect.x, m_pos.y, contentRect.width, s_centeredCommentLabelStyle.CalcHeight(labelContent, contentRect.width));
						GUI.Label(pos, labelContent, s_centeredCommentLabelStyle);
						AdvancePos(pos.height, 2f);
					} else {
						// Current entries
						for(int j = 0; j < actualListProp.arraySize; j++) {
							// Single line
							SerializedProperty feedbackStringProp = actualListProp.GetArrayElementAtIndex(j);

							// String
							pos.Set(contentRect.x, m_pos.y, contentRect.width - 30f, m_pos.height);
							feedbackStringProp.stringValue = EditorGUI.TextField(pos, feedbackStringProp.stringValue);

							// Remove button
							pos.x += pos.width;
							pos.width = 30f;
							if(GUI.Button(pos, "-")) {
								// Delete entry
								actualListProp.DeleteArrayElementAtIndex(j);
								j--;	// We just deleted element j, so all remaining elements have moved up one position - keep up
							}
							AdvancePos(0f, 2f);
						}
					}

					// Add new entry button
					// Centered
					pos.Set(contentRect.x + (contentRect.width - 100)/2f, m_pos.y, 100f, m_pos.height);
					if(GUI.Button(pos, "+")) {
						// Just add a new empty entry at the end of the list
						actualListProp.arraySize++;
					}
					AdvancePos(0f, 2f);

					// Some spacing to breath
					AdvancePos(10f);
				}
			}

			// Restore indentation
			EditorGUI.indentLevel = indentLevel;
			EditorGUI.indentLevel--;
		}

		// Make sure changed are stored!
		_property.serializedObject.ApplyModifiedProperties();
	}
}