using SimpleJSON;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This class is responsible for building data required to use Addressables system. Typically these data is composed by:
/// 1)Addressables catalog used by the player, which is generated out of the editor Addressables catalog and it's adapted to use the provider mode specified at build time.
/// 2)Asset bundles, which may be local or remote.
/// 
/// This class can be extended if you need to customize its behaviour.
/// </summary>
public class AddressablesEditorManager
{
    private static Logger sm_logger = new ConsoleLogger("AddressablesEditor");

    private static string STREAMING_ASSETS_ROOT_PATH = FileEditorTools.PathCombine("Assets", "StreamingAssets");

    private const string ADDRESSSABLES_CATALOG_FILENAME = "addressablesCatalog.json";
    private const string ADDRESSABLES_EDITOR_CATALOG_PATH = "Assets/Editor/Addressables/editor_" + ADDRESSSABLES_CATALOG_FILENAME;

    private const string ADDRESSABLES_LOCAL_FOLDER_NAME = "Addressables";    

    private string m_localDestinationPath;
    private string m_editorCatalogFolderParent;    
    private string m_engineCatalogPath;
    private string m_assetBundlesLocalDestinationPath;

    public AddressablesEditorManager()
    {
        m_localDestinationPath = FileEditorTools.PathCombine(STREAMING_ASSETS_ROOT_PATH, ADDRESSABLES_LOCAL_FOLDER_NAME);                
        m_engineCatalogPath = FileEditorTools.PathCombine(m_localDestinationPath, ADDRESSSABLES_CATALOG_FILENAME);
        m_assetBundlesLocalDestinationPath = FileEditorTools.PathCombine(m_localDestinationPath, "AssetBundles");
    }

    public void ClearBuild()
    {        
        FileEditorTools.DeleteFileOrDirectory(m_localDestinationPath);        
    }

    public virtual void CustomizeEditorCatalog()
    {
        Debug.Log("Customize editor catalog");
    }

    public void GenerateEngineCatalog()
    {        
        if (FileEditorTools.Exists(m_localDestinationPath))
        {
            FileEditorTools.DeleteFileOrDirectory(m_localDestinationPath);            
        }

        FileEditorTools.CreateDirectory(m_localDestinationPath);

        BuildCatalog(ADDRESSABLES_EDITOR_CATALOG_PATH, m_engineCatalogPath, AddressablesTypes.EProviderMode.AsCatalog);
    }

    public void BuildAssetBundles()
    {
        AssetBundlesEditorTools.BuildAssetBundles();
    }

    public void DistributeAssetBundles()
    {
        Debug.Log("Distributing build...");        
        AssetBundlesEditorTools.CopyAssetBundles(m_assetBundlesLocalDestinationPath);
    }

    public void Build()
    {
        ClearBuild();
        CustomizeEditorCatalog();
        GenerateEngineCatalog();
        BuildAssetBundles();
        DistributeAssetBundles();
    }

    private void BuildCatalog(string editorCatalogPath, string engineCatalogPath, AddressablesTypes.EProviderMode providerMode)
    {
        // Loads the catalog
        TextAsset textAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(editorCatalogPath, typeof(TextAsset));
        if (textAsset == null)
        {
            sm_logger.LogError("Engine catalog wasn't generated because editor catalog " + editorCatalogPath + " couldn not be loaded");
        }
        else
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();

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
