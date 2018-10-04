﻿using UnityEditor;
using UnityEditor.Build;
using UnityEngine;


public class PlatformBuilderHelper
{
	public static string getAbsolutePlatformResourcesPath()
	{
		return Application.dataPath + "/PlatformResources/";
	}
    public static string getRelativePlatformResourcesPath()
    {
		return "Assets/PlatformResources/";
    }

    public readonly static string resourcesFolder = "Resources";


    public static bool platformBuilderReady = false;
}


public class BuildPreProcessor : IPreprocessBuild {
	public int callbackOrder { get { return 0; } }

	public void OnPreprocessBuild(BuildTarget target, string path)
	{
        PlatformBuilderHelper.platformBuilderReady = false;
        Debug.Log ("Preprocessing build for: " + target.ToString () + " platform");
		string dpath = PlatformBuilderHelper.getRelativePlatformResourcesPath();

        string resourcesPath = dpath + PlatformBuilderHelper.resourcesFolder;
        string platformPath = dpath + target.ToString();

        if (AssetDatabase.IsValidFolder(resourcesPath))
        {
            Debug.Log("Resources folder: " + resourcesPath + " already exists.");
            if (AssetDatabase.DeleteAsset(resourcesPath))
            {
                Debug.Log("Resources folder deleted.");
            }
            else
            {
                Debug.Log("Resources folder couldn't be remove.");
                return;
            }
        }

        Debug.Log("Moving folder: " + platformPath + " to " + resourcesPath);
        string err = AssetDatabase.MoveAsset(platformPath, resourcesPath);

        PlatformBuilderHelper.platformBuilderReady = string.IsNullOrEmpty(err);

        if (PlatformBuilderHelper.platformBuilderReady)
        {
            Debug.Log("Ok!");
        }
        else
        {
            Debug.Log("Error! : " + err);
        }
    }
}


public class BuildPostProcessor : IPostprocessBuild {
	public int callbackOrder { get { return 0; } }

	public void OnPostprocessBuild(BuildTarget target, string path)
	{
		Debug.Log ("Postprocessing build for: " + target.ToString () + " platform");
        if (PlatformBuilderHelper.platformBuilderReady)
        {
            string dpath = PlatformBuilderHelper.getRelativePlatformResourcesPath();
            string resourcesPath = dpath + PlatformBuilderHelper.resourcesFolder;
            string platformPath = dpath + target.ToString();

            if (AssetDatabase.IsValidFolder(resourcesPath))
            {
                Debug.Log("Moving folder: " + resourcesPath + " to " + platformPath);
                string err = AssetDatabase.MoveAsset(resourcesPath, platformPath);
                if (string.IsNullOrEmpty(err))
                {
                    Debug.Log("Ok!");
                }
                else
                {
                    Debug.Log("Error! : " + err);
                }

            }
            else
            {
                Debug.Log("resources folder: " + resourcesPath + " don't exists.");
            }
        }
	}
}
