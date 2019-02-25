using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorAssetBundlesManager
{
    public static string ASSET_BUNDLES_PATH = "AssetBundles";
    public static string DOWNLOADABLES_FOLDER = "Downloadables";
    public static string DOWNLOADABLES_CATALOG_NAME = "downloadablesCatalog.json";
    public static string DOWNLOADABLES_CATALOG_PATH = Path.Combine(DOWNLOADABLES_FOLDER, DOWNLOADABLES_CATALOG_NAME);

    public static void Clear()
    {
        string assetBundleDirectory = GetAssetBundlesDirectory();
        EditorFileUtils.DeleteFileOrDirectory(assetBundleDirectory);
    }

    public static string GetAssetBundlesDirectory()
    {
        return ASSET_BUNDLES_PATH + "/" + EditorUserBuildSettings.activeBuildTarget.ToString();        
    }

    public static void BuildAssetBundles(BuildTarget platform)
    {
        Debug.Log("Building asset bundles...");
        string assetBundleDirectory = EditorFileUtils.PathCombine(ASSET_BUNDLES_PATH, platform.ToString());

        EditorFileUtils.DeleteFileOrDirectory(assetBundleDirectory);
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, platform);
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

    public static void CopyAssetBundles(string dstPath, List<string> fileNames)
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

        JSONClass json = catalog.ToJSON();
        EditorFileUtils.WriteToFile(DOWNLOADABLES_CATALOG_PATH, json.ToString());

        // It copies it to the player's folder too
        if (!string.IsNullOrEmpty(playerFolder))
        {
            string path = Path.Combine(playerFolder, DOWNLOADABLES_CATALOG_NAME);
            EditorFileUtils.WriteToFile(path, json.ToString());
        }
    }

    private static string ASSETS_LUT_PATH = "Assets/Resources/AssetsLUT";
    private static string ASSETS_LUT_FILENAME = "assetsLUT.json";
    private static string ASSETS_LUT_FULL_PATH = ASSETS_LUT_PATH + "/" + ASSETS_LUT_FILENAME;

    private static string ASSETS_LUT_AB_PREFIX = "AssetBundles/";
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
            foreach (KeyValuePair<string, Downloadables.CatalogEntry> pair in entries)
            {
                if (pair.Key.Contains(ASSETS_LUT_AB_PREFIX_IOS) || pair.Key.Contains(ASSETS_LUT_AB_PREFIX_ANDROID))
                {
                    entries.Remove(pair.Key);
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
}
