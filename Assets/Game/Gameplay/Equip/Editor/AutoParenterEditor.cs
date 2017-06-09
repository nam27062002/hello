using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AutoParenter))]
public class AutoParenterEditor : Editor {

	AutoParenter m_target = null;

	private void OnEnable() {
		if ( !Application.isPlaying ){
			m_target = target as AutoParenter;
		}
	}

	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
		if ( GUILayout.Button("Copy Target Position And Rotation") )
		{
			m_target.CopyTargetPosAndRot();
		}
    }
}
