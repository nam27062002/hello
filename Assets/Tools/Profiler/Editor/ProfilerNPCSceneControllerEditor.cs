using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


[CustomEditor(typeof(ProfilerNPCSceneController))]
[CanEditMultipleObjects]
public class ProfilerNPCSceneControllerEditor : Editor {

	private ProfilerNPCSceneController m_component;


	public void Awake() {
		m_component = target as ProfilerNPCSceneController;
	}

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		if (GUILayout.Button("Build")) {		
			if(!ContentManager.ready) ContentManager.InitContent(true);

			UnityEngine.SceneManagement.Scene scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Game/Scenes/Levels/Spawners/" + m_component.npcScene + ".unity", UnityEditor.SceneManagement.OpenSceneMode.Additive);

			List<ISpawner> spawners = new List<ISpawner>();
			GameObject[] sceneRoot = scene.GetRootGameObjects();
			for (int i = 0; i < sceneRoot.Length; ++i) {
				FindISpawner(sceneRoot[i].transform, ref spawners);
				sceneRoot[i].SetActive(false);
			}

			m_component.Build(spawners);

			UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);

			GameObject singletons = GameObject.Find("Singletons");
			if (singletons != null) {
				GameObject.DestroyImmediate(singletons);
			}
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
