using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(FireNodeSetup))]
public class FireNodeSetupEditor : Editor {

	private FireNodeSetup m_component;
	private SerializedProperty m_boxelSizeProp = null;

	public void Awake() {
		m_component = target as FireNodeSetup;
	}

	void OnEnable() {
		m_boxelSizeProp = serializedObject.FindProperty("m_boxelSize");
	}

	void OnDisable() {
		m_boxelSizeProp = null;
	}

	public override void OnInspectorGUI() {	
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(m_boxelSizeProp, true);
		if (EditorGUI.EndChangeCheck()) {
			if (m_boxelSizeProp.intValue < 1) {
				m_boxelSizeProp.intValue = 1;
			}

			m_component.Init();
			m_component.Build();
		}

		EditorGUILayoutExt.Separator(new SeparatorAttribute("Fire Nodes Auto Setup"));

		if (GUILayout.Button("Refresh")) {
			m_component.Init();
			m_component.Build();
		}

		serializedObject.ApplyModifiedProperties();
	}
}