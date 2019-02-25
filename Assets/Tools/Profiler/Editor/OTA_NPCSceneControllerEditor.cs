using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;


[CustomEditor(typeof(OTA_NPCSceneController))]
[CanEditMultipleObjects]
public class OTA_NPCSceneControllerEditor : Editor {

    private OTA_NPCSceneController m_component;
    private EditorAddressablesEntities m_addressablesEntities;

    private AssetBundleSubsets m_assetBundleSubsets;

    public void Awake() {
        m_component = target as OTA_NPCSceneController;
        m_addressablesEntities = new EditorAddressablesEntities();
    }

    private void OnEnable() {
        if (m_addressablesEntities == null) {
            m_addressablesEntities = new EditorAddressablesEntities();
        }
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Build NPCs")) {
            m_assetBundleSubsets = new AssetBundleSubsets(3);
            m_assetBundleSubsets.ChangeSubsetPrefix("npc_");
            m_assetBundleSubsets.ChangeSubsetName(0, "village");
            m_assetBundleSubsets.ChangeSubsetName(1, "castle");
            m_assetBundleSubsets.ChangeSubsetName(2, "dark");

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
                //m_component.Build(i, spawners);
                ParseISpawner(i, spawners);

                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);

                GameObject singletons = GameObject.Find("Singletons");
                if (singletons != null) {
                    DestroyImmediate(singletons);
                }

                spawners.Clear();
            }

            //m_component.CompareSets();
            m_assetBundleSubsets.BuildSubsets();

        }

        if (GUILayout.Button("Build Particles")) {
            ContentManager.InitContent(true, false);

            m_assetBundleSubsets = new AssetBundleSubsets(3);
            m_assetBundleSubsets.ChangeSubsetPrefix("particles_");
            m_assetBundleSubsets.ChangeSubsetName(0, "village");
            m_assetBundleSubsets.ChangeSubsetName(1, "castle");
            m_assetBundleSubsets.ChangeSubsetName(2, "dark");

            string[] particleDefinitions = new string[3];
            particleDefinitions[0] = "PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA1";
            particleDefinitions[1] = "PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA2";
            particleDefinitions[2] = "PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA3";

            for (int i = 0; i < 3; ++i) {
                List<DefinitionNode> poolSizes = DefinitionsManager.SharedInstance.GetDefinitionsList(particleDefinitions[i]);
                for (int j = 0; j < poolSizes.Count; ++j) {
                    DefinitionNode def = poolSizes[j];

                    string lods = "Master";
                    if (def.GetAsBool("veryHighVersion"))   lods += ";VeryHigh";
                    if (def.GetAsBool("highVersion"))       lods += ";High";
                    if (def.GetAsBool("lowVersion"))        lods += ";Low";

                    m_assetBundleSubsets.AddAssetName(i, def.sku, lods);
                }
            }

            m_assetBundleSubsets.BuildSubsets();
            m_assetBundleSubsets.AssignBundles();
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

        if (GUILayout.Button("TEST")) {
            m_assetBundleSubsets = new AssetBundleSubsets(3);
        }
    }

    public void FindISpawner(Transform _t, ref List<ISpawner> _list) {
        ISpawner c = _t.GetComponent<ISpawner>();
        if (c != null) {
            _list.Add(c);
        }
        // Not found, iterate children transforms
        foreach (Transform t in _t) {
            FindISpawner(t, ref _list);
        }
    }

    private void ParseISpawner(int _set, List<ISpawner> _spawners) {
        //lets instantiate one of each NPC           
        for (int i = 0; i < _spawners.Count; ++i) {
            List<string> prefabs = _spawners[i].GetPrefabList();

            if (prefabs != null) {
                for (int j = 0; j < prefabs.Count; ++j) {
                    if (prefabs[j].Contains("Drunken")) {
                        Debug.LogError(_spawners[i].name);
                    }
                    m_assetBundleSubsets.AddAssetName(_set, prefabs[j]);
                }
            }
        }
    }

    private void ExportNPCassetBundles() {
        List<AddressablesCatalogEntry> entries;
        List<string> bundles;

        m_addressablesEntities.GetEntriesAll(out entries, out bundles);

        StreamWriter writer = new StreamWriter("Assets/Editor/Addressables/editor_npc_addressables.json", false);
        writer.AutoFlush = true;

        writer.Write("{\"entries\":[");
        for (int i = 0; i < entries.Count; ++i) {
            writer.Write(entries[i].ToJSON().ToString());
            if (i < entries.Count - 1) {
                writer.Write(",");
            }
        }
        writer.Write("]}");
        writer.Close();
    }

    private void ImportNPCassetBundles() {
        string file = File.ReadAllText("Assets/Editor/Addressables/editor_npc_addressables.json");
        SimpleJSON.JSONNode data = SimpleJSON.JSONNode.Parse(file);

        SimpleJSON.JSONArray entries = data["entries"].AsArray;
        for (int i = 0; i < entries.Count; ++i) {
            SimpleJSON.JSONClass entry = entries[i].AsObject;
            string assetPath = AssetDatabase.GUIDToAssetPath(entry["guid"]);
            AssetImporter ai = AssetImporter.GetAtPath(assetPath);
            if (ai != null) {
                ai.SetAssetBundleNameAndVariant(entry["abName"], "");
            }
        }
    }

    private void CreateAddressables() {
        List<AddressablesCatalogEntry> entries;
        List<string> bundles;

        m_addressablesEntities.GetEntriesPrefab(out entries, out bundles);

        StreamWriter writer = new StreamWriter("Assets/Editor/Addressables/editor_addressablesCatalog.json", false) {
            AutoFlush = true
        };

        writer.Write("{");
        writer.Write("\"entries\":[");
        for (int i = 0; i < entries.Count; ++i) {
            writer.Write(entries[i].ToJSON().ToString());
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
