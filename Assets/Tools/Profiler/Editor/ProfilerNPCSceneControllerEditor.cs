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
				spawners.AddRange(sceneRoot[i].transform.FindComponentsRecursive<ISpawner>());
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
}
