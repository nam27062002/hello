using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameSettings))]
public class GameSettingsEditor : Editor {
	
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		PlayerSettings.bundleVersion = GameSettings.iOSVersion.ToString();
	}
}
