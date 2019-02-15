using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


[CustomEditor(typeof(OTA_NPCSceneController))]
[CanEditMultipleObjects]
public class OTA_NPCSceneControllerEditor : Editor {

	private OTA_NPCSceneController m_component;


	public void Awake() {
		m_component = target as OTA_NPCSceneController;
	}

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Build")) {
            string[] scenePaths = new string[3];
            scenePaths[0] = "Assets/Game/Scenes/Levels/Spawners/" + m_component.area1Scene + ".unity";
            scenePaths[1] = "Assets/Game/Scenes/Levels/Spawners/" + m_component.area2Scene + ".unity";
            scenePaths[2] = "Assets/Game/Scenes/Levels/Spawners/" + m_component.area3Scene + ".unity";

            List<ISpawner> spawners = new List<ISpawner>();

            for (int i = 0; i < 3; ++i) {
                UnityEngine.SceneManagement.Scene scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePaths[i], UnityEditor.SceneManagement.OpenSceneMode.Additive);
                GameObject[] sceneRoot = scene.GetRootGameObjects();
                for (int t = 0; t < sceneRoot.Length; ++t) {
                    FindISpawner(sceneRoot[t].transform, ref spawners);
                    sceneRoot[t].SetActive(false);
                }
                m_component.Build(i + 1, spawners);

                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);

                GameObject singletons = GameObject.Find("Singletons");
                if (singletons != null) {
                    DestroyImmediate(singletons);
                }

                spawners.Clear();
            }

            m_component.CompareSets();
        }


        if (GUILayout.Button("Find by GUID")) {
            m_component.FindGUI();
        }
    }

	public void FindISpawner(Transform _t, ref List<ISpawner> _list) {		
		ISpawner c = _t.GetComponent<ISpawner>();
		if (c != null) {
			_list.Add(c);
		}
		// Not found, iterate children transforms
		foreach(Transform t in _t) {
			FindISpawner(t, ref _list);
		}
	}
}
