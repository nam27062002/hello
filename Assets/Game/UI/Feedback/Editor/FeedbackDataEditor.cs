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
				// Title
				EditorUtils.Separator(EditorUtils.Orientation.HORIZONTAL, 5, ((FeedbackData.Type)i).ToString());

				// Probability slider
				EditorGUILayout.Slider(probabilitesProp.GetArrayElementAtIndex(i), 0f, 1f, "Probability");

				// Feedback list
				// Since multidimensional arrays are not serializable, we had to create a custom class FeedbackList
				// http://answers.unity3d.com/questions/411696/how-to-initialize-array-via-serializedproperty.html
				SerializedProperty feedbacksListProp = feedbacksProp.GetArrayElementAtIndex(i);
				EditorGUILayout.PropertyField(feedbacksListProp.FindPropertyRelative("data"), new GUIContent("Feedbacks"), true);
			}

			// Make sure changed are stored!
			serializedObject.ApplyModifiedProperties();
		}
	}
}