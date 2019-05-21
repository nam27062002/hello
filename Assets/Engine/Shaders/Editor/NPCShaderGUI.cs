// NPCShaderGUI.cs
// Hungry Dragon
// 
// Created by Diego Campos on 26/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.IO;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
internal class NPCDiffuseShaderGUI : ShaderGUI
{
    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    private static class Styles
    {
        readonly public static string mainTextureText = "Albedo Texture";
        readonly public static string colorModeText = "Color Mode";

        readonly public static string tintColorText = "Tint Color";
        readonly public static string tint1ColorText = "Tint1 Color";
        readonly public static string tint2ColorText = "Tint2 Color";
        readonly public static string rampColorText = "Ramp Texture";
        readonly public static string renderQueueText = "Render queue";
        readonly public static string stencilMaskText = "Stencil mask";
        readonly public static string cullModeText = "Cull Mode";
        readonly public static string opaqueAlphaText = "Opaque alpha";
    }

    MaterialProperty mp_mainTexture;
    MaterialProperty mp_Tint1Color;
    MaterialProperty mp_Tint2Color;
    MaterialProperty mp_RampColor;
    MaterialProperty mp_ColorMode;
    MaterialProperty mp_stencilMask;
    MaterialProperty mp_cullMode;
    MaterialProperty mp_opaqueAlpha;

    MaterialEditor m_materialEditor;
    
    readonly static int m_labelWidth = 150;

    private GUISkin editorSkin;
    private readonly static string editorSkinPath = "Assets/Engine/Shaders/Editor/GUISkin/MaterialEditorSkin.guiskin";

    private bool m_npcDiffuseTransparent = false;

    //------------------------------------------------------------------------//
    // METHODS																  //
    //------------------------------------------------------------------------//
    void IniEditorSkin()
    {
        if (editorSkin == null)
        {
            editorSkin = AssetDatabase.LoadAssetAtPath(editorSkinPath, typeof(GUISkin)) as GUISkin;
        }
    }

    public void FindProperties(MaterialProperty[] props)
    {
        mp_mainTexture = FindProperty("_MainTex", props);
        mp_Tint1Color = FindProperty("_Tint1", props);
        mp_Tint2Color = FindProperty("_Tint2", props);
        mp_RampColor = FindProperty("_RampTex", props);
        mp_ColorMode = FindProperty("ColorMode", props);

        mp_stencilMask = FindProperty("_StencilMask", props);
        if (m_npcDiffuseTransparent)
        {
            mp_cullMode = FindProperty("_Cull", props);
            mp_opaqueAlpha = FindProperty("_OpaqueAlpha", props);
        }
    }

    /// <summary>
    /// Draw the inspector.
    /// </summary>
    /// 
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        m_materialEditor = materialEditor;
        Material material = materialEditor.target as Material;
        m_npcDiffuseTransparent = material.shader.name.Contains("Transparent");

        IniEditorSkin();
        FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly

        GUILayout.BeginHorizontal(editorSkin.customStyles[3]);
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("NPC Diffuse standard shader", editorSkin.customStyles[3]);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        materialEditor.TextureProperty(mp_mainTexture, Styles.mainTextureText);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical(editorSkin.customStyles[3]);
        materialEditor.ShaderProperty(mp_ColorMode, Styles.colorModeText);
        EditorGUILayout.EndVertical();

        int colorMode = (int)mp_ColorMode.floatValue;

        switch(colorMode)
        {
            case 1:
                materialEditor.ShaderProperty(mp_Tint1Color, Styles.tintColorText);
                break;

            case 2:
                materialEditor.ShaderProperty(mp_Tint1Color, Styles.tint1ColorText);
                materialEditor.ShaderProperty(mp_Tint2Color, Styles.tint2ColorText);
                break;

            case 3:
            case 4:
                materialEditor.TextureProperty(mp_RampColor, Styles.rampColorText, false);
                break;

        }

        EditorGUILayout.BeginVertical(editorSkin.customStyles[3]);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(Styles.renderQueueText);
        int renderQueue = EditorGUILayout.IntField(material.renderQueue);
        if (material.renderQueue != renderQueue)
        {
            material.renderQueue = renderQueue;
        }
        EditorGUILayout.EndHorizontal();

        if (m_npcDiffuseTransparent)
        {
            materialEditor.ShaderProperty(mp_opaqueAlpha, Styles.opaqueAlphaText);
            materialEditor.ShaderProperty(mp_cullMode, Styles.cullModeText);
        }

        materialEditor.ShaderProperty(mp_stencilMask, Styles.stencilMaskText);
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Log keywords", editorSkin.customStyles[3]))
        {
            //            material.shaderKeywords = null;
            DebugKeywords(material);
        }
/*
        if (GUILayout.Button("Reset keywords"))
        {
            material.shaderKeywords = null;
        }
*/
    }

    static void SetKeyword(Material m, string keyword, bool state)
    {
        if (state)
            m.EnableKeyword(keyword);
        else
            m.DisableKeyword(keyword);
    }


    private bool featureSet(MaterialProperty feature, string label)
    {
        EditorGUILayout.BeginVertical(editorSkin.customStyles[1]);
        m_materialEditor.ShaderProperty(feature, label);
        EditorGUILayout.EndVertical();

        return feature.floatValue > 0.0f;
    }

    static void DebugKeywords(Material mat)
    {
        foreach (string kw in mat.shaderKeywords)
            Debug.Log("Material keywords: " + kw);
    }

}