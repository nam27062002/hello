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
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Object = UnityEngine.Object;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor window.
/// </summary>
public class LightmapTool : EditorWindow {

 
    public static class LightingSettingsHepler
    {
        public static void SetIndirectResolution(float val)
        {
            SetFloat("m_LightmapEditorSettings.m_Resolution", val);
        }

        public static void SetMaxAtlasResolution(int val)
        {
            SetInt("m_LightmapEditorSettings.m_TextureWidth", val);
            SetInt("m_LightmapEditorSettings.m_TextureHeight", val);
        }


        public static void SetAmbientOcclusion(float val)
        {
            SetFloat("m_LightmapEditorSettings.m_CompAOExponent", val);
        }

        public static void SetBakedGiEnabled(bool enabled)
        {
            SetBool("m_GISettings.m_EnableBakedLightmaps", enabled);
        }

        public static void SetFinalGatherEnabled(bool enabled)
        {
            SetBool("m_LightmapEditorSettings.m_FinalGather", enabled);
        }

        public static void SetFinalGatherRayCount(int val)
        {
            SetInt("m_LightmapEditorSettings.m_FinalGatherRayCount", val);
        }

        public static void SetFloat(string name, float val)
        {
            ChangeProperty(name, property => property.floatValue = val);
        }

        public static void SetInt(string name, int val)
        {
            ChangeProperty(name, property => property.intValue = val);
        }

        public static void SetBool(string name, bool val)
        {
            ChangeProperty(name, property => property.boolValue = val);
        }

        public static void ChangeProperty(string name, Action<SerializedProperty> changer)
        {
            SerializedObject lightmapSettings = getLightmapSettings();
            SerializedProperty prop = lightmapSettings.FindProperty(name);
            if (prop != null)
            {
                changer(prop);
                lightmapSettings.ApplyModifiedProperties();
            }
            else Debug.LogError("lighmap property not found: " + name);
        }

        public static SerializedProperty GetProperty(string name)
        {
            SerializedObject lightmapSettings = getLightmapSettings();
            SerializedProperty prop = lightmapSettings.FindProperty(name);
            if (prop != null)
            {
                return prop;
            }
            else Debug.LogError("lighmap property not found: " + name);
            return null;
        }

        public static SerializedObject getLightmapSettings()
        {
            MethodInfo getLightmapSettingsMethod = typeof(LightmapEditorSettings).GetMethod("GetLightmapSettings", BindingFlags.Static | BindingFlags.NonPublic);
            Object lightmapSettings = getLightmapSettingsMethod.Invoke(null, null) as Object;
            return new SerializedObject(lightmapSettings);
        }

        public static void Test()
        {
            SetBakedGiEnabled(true);
            SetIndirectResolution(1.337f);
            SetAmbientOcclusion(1.337f);
            SetFinalGatherEnabled(true);
            SetFinalGatherRayCount(1337);
        }
    }

    [MenuItem("Tools/Set Lightmap properties")]
    static void SetLightmapProperties()
    {

        SerializedObject so = LightingSettingsHepler.getLightmapSettings();

        SerializedProperty sp = so.GetIterator();

        while (sp.Next(true))
        {
            Debug.Log("Property ---> path: " + sp.propertyPath + " type: " + sp.propertyType + " displayName: " + sp.displayName);
        }


        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded)
            {
                SceneManager.SetActiveScene(s);
                LightingSettingsHepler.SetMaxAtlasResolution(1024);
            }
        }
    }

    //------------------------------------------------------------------//
    // CONSTANTS														//
    //------------------------------------------------------------------//

    //------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES											//
    //------------------------------------------------------------------//
    // Window instance
    private static LightmapTool m_instance = null;
	public static LightmapTool instance {
		get {
			if(m_instance == null) {
				m_instance = (LightmapTool)EditorWindow.GetWindow(typeof(LightmapTool));
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