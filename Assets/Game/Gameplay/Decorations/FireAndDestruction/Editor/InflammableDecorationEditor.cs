using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(InflammableDecoration))]
[CanEditMultipleObjects]
public class InflammableDecorationEditor : Editor {
    static private string FIRE_PREFAB = "Assets/Art/3D/Gameplay/Particles/Master/PF_FireProc.prefab";

    static private bool m_editFireNodes;
	static private List<GameObject> m_fireParticles;

	private InflammableDecoration m_component;


	public void Awake() {
		m_component = target as InflammableDecoration;
		m_editFireNodes = false;
	}

	void OnEnable() {
		m_editFireNodes = false;
		m_fireParticles = new List<GameObject>();
	}

	void OnDisable() {
		SetFireNodesData();
	}

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        EditorGUILayoutExt.Separator(new SeparatorAttribute("Fire Nodes Setup"));

        EditorGUILayout.TextArea("Create a list of fire nodes from mesh and boxel size.");
        if (GUILayout.Button("Auto Build")) {
            if (m_editFireNodes == true) {
                SetFireNodesData();
                m_editFireNodes = false;
            }
            m_component.SetupFireNodes();
        }

        EditorGUILayout.TextArea("Effect used:\n" + FIRE_PREFAB);
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
        if (m_editFireNodes) {
            List<FireNode> fireNodes = m_component.fireNodes;

            for (int i = 0; i < fireNodes.Count; ++i) {
                FireNode fireNode = fireNodes[i];
                Vector3 position = m_component.transform.TransformPoint(fireNode.localPosition);
                Vector3 scale = GameConstants.Vector3.one * fireNode.scale;
                switch (Tools.current) {
                    case Tool.Scale: scale = Handles.ScaleHandle(scale, position, m_component.transform.rotation, 1f); break;
                    default: position = Handles.PositionHandle(position, m_component.transform.rotation); break;
                }

                fireNode.localPosition = m_component.transform.InverseTransformPoint(position);
                fireNode.scale = scale.x;

                if (i < m_fireParticles.Count) {
                    m_fireParticles[i].transform.position = position;
                    m_fireParticles[i].transform.localScale = scale;                    
                }
            }

            m_component.fireNodes = fireNodes;
        }
	}

	private void GetFireNodesData() {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FIRE_PREFAB);

        List<FireNode> fireNodes = m_component.fireNodes;
        for (int i = 0; i < fireNodes.Count; i++) {			
			m_fireParticles.Add(Instantiate(prefab));
			m_fireParticles[i].transform.position = m_component.transform.TransformPoint(m_component.transform.position +    fireNodes[i].localPosition);
            m_fireParticles[i].hideFlags = HideFlags.HideAndDontSave;
            FireProcController fpc = m_fireParticles[i].GetComponent<FireProcController>();
            fpc.EditorSetPower(6f);
            m_fireParticles[i].SetActive(true);            
        }
	}

	private void SetFireNodesData() {		
		for (int i = 0; i < m_fireParticles.Count; i++) {
            FireProcController fpc = m_fireParticles[i].GetComponent<FireProcController>();
            fpc.EditorSetPower(0f);
            Object.DestroyImmediate(m_fireParticles[i]);
		}
		m_fireParticles.Clear();
	}
}