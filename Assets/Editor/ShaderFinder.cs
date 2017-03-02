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
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System;


//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor window.
/// </summary>
public class ShaderFinder : EditorWindow
{
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
    public static ShaderFinder instance
    {
        get
        {
            if (m_instance == null)
            {
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

    public enum SavelistSlot { List0, List1, List2, List3, List4, List5 };

    private WhereToSearch m_whereToSearch;
    private TypeOfSearch m_typeOfSearch;
    private SavelistSlot m_saveListSlot;

    //------------------------------------------------------------------//
    // METHODS															//
    //------------------------------------------------------------------//
    /// <summary>
    /// Opens the window.
    /// </summary>
    [MenuItem("Tools/Shader Finder")]

    public static void OpenWindow()
    {
        instance.Show();
    }

    /// <summary>
    /// The editor has been enabled - target object selected.
    /// </summary>
    private void OnEnable()
    {
        m_shaderFinder = new SerializedObject(this);
        m_propertyShaderList = m_shaderFinder.FindProperty("m_shaderList");

        string slotNumber;
        slotNumber = EditorPrefs.GetString(m_keySlotNumber, "List0");
        m_saveListSlot = (SavelistSlot)Enum.Parse(typeof(SavelistSlot), slotNumber);

        LoadShaderList("", out m_shaderList);
    }

    /// <summary>
    /// The editor has been disabled - target object unselected.
    /// </summary>
    private void OnDisable()
    {

    }

    private AssetFinderResult[] m_checkResults;
    private Vector2 scrollPos = Vector2.zero;
    /// <summary>
    /// Update the inspector window.
    /// </summary>
    public void OnGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        EditorGUILayout.LabelField("Shader finder can check for used shaders in scene hierarchy or assets folder.");
        m_whereToSearch = (WhereToSearch)EditorGUILayout.EnumPopup(m_whereToSearch);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.textField);
        EditorGUILayout.BeginHorizontal();
        m_saveListSlot = (SavelistSlot)EditorGUILayout.EnumPopup("List slot: ", m_saveListSlot);

        if (GUI.changed)
        {
            EditorPrefs.SetString(m_keySlotNumber, m_saveListSlot.ToString());
        }

        if (GUILayout.Button("Load list"))
        {
            LoadShaderList(m_saveListSlot.ToString(), out m_shaderList);
        }

        if (GUILayout.Button("Save list"))
        {
            string slotName = m_saveListSlot.ToString();
            SaveShaderList(slotName, ref m_shaderList);
        }

        if (GUILayout.Button("Delete list"))
        {
            DeleteShaderList(m_saveListSlot.ToString());
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.textField);
        m_shaderFinder.Update();
        //        EditorGUILayout.ObjectField(m_propertyShaderList);
        EditorGUILayout.PropertyField(m_propertyShaderList, new GUIContent("Shader list to find:"), true);

        m_shaderFinder.ApplyModifiedProperties();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.textField);
        m_typeOfSearch = (TypeOfSearch)EditorGUILayout.EnumPopup("Search for: ", m_typeOfSearch);
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Check!"))
        {
            if (m_whereToSearch == WhereToSearch.SearchInScene)
            {
                m_checkResults = findShaderInScene(m_typeOfSearch);
            }
            else
            {
                m_checkResults = findShaderInAssets(m_typeOfSearch);
            }

            Debug.Log("Find " + m_checkResults.Length + " results.");
        }

        if (m_checkResults != null)
        {
//            EditorGUILayout.BeginVertical(EditorStyles.textField, GUILayout.Height(1));
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.textField);
            EditorGUILayout.BeginVertical();
            for (int c = 0; c < m_checkResults.Length; c++)
            {
                AssetFinderResult result = m_checkResults[c];

                EditorGUILayout.LabelField(">" + result.m_gameObjectName + " - Material: " + result.m_materialName + " - Shader: " + result.m_shaderName);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
//            EditorGUILayout.EndVertical();
        }

    }

    private static readonly string m_keySlotNumber = "ShaderFinder_SlotNumber";
    private static readonly string m_keyListSize = "ShaderFinder_ListSize";
    private static readonly string m_keyListElem = "ShaderFinder_ListElem";


    void LoadShaderList(string slot, out Shader[] list)
    {
        int listSize = EditorPrefs.GetInt(m_keyListSize + slot, 0);

        list = new Shader[listSize];

        for (int c = 0; c < listSize; c++)
        {
            string shaderName = EditorPrefs.GetString(m_keyListElem + slot + c, "");
            list[c] = Shader.Find(shaderName);
        }

    }

    void SaveShaderList(string slot, ref Shader[] list)
    {
        EditorPrefs.SetInt(m_keyListSize + slot, list.Length);

        int c = 0;
        foreach (Shader shader in list)
        {
            EditorPrefs.SetString(m_keyListElem + slot + c++, shader.name);
        }
    }

    void DeleteShaderList(string slot)
    {
        int c = 0;
        bool exists = false;
        do
        {
            string strKey = m_keyListElem + slot + c++;
            if (EditorPrefs.HasKey(strKey))
            {
                EditorPrefs.DeleteKey(strKey);
                exists = true;
            }
            else
            {
                break;
            }

        } while (true);

        if (exists)
        {
            EditorPrefs.DeleteKey(m_keyListSize + slot);
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


    public AssetFinderResult[] findShaderInAssets(TypeOfSearch search)
    {
        List<Material> materialList;
        AssetFinder.FindAssetInContent<Material>(Directory.GetCurrentDirectory() + "\\Assets", out materialList);

        List<AssetFinderResult> results = new List<AssetFinderResult>();

        bool kindOfSearch = search != TypeOfSearch.ObjectsContainingShadersInList;

        foreach (Material mat in materialList)
        {
            bool result = checkForShadersInMaterial(mat) ^ kindOfSearch;
            if (result)
            {
                results.Add(new AssetFinderResult("", mat.name, mat.shader.name));
            }
        }

        return results.ToArray();
    }
}