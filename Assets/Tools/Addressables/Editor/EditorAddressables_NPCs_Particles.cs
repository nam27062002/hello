#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

public static class EditorAddressables_NPCs_Particles {
    public static void GetEntriesAll(out List<AddressablesCatalogEntry> _entries, out List<string> _bundles) {
        List<AddressablesCatalogEntry> entries = new List<AddressablesCatalogEntry>();
        HashSet<string> bundlesSet = new HashSet<string>();

        GetEntriesFromDirectory(new DirectoryInfo("Assets/AI"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Entities"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Resources/Game/Entities/NewEntites/"), true, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Resources/Game/Equipable/items/NPC/"), false, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Resources/Particles/"), false, entries, bundlesSet);

        _entries = entries;
        _bundles = bundlesSet.ToList();
    }

    public static void GetEntriesPrefab(out List<AddressablesCatalogEntry> _entries, out List<string> _bundles) {
        List<AddressablesCatalogEntry> entries = new List<AddressablesCatalogEntry>();
        HashSet<string> bundlesSet = new HashSet<string>();

        GetEntriesFromDirectory(new DirectoryInfo("Assets/AI"), false, null, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Art/3D/Gameplay/Entities"), false, null, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Resources/Game/Entities/NewEntites/"), true, entries, bundlesSet);
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Resources/Game/Equipable/items/NPC/"), false, entries, bundlesSet);
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

                        AddressablesCatalogEntry entry = new AddressablesCatalogEntry(id, AssetDatabase.AssetPathToGUID(filePath), true) {
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