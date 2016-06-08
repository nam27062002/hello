﻿using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(InflammableDecoration))]
public class InflammableDecorationEditor : Editor {
	static private bool m_editFireNodes;
	static private List<Transform> m_fireNodes;
	static private List<GameObject> m_fireParticles;

	public void Awake() {
		m_editFireNodes = false;
	}

	void OnEnable() {
		m_editFireNodes = false;
		m_fireNodes = new List<Transform>();
		m_fireParticles = new List<GameObject>();
	}

	void OnDisable() {
		SetFireNodesData();
	}

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		EditorGUILayoutExt.Separator(new SeparatorAttribute("Fire Nodes"));
		if (m_editFireNodes == false) {
			if (GUILayout.Button("Show")) {
				GetFireNodesData();
				m_editFireNodes = true;
			}
		} else {
			if (GUILayout.Button("Hide")) {
				SetFireNodesData();
				m_editFireNodes = false;
			}
		}
	}

	void OnSceneGUI() {
		for (int i = 0; i < m_fireNodes.Count; i++) {
			switch (Tools.current) {
				case Tool.Move:		 m_fireNodes[i].position = Handles.PositionHandle(m_fireNodes[i].position, m_fireNodes[i].rotation);								break;
				case Tool.Rotate:	 m_fireNodes[i].localRotation = Handles.RotationHandle(m_fireNodes[i].localRotation, m_fireNodes[i].position);						break;
				case Tool.Scale:	 m_fireNodes[i].localScale = Handles.ScaleHandle(m_fireNodes[i].localScale, m_fireNodes[i].position, m_fireNodes[i].rotation, 1.5f);break;
			}
			m_fireParticles[i].transform.CopyFrom(m_fireNodes[i]);
			m_fireParticles[i].GetComponent<ParticleSystem>().Simulate(1f, true);
		}
	}

	private void GetFireNodesData() {
		GameObject prefab = (GameObject)Resources.Load("Particles/PF_BuildingFire");
		InflammableDecoration targetGO = (InflammableDecoration)target;

		FireNode[] nodes = targetGO.transform.GetComponentsInChildren<FireNode>();
		for (int i = 0; i < nodes.Length; i++) {
			m_fireNodes.Add(nodes[i].transform);

			m_fireParticles.Add(Instantiate(prefab));
			m_fireParticles[i].transform.CopyFrom(m_fireNodes[i]);
			m_fireParticles[i].hideFlags = HideFlags.HideAndDontSave;
			m_fireParticles[i].GetComponent<ParticleSystem>().Simulate(1f, true);
		}
	}

	private void SetFireNodesData() {		
		for (int i = 0; i < m_fireParticles.Count; i++) {
			GameObject.DestroyImmediate(m_fireParticles[i]);
		}
		m_fireParticles.Clear();
		m_fireNodes.Clear();
	}
}