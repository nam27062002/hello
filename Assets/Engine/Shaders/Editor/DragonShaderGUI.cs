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
        public static string mainTextureText = "MainTex";
        public static string blendTextureText = "Blend Texture";

        public static string normalTextureText = "Normal Texture";
        public static string normalStrengthText = "Normal Texture strength";
        public static GUIContent alphaCutoffText = new GUIContent("Alpha Cutoff", "Threshold for alpha cutoff");

//        public static GUIContent fogText = new GUIContent("Fog");
///        public static GUIContent darkenText = new GUIContent("Darken");

        public static string fogText = "Fog";
        public static string darkenText = "Darken";
        public static string specularText = "Specular";
        public static string automaticBlendingText = "Automatic blending";
        public static string overlayColorText = "Vertex Color Tint";

        public static string specularFactorText = "Specular factor:";
        public static string specularDirText = "Specular direction:";

        public static string darkenPositionText = "Darken position:";
        public static string darkenDistanceText = "Darken distance:";

        public static string whiteSpaceString = " ";
        public static string primaryMapsText = "Maps";
        public static string renderOptions = "Render Options";
        public static string renderingMode = "Rendering Mode";
//        public static GUIContent emissiveWarning = new GUIContent("Emissive value is animated but the material has not been configured to support emissive. Please make sure the material itself has some amount of emissive.");
//        public static GUIContent emissiveColorWarning = new GUIContent("Ensure emissive color is non-black for emission to have effect.");
        public static readonly string[] blendNames = Enum.GetNames(typeof(BlendMode));
    }

    MaterialProperty mp_mainTexture;
    MaterialProperty mp_cutOff;
    MaterialProperty mp_normalTexture;
    MaterialProperty mp_normalStrength;
    MaterialProperty mp_detailTexture;

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
    MaterialProperty mp_innerLightWavePhase;
    MaterialProperty mp_innerLightWaveSpeed;

    MaterialProperty mp_blendMode;

    MaterialEditor m_MaterialEditor;
    ColorPickerHDRConfig m_ColorPickerHDRConfig = new ColorPickerHDRConfig(0f, 99f, 1 / 99f, 3f);

    bool m_FirstTimeApply = true;


    //------------------------------------------------------------------------//
    // METHODS																  //
    //------------------------------------------------------------------------//
    public void FindProperties(MaterialProperty[] props)
    {
        mp_mainTexture = FindProperty("_MainTex", props);
        mp_cutOff = FindProperty("_Cutoff", props);
        mp_normalTexture = FindProperty("_BumpMap", props);
        mp_normalStrength = FindProperty("_NormalStrenght", props);
        mp_detailTexture = FindProperty("_DetailTex", props);

        mp_tint = FindProperty("_Tint", props);
        mp_colorAdd = FindProperty("_ColorAdd", props);
        mp_innerLightAdd = FindProperty("_InnerLightAdd", props);
        mp_innerLightColor = FindProperty("_InnerLightColor", props);
        mp_specExponent = FindProperty("_SpecExponent", props);
        mp_fresnel = FindProperty("_Fresnel", props);
        mp_fresnelColor = FindProperty("_FresnelColor", props);
        mp_ambientAdd = FindProperty("_AmbientAdd", props);
        mp_secondLightDir = FindProperty("_SecondLightDir", props);
        mp_secondLightColor = FindProperty("_SecondLightColor", props);

        mp_reflectionMap = FindProperty("_ReflectionMap", props);
        mp_reflectionMap = FindProperty("_ReflectionAmount", props);
        mp_innerLightWavePhase = FindProperty("_InnerLightWavePhase", props);
        mp_innerLightWaveSpeed = FindProperty("_InnerLightWaveSpeed", props);

        mp_blendMode = FindProperty("_Mode", props);
    }

    /// <summary>
    /// Draw the inspector.
    /// </summary>
    /// 
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
        m_MaterialEditor = materialEditor;
        Material material = materialEditor.target as Material;

        // Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing
        // material to a standard shader.
        // Do this before any GUI code has been issued to prevent layout issues in subsequent GUILayout statements (case 780071)
        if (m_FirstTimeApply)
        {
            MaterialChanged(material);
            m_FirstTimeApply = false;
        }

        ShaderPropertiesGUI(material);
/*
        if (GUILayout.Button("Reset keywords"))
        {
            material.shaderKeywords = null;
        }
*/
    }

    static void MaterialChanged(Material material)
    {
//        material.shaderKeywords = null;

        SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));

        SetMaterialKeywords(material);


        string kwl = "";
        foreach (string kw in material.shaderKeywords)
        {
            kwl += kw + ",";
        }

        Debug.Log("Material keywords: " + kwl);

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

    public void ShaderPropertiesGUI(Material material)
    {
        // Use default labelWidth
        EditorGUIUtility.labelWidth = 0f;

        // Detect any changes to the material
        EditorGUI.BeginChangeCheck();
        {
            BlendModePopup();

            // Primary properties
            GUILayout.Label(Styles.primaryMapsText, EditorStyles.boldLabel);

            m_MaterialEditor.TextureProperty(mp_mainTexture, Styles.mainTextureText);
            if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout))
            {
                m_MaterialEditor.ShaderProperty(mp_cutOff, Styles.alphaCutoffText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1);
            }

            m_MaterialEditor.TextureProperty(mp_normalTexture, Styles.normalTextureText, false);
            if (material.GetTexture("_BumpMap") != null)
            {
                m_MaterialEditor.ShaderProperty(mp_normalStrength, Styles.normalStrengthText);
            }


            EditorGUILayout.Space();
            GUILayout.Label(Styles.renderOptions, EditorStyles.boldLabel);

        }
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var obj in mp_blendMode.targets)
                MaterialChanged((Material)obj);
        }
    }



    void BlendModePopup()
    {
        EditorGUI.showMixedValue = mp_blendMode.hasMixedValue;
        BlendMode mode = (BlendMode)mp_blendMode.floatValue;

        EditorGUI.BeginChangeCheck();
        mode = (BlendMode)EditorGUILayout.Popup(Styles.renderingMode, (int)mode, Styles.blendNames);
        if (EditorGUI.EndChangeCheck())
        {
            m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
            mp_blendMode.floatValue = (float)mode;
        }

        EditorGUI.showMixedValue = false;
    }

    public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
    {
        switch (blendMode)
        {
            case BlendMode.Opaque:
                material.SetOverrideTag("RenderType", "");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("CUTOUT");
//                material.DisableKeyword("_ALPHATEST_ON");
//                material.DisableKeyword("_ALPHABLEND_ON");
//                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;
            case BlendMode.Cutout:
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("CUTOUT");
//                material.EnableKeyword("_ALPHATEST_ON");
//                material.DisableKeyword("_ALPHABLEND_ON");
//                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                break;
            case BlendMode.Transparent:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("CUTOUT");
//                material.DisableKeyword("_ALPHATEST_ON");
//                material.DisableKeyword("_ALPHABLEND_ON");
//                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                break;
        }
    }

}