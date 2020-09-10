// PreBuildChecks.cs
// Hungry Dragon
//
// Common checks to perform in Unity Editor before generating a build
//
// Created by Jordi Riambau on 16th July 2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PreBuildChecks : Editor
{
    readonly static List<string> badJunkPrefabs = new List<string>()
    {
        "PF_BadJunkBone",
        "PF_BadJunkEye",
        "PF_BadJunkFrog",
        "PF_BadJunkMagicBottle"
    };

    [MenuItem("Hungry Dragon/Tools/Gameplay/Run game checks before build...", false, -150)]
    static void Init()
    {
        int errorCounter = 0;

        // Add the prebuild checks to perform
        errorCounter += PrefabAnalysis();

        EditorUtility.DisplayDialog("Result", errorCounter == 0 ? "No errors were found" : errorCounter + " errors found. Please check the console log", "Close");
    }

    /// <summary>
    /// Iterates over project prefabs to check if the entity sku is correctly set
    /// </summary>
    static int PrefabAnalysis()
    {
        EditorUtility.DisplayProgressBar("Prefabs analysis", "Scanning prefabs...", 0.0f);
        int errorCounter = 0;

        string[] guid = AssetDatabase.FindAssets("PF_");
        for (int i = 0; i < guid.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid[i]);
            GameObject asset = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            if (asset != null && !badJunkPrefabs.Contains(asset.name))
            {
                Entity entity = asset.GetComponent<Entity>();
                if (entity != null && entity.sku == "BadJunk")
                {
                    Debug.LogError("Wrong Entity SKU found at: " + path);
                    errorCounter++;
                }
            }

            float progress = (float)i / guid.Length;
            EditorUtility.DisplayProgressBar("Prefabs analysis", "Scanning prefabs...", progress);
        }

        EditorUtility.ClearProgressBar();
        return errorCounter;
    }
}
