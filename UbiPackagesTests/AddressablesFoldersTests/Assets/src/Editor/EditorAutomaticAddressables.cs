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

    private static EditorAssetBundlesConfig GetEditorAssetBundlesConfig()
    {
        EditorAssetBundlesConfig returnValue = new EditorAssetBundlesConfig();

        returnValue.SetAssetBundleLocation("ab_assets_castle", EditorAssetBundlesConfigEntry.ELocation.Local, true);
        returnValue.SetAssetBundleLocation("ab_assets_village", EditorAssetBundlesConfigEntry.ELocation.Local, true);
        returnValue.SetAssetBundleLocation("ab_assets_dark", EditorAssetBundlesConfigEntry.ELocation.Local, true);

        return returnValue;
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
            catalog.Add(AddressablesCatalog.CATALOG_ATT_AB_CONFIG, GetEditorAssetBundlesConfig().ToJSON());

            /*JSONArray groups = new JSONArray();
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
            */
        }

        UnityEngine.Debug.Log("Android: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android));
        UnityEngine.Debug.Log("iOS: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS));
        return catalog;
    }
   
    private static void GetEntriesPrefab(out List<AddressablesCatalogEntry> _entries, out List<string> _bundles) {
        List<AddressablesCatalogEntry> entries = new List<AddressablesCatalogEntry>();
        HashSet<string> bundlesSet = new HashSet<string>();

        bool addToCatalog = true;
#if USE_CHINA
        //addToCatalog = false;
#endif

        GetEntriesFromDirectory(new DirectoryInfo("Assets/AssetCatalog/Level/Medieval/Village/Instantiables"), false, entries, bundlesSet, null, BuildTarget.NoTarget, AddressablesTypes.ELocationType.AssetBundles, null, addToCatalog);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/AssetCatalog/Level/Medieval/Castle/Instantiables"), false, entries, bundlesSet, null, BuildTarget.NoTarget, AddressablesTypes.ELocationType.AssetBundles, null, addToCatalog);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/AssetCatalog/Level/Medieval/Dark/Instantiables"), false, entries, bundlesSet, null, BuildTarget.NoTarget, AddressablesTypes.ELocationType.AssetBundles, "USE_DARK", addToCatalog);

        // China
        GetEntriesFromDirectory(new DirectoryInfo("Assets/AssetCatalog/China/Instantiables"), false, entries, bundlesSet, null, BuildTarget.NoTarget, AddressablesTypes.ELocationType.Resources, "USE_CHINA", addToCatalog);
        
        _entries = entries;
        _bundles = bundlesSet.ToList();
    }

    private static void GetEntriesFromDirectory(DirectoryInfo _directory, bool _addLastFolder,  List<AddressablesCatalogEntry> _entries, HashSet<string> _bundles, System.Type[] _allowedTypes = null, 
                                                BuildTarget platform = BuildTarget.NoTarget, AddressablesTypes.ELocationType locationType = AddressablesTypes.ELocationType.AssetBundles, 
                                                string defineSymbol = null, bool addToCatalogPlayer = true) {
        string platformAsString = (platform == BuildTarget.NoTarget) ? null : platform.ToString();

        DirectoryInfo[] directories = _directory.GetDirectories();
        foreach (DirectoryInfo directory in directories) {
            GetEntriesFromDirectory(directory, _addLastFolder, _entries, _bundles, _allowedTypes, platform, locationType, defineSymbol, addToCatalogPlayer);
        }

        FileInfo[] files = _directory.GetFiles();
        foreach (FileInfo file in files) {
            string filePath = file.FullName;
            string assetsToken = "Assets" + Path.DirectorySeparatorChar;
            filePath = filePath.Substring(filePath.IndexOf(assetsToken, System.StringComparison.Ordinal));


            AssetImporter ai = AssetImporter.GetAtPath(filePath);
            if (ai != null) {
                string assetBundle = ai.assetBundleName;
                if (!string.IsNullOrEmpty(assetBundle) || locationType == AddressablesTypes.ELocationType.Resources) {
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

                        AddressablesCatalogEntry entry = new AddressablesCatalogEntry(id, variant, AssetDatabase.AssetPathToGUID(filePath), true, true, defineSymbol, addToCatalogPlayer) {
                            LocationType = locationType,
                            AssetName = assetName,                            
                            AssetBundleName = assetBundle,
                            Platform = platformAsString                                                        
                        };



                        //UnityEditor.PrefabType
                        //UnityEditor.SceneAsset
                        _entries.Add(entry);
                    }

                    if (_bundles != null && !string.IsNullOrEmpty(assetBundle)) {
                        _bundles.Add(assetBundle);
                    }
                }
            }
        }
    }
}
#endif