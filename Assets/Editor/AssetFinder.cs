// MonoBehaviourTemplateEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor window.
/// </summary>
public class AssetFinder : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Window instance
	private static AssetFinder m_instance = null;
	public static AssetFinder instance {
		get {
			if(m_instance == null) {
				m_instance = (AssetFinder)EditorWindow.GetWindow(typeof(AssetFinder));
			}
			return m_instance;
		}
	}

    //------------------------------------------------------------------------//
    // Find any asset type in Content browser
    //------------------------------------------------------------------------//
    static void FindAssetInContent<T>(string path, out List<T> assetList) where T : UnityEngine.Object
    {
        assetList = new List<T>();

        //        string[] fileList = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        string typeName = typeof(T).ToString();
        typeName = typeName.Contains("UnityEngine") ? typeName.Replace("UnityEngine.", "") : typeName;
        string filter = "t:" + typeName;
        Debug.Log("filter: " + filter);
        string[] guids = AssetDatabase.FindAssets(filter);

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (asset != null)
            {
                assetList.Add(asset);
            }
        }
    }

    /// <summary>
    /// Resets all shader keywords stored in materials or material selection
    /// </summary>
    [MenuItem("Hungry Dragon/Tools/Material keyword reset")]
    public static void MaterialkeywordReset()
    {
        Debug.Log("Obtaining material list");

//        EditorUtility.("Material keyword reset", "Obtaining Material list ...", "");

        List<Material> materialList;
        FindAssetInContent<Material>(Directory.GetCurrentDirectory() + "\\Assets", out materialList);

//        AssetDatabase.StartAssetEditing();
        for (int c = 0; c <materialList.Count; c++)
        {
            materialList[c].shaderKeywords = null;
            EditorUtility.SetDirty(materialList[c]);
        }

        AssetDatabase.SaveAssets();

        Debug.Log("list length: " + materialList.Count);

    }



    //------------------------------------------------------------------//
    // METHODS															//
    //------------------------------------------------------------------//
    /// <summary>
    /// Opens the window.
    /// </summary>
    public static void OpenWindow() {
		instance.Show();
	}

	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		
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

	}

}