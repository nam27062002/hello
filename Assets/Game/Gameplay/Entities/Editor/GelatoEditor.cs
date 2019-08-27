using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Gelato), true)]	
public class GelatoEditor : Editor {

	public override void OnInspectorGUI() {
		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element

		do {
			Debug.Log(p.name);
			if (p.name == "m_Script" || p.name == "m_hideNeedTierMessage" || p.name == "m_ObjectHideFlags") {
				// do nothing			
			} else {
				// Default
				EditorGUILayout.PropertyField(p, true);
			}
		} while (p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}
}
