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
        public string assetpath;
        public string guid;
        public Type assetType;
        public List<string> references;
    };

    Dictionary<string, AssetInfo> assetDictionary = new Dictionary<string, AssetInfo>();

    string textarea;

    void gatherAssetInfo()
    {
        string[] assetList = AssetDatabase.GetAllAssetPaths();

///        StringB

        foreach (string assetPath in assetList)
        {
            AssetInfo asset;
            asset.assetpath = assetPath;
            asset.guid = AssetDatabase.AssetPathToGUID(assetPath);
            asset.assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            asset.references = new List<string>();

            assetDictionary.Add(asset.guid, asset);
        }


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
        gatherAssetInfo();
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called 100 times per second on all visible windows.
	/// </summary>
	public void Update() {
		
	}

	/// <summary>
	/// OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update.
	/// Called less times as if it was OnGUI/Update
	/// </summary>
	public void OnInspectorUpdate() {
		
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
        EditorGUILayout.LabelField("Asset list:");


//        EditorGUILayout.BeginScrollView()

	}
}