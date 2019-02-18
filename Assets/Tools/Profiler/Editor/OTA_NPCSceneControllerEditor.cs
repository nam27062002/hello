using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
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

        if (GUILayout.Button("Export NPCs AB")) {
            ExportNPCassetBundles();
        }

        if (GUILayout.Button("Import NPCs AB")) {
            ImportNPCassetBundles();
        }

        if (GUILayout.Button("Create Addressables")) {
            CreateAddressables();
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

    private void ExportNPCassetBundles() {
        string[] paths = { "Assets/AI", "Assets/Art/3D/Gameplay/Entities", "Assets/Resources/Game/Entities/NewEntites/" };

        List<string> entries = new List<string>();
        for (int i = 0; i < paths.Length; ++i) {
            PrintDirectory(new DirectoryInfo(paths[i]), entries, null);
        }

        StreamWriter writer = new StreamWriter("Assets/Editor/Addressables/editor_npc_addressables.json", false);
        writer.AutoFlush = true;

        writer.Write("{\"entries\":[");
        for (int i = 0; i < entries.Count; ++i) {
            writer.Write(entries[i]);
            if (i < entries.Count - 1) {
                writer.Write(",");
            }
        }
        writer.Write("]}");
        writer.Close();
    }

    private void PrintDirectory(DirectoryInfo _directory, List<string> _entries, HashSet<string> _bundles) {
        DirectoryInfo[] directories = _directory.GetDirectories();
        foreach (DirectoryInfo directory in directories) {
            PrintDirectory(directory, _entries, _bundles);
        }

        FileInfo[] files = _directory.GetFiles();
        foreach (FileInfo file in files) {
            string filePath = file.FullName;
            filePath = filePath.Substring(filePath.IndexOf("Assets/", System.StringComparison.Ordinal));

            AssetImporter ai = AssetImporter.GetAtPath(filePath);
            if (ai != null) {
                string assetBundle = ai.assetBundleName;

                if (!string.IsNullOrEmpty(assetBundle)) {
                    if (_entries != null) {
                        _entries.Add("{\"id\":\"" + Path.GetFileNameWithoutExtension(file.Name) + "\"," +
                                      "\"locationType\":\"AssetBundles\"," +
                                      "\"guid\":\"" + AssetDatabase.AssetPathToGUID(filePath) + "\"," +
                                      "\"abName\":\"" + assetBundle + "\"}");
                    }

                    if (_bundles != null) {
                        _bundles.Add(assetBundle);
                    }
                }
            }
        }
    }

    private void ImportNPCassetBundles() {
        string file = File.ReadAllText("Assets/Editor/Addressables/editor_npc_addressables.json");
        SimpleJSON.JSONNode data = SimpleJSON.JSONNode.Parse(file);

        SimpleJSON.JSONArray entries = data["entries"].AsArray;
        for (int i = 0; i < entries.Count; ++i) {
            string assetPath = AssetDatabase.GUIDToAssetPath(entries["guid"]);
            AssetImporter ai = AssetImporter.GetAtPath(assetPath);
            if (ai != null) {
                ai.SetAssetBundleNameAndVariant(entries["abName"], "");
            }
        }
    }

    private void CreateAddressables() {
        string[] paths = { "Assets/AI", "Assets/Art/3D/Gameplay/Entities", "Assets/Resources/Game/Entities/NewEntites/" };

        List<string> entries = new List<string>();
        HashSet<string> bundlesSet = new HashSet<string>();

        PrintDirectory(new DirectoryInfo("Assets/AI"), null, bundlesSet);
        PrintDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Entities"), null, bundlesSet);
        PrintDirectory(new DirectoryInfo("Assets/Resources/Game/Entities/NewEntites/"), entries, bundlesSet);

        List<string> bundles = bundlesSet.ToList();


        StreamWriter writer = new StreamWriter("Assets/Editor/Addressables/editor_addressablesCatalog.json", false);
        writer.AutoFlush = true;

        writer.Write("{");
        writer.Write("\"entries\":[");
        for (int i = 0; i < entries.Count; ++i) {
            writer.Write(entries[i]);
            if (i < entries.Count - 1) {
                writer.Write(",");
            }
        }
        writer.Write("],");
        writer.Write("\"localAssetBundles\":[");
        for (int i = 0; i < bundles.Count; ++i) {
            writer.Write("\"" + bundles[i] + "\"");
            if (i < bundles.Count - 1) {
                writer.Write(",");
            }
        }
        writer.Write("],");
        writer.Write("\"areas\":[");
        writer.Write("{\"id\":\"area1\", \"assetBundles\":[\"npc_shared\",\"npc_medieval_common\",\"npc_medieval_village\",\"npc_medieval_village_castle\",\"npc_medieval_village_dark\"]},");
        writer.Write("{\"id\":\"area2\", \"assetBundles\":[\"npc_shared\",\"npc_medieval_common\",\"npc_medieval_castle\",\"npc_medieval_village_castle\",\"npc_medieval_castle_dark\"]},");
        writer.Write("{\"id\":\"area3\", \"assetBundles\":[\"npc_shared\",\"npc_medieval_common\",\"npc_medieval_dark\",\"npc_medieval_village_dark\",\"npc_medieval_castle_dark\"]}");
        writer.Write("]");
        writer.Write("}");
        writer.Close();
    }
}
