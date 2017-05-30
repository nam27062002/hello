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
        Fade,       // Old school alpha-blending mode, fresnel does not affect amount of transparency
        Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
    }

    private enum WorkflowMode
    {
        Specular,
        Metallic,
        Dielectric
    }

    public enum SmoothnessMapChannel
    {
        SpecularMetallicAlpha,
        AlbedoAlpha,
    }

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//


    private static class Styles
    {
        public static GUIContent uvSetLabel = new GUIContent("UV Set");
        public static GUIContent[] uvSetOptions = new GUIContent[] { new GUIContent("UV channel 0"), new GUIContent("UV channel 1") };

        public static string emptyTootip = "";
        public static GUIContent mainTextureText = new GUIContent("MainTex", "Main texture (RGB) and Transparency (A)");
        public static GUIContent blendTextureText = new GUIContent("Blend Texture", "Blend Texture");
        public static GUIContent normalTextureText = new GUIContent("Normal Map", "Normal Map");
        public static GUIContent alphaCutoffText = new GUIContent("Alpha Cutoff", "Threshold for alpha cutoff");

//        public static GUIContent fogText = new GUIContent("Fog");
///        public static GUIContent darkenText = new GUIContent("Darken");


        public static string fogText = "Fog";
        public static string darkenText = "Darken";
        public static string specularText = "Specular";


        public static string whiteSpaceString = " ";
        public static string primaryMapsText = "Maps";
        public static string renderOptions = "Render Options";
        public static string renderingMode = "Rendering Mode";
//        public static GUIContent emissiveWarning = new GUIContent("Emissive value is animated but the material has not been configured to support emissive. Please make sure the material itself has some amount of emissive.");
//        public static GUIContent emissiveColorWarning = new GUIContent("Ensure emissive color is non-black for emission to have effect.");
        public static readonly string[] blendNames = Enum.GetNames(typeof(BlendMode));
    }

    /*
            _Color("Color", Color) = (1,1,1,1)
            _MainTex("Main Texture", 2D) = "white" {}
            _SecondTexture("Blend Texture", 2D) = "white" {}
            _NormalTexture("Blend Texture", 2D) = "white" {}

            _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

            [ToggleOff] _EnableSpecular("Fog", Float) = 1.0
            [ToggleOff] _EnableFog("Fog", Float) = 1.0
            [ToggleOff] _EnableDarken("Darken", Float) = 1.0

            _BumpScale("Scale", Float) = 1.0
            _Specular("Specular Factor", float) = 3
            _SpecularDir("Specular Dir", Vector) = (0,0,-1,0)

            [Enum(UV0, 0, UV1, 1)] _UVSec("UV Set for secondary textures", Float) = 0

            // Blending state
            [HideInInspector] _Mode("__mode", Float) = 0.0
            [HideInInspector] _SrcBlend("__src", Float) = 1.0
            [HideInInspector] _DstBlend("__dst", Float) = 0.0
            [HideInInspector] _ZWrite("__zw", Float) = 1.0

            _StencilMask("Stencil Mask", int) = 5
    */
    MaterialProperty mp_color;
    MaterialProperty mp_mainTexture;
    MaterialProperty mp_blendTexture;
    MaterialProperty mp_normalTexture;
    MaterialProperty mp_cutoff;
    MaterialProperty mp_enableSpecular;
    MaterialProperty mp_enableFog;
    MaterialProperty mp_enableDarken;
    MaterialProperty mp_normalStrength;
    MaterialProperty mp_specularPower;
    MaterialProperty mp_specularDirection;
    MaterialProperty mp_UVSec;
    MaterialProperty mp_blendMode;

    MaterialEditor m_MaterialEditor;
    ColorPickerHDRConfig m_ColorPickerHDRConfig = new ColorPickerHDRConfig(0f, 99f, 1 / 99f, 3f);

    WorkflowMode m_WorkflowMode = WorkflowMode.Specular;

    bool m_FirstTimeApply = true;


    //------------------------------------------------------------------------//
    // METHODS																  //
    //------------------------------------------------------------------------//


    public void FindProperties(MaterialProperty[] props)
    {
        mp_color = FindProperty("_Color", props);
        mp_mainTexture = FindProperty("_MainTex", props);
        mp_blendTexture = FindProperty("_SecondTexture", props);
        mp_normalTexture = FindProperty("_NormalTex", props);

        mp_cutoff = FindProperty("_Cutoff", props);
        mp_enableSpecular = FindProperty("_EnableSpecular", props);
        mp_enableFog = FindProperty("_EnableFog", props);
        mp_enableDarken = FindProperty("_EnableDarken", props);
        mp_normalStrength = FindProperty("_NormalStrength", props);
        mp_specularPower = FindProperty("_SpecularPower", props);
        mp_specularDirection = FindProperty("_SpecularDir", props);
        mp_UVSec = FindProperty("_UVSec", props);

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
            MaterialChanged(material, m_WorkflowMode);
            m_FirstTimeApply = false;
        }

        ShaderPropertiesGUI(material);

        if (GUILayout.Button("Reset keywords"))
        {
            material.shaderKeywords = null;
        }
    }

/*
    bool HasValidEmissiveKeyword(Material material)
    {
        // Material animation might be out of sync with the material keyword.
        // So if the emission support is disabled on the material, but the property blocks have a value that requires it, then we need to show a warning.
        // (note: (Renderer MaterialPropertyBlock applies its values to emissionColorForRendering))
        bool hasEmissionKeyword = material.IsKeywordEnabled("_EMISSION");
        if (!hasEmissionKeyword && ShouldEmissionBeEnabled(material, emissionColorForRendering.colorValue))
            return false;
        else
            return true;
    }
*/
    static void MaterialChanged(Material material, WorkflowMode workflowMode)
    {
//        material.shaderKeywords = null;

        SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));

        SetMaterialKeywords(material, workflowMode);

        string kwl = "";
        foreach (string kw in material.shaderKeywords)
        {
            kwl += kw + ",";
        }



        Debug.Log("Material keywords: " + kwl);
    }


    static void SetMaterialKeywords(Material material, WorkflowMode workflowMode)
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

            m_MaterialEditor.TexturePropertySingleLine(Styles.mainTextureText, mp_mainTexture, mp_color);
            if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout))
            {
                m_MaterialEditor.ShaderProperty(mp_cutoff, Styles.alphaCutoffText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1);
            }

            //            DoSpecularMetallicArea();
            m_MaterialEditor.TexturePropertySingleLine(Styles.normalTextureText, mp_normalTexture, mp_normalTexture.textureValue != null ? mp_normalStrength : null);
//            EditorGUI.BeginChangeCheck();
            m_MaterialEditor.TextureScaleOffsetProperty(mp_mainTexture);

            // Blend Texture properties
            m_MaterialEditor.TexturePropertySingleLine(Styles.blendTextureText, mp_blendTexture);
            m_MaterialEditor.TextureScaleOffsetProperty(mp_blendTexture);
            //            m_MaterialEditor.ShaderProperty(mp_UVSec, Styles.uvSetLabel.text);
            EditorGUILayout.Space();
            GUILayout.Label(Styles.renderOptions, EditorStyles.boldLabel);
            m_MaterialEditor.ShaderProperty(mp_enableFog, Styles.fogText);
            m_MaterialEditor.ShaderProperty(mp_enableDarken, Styles.darkenText);
            m_MaterialEditor.ShaderProperty(mp_enableSpecular, Styles.specularText);
//            if (material.GetInt)

        }
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var obj in mp_blendMode.targets)
                MaterialChanged((Material)obj, m_WorkflowMode);
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
/*
    void DoEmissionArea(Material material)
    {
        bool showHelpBox = !HasValidEmissiveKeyword(material);

        bool hadEmissionTexture = emissionMap.textureValue != null;

        // Texture and HDR color controls
        m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText, emissionMap, emissionColorForRendering, m_ColorPickerHDRConfig, false);

        // If texture was assigned and color was black set color to white
        float brightness = emissionColorForRendering.colorValue.maxColorComponent;
        if (emissionMap.textureValue != null && !hadEmissionTexture && brightness <= 0f)
            emissionColorForRendering.colorValue = Color.white;

        // Emission for GI?
        m_MaterialEditor.LightmapEmissionProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1);

        if (showHelpBox)
        {
            EditorGUILayout.HelpBox(Styles.emissiveWarning.text, MessageType.Warning);
        }
    }
*/

    /*
        void DoSpecularMetallicArea()
        {
            bool hasGlossMap = false;
            if (m_WorkflowMode == WorkflowMode.Specular)
            {
                hasGlossMap = specularMap.textureValue != null;
                m_MaterialEditor.TexturePropertySingleLine(Styles.specularMapText, specularMap, hasGlossMap ? null : specularColor);
            }
            else if (m_WorkflowMode == WorkflowMode.Metallic)
            {
                hasGlossMap = metallicMap.textureValue != null;
                m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicMap, hasGlossMap ? null : metallic);
            }

            bool showSmoothnessScale = hasGlossMap;
            if (smoothnessMapChannel != null)
            {
                int smoothnessChannel = (int)smoothnessMapChannel.floatValue;
                if (smoothnessChannel == (int)SmoothnessMapChannel.AlbedoAlpha)
                    showSmoothnessScale = true;
            }

            int indentation = 2; // align with labels of texture properties
            m_MaterialEditor.ShaderProperty(showSmoothnessScale ? smoothnessScale : smoothness, showSmoothnessScale ? Styles.smoothnessScaleText : Styles.smoothnessText, indentation);

            ++indentation;
            if (smoothnessMapChannel != null)
                m_MaterialEditor.ShaderProperty(smoothnessMapChannel, Styles.smoothnessMapChannelText, indentation);
        }

    */

    public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
    {
        switch (blendMode)
        {
            case BlendMode.Opaque:
                material.SetOverrideTag("RenderType", "");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
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
//                material.EnableKeyword("_ALPHATEST_ON");
//                material.DisableKeyword("_ALPHABLEND_ON");
//                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                break;
            case BlendMode.Fade:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
//                material.DisableKeyword("_ALPHATEST_ON");
//                material.EnableKeyword("_ALPHABLEND_ON");
//                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                break;
            case BlendMode.Transparent:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
//                material.DisableKeyword("_ALPHATEST_ON");
//                material.DisableKeyword("_ALPHABLEND_ON");
//                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                break;
        }
    }

}