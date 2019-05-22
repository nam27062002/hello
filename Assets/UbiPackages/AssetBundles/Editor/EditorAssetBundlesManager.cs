using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorAssetBundlesManager
{
    public static string ASSET_BUNDLES_PATH = "AssetBundles";
    public static string DOWNLOADABLES_FOLDER = "AssetBundles/Remote";
    public static string ASSET_BUNDLES_LOCAL_FOLDER = "AssetBundles/Local";
    public static string DOWNLOADABLES_CATALOG_NAME = "downloadablesCatalog.json";
    public static string DOWNLOADABLES_CATALOG_PATH = Path.Combine(DOWNLOADABLES_FOLDER, DOWNLOADABLES_CATALOG_NAME);

    public static void Clear()
    {
        string directory = GetAssetBundlesDirectory();
        EditorFileUtils.DeleteFileOrDirectory(directory);

        directory = GetPlatformDirectory(DOWNLOADABLES_FOLDER);
        EditorFileUtils.DeleteFileOrDirectory(directory);

        directory = GetPlatformDirectory(ASSET_BUNDLES_LOCAL_FOLDER);
        EditorFileUtils.DeleteFileOrDirectory(directory);

        EditorFileUtils.DeleteFileOrDirectory(DOWNLOADABLES_CATALOG_PATH);
    }

    public static string GetAssetBundlesDirectory()
    {
        return GetPlatformDirectory(ASSET_BUNDLES_PATH);
    }

    private static string GetPlatformDirectory(string directory)
    {
        return directory + "/" + EditorUserBuildSettings.activeBuildTarget.ToString();
    }

    public static void BuildAssetBundles(BuildTarget platform, List<string> abNames=null)
    {
        Debug.Log("Building asset bundles...");
        string assetBundleDirectory = EditorFileUtils.PathCombine(ASSET_BUNDLES_PATH, platform.ToString());

        // LZ4 algorithm is used to reduce memory footprint
        BuildAssetBundleOptions compression = BuildAssetBundleOptions.ChunkBasedCompression;

        // LZMA algorithm, it gives the smallest possible size
        //BuildAssetBundleOptions compression = BuildAssetBundleOptions.None;

        // If we're rebuilding all asset bundles then we need to clean up first
        if (abNames == null)
        {
            EditorFileUtils.DeleteFileOrDirectory(assetBundleDirectory);

            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }

            BuildPipeline.BuildAssetBundles(assetBundleDirectory, compression, platform);
        }
        else
        {
            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }

            // Rebuilds only the selected ones        
            BuildPipeline.BuildAssetBundles(assetBundleDirectory, GetBuildsForPaths(abNames).ToArray(), compression, platform);
        }
    }    

    private static List<AssetBundleBuild> GetBuildsForPaths(List<string> abNames)
    {
        List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();

        // Get asset bundle names from selection
        foreach (var o in abNames)
        {
            AssetBundleBuild build = new AssetBundleBuild();

            build.assetBundleName = o;
            build.assetBundleVariant = null;
            build.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(o);

            assetBundleBuilds.Add(build);
        }

        return assetBundleBuilds;
    }

    public static AssetBundleManifest LoadAssetBundleManifest(out AssetBundle manifestBundle)
    {
        AssetBundleManifest abManifest = null;
        manifestBundle = null;

        string activeBuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
        string assetBundlesDirectory = GetAssetBundlesDirectory();
        assetBundlesDirectory = assetBundlesDirectory + "/" + activeBuildTarget;        

        if (assetBundlesDirectory != null && File.Exists(assetBundlesDirectory))
        {
            abManifest = AssetBundlesManager.LoadManifest(assetBundlesDirectory, out manifestBundle);
        }

        return abManifest;
    }

    public static void CopyAssetBundles(string dstPath, List<string> fileNames, bool asText = false)
    {
        string assetBundlesPath = GetAssetBundlesDirectory();

        if (EditorFileUtils.GetFilesAmount(assetBundlesPath) > 0 && fileNames.Count > 0)
        {
            if (EditorFileUtils.Exists(dstPath))
            {
                EditorFileUtils.DeleteFileOrDirectory(dstPath);
            }

            EditorFileUtils.CreateDirectory(dstPath);

            int count = fileNames.Count;
            string srcFile;
            string dstFile;
            string directory;
            string fileName;
            for (int i = 0; i < count; i++)
            {
                fileName = fileNames[i];
                srcFile = EditorFileUtils.PathCombine(assetBundlesPath, fileName);

                if (File.Exists(srcFile))
                {
                    directory = dstPath;
                    dstFile = GetDirectoryAndFileName(ref directory, ref fileName);
                    if (asText)
                    {
                        dstFile += ".txt";
                    }

                    EditorFileUtils.CreateDirectory(directory);
                    File.Copy(srcFile, dstFile, true);
                }
                else
                {
                    Debug.LogError("Asset bundle " + fileNames[i] + " not found");
                }
            }
        }
    }
    
    public static void DeleteAssetBundles(List<string> fileNames)
    {
        string assetBundlesPath = GetAssetBundlesDirectory();

        if (EditorFileUtils.GetFilesAmount(assetBundlesPath) > 0 && fileNames.Count > 0)
        {            
            int count = fileNames.Count;
            string fileName;
            for (int i = 0; i < count; i++)
            {
                fileName = EditorFileUtils.PathCombine(assetBundlesPath, fileNames[i]);
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }                
            }
        }
    }

    public static string GetDirectoryAndFileName(ref string dstPath, ref string fileName)
    {
        string[] tokens = fileName.Split('/');
        int count = tokens.Length;
        for (int i = 0; i < count - 1; i++)
        {
            dstPath = EditorFileUtils.PathCombine(dstPath, tokens[i]);
        }

        fileName = tokens[count - 1];

        return EditorFileUtils.PathCombine(dstPath, fileName);
    }    

    public static void GenerateDownloadablesCatalog(List<string> fileNames, string playerFolder)
    {
        string assetBundlesDirectory = GetAssetBundlesDirectory();

        Downloadables.Catalog catalog = new Downloadables.Catalog();

        if (fileNames != null)
        {
            string fileName;
            string path;
            Downloadables.CatalogEntry entry;
            int count = fileNames.Count;
            for (int i = 0; i < count; i++)
            {
                fileName = fileNames[i];
                path = Path.Combine(assetBundlesDirectory, fileName);

                if (File.Exists(path))
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    entry = new Downloadables.CatalogEntry();
                    entry.Setup(StringUtils.CRC32(bytes), bytes.Length);

                    catalog.AddEntry(fileName, entry);
                }
            }
        }

        if (!Directory.Exists(DOWNLOADABLES_FOLDER))
        {
            Directory.CreateDirectory(DOWNLOADABLES_FOLDER);
        }

        catalog.UrlBase = AssetBundles.LaunchAssetBundleServer.GetServerURL() + EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
        JSONClass json = catalog.ToJSON();       
        EditorFileUtils.WriteToFile(DOWNLOADABLES_CATALOG_PATH, json.ToString());

        // AssetsLUT is generated so remote asset bundles can work
        GenerateAssetsLUTFromDownloadablesCatalog();
        // It copies it to the player's folder too
        /*if (!string.IsNullOrEmpty(playerFolder))
        {
            string path = Path.Combine(playerFolder, DOWNLOADABLES_CATALOG_NAME);
            EditorFileUtils.WriteToFile(path, json.ToString());
        }*/
    }

    private static string ASSETS_LUT_PATH = "Assets/Resources/AssetsLUT";
    private static string ASSETS_LUT_FILENAME = "assetsLUT.json";
    private static string ASSETS_LUT_FULL_PATH = ASSETS_LUT_PATH + "/" + ASSETS_LUT_FILENAME;

    private static string ASSETS_LUT_AB_PREFIX = Downloadables.Manager.REMOTE_FOLDER;
    private static string ASSETS_LUT_AB_PREFIX_IOS = ASSETS_LUT_AB_PREFIX + "iOS/";
    private static string ASSETS_LUT_AB_PREFIX_ANDROID = ASSETS_LUT_AB_PREFIX + "Android/";    

    public static void GenerateAssetsLUTFromDownloadablesCatalog()
    {
        Downloadables.Catalog assetsLUTCatalog = new Downloadables.Catalog();
        
        string assetsLUTPath = ASSETS_LUT_PATH;        
        string assetsLUTFullPath = ASSETS_LUT_FULL_PATH;
        JSONNode assetsLUTJson = null;
        if (File.Exists(assetsLUTFullPath))
        {
            assetsLUTJson = JSON.Parse(File.ReadAllText(assetsLUTFullPath));
            assetsLUTCatalog.Load(assetsLUTJson, null);

            // Deletes all asset bundle entries because we are going to reenter them
            Dictionary<string, Downloadables.CatalogEntry> entries = assetsLUTCatalog.GetEntries();
            if (entries != null)
            {
                List<string> keysToDelete = new List<string>();
                foreach (KeyValuePair<string, Downloadables.CatalogEntry> pair in entries)
                {                    
                    if (pair.Key.Contains("iOS/") || pair.Key.Contains("Android/"))
                    {
                        keysToDelete.Add(pair.Key);
                    }
                }

                int count = keysToDelete.Count;
                for (int i = 0; i < count; i++)
                {
                    entries.Remove(keysToDelete[i]);
                }
            }
        }

        string fileName = Path.Combine(DOWNLOADABLES_FOLDER, DOWNLOADABLES_CATALOG_NAME);
        if (File.Exists(fileName))
        {
            JSONNode json = JSON.Parse(File.ReadAllText(fileName));
            Downloadables.Catalog catalog = new Downloadables.Catalog();
            catalog.Load(json, null);

            Downloadables.CatalogEntry entry;
            Dictionary<string, Downloadables.CatalogEntry> entries = catalog.GetEntries();
            foreach (KeyValuePair<string, Downloadables.CatalogEntry> pair in entries)
            {
                entry = new Downloadables.CatalogEntry();
                entry.CRC = pair.Value.CRC;
                entry.Size = pair.Value.Size;
                assetsLUTCatalog.AddEntry(ASSETS_LUT_AB_PREFIX_IOS + pair.Key, entry);

                entry = new Downloadables.CatalogEntry();
                entry.CRC = pair.Value.CRC;
                entry.Size = pair.Value.Size;
                assetsLUTCatalog.AddEntry(ASSETS_LUT_AB_PREFIX_ANDROID + pair.Key, entry);
            }

            if (!Directory.Exists(assetsLUTPath))
            {
                Directory.CreateDirectory(assetsLUTPath);
            }

            json = assetsLUTCatalog.ToJSON();

            if (json != null)
            {                
                JSONClass assets = (JSONClass)json[Downloadables.Catalog.CATALOG_ATT_ENTRIES];
                if (assets != null)
                {
                    string id;
                    System.Collections.ArrayList keys = assets.GetKeys();
                    int count = keys.Count;
                    string type;
                    for (int i = 0; i < count; i++)
                    {
                        id = (string)keys[i];
                        type = (id.Contains("iOS/") || id.Contains("Android/")) ? "bundle" : "content";                            
                        assets[id].Add("type", type);                        
                    }
                }
            }

            if (assetsLUTJson != null)
            {
                string key = "urlBase";                
                json.Add(key, AssetBundles.LaunchAssetBundleServer.GetServerURL() + EditorUserBuildSettings.activeBuildTarget.ToString() + "/");

                key = "release";
                json.Add(key, assetsLUTJson[key]);

                key = "deltas";
                json.Add(key, assetsLUTJson[key]);

                key = "revision";
                json.Add(key, assetsLUTJson[key]);
            }

            File.WriteAllText(assetsLUTFullPath, json.ToString());            
            Debug.Log("AssetsLUT generated in " + assetsLUTPath);
        }
        else
        {
            Debug.LogError("No downloadables catalog found in " + DOWNLOADABLES_FOLDER);
        }
    }

    public static void GenerateDownloadablesCatalogFromAssetsLUT()
    {
        Downloadables.Catalog downloadablesCatalog = new Downloadables.Catalog();
        Downloadables.Catalog assetsLUTCatalog = new Downloadables.Catalog();
        
        string assetsLUTPath = ASSETS_LUT_PATH;        
        string assetsLUTFullPath = ASSETS_LUT_FULL_PATH;
        JSONNode assetsLUTJson = null;
        if (File.Exists(assetsLUTFullPath))
        {
            assetsLUTJson = JSON.Parse(File.ReadAllText(assetsLUTFullPath));
            assetsLUTCatalog.Load(assetsLUTJson, null);

            string prefix = ASSETS_LUT_AB_PREFIX + EditorUserBuildSettings.activeBuildTarget + "/";            

            // Deletes all asset bundle entries because we are going to reenter them
            Dictionary<string, Downloadables.CatalogEntry> entries = assetsLUTCatalog.GetEntries();
            foreach (KeyValuePair<string, Downloadables.CatalogEntry> pair in entries)
            {
                if (pair.Key.Contains(prefix))
                {                                        
                    downloadablesCatalog.AddEntry(pair.Key.Replace(prefix, ""), pair.Value);
                }                
            }

            JSONNode json = downloadablesCatalog.ToJSON();
            if (assetsLUTJson != null)
            {
                string key = "urlBase";
                json.Add(key, assetsLUTJson[key]);

                key = "release";
                json.Add(key, assetsLUTJson[key]);

                key = "deltas";
                json.Add(key, assetsLUTJson[key]);

                key = "revision";
                json.Add(key, assetsLUTJson[key]);
            }

            Debug.Log("Downloadables catalog: " + json.ToString());
        }
        else
        {
            Debug.LogError("No assetsLUT file found in " + assetsLUTPath);
        }
    }

    public static void ClearDownloadablesCache()
    {
        string path = Downloadables.Manager.DOWNLOADABLESS_ROOT_PATH;
        if (Directory.Exists(path))
        {
            EditorFileUtils.DeleteFileOrDirectory(path);
        }
    }
}
