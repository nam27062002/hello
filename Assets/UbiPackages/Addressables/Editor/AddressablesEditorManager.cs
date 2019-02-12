using SimpleJSON;
using System.Collections.Generic;
using System.IO;
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

    public static string REMOTE_ASSETS_FOLDER_NAME = "RemoteAssets";

    public static AddressablesCatalog GetEditorCatalog(string editorCatalogPath)
    {
        return AddressablesManager.GetEditorCatalog(editorCatalogPath);        
    }

    public const string ADDRESSSABLES_CATALOG_FILENAME = AddressablesManager.ADDRESSSABLES_CATALOG_FILENAME;
    public const string ADDRESSABLES_EDITOR_CATALOG_FILENAME = AddressablesManager.ADDRESSABLES_EDITOR_CATALOG_FILENAME;
    private const string ADDRESSABLES_EDITOR_CATALOG_PATH = AddressablesManager.ADDRESSABLES_EDITOR_CATALOG_PATH;

    private const string ADDRESSABLES_LOCAL_FOLDER_NAME = "Addressables";    

    private string m_localDestinationPath;
    private string m_editorCatalogFolderParent;    
    private string m_playerCatalogPath;
    private string m_assetBundlesLocalDestinationPath;

    public AddressablesEditorManager()
    {
        m_localDestinationPath = FileEditorTools.PathCombine(STREAMING_ASSETS_ROOT_PATH, ADDRESSABLES_LOCAL_FOLDER_NAME);                
        m_playerCatalogPath = FileEditorTools.PathCombine(m_localDestinationPath, ADDRESSSABLES_CATALOG_FILENAME);
        m_assetBundlesLocalDestinationPath = FileEditorTools.PathCombine(m_localDestinationPath, "AssetBundles");
    }

    public void ClearBuild()
    {
        Debug.Log("Clearing addressables...");
        FileEditorTools.DeleteFileOrDirectory(m_localDestinationPath);
        FileEditorTools.DeleteFileOrDirectory(AssetBundlesEditorManager.DOWNLOADABLES_FOLDER);
    }

    public virtual void CustomizeEditorCatalog()
    {
        Debug.Log("Customizing editor catalog...");
    }

    public void GeneratePlayerCatalog()
    {
        Debug.Log("Generating player catalog...");

        if (FileEditorTools.Exists(m_localDestinationPath))
        {
            FileEditorTools.DeleteFileOrDirectory(m_localDestinationPath);            
        }

        FileEditorTools.CreateDirectory(m_localDestinationPath);

        BuildCatalog(ADDRESSABLES_EDITOR_CATALOG_PATH, m_playerCatalogPath, AddressablesTypes.EProviderMode.AsCatalog);
    }

    public void BuildAssetBundles()
    {
        AssetBundlesEditorManager.BuildAssetBundles();
    }    

    public void Build()
    {
        ClearBuild();
        CustomizeEditorCatalog();
        GeneratePlayerCatalog();
        BuildAssetBundles();
        ProcessAssetBundles();       
    }  

    private void BuildCatalog(string editorCatalogPath, string playerCatalogPath, AddressablesTypes.EProviderMode providerMode)
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();

        AddressablesCatalog editorCatalog = GetEditorCatalog(editorCatalogPath);
        if (editorCatalog != null)
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();            

            // Copies editor catalog in player catalog
            AddressablesCatalog playerCatalog = new AddressablesCatalog();
            playerCatalog.Load(editorCatalog.ToJSON(), sm_logger);

            // Clears player catalog entries because they'll be added only with the information that player requires
            Dictionary<string, AddressablesCatalogEntry> playerCatalogEntries = playerCatalog.GetEntries();
            playerCatalogEntries.Clear();

            // Loops through all entries to configure actual assets according to their location and provider mode
            Dictionary<string, AddressablesCatalogEntry> entries = editorCatalog.GetEntries();
            List<string> scenesToAdd = new List<string>();
            List<string> scenesToRemove = new List<string>();
            foreach (KeyValuePair<string, AddressablesCatalogEntry> pair in entries)
            {                
                if (ProcessEntry(pair.Value, scenesToAdd, scenesToRemove))
                {
                    playerCatalog.GetEntries().Add(pair.Key, pair.Value);
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
                    scenes[i].enabled = !scenesToRemove.Contains(scenePath);                    
                    
                    newSceneList.Add(scenes[i]);                                            
                    
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
            
            JSONClass json = playerCatalog.ToJSON();            
            FileEditorTools.WriteToFile(playerCatalogPath, json.ToString());            
        }
    }

    /// <summary>
    /// Processes asset bundles along with the addressables catalog in order to determine which bundles are local and which ones are remote.
    /// </summary>
    public void ProcessAssetBundles()
    {
        Debug.Log("Processing asset bundles...");

        AddressablesCatalog catalog = GetEditorCatalog(m_playerCatalogPath);

        AssetBundle manifestBundle = null;
        AssetBundleManifest abManifest = AssetBundlesEditorManager.LoadAssetBundleManifest(out manifestBundle);
        
        ParseAssetBundlesOutput output = AddressablesEditorManager.ParseAssetBundles(catalog, abManifest);

        if (manifestBundle != null)
        {
            manifestBundle.Unload(true);
        }        

        if (catalog != null)
        {
            if (output.m_ABInManifestNotUsed != null && output.m_ABInManifestNotUsed.Count > 0)
            {
                sm_logger.LogWarning("The following asset bundles have been generated but they are not used by addressables: " + UbiListUtils.GetListAsString(output.m_ABInManifestNotUsed));
            }

            if (output.m_LocalABNotUsedList != null && output.m_LocalABNotUsedList.Count > 0)
            {
                sm_logger.LogWarning("The following asset bundles are defined as local in <" + ADDRESSABLES_EDITOR_CATALOG_FILENAME + 
                                     "> but no entries use them. Consider delete them from the field <" + 
                                     AddressablesCatalog.CATALOG_ATT_LOCAL_AB_LIST + ">: " + UbiListUtils.GetListAsString(output.m_LocalABNotUsedList));
            }

            if (output.m_ABInUsedNotInManifest != null && output.m_ABInUsedNotInManifest.Count > 0)
            {
                sm_logger.LogError("The following asset bundles are used by some entries in <" + ADDRESSABLES_EDITOR_CATALOG_FILENAME + 
                                    "> but they haven't been generated: " + UbiListUtils.GetListAsString(output.m_ABInUsedNotInManifest));
            }

            // Updates local AB list
            catalog.SetLocalABList(output.m_LocalABList);            

            // Copy local asset bundles
            AssetBundlesEditorManager.CopyAssetBundles(m_assetBundlesLocalDestinationPath, output.m_LocalABList);

            // Copy manifest
            AssetBundlesEditorManager.CopyAssetBundlesManifest(m_assetBundlesLocalDestinationPath);

            // Copy remote asset bundles
            AssetBundlesEditorManager.CopyAssetBundles(AssetBundlesEditorManager.DOWNLOADABLES_FOLDER, output.m_RemoteABList);

            // Generates remote AB list file
            GenerateDownloadablesCatalog(output.m_RemoteABList, m_playerCatalogPath);            

            // Copy player catalog into the player's folder
            JSONClass json = catalog.ToJSON();
            FileEditorTools.WriteToFile(m_playerCatalogPath, json.ToString());
        }
    }    

    public void GenerateDownloadablesCatalog(List<string> fileNames, string playerFolder)
    {
        AssetBundlesEditorManager.GenerateDownloadablesCatalog(fileNames, playerFolder);
    }     

    private bool ProcessEntry(AddressablesCatalogEntry entry, List<string> scenesToAdd, List<string> scenesToRemove)
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
                string fileName = FileEditorTools.GetFileName(path, false);
                entry.AssetName = fileName;

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

    public class ParseAssetBundlesOutput
    {
        public List<string> m_LocalABNotUsedList = new List<string>();
        public List<string> m_ABInUsedNotInManifest = new List<string>();
        public List<string> m_ABInManifestNotUsed = new List<string>();
        public List<string> m_LocalABList = new List<string>();
        public List<string> m_RemoteABList = new List<string>();  
        
        public void Reset()
        {
            m_LocalABNotUsedList.Clear();
            m_ABInUsedNotInManifest.Clear();
            m_ABInManifestNotUsed.Clear();
            m_LocalABList.Clear();
            m_RemoteABList.Clear();
        }
    }

    public static ParseAssetBundlesOutput ParseAssetBundles(AddressablesCatalog catalog, AssetBundleManifest abManifest)
    {
        ParseAssetBundlesOutput returnValue = new ParseAssetBundlesOutput();

        List<string> catalogLocalABs;
        List<string> catalogUsedABs;
        if (catalog == null)
        {
            catalogLocalABs = new List<string>();
            catalogUsedABs = new List<string>();
        }
        else
        {
            catalogLocalABs = catalog.GetLocalABList();
            catalogUsedABs = catalog.GetUsedABList();
        }

        // Searches for local ABs that are not used
        // We ignore the local ones that are not defined in used because they're not really neccessary
        UbiListUtils.SplitIntersectionAndDisjoint(catalogLocalABs, catalogUsedABs, out catalogLocalABs, out returnValue.m_LocalABNotUsedList);

        if (abManifest == null)
        {
            UbiListUtils.AddRange(returnValue.m_ABInUsedNotInManifest, catalogUsedABs, false, true);
            returnValue.m_LocalABList.Clear();
        }
        else
        {
            List<string> catalogRemoteABs;
            UbiListUtils.SplitIntersectionAndDisjoint(catalogUsedABs, catalogLocalABs, out catalogLocalABs, out catalogRemoteABs);

            List<string> manifestABs = new List<string>(abManifest.GetAllAssetBundles());

            // Loops through all local asset bundles to add them and their dependencies if they're in the
            // manifest or to add them to returnValue.m_ABInUsedNotInManifest if they're not.            
            string abName;            
            List<string> dependencies;
            int count = catalogLocalABs.Count;
            for (int i = 0; i < count; i++)
            {
                abName = catalogLocalABs[i];
                if (manifestABs.Contains(abName))
                {
                    returnValue.m_LocalABList.Add(abName);
                    dependencies = new List<string>(abManifest.GetAllDependencies(abName));
                    UbiListUtils.AddRange(returnValue.m_LocalABList, dependencies, false, true);
                }
                else
                {
                    returnValue.m_ABInUsedNotInManifest.Add(abName);
                }
            }

            // Loops through all local asset bundles to add them and their dependencies if they're in the
            // manifest or to add them to returnValue.m_ABInUsedNotInManifest if they're not.
            if (catalogRemoteABs != null)
            {
                List<string> remoteDependencies;
                List<string> localDependencies;
                count = catalogRemoteABs.Count;
                for (int i = 0; i < count; i++)
                {
                    abName = catalogRemoteABs[i];
                    if (manifestABs.Contains(abName))
                    {
                        returnValue.m_RemoteABList.Add(abName);
                        dependencies = new List<string>(abManifest.GetAllDependencies(abName));
                        UbiListUtils.SplitIntersectionAndDisjoint(dependencies, returnValue.m_LocalABList, out localDependencies, out remoteDependencies);
                        UbiListUtils.AddRange(returnValue.m_RemoteABList, remoteDependencies, false, true);
                    }
                    else
                    {
                        returnValue.m_ABInUsedNotInManifest.Add(abName);
                    }
                }
            }

            if (manifestABs != null)
            {
                count = manifestABs.Count;
                for (int i = 0; i < count; i++)
                {
                    abName = manifestABs[i];
                    if (!returnValue.m_LocalABList.Contains(abName) && !returnValue.m_RemoteABList.Contains(abName))
                    {
                        returnValue.m_ABInManifestNotUsed.Add(abName);
                    }
                }
            }
        }

        return returnValue;
    }    
}
