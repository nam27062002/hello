using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class HDEditorAssetBundlesMenu : MonoBehaviour
{
    private const string ROOT_MENU = "Tech/AssetBundles/";
    private const string MENU_EXPORT_ASSET_BUNDLES_NPCS = ROOT_MENU + "Export asset bundles of NPCs and Particles";
    private const string MENU_IMPORT_ASSET_BUNDLES_NPCS = ROOT_MENU + "Import asset bundles of NPCs and Particles";
    private const string MENU_CREATE_ADDRESSABLES_NPCS = ROOT_MENU + "Create addressables only NPCs";
    private const string MENU_AUTO_GENERATE = ROOT_MENU + "Auto Generate/";
    private const string MENU_AUTO_GENERATE_NPCS = MENU_AUTO_GENERATE + "Auto Generate NPCs Asset Bundles";
    private const string MENU_AUTO_GENERATE_PARTICLES = MENU_AUTO_GENERATE + "Auto Generate Particles Asset Bundles";
    private const string MENU_AUTO_PROCESS_SCENES = MENU_AUTO_GENERATE + "Auto Process scenes";

    private const string MENU_DRAGONS_ASSIGN_BUNDLES =ROOT_MENU + "Dragons/Auto Assign Dragon Bundles";

   
    [MenuItem(MENU_EXPORT_ASSET_BUNDLES_NPCS, false, 50)]
    public static void ExportAssetBundles() {
        StreamWriter writer = new StreamWriter("Assets/Editor/Addressables/export_asset_bundles.json", false);
        writer.AutoFlush = true;

        writer.Write(EditorAutomaticAddressables.BuildRestoreCatalog().ToString());
        writer.Close();
    }

    [MenuItem(MENU_IMPORT_ASSET_BUNDLES_NPCS, false, 51)]
    public static void ImportAssetBundles() {
        string file = File.ReadAllText("Assets/Editor/Addressables/export_asset_bundles.json");
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
        StreamWriter writer = new StreamWriter("Assets/Editor/Addressables/editor_addressablesCatalog.json", false) {
            AutoFlush = true
        };

        writer.Write(EditorAutomaticAddressables.BuildCatalog(true).ToString());
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
                        assetBundleSubsets.AddAssetName(i, prefabs[j]);
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

        assetBundleSubsets.BuildSubsets();
        assetBundleSubsets.AssignBundles();
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


    [MenuItem(MENU_AUTO_PROCESS_SCENES, false, 54)]
    public static void ProcessScenes() {
        SceneOptimizerEditor.BatchOptimization();
    }

    [MenuItem(MENU_DRAGONS_ASSIGN_BUNDLES, false, 54)]
    public static void AssgignDragonBundles()
    {
        ContentManager.InitContent(true, false);
    
        Dictionary<string, DefinitionNode> dragons = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.DRAGONS);
        
        foreach(KeyValuePair<string, DefinitionNode> pair in dragons)
        {
            DefinitionNode dragonDef = pair.Value;
            // if ( pair.Key != "dragon_baby" && pair.Key != "dragon_crocodile" && pair.Key != "dragon_reptile" )
            {    
                List<string> prefabs = new List<string>();
                List<string> local_prefabs = new List<string>();
                List<string> materials = new List<string>();
                List<string> local_materials = new List<string>();
                List<string> icons = new List<string>();
                List<string> local_icons = new List<string>();


                // Assign dragon stuff to a bundle
                if ( dragonDef.Get("type") == "normal" )
                {
                    string menuPrefab = dragonDef.Get("menuPrefab");
                    if (!string.IsNullOrEmpty(menuPrefab))
                        local_prefabs.Add(menuPrefab);

                    string gamePrefab = dragonDef.Get("gamePrefab");
                    if ( !string.IsNullOrEmpty( gamePrefab ) )
                        prefabs.Add(gamePrefab);
                    
                    string resultsPrefab = dragonDef.Get("resultsPrefab");
                    if ( !string.IsNullOrEmpty(resultsPrefab) )
                        prefabs.Add( resultsPrefab );
                        
                    string animojiPrefab = dragonDef.Get("animojiPrefab");
                    if (!string.IsNullOrEmpty(animojiPrefab))
                    {
                        // Set animoji direclty
                        AssignItem("prefab", animojiPrefab, pair.Key + "_animoji", "" );
                    }
                }
                else
                {
                    List<DefinitionNode> specialTierDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SPECIAL_DRAGON_TIERS, "specialDragon", pair.Key);
                    for (int i = 0; i < specialTierDefs.Count; i++)
                    {
                        DefinitionNode def = specialTierDefs[i];

                        string menuPrefab = def.Get("menuPrefab");
                        if (!string.IsNullOrEmpty(menuPrefab) && !local_prefabs.Contains(menuPrefab))
                            local_prefabs.Add(menuPrefab);

                        string gamePrefab = def.Get("gamePrefab");
                        if ( !string.IsNullOrEmpty( gamePrefab ) && !prefabs.Contains(gamePrefab) )
                            prefabs.Add(gamePrefab);
                        
                        string resultsPrefab = def.Get("resultsPrefab");
                        if ( !string.IsNullOrEmpty(resultsPrefab) && !prefabs.Contains(resultsPrefab))
                            prefabs.Add( resultsPrefab );
                    }
                }
                
                List<DefinitionNode> skins = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", pair.Key);
                for (int i = 0; i < skins.Count; i++)
                {
                    bool local = skins[i].GetAsInt("unlockLevel") == 0;
                    string skin = skins[i].Get("skin");
                    string icon = skins[i].Get("icon");
                    if (local)   // Default skin
                    {
                        local_materials.Add(skin + "_body");
                        local_materials.Add(skin + "_wings");
                        local_icons.Add( icon );
                    }
                    else
                    {
                        materials.Add(skin + "_body");
                        materials.Add(skin + "_wings");
                        icons.Add(icon);
                    }

                    string skin_ingame = skins[i].Get("skin") + "_ingame";
                    materials.Add(skin_ingame + "_body");
                    materials.Add(skin_ingame + "_wings");

                    // Search body parts
                    List<string> bodyParts = skins[i].GetAsList<string>("body_parts");
                    for (int j = 0; j < bodyParts.Count; j++)
                    {
                        if (local)
                        {
                            local_prefabs.Add(bodyParts[j]);
                        }
                        else
                        {
                            prefabs.Add(bodyParts[j]);
                        }
                    }
                }


                // Assign prefabs to bundle
                for (int i = 0; i < prefabs.Count; i++) {
                    AssignItem("prefab", prefabs[i], pair.Key, "" );
                }

                for (int i = 0; i < local_prefabs.Count; i++){
                    AssignItem("prefab", local_prefabs[i], pair.Key + "_local", "");
                }

                // Assign materials to bundle
                for (int i = 0; i < materials.Count; i++) {
                    AssignItem("material", materials[i], pair.Key, "");
                    AssignMaterialTextures(materials[i], pair.Key, "");
                }

                for (int i = 0; i < local_materials.Count; i++) {
                    AssignItem("material", local_materials[i], pair.Key + "_local", "");
                    AssignMaterialTextures(local_materials[i], pair.Key + "_local", "");
                }

                for (int i = 0; i < icons.Count; i++){
                    AssignItem("texture", icons[i], pair.Key, "");
                }

                for (int i = 0; i < local_icons.Count; i++){
                    AssignItem("texture", local_icons[i], pair.Key + "_local", "");
                }

            }
            /*
            else
            {
                // Check animoji only
                string animojiPrefab = dragonDef.Get("animojiPrefab");
                if (!string.IsNullOrEmpty(animojiPrefab))
                {
                    // Set animoji direclty
                }
            }
            */
        }
    }

    static void AssignItem( string item_type,  string materialName, string bundleName, string variant)
    {
        string[] guids = AssetDatabase.FindAssets("t:" + item_type + " " + materialName);
        for (int j = 0; j < guids.Length; ++j)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[j]);
            if (Path.GetFileNameWithoutExtension(assetPath) == materialName)  // if it is exactly this one
            {
                AssetImporter ai = AssetImporter.GetAtPath(assetPath);
                ai.SetAssetBundleNameAndVariant(bundleName, variant);
            }
        }
    }

    static void AssignMaterialTextures( string materialName, string bundleName, string variant )
    {
        string[] guids = AssetDatabase.FindAssets("t: material " + materialName);
        for (int j = 0; j < guids.Length; ++j)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[j]);
            if (Path.GetFileNameWithoutExtension(assetPath) == materialName)  // if it is exactly this one
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                Shader shader = mat.shader;
                for(int i=0; i<ShaderUtil.GetPropertyCount(shader); i++) 
                {
                    if(ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv) 
                    {
                        Texture texture = mat.GetTexture(ShaderUtil.GetPropertyName(shader, i));
                        if ( texture != null )
                        {
                            AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
                            ai.SetAssetBundleNameAndVariant(bundleName, variant);
                        }
                    }
                }
            }
        }
    }

     


}
