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
			if (!string.IsNullOrEmpty(m_target.parentName)) {
				Transform t = m_target.transform;
				Transform p;
				if (m_target.parentRoot == null)
					p = t.parent.FindTransformRecursive(m_target.parentName);
				else
					p = m_target.parentRoot.FindTransformRecursive(m_target.parentName);

				if (p != null) {
					t.position = p.position;
					t.rotation = p.rotation;
				} 
			}
		}
    }
}
