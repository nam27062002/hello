using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundlesEditorTools
{
    private static string ASSET_BUNDLES_PATH = "AssetBundles";    

    public static void BuildAssetBundles()
    {
        Debug.Log("Building asset bundles...");
        string assetBundleDirectory = FileEditorTools.PathCombine(ASSET_BUNDLES_PATH, EditorUserBuildSettings.activeBuildTarget.ToString());

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

        if (FileEditorTools.GetFilesAmount(assetBundlesPath) > 0)
        {
            if (FileEditorTools.Exists(dstPath))
            {
                FileEditorTools.DeleteFileOrDirectory(dstPath);
            }

            FileEditorTools.CreateDirectory(dstPath);
            FileEditorTools.CopyDirectory(assetBundlesPath, dstPath);

            // Rename the dependencies bundle
            string origDependenciesManifestPath = FileEditorTools.PathCombine(dstPath, activeBuildTarget);
            string newDependenciesManifestPath = FileEditorTools.PathCombine(dstPath, AssetBundlesManager.DependenciesFileName);
            FileEditorTools.RenameFile(origDependenciesManifestPath, newDependenciesManifestPath);
            FileEditorTools.DeleteFileOrDirectory(origDependenciesManifestPath);

            // Rename the dependencies manifest        
            string extension = ".manifest";
            origDependenciesManifestPath += extension;
            newDependenciesManifestPath += extension;
            FileEditorTools.RenameFile(origDependenciesManifestPath, newDependenciesManifestPath);
            FileEditorTools.DeleteFileOrDirectory(origDependenciesManifestPath);
        }
    }   
}
