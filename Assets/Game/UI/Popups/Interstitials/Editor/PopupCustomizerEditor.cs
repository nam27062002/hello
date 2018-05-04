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

	private PopupCustomizer m_target;

	void OnEnable() {
		m_target = target as PopupCustomizer;
	}

	public override void OnInspectorGUI() {
		// draw inspector
		DrawDefaultInspector();

		// draw export button
		if (GUILayout.Button("Save JSON")) {
			m_target.SaveJSON();
		}
	}
}
