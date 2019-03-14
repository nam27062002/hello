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

    private void ExportNPCassetBundles() { HDEditorAssetBundlesMenu.ExportAssetBundles(); }
    private void ImportNPCassetBundles() { HDEditorAssetBundlesMenu.ImportAssetBundles(); } 
    private void CreateAddressables()    { HDEditorAssetBundlesMenu.CreateAddressablesNPCs(); }
}
