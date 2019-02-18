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
        FileEditorTools.DeleteFileOrDirectory(assetBundleDirectory);
    }

    public static string GetAssetBundlesDirectory()
    {
        return FileEditorTools.PathCombine(ASSET_BUNDLES_PATH, EditorUserBuildSettings.activeBuildTarget.ToString());
    }

    public static void BuildAssetBundles()
    {
        Debug.Log("Building asset bundles...");
        string assetBundleDirectory = GetAssetBundlesDirectory();

        FileEditorTools.DeleteFileOrDirectory(assetBundleDirectory);
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
    }

    public static AssetBundleManifest LoadAssetBundleManifest(out AssetBundle manifestBundle)
    {
        AssetBundleManifest abManifest = null;
        manifestBundle = null;

        string activeBuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
        string assetBundlesDirectory = GetAssetBundlesDirectory();
        assetBundlesDirectory = Path.Combine(assetBundlesDirectory, activeBuildTarget);
        
        if (assetBundlesDirectory != null && File.Exists(assetBundlesDirectory))
        {
            abManifest = AssetBundlesManager.LoadManifest(assetBundlesDirectory, out manifestBundle);
        }        

        return abManifest;
    }


    public static void CopyAssetBundles(string dstPath, List<string> fileNames)
    {        
        string assetBundlesPath = GetAssetBundlesDirectory();

        if (FileEditorTools.GetFilesAmount(assetBundlesPath) > 0 && fileNames.Count > 0)
        {
            if (FileEditorTools.Exists(dstPath))
            {
                FileEditorTools.DeleteFileOrDirectory(dstPath);
            }

            FileEditorTools.CreateDirectory(dstPath);
            
            int count = fileNames.Count;
            string srcFile;
            string dstFile;
            string directory;
            string fileName;
            for (int i = 0; i < count; i++)
            {
                fileName = fileNames[i];
                srcFile = FileEditorTools.PathCombine(assetBundlesPath, fileName);
                
                if (File.Exists(srcFile))
                {
                    directory = dstPath;
                    dstFile = GetDirectoryAndFileName(ref directory, ref fileName);                    
                    FileEditorTools.CreateDirectory(directory);
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
            dstPath = FileEditorTools.PathCombine(dstPath, tokens[i]);
        }

        fileName = tokens[count - 1];

        return FileEditorTools.PathCombine(dstPath, fileName);
    }

    public static void CopyAssetBundlesManifest(string dstPath)
    {
        string activeBuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();

        // Rename the dependencies bundle
        string assetBundlesPath = GetAssetBundlesDirectory();
        string origDependenciesManifestPath = FileEditorTools.PathCombine(assetBundlesPath, activeBuildTarget);
        string newDependenciesManifestPath = FileEditorTools.PathCombine(dstPath, AssetBundlesManager.DEPENDENCIES_FILENAME);
        if (File.Exists(origDependenciesManifestPath))
        {
            FileEditorTools.RenameFile(origDependenciesManifestPath, newDependenciesManifestPath);

            // Rename the dependencies manifest        
            string extension = ".manifest";
            origDependenciesManifestPath += extension;
            newDependenciesManifestPath += extension;
            FileEditorTools.RenameFile(origDependenciesManifestPath, newDependenciesManifestPath);
        }
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
        FileEditorTools.WriteToFile(DOWNLOADABLES_CATALOG_PATH, json.ToString());

        // It copies it to the player's folder too
        if (!string.IsNullOrEmpty(playerFolder))
        {
            string path = Path.Combine(playerFolder, DOWNLOADABLES_CATALOG_NAME);
            FileEditorTools.WriteToFile(path, json.ToString());
        }        
    }
}
