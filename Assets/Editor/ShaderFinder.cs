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
using SimpleJSON;
//using System;


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

    private static readonly string m_keySlotNumber = "ShaderFinder_SlotNumber";
    private static readonly string m_keyListSize = "ShaderFinder_ListSize";
    private static readonly string m_keyListElem = "ShaderFinder_ListElem";

    public class AssetFinderResult
    {
        public GameObject m_gameObject;
        public Material m_material;
        public Shader m_shader;

        public AssetFinderResult(GameObject gameobject, Material materialName, Shader shaderName)
        {
            m_gameObject = gameobject;
            m_material = materialName;
            m_shader = shaderName;
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
    private SerializedObject m_shaderFinder;

    [SerializeField]
    public Shader[] m_shaderList = null;
    private SerializedProperty m_propertyShaderList;

    [SerializeField]
    private Shader m_replaceWithShader = null;
    private SerializedProperty m_propertyReplaceWithShader;

    private bool m_searchScene = true;


    public enum WhereToSearch { SearchInScene, SearchInAssets };

    public enum TypeOfSearch { ObjectsContainingShadersInList, ObjectNotContainingShadersInList, ReplaceShader  };

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
        m_propertyReplaceWithShader = m_shaderFinder.FindProperty("m_replaceWithShader");

        string slotNumber;
        slotNumber = EditorPrefs.GetString(m_keySlotNumber, "List0");
        m_saveListSlot = (SavelistSlot)System.Enum.Parse(typeof(SavelistSlot), slotNumber);

        LoadShaderList(m_saveListSlot.ToString(), out m_shaderList);
    }

    /// <summary>
    /// The editor has been disabled - target object unselected.
    /// </summary>
    private void OnDisable()
    {

    }


    private void SelectObject(Object obj)
    {
        Object[] selection = new Object[1];
        selection[0] = obj;
        Selection.objects = selection;
    }


    private AssetFinderResult[] m_checkResults;
    private Vector2 scrollPos = Vector2.zero;

    /// <summary>
    /// Update the inspector window.
    /// </summary>
    public void OnGUI()
    {
        m_shaderFinder.Update();
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        EditorGUILayout.LabelField("Shader finder can check for used shaders in scene hierarchy or assets folder.");
        m_whereToSearch = (WhereToSearch)EditorGUILayout.EnumPopup(m_whereToSearch);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.textField);
        EditorGUILayout.BeginHorizontal();
        GUI.changed = false;
        m_saveListSlot = (SavelistSlot)EditorGUILayout.EnumPopup("List slot: ", m_saveListSlot);

        if (GUI.changed)
        {
            EditorPrefs.SetString(m_keySlotNumber, m_saveListSlot.ToString());
            LoadShaderList(m_saveListSlot.ToString(), out m_shaderList);
        }
/*
        if (GUILayout.Button("Load list"))
        {
            LoadShaderList(m_saveListSlot.ToString(), out m_shaderList);
        }
*/
        if (GUILayout.Button("Save list"))
        {
            string slotName = m_saveListSlot.ToString();
            SaveShaderList(slotName, ref m_shaderList);
        }

        if (GUILayout.Button("Delete list"))
        {
            DeleteShaderList(m_saveListSlot.ToString());
        }

        if (GUILayout.Button("Import list"))
        {
            importShaderList(out m_shaderList);
        }

        if (GUILayout.Button("Export list"))
        {
            exportShaderList(ref m_shaderList);
        }


        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.textField);
        if (EditorGUILayout.PropertyField(m_propertyShaderList, new GUIContent("Shader list to find:")))
        {
            for (int c = 0; c < m_propertyShaderList.arraySize; c++)
            {
                EditorGUILayout.BeginHorizontal();
                SerializedProperty elementProperty = m_propertyShaderList.GetArrayElementAtIndex(c);
                EditorGUILayout.PropertyField(elementProperty, new GUIContent(""), true);

                if (c < 1)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("/\\", GUILayout.Width(50.0f)))
                {
                    m_propertyShaderList.MoveArrayElement(c, c - 1);
                }
                GUI.enabled = true;

                if (c >= m_propertyShaderList.arraySize - 1)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button("\\/", GUILayout.Width(50.0f)))
                {
                    m_propertyShaderList.MoveArrayElement(c, c + 1);
                }
                GUI.enabled = true;

                if (GUILayout.Button("-", GUILayout.Width(50.0f)))
                {
                    m_propertyShaderList.DeleteArrayElementAtIndex(c);
                }


                EditorGUILayout.EndHorizontal();

            }
            if (GUILayout.Button("Add shader to list"))
            {
                m_propertyShaderList.arraySize = m_propertyShaderList.arraySize + 1;
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.textField);
        m_typeOfSearch = (TypeOfSearch)EditorGUILayout.EnumPopup("Search for: ", m_typeOfSearch);

        if (m_typeOfSearch == TypeOfSearch.ReplaceShader)
        {
            EditorGUILayout.PropertyField(m_propertyReplaceWithShader, new GUIContent("Replace with shader:"));
        }
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

        }

        if (m_checkResults != null && m_checkResults.Length > 0)
        {
            bool gameObjectsInList = m_checkResults[0].m_gameObject != null;
            float buttonWidth = (position.size.x - 40.0f) * (gameObjectsInList ? 0.33333f : 0.5f);
            EditorGUILayout.BeginVertical(EditorStyles.textField);
            EditorGUILayout.LabelField("Shader finder found: " + m_checkResults.Length + " results.");
            EditorGUILayout.BeginHorizontal();
            if (gameObjectsInList) {
                EditorGUILayout.LabelField("GameObject", EditorStyles.textField, GUILayout.Width(buttonWidth));
            }
            EditorGUILayout.LabelField("Material", EditorStyles.textField, GUILayout.Width(buttonWidth));
            EditorGUILayout.LabelField("Shader", EditorStyles.textField, GUILayout.Width(buttonWidth));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

//          EditorGUILayout.BeginVertical(EditorStyles.textField, GUILayout.Height(1));
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.textField);
            EditorGUILayout.BeginVertical();
            for (int c = 0; c < m_checkResults.Length; c++)
            {
                AssetFinderResult result = m_checkResults[c];
                if (result.m_material == null) continue;
                EditorGUILayout.BeginHorizontal();

                if (result.m_gameObject != null && GUILayout.Button(result.m_gameObject.name, GUILayout.Width(buttonWidth)))
                {
                    SelectObject(result.m_gameObject);
                }
                if (GUILayout.Button(result.m_material.name, GUILayout.Width(buttonWidth)))
                {
                    SelectObject(result.m_material);
                }
                if (GUILayout.Button(result.m_shader.name, GUILayout.Width(buttonWidth)))
                {
                    SelectObject(result.m_shader);
                }

                //                EditorGUILayout.LabelField(">" + ((result.m_gameObject != null) ? result.m_gameObject.name: "") + " - Material: " + result.m_material.name + " - Shader: " + result.m_shader.name);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            if (m_typeOfSearch == TypeOfSearch.ReplaceShader && m_replaceWithShader != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.textField);
                if (GUILayout.Button("Replace list with shader: " + m_replaceWithShader.name))
                {
                    if (EditorUtility.DisplayDialog("Replace list with shader", "Warning! You can't UNDO this action. Are you sure?", "Go ahead!", "May be later..."))
                    {
                        replaceMaterialShader(ref m_checkResults, ref m_replaceWithShader);
//                        m_checkResults = null;
                    }
                }
                EditorGUILayout.EndVertical();

            }
            //          EditorGUILayout.EndVertical();
        }

        m_shaderFinder.ApplyModifiedProperties();
    }


    void importShaderList(out Shader[] list)
    {
        List<Shader> tempList = new List<Shader>();

        string path = EditorUtility.OpenFilePanel("Shaderlist", Directory.GetCurrentDirectory() + "\\Assets\\Editor\\ShaderLists", "shaderList");
        if (path.Length != 0)
        {
//            JSONNode node = JSONNode.LoadFromFile(path);
            JSONNode node = JSONNode.LoadFromFile(path);
            JSONArray array = node["ShaderList"].AsArray;
            for (int c = 0; c < array.Count; c++)
            {
                Shader tempShader = Shader.Find(array[c]);
                if (tempShader != null)
                {
                    tempList.Add(tempShader);
                }
            }
        }

        list = tempList.ToArray();
    }

    void exportShaderList(ref Shader[] list)
    {
        string path = EditorUtility.SaveFilePanel("Shaderlist", Directory.GetCurrentDirectory() + "\\Assets\\Editor\\ShaderLists", "default", "shaderList");
        if (path.Length != 0)
        {
            JSONNode jsonData = new JSONClass();

            SimpleJSON.JSONArray shaderList = new SimpleJSON.JSONArray();
            foreach (Shader shader in list)
            {
                shaderList.Add(shader.name);
            }

            jsonData.Add("ShaderList", shaderList);

            jsonData.SaveToFile(path);

/*
            node.Add(m_keyListSize, list.Length.ToString());
            for (int c = 0; c < list.Length; c++)
            {
                node.Add(m_keyListElem)
            }
*/
        }

    }

    void LoadShaderList(string slot, out Shader[] list)
    {
        int listSize = EditorPrefs.GetInt(m_keyListSize + slot, 0);

        //        list = new Shader[listSize];
        List<Shader> localList = new List<Shader>();

        for (int c = 0; c < listSize; c++)
        {
            string shaderName = EditorPrefs.GetString(m_keyListElem + slot + c, "");
            Shader tempShader = Shader.Find(shaderName);
            if (tempShader != null)
            {
                localList.Add(tempShader);
            }
        }
        list = localList.ToArray();

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

        bool kindOfSearch = (search == TypeOfSearch.ObjectNotContainingShadersInList);

        foreach (Renderer rend in renderers)
        {
            Material[] materials = rend.sharedMaterials;
            foreach (Material mat in materials)
            {
                if (mat == null)
                {
                    Debug.Log("Object: " + rend.gameObject + " has a null Material!");
                    continue;
                }
                bool result = checkForShadersInMaterial(mat) ^ kindOfSearch;
                if (result)
                {
                    results.Add(new AssetFinderResult(rend.gameObject, mat, mat.shader));
                }
            }
        }

        return results.ToArray();
    }


    public AssetFinderResult[] findShaderInAssets(TypeOfSearch search)
    {
        Material[] materialList;
        AssetFinder.FindAssetInContent<Material>(Directory.GetCurrentDirectory() + "\\Assets", out materialList);

        List<AssetFinderResult> results = new List<AssetFinderResult>();

        bool kindOfSearch = (search == TypeOfSearch.ObjectNotContainingShadersInList);

        for (int c = 0; c < materialList.Length; c++)
        {
            bool result = checkForShadersInMaterial(materialList[c]) ^ kindOfSearch;
            if (result)
            {
                results.Add(new AssetFinderResult(null, materialList[c], materialList[c].shader));
            }
        }

        return results.ToArray();
    }


    public void replaceMaterialShader(ref AssetFinderResult[] materialList, ref Shader replaceWithShader)
    {
        Dictionary<string, AssetFinderResult> mapList = new Dictionary<string, AssetFinderResult>();
        for (int c = 0; c < materialList.Length; c++)
        {
            AssetFinderResult res = materialList[c];
            mapList[res.m_material.name] = res;
        }

        foreach (KeyValuePair<string, AssetFinderResult> pair in mapList)
        {
            pair.Value.m_material.shader = replaceWithShader;
            EditorUtility.SetDirty(pair.Value.m_material);
        }

        materialList = new AssetFinderResult[mapList.Count];
        mapList.Values.CopyTo(materialList, 0);

        AssetDatabase.SaveAssets();


    }
}