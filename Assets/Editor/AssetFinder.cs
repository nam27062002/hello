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


    //------------------------------------------------------------------------//
    // Find any asset type in Content browser
    //------------------------------------------------------------------------//
    static void FindAssetInScene<T>(out T[] assetList) where T : UnityEngine.Object
    {
        assetList = Object.FindObjectsOfType(typeof(T)) as T[];

        /*
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
        */
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

    /// <summary>
    /// Resets all shader keywords stored in materials or material selection
    /// </summary>
    [MenuItem("Hungry Dragon/Tools/Texture mipmap reset")]
    public static void TextureMipmapReset()
    {
        Debug.Log("Obtaining texture list");

        //        EditorUtility.("Material keyword reset", "Obtaining Material list ...", "");

        List<Texture2D> textureList;
        FindAssetInContent<Texture2D>(Directory.GetCurrentDirectory() + "\\Assets", out textureList);

        float c = 0;
        foreach (Texture2D texture in textureList)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.mipmapEnabled = false;
                       
            if (EditorUtility.DisplayCancelableProgressBar( "Reimporting texture", path, c / (float)textureList.Count))
            {
                EditorUtility.ClearProgressBar();
                break;
            }

            AssetDatabase.ImportAsset(path);
        }

        Debug.Log("list length: " + textureList.Count);

    }

    [MenuItem("Hungry Dragon/Tools/Find shader in scene")]
    public static void FindShaderInScene()
    {
        Renderer[] renderers;
        FindAssetInScene<Renderer>(out renderers);
        bool found = false;

        foreach (Renderer rend in renderers)
        {
            Material[] materials = rend.materials;
            int matID = 0;
            foreach (Material mat in materials)
            {
                if (mat.shader.name == "Hungry Dragon/Automatic Texture Blending + Lightmap And Recieve Shadow")
                {
                    Debug.Log("GameObject:" + rend.gameObject.name);
                    Debug.Log("MatID" + matID++ + " Name:" + mat.name);
                    Debug.Log("Shader:" + mat.shader.name);
                    found = true;
                }
            }
        }

        if (!found)
        {
            Debug.Log("No shader instance found of:" + "Hungry Dragon/Automatic Texture Blending + Lightmap And Recieve Shadow");
        }
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
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {

	}

}