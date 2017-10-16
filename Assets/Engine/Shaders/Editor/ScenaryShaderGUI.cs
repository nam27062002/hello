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
using System.IO;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

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
        readonly public static string mainTextureText = "MainTex";

        readonly public static string enableBlendTextureText = "Enable Blend Texture";
        readonly public static string blendTextureText = "Blend Texture";

        readonly public static string enableNormalMapText = "Enable Normal map";
        readonly public static string normalTextureText = "Normal Texture";
        readonly public static string normalStrengthText = "Normal Texture strength";

        readonly public static string enableCutoffText = "Enable Alpha cutoff";
        readonly public static string CutoffText = "Alpha cutoff threshold";

        readonly public static string enableSpecularText = "Enable Specular";
        readonly public static string specularPowerText = "Specular Power";
        readonly public static string specularDirText = "Specular direction";

        readonly public static string enableFogText = "Enable Fog";
        readonly public static string enableDarkenText = "Enable Darken";

        readonly public static string automaticBlendingText = "Automatic blending";
        readonly public static string overlayColorText = "Vertex Color Tint";

        readonly public static string darkenPositionText = "Darken position";
        readonly public static string darkenDistanceText = "Darken distance";

        readonly public static string vertexColorModeText = "VertexColorMode";

        readonly public static string enableEmissiveBlink = "Enable emissive blink";
        readonly public static string emissivePowerText = "Emissive power";
        readonly public static string blinkTimeMultiplierText = "Blink time multiplier";
        readonly public static string emissionTypeText = "Emission type";

        readonly public static string lightmapContrastText = "Lightmap contrast";

        readonly public static string blendModeText = "Blend mode";
        readonly public static string renderQueueText = "Render queue";

        readonly public static string cullModeText = "Cull mode";
        readonly public static string cullWarningText = "Warning! You have activated double sided in opaque object.";


    }

    /// <summary>
    /// Material Properties
    /// </summary>
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

    MaterialProperty mp_BlendMode;
    MaterialProperty mp_DoubleSided;

    MaterialProperty mp_EmissivePower;
    MaterialProperty mp_BlinkTimeMultiplier;

    MaterialProperty mp_Cull;


/// <summary>
/// Toggle Material Properties
/// </summary>
    MaterialProperty mp_EnableBlendTexture;
    MaterialProperty mp_EnableAutomaticBlend;

    MaterialProperty mp_EnableSpecular;
    MaterialProperty mp_EnableNormalMap;

    MaterialProperty mp_EnableCutoff;
    MaterialProperty mp_EnableFog;
    MaterialProperty mp_EnableDarken;

//    MaterialProperty mp_EnableEmissiveBlink;
    MaterialProperty mp_EnableLightmapContrast;

    /// <summary>
    /// Enum Material PProperties
    /// </summary>

    MaterialProperty mp_VertexcolorMode;
    MaterialProperty mp_EmissionType;

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
        mp_blendTexture = FindProperty("_SecondTexture", props);
//        mp_lightmapIntensity = FindProperty("_LightmapIntensity", props);

        mp_normalTexture = FindProperty("_NormalTex", props);
        mp_normalStrength = FindProperty("_NormalStrength", props);

        mp_cutOff = FindProperty("_CutOff", props);

        mp_specularPower = FindProperty("_SpecularPower", props);
        mp_specularDirection = FindProperty("_SpecularDir", props);

        mp_darkenPosition = FindProperty("_DarkenPosition", props);
        mp_darkenDistance = FindProperty("_DarkenDistance", props);

        mp_EmissivePower = FindProperty("_EmissivePower", props);
        mp_BlinkTimeMultiplier = FindProperty("_BlinkTimeMultiplier", props);

        /// Toggle Material Properties

        mp_EnableBlendTexture = FindProperty("_EnableBlendTexture", props);
        mp_EnableAutomaticBlend = FindProperty("_EnableAutomaticBlend", props);

        mp_EnableSpecular = FindProperty("_EnableSpecular", props);
        mp_EnableNormalMap = FindProperty("_EnableNormalMap", props);

        mp_EnableCutoff = FindProperty("_EnableCutoff", props);
        mp_EnableFog = FindProperty("_EnableFog", props);
        mp_EnableDarken = FindProperty("_EnableDarken", props);

//        mp_EnableEmissiveBlink = FindProperty("_EnableEmissiveBlink", props);
        mp_EnableLightmapContrast = FindProperty("_EnableLightmapContrast", props);

        /// Enum Material PProperties

        mp_VertexcolorMode = FindProperty("VertexColor", props);
        mp_EmissionType = FindProperty("Emissive", props);
        mp_Cull = FindProperty("_Cull", props);

        mp_BlendMode = FindProperty("_BlendMode", props);
        mp_DoubleSided = FindProperty("_DoubleSided", props);
    }

    private bool featureSet(MaterialProperty feature, string label)
    {
        EditorGUILayout.BeginVertical(editorSkin.customStyles[0]);
        m_materialEditor.ShaderProperty(feature, label);
        EditorGUILayout.EndVertical();

        return feature.floatValue > 0.0f;
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
                material.DisableKeyword("CUTOFF");
                Debug.Log("Blend mode transparent");
                break;

            case 2:
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.SetFloat("_ZWrite", 1.0f);
//                material.renderQueue = 2500;
                material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
                material.EnableKeyword("CUTOFF");

                Debug.Log("Blend mode cutout");
                break;
        }

    }

    /// <summary>
    /// Draw the inspector.
    /// </summary>
    /// 
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        IniEditorSkin();
        FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
        Material material = materialEditor.target as Material;

        m_materialEditor = materialEditor;

        GUILayout.BeginHorizontal(editorSkin.customStyles[0]);
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Scenary standard shader", editorSkin.customStyles[0]);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical(editorSkin.customStyles[0]);
        materialEditor.ShaderProperty(mp_BlendMode, Styles.blendModeText);
        EditorGUILayout.EndVertical();

        int blendMode = (int)mp_BlendMode.floatValue;
        if (EditorGUI.EndChangeCheck())
        {
            mp_DoubleSided.floatValue = blendMode == 0 ? 0.0f : 1.0f;
            setBlendMode(material, blendMode);
        }
        if (blendMode == 2)
        {
            materialEditor.ShaderProperty(mp_cutOff, Styles.CutoffText);
        }

        materialEditor.TextureProperty(mp_mainTexture, Styles.mainTextureText);

        materialEditor.TextureProperty(mp_normalTexture, Styles.normalTextureText, false);

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

        if (featureSet(mp_EnableBlendTexture, Styles.enableBlendTextureText))
        {
            materialEditor.TextureProperty(mp_blendTexture, Styles.blendTextureText);
            materialEditor.ShaderProperty(mp_EnableAutomaticBlend, Styles.automaticBlendingText);
        }

        if (featureSet(mp_EnableSpecular, Styles.enableSpecularText))
        {
            materialEditor.ShaderProperty(mp_specularPower, Styles.specularPowerText);
            materialEditor.ShaderProperty(mp_specularDirection, Styles.specularDirText);
        }

        featureSet(mp_EnableFog, Styles.enableFogText);
/*
        if(featureSet(mp_EnableDarken, Styles.enableDarkenText))
        {
            materialEditor.ShaderProperty(mp_darkenPosition, Styles.darkenPositionText);
            materialEditor.ShaderProperty(mp_darkenDistance, Styles.darkenDistanceText);
        }
*/
        featureSet(mp_VertexcolorMode, Styles.vertexColorModeText);
        featureSet(mp_EmissionType, Styles.emissionTypeText);

//        if (featureSet(mp_EnableEmissiveBlink, Styles.enableEmissiveBlink))
        if (mp_EmissionType.floatValue == 1.0f)
        {
            materialEditor.ShaderProperty(mp_EmissivePower, Styles.emissivePowerText);
            materialEditor.ShaderProperty(mp_BlinkTimeMultiplier, Styles.blinkTimeMultiplierText);
        }
        /*
                if (featureSet(mp_EnableLightmapContrast, Styles.lightmapContrastText))
                {
                    materialEditor.
                }
        */
        /*
                if (GUILayout.Button("Reset keywords", editorSkin.customStyles[0]))
                {
                    material.shaderKeywords = null;
                }
        */
        if (mp_BlendMode.floatValue == 0.0f)
        {

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(editorSkin.customStyles[0]);
            materialEditor.ShaderProperty(mp_DoubleSided, Styles.cullModeText);
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                material.SetFloat("_Cull", mp_DoubleSided.floatValue == 1.0f ? (float)UnityEngine.Rendering.CullMode.Off : (float)UnityEngine.Rendering.CullMode.Back);
            }

            if (mp_Cull.floatValue == (float)UnityEngine.Rendering.CullMode.Off)
            {
                EditorGUILayout.HelpBox(Styles.cullWarningText, MessageType.Warning);
            }
        }


        EditorGUILayout.BeginHorizontal(editorSkin.customStyles[0]);
        EditorGUILayout.LabelField(Styles.renderQueueText);
        int renderQueue = EditorGUILayout.IntField(material.renderQueue);
        if (material.renderQueue !=  renderQueue)
        {
            material.renderQueue = renderQueue;
        }
        EditorGUILayout.EndHorizontal();
    }

    static void DebugKeywords(Material mat)
    {
        foreach (string kw in mat.shaderKeywords)
            Debug.Log("Material keywords: " + kw);
    }

    static void SetKeyword(Material m, string keyword, bool state)
    {
        if (state)
            m.EnableKeyword(keyword);
        else
            m.DisableKeyword(keyword);
    }


    ///////////////////////////////////////////////////////////////////////////////////////////////////
    //  Tools
    ///////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Seek for all scenary standard shaders and set cull mode to back
    /// </summary>
    [MenuItem("Tools/Scenary/set opaque to cull back")]
    public static void SetOpaqueToCullBack()
    {
        Debug.Log("Obtaining material list");

        //        EditorUtility.("Material keyword reset", "Obtaining Material list ...", "");

        Material[] materialList;
        AssetFinder.FindAssetInContent<Material>(Directory.GetCurrentDirectory() + "\\Assets", out materialList);

        Shader shader = Shader.Find("Hungry Dragon/Scenary/Scenary Standard");

        int sChanged = 0;

        for (int c = 0; c < materialList.Length; c++)
        {
            Material mat = materialList[c];
            // UnlitShadowLightmap.shader
            if (mat.shader.name == "Hungry Dragon/Scenary/Scenary Standard")
            {
                int blendMode = (int)mat.GetFloat("_BlendMode");
                int cullMode = (int)mat.GetFloat("_Cull");

                if (blendMode == 0 && cullMode == 0)
                {
                    setBlendMode(mat, 0);
                    sChanged++;
                }
            }
        }

        Debug.Log(sChanged + " materials changed");
    }



    /// <summary>
    /// Seek for old scenary shaders and change by new scenary standard material
    /// </summary>
    [MenuItem("Tools/Scenary/Replace old scenary shaders")]
    public static void ReplaceOldScenaryShaders()
    {
        Debug.Log("Obtaining material list");

        //        EditorUtility.("Material keyword reset", "Obtaining Material list ...", "");

        Material[] materialList;
        AssetFinder.FindAssetInContent<Material>(Directory.GetCurrentDirectory() + "\\Assets", out materialList);

        Shader shader = Shader.Find("Hungry Dragon/Scenary/Scenary Standard");

        int sChanged = 0;

        for (int c = 0; c < materialList.Length; c++)
        {
            Material mat = materialList[c];

            // UnlitShadowLightmap.shader
            if (mat.shader.name == "Hungry Dragon/Scenary/Diffuse + Lightmap")
            {
                mat.shader = shader;
                mat.SetFloat("_EnableFog", 1.0f);
                mat.SetFloat("_EnableOpaqueAlpha", 1.0f);

                mat.EnableKeyword("FOG");
                mat.EnableKeyword("OPAQUEALPHA");

                setBlendMode(mat, 0);   //Opaque
                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // UnlitShadowLightmapDarken.shader
            else if (mat.shader.name == "Hungry Dragon/Scenary/Diffuse + Lightmap + Darken")
            {
                mat.shader = shader;
                mat.SetFloat("_EnableDarken", 1.0f);
                mat.SetFloat("_EnableFog", 1.0f);
                mat.SetFloat("_EnableOpaqueAlpha", 1.0f);

                mat.EnableKeyword("DARKEN");
                mat.EnableKeyword("FOG");
                mat.EnableKeyword("OPAQUEALPHA");

                setBlendMode(mat, 0);   //Opaque
                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // UnlitShadowLightmapEmissive.shader
            else if (mat.shader.name == "Hungry Dragon/Scenary/Diffuse + Lightmap + Emissive blink")
            {
                mat.shader = shader;
                mat.SetFloat("_EnableEmissiveBlink", 1.0f);
                mat.SetFloat("_EnableFog", 1.0f);
                mat.SetFloat("_EnableOpaqueAlpha", 1.0f);

                mat.EnableKeyword("EMISSIVEBLINK");
                mat.EnableKeyword("FOG");
                mat.EnableKeyword("OPAQUEALPHA");

                setBlendMode(mat, 0);   //Opaque
                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // UnlitShadowLightmapNormal.shader
            else if (mat.shader.name == "Hungry Dragon/Scenary/Diffuse + Lightmap + Normal Map")
            {
                mat.shader = shader;
                mat.SetFloat("_EnableSpecular", 1.0f);
                mat.SetFloat("_SpecularPower", 20.0f);
                mat.SetFloat("_EnableNormalMap", 1.0f);
                mat.SetFloat("_NormalStrength", 1.0f);
                mat.SetFloat("_EnableFog", 1.0f);
                mat.SetFloat("_EnableOpaqueAlpha", 1.0f);

                mat.EnableKeyword("NORMALMAP");
                mat.EnableKeyword("SPECULAR");
                mat.EnableKeyword("FOG");
                mat.EnableKeyword("OPAQUEALPHA");

                setBlendMode(mat, 0);   //Opaque
                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // UnlitShadowLightmapCutoff.shader
            else if (mat.shader.name == "Hungry Dragon/Scenary/Diffuse + LightMap + AlphaCutoff (cutoff vegetation)")
            {
                mat.shader = shader;
                mat.SetFloat("_EnableFog", 1.0f);
                mat.SetFloat("_EnableCutoff", 1.0f);
                mat.SetFloat("_Cutoff", 0.5f);

                mat.EnableKeyword("FOG");
                mat.EnableKeyword("CUTOFF");

                EditorUtility.SetDirty(mat);

                setBlendMode(mat, 2);   //Cutoff
                Debug.Log("Cutoff: " + mat.name);
                sChanged++;
            }
            // UnlitShadowLightmapVColorMultiply.shader
            else if (mat.shader.name == "Hungry Dragon/Scenary/Diffuse + Lightmap + Vertex Color Multiply")
            {
                mat.shader = shader;
                mat.SetFloat("_EnableFog", 1.0f);
                mat.SetFloat("_EnableOpaqueAlpha", 1.0f);
                mat.SetFloat("VertexColor", 3.0f);

                mat.EnableKeyword("FOG");
                mat.EnableKeyword("OPAQUEALPHA");
                mat.EnableKeyword("VERTEXCOLOR_MODULATE");

                setBlendMode(mat, 0);   //Opaque
                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // AutomaticTextureBlending.shader
            else if (mat.shader.name == "Hungry Dragon/Scenary/Automatic Texture Blending + Lightmap")
            {
                mat.shader = shader;
                mat.SetFloat("_EnableFog", 1.0f);
                mat.SetFloat("_EnableOpaqueAlpha", 1.0f);
                mat.SetFloat("_EnableBlendTexture", 1.0f);
                mat.SetFloat("_EnableAutomaticBlend", 1.0f);

                mat.EnableKeyword("FOG");
                mat.EnableKeyword("OPAQUEALPHA");
                mat.EnableKeyword("BLEND_TEXTURE");
                mat.EnableKeyword("CUSTOM_VERTEXCOLOR");

                setBlendMode(mat, 0);   //Opaque
                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // AutomaticTextureBlendingDarken.shader
            else if (mat.shader.name == "Hungry Dragon/Scenary/Automatic Texture Blending + Lightmap + Darken")
            {
                mat.shader = shader;
                mat.SetFloat("_EnableFog", 1.0f);
                mat.SetFloat("_EnableDarken", 1.0f);
                mat.SetFloat("_EnableOpaqueAlpha", 1.0f);
                mat.SetFloat("_EnableBlendTexture", 1.0f);
                mat.SetFloat("_EnableAutomaticBlend", 1.0f);

                mat.EnableKeyword("FOG");
                mat.EnableKeyword("DARKEN");
                mat.EnableKeyword("OPAQUEALPHA");
                mat.EnableKeyword("BLEND_TEXTURE");
                mat.EnableKeyword("CUSTOM_VERTEXCOLOR");

                setBlendMode(mat, 0);   //Opaque
                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // TextureBlendingBasic.shader
            else if (mat.shader.name == "Hungry Dragon/Scenary/Texture Blending + Lightmap")
            {
                mat.shader = shader;
                mat.SetFloat("_EnableFog", 1.0f);
                mat.SetFloat("_EnableOpaqueAlpha", 1.0f);
                mat.SetFloat("_EnableBlendTexture", 1.0f);

                mat.EnableKeyword("FOG");
                mat.EnableKeyword("OPAQUEALPHA");
                mat.EnableKeyword("BLEND_TEXTURE");

                setBlendMode(mat, 0);   //Opaque
                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // TextureBlendingAdditive.shader
            else if (mat.shader.name == "Hungry Dragon/Scenary/Texture Blending + Lightmap + Vertex Color Additive")
            {
                mat.shader = shader;
                mat.SetFloat("_EnableFog", 1.0f);
                mat.SetFloat("_EnableOpaqueAlpha", 1.0f);
                mat.SetFloat("_EnableBlendTexture", 1.0f);
                mat.SetFloat("VertexColor", 2.0f);

                mat.EnableKeyword("FOG");
                mat.EnableKeyword("OPAQUEALPHA");
                mat.EnableKeyword("BLEND_TEXTURE");
                mat.EnableKeyword("VERTEXCOLOR_ADDITIVE");

                setBlendMode(mat, 0);   //Opaque
                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // TextureBlendingDarken.shader
            else if (mat.shader.name == "Hungry Dragon/Scenary/Texture Blending + Lightmap + Darken")
            {
                mat.shader = shader;
                mat.SetFloat("_EnableFog", 1.0f);
                mat.SetFloat("_EnableOpaqueAlpha", 1.0f);
                mat.SetFloat("_EnableDarken", 1.0f);
                mat.SetFloat("_EnableBlendTexture", 1.0f);

                mat.EnableKeyword("FOG");
                mat.EnableKeyword("OPAQUEALPHA");
                mat.EnableKeyword("DARKEN");
                mat.EnableKeyword("BLEND_TEXTURE");

                setBlendMode(mat, 0);   //Opaque
                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // TextureBlendingNormal.shader
            else if (mat.shader.name == "Hungry Dragon/Scenary/Texture Blending + Lightmap + Vertex Color Overlay + Normal Map")
            {
                mat.shader = shader;
                mat.SetFloat("_EnableSpecular", 1.0f);
                mat.SetFloat("_SpecularPower", 20.0f);
                mat.SetFloat("_EnableNormalMap", 1.0f);
                mat.SetFloat("_NormalStrength", 1.0f);
                mat.SetFloat("_EnableFog", 1.0f);
                mat.SetFloat("_EnableOpaqueAlpha", 1.0f);
                mat.SetFloat("VertexColor", 1.0f);

                mat.EnableKeyword("NORMALMAP");
                mat.EnableKeyword("SPECULAR");
                mat.EnableKeyword("FOG");
                mat.EnableKeyword("OPAQUEALPHA");
                mat.EnableKeyword("VERTEXCOLOR_OVERLAY");

                setBlendMode(mat, 0);   //Opaque
                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // TextureBlendingOverlay.shader
            else if (mat.shader.name == "Hungry Dragon/Scenary/Texture Blending + Lightmap + Vertex Color Overlay")
            {
                mat.shader = shader;
                mat.SetFloat("_EnableFog", 1.0f);
                mat.SetFloat("_EnableOpaqueAlpha", 1.0f);
                mat.SetFloat("_EnableBlendTexture", 1.0f);
                mat.SetFloat("VertexColor", 1.0f);

                mat.EnableKeyword("FOG");
                mat.EnableKeyword("OPAQUEALPHA");
                mat.EnableKeyword("BLEND_TEXTURE");
                mat.EnableKeyword("VERTEXCOLOR_OVERLAY");

                setBlendMode(mat, 0);   //Opaque
                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // UnlitShadowLightmapTransparent.shader
            else if (mat.shader.name == "Hungry Dragon/Scenary/Diffuse + Lightmap + Transparent (On Line Decorations)")
            {
                mat.shader = shader;
                mat.SetFloat("_EnableFog", 1.0f);

                mat.EnableKeyword("FOG");
                //                mat.EnableKeyword("CUTOFF");

                setBlendMode(mat, 1);   //Transparent
                EditorUtility.SetDirty(mat);
                sChanged++;
            }
        }
        Debug.Log(sChanged + " materials changed.");
    }
}