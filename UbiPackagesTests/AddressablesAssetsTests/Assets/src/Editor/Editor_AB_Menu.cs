using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


// Output the build size or a failure depending on BuildPlayer.

public class Editor_AB_Menu : MonoBehaviour
{
    private const string PRINT_ENTRIES_MENU = "Tech/AssetBundles/Update Textures Folder";
    
 
    [MenuItem(PRINT_ENTRIES_MENU)]
    private static void BuildPlayer()
    {
        GetEntriesFromDirectory(new DirectoryInfo("Assets/Textures"));

        AssetDatabase.Refresh();
        OnDone(PRINT_ENTRIES_MENU);
    }

    private static void GetEntriesFromDirectory(DirectoryInfo _directory) {
        DirectoryInfo[] directories = _directory.GetDirectories();
        foreach (DirectoryInfo directory in directories) {
            GetEntriesFromDirectory(directory);
        }


        string output = "";
        FileInfo[] files = _directory.GetFiles();
        foreach (FileInfo file in files) {
            string filePath = file.FullName;
            string assetsToken = "Assets" + Path.DirectorySeparatorChar;
            filePath = filePath.Substring(filePath.IndexOf(assetsToken, System.StringComparison.Ordinal));


            AssetImporter ai = AssetImporter.GetAtPath(filePath);
            if (ai != null) {
                string assetBundle = "dragon_skins";
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

        Debug.Log(output);
    }

    private static void OnDone(string taskName)
    {
        AssetDatabase.Refresh();
        Debug.Log(taskName + " done.");
    }    
}

