using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


// Output the build size or a failure depending on BuildPlayer.

public class Editor_AB_Menu : MonoBehaviour
{
    private const string PRINT_ENTRIES_MENU = "Tech/AssetBundles/Update Textures Folder";
    private static int asset_count = 0;
    private static int asset_per_bundle = 10;
 
    [MenuItem(PRINT_ENTRIES_MENU)]
    private static void BuildPlayer()
    {
        HashSet<string> bundles = new HashSet<string>();

        asset_count = 0;
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Textures"), ref bundles);

        AssetDatabase.Refresh();
        OnDone(PRINT_ENTRIES_MENU);
    }

    private static void GetEntriesFromDirectory(DirectoryInfo _directory, ref HashSet<string> _bundles) {
        DirectoryInfo[] directories = _directory.GetDirectories();
        foreach (DirectoryInfo directory in directories) {
            GetEntriesFromDirectory(directory, ref _bundles);
        }

        string output = "";
        FileInfo[] files = _directory.GetFiles();
        foreach (FileInfo file in files) {
            string filePath = file.FullName;
            string assetsToken = "Assets" + Path.DirectorySeparatorChar;
            filePath = filePath.Substring(filePath.IndexOf(assetsToken, System.StringComparison.Ordinal));


            AssetImporter ai = AssetImporter.GetAtPath(filePath);
            if (ai != null) {
                string assetBundle = "";

                System.Type t = AssetDatabase.GetMainAssetTypeAtPath(filePath);
                if (t == typeof(UnityEngine.Material)) {
                    assetBundle = "dragon_objects";
                } else {
                    assetBundle = "dragon_skins_" + (asset_count / asset_per_bundle);
                    asset_count++;
                }

                _bundles.Add(assetBundle);

                string assetName = Path.GetFileNameWithoutExtension(file.Name);
                string assetPath = Path.GetDirectoryName(filePath);
                string id = assetName;

                ai.SetAssetBundleNameAndVariant(assetBundle, "");

                AddressablesCatalogEntry entry = new AddressablesCatalogEntry(id, null, AssetDatabase.AssetPathToGUID(filePath), true) {
                    LocationType = AddressablesTypes.ELocationType.AssetBundles,
                    AssetName = assetName,
                    AssetBundleName = assetBundle
                };

                output += entry.ToJSON().ToString() + ",\n";
            }
        }

        StreamWriter writer = new StreamWriter("Assets/entries.txt", false) {
            AutoFlush = true
        };

        writer.Write("{\"entries\":[");
        writer.Write(output);
        writer.Write("],\"localAssetBundles\":[");
        int i = 0;
        foreach(string bundle in _bundles) {
            if (i > 0) writer.Write(",");
            writer.Write("\"" + bundle + "\"");
            i++;
        }
        writer.Write("],\"groups\":[]}");
        writer.Close();
    }

    private static void OnDone(string taskName)
    {
        AssetDatabase.Refresh();
        Debug.Log(taskName + " done.");
    }    
}

