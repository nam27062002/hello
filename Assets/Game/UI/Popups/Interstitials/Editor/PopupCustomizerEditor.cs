//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
[CustomEditor(typeof(PopupCustomizer))]
public class PopupCustomizerEditor : Editor {
	
	public override void OnInspectorGUI() {
		// draw inspector
		DrawDefaultInspector();

		// draw export button
		if (GUILayout.Button("Export JSON")) {
			ExportJSON();
		}
	}

	private void ExportJSON() {
		SerializedProperty p = serializedObject.GetIterator();
		/*do {
			//if (p.type
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)
		*/
	}
}
