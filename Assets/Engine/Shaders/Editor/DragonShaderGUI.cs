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

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
internal class DragonShaderGUI : ShaderGUI
{

    //------------------------------------------------------------------------//
    // CONSTANTS AND ENUMERATORS											  //
    //------------------------------------------------------------------------//
/*
    public enum BlendMode
    {
        Opaque,
        Cutout,
        Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
    }
*/
    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//


    private static class Styles
    {
        readonly public static string mainTextureText = "Main Texture";
        readonly public static string detailTextureText = "Detail Texture";
        readonly public static string normalTextureText = "Normal Texture";

        readonly public static string tintColorText = "Color tint";
        readonly public static string colorMultiplyText = "Color Multiply";
        readonly public static string colorAddText = "Color Add";

        readonly public static string innerLightAddText = "Inner Light Add";
        readonly public static string innerLightColorText = "Inner Light Color";
        readonly public static string innerLightWavePhaseText = "Inner Light wave phase";
        readonly public static string innerLightWaveSpeedText = "Inner Light wave speed";

        readonly public static string ambientAddText = "Ambient Add";

        readonly public static string enableNormalMapText = "Enable Normal map";
        readonly public static string normalStrengthText = "Normal Texture strength";

        readonly public static string CutoffText = "Alpha cutoff threshold";

        readonly public static string enableSpecularText = "Enable Specular";
        readonly public static string specularPowerText = "Specular Exponent";
        readonly public static string secondLightDirectionText = "Second light direction";
        readonly public static string secondLightColorText = "Second light color";
        readonly public static string enableOpaqueSpecularText = "Enable Opaque Specular";

        readonly public static string enableFresnelText = "Enable Fresnel";
        readonly public static string enableOpaqueFresnelText = "Enable Opaque Fresnel";
        readonly public static string fresnelPowerText = "Fresnel Power";
        readonly public static string fresnelColorText = "Fresnel Color";

        readonly public static string additionalFXLayerText = "Additional FX layer";
        readonly public static string reflectionMapText = "Reflection Texture";
        readonly public static string reflectionAmountText = "Reflection amount";
        readonly public static string fireMapText = "Fire Texture";
        readonly public static string fireAmountText = "Fire amount";

        readonly public static string reflectionLayerText = "Reflection layer are actually used by chinese dragon. Applied as (Reflection Texture intensity) * (Reflection Amount) * (Detail Texture.b)";
        readonly public static string fireLayerText = "Fire layer are actually used by pet Phoenix. Applied as (Fire Amount) * (Detail Texture.b)";

        readonly public static string selfIluminationText = "Self ilumination";

        readonly public static string normalSelfIluminationText = "Default Self ilumination. Based on (Main Texture.rgb) * (Detail Texture.r) * _InnerLightAdd * (_InnerLightColor.rgb)";
        readonly public static string autoInnerLightSelfIluminationText = "Devil dragon self ilumination.";
        readonly public static string blinkLightsSelfIluminationText = "Reptile dragon rings self ilumination.";

        readonly public static string blendModeText = "Blend Mode";
        readonly public static string renderQueueText = "Render queue";
        readonly public static string stencilMaskText = "Stencil mask";

    }
    MaterialProperty mp_mainTexture;
    MaterialProperty mp_detailTexture;
    MaterialProperty mp_normalTexture;
    MaterialProperty mp_normalStrength;
    MaterialProperty mp_cutOff;

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

    MaterialProperty mp_colorMultiply;
    MaterialProperty mp_colorAdd;
    MaterialProperty mp_innerLightAdd;
    MaterialProperty mp_innerLightColor;
    MaterialProperty mp_innerLightWavePhase;
    MaterialProperty mp_innerLightWaveSpeed;

    MaterialProperty mp_BlendMode;
    MaterialProperty mp_stencilMask;

    /// <summary>
    /// Toggle Material Properties
    /// </summary>
    MaterialProperty mp_EnableSpecular;
    MaterialProperty mp_EnableNormalMap;

    MaterialProperty mp_EnableCutoff;
    MaterialProperty mp_EnableFresnel;
    MaterialProperty mp_EnableSilhouette;
    MaterialProperty mp_EnableOpaqueFresnel;
    MaterialProperty mp_EnableOpaqueSpecular;

    /// <summary>
    /// Enum Material PProperties
    /// </summary>

    MaterialProperty mp_FxLayer;
    MaterialProperty mp_SelfIlluminate;

    MaterialEditor m_materialEditor;
    ColorPickerHDRConfig m_ColorPickerHDRConfig = new ColorPickerHDRConfig(0f, 99f, 1 / 99f, 3f);

    readonly static string kw_normalmap = "NORMALMAP";
    readonly static string kw_specular = "SPECULAR";
    readonly static string kw_cutOff = "CUTOFF";
    readonly static string kw_doubleSided = "DOUBLESIDED";
    readonly static string kw_opaqueAlpha = "OPAQUEALPHA";
    readonly static string kw_emissiveBlink = "EMISSIVEBLINK";
    readonly static string kw_fresnel = "FRESNEL";
    readonly static string kw_reflection = "FXLAYER_REFLECTION";
    readonly static string kw_autoInnerLight = "SELFILLUMINATE_AUTOINNERLIGHT";
    readonly static string kw_blinkLights = "SELFILLUMINATE_BLINKLIGHTS";
    readonly static string kw_fire = "FXLAYER_FIRE";

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

        mp_colorMultiply = FindProperty("_Tint", props);
        mp_colorAdd = FindProperty("_ColorAdd", props);
        mp_innerLightAdd = FindProperty("_InnerLightAdd", props);
        mp_innerLightColor = FindProperty("_InnerLightColor", props);
        mp_innerLightWavePhase = FindProperty("_InnerLightWavePhase", props);
        mp_innerLightWaveSpeed = FindProperty("_InnerLightWaveSpeed", props);

        mp_fresnel = FindProperty("_Fresnel", props);
        mp_fresnelColor = FindProperty("_FresnelColor", props);
        mp_ambientAdd = FindProperty("_AmbientAdd", props);

        mp_reflectionMap = FindProperty("_ReflectionMap", props);
        mp_reflectionAmount = FindProperty("_ReflectionAmount", props);
        mp_fireMap = FindProperty("_FireMap", props);
        mp_fireAmount = FindProperty("_FireAmount", props);

        mp_BlendMode = FindProperty("_BlendMode", props);
        mp_stencilMask = FindProperty("_StencilMask", props);

        /// Toggle Material Properties

        mp_EnableSpecular = FindProperty("_EnableSpecular", props);
        mp_EnableNormalMap = FindProperty("_EnableNormalMap", props);
        mp_EnableCutoff = FindProperty("_EnableCutoff", props);
        mp_EnableFresnel = FindProperty("_EnableFresnel", props);
        mp_EnableSilhouette = FindProperty("_EnableSilhouette", props);
        mp_EnableOpaqueFresnel = FindProperty("_EnableOpaqueFresnel", props);
        mp_EnableOpaqueSpecular = FindProperty("_EnableOpaqueSpecular", props);

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

        GUILayout.BeginHorizontal(editorSkin.customStyles[1]);
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Dragon standard shader", editorSkin.customStyles[1]);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical(editorSkin.customStyles[1]);
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
        materialEditor.TextureProperty(mp_detailTexture, Styles.detailTextureText, false);
        materialEditor.TextureProperty(mp_normalTexture, Styles.normalTextureText, false);

        bool bNormalMap = mp_normalTexture.textureValue != null as Texture;

        SetKeyword(material, kw_normalmap, bNormalMap);
        EditorGUI.BeginChangeCheck();

        if (bNormalMap)
        {
            materialEditor.ShaderProperty(mp_normalStrength, Styles.normalStrengthText);
        }

        if (featureSet(mp_EnableSpecular, Styles.enableSpecularText))
        {
            materialEditor.ShaderProperty(mp_specExponent, Styles.specularPowerText);
            materialEditor.ShaderProperty(mp_secondLightDir, Styles.secondLightDirectionText);
            materialEditor.ShaderProperty(mp_secondLightColor, Styles.secondLightColorText);
            materialEditor.ShaderProperty(mp_EnableOpaqueSpecular, Styles.enableOpaqueSpecularText);
        }

        if (featureSet(mp_EnableFresnel, Styles.enableFresnelText))
        {
            materialEditor.ShaderProperty(mp_fresnel, Styles.fresnelPowerText);
            materialEditor.ShaderProperty(mp_fresnelColor, Styles.fresnelColorText);
            materialEditor.ShaderProperty(mp_EnableOpaqueFresnel, Styles.enableOpaqueFresnelText);
        }

        featureSet(mp_FxLayer, Styles.additionalFXLayerText);
        int fxLayer = (int)mp_FxLayer.floatValue;

        switch (fxLayer)
        {
            case 1:     //FXLayer_Reflection
                EditorGUILayout.HelpBox(Styles.reflectionLayerText, MessageType.Info);
                materialEditor.TextureProperty(mp_reflectionMap, Styles.reflectionMapText, false);
                materialEditor.ShaderProperty(mp_reflectionAmount, Styles.reflectionAmountText);
                break;

            case 2:     //FXLayer_Fire
                EditorGUILayout.HelpBox(Styles.fireLayerText, MessageType.Info);
                materialEditor.TextureProperty(mp_fireMap, Styles.fireMapText, false);
                materialEditor.ShaderProperty(mp_fireAmount, Styles.fireAmountText);
                break;
        }

        featureSet(mp_SelfIlluminate, Styles.selfIluminationText);
        int selfIluminateMode = (int)mp_SelfIlluminate.floatValue;

        switch (selfIluminateMode)
        {
            case 0:     //SELFILUMINATE_NORMAL
                EditorGUILayout.HelpBox(Styles.normalSelfIluminationText, MessageType.Info);
                materialEditor.ShaderProperty(mp_innerLightAdd, Styles.innerLightAddText);
                materialEditor.ShaderProperty(mp_innerLightColor, Styles.innerLightColorText);
                break;

            case 1:     //SELFILUMINATE_AUTOINNERLIGHTS
                EditorGUILayout.HelpBox(Styles.autoInnerLightSelfIluminationText, MessageType.Info);
                materialEditor.ShaderProperty(mp_innerLightWavePhase, Styles.innerLightWavePhaseText);
                materialEditor.ShaderProperty(mp_innerLightWaveSpeed, Styles.innerLightWaveSpeedText);
                materialEditor.ShaderProperty(mp_innerLightColor, Styles.innerLightColorText);
                break;

            case 3:     //SELFILLUMINATE_BLINKLIGHTS
                EditorGUILayout.HelpBox(Styles.blinkLightsSelfIluminationText, MessageType.Info);
                break;
        }

        EditorGUILayout.BeginVertical(editorSkin.customStyles[1]);
        EditorGUILayout.LabelField(Styles.tintColorText);
        EditorGUILayout.EndVertical();
        materialEditor.ShaderProperty(mp_colorMultiply, Styles.colorMultiplyText);
        materialEditor.ShaderProperty(mp_colorAdd, Styles.colorAddText);

        EditorGUILayout.BeginHorizontal(editorSkin.customStyles[1]);
        EditorGUILayout.LabelField(Styles.renderQueueText);
        int renderQueue = EditorGUILayout.IntField(material.renderQueue);
        if (material.renderQueue != renderQueue)
        {
            material.renderQueue = renderQueue;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(editorSkin.customStyles[1]);
        materialEditor.ShaderProperty(mp_stencilMask, Styles.stencilMaskText);

/*      EditorGUILayout.LabelField(Styles.stencilMaskText);
        int stencilMask = EditorGUILayout.IntField(material.renderQueue);
        if (material.renderQueue != renderQueue)
        {
            material.renderQueue = renderQueue;
        }
*/
        EditorGUILayout.EndHorizontal();



        if (GUILayout.Button("Log keywords", editorSkin.customStyles[1]))
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


    public static void setBlendMode(Material material, int blendMode)
    {
        material.SetFloat("_BlendMode", blendMode);

        switch (blendMode)
        {
            case 0:                         //Opaque
                material.SetOverrideTag("RenderType", "Opaque");
                material.SetOverrideTag("Queue", "Geometry");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.SetFloat("_ZWrite", 1.0f);
                ///                material.renderQueue = 2000;
                material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Back);
                SetKeyword(material, kw_cutOff, false);
                SetKeyword(material, kw_doubleSided, false);
                SetKeyword(material, kw_opaqueAlpha, true);
                material.SetFloat("_EnableCutoff", 0.0f);
                material.SetFloat("_EnableDoublesided", 0.0f);
                //                material.DisableKeyword("CUTOFF");
                Debug.Log("Blend mode opaque");
                break;

            case 1:                         //Cutout
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetOverrideTag("Queue", "AlphaTest");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                //                material.renderQueue = 3000;
                material.SetFloat("_ZWrite", 1.0f);
                material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
                SetKeyword(material, kw_cutOff, true);
                SetKeyword(material, kw_doubleSided, true);
                SetKeyword(material, kw_opaqueAlpha, false);
                material.SetFloat("_EnableCutoff", 1.0f);
                material.SetFloat("_EnableDoublesided", 1.0f);
                Debug.Log("Blend mode cutout");
                break;

            case 2:                         //Transparent
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetOverrideTag("Queue", "Transparent");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                //                material.renderQueue = 3000;
                material.SetFloat("_ZWrite", 0.0f);
                material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Back);
                SetKeyword(material, kw_cutOff, false);
                SetKeyword(material, kw_doubleSided, false);
                SetKeyword(material, kw_opaqueAlpha, false);
                material.SetFloat("_EnableCutoff", 0.0f);
                material.SetFloat("_EnableDoublesided", 0.0f);
                Debug.Log("Blend mode transparent");
                break;

        }
    }

    public static void changeMaterial(Material mat, Shader _newShader, int blendMode)
    {
        int rQueue = mat.renderQueue;
        mat.shader = _newShader;
        mat.shaderKeywords = null;
        setBlendMode(mat, blendMode);
        mat.renderQueue = rQueue;
    }

    /// <summary>
    /// Seek for old scenary shaders and change by new scenary standard material
    /// </summary>
    [MenuItem("Tools/Dragon/Replace old dragon shaders")]
    public static void ReplaceOldScenaryShaders()
    {
        Debug.Log("Obtaining material list");

        //        EditorUtility.("Material keyword reset", "Obtaining Material list ...", "");

        Material[] materialList;
        AssetFinder.FindAssetInContent<Material>(Directory.GetCurrentDirectory() + "\\Assets", out materialList);

        Shader shader = Shader.Find("Hungry Dragon/Dragon/Dragon standard");

        int sChanged = 0;

        for (int c = 0; c < materialList.Length; c++)
        {
            Material mat = materialList[c];

            // DragonBody.shader
            if (mat.shader.name == "Hungry Dragon/Dragon/Body")
            {
                changeMaterial(mat, shader, 0);

                SetKeyword(mat, kw_normalmap, true);
                SetKeyword(mat, kw_specular, true);
                SetKeyword(mat, kw_fresnel, true);

                mat.SetFloat("_EnableSpecular", 1.0f);
                mat.SetFloat("_EnableFresnel", 1.0f);

                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // DragonWings.shader
            else if (mat.shader.name == "Hungry Dragon/Dragon/Wings (Transparent)")
            {
                changeMaterial(mat, shader, 1);

                SetKeyword(mat, kw_normalmap, true);
                SetKeyword(mat, kw_specular, true);
                SetKeyword(mat, kw_fresnel, true);

                mat.SetFloat("_EnableSpecular", 1.0f);
                mat.SetFloat("_EnableFresnel", 1.0f);

                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // DragonBodyChinesse.shader
            else if (mat.shader.name == "Hungry Dragon/Dragon/Body Chinese")
            {
                changeMaterial(mat, shader, 0);

                SetKeyword(mat, kw_normalmap, true);
                SetKeyword(mat, kw_specular, true);
                SetKeyword(mat, kw_fresnel, true);

                mat.SetFloat("_EnableSpecular", 1.0f);
                mat.SetFloat("_EnableFresnel", 1.0f);

                SetKeyword(mat, kw_reflection, true);
                mat.SetFloat("FXLayer", 1.0f);

                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // DragonBodyDevil.shader
            else if (mat.shader.name == "Hungry Dragon/Dragon/Body Devil")
            {
                changeMaterial(mat, shader, 0);

                SetKeyword(mat, kw_normalmap, true);
                SetKeyword(mat, kw_specular, true);
                SetKeyword(mat, kw_fresnel, true);

                mat.SetFloat("_EnableSpecular", 1.0f);
                mat.SetFloat("_EnableFresnel", 1.0f);

                SetKeyword(mat, kw_autoInnerLight, true);
                mat.SetFloat("SelfIlluminate", 1.0f);

                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // DragonBodyReptilus.shader
            else if (mat.shader.name == "Hungry Dragon/Dragon/Body reptilus")
            {
                changeMaterial(mat, shader, 0);

                SetKeyword(mat, kw_normalmap, true);
                SetKeyword(mat, kw_specular, true);
                SetKeyword(mat, kw_fresnel, true);
                mat.SetFloat("_EnableSpecular", 1.0f);
                mat.SetFloat("_EnableFresnel", 1.0f);

                SetKeyword(mat, kw_blinkLights, true);
                mat.SetFloat("SelfIlluminate", 2.0f);

                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // DragonDeath.shader
            else if (mat.shader.name == "Hungry Dragon/Dragon/Death")
            {
                changeMaterial(mat, shader, 1);

                SetKeyword(mat, kw_normalmap, true);
                SetKeyword(mat, kw_specular, true);

                SetKeyword(mat, kw_fresnel, true);

                mat.SetFloat("_EnableSpecular", 1.0f);
                mat.SetFloat("_EnableFresnel", 1.0f);

                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // DragonDeathChinese.shader
            else if (mat.shader.name == "Hungry Dragon/Dragon/Death Chinese")
            {
                changeMaterial(mat, shader, 1);

                SetKeyword(mat, kw_normalmap, true);
                SetKeyword(mat, kw_specular, true);

                SetKeyword(mat, kw_fresnel, true);

                mat.SetFloat("_EnableSpecular", 1.0f);
                mat.SetFloat("_EnableFresnel", 1.0f);

                SetKeyword(mat, kw_reflection, true);
                mat.SetFloat("FXLayer", 1.0f);

                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // DragonPetPhoenix.shader
            else if (mat.shader.name == "Hungry Dragon/Dragon/PetPhoenix")
            {
                changeMaterial(mat, shader, 0);

                SetKeyword(mat, kw_normalmap, true);
                SetKeyword(mat, kw_specular, true);
                SetKeyword(mat, kw_fresnel, true);
                mat.SetFloat("_EnableSpecular", 1.0f);
                mat.SetFloat("_EnableFresnel", 1.0f);

                SetKeyword(mat, kw_fire, true);
                mat.SetFloat("FXLayer", 2.0f);

                EditorUtility.SetDirty(mat);
                sChanged++;
            }
            // DragonWingsDevil.shader
            else if (mat.shader.name == "Hungry Dragon/Dragon/Wings Devil (Transparent)")
            {
                changeMaterial(mat, shader, 1);

                SetKeyword(mat, kw_normalmap, true);
                SetKeyword(mat, kw_specular, true);

                SetKeyword(mat, kw_fresnel, true);

                mat.SetFloat("_EnableSpecular", 1.0f);
                mat.SetFloat("_EnableFresnel", 1.0f);

                SetKeyword(mat, kw_autoInnerLight, true);
                mat.SetFloat("SelfIlluminate", 1.0f);

                EditorUtility.SetDirty(mat);
                sChanged++;
            }

        }

        Debug.Log(sChanged + " materials changed.");

    }
}