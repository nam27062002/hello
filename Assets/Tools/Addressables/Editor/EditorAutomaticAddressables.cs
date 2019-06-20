#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using SimpleJSON;


public static class EditorAutomaticAddressables {
    private static string[] VARIANTS_PATH = { Path.DirectorySeparatorChar + "Low" + Path.DirectorySeparatorChar, Path.DirectorySeparatorChar + "Master" + Path.DirectorySeparatorChar,
                                              Path.DirectorySeparatorChar + "High" + Path.DirectorySeparatorChar, Path.DirectorySeparatorChar + "VeryHigh" + Path.DirectorySeparatorChar};
    private static string[] VARIANTS = { "Low", "Master", "High", "VeryHigh" };

    private static string[] AREAS = { "village", "castle", "dark" };

    private static string[] LOCAL_DRAGONS = { "dragon_baby", "dragon_crocodile", "dragon_reptile" };


    public static JSONClass BuildRestoreCatalog() {
        List<AddressablesCatalogEntry> entryList = new List<AddressablesCatalogEntry>();
        GetEntriesFromDirectory(new DirectoryInfo("Assets"), false, entryList, null);

        JSONClass catalog = new JSONClass();
        {
            JSONArray entries = new JSONArray();
            {
                foreach (AddressablesCatalogEntry entry in entryList) {
                    entries.Add(entry.ToJSON());
                }
            }
            catalog.Add("entries", entries);
        }

        return catalog;
    }


    public static JSONClass BuildCatalog(bool _allBundlesLocal) {
        List<AddressablesCatalogEntry> entryList;
        List<string> bundleList;

        GetEntriesPrefab(out entryList, out bundleList);

        JSONClass catalog = new JSONClass();
        {
            JSONArray entries = new JSONArray();
            {
                foreach (AddressablesCatalogEntry entry in entryList) {
                    entries.Add(entry.ToJSON());
                }
            }
            catalog.Add("entries", entries);


            JSONArray localAssetBundles = new JSONArray();
            {
                if (_allBundlesLocal) {
                    foreach (string bundle in bundleList) {
                        localAssetBundles.Add(bundle);
                    }
                } else {
                    foreach (string bundle in bundleList) {
                        if (bundle.Contains(AREAS[0]) || bundle.Contains("shared") || bundle.EndsWith("_local", System.StringComparison.InvariantCulture)) {
                            localAssetBundles.Add(bundle);
                        }
                        else
                        {
                            for (int i = 0; i < LOCAL_DRAGONS.Length; i++)
                            {
                                if ( bundle.Contains( LOCAL_DRAGONS[i]) && !bundle.Contains( "animoji" )) 
                                {
                                    localAssetBundles.Add(bundle);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catalog.Add("localAssetBundles", localAssetBundles);


            JSONArray groups = new JSONArray();
            {
                for (int i = 0; i < 3; ++i) {
                    JSONClass group = new JSONClass();
                    {
                        JSONArray bundlesInArea = new JSONArray();
                        foreach (string bundle in bundleList) {
                            if (bundle.Contains(AREAS[i]) || bundle.Contains("shared")) {
                                bundlesInArea.Add(bundle);
                            }
                        }
                        group.Add("id", "area" + (i + 1));
                        group.Add("assetBundles", bundlesInArea);
                    }
                    groups.Add(group);
                }

            }
            catalog.Add("groups", groups);
            
            
            
            
        }

        return catalog;
    }




    private static void GetEntriesAll(out List<AddressablesCatalogEntry> _entries, out List<string> _bundles) {
        List<AddressablesCatalogEntry> entries = new List<AddressablesCatalogEntry>();
        HashSet<string> bundlesSet = new HashSet<string>();

        GetEntriesFromDirectory(new DirectoryInfo("Assets/AI"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Entities/Assets/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Entities/PrefabsMenu/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/Particles/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/VFX/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Game/Scenes/Levels/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/" + IEntity.ENTITY_PREFABS_PATH), true, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Blockers/Prefabs/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Equipable/items/NPC/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Particles/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Projectiles/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Dragons/Prefabs/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Dragons/Skins/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Dragons/Items/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/UI/Metagame/Dragons/Disguises/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Pets/Prefabs/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/UI/Metagame/Pets/"), false, entries, bundlesSet);

        // AR (only for iOS)
        GetEntriesFromDirectory(new DirectoryInfo("Assets/PlatformResources/iOS/AR/Animojis/"), false, entries, bundlesSet, null, BuildTarget.iOS);

        _entries = entries;
        _bundles = bundlesSet.ToList();
    }

    private static void GetEntriesPrefab(out List<AddressablesCatalogEntry> _entries, out List<string> _bundles) {
        List<AddressablesCatalogEntry> entries = new List<AddressablesCatalogEntry>();
        HashSet<string> bundlesSet = new HashSet<string>();

        System.Type[] instanciableTypes = new System.Type[] { typeof(UnityEngine.GameObject), typeof(UnityEditor.SceneAsset) };
        GetEntriesFromDirectory(new DirectoryInfo("Assets/AI"), false, null, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Entities"), false, null, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Entities/PrefabsMenu/"), false, entries, bundlesSet, instanciableTypes);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/Particles/"), false, null, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/VFX/"), false, null, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Game/Scenes/Levels/"), false, entries, bundlesSet, instanciableTypes);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/" + IEntity.ENTITY_PREFABS_PATH), true, entries, bundlesSet, instanciableTypes);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Blockers/Prefabs/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Equipable/items/NPC/"), false, entries, bundlesSet, instanciableTypes);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Particles/"), false, entries, bundlesSet, instanciableTypes);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Projectiles/"), false, entries, bundlesSet, instanciableTypes);

        // Add all dragon bundles to bundleSet and (prefabs and materials) to entries
        System.Type[] instanciableTypesAndMaterials = new System.Type[] { typeof(UnityEngine.GameObject), typeof(UnityEditor.SceneAsset), typeof( UnityEngine.Material ) };
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Dragons/Prefabs/"), false, entries, bundlesSet, instanciableTypes);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Dragons/Skins/"), false, entries, bundlesSet, instanciableTypesAndMaterials);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Dragons/Items/"), false, entries, bundlesSet, instanciableTypes);

        // Disguise icons
        System.Type[] instanciableTypesTextures = new System.Type[] { typeof(UnityEngine.Texture2D) };
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/UI/Metagame/Dragons/Disguises/"), false, entries, bundlesSet, instanciableTypesTextures);

        // Add all pets bundles
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Pets/Prefabs/"), false, entries, bundlesSet, instanciableTypes);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/UI/Metagame/Pets/"), false, entries, bundlesSet, instanciableTypesTextures);

        // AR (only for iOS)
        GetEntriesFromDirectory(new DirectoryInfo("Assets/PlatformResources/iOS/AR/Animojis/"), false, entries, bundlesSet, instanciableTypes, BuildTarget.iOS);

        _entries = entries;
        _bundles = bundlesSet.ToList();
    }

    private static void GetEntriesFromDirectory(DirectoryInfo _directory, bool _addLastFolder,  List<AddressablesCatalogEntry> _entries, HashSet<string> _bundles, System.Type[] _allowedTypes = null, BuildTarget platform = BuildTarget.NoTarget) {
        string platformAsString = (platform == BuildTarget.NoTarget) ? null : platform.ToString();

        DirectoryInfo[] directories = _directory.GetDirectories();
        foreach (DirectoryInfo directory in directories) {
            GetEntriesFromDirectory(directory, _addLastFolder, _entries, _bundles, _allowedTypes);
        }

        FileInfo[] files = _directory.GetFiles();
        foreach (FileInfo file in files) {
            string filePath = file.FullName;
            string assetsToken = "Assets" + Path.DirectorySeparatorChar;
            filePath = filePath.Substring(filePath.IndexOf(assetsToken, System.StringComparison.Ordinal));


            AssetImporter ai = AssetImporter.GetAtPath(filePath);
            if (ai != null) {
                string assetBundle = ai.assetBundleName;
                if (!string.IsNullOrEmpty(assetBundle)) {
                    bool createEntry = false;

                    if (_entries != null) {
                        if (_allowedTypes != null) {
                            System.Type t = AssetDatabase.GetMainAssetTypeAtPath(filePath);
                            createEntry = _allowedTypes.Contains(t);
                        } else {
                            createEntry = true;
                        }
                    }

                    if (createEntry) {
                        string assetName = Path.GetFileNameWithoutExtension(file.Name);
                        string assetPath = Path.GetDirectoryName(filePath);
                        string id = assetName;

                        if (_addLastFolder) {
                            id = assetPath.Substring(assetPath.LastIndexOf(Path.DirectorySeparatorChar) + 1) + "/" + id;
                        }

                        string variant = null;
                        for (int i = 0; i < VARIANTS.Length; ++i) {
                            if (filePath.Contains(VARIANTS_PATH[i])) {
                                variant = VARIANTS[i];
                                break;
                            }
                        }

                        AddressablesCatalogEntry entry = new AddressablesCatalogEntry(id, variant, AssetDatabase.AssetPathToGUID(filePath), true, true) {
                            LocationType = AddressablesTypes.ELocationType.AssetBundles,
                            AssetName = assetName,
                            AssetBundleName = assetBundle,
                            Platform = platformAsString                                                        
                        };



                        //UnityEditor.PrefabType
                        //UnityEditor.SceneAsset
                        _entries.Add(entry);
                    }

                    if (_bundles != null) {
                        _bundles.Add(assetBundle);
                    }
                }
            }
        }
    }
}
#endif