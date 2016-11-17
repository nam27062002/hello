using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MenuPetLoader))]
public class MenuPetLoaderEditor : Editor {

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		if (GUILayout.Button("Load Pet Preview")) {
			((MenuPetLoader)target).Reload();
		}
	}
}
