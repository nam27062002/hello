using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(ParticleTester))]
public class ParticleTesterEditor : Editor {

	private ParticleTester m_target;

	// Use this for initialization
	void Start () {
		m_target = (ParticleTester)target;
	}
	
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		if (GUILayout.Button("spawn")) {
			((ParticleTester)target).SpawnParticle();
		}
	}
}
