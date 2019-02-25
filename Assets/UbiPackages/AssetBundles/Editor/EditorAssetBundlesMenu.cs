using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class EditorAssetBundlesMenu : MonoBehaviour
{
    private const string ROOT_MENU = "Tech/AssetBundles/";
    private const string MENU_BROWSER = ROOT_MENU + "Browser";
    private const string MENU_LAUNCH_LOCAL_SERVER = ROOT_MENU + "Launch Local Server";
    private const string MENU_GENERATE_ASSETS_LUT_FROM_DOWNLOADABLES_CATALOG = ROOT_MENU + "Generate AssetsLUT from Downloadables";
    private const string MENU_GENERATE_DOWNLOADABLES_CATALOG_FROM_ASSETS_LUT = ROOT_MENU + "Generate Downloadables from AssetsLUT";
    private const string MENU_EXPORT_ASSET_BUNDLES_NPCS = ROOT_MENU + "Export asset bundles of NPCs and Particles";
    private const string MENU_IMPORT_ASSET_BUNDLES_NPCS = ROOT_MENU + "Import asset bundles of NPCs and Particles";
    private const string MENU_CREATE_ADDRESSABLES_NPCS = ROOT_MENU + "Create addressables only NPCs";
    private const string MENU_AUTO_GENERATE = ROOT_MENU + "Auto Generate/";
    private const string MENU_AUTO_GENERATE_NPCS = MENU_AUTO_GENERATE + "Auto Generate NPCs Asset Bundles";
    private const string MENU_AUTO_GENERATE_PARTICLES = MENU_AUTO_GENERATE + "Auto Generate Particles Asset Bundles";


    [MenuItem(MENU_BROWSER, false, 1)]
    static void ShowBrowser()
    {
        AssetBundleBrowser.AssetBundleBrowserMain.ShowWindow();        
    }
        
    [MenuItem(MENU_LAUNCH_LOCAL_SERVER, false, 2)]
    public static void ToggleLocalAssetBundleServer()
    {
        AssetBundles.LaunchAssetBundleServer.SetRemoteAssetsFolderName(EditorAssetBundlesManager.DOWNLOADABLES_FOLDER);
        AssetBundles.LaunchAssetBundleServer.ToggleLocalAssetBundleServer();
    }

    
    [MenuItem(MENU_LAUNCH_LOCAL_SERVER, true, 3)]
    public static bool ToggleLocalAssetBundleServerValidate()
    {
        return AssetBundles.LaunchAssetBundleServer.ToggleLocalAssetBundleServerValidate(MENU_LAUNCH_LOCAL_SERVER);
    }

    [MenuItem(MENU_GENERATE_ASSETS_LUT_FROM_DOWNLOADABLES_CATALOG, false, 4)]
    public static void GenerateAssetsLUTFromDownloadablesCatalog()
    {
        EditorAssetBundlesManager.GenerateAssetsLUTFromDownloadablesCatalog();        
    }

    [MenuItem(MENU_GENERATE_DOWNLOADABLES_CATALOG_FROM_ASSETS_LUT, false, 5)]
    public static void GenerateDownloadablesCatalogFromAssetsLUT()
    {
        EditorAssetBundlesManager.GenerateDownloadablesCatalogFromAssetsLUT();
    }

    [MenuItem(MENU_EXPORT_ASSET_BUNDLES_NPCS, false, 50)]
    public static void ExportAssetBundles() {
        List<AddressablesCatalogEntry> entries;
        List<string> bundles;

        EditorAddressables_NPCs_Particles.GetEntriesAll(out entries, out bundles);

        StreamWriter writer = new StreamWriter("Assets/Editor/Addressables/export_npc_asset_bundles.json", false);
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

    [MenuItem(MENU_IMPORT_ASSET_BUNDLES_NPCS, false, 51)]
    public static void ImportAssetBundles() {
        string file = File.ReadAllText("Assets/Editor/Addressables/import_npc_asset_bundles.json");
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

    [MenuItem(MENU_CREATE_ADDRESSABLES_NPCS, false, 52)]
    public static void CreateAddressablesNPCs() {
        List<AddressablesCatalogEntry> entries;
        List<string> bundles;

        EditorAddressables_NPCs_Particles.GetEntriesPrefab(out entries, out bundles);

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

        string[] areas = { "village", "castle", "dark" };
        for (int i = 0; i < 3; ++i) {
            if (i > 0) { writer.Write(","); }
            writer.Write("{\"id\":\"area" + (i + 1) + "\", \"assetBundles\":[");
            int bundlesAdded = 0;
            for (int b = 0; b < bundles.Count; ++b) {
                if (bundles[b].Contains(areas[i])) {
                    if (bundlesAdded > 0) { writer.Write(","); }
                    writer.Write("\"" + bundles[b] + "\"");
                    bundlesAdded++;
                }
            }
            writer.Write("]}");
        }
        writer.Write("]");
        writer.Write("}");
        writer.Close();
    }

    [MenuItem(MENU_AUTO_GENERATE_NPCS, false, 53)]
    public static void AutoGenerateAssetsBundlesNPCs() {
        AssetBundleSubsets assetBundleSubsets = new AssetBundleSubsets(3);
        assetBundleSubsets.ChangeSubsetPrefix("npc_");
        assetBundleSubsets.ChangeSubsetName(0, "village");
        assetBundleSubsets.ChangeSubsetName(1, "castle");
        assetBundleSubsets.ChangeSubsetName(2, "dark");

        string[] scenePaths = new string[3];
        scenePaths[0] = "Assets/Game/Scenes/Levels/Spawners/SP_Medieval_Final_Village.unity";
        scenePaths[1] = "Assets/Game/Scenes/Levels/Spawners/SP_Medieval_Final_Castle.unity";
        scenePaths[2] = "Assets/Game/Scenes/Levels/Spawners/SP_Medieval_Final_Dark.unity";

        List<ISpawner> spawners = new List<ISpawner>();

        for (int i = 0; i < 3; ++i) {
            UnityEngine.SceneManagement.Scene scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePaths[i], UnityEditor.SceneManagement.OpenSceneMode.Additive);
            GameObject[] sceneRoot = scene.GetRootGameObjects();
            for (int t = 0; t < sceneRoot.Length; ++t) {
                FindISpawner(sceneRoot[t].transform, ref spawners);
                sceneRoot[t].SetActive(false);
            }


            for (int s = 0; s < spawners.Count; ++s) {
                List<string> prefabs = spawners[s].GetPrefabList();

                if (prefabs != null) {
                    for (int j = 0; j < prefabs.Count; ++j) {
                        assetBundleSubsets.AddAssetName(s, prefabs[j]);
                    }
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);

            GameObject singletons = GameObject.Find("Singletons");
            if (singletons != null) {
                DestroyImmediate(singletons);
            }

            spawners.Clear();
        }

        //m_component.CompareSets();
        assetBundleSubsets.BuildSubsets();
    }

    [MenuItem(MENU_AUTO_GENERATE_PARTICLES, false, 54)]
    public static void AutoGenerateAssetsBundlesParticles() {
        ContentManager.InitContent(true, false);

        AssetBundleSubsets assetBundleSubsets = new AssetBundleSubsets(3);
        assetBundleSubsets.ChangeSubsetPrefix("particles_medieval_");
        assetBundleSubsets.ChangeSubsetName(0, "village");
        assetBundleSubsets.ChangeSubsetName(1, "castle");
        assetBundleSubsets.ChangeSubsetName(2, "dark");

        string[] particleDefinitions = new string[3];
        particleDefinitions[0] = "PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA1";
        particleDefinitions[1] = "PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA2";
        particleDefinitions[2] = "PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA3";

        for (int i = 0; i < 3; ++i) {
            List<DefinitionNode> poolSizes = DefinitionsManager.SharedInstance.GetDefinitionsList(particleDefinitions[i]);
            for (int j = 0; j < poolSizes.Count; ++j) {
                DefinitionNode def = poolSizes[j];

                string lods = "Master";
                if (def.GetAsBool("veryHighVersion")) lods += ";VeryHigh";
                if (def.GetAsBool("highVersion")) lods += ";High";
                if (def.GetAsBool("lowVersion")) lods += ";Low";

                assetBundleSubsets.AddAssetName(i, def.sku, lods);
            }
        }

        assetBundleSubsets.BuildSubsets();
        assetBundleSubsets.AssignBundles();
    }

    private static void FindISpawner(Transform _t, ref List<ISpawner> _list) {
        ISpawner c = _t.GetComponent<ISpawner>();
        if (c != null) {
            _list.Add(c);
        }
        // Not found, iterate children transforms
        foreach (Transform t in _t) {
            FindISpawner(t, ref _list);
        }
    }
}
