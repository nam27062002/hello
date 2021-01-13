using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AutoParenter), true)]
[CanEditMultipleObjects]
public class AutoParenterEditor : Editor {
	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		GUI.enabled = !Application.isPlaying;
		if ( GUILayout.Button("Copy Target Position And Rotation") )
		{
			Undo.RecordObjects(targets, "AutoParenter.CopyTargetPosAndRot");
			for(int i = 0; i < targets.Length; ++i) {
				(targets[i] as AutoParenter).CopyTargetPosAndRot();
			}
		}
		GUI.enabled = true;

		GUI.color = Colors.paleGreen;
		if(GUILayout.Button("Re-parent!", GUILayout.Height(30f))) {
			Undo.RecordObjects(targets, "AutoParenter.Reparent");
			for(int i = 0; i < targets.Length; ++i) {
				(targets[i] as AutoParenter).Reparent();
			}
		}
		GUI.color = Color.white;
	}
}
