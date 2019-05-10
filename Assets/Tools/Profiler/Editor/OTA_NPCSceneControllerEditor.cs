using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;


[CustomEditor(typeof(OTA_NPCSceneController))]
[CanEditMultipleObjects]
public class OTA_NPCSceneControllerEditor : Editor {

    private OTA_NPCSceneController m_component;
    private AssetBundleSubsets m_assetBundleSubsets;

    public void Awake() {
        m_component = target as OTA_NPCSceneController;
    }

    private void OnEnable() {

    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Print single file")) {
            m_component.Build(OTA_NPCSceneController.Log.Assets);
        }

        if (GUILayout.Button("Print multiple files")) {
            m_component.Build(OTA_NPCSceneController.Log.Subsets);
        }
    }
}
