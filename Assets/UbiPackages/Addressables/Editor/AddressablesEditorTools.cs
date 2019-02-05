using SimpleJSON;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AddressablesEditorTools
{
    private static Logger sm_logger = new ConsoleLogger("AddressablesEditor");
    
    public static void Build(string editorCatalogPath, string engineCatalogPath, AddressablesTypes.EProviderMode providerMode)
    {
        // Loads the catalog
        TextAsset textAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(editorCatalogPath, typeof(TextAsset));
        if (textAsset == null)
        {
            sm_logger.LogError("Engine catalog wasn't generated because editor catalog " + editorCatalogPath + " couldn not be loaded");
        }
        else
        { 
            JSONNode catalogJSON = JSON.Parse(textAsset.text);
            AddressablesCatalog editorCatalog = new AddressablesCatalog();
            editorCatalog.Load(catalogJSON, sm_logger);

            AddressablesCatalog engineCatalog = new AddressablesCatalog();            

            // Loops through all entries to configure actual assets according to their location and provider mode
            Dictionary<string, AddressablesCatalogEntry> entries = editorCatalog.GetEntries();
            List<string> scenesToAdd = new List<string>();
            List<string> scenesToRemove = new List<string>();
            foreach (KeyValuePair<string, AddressablesCatalogEntry> pair in entries)
            {                
                if (ProcessEntry(pair.Value, scenesToAdd, scenesToRemove))
                {
                    engineCatalog.GetEntries().Add(pair.Key, pair.Value);
                }
            }

            if (scenesToAdd.Count > 0 || scenesToRemove.Count > 0)
            {
                List<EditorBuildSettingsScene> newSceneList = new List<EditorBuildSettingsScene>();

                EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
                int count = scenes.Length;
                string scenePath;
                for (int i = 0; i < count; i++)
                {
                    scenePath = scenes[i].path;
                    if (!scenesToRemove.Contains(scenePath))
                    {
                        newSceneList.Add(scenes[i]);                        
                    }

                    if (scenesToAdd.Contains(scenePath))
                    {
                        scenesToAdd.Remove(scenePath);
                    }
                }                

                if (scenesToAdd.Count > 0)
                {
                    count = scenesToAdd.Count;                    
                    for (int i = 0; i < count; i++)
                    {                                                 
                        newSceneList.Add(new EditorBuildSettingsScene(scenesToAdd[i], true));                        
                    }
                }

                EditorBuildSettings.scenes = newSceneList.ToArray();
            }
            
            SimpleJSON.JSONClass json = engineCatalog.ToJSON();            
            FileEditorTools.WriteToFile(engineCatalogPath, json.ToString());            
        }
    }

    private static bool ProcessEntry(AddressablesCatalogEntry entry, List<string> scenesToAdd, List<string> scenesToRemove)
    {        
        string path = entry.Path;
        bool success = !string.IsNullOrEmpty(path);

        if (success)
        {
            path = GetPathWithAssets(path);
            AssetImporter assetImporter = AssetImporter.GetAtPath(path);
            if (assetImporter == null)
            {
                sm_logger.LogError("No resource found in " + path);
            }
            else
            {
                string assetBundleNameFromCatalog = "";

                if (entry.LocationType == AddressablesTypes.ELocationType.AssetBundles)
                {
                    assetBundleNameFromCatalog = entry.AssetBundleName;
                }

                if (assetImporter.assetBundleName != assetBundleNameFromCatalog)
                {
                    assetImporter.assetBundleName = assetBundleNameFromCatalog;
                    assetImporter.SaveAndReimport();
                }

                if (IsScene(path))
                {
                    List<string> list = (entry.LocationType == AddressablesTypes.ELocationType.Resources) ? scenesToAdd : scenesToRemove;
                    list.Add(GetPathWithAssets(entry.Path));
                }
            }
        }

        return success;
    }

    private static bool IsScene(string path)
    {
        return !string.IsNullOrEmpty(path) && path.IndexOf(".unity") > -1;
    }

    private static string GetPathWithAssets(string path)
    {
        return FileEditorTools.PathCombine("Assets", path);
    }
}
