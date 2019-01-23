using System.IO;
using UnityEditor;
using UnityEngine;

public class UnitTestsEditor : MonoBehaviour
{
    private static string ASSET_BUNDLES_PATH = "AssetBundles";
    private static string STREAMING_ASSETS_ROOT_PATH = "Assets/StreamingAssets";
    private static string REMOTE_ASSETS_ROOT_PATH = "Assets/RemoteAssets";
    private static string LOCAL_ASSET_BUNDLES_PATH = PathCombine(STREAMING_ASSETS_ROOT_PATH, "AssetBundles");
    private static string REMOTE_ASSET_BUNDLES_PATH = PathCombine(REMOTE_ASSETS_ROOT_PATH, "AssetBundles");

    [MenuItem("UnitTests/Build Asset Bundles")]
    static void BuildAssetBundles()
    {
        Debug.Log("Building UnitTests asset bundles...");

    }

    private static bool VERBOSE = false;

    [MenuItem("UnitTests/Prepare Build")]
    static void PrepareBuild()
    {
        Debug.Log("Preparing UnitTests build...");

        // Assumes asset bundles have already been built

        ClearBuildAssets();

        CopyAssetBundles(STREAMING_ASSETS_ROOT_PATH);
        //CopyAssetBundles(REMOTE_ASSETS_ROOT_PATH);
       
        AssetDatabase.Refresh();

        Debug.Log("UnitTests build ready");
    }

    [MenuItem("UnitTests/Clear Build")]
    static void ClearBuild()
    {
        ClearBuildAssets();
        AssetDatabase.Refresh();        
    }

    static void ClearBuildAssets()
    {
        //DeleteFileOrDirectory(LOCAL_ASSET_BUNDLES_PATH);
        //DeleteFileOrDirectory(REMOTE_ASSET_BUNDLES_PATH);        
        DeleteFileOrDirectory(STREAMING_ASSETS_ROOT_PATH);
        //DeleteFileOrDirectory(REMOTE_ASSETS_ROOT_PATH);        
    }

    private static void CopyAssetBundles(string dstPath)
    {
        string activeBuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
        string assetBundlesPath = PathCombine(ASSET_BUNDLES_PATH, activeBuildTarget);
        
        /*
        if (!Directory.Exists(dstPath))
        {
            CreateDirectory(dstPath);
        }
        else
        {                                
            DeleteFileOrDirectory(dstPath);
        }

        dstPath = PathCombine(dstPath, "AssetBundles");

        if (!Directory.Exists(dstPath))
        {
            CreateDirectory(dstPath);
        }*/      

        // Copy all asset bundles to StreamingAssets
        CopyFileOrDirectory(assetBundlesPath, dstPath);

        // Rename the dependencies manifest
        string origDependenciesManifestPath = PathCombine(dstPath, activeBuildTarget);
        string newDependenciesManifestPath = PathCombine(dstPath, AssetBundlesManager.DependenciesFileName);
        RenameFile(origDependenciesManifestPath, newDependenciesManifestPath);
        DeleteFileOrDirectory(origDependenciesManifestPath);

        string extension = ".manifest";
        origDependenciesManifestPath += extension;
        newDependenciesManifestPath += extension;
        RenameFile(origDependenciesManifestPath, newDependenciesManifestPath);
        DeleteFileOrDirectory(origDependenciesManifestPath);
    }

    private static void CreateDirectory(string path)
    {
        if (VERBOSE)
        {
            Debug.Log("Deleting " + path);
        }
        Directory.CreateDirectory(path);
    }

    private static void DeleteFileOrDirectory(string path)
    {        
        if (VERBOSE)
        {
            Debug.Log("Deleting " + path);
        }

        //if (File.Exists(path))
        {
            FileUtil.DeleteFileOrDirectory(path);
        }
    }        

    private static void CopyFileOrDirectory(string srcPath, string dstPath)
    {
        if (VERBOSE)
        {
            Debug.Log("Copy " + srcPath + " into " + dstPath);
        }

        FileUtil.CopyFileOrDirectory(srcPath, dstPath);
    }

    private static void RenameFile(string oldPath, string newPath)
    {
        if (VERBOSE)
        {
            Debug.Log("Rename " + oldPath + " to " + newPath);
        }

        FileUtil.ReplaceFile(oldPath, newPath);
    }

    private static string PathCombine(string path1, string path2)
    {
        return path1 + "/" + path2;
        //return Path.Combine(path1, path2);
    }
}
