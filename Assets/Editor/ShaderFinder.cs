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
public class ShaderFinder : EditorWindow {
    //------------------------------------------------------------------//
    // CONSTANTS														//
    //------------------------------------------------------------------//


    public class AssetFinderResult
    {
        public string m_gameObjectName;
        public string m_materialName;
        public string m_shaderName;

        public AssetFinderResult(string gameobject, string materialName, string shaderName)
        {
            m_gameObjectName = gameobject;
            m_materialName = materialName;
            m_shaderName = shaderName;
        }
    }



    //------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES											//
    //------------------------------------------------------------------//
    // Window instance
    private static ShaderFinder m_instance = null;
	public static ShaderFinder instance {
		get {
			if(m_instance == null) {
				m_instance = EditorWindow.GetWindow<ShaderFinder>();
			}
			return m_instance;
		}
	}

    // Shader list
    //    public List<Shader> m_shaderList = new List<Shader>();
    [SerializeField]
    public Shader[] m_shaderList;
    private SerializedObject m_shaderFinder;
    private SerializedProperty m_propertyShaderList;


    private bool m_searchScene = true;


    public enum WhereToSearch { SearchInScene, SearchInAssets };

    public enum TypeOfSearch { ObjectsContainingShadersInList, ObjectNotContainingShadersInList };

    private WhereToSearch m_whereToSearch;
    private TypeOfSearch m_typeOfSearch;

    //------------------------------------------------------------------//
    // METHODS															//
    //------------------------------------------------------------------//
    /// <summary>
    /// Opens the window.
    /// </summary>
    [MenuItem("Hungry Dragon/Tools/Shader Finder")]

    public static void OpenWindow() {
		instance.Show();
	}

	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
        m_shaderFinder = new SerializedObject(this);
        m_propertyShaderList = m_shaderFinder.FindProperty("m_shaderList");

        LoadShaderList(out m_shaderList);
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {

	}

    private AssetFinderResult[] m_checkResults;
    private Vector2 scrollPos = Vector2.zero;
	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {

        EditorGUILayout.LabelField("Shader finder can check for used shaders in scene hierarchy or assets folder.");
        m_whereToSearch = (WhereToSearch)EditorGUILayout.EnumPopup(m_whereToSearch);

        m_shaderFinder.Update();
//        EditorGUILayout.ObjectField(m_propertyShaderList);
        EditorGUILayout.PropertyField(m_propertyShaderList, new GUIContent("Shader list to find:"), true);

        m_shaderFinder.ApplyModifiedProperties();

        if (GUILayout.Button("Save shader list"))
        {
            SaveShaderList(ref m_shaderList);
        }

        m_typeOfSearch = (TypeOfSearch)EditorGUILayout.EnumPopup("Search for: ", m_typeOfSearch);


        if (GUILayout.Button("Check!"))
        {
            m_checkResults = findShaderInScene(m_typeOfSearch);

            Debug.Log("Find " + m_checkResults.Length + " results.");
        }

        if (m_checkResults != null)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical();
            for (int c = 0; c < m_checkResults.Length; c++)
            {
                AssetFinderResult result = m_checkResults[c];

                EditorGUILayout.LabelField(">" + result.m_gameObjectName + " - Material: " + result.m_materialName + " - Shader: " + result.m_shaderName);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }



    }

    private static readonly string m_keyListSize = "ShaderFinder_ListSize";
    private static readonly string m_keyListElem = "ShaderFinder_ListElem";


    void LoadShaderList(out Shader[] list)
    {
        int listSize = EditorPrefs.GetInt(m_keyListSize, 0);

        list = new Shader[listSize];

        for (int c = 0; c < listSize; c++)
        {
            string shaderName = EditorPrefs.GetString(m_keyListElem + c, "");
            list[c] = Shader.Find(shaderName);
        }

    }

    void SaveShaderList(ref Shader[] list)
    {

        EditorPrefs.SetInt(m_keyListSize, list.Length);

        int c = 0;
        foreach (Shader shader in list)
        {
            EditorPrefs.SetString(m_keyListElem + c++, shader.name);
        }
    }

    public bool checkForShadersInMaterial(Material mat)
    {
        foreach (Shader shader in m_shaderList)
        {
            if (mat.shader.name == shader.name)
                return true;
        }
        return false;
    }

    public AssetFinderResult[] findShaderInScene(TypeOfSearch search)
    {
        Renderer[] renderers;
        AssetFinder.FindAssetInScene<Renderer>(out renderers);

        List<AssetFinderResult> results = new List<AssetFinderResult>();

        bool kindOfSearch = search != TypeOfSearch.ObjectsContainingShadersInList;

        foreach (Renderer rend in renderers)
        {
            Material[] materials = rend.materials;
            foreach (Material mat in materials)
            {
                bool result = checkForShadersInMaterial(mat) ^ kindOfSearch;
                if (result)
                {
                    results.Add(new AssetFinderResult(rend.gameObject.name, mat.name, mat.shader.name));
                }
            }
        }

        return results.ToArray();
    }

}