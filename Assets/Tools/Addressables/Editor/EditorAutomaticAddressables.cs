#if UNITY_EDITOR
#define ALL_BUNDLES_LOCAL

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using SimpleJSON;


public static class EditorAutomaticAddressables {
    private static string[] VARIANTS_PATH = { "/Low/", "/Master/", "/High/", "/VeryHigh/" };
    private static string[] VARIANTS = { "Low", "Master", "High", "VeryHigh" };

    private static string[] AREAS = { "village", "castle", "dark" };


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


    public static JSONClass BuildCatalog() {
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
#if ALL_BUNDLES_LOCAL
                foreach (string bundle in bundleList) {
                    localAssetBundles.Add(bundle);
                }
#else
                foreach (string bundle in bundleList) {
                    if (bundle.Contains(AREAS[0]) || bundle.Contains("shared")) {
                        localAssetBundles.Add(bundle);
                    }
                }
#endif
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
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Entities"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/Particles/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/VFX/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Game/Scenes/Levels/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Resources/Game/Entities/NewEntites/"), true, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Resources/Game/Equipable/items/NPC/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Resources/Game/Projectiles/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Resources/Particles/"), false, entries, bundlesSet);

        _entries = entries;
        _bundles = bundlesSet.ToList();
    }

    private static void GetEntriesPrefab(out List<AddressablesCatalogEntry> _entries, out List<string> _bundles) {
        List<AddressablesCatalogEntry> entries = new List<AddressablesCatalogEntry>();
        HashSet<string> bundlesSet = new HashSet<string>();

        GetEntriesFromDirectory(new DirectoryInfo("Assets/AI"), false, null, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Entities"), false, null, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/Particles/"), false, null, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/VFX/"), false, null, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Game/Scenes/Levels/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Resources/Game/Entities/NewEntites/"), true, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Resources/Game/Equipable/items/NPC/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Resources/Game/Projectiles/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Resources/Particles/"), false, entries, bundlesSet);

        _entries = entries;
        _bundles = bundlesSet.ToList();
    }

    private static void GetEntriesFromDirectory(DirectoryInfo _directory, bool _addLastFolder,  List<AddressablesCatalogEntry> _entries, HashSet<string> _bundles) {
        DirectoryInfo[] directories = _directory.GetDirectories();
        foreach (DirectoryInfo directory in directories) {
            GetEntriesFromDirectory(directory, _addLastFolder, _entries, _bundles);
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
                        string assetName = Path.GetFileNameWithoutExtension(file.Name);
                        string assetPath = Path.GetDirectoryName(filePath);
                        string id = assetName;

                        if (_addLastFolder) {
                            id = assetPath.Substring(assetPath.LastIndexOf('/') + 1) + "/" + id;
                        }

                        string variant = null;
                        for (int i = 0; i < VARIANTS.Length; ++i) {
                            if (filePath.Contains(VARIANTS_PATH[i])) {
                                variant = VARIANTS[i];
                                break;
                            }
                        }

                        AddressablesCatalogEntry entry = new AddressablesCatalogEntry(id, variant, AssetDatabase.AssetPathToGUID(filePath), true) {
                            LocationType = AddressablesTypes.ELocationType.AssetBundles,
                            AssetName = assetName,
                            AssetBundleName = assetBundle
                        };

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