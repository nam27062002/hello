using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DragonPowerUp))]
public class DragonPowerUpEditor : Editor {

	DragonPowerUp m_target = null;

	public void Awake() {
		m_target = (DragonPowerUp)target;
		

	}
	
	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
		if ( Application.isPlaying )
		{
			if ( GUILayout.Button("Reload Power Ups" ))
			{
				m_target.ResetPowerUps();
			}
		}
    }
}
