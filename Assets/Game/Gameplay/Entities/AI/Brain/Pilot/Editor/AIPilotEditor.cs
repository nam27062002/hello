// AIPilotEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using AI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the AIPilot class (and all its heirs).
/// </summary>
[CustomEditor(typeof(AIPilot), true)]	// True to be used by heir classes as well
public class AIPilotEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	AIPilot m_targetAIPilot = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetAIPilot = target as AIPilot;

        m_targetAIPilot.databaseKey = target.name;

        // Load and validate stored data
        Load();
		m_targetAIPilot.ValidateComponentsData();
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Save target object
		Save();

		// Clear target object
		m_targetAIPilot = null;
	}

    /// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
        // Update the serialized object - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();

        // Default inspector
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();

        // If there have been changes, validate required component data
        if (EditorGUI.EndChangeCheck()) {
            m_targetAIPilot.ValidateComponentsData();
        }

        // Draw the data list!
        Undo.RecordObject(m_targetAIPilot, "AIPilot Changed");
        if (Application.isPlaying && m_targetAIPilot != null && m_targetAIPilot.brain != null) {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Current State: " + m_targetAIPilot.brain.current.name);
        }

        EditorGUILayout.Space();
        EditorGUILayoutExt.Separator(new SeparatorAttribute("State Machine Components Data"));
        Dictionary<string, AIPilot.StateComponentDataKVP> componentsData = BrainDataBase.instance.GetDataFor(m_targetAIPilot.databaseKey);

        if (componentsData != null) {
            foreach (AIPilot.StateComponentDataKVP kvp in componentsData.Values) {
                // Skip if data is null (meaning this component type doesn't need a data object)
                if (kvp.data == null) continue;

                // Make it foldable!
                // [AOC] Let's do nicer visuals than the boring default foldout!
                Type dataType = kvp.data.GetType();
                //kvp.folded = !EditorGUILayout.Foldout(!kvp.folded, dataType.Name);
				string behaviorName = dataType.Name.Replace("Data", "");
				behaviorName = System.Text.RegularExpressions.Regex.Replace(behaviorName, "[A-Z]", " $0").Trim();
				if (GUILayout.Button((kvp.folded ? "► " : "▼ ") + behaviorName, "ShurikenModuleTitle", GUILayout.ExpandWidth(true))) {
                    kvp.folded = !kvp.folded;
                }
                if (!kvp.folded) {
                    EditorGUI.indentLevel++;

                    // Unity's default editor doesn't know how to draw an object referenced by its base class, so we have to do it by ourselves -_-
                    // Find and iterate all properties of this data object
                    FieldInfo[] fields = TypeUtil.GetFields(dataType);
                    foreach (FieldInfo f in fields)
                    {
                        object currentValue = f.GetValue(kvp.data);
                        object newValue = DoField(f, currentValue);
                        f.SetValue(kvp.data, newValue);
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }

        // Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();
    }
    
	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}

	//------------------------------------------------------------------------//
	// INTERNAL UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Perform all the required operations to load the object's data.
	/// </summary>
	private void Load() {
		m_targetAIPilot.LoadFromJson(true);
	}

	/// <summary>
	/// Perform all the required operations to properly save the object's data.
	/// </summary>
	private void Save() {
		m_targetAIPilot.SaveToJson();
		EditorUtility.SetDirty(m_targetAIPilot);	// Set dirty required so when assets database is refreshed, this asset is saved as well
	}

	/// <summary>
	/// Draws the default inspector type for the given field.
	/// </summary>
	/// <returns>The new value of the field.</returns>
	/// <param name="_f">The field to be displayed.</param>
	/// <param name="_currentValue">The current value of the field.</param>
	private object DoField(FieldInfo _f, object _currentValue) {

		string fieldName = System.Text.RegularExpressions.Regex.Replace(_f.Name, "[A-Z]", " $0").Trim();

		// Check field type and use different editor widgets based on that
		// List
		if(_currentValue is IList) {
			// [AOC] TODO!! See HSX's EditorGUIField
		}

		// Enum
		else if(_f.FieldType.IsEnum) {
			return EditorGUILayout.EnumPopup(fieldName, (Enum)_currentValue);
		}

		// String
		else if(_f.FieldType == typeof(string)) {
			return EditorGUILayout.TextField(fieldName, (string)_currentValue);
		}

		// Float
		else if(_f.FieldType == typeof(float)) {
			return EditorGUILayout.FloatField(fieldName, (float)_currentValue);
		}

		// Bool
		else if(_f.FieldType == typeof(bool)) {
			return  EditorGUILayout.Toggle(fieldName, (bool)_currentValue);
		}

		// Int
		else if(_f.FieldType == typeof(int)) {
			return EditorGUILayout.IntField(fieldName, (int)_currentValue);
		}

		// Double
		else if(_f.FieldType == typeof(double)) {
			return EditorGUILayout.DoubleField(fieldName, (double)_currentValue);
		}

		// Vector 3
		else if(_f.FieldType == typeof(Vector3)) {
			return EditorGUILayout.Vector3Field(fieldName, (Vector3)_currentValue);
		}

		// Range
		else if(_f.FieldType == typeof(Range)) {
			// Impossible to use Range's custom editor, so use a horizontal layout instead
			Range currentRange = (Range)_currentValue;
			if(currentRange == null) currentRange = new Range();
			Range newRange = new Range();
			EditorGUILayout.BeginHorizontal(); {
				// Prefix label
				EditorGUILayout.PrefixLabel(fieldName);

				// Reset indent level whithin the horizontal layout
				int indentLevelBckp = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;

				// Min and max fields, with small labels
				EditorGUIUtility.labelWidth = 30f;
				newRange.min = EditorGUILayout.FloatField("min", currentRange.min);
				newRange.max = EditorGUILayout.FloatField("max", currentRange.max);

				// Restore indent level and label width
				EditorGUIUtility.labelWidth = 0f;
				EditorGUI.indentLevel = indentLevelBckp;
			} EditorGUILayout.EndHorizontal();
			return newRange;
		}

		// Action
		else if(_f.FieldType == typeof(Actions.Action)) {
			Actions.Action currentAction = (Actions.Action)_currentValue;

			EditorGUILayout.BeginHorizontal(); {
				// Prefix label
				EditorGUILayout.PrefixLabel(fieldName);

				// Reset indent level whithin the horizontal layout
				int indentLevelBckp = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;

				// Min and max fields, with small labels
				EditorGUIUtility.labelWidth = 30f;
				currentAction.active = EditorGUILayout.Toggle(currentAction.active);

				// Restore indent level and label width
				EditorGUIUtility.labelWidth = 0f;
				EditorGUI.indentLevel = indentLevelBckp;
			} EditorGUILayout.EndHorizontal();

			return currentAction;
		}

		// Class - put it at the end so other classes with custom display (string, Range) are processed before
		else if(_f.FieldType.IsClass) {
			// If current value is null, create a new instance of the type (Unity's default behaviour)
			if(_currentValue == null) {
				_currentValue = Activator.CreateInstance(_f.FieldType);	// See http://stackoverflow.com/questions/752/get-a-new-object-instance-from-a-type
			}

			// [AOC] Quick'n'dirty foldout management, all fields with identic parent class, type and field name will be folded/unfolded together
			string expandedPropertyID = _f.DeclaringType.FullName + "." + _f.FieldType.FullName + "." + _f.Name;
			bool expanded = EditorPrefs.GetBool(expandedPropertyID, true);
			expanded = EditorGUILayout.Foldout(expanded, _f.Name);
			EditorPrefs.SetBool(expandedPropertyID, expanded);
			if(expanded) {
				// Iterate through children fields!
				EditorGUI.indentLevel++;
				FieldInfo[] fields = TypeUtil.GetFields(_f.FieldType);
				foreach(FieldInfo f in fields) {
					object currentValue = f.GetValue(_currentValue);
					object newValue = DoField(f, currentValue);		// [AOC] Recursive call!
					f.SetValue(_currentValue, newValue);
				}
				EditorGUI.indentLevel--;
			}
			return _currentValue;	// In this case we directly modify the input object
		}

		// Unknown
		else {
			EditorGUILayout.TextField(fieldName, "type " + _f.FieldType.Name + " not supported");
		}
		return _currentValue;	// Value not processed
	}
}