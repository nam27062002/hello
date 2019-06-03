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
public class EditorAddressablesManager
{
    private static Logger sm_logger = new ConsoleLogger("AddressablesEditor");

    private static string RESOURCES_ROOT_PATH = EditorFileUtils.PathCombine("Assets", "Resources");

    public static string REMOTE_ASSETS_FOLDER_NAME = "RemoteAssets";

    private static string GENERATED_FOLDER = "Generated";
    private static string RESOURCES_GENERATED_FOLDER = EditorFileUtils.PathCombine("Resources", GENERATED_FOLDER);

    public static AddressablesCatalog GetEditorCatalogFromPath(string path, bool editorMode)
    {
        return AddressablesManager.GetCatalog(path, editorMode);
    }

    public static AddressablesCatalog GetEditorCatalog(bool useGenerated=true)
    {
        return AddressablesManager.GetEditorCatalog(useGenerated);
    }

    public const string ADDRESSSABLES_CATALOG_FILENAME = AddressablesManager.ADDRESSSABLES_CATALOG_FILENAME;
    public const string ADDRESSABLES_EDITOR_CATALOG_FILENAME = AddressablesManager.ADDRESSABLES_EDITOR_CATALOG_FILENAME;
    private const string ADDRESSABLES_EDITOR_CATALOG_PATH = AddressablesManager.ADDRESSABLES_EDITOR_CATALOG_PATH;
    public const string ADDRESSABLES_EDITOR_GENERATED_PATH = AddressablesManager.ADDRESSABLES_EDITOR_GENERATED_PATH;
    public const string ADDRESSABLES_EDITOR_GENERATED_CATALOG_PATH = AddressablesManager.ADDRESSABLES_EDITOR_GENERATED_CATALOG_PATH;

    public static bool ADDRESSABLES_LOCAL_IN_STREAMING_ASSETS = AssetBundlesManager.LOCAL_IN_STREAMING_ASSETS;
    public static string ADDRESSABLES_LOCAL_FOLDER = (ADDRESSABLES_LOCAL_IN_STREAMING_ASSETS) ? "StreamingAssets" : "Resources/Addressables";
    private static string ADDRESSABLES_PLAYER_ASSET_BUNDLES_PATH = "Assets/" + ADDRESSABLES_LOCAL_FOLDER + "/AssetBundles";    

    private const string ADDRESSABLES_LOCAL_FOLDER_NAME = "Addressables";

    private string m_localDestinationPath;
    private string m_editorCatalogFolderParent;    
    private string m_playerCatalogPath;
    private string m_assetBundlesLocalDestinationPath;        

    public EditorAddressablesManager()
    {
        m_localDestinationPath = EditorFileUtils.PathCombine(RESOURCES_ROOT_PATH, ADDRESSABLES_LOCAL_FOLDER_NAME);                
        m_playerCatalogPath = EditorFileUtils.PathCombine(m_localDestinationPath, ADDRESSSABLES_CATALOG_FILENAME);        
		m_assetBundlesLocalDestinationPath = EditorAssetBundlesManager.ASSET_BUNDLES_LOCAL_FOLDER;
    }

    public void ClearBuild(BuildTarget target)
    {
        Debug.Log("Clearing addressables...");
        EditorFileUtils.DeleteFileOrDirectory(m_localDestinationPath);
        EditorFileUtils.DeleteFileOrDirectory(EditorAssetBundlesManager.DOWNLOADABLES_FOLDER + "/" + target.ToString());
        ClearResourcesGenerated();
        EditorFileUtils.DeleteFileOrDirectory(EditorFileUtils.PathCombine(m_assetBundlesLocalDestinationPath, target.ToString()));
        EditorFileUtils.DeleteFileOrDirectory(ADDRESSABLES_PLAYER_ASSET_BUNDLES_PATH);
        EditorFileUtils.DeleteFileOrDirectory(ADDRESSABLES_EDITOR_GENERATED_PATH);
    }

    public void ClearResourcesGenerated()
    {
        string directory = EditorFileUtils.PathCombine("Assets", RESOURCES_GENERATED_FOLDER);
        if (EditorFileUtils.Exists(directory))
        {
            EditorFileUtils.DeleteFileOrDirectory(directory);
        }
    }

    public virtual void CustomizeEditorCatalog()
    {
        Debug.Log("Customizing editor catalog...");
    }

    public void GeneratePlayerCatalog()
    {
        Debug.Log("Generating player catalog...");

        if (EditorFileUtils.Exists(m_playerCatalogPath))
        {
            EditorFileUtils.DeleteFileOrDirectory(m_playerCatalogPath);            
        }
        
        EditorFileUtils.CreateDirectory(m_localDestinationPath);

        BuildCatalog(m_playerCatalogPath, AddressablesTypes.EProviderMode.AsCatalog);
    }

    public void BuildAssetBundles(BuildTarget platform)
    {
        EditorAssetBundlesManager.BuildAssetBundles(platform);
    }    

    public void BuildForTargetPlatform()
    {
        ClearBuild(EditorUserBuildSettings.activeBuildTarget);
        CustomizeEditorCatalog();
        GeneratePlayerCatalog();

        if (AddressablesManager.Mode_NeedsAssetBundles())
        {
            BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget);
            GenerateAssetBundlesCatalog();
            ProcessAssetBundles(EditorUserBuildSettings.activeBuildTarget, true);
        }
    }
    
    protected virtual JSONNode GetExternalAddressablesCatalogJSON()
    {
        return null;
    }

    public AddressablesCatalog GenerateFullEditorCatalog()
    {
        AddressablesCatalog editorCatalog = GetEditorCatalog(false);

        JSONNode externalCatalogJSON = GetExternalAddressablesCatalogJSON();
        if (editorCatalog == null)
        {
            if (externalCatalogJSON != null)
            {
                editorCatalog = new AddressablesCatalog(true);
                editorCatalog.Load(externalCatalogJSON, sm_logger);
            }
        }
        else
        {
            editorCatalog.Join(externalCatalogJSON, sm_logger);
        }

        EditorFileUtils.DeleteFileOrDirectory(ADDRESSABLES_EDITOR_GENERATED_CATALOG_PATH);

        if (editorCatalog != null)
        {
            // Writes the full editor catalog
            if (!Directory.Exists(ADDRESSABLES_EDITOR_GENERATED_PATH))
            {
                Directory.CreateDirectory(ADDRESSABLES_EDITOR_GENERATED_PATH);
            }

            EditorFileUtils.WriteToFile(ADDRESSABLES_EDITOR_GENERATED_CATALOG_PATH, editorCatalog.ToJSON().ToString());
        }

        return editorCatalog;
    }

    private void BuildCatalog(string playerCatalogPath, AddressablesTypes.EProviderMode providerMode)
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();

        ClearResourcesGenerated();

        AddressablesCatalog editorCatalog = GenerateFullEditorCatalog();
        if (editorCatalog != null)
        {                        
            editorCatalog.OptimizeEntriesAssetNames();

            AssetDatabase.RemoveUnusedAssetBundleNames();            

            // Copies editor catalog in player catalog
            AddressablesCatalog playerCatalog = new AddressablesCatalog();
            playerCatalog.Load(editorCatalog.ToJSON(), sm_logger);

            // Clears player catalog entries because they'll be added only with the information that player requires
            playerCatalog.ClearEntries();            

            // Loops through all entries to configure actual assets according to their location and provider mode
            List<AddressablesCatalogEntry> entries = editorCatalog.GetEntries();
            List<string> scenesToAdd = new List<string>();
            List<string> scenesToRemove = new List<string>();
            AddressablesCatalogEntry entry;
            int count = entries.Count;
            for (int i = 0; i < count; i++)
            {                
                if (ProcessEntry(entries[i], scenesToAdd, scenesToRemove))
                {
                    entry = new AddressablesCatalogEntry();
                    entry.Load(entries[i].ToJSON());
                    playerCatalog.AddEntry(entry);
                }
            }            

            if (scenesToAdd.Count > 0 || scenesToRemove.Count > 0)
            {
                List<EditorBuildSettingsScene> newSceneList = new List<EditorBuildSettingsScene>();

                EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
                count = scenes.Length;
                string scenePath;
				bool enabled;
                for (int i = 0; i < count; i++)
                {
                    scenePath = scenes[i].path;
					if (scenesToAdd.Contains(scenePath)) 
					{
						enabled = true;
						scenesToAdd.Remove(scenePath);
					} 
					else if (scenesToRemove.Contains(scenePath)) 
					{
						enabled = false;
					} 
					else 
					{
						enabled = scenes [i].enabled;
					}

					scenes[i].enabled = enabled;                                        
                    newSceneList.Add(scenes[i]);                                                                                   
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
            EditorFileUtils.WriteToFile(playerCatalogPath, json.ToString());            
        }
    }

    public void GenerateAssetBundlesCatalog()
    {
        if (!File.Exists(ADDRESSABLES_EDITOR_CATALOG_PATH))
        {
            Debug.LogError(ADDRESSABLES_EDITOR_CATALOG_PATH + " not found");
            return;
        }

		string path = Path.Combine(m_localDestinationPath, AssetBundlesManager.ASSET_BUNDLES_CATALOG_FILENAME); 		       
        if (AddressablesManager.EffectiveMode == AddressablesManager.EMode.AllInResources)
        {
            // No catalog is required
			if (File.Exists(path)) 
			{
				EditorFileUtils.DeleteFileOrDirectory(path);
			}
        }
        else
        {
            AssetBundle manifestBundle = null;
            AssetBundleManifest abManifest = EditorAssetBundlesManager.LoadAssetBundleManifest(out manifestBundle);

            if (abManifest == null)
            {
                Debug.LogError("No asset bundle manifest found. You might need to build asset bundles first.");

                if (manifestBundle != null)
                {
                    manifestBundle.Unload(true);
                }

                return;
            }

            AddressablesCatalog editorCatalog = GetEditorCatalog(true);
            ParseAssetBundlesOutput output = ParseAssetBundles(editorCatalog, abManifest);

            List<string> abList = UbiListUtils.AddRange(output.m_LocalABList, output.m_RemoteABList, true, true);

            AssetBundlesCatalog abCatalog = new AssetBundlesCatalog();

            List<string> dependencies;
            int count = abList.Count;
            for (int i = 0; i < count; i++)
            {
                dependencies = new List<string>(abManifest.GetDirectDependencies(abList[i]));
                abCatalog.AddDirectDependencies(abList[i], dependencies);
            }

            abCatalog.SetGroups(editorCatalog.GetGroups());
            abCatalog.SetExplicitLocalAssetBundlesList(editorCatalog.GetLocalABList());

			JSONNode json = abCatalog.ToJSON();
			EditorFileUtils.CreateDirectory(m_localDestinationPath);
			EditorFileUtils.WriteToFile(path, json.ToString());     

            if (manifestBundle != null)
            {
                manifestBundle.Unload(true);
            }
        }			                   
    }   

    /// <summary>
    /// Processes asset bundles along with the addressables catalog in order to determine which bundles are local and which ones are remote.
    /// </summary>
    public void ProcessAssetBundles(BuildTarget target, bool copyToPlayer)
    {
        Debug.Log("Processing asset bundles...");

        AddressablesCatalog catalog = GetEditorCatalog(true);

        AssetBundle manifestBundle = null;
        AssetBundleManifest abManifest = EditorAssetBundlesManager.LoadAssetBundleManifest(out manifestBundle);
        
        ParseAssetBundlesOutput output = ParseAssetBundles(catalog, abManifest);

        if (manifestBundle != null)
        {
            manifestBundle.Unload(true);
        }        

        if (catalog != null)
        {
            if (output.m_ABInManifestNotUsed != null && output.m_ABInManifestNotUsed.Count > 0)
            {
                sm_logger.LogError("The following asset bundles have been generated but they are not used by addressables: " + UbiListUtils.GetListAsString(output.m_ABInManifestNotUsed));
            }

            if (output.m_LocalABNotUsedList != null && output.m_LocalABNotUsedList.Count > 0)
            {
                sm_logger.LogError("The following asset bundles are defined as local in <" + ADDRESSABLES_EDITOR_CATALOG_FILENAME + 
                                     "> but no entries use them. Consider delete them from the field <" + 
                                     AddressablesCatalog.CATALOG_ATT_LOCAL_AB_LIST + ">: " + UbiListUtils.GetListAsString(output.m_LocalABNotUsedList));
            }

            if (output.m_ABInUsedNotInManifest != null && output.m_ABInUsedNotInManifest.Count > 0)
            {
                sm_logger.LogError("The following asset bundles are used by some entries in <" + ADDRESSABLES_EDITOR_CATALOG_FILENAME + 
                                    "> but they haven't been generated: " + UbiListUtils.GetListAsString(output.m_ABInUsedNotInManifest));
            }
            
            if (copyToPlayer)
            {
                string localAssetBundlesPath = EditorFileUtils.PathCombine(m_assetBundlesLocalDestinationPath, target.ToString());
                if (EditorFileUtils.Exists(localAssetBundlesPath))
                {
                    EditorFileUtils.DeleteFileOrDirectory(localAssetBundlesPath);
                }

                EditorFileUtils.CreateDirectory(localAssetBundlesPath);

                // Copy local asset bundles
                EditorAssetBundlesManager.CopyAssetBundles(localAssetBundlesPath, output.m_LocalABList);

                // Copy local asset bundles to the player folder so they can be used by the player
                if (AddressablesManager.Mode_NeedsAssetBundles())
                {                    
                    EditorFileUtils.DeleteFileOrDirectory(ADDRESSABLES_PLAYER_ASSET_BUNDLES_PATH);
                    EditorFileUtils.CreateDirectory(localAssetBundlesPath);
                    EditorAssetBundlesManager.CopyAssetBundles(ADDRESSABLES_PLAYER_ASSET_BUNDLES_PATH, output.m_LocalABList, !ADDRESSABLES_LOCAL_IN_STREAMING_ASSETS);
                }

                // Deletes original files that were moved to local
                EditorAssetBundlesManager.DeleteAssetBundles(output.m_LocalABList);
            }

            string remoteFolder = EditorAssetBundlesManager.DOWNLOADABLES_FOLDER + "/" + target.ToString();
            if (AddressablesManager.EffectiveMode == AddressablesManager.EMode.AllInLocalAssetBundles)
            {
                if (EditorFileUtils.Exists(remoteFolder))
                {
                    EditorFileUtils.DeleteFileOrDirectory(remoteFolder);
                }
            }
            else
            {
                // Copy remote asset bundles                
                EditorAssetBundlesManager.CopyAssetBundles(remoteFolder, output.m_RemoteABList);

                // Not used asset bundles are stored anyway just in case they haven't been defined in catalog but they are used
                if (output.m_ABInManifestNotUsed != null && output.m_ABInManifestNotUsed.Count > 0)
                {
                    //EditorAssetBundlesManager.CopyAssetBundles(remoteFolder, output.m_ABInManifestNotUsed);
                }

                // Generates remote AB list file            
                string downloadablesCatalogFolder = EditorAssetBundlesManager.GetPlatformDirectory(EditorAssetBundlesManager.ASSET_BUNDLES_PATH);
                GenerateDownloadablesCatalog(output.m_RemoteABList, downloadablesCatalogFolder);

                // Deletes original files that were moved to local
                EditorAssetBundlesManager.DeleteAssetBundles(output.m_RemoteABList);

                GenerateDownloadablesConfig(m_localDestinationPath);
            }
        }
    }    

    public void CopyLocalAndRemoteAssetBundlesToSource(BuildTarget target)
    {        
        string sourcePath = EditorAssetBundlesManager.GetAssetBundlesDirectory();        

        if (Directory.Exists(sourcePath))
        {
            string targetAsString = target.ToString();
            string localAssetBundlesPath = EditorFileUtils.PathCombine(m_assetBundlesLocalDestinationPath, targetAsString);
            EditorFileUtils.CopyFilesInDirectory(localAssetBundlesPath, sourcePath);

            string remoteAssetBundlesPath = EditorFileUtils.PathCombine(EditorAssetBundlesManager.DOWNLOADABLES_FOLDER, targetAsString);
            EditorFileUtils.CopyFilesInDirectory(remoteAssetBundlesPath, sourcePath);
        }        
    }
    
    public void DeleteLocalAssetBundlesInPlayerDestination()
    {        
        if (Directory.Exists(ADDRESSABLES_PLAYER_ASSET_BUNDLES_PATH))
        {
            EditorFileUtils.DeleteFileOrDirectory(ADDRESSABLES_PLAYER_ASSET_BUNDLES_PATH);
        }
    }

    public void GenerateDownloadablesCatalog(List<string> fileNames, string playerFolder)
    {
        EditorAssetBundlesManager.GenerateDownloadablesCatalog(fileNames, playerFolder);
    }

    public void GenerateDownloadablesConfig(string playerFolder)
    {
        string sourceFileName = "editor_" + Downloadables.Manager.DOWNLOADABLES_CONFIG_FILENAME;
		string sourcePath = "Assets/Editor/Downloadables/" + sourceFileName;

        string destPath = playerFolder + "/" + Downloadables.Manager.DOWNLOADABLES_CONFIG_FILENAME;

        if (File.Exists(destPath))
        {
            File.Delete(destPath);
            File.Delete(destPath + ".meta");
        }

        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, destPath);
        }

		AssetDatabase.Refresh ();
    }

    private bool ProcessEntry(AddressablesCatalogEntry entry, List<string> scenesToAdd, List<string> scenesToRemove)
    {        
        string entryPath = (entry == null) ? null : AssetDatabase.GUIDToAssetPath(entry.GUID);
        string path = entryPath;
        bool success = !string.IsNullOrEmpty(path);

        if (success)
        {            
            AssetImporter assetImporter = AssetImporter.GetAtPath(path);
            if (assetImporter == null)
            {
                sm_logger.LogError("No resource found in " + path);
            }
            else            
            {
				if (AddressablesManager.Mode_NeedsAssetBundles())
                {
                    string assetBundleNameFromCatalog = "";
                    switch (entry.LocationType)
                    {
                        case AddressablesTypes.ELocationType.AssetBundles:
                            assetBundleNameFromCatalog = entry.AssetBundleName;
                            break;
                    }

                    if (assetImporter.assetBundleName != assetBundleNameFromCatalog)
                    {
                        assetImporter.assetBundleName = assetBundleNameFromCatalog;
                        assetImporter.SaveAndReimport();
                    }
                }

                if (IsScene(path))
                {
                    List<string> list = (entry.LocationType == AddressablesTypes.ELocationType.Resources) ? scenesToAdd : scenesToRemove;
                    list.Add(path);
                }
                else if (entry.LocationType == AddressablesTypes.ELocationType.Resources)
                {
                    // if the asset is not already in Resources folder then it's copied to Resources folder
                    if (path.Contains("Resources"))
                    {
                        string resourcesPath = path.Replace("Assets/Resources/", "");
                        entry.AssetName = EditorFileUtils.GetPathWithoutExtension(resourcesPath);
                    }
                    else
                    {                        
                        string token = "Assets/";
                        if (path.StartsWith(token))
                        {
                            string pathFromResources = path.Substring(token.Length);                            
                            string resourcesPath = token + RESOURCES_GENERATED_FOLDER + "/" + pathFromResources;                            

                            // Moves the file
                            EditorFileUtils.Move(path, resourcesPath);

                            // Moves the meta
                            path += ".meta";
                            resourcesPath += ".meta";

							EditorFileUtils.Move(path, resourcesPath);                           

                            // Moves the meta
                            entry.AssetName = GENERATED_FOLDER + "/" + EditorFileUtils.GetPathWithoutExtension(pathFromResources);
                        }
                        else
                        {
                            Debug.Log("Wrong path for entry id " + entry.Id + ". It should start by " + token);
                        }                                                
                    }
                }
            }
        }

        return success;
    }

	private static void CreateDirectory(string path)
	{
		char separator = '/';
		string separatorAsString = "" + separator;
		string[] directories = path.Split (separator);
		string currentPath = "";
		string newPath = "";
		for (int i = 0; i < directories.Length; i++) 
		{			
			if (i > 0) 
			{
				newPath += separatorAsString;
			}

			newPath += directories[i];

			if (!AssetDatabase.IsValidFolder(newPath)) 
			{
				if (directories [i] == "Resources")
					Debug.Log ("Dentro");
				
				Debug.Log ("Create directory " + directories[i] + " in " + currentPath);
				AssetDatabase.CreateFolder(currentPath, directories[i]);
			}

			if (i > 0) 
			{
				currentPath += separatorAsString;
			}

			currentPath += directories[i];
		}
	}

	private static void Move(string srcPath, string dstPath)
	{				
		string dstFolder = Path.GetDirectoryName(dstPath);		
		CreateDirectory(dstFolder);
		AssetDatabase.MoveAsset (srcPath, dstPath);		
	}

    public void MoveGeneratedResourcesToOriginalUbication()
    {
        string directory = EditorFileUtils.PathCombine("Assets", RESOURCES_GENERATED_FOLDER);
        if (Directory.Exists(directory))
        {
            InternalMoveResourcesToOriginalUbication(directory);
            ClearResourcesGenerated();

            AssetDatabase.Refresh();
        }
    }

    private static string META_EXTENSION = ".meta";

    private void InternalMoveResourcesToOriginalUbication(string directory)
    {
        string[] files = Directory.GetFiles(directory);
        int count = files.Length;
        string destPath;
        bool needsToMove;
        for (int i = 0; i < count; i++)
        {
            destPath = files[i].Replace(RESOURCES_GENERATED_FOLDER, "");

            needsToMove = true;

            // metas of directories don't need to be moved in order to prevent many files from being reimported when changing addressables mode
            if (Path.GetExtension(files[i]) == META_EXTENSION)
            {
                string fileWithoutExtension = destPath.Replace(META_EXTENSION, "");                
                if (Directory.Exists(fileWithoutExtension))
                {
                    needsToMove = false;
                }
            }

            if (needsToMove)
            {
                EditorFileUtils.Move(files[i], destPath);
            }
        }

        string[] directories = Directory.GetDirectories(directory);
        count = directories.Length;
        for (int i = 0; i < count; i++)
        {
            InternalMoveResourcesToOriginalUbication(directories[i]);
        }
    }

    private static bool IsScene(string path)
    {
        return !string.IsNullOrEmpty(path) && path.IndexOf(".unity") > -1;
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
        // We ignore the local ones that are not defined in used because they're not really necessary
        //UbiListUtils.SplitIntersectionAndDisjoint(catalogLocalABs, catalogUsedABs, out catalogLocalABs, out returnValue.m_LocalABNotUsedList);

        if (abManifest == null)
        {
            UbiListUtils.AddRange(returnValue.m_ABInUsedNotInManifest, catalogUsedABs, false, true);
            returnValue.m_LocalABList.Clear();
        }
        else
        {            
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
                    if (!returnValue.m_LocalABList.Contains(abName))
                    {
                        returnValue.m_LocalABList.Add(abName);
                    }

                    dependencies = new List<string>(abManifest.GetAllDependencies(abName));
                    UbiListUtils.AddRange(returnValue.m_LocalABList, dependencies, false, true);
                }
                else if (!returnValue.m_ABInUsedNotInManifest.Contains(abName))
                {
                    returnValue.m_ABInUsedNotInManifest.Add(abName);
                }
            }

            List<string> catalogRemoteABs;
            UbiListUtils.SplitIntersectionAndDisjoint(catalogUsedABs, returnValue.m_LocalABList, out catalogLocalABs, out catalogRemoteABs);

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
                    else if (!returnValue.m_ABInUsedNotInManifest.Contains(abName))
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
                    if (!returnValue.m_LocalABList.Contains(abName) && !returnValue.m_RemoteABList.Contains(abName) &&
                        !returnValue.m_ABInManifestNotUsed.Contains(abName))
                    {
                        returnValue.m_ABInManifestNotUsed.Add(abName);
                    }
                }
            }
        }

        return returnValue;
    }    
}
