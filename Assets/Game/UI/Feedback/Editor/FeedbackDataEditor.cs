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
namespace LevelEditor {
	/// <summary>
	/// Custom editor for the FeedbackData class.
	/// </summary>
	[CustomEditor(typeof(FeedbackData))]
	public class FeedbackDataEditor : Editor {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		
		//------------------------------------------------------------------//
		// PROPERTIES														//
		//------------------------------------------------------------------//
		private FeedbackData targetData { get { return target as FeedbackData; }}
		
		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//

		/*public enum Type {
			DAMAGE,
			EAT,
			BURN,
			DESTROY,
			
			COUNT
		};
		
		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		[SerializeField] private float[] m_probabilities = new float[(int)Type.COUNT];
		[SerializeField] private List<string>[] m_feedbacks = new List<string>[(int)Type.COUNT];*/
		
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
		/// The scene is being refreshed.
		/// </summary>
		public void OnSceneGUI() {

		}
		
		/// <summary>
		/// Draw the inspector.
		/// </summary>
		public override void OnInspectorGUI() {
			// Aux vars
			int numTypes = (int)FeedbackData.Type.COUNT;

			// Draw probability and strings for each type together
			// Use serialized object to access private vars
			SerializedProperty probabilitesProp = serializedObject.FindProperty("m_probabilities");
			SerializedProperty feedbacksProp = serializedObject.FindProperty("m_feedbacks");
		
			// Make sure both arrays have the size they must have
			if(probabilitesProp.arraySize != numTypes) {
				probabilitesProp.arraySize = numTypes;
			}

			if(feedbacksProp.arraySize != numTypes) {
				feedbacksProp.arraySize = numTypes;
			}

			// Draw editor for each type
			for(int i = 0; i < numTypes; i++) {
				// Make it foldable
				// Reuse the isExpanded property of the Probability field to store the folded status
				// Show probability slider next to property name
				SerializedProperty probabilityProp = probabilitesProp.GetArrayElementAtIndex(i);
				EditorGUILayout.BeginHorizontal(); {
					// Foldable
					probabilityProp.isExpanded = EditorGUILayout.Foldout(probabilityProp.isExpanded, ((FeedbackData.Type)i).ToString());

					// Probability slider
					EditorGUIUtility.labelWidth = EditorStyles.largeLabel.CalcSize(new GUIContent("Probability")).x;
					EditorGUILayout.Slider(probabilityProp, 0f, 1f, "Probability");
					EditorGUIUtility.labelWidth = 0;
				} EditorGUILayoutExt.EndHorizontalSafe();

				// Foldable content
				if(probabilityProp.isExpanded) {
					// Indented
					EditorGUI.indentLevel++;

					// Feedback list
					// Since multidimensional arrays are not serializable, we had to create a custom class FeedbackList
					// http://answers.unity3d.com/questions/411696/how-to-initialize-array-via-serializedproperty.html
					// Display it beautifully though
					SerializedProperty feedbacksListProp = feedbacksProp.GetArrayElementAtIndex(i);
					SerializedProperty actualListProp = feedbacksListProp.FindPropertyRelative("data");

					// Special message if list is empty
					if(actualListProp.arraySize == 0) {
						// Comment-like label
						GUIStyle style = new GUIStyle(EditorStyles.label);
						style.normal.textColor = Colors.gray;
						style.fontStyle = FontStyle.Italic;
						style.wordWrap = true;
						EditorGUILayout.LabelField("", "Feedbacks list for " + ((FeedbackData.Type)i).ToString() + " is empty. Add some with the \"+\" button.", style);
					} else {
						// Current entries
						for(int j = 0; j < actualListProp.arraySize; j++) {
							// Single line
							EditorGUILayout.BeginHorizontal(); {
								// Feedback text
								SerializedProperty feedbackStringProp = actualListProp.GetArrayElementAtIndex(j);
								feedbackStringProp.stringValue = EditorGUILayout.TextField(feedbackStringProp.stringValue);

								// Remove button
								if(GUILayout.Button("-", GUILayout.Width(30))) {
									// Delete entry
									actualListProp.DeleteArrayElementAtIndex(j);
									j--;	// We just deleted element j, so all remaining elements have moved up one position - keep up
								}
							} EditorGUILayoutExt.EndHorizontalSafe();
						}
					}

					// Add new entry button
					// Centered
					EditorGUILayout.BeginHorizontal(); {
						GUILayout.FlexibleSpace();
						if(GUILayout.Button("+", GUILayout.Width(100))) {
							// Just add a new empty entry at the end of the list
							actualListProp.arraySize++;
						}
						GUILayout.FlexibleSpace();
					} EditorGUILayoutExt.EndHorizontalSafe();

					// Some spacing to breath
					GUILayout.Space(10);

					// Restore indentation
					EditorGUI.indentLevel--;
				}
			}

			// Make sure changed are stored!
			serializedObject.ApplyModifiedProperties();
		}
	}
}