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


    static bool addVariant(ShaderVariantCollection svc, Shader shader, PassType type, List<string> keywords, List<string> currentVariant, int level)
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

            addVariant(svc, shader, type, keywords, currentVariant, level + 1);

            ShaderVariantCollection.ShaderVariant sv;
            sv.keywords = currentVariant.ToArray();
            sv.passType = type;
            sv.shader = shader;
            DebugVariant(shader, sv.keywords, svc.Add(sv));

            currentVariant.Remove(currentVariantToken);

        }
        return true;
/*
        ShaderVariantCollection.ShaderVariant sv;
        sv.keywords = keywords;

        Material m = new Material(shader);
        string lightMode = m.GetTag("LightMode", false, "Normal");

        try
        {
            sv.passType = (PassType)Enum.Parse(typeof(PassType), lightMode, true);
        }
        catch(ArgumentException)
        {
            Debug.Log(lightMode + " is not a valid light mode.");
            return false;
        }

        sv.shader = shader;
        return svc.Add(sv);
*/
        
    }

    static void DebugVariant(Shader shader, string[] keywords, bool succes)
    {
        string kw = "";
        for (int c = 0; c < keywords.Length; c++)
        {
            kw += keywords[c] + " ";
        }
        Debug.Log("Variant: " + shader.name + " with keywords: " + kw + ((succes) ? " added to collection." : "already exists."));
    }

    [MenuItem("Tools/Create shader caches")]
    static void CreateShaderCaches()
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
                            addVariant(svc, shader, (PassType)(-1), keywords, currentVariant, 0);
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
                addVariant(svc, shader, (PassType)(-1), keywords, currentVariant, 0);
                shader = null;
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