using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MenuDragonLoader))]
public class MenuDragonLoaderEditor : Editor {

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		if (GUILayout.Button("Load Dragon Preview")) {
			((MenuDragonLoader)target).RefreshDragon();
		}
	}
}
