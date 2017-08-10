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
internal class ScenaryShaderGUI : ShaderGUI {
    
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

    MaterialProperty mp_blendTexture;
    MaterialProperty mp_lightmapIntensity;

    MaterialProperty mp_normalTexture;
    MaterialProperty mp_normalStrength;

    MaterialProperty mp_cutOff;

    MaterialProperty mp_specularPower;
    MaterialProperty mp_specularDirection;

    MaterialProperty mp_darkenPosition;
    MaterialProperty mp_darkenDistance;

    MaterialProperty mp_blendMode;

    MaterialProperty mp_EmissivePower;
    MaterialProperty mp_BlinkTimeMultiplier;

    MaterialEditor m_MaterialEditor;
    ColorPickerHDRConfig m_ColorPickerHDRConfig = new ColorPickerHDRConfig(0f, 99f, 1 / 99f, 3f);

    readonly static string kw_blendTexture = "BLEND_TEXTURE";
    readonly static string kw_automaticBlend = "CUSTOM_VERTEXPOSITION";
    readonly static string kw_fog = "FOG";
    readonly static string kw_darken = "DARKEN";
    readonly static string kw_normalmap = "NORMALMAP";
    readonly static string kw_specular = "SPECULAR";
    readonly static string kw_cutOff = "CUTOFF";
    readonly static string kw_emissiveBlink = "EMISSIVEBLINK";


    bool m_FirstTimeApply = true;

    //------------------------------------------------------------------------//
    // METHODS																  //
    //------------------------------------------------------------------------//

    public void FindProperties(MaterialProperty[] props)
    {
        mp_mainTexture = FindProperty("_MainTex", props);
        mp_blendTexture = FindProperty("_SecondTexture", props);
        mp_lightmapIntensity = FindProperty("_LightmapIntensity", props);

        mp_normalTexture = FindProperty("_NormalTex", props);
        mp_normalStrength = FindProperty("_NormalStrength", props);

        mp_cutOff = FindProperty("_CutOff", props);

        mp_specularPower = FindProperty("_SpecularPower", props);
        mp_specularDirection = FindProperty("_SpecularDir", props);

        mp_darkenPosition = FindProperty("_DarkenPosition", props);
        mp_darkenDistance = FindProperty("_DarkenDistance", props);

        mp_blendMode = FindProperty("_Mode", props);

        mp_EmissivePower = FindProperty("_EmissivePower", props);
        mp_BlinkTimeMultiplier = FindProperty("_BlinkTimeMultiplier", props);
    }


    /// <summary>
    /// Draw the inspector.
    /// </summary>
    /// 
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
        Material material = materialEditor.target as Material;

        materialEditor.TextureProperty(mp_mainTexture, Styles.mainTextureText);

//        m_
        bool textureBlend = material.IsKeywordEnabled(kw_blendTexture);

        EditorGUI.BeginChangeCheck();
        textureBlend = EditorGUILayout.Foldout(textureBlend, Styles.blendTextureText, true);
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword(material, kw_blendTexture, textureBlend);
        }
        if (textureBlend)
        {
            m_MaterialEditor.TextureProperty(mp_blendTexture, Styles.blendTextureText);

            bool automaticBlend = material.IsKeywordEnabled(kw_automaticBlend);
            EditorGUI.BeginChangeCheck();
            automaticBlend = EditorGUILayout.Toggle(automaticBlend, Styles.automaticBlendingText);
            if (EditorGUI.EndChangeCheck())
            {
                SetKeyword(material, kw_automaticBlend, automaticBlend);
            }
        }


    }

    static void MaterialChanged(Material material)
    {
//        material.shaderKeywords = null;

        SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));

        SetMaterialKeywords(material);

/*
        string kwl = "";
        foreach (string kw in material.shaderKeywords)
        {
            kwl += kw + ",";
        }

        Debug.Log("Material keywords: " + kwl);
*/
    }


    static void SetMaterialKeywords(Material material)
    {
        // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
        // (MaterialProperty value might come from renderer material property block)
        SetKeyword(material, "NORMALMAP", material.GetTexture("_NormalTex"));
        SetKeyword(material, "BLEND_TEXTURE", material.GetTexture("_SecondTexture"));
    }


    static void SetKeyword(Material m, string keyword, bool state)
    {
        if (state)
            m.EnableKeyword(keyword);
        else
            m.DisableKeyword(keyword);
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
                material.DisableKeyword("CUTOFF");
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
                material.EnableKeyword("CUTOFF");
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
                material.DisableKeyword("CUTOFF");
//                material.DisableKeyword("_ALPHATEST_ON");
//                material.DisableKeyword("_ALPHABLEND_ON");
//                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                break;
        }
    }

}