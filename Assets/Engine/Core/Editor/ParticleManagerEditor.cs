using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ParticleManager))]
public class ParticleManagerEditor : Editor {

	ParticleManager m_target = null;

	public void Awake() {
		m_target = (ParticleManager)target;
	}

	
	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		if ( Application.isPlaying )
		{
			if ( GUILayout.Button("Clean Unused Particles" ))
			{
				ParticleManager.ClearUnsued();
			}
		}
    }
}
