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
using System.IO;
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
    private struct AssetInfo
    {
        public bool checking;
        public string assetpath;
        public string guid;
        public Type assetType;
        public string bundleName;
        public bool resource;
        public bool inBuild;
        public List<string> dependencies;
    };

    private class NTree<T>
    {
        public T data;
        public List<NTree<T>> children;

        public NTree(T _data)
        {
            data = _data;
            children = new List<NTree<T>>();
        }

        public void addChild(NTree<T> data)
        {
            children.Add(data);
        }

        ~NTree()
        {
            children = null;
        }
    }

    private enum FindMode
    {
        Dependencies = 0,
        References
    }

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

    static Dictionary<string, AssetInfo> assetDictionary = new Dictionary<string, AssetInfo>();

    UnityEngine.Object initialAsset = null;
    UnityEngine.Object finalAsset = null;

    string initialAssetPath, finalAssetPath;

    List<string> dependencyList = new List<string>();

    NTree<AssetInfo> assetReferencesTree = null;
    List<List<string>> assetReferencesLists = new List<List<string>>();

    FindMode mode = FindMode.Dependencies;

    GUIStyle guiStyle = new GUIStyle();

    bool isInBuid(string assetPath)
    {
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (assetPath == scene.path)
            {
                return scene.enabled;
            }
        }
        return false;
    }


    void gatherAssetInfo()
    {
        string[] assetList = AssetDatabase.GetAllAssetPaths();
        assetDictionary.Clear();

        float assetCount = (float)assetList.Length;
        float count = 0.0f;

        foreach (string assetPath in assetList)
        {
            getAssetInfo(assetPath);

            if (EditorUtility.DisplayCancelableProgressBar("Building asset dependency database", assetPath, (count++) / assetCount))
            {
//                assetDictionary.Clear();
                break;
            }

        }
        EditorUtility.ClearProgressBar();
    }

    AssetInfo getAssetInfo(string assetPath)
    {
        if (assetDictionary.ContainsKey(assetPath))
            return assetDictionary[assetPath];

        AssetInfo asset;
        asset.checking = false;
        asset.assetpath = assetPath;
        asset.guid = AssetDatabase.AssetPathToGUID(assetPath);
        asset.assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
        asset.bundleName = AssetDatabase.GetImplicitAssetBundleName(assetPath);
        asset.resource = assetPath.Contains("Resources");
        asset.inBuild = (asset.assetType == typeof(UnityEditor.SceneAsset) && isInBuid(assetPath));
        asset.dependencies = new List<string>(AssetDatabase.GetDependencies(assetPath, false));

        assetDictionary.Add(assetPath, asset);

        return asset;
    }

    void setChecking(string assetPath, bool value)
    {
        if (!assetDictionary.ContainsKey(assetPath))
            return;
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


    void resetLists()
    {
        dependencyList.Clear();
        assetReferencesLists.Clear();
    }

    /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Find dependences among two assets
    /// 
    void startFindingDependencies(bool firstTime = true)
    {
        initialAssetPath = AssetDatabase.GetAssetPath(initialAsset);
        finalAssetPath = AssetDatabase.GetAssetPath(finalAsset);
        dependencyList.Clear();
        unchecking();

        if (findDependencies(initialAssetPath))
        {
            dependencyList.Add(initialAssetPath);
        }

        if (firstTime && dependencyList.Count == 0)
        {
            UnityEngine.Object tempObj = initialAsset;
            initialAsset = finalAsset;
            finalAsset = tempObj;

            startFindingDependencies(false);
        }
    }


    bool findDependencies(string assetPath)
    {
        AssetInfo asset = getAssetInfo(assetPath);  //assetDictionary[assetPath];
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

    /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Find all reference tree of an asset
    /// 

    void startFindingReferences(string initialAssetPath, bool rich = true)
    {
//        initialAssetPath = AssetDatabase.GetAssetPath(initialAsset);
        unchecking();

        assetReferencesTree = findReferences(initialAssetPath);

        unchecking();
        assetReferencesLists.Clear();

        List<string> referenceList = new List<string>();

        while (createReferenceLists(assetReferencesTree, referenceList, rich))
        {
            assetReferencesLists.Add(referenceList);
            referenceList = new List<string>();
        }
    }


    void getSoundsDependencyLog()
    {
        string[] allSounds = AssetDatabase.FindAssets("t:AudioClip");

        float assetCount = (float)allSounds.Length;
        float count = 0.0f;

        using (StreamWriter sw = new StreamWriter("soundDependency.log"))
        {
            sw.WriteLine("Sound dependency log");
            sw.WriteLine("----------------------------------------------------------------------------------------------");
            foreach (string assetGUID in allSounds)
            {

                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);

                if (EditorUtility.DisplayCancelableProgressBar("Finding sound references: ", assetPath, (count++) / assetCount))
                {
                    //                assetDictionary.Clear();
                    break;
                }
                startFindingReferences(assetPath, false);

                if (assetReferencesLists.Count > 0)
                {
                    sw.WriteLine("====================================================================================================");
                    sw.WriteLine("Dependencies for asset: " + assetPath);
                    sw.WriteLine("====================================================================================================");
                    for (int c = 0; c < assetReferencesLists.Count; c++)
                    {
                        List<string> referenceList = assetReferencesLists[c];

                        foreach (string reference in referenceList)
                            sw.WriteLine(reference, guiStyle);

                        if (c != assetReferencesLists.Count - 1)
                            sw.WriteLine("-------------------------------------------------------------------------------------------------");
                    }
                }

            }
        }
        EditorUtility.ClearProgressBar();

    }

    string getRichName(NTree<AssetInfo> node)
    {
        StringBuilder sb = new StringBuilder();

        if (node.data.resource || node.data.inBuild)
        {
            sb.Append("<color=red>");
            sb.Append(node.data.assetpath);
            sb.Append("</color>");
        }
        else if (node.data.bundleName != "")
        {
            sb.Append("<color=green>");
            sb.Append(node.data.assetpath);
            sb.Append("</color>");
        }
        else
        {
            sb.Append("<color=silver>");
            sb.Append(node.data.assetpath);
            sb.Append("</color>");
        }

        return sb.ToString();
    }

    bool createReferenceLists(NTree<AssetInfo> node, List<string> referenceList, bool rich = true)
    {
        if (node.data.checking) return false;

        if (node.children.Count == 0)
        {
//          referenceList.Add(node.data.assetpath);
            referenceList.Add(rich ? getRichName(node): node.data.assetpath);
            node.data.checking = true;
            return true;
        }

        for (int c = 0; c < node.children.Count; c++)
        {
            if (createReferenceLists(node.children[c], referenceList, rich))
            {
                //              referenceList.Add(node.data.assetpath);
                referenceList.Add(rich ? getRichName(node): node.data.assetpath);
                return true;
            }
        }

        return false;
    }

    NTree<AssetInfo> findReferences(string assetPath)
    {
        AssetInfo asset = getAssetInfo(assetPath);  //assetDictionary[assetPath];
        if (asset.checking) return null;
        setChecking(assetPath, true);

        NTree<AssetInfo> currentNode = new NTree<AssetInfo>(asset);
        List<string> references = new List<string>();

        foreach (KeyValuePair<string, AssetInfo> assetInfo in assetDictionary)
        {
            if (assetPath == assetInfo.Key) continue;

            string[] dependencies = assetInfo.Value.dependencies.ToArray();

            foreach (string dependency in dependencies)
            {
                if (assetPath == dependency)
                {
                    if (!references.Contains(assetInfo.Key))
                    {
                        references.Add(assetInfo.Key);
                    }
                    break;
                }
            }
        }

        foreach (string reference in references)
        {
            NTree<AssetInfo> childNode = findReferences(reference);
            if (childNode != null)
            {
                currentNode.addChild(childNode);
            }
        }

//        setChecking(assetPath, false);

        return currentNode;
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
        resetLists();
        guiStyle.richText = true;
        Debug.Log("AssetBundleDependencyTool.OnEnable()");
/*
        if (assetDictionary.Count == 0)
        {
            gatherAssetInfo();
        }
*/
    }

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
        Debug.Log("AssetBundleDependencyTool.OnDisable()");
    }

    Vector2 scrollPos = Vector2.zero;

    /// <summary>
    /// Update the inspector window.
    /// </summary>
    public void OnGUI() {
        initialAsset = EditorGUILayout.ObjectField("Initial Asset: ", initialAsset, typeof(UnityEngine.Object), false);
        finalAsset = EditorGUILayout.ObjectField("Final Asset: ", finalAsset, typeof(UnityEngine.Object), false);

        GUILayout.Label("<color=red>Red assets come from Resources folder or scenes checked in EditorBuildSettings</color>", guiStyle);
        GUILayout.Label("<color=green>Green assets come from Asset Bundles</color>", guiStyle);

        if (GUILayout.Button("Find asset relationship"))
        {
            resetLists();
            if (initialAsset != null && finalAsset != null)
            {
                startFindingDependencies();
            }
            mode = FindMode.Dependencies;
        }

        if (GUILayout.Button("Find all asset references"))
        {
            resetLists();
            if (initialAsset != null)
            {
                startFindingReferences(AssetDatabase.GetAssetPath(initialAsset));
            }
            mode = FindMode.References;
        }

        if (GUILayout.Button("Get sounds references log"))
        {
            getSoundsDependencyLog();
        }


        GUILayout.BeginVertical();

        if (mode == FindMode.Dependencies)
        {
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
        }
        else
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            if (assetReferencesLists.Count > 0)
            {

                for (int c = 0; c < assetReferencesLists.Count; c++)
                {
                    GUILayout.Label("<color=yellow>----------------------------------------------------------------------------------------------------------------------------------------</color>", guiStyle);
                    List<string> referenceList = assetReferencesLists[c];

                    foreach(string reference in referenceList)
                        GUILayout.Label(reference, guiStyle);
                }
            }
            EditorGUILayout.EndScrollView();

            if (assetDictionary.Count == 0)
            {
                GUILayout.Label("Asset dependency database must be built");
            }
        }

        GUILayout.EndVertical();

        if (GUILayout.Button("Rebuild asset dependency database"))
        {
            gatherAssetInfo();
        }

        if (GUILayout.Button("Clear asset dependency database"))
        {
            assetDictionary.Clear();
        }

    }

}