// LightmapTool.cs
// Hungry Dragon
// 
// Created by Diego Campos on 04/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.IO;
using Object = UnityEngine.Object;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor window.
/// </summary>
public class ShaderCacheTool : EditorWindow {


    private static string[] qualityVariants =
    {
        "LOW_DETAIL_ON",
        "MEDIUM_DETAIL_ON",
        "HI_DETAIL_ON"
    };

    private static string shaderCacheSource = "shader_cache_source.txt";
    private static string shaderCacheExcludeList = "shader_cache_exclude_list.txt";

    private static Dictionary<string, string[]> m_shaderValidKeywords = new Dictionary<string, string[]>();

    private static bool addVariantRecursive(ShaderVariantCollection svc, Shader shader, PassType type, List<string> keywords, List<string> currentVariant, int level)
    {
        if (level >= keywords.Count) return false;


        if (type == (PassType)(-1))
        {
            Material m = new Material(shader);
            string lightMode = m.GetTag("LightMode", false, "Normal");

            try
            {
                type = (PassType)Enum.Parse(typeof(PassType), lightMode, true);
            }
            catch (ArgumentException)
            {
                Debug.Log(lightMode + " is not a valid light mode.");
                return false;
            }

        }


        string[] variantTokens = keywords[level].Split(' ');

        for (int c = 0; c < variantTokens.Length; c++)
        {
            string currentVariantToken = variantTokens[c];
            if (currentVariantToken != "_")
            {
                currentVariant.Add(currentVariantToken);
            }

            addVariantRecursive(svc, shader, type, keywords, currentVariant, level + 1);

            ShaderVariantCollection.ShaderVariant sv;
            sv.keywords = currentVariant.ToArray();
            sv.passType = type;
            sv.shader = shader;
            DebugVariant(shader, sv.keywords, svc.Add(sv));

            currentVariant.Remove(currentVariantToken);

        }
        return true;
    }

    private static bool addVariant(ShaderVariantCollection svc, Shader shader, PassType type, List<string> keywords)
    {
        ShaderVariantCollection.ShaderVariant sv;
        sv.keywords = keywords.ToArray();
        sv.passType = type;
        sv.shader = shader;
        return svc.Add(sv);
    }

    private static void DebugVariant(Shader shader, string[] keywords, bool succes)
    {
        string kw = "";
        for (int c = 0; c < keywords.Length; c++)
        {
            kw += keywords[c] + " ";
        }
        Debug.Log("Variant: " + shader.name + " with keywords: " + kw + ((succes) ? " added to collection." : "already exists."));
    }


    private static string[] excludeList;
    private static void loadShaderCacheExcludeList()
    {
        excludeList = File.ReadAllLines(shaderCacheExcludeList);
    }

    private static bool excludeShader(Shader shader)
    {
        if (excludeList == null) return false;
        for (int c = 0; c < excludeList.Length; c++)
        {
            if (shader.name == excludeList[c]) return true;
        }
        return false;
    }


    [MenuItem("Tools/Create shader caches recursive")]
    static void CreateShaderCachesRecursive()
    {
        string[] csline = File.ReadAllLines(shaderCacheSource);
        if (csline == null)
        {
            Debug.Log("Unable to find shader cache source file.");
            return;
        }

        for (int quality = 0; quality < qualityVariants.Length; quality++)
        {
            ShaderVariantCollection svc = new ShaderVariantCollection();

            int idx = 0;
            Shader shader = null;
            List<string> keywords = new List<string>();
            List<string> currentVariant = new List<string>();

            while (idx < csline.Length)
            {
                if (csline[idx].Length > 0)
                {
                    if (csline[idx][0] == 'S')
                    {
                        if (shader != null)
                        {
                            addVariantRecursive(svc, shader, (PassType)(-1), keywords, currentVariant, 0);
                            shader = null;
                        }
                        keywords.Clear();
                        keywords.Add(qualityVariants[quality]);

                        string shaderName = csline[idx].Substring(2);

                        shader = Shader.Find(shaderName);
                        if (shader == null)
                        {
                            Debug.Log("Unable to find shader: " + shaderName);
                            return;
                        }

                    }
                    else if (csline[idx][0] == 'V')
                    {
                        string keywordVariation = csline[idx].Substring(2);
                        keywords.Add(keywordVariation);
                    }
                }

                idx++;
            }

            if (shader != null)
            {
                addVariantRecursive(svc, shader, (PassType)(-1), keywords, currentVariant, 0);
                shader = null;
            }

            string svcName = "SVC_" + qualityVariants[quality] + ".shadervariants";
            AssetDatabase.CreateAsset(svc, "Assets/Misc/ShaderVariantCollections/" + svcName);

        }

    }


    private static string[] getShaderValidKeywords(Shader shader)
    {
        string[] validKeywords = null;
        if (!m_shaderValidKeywords.ContainsKey(shader.name))
        {
            string shaderPath = AssetDatabase.GetAssetPath(shader);
            Debug.Log("Shader path: " + shaderPath);

            string[] shlines = File.ReadAllLines(shaderPath);
            List<string> vkeywords = new List<string>();

            foreach (string line in shlines)
            {
                int off = 0;
                int pos = line.IndexOf("shader_feature");
                if (pos == -1)
                {
                    pos = line.IndexOf("multi_compile");
                    if (pos != -1)
                    {
                        off = 13;
                    }
                }
                else
                {
                    off = 14;
                }

                if (pos >= 0)
                {
                    string kwpack = line.Substring(pos + off);
                    string[] kwl = kwpack.Split(' ');

                    for (int c = 0; c < kwl.Length; c++)
                    {
                        if (kwl[c].Length > 0 && kwl[c][0] != '_')
                        {
                            vkeywords.Add(kwl[c]);
                        }
                    }
                }
            }

            validKeywords = vkeywords.ToArray();

            string ds = "";
            foreach(string vk in validKeywords)
            {
                ds += vk + " ";
            }


            Debug.Log("Valid keywords: " + ds);

            m_shaderValidKeywords[shader.name] = validKeywords;

        }
        else
        {
            validKeywords = m_shaderValidKeywords[shader.name];
        }
        return validKeywords;
    }

    private static string[] stripMaterialKeywords(Material mat)
    {
        Shader shader = mat.shader;

        List<string> strippedKeywords = new List<string>();

        List<string> validKeywords = new List<string>(getShaderValidKeywords(mat.shader));

        foreach (string kw in mat.shaderKeywords)
        {
            if (validKeywords.Contains(kw))
            {
                strippedKeywords.Add(kw);
            }
            else
            {
                Debug.Log("Stripped keyword: " + kw);
            }
        }
        return strippedKeywords.ToArray();
    }

    [MenuItem("Tools/Create shader caches from materials")]
    static void CreateShaderCachesFromMaterials()
    {
        Material[] materialList;
        AssetFinder.FindAssetInContent<Material>(Directory.GetCurrentDirectory() + "\\Assets", out materialList);
        m_shaderValidKeywords.Clear();

        loadShaderCacheExcludeList();

        List<string> keywords = new List<string>();

        for (int quality = 0; quality < qualityVariants.Length; quality++)
        {
            ShaderVariantCollection svc = new ShaderVariantCollection();

            for (int c = 0; c < materialList.Length; c++)
            {
                Material m = materialList[c];

                if (excludeShader(m.shader)) continue;

                ShaderVariantCollection.ShaderVariant sv;

                string lightMode = m.GetTag("LightMode", false, "Normal");
                try
                {
                    sv.passType = (PassType)Enum.Parse(typeof(PassType), lightMode, true);
                }
                catch (ArgumentException)
                {
                    Debug.Log(lightMode + " is not a valid light mode.");
                    continue;
                }

                string[] validKeywords = getShaderValidKeywords(m.shader);


                keywords.Clear();
                keywords.AddRange(m.shaderKeywords);
                keywords.Add(qualityVariants[quality]);
                sv.keywords = keywords.ToArray();
                sv.shader = m.shader;

                DebugVariant(sv.shader, sv.keywords, svc.Add(sv));
            }
            string svcName = "SVC_" + qualityVariants[quality] + ".shadervariants";
            AssetDatabase.CreateAsset(svc, "Assets/Misc/ShaderVariantCollections/" + svcName);
        }
    }


    //------------------------------------------------------------------//
    // CONSTANTS														//
    //------------------------------------------------------------------//

        //------------------------------------------------------------------//
        // MEMBERS AND PROPERTIES											//
        //------------------------------------------------------------------//
        // Window instance
    private static ShaderCacheTool m_instance = null;
	public static ShaderCacheTool instance {
		get {
			if(m_instance == null) {
				m_instance = (ShaderCacheTool)EditorWindow.GetWindow(typeof(ShaderCacheTool));
			}
			return m_instance;
		}
	}

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Opens the window.
	/// </summary>
	//[MenuItem("Hungry Dragon/Tools/TemplateEditorWindow")]	// UNCOMMENT TO ADD MENU ENTRY!!!
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