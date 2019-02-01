using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundlesEditorTools
{
    private static string ASSET_BUNDLES_PATH = "AssetBundles";    

    public static void BuildAssetBundles()
    {
        Debug.Log("Building asset bundles...");
        string assetBundleDirectory = "AssetBundles/" + EditorUserBuildSettings.activeBuildTarget.ToString();

        FileEditorTools.DeleteFileOrDirectory(assetBundleDirectory);
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
    }

    public static void CopyAssetBundles(string dstPath)
    {
        string activeBuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
        string assetBundlesPath = FileEditorTools.PathCombine(ASSET_BUNDLES_PATH, activeBuildTarget);

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
        FileEditorTools.CopyFileOrDirectory(assetBundlesPath, dstPath);

        // Rename the dependencies manifest
        string origDependenciesManifestPath = FileEditorTools.PathCombine(dstPath, activeBuildTarget);
        string newDependenciesManifestPath = FileEditorTools.PathCombine(dstPath, AssetBundlesManager.DependenciesFileName);
        FileEditorTools.RenameFile(origDependenciesManifestPath, newDependenciesManifestPath);
        FileEditorTools.DeleteFileOrDirectory(origDependenciesManifestPath);

        string extension = ".manifest";
        origDependenciesManifestPath += extension;
        newDependenciesManifestPath += extension;
        FileEditorTools.RenameFile(origDependenciesManifestPath, newDependenciesManifestPath);
        FileEditorTools.DeleteFileOrDirectory(origDependenciesManifestPath);
    }   
}
