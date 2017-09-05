// ScenaryShaderGUI.cs
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

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
internal class DragonShaderGUI : ShaderGUI {
    
    //------------------------------------------------------------------------//
    // CONSTANTS AND ENUMERATORS											  //
    //------------------------------------------------------------------------//

    public enum BlendMode
    {
        Opaque,
        Cutout,
        Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
    }
    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//


    private static class Styles
    {
/*
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _BumpMap ("Normal Map (RGB)", 2D) = "white" {}
        _NormalStrenght("Normal Strenght", float) = 1.0

        _DetailTex ("Detail (RGB)", 2D) = "white" {} // r -> inner light, g -> specular

        _Tint("Color Multiply", Color) = (1,1,1,1)
        _ColorAdd("Color Add", Color) = (0,0,0,0)

        _InnerLightAdd("Inner Light Add", float) = 0.0
        _InnerLightColor("Inner Light Color", Color) = (1,1,1,1)

        _SpecExponent("Specular Exponent", float) = 1.0
        _Fresnel("Fresnel factor", Range(0, 10)) = 1.5
        _FresnelColor("Fresnel Color", Color) = (1,1,1,1)
        _AmbientAdd("Ambient Add", Color) = (0,0,0,0)
        _SecondLightDir("Second Light dir", Vector) = (0,0,-1,0)
        _SecondLightColor("Second Light Color", Color) = (0.0, 0.0, 0.0, 0.0)

        _ReflectionMap("Reflection Map", Cube) = "white" {}
        _ReflectionAmount("Reflection amount", Range(0.0, 1.0)) = 0.0

        _InnerLightWavePhase("Inner Light Wave Phase", float) = 1.0
        _InnerLightWaveSpeed("Inner Light Wave Speed", float) = 1.0

        // Blending state
        [HideInInspector] _Mode("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0

        _StencilMask("Stencil Mask", int) = 10
*/
        readonly public static string mainTextureText = "Main Texture";
        readonly public static string detailTextureText = "Detail Texture";
        readonly public static string normalTextureText = "Normal Texture";

        readonly public static string colorMultiplyText = "Color Multiply";
        readonly public static string colorAddText = "Color Add";

        readonly public static string enableNormalMapText = "Enable Normal map";
        readonly public static string normalStrengthText = "Normal Texture strength";

        readonly public static string CutoffText = "Alpha cutoff threshold";

        readonly public static string enableSpecularText = "Enable Specular";
        readonly public static string specularPowerText = "Specular Exponent";
        readonly public static string secondLightDirectionText = "Second light direction";
        readonly public static string secondLightColorText = "Second light color";

        readonly public static string enableFresnelText = "Enable Fresnel";
        readonly public static string fresnelPowerText = "Fresnel Power";
        readonly public static string fresnelColorText = "Fresnel Color";

        readonly public static string additionalFXLayerText = "Additional FX layer";
        readonly public static string reflectionMapText = "Reflection Texture";
        readonly public static string reflectionAmountText = "Reflection amount";
        readonly public static string fireMapText = "Fire Texture";
        readonly public static string fireAmountText = "Fire amount";

        readonly public static string blendModeText = "Blend Mode";
        readonly public static string renderQueueText = "Render queue";

    }
    MaterialProperty mp_mainTexture;
    MaterialProperty mp_detailTexture;
    MaterialProperty mp_normalTexture;
    MaterialProperty mp_normalStrength;
    MaterialProperty mp_cutOff;

    MaterialProperty mp_tint;
    MaterialProperty mp_colorAdd;
    MaterialProperty mp_innerLightAdd;
    MaterialProperty mp_innerLightColor;
    MaterialProperty mp_specExponent;
    MaterialProperty mp_fresnel;
    MaterialProperty mp_fresnelColor;
    MaterialProperty mp_ambientAdd;
    MaterialProperty mp_secondLightDir;
    MaterialProperty mp_secondLightColor;

    MaterialProperty mp_reflectionMap;
    MaterialProperty mp_reflectionAmount;
    MaterialProperty mp_fireMap;
    MaterialProperty mp_fireAmount;


    MaterialProperty mp_innerLightWavePhase;
    MaterialProperty mp_innerLightWaveSpeed;

    MaterialProperty mp_BlendMode;


    /// <summary>
    /// Toggle Material Properties
    /// </summary>
    MaterialProperty mp_EnableSpecular;
    MaterialProperty mp_EnableNormalMap;

    MaterialProperty mp_EnableCutoff;
    MaterialProperty mp_EnableFresnel;
    MaterialProperty mp_EnableSilhouette;

    /// <summary>
    /// Enum Material PProperties
    /// </summary>

    MaterialProperty mp_FxLayer;
    MaterialProperty mp_SelfIlluminate;

    MaterialEditor m_materialEditor;
    ColorPickerHDRConfig m_ColorPickerHDRConfig = new ColorPickerHDRConfig(0f, 99f, 1 / 99f, 3f);

    readonly static string kw_blendTexture = "BLEND_TEXTURE";
    readonly static string kw_automaticBlend = "CUSTOM_VERTEXPOSITION";
    readonly static string kw_fog = "FOG";
    readonly static string kw_darken = "DARKEN";
    readonly static string kw_normalmap = "NORMALMAP";
    readonly static string kw_specular = "SPECULAR";
    readonly static string kw_cutOff = "CUTOFF";
    readonly static string kw_emissiveBlink = "EMISSIVEBLINK";

    private GUISkin editorSkin;
    private readonly static string editorSkinPath = "Assets/Engine/Shaders/Editor/GUISkin/MaterialEditorSkin.guiskin";

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
        mp_detailTexture = FindProperty("_DetailTex", props);
        mp_normalTexture = FindProperty("_BumpMap", props);
        mp_normalStrength = FindProperty("_NormalStrenght", props);
        mp_cutOff = FindProperty("_Cutoff", props);

        mp_specExponent = FindProperty("_SpecExponent", props);
        mp_secondLightDir = FindProperty("_SecondLightDir", props);
        mp_secondLightColor = FindProperty("_SecondLightColor", props);

        mp_tint = FindProperty("_Tint", props);
        mp_colorAdd = FindProperty("_ColorAdd", props);
        mp_innerLightAdd = FindProperty("_InnerLightAdd", props);
        mp_innerLightColor = FindProperty("_InnerLightColor", props);

        mp_fresnel = FindProperty("_Fresnel", props);
        mp_fresnelColor = FindProperty("_FresnelColor", props);
        mp_ambientAdd = FindProperty("_AmbientAdd", props);

        mp_reflectionMap = FindProperty("_ReflectionMap", props);
        mp_reflectionAmount = FindProperty("_ReflectionAmount", props);
        mp_fireMap = FindProperty("_FireMap", props);
        mp_fireAmount = FindProperty("_FireAmount", props);

        mp_innerLightWavePhase = FindProperty("_InnerLightWavePhase", props);
        mp_innerLightWaveSpeed = FindProperty("_InnerLightWaveSpeed", props);

        mp_BlendMode = FindProperty("_BlendMode", props);

        /// Toggle Material Properties

        mp_EnableSpecular = FindProperty("_EnableSpecular", props);
        mp_EnableNormalMap = FindProperty("_EnableNormalMap", props);
        mp_EnableCutoff = FindProperty("_EnableCutoff", props);
        mp_EnableFresnel = FindProperty("_EnableFresnel", props);
        mp_EnableSilhouette = FindProperty("_EnableSilhouette", props);

        /// Enum Material Properties

        mp_FxLayer = FindProperty("FXLayer", props);
        mp_SelfIlluminate = FindProperty("SelfIlluminate", props);
    }

    /// <summary>
    /// Draw the inspector.
    /// </summary>
    /// 
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        IniEditorSkin();
        FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
        m_materialEditor = materialEditor;
        Material material = materialEditor.target as Material;

        GUILayout.BeginHorizontal(editorSkin.customStyles[0]);
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Dragon standard shader", editorSkin.customStyles[0]);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical(editorSkin.customStyles[0]);
        materialEditor.ShaderProperty(mp_BlendMode, Styles.blendModeText);
        EditorGUILayout.EndVertical();

        int blendMode = (int)mp_BlendMode.floatValue;
        if (EditorGUI.EndChangeCheck())
        {
            setBlendMode(material, blendMode);
        }
        if (blendMode == 1)
        {
            materialEditor.ShaderProperty(mp_cutOff, Styles.CutoffText);
        }

        materialEditor.TextureProperty(mp_mainTexture, Styles.mainTextureText);
        GUILayout.BeginHorizontal();
        materialEditor.TextureProperty(mp_detailTexture, Styles.detailTextureText, false);
        materialEditor.TextureProperty(mp_normalTexture, Styles.normalTextureText, false);
        GUILayout.EndHorizontal();

        bool normalMap = mp_normalTexture.textureValue != null as Texture;

        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword(material, kw_normalmap, normalMap);
            EditorUtility.SetDirty(material);
            Debug.Log("EnableNormalMap " + (normalMap));
            //            DebugKeywords(material);
        }

        EditorGUI.BeginChangeCheck();

        if (normalMap)
        {
            materialEditor.ShaderProperty(mp_normalStrength, Styles.normalStrengthText);
        }

        if (featureSet(mp_EnableSpecular, Styles.enableSpecularText))
        {
            materialEditor.ShaderProperty(mp_specExponent, Styles.specularPowerText);
            materialEditor.ShaderProperty(mp_secondLightDir, Styles.secondLightDirectionText);
            materialEditor.ShaderProperty(mp_secondLightColor, Styles.secondLightColorText);
        }

        if (featureSet(mp_EnableFresnel, Styles.enableFresnelText))
        {
            materialEditor.ShaderProperty(mp_fresnel, Styles.fresnelPowerText);
            materialEditor.ShaderProperty(mp_fresnelColor, Styles.fresnelColorText);
        }

        featureSet(mp_FxLayer, Styles.additionalFXLayerText);
        int fxLayer = (int)mp_FxLayer.floatValue;

        switch(fxLayer)
        {
            case 1:
                materialEditor.TextureProperty(mp_reflectionMap, Styles.reflectionMapText, false);
                materialEditor.ShaderProperty(mp_reflectionAmount, Styles.reflectionAmountText);
                break;

            case 2:
                materialEditor.TextureProperty(mp_fireMap, Styles.fireMapText, false);
                materialEditor.ShaderProperty(mp_fireAmount, Styles.fireAmountText);
                break;
        }




        //        DebugKeywords(material);

        if (GUILayout.Button("Reset keywords"))
        {
            material.shaderKeywords = null;
        }

    }

    static void SetMaterialKeywords(Material material)
    {
        // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
        // (MaterialProperty value might come from renderer material property block)
        SetKeyword(material, "NORMALMAP", material.GetTexture("_BumpMap"));
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
        EditorGUILayout.BeginVertical(editorSkin.customStyles[0]);
        m_materialEditor.ShaderProperty(feature, label);
        EditorGUILayout.EndVertical();

        return feature.floatValue > 0.0f;
    }

    static void DebugKeywords(Material mat)
    {
        foreach (string kw in mat.shaderKeywords)
            Debug.Log("Material keywords: " + kw);
    }


    public static void setBlendMode(Material material, int blendMode)
    {
        material.SetFloat("_BlendMode", blendMode);

        switch (blendMode)
        {
            case 0:
                material.SetOverrideTag("RenderType", "Opaque");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.SetFloat("_ZWrite", 1.0f);
                ///                material.renderQueue = 2000;
                material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Back);
                material.DisableKeyword("CUTOFF");
                Debug.Log("Blend mode opaque");
                break;

            case 1:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                //                material.renderQueue = 3000;
                material.SetFloat("_ZWrite", 0.0f);
                material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
                material.EnableKeyword("CUTOFF");
                material.EnableKeyword("DOUBLESIDED");
                Debug.Log("Blend mode transparent");
                break;

            case 2:
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.SetFloat("_ZWrite", 1.0f);
                //                material.renderQueue = 2500;
                material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);

                Debug.Log("Blend mode cutout");
                break;
        }

    }

}