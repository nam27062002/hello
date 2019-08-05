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

    //    private static string shaderCacheSource = "shader_cache_source.txt";
    private static string materialDatabase = "material_database.txt";
    private static string shaderCacheExcludeList = "shader_cache_exclude_list.txt";

    private static string logFile = "shader_cache.log";

    private static bool OpenLogFile()
    {
        try
        {
            File.Delete(logFile);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static bool ResetMaterialDatabase()
    {
        try
        {
            File.Delete(materialDatabase);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }



    private static void Log(object msg)
    {
        try
        {
            StreamWriter swl = File.AppendText(logFile);
            swl.WriteLine(msg);
            swl.Close();
        }
        catch (Exception e)
        {
            Debug.Log("Exception :" + e);
        }
    }

    private static void insertMaterialDatabase(Material mat)
    {

    }

    private class ShaderContent
    {
        public string lightmode;
        public string[] keywords;

        public ShaderContent()
        {
            lightmode = null;
            keywords = null;
        }
        public ShaderContent(string lm, string[] kw)
        {
            lightmode = lm;
            keywords = kw;
        }
    }

    private static Dictionary<string, ShaderContent> m_shaderValidKeywords = new Dictionary<string, ShaderContent>();


    private static bool addVariant(ShaderVariantCollection svc, Shader shader, PassType type, List<string> keywords)
    {
        ShaderVariantCollection.ShaderVariant sv;
        sv.keywords = keywords.ToArray();
        sv.passType = type;
        sv.shader = shader;
        return svc.Add(sv);
    }

    private static void DebugVariant(Shader shader, string lightMode, string[] keywords, bool succes)
    {
        string kw = "";
        for (int c = 0; c < keywords.Length; c++)
        {
            kw += keywords[c] + " ";
        }
        Log("Variant: " + shader.name + " LightMode: " + lightMode + " with keywords: " + kw + ((succes) ? " added to collection." : "already exists."));
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

    
    private static char[] splitChars = new[] { ' ', '=', '{', '}', '"' };
    private static ShaderContent getShaderContent(Shader shader)
    {
        ShaderContent sc = new ShaderContent();
        if (!m_shaderValidKeywords.ContainsKey(shader.name))
        {
            Log("Shader name: " + shader.name);
            string shaderPath = AssetDatabase.GetAssetPath(shader);
            Log("Shader path: " + shaderPath);

            string[] shlines = null;

            try
            {
                shlines = File.ReadAllLines(shaderPath);
            }
            catch (FileNotFoundException)
            {
                shlines = new string[] { "" };
            }


            List<string> vkeywords = new List<string>();
            string lightmode = "Normal";

            foreach (string line in shlines)
            {
                if (line.Length > 0 && line[0] == '/') continue;

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
                    continue;
                }

                pos = line.IndexOf("LightMode");
                if (pos >= 0)
                {
                    off = 10;
                    string lightstr = line.Substring(pos + off);
                    string[] lm = lightstr.Split(splitChars);
                    if (lm.Length > 0)
                    {
                        for (int c = 0; c < lm.Length; c++)
                        {
                            if (lm[c].Length > 0)
                            {
                                lightmode = lm[c];
                                Debug.Log("LightMode = " + lm[c]);
                                break;
                            }
                        }
                    }
                }
            }

            sc.lightmode = lightmode;
            sc.keywords = vkeywords.ToArray();
//            ShaderContent sc = new ShaderContent(lightmode, vkeywords.ToArray());
            m_shaderValidKeywords[shader.name] = sc;

        }
        else
        {
            sc = m_shaderValidKeywords[shader.name];
        }
        return sc;
    }

    private static string[] stripMaterialKeywords(Material mat, out ShaderContent sc, bool modifyMaterial = false)
    {
        Shader shader = mat.shader;

        List<string> strippedKeywords = new List<string>();

        sc = getShaderContent(mat.shader);
        List<string> validKeywords = new List<string>(sc.keywords);

        foreach (string kw in mat.shaderKeywords)
        {
            if (validKeywords.Contains(kw))
            {
                strippedKeywords.Add(kw);
            }
            else
            {
                Log("Stripped keyword: " + kw);
            }
        }

        string[] newKeywordList = strippedKeywords.ToArray();

        if (modifyMaterial && (newKeywordList.Length != mat.shaderKeywords.Length))
        {
            mat.shaderKeywords = newKeywordList;
            EditorUtility.SetDirty(mat);
        }

        return newKeywordList;
    }
    private static string getLightMode(Material m)
    {
        string lightMode = m.GetTag("LightMode", false, "Normal");
        return lightMode;
    }

    [MenuItem("Tools/Create shader caches from materials")]
    static void CreateShaderCachesFromMaterials()
    {
        OpenLogFile();

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

                ShaderContent sc;               
                ShaderVariantCollection.ShaderVariant sv;

                string[] strippedKeywords = stripMaterialKeywords(m, out sc, true);
//                string lightMode = getLightMode(m);// m.GetTag("LightMode", false, "Normal");
                try
                {
                    sv.passType = (PassType)Enum.Parse(typeof(PassType), sc.lightmode, true);
                }
                catch (ArgumentException)
                {
                    Log(sc.lightmode + " is not a valid light mode.");
                    continue;
                }


                keywords.Clear();
                keywords.AddRange(strippedKeywords);
                keywords.Add(qualityVariants[quality]);
                sv.keywords = keywords.ToArray();
                sv.shader = m.shader;

                DebugVariant(sv.shader, sc.lightmode, sv.keywords, svc.Add(sv));
                EditorUtility.DisplayProgressBar("Stripping shader caches", "Processing materials", (float)(c + quality * materialList.Length) / ((float)materialList.Length * 3.0f));

            }
            string svcName = "SVC_" + qualityVariants[quality] + ".shadervariants";
            AssetDatabase.CreateAsset(svc, "Assets/Misc/ShaderVariantCollections/" + svcName);
        }


        Log("************************");
        Log("* COMPLETE SHADER LIST *");
        Log("************************");
        foreach (KeyValuePair<string, ShaderContent> pair in m_shaderValidKeywords)
        {
            Log(pair.Key);
        }

        EditorUtility.ClearProgressBar();
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