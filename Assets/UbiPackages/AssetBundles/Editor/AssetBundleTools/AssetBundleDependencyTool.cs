// MonoBehaviourTemplateEditor.cs
// Hungry Dragon
// 
// Created by Diego Campos on 05/03/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Text;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor window.
/// </summary>
public class AssetBundleDependencyTool : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Window instance
	private static AssetBundleDependencyTool m_instance = null;
	public static AssetBundleDependencyTool instance {
		get {
			if(m_instance == null) {
				m_instance = (AssetBundleDependencyTool)EditorWindow.GetWindow(typeof(AssetBundleDependencyTool));
			}
			return m_instance;
		}
	}

    private struct AssetInfo
    {
        public bool checking;
        public string assetpath;
        public string guid;
        public Type assetType;
        public List<string> dependencies;
    };

    static Dictionary<string, AssetInfo> assetDictionary = new Dictionary<string, AssetInfo>();

    UnityEngine.Object initialAsset = null;
    UnityEngine.Object finalAsset = null;

    string initialAssetPath, finalAssetPath;

    List<string> dependencyList = new List<string>();


    void gatherAssetInfo()
    {
        string[] assetList = AssetDatabase.GetAllAssetPaths();
        assetDictionary.Clear();

        float assetCount = (float)assetList.Length;
        float count = 0.0f;

        foreach (string assetPath in assetList)
        {
            AssetInfo asset;
            asset.checking = false;
            asset.assetpath = assetPath;
            asset.guid = AssetDatabase.AssetPathToGUID(assetPath);
            asset.assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            asset.dependencies = new List<string>(AssetDatabase.GetDependencies(assetPath, false));

            assetDictionary.Add(assetPath, asset);

            if (EditorUtility.DisplayCancelableProgressBar("Building asset dependency database", assetPath, (count++) / assetCount))
            {
                assetDictionary.Clear();
                break;
            }

        }
        EditorUtility.ClearProgressBar();
    }

    void setChecking(string assetPath, bool value)
    {
        AssetInfo asset = assetDictionary[assetPath];
        asset.checking = value;
        assetDictionary[assetPath] = asset;
    }

    void unchecking()
    {
        string[] assetList = AssetDatabase.GetAllAssetPaths();

        foreach (string assetPath in assetList)
        {
            setChecking(assetPath, false);
        }
    }

    void startFindingDependencies()
    {
        initialAssetPath = AssetDatabase.GetAssetPath(initialAsset);
        finalAssetPath = AssetDatabase.GetAssetPath(finalAsset);
        dependencyList.Clear();

        if (findDependencies(initialAssetPath))
        {
            dependencyList.Add(initialAssetPath);
        }
    }


    bool findDependencies(string assetPath)
    {
        AssetInfo asset = assetDictionary[assetPath];
        if (asset.checking) return false;
        setChecking(assetPath, true);

        foreach(string dependency in asset.dependencies)
        {
            if (dependency == finalAssetPath || findDependencies(dependency))
            {
                dependencyList.Add(dependency);
                return true;
            }
        }

        return false;
    }


    //------------------------------------------------------------------//
    // METHODS															//
    //------------------------------------------------------------------//
    /// <summary>
    /// Opens the window.
    /// </summary>
    [MenuItem("Tech/AssetBundles/Dependency tool")]	// UNCOMMENT TO ADD MENU ENTRY!!!
	public static void OpenWindow() {
		instance.Show();
	}

	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
        if (assetDictionary.Count == 0)
        {
            gatherAssetInfo();
        }
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {

	}


	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
        initialAsset = EditorGUILayout.ObjectField("Initial Asset: ", initialAsset, typeof(UnityEngine.Object), false);
        finalAsset = EditorGUILayout.ObjectField("Final Asset: ", finalAsset, typeof(UnityEngine.Object), false);
        if (GUILayout.Button("Find asset relationship"))
        {
            if (initialAsset != null && finalAsset != null)
            {
                startFindingDependencies();
            }
        }

        GUILayout.BeginVertical();
        if (dependencyList.Count > 0)
        {
            for (int c = dependencyList.Count - 1; c >= 0; c--)
            {
                GUILayout.Label(dependencyList[c]);
            }
        }
        else
        {
            GUILayout.Label("There are no relationship among both assets");
        }

        if (assetDictionary.Count == 0)
        {
            GUILayout.Label("Asset dependency database must be built");
        }
        GUILayout.EndVertical();

        if (GUILayout.Button("Rebuild asset dependency database"))
        {
            gatherAssetInfo();
        }

    }
}