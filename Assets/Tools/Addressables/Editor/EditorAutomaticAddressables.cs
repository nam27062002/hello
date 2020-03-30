#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;
using UnityEditor;


public static class EditorAutomaticAddressables {
    private static string[] VARIANTS_PATH = { Path.DirectorySeparatorChar + "Low" + Path.DirectorySeparatorChar, Path.DirectorySeparatorChar + "Master" + Path.DirectorySeparatorChar,
                                              Path.DirectorySeparatorChar + "High" + Path.DirectorySeparatorChar, Path.DirectorySeparatorChar + "VeryHigh" + Path.DirectorySeparatorChar};
    private static string[] VARIANTS = { "Low", "Master", "High", "VeryHigh" };

    private static string[] AREAS = { "village", "castle", "dark" };

    private static string[] LOCAL_DRAGONS = { "dragon_baby", "dragon_crocodile", "dragon_reptile" };

    private static Dictionary<FlavourFactory.Setting_EAddressablesVariant, string> s_flavourAddressablesVariantToAssetFolder;

    private static void Flavour_Init()
    {
        if (s_flavourAddressablesVariantToAssetFolder == null)
        {
            s_flavourAddressablesVariantToAssetFolder = new Dictionary<FlavourFactory.Setting_EAddressablesVariant, string>();
            s_flavourAddressablesVariantToAssetFolder.Add(FlavourFactory.Setting_EAddressablesVariant.WW, "");

            // flavourAddressablesVariant, folder name where the assets for flavourSku are stored in
            s_flavourAddressablesVariantToAssetFolder.Add(FlavourFactory.Setting_EAddressablesVariant.CN, "flavour_china");
        }
    }

    public static string FlavourAddressablesVariantToAssetFolder(FlavourFactory.Setting_EAddressablesVariant flavourAddressablesVariant)
    {
        string returnValue = "";

        Flavour_Init();
       
        if (s_flavourAddressablesVariantToAssetFolder.ContainsKey(flavourAddressablesVariant))
        {
            returnValue = s_flavourAddressablesVariantToAssetFolder[flavourAddressablesVariant];
        }
        else
        {
            Debug.LogError("No folder found for flavour addressables variant: " + flavourAddressablesVariant + " so " + flavourAddressablesVariant + " is used as asset folder");
            returnValue = FlavourFactory.Setting_EAddressablesVariantToString(flavourAddressablesVariant);                
        }

        return returnValue;
    }

    public static JSONClass BuildRestoreCatalog() {
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

            //catalog.Add(AddressablesCatalog.CATALOG_ATT_AB_CONFIG, GetEditorAssetBundlesConfig().ToJSON());

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


    private static void GetEntriesPrefab(out List<AddressablesCatalogEntry> entryList, out List<string> bundleList) {
        entryList = new List<AddressablesCatalogEntry>();
        bundleList = new List<string>();

        Flavour_Init();

        foreach (KeyValuePair<FlavourFactory.Setting_EAddressablesVariant, string> pair in s_flavourAddressablesVariantToAssetFolder)
        {
            GetFlavourEntriesPrefab(pair.Key, ref entryList, ref bundleList);
        }        
    }    

    private static void GetFlavourEntriesPrefab(FlavourFactory.Setting_EAddressablesVariant flavourSku, ref List<AddressablesCatalogEntry> _entries, ref List<string> _bundles) {
        List<AddressablesCatalogEntry> entries = new List<AddressablesCatalogEntry>();
        HashSet<string> bundlesSet = new HashSet<string>();

        string rootPath = "Assets/";
        rootPath += FlavourAddressablesVariantToAssetFolder(flavourSku) + "/";

        System.Type[] instanciableTypesAnimationControllers = new System.Type[] { typeof(UnityEngine.AnimatorOverrideController),
                                                                                  typeof(UnityEditor.Animations.AnimatorController)};
        GetEntriesFromDirectory(flavourSku, rootPath + "Art/3D/Gameplay/Dragons/", false, entries, bundlesSet, instanciableTypesAnimationControllers);        

        System.Type[] instanciableTypes = new System.Type[] { typeof(UnityEngine.GameObject), typeof(UnityEditor.SceneAsset) };
        GetEntriesFromDirectory(flavourSku, rootPath + "AI", false, null, bundlesSet);
        GetEntriesFromDirectory(flavourSku, rootPath + "Art/3D/Gameplay/Entities", false, null, bundlesSet);
        GetEntriesFromDirectory(flavourSku, rootPath + "Art/3D/Gameplay/Entities/PrefabsMenu/", false, entries, bundlesSet, instanciableTypes);
        GetEntriesFromDirectory(flavourSku, rootPath + "Art/Particles/", false, null, bundlesSet);
        GetEntriesFromDirectory(flavourSku, rootPath + "Art/VFX/", false, null, bundlesSet);
        GetEntriesFromDirectory(flavourSku, rootPath + "Game/Scenes/Levels/", false, entries, bundlesSet, instanciableTypes);
        GetEntriesFromDirectory(flavourSku, rootPath + IEntity.ENTITY_PREFABS_PATH, true, entries, bundlesSet, instanciableTypes);
        GetEntriesFromDirectory(flavourSku, rootPath + "Art/3D/Gameplay/Blockers/Prefabs/", false, entries, bundlesSet);
        GetEntriesFromDirectory(flavourSku, rootPath + "Art/3D/Gameplay/Equipable/items/NPC/", false, entries, bundlesSet, instanciableTypes);
        GetEntriesFromDirectory(flavourSku, rootPath + "Art/3D/Gameplay/Particles/", false, entries, bundlesSet, instanciableTypes);
        GetEntriesFromDirectory(flavourSku, rootPath + "Art/3D/Gameplay/Projectiles/", false, entries, bundlesSet, instanciableTypes);

        // Add all dragon bundles to bundleSet and (prefabs and materials) to entries
        System.Type[] instanciableTypesAndMaterials = new System.Type[] { typeof(UnityEngine.GameObject), typeof(UnityEditor.SceneAsset), typeof( UnityEngine.Material ) };
        GetEntriesFromDirectory(flavourSku, rootPath + "Art/3D/Gameplay/Dragons/Prefabs/", false, entries, bundlesSet, instanciableTypes);
        GetEntriesFromDirectory(flavourSku, rootPath + "Art/3D/Gameplay/Dragons/Skins/", false, entries, bundlesSet, instanciableTypesAndMaterials);
        GetEntriesFromDirectory(flavourSku, rootPath + "Art/3D/Gameplay/Dragons/Items/", false, entries, bundlesSet, instanciableTypes);

           // Disguise icons
        System.Type[] instanciableTypesTextures = new System.Type[] { typeof(UnityEngine.Texture2D) };
        GetEntriesFromDirectory(flavourSku, rootPath + "Art/UI/Metagame/Dragons/Disguises/", false, entries, bundlesSet, instanciableTypesTextures);

        // Add all pets bundles
        GetEntriesFromDirectory(flavourSku, rootPath + "Art/3D/Gameplay/Pets/Prefabs/", false, entries, bundlesSet, instanciableTypes);
        GetEntriesFromDirectory(flavourSku, rootPath + "Art/UI/Metagame/Pets/", false, entries, bundlesSet, instanciableTypesTextures);

        // AR (only for iOS)
        GetEntriesFromDirectory(flavourSku, rootPath + "PlatformResources/iOS/AR/Animojis/", false, entries, bundlesSet, instanciableTypes, BuildTarget.iOS);

        // You can also add assets that are supposed to be loaded from Resources
        // Example: This added all assets in Assets/Art/R folder and its subfolders. Since we pass false to _addLastFolder directory the name of the asset will be used as id in Addressables catalog.
        //GetEntriesFromDirectory("Assets/Art/R", false, entries, bundlesSet, instanciableTypes, BuildTarget.NoTarget, AddressablesTypes.ELocationType.Resources);

        UbiListUtils.AddRange(_entries, entries, false, true);
        _bundles.AddRange(bundlesSet);        
    }

    private static void GetEntriesFromDirectory(FlavourFactory.Setting_EAddressablesVariant flavourAddressablesVariant, string _directoryPath, bool _addLastFolder, List<AddressablesCatalogEntry> _entries, HashSet<string> _bundles, System.Type[] _allowedTypes = null, BuildTarget platform = BuildTarget.NoTarget,
                                                AddressablesTypes.ELocationType locationType = AddressablesTypes.ELocationType.AssetBundles, string defineSymbol = null, bool addToCatalogPlayer = true)
    {
        if (!Directory.Exists(_directoryPath))
            return;

        DirectoryInfo _directory = new DirectoryInfo(_directoryPath);
        GetEntriesFromDirectory(flavourAddressablesVariant, _directory, _addLastFolder, _entries, _bundles, _allowedTypes, platform, locationType, defineSymbol, addToCatalogPlayer);
    }

    private static void GetEntriesFromDirectory(FlavourFactory.Setting_EAddressablesVariant flavourAddressablesVariant, DirectoryInfo _directory, bool _addLastFolder,  List<AddressablesCatalogEntry> _entries, HashSet<string> _bundles, System.Type[] _allowedTypes = null, BuildTarget platform = BuildTarget.NoTarget,
                                                AddressablesTypes.ELocationType locationType = AddressablesTypes.ELocationType.AssetBundles, string defineSymbol = null, bool addToCatalogPlayer = true) {       
        string platformAsString = (platform == BuildTarget.NoTarget) ? null : platform.ToString();

        DirectoryInfo[] directories = _directory.GetDirectories();
        foreach (DirectoryInfo directory in directories) {
            GetEntriesFromDirectory(flavourAddressablesVariant, directory, _addLastFolder, _entries, _bundles, _allowedTypes, platform, locationType, defineSymbol, addToCatalogPlayer);
        }

        FileInfo[] files = _directory.GetFiles();
        foreach (FileInfo file in files) {
            string filePath = file.FullName;
            string assetsToken = "Assets" + Path.DirectorySeparatorChar;
            filePath = filePath.Substring(filePath.IndexOf(assetsToken, System.StringComparison.Ordinal));


            AssetImporter ai = AssetImporter.GetAtPath(filePath);
            if (ai != null) {
                string assetBundle = ai.assetBundleName;
                if (locationType == AddressablesTypes.ELocationType.Resources || locationType == AddressablesTypes.ELocationType.AssetBundles && !string.IsNullOrEmpty(assetBundle)) {                 
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
                        string assetName = FileUtils.GetPathWithoutExtension(file.Name);
                        string assetPath = Path.GetDirectoryName(filePath);
                        string id = assetName;

                        if (_addLastFolder) {
                            id = assetPath.Substring(assetPath.LastIndexOf(Path.DirectorySeparatorChar) + 1) + "/" + id;
                        }

                        string variant = null;
                        for (int i = 0; i < VARIANTS.Length; ++i) {
                            if (filePath.Contains(VARIANTS_PATH[i])) {
                                variant = "" + VARIANTS[i];
                                break;
                            }
                        }

                        if (variant == null)
                        {
                            variant = FlavourFactory.Setting_EAddressablesVariantToString(flavourAddressablesVariant);
                        }
                        else
                        {
                            variant = FlavourFactory.Setting_EAddressablesVariantToString(flavourAddressablesVariant) + HDAddressablesManager.FLAVOUR_VARIANT_SEPARATOR + variant;
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

                    if (_bundles != null) {
                        _bundles.Add(assetBundle);
                    }
                }
            }
        }
    }
}
#endif