﻿// AIPilotEditor.cs
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

		// Validate stored data
		ValidateData();
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
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
		DrawDefaultInspector();

		// Draw the data list!
		Undo.RecordObject(m_targetAIPilot, "AIPilot Changed");
		EditorGUILayout.Space();
		EditorGUILayoutExt.Separator(new SeparatorAttribute("State Machine Components Data"));
		foreach(AIPilot.StateComponentDataKVP kvp in m_targetAIPilot.componentsData) {
			// Skip if data is null (meaning this component type doesn't need a data object)
			if(kvp.data == null) continue;

			// Make it foldable!
			// [AOC] Let's do nicer visuals than the boring default foldout!
			Type dataType = kvp.data.GetType();
			//kvp.folded = !EditorGUILayout.Foldout(!kvp.folded, dataType.Name);
			if(GUILayout.Button((kvp.folded ? "► " : "▼ ") + dataType.Name, "ShurikenModuleTitle", GUILayout.ExpandWidth(true))) {
				kvp.folded = !kvp.folded;
			}
			if(!kvp.folded) {
				EditorGUI.indentLevel++;

				// Unity's default editor doesn't know how to draw an object referenced by its base class, so we have to do it by ourselves -_-
				// Find and iterate all properties of this data object
				FieldInfo[] fields = TypeUtil.GetFields(dataType);
				foreach(FieldInfo f in fields) {
					object currentValue = f.GetValue(kvp.data);
					object newValue = DoField(f, currentValue);
					f.SetValue(kvp.data, newValue);
				}

				EditorGUI.indentLevel--;
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
	/// Make sure the target AI Pilot has exactly one data per type component.
	/// Will add data objects when missing and remove them when component not 
	/// found in any state of the state machine (brain).
	/// </summary>
	private void ValidateData() {
		// Iterate all components in all states of the state machine
		// If a data object for that component type doesn't exist, add it
		HashSet<string> validComponentNames = new HashSet<string>();	// Store component names for later usage
		foreach(State state in m_targetAIPilot.brainResource.states) {
			foreach(StateComponent component in state.componentAssets) {
				// If this component data type has already been checked, skip it
				string typeName = component.GetType().AssemblyQualifiedName;
				if(validComponentNames.Add(typeName)) {		// Returns true if the value was not already in the hash
					// Check whether we have a data object for this component type
					// Inefficient, but since it's an editor code, we don't care
					bool found = false;
					foreach(AIPilot.StateComponentDataKVP kvp in m_targetAIPilot.componentsData) {
						if(kvp.typeName == typeName) {
							// Special case!! If data is null, it may be because this component type didn't have data up until now
							// Force brute create a new data object (it will still be null if component's requirements haven't changed)
							if(kvp.data == null) {
								kvp.data = component.CreateData();
							}
							found = true;
							break;
						}
					}

					// If data wasn't found, create one and add it to the pilot
					if(!found) {
						AIPilot.StateComponentDataKVP newKvp = new AIPilot.StateComponentDataKVP();
						newKvp.typeName = typeName;
						newKvp.data = component.CreateData();	// [AOC] CreateData() will create the proper data object for this specific component type. Can be null!
						m_targetAIPilot.componentsData.Add(newKvp);	
					}
				}
			}
		}

		// Iterate all data objects.
		// If the component type linked to a data object is not found on the state machine, delete it
		// Reverse iteration since we'll be deleting items from the same list we're iterating
		for(int i = m_targetAIPilot.componentsData.Count - 1; i >= 0; i--) {
			// Is it a valid component?
			if(!validComponentNames.Contains(m_targetAIPilot.componentsData[i].typeName)) {
				// No, delete its data object
				m_targetAIPilot.componentsData.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Draws the default inspector type for the given field.
	/// </summary>
	/// <returns>The new value of the field.</returns>
	/// <param name="_f">The field to be displayed.</param>
	/// <param name="_currentValue">The current value of the field.</param>
	private object DoField(FieldInfo _f, object _currentValue) {
		// Check field type and use different editor widgets based on that
		// List
		if(_currentValue is IList) {
			// [AOC] TODO!! See HSX's EditorGUIField
		}

		// Enum
		else if(_f.FieldType.IsEnum) {
			return EditorGUILayout.EnumPopup(_f.Name, (Enum)_currentValue);
		}

		// String
		else if(_f.FieldType == typeof(string)) {
			return EditorGUILayout.TextField(_f.Name, (string)_currentValue);
		}

		// Float
		else if(_f.FieldType == typeof(float)) {
			return EditorGUILayout.FloatField(_f.Name, (float)_currentValue);
		}

		// Bool
		else if(_f.FieldType == typeof(bool)) {
			return  EditorGUILayout.Toggle(_f.Name, (bool)_currentValue);
		}

		// Int
		else if(_f.FieldType == typeof(int)) {
			return EditorGUILayout.IntField(_f.Name, (int)_currentValue);
		}

		// Double
		else if(_f.FieldType == typeof(double)) {
			return EditorGUILayout.DoubleField(_f.Name, (double)_currentValue);
		}

		// Range
		else if(_f.FieldType == typeof(Range)) {
			// Impossible to use Range's custom editor, so use a horizontal layout instead
			Range currentRange = (Range)_currentValue;
			if(currentRange == null) currentRange = new Range();
			Range newRange = new Range();
			EditorGUILayout.BeginHorizontal(); {
				// Prefix label
				EditorGUILayout.PrefixLabel(_f.Name);

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
			EditorGUILayout.TextField(_f.Name, "type " + _f.FieldType.Name + " not supported");
		}
		return _currentValue;	// Value not processed
	}
}