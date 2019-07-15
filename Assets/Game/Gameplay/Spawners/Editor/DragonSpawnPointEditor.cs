
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DragonSpawnPoint), true)]
[CanEditMultipleObjects]
public class DragonSpawnPointEditor : Editor {

	public override void OnInspectorGUI() {
		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element

		bool isCandleEffectEnabled = false;
		do {			
			// Properties requiring special treatment
			if (p.name == "m_ObjectHideFlags" || p.name == "m_Script") {

			} else if (p.name == "m_enableCandleEffect") {
				isCandleEffectEnabled = p.boolValue;
				EditorGUILayout.PropertyField(p, true);
			} else if (p.name == "m_candleData") {
				if (isCandleEffectEnabled) {
					EditorGUILayout.PropertyField(p, true);
				}
			} else {
				EditorGUILayout.PropertyField(p, true);
			}			
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}
}
