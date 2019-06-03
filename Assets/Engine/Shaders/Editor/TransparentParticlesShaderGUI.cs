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
using System.Collections.Generic;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
internal class TransparentParticlesShaderGUI : ShaderGUI {

    //------------------------------------------------------------------------//
    // CONSTANTS AND ENUMERATORS											  //
    //------------------------------------------------------------------------//
    
    public enum ColorSource
    {
        TwoColors,
        TextureRamp,
        OneColor
    }

    public enum PostEffect
    {
        None,
        Noise,
        FlowMap
    }


    private static class Styles
    {
        readonly public static string basicColorText = "Basic color";
        readonly public static string saturatedColorText = "Saturated color";
        readonly public static string mainTexText = "Particle texture";
        readonly public static string colorRampText = "Color ramp";
        readonly public static string dissolveTexText = "Dissolve texture";
        readonly public static string emissionSaturationText = "Emission saturation";
        readonly public static string opacitySaturationText = "Opacity saturation";
        readonly public static string colorMultiplierText = "Color multiplier";
        readonly public static string enableDissolveText = "Enable Alpha Dissolve";
        readonly public static string colorSourceText = "Color source";
        readonly public static string enableColorVertexText = "Enable Color Vertex";
        readonly public static string dissolveStepMinText = "Dissolve step min";
        readonly public static string dissolveStepMaxText = "Dissolve step max";
        readonly public static string enableAutomaticPanningText = "Enable Automatic Panning";
        readonly public static string panningText = "Panning";
        readonly public static string tintColorText = "Tint Color";
        readonly public static string enableEmissivePowerText = "Enable Emissive Power";
        readonly public static string emissivePowerText = "Enable Emissive Power";
        readonly public static string alphaBlendOffsetText = "Alpha Blend Offset";
        readonly public static string particlesText = "Particles";
        readonly public static string standardParticlesText = "Standard";
        readonly public static string extendedParticlesText = "Extended";
        readonly public static string enableExtendedParticlesText = "Enable extended particles";
        readonly public static string blendModeText = "Blend Mode";
        readonly public static string rgbColorVertexText = "Use RGB color vertex";
        readonly public static string renderQueueText = "Render queue";
        readonly public static string zTestText = "Z Test";
        readonly public static string zCullMode = "Cull Mode";
        readonly public static string dissolveTipText = "Alpha dissolve receives custom data from particle system in TEXCOORD0.zw and MainTex.gb.";
        readonly public static string postEffectText = "PostEffect";
        readonly public static string enableNoiseTextureText = "Enable noise texture";
        readonly public static string noiseTextureText = "Noise texture";
        readonly public static string enableNoiseUVchannelText = "Enable noise UV channel";
        readonly public static string noiseTextureEmissionText = "R: Emission";
        readonly public static string noiseTextureAlphaText = "G: Alpha";
        readonly public static string noiseTextureDissolveText = "B: Dissolve";

        readonly public static string enableFlowMapUVchannelText = "Enable flowmap UV channel";
        readonly public static string flowMapTextureText = "Flowmap texture";
        readonly public static string flowMapSpeedText = "Speed";
        readonly public static string flowMapIntensityText = "Intensity";

        readonly public static string stencilMaskText = "Stencil mask";
        readonly public static string stencilCompareText = "Stencil compare";

    }

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//


    /// <summary>
    /// Material Properties
    /// </summary>
    MaterialProperty mp_basicColor;
    MaterialProperty mp_saturatedColor;
    MaterialProperty mp_mainTex;
    MaterialProperty mp_colorRamp;
    MaterialProperty mp_dissolveTex;
    MaterialProperty mp_noiseTex;
    MaterialProperty mp_emissionSaturation;
    MaterialProperty mp_opacitySaturation;
    MaterialProperty mp_colorMultiplier;
    MaterialProperty mp_dissolveStep;
    MaterialProperty mp_panning;
    MaterialProperty mp_tintColor;
    MaterialProperty mp_emissivePower;
    MaterialProperty mp_alphaBlendOffset;
    MaterialProperty mp_noisePanning;

    /// <summary>
    /// Toggle Material Properties
    /// </summary>
    MaterialProperty mp_enableDissolve;
    MaterialProperty mp_enableColorRamp;
    MaterialProperty mp_enableColorVertex;
//    MaterialProperty mp_enableAutomaticPanning;
    MaterialProperty mp_enableEmissivePower;
    MaterialProperty mp_enableExtendedParticles;
    MaterialProperty mp_enableRBGColorVertex;
    MaterialProperty mp_enableNoiseTexture;
    MaterialProperty mp_enableNoiseUVChannel;
    MaterialProperty mp_enableNoiseTextureEmission;
    MaterialProperty mp_enableNoiseTextureAlpha;
    MaterialProperty mp_enableNoiseTextureDissolve;

    /// <summary>
    /// Enum Material PProperties
    /// </summary>

    MaterialProperty mp_blendMode;
    MaterialProperty mp_srcBlend;
    MaterialProperty mp_dstBlend;
    MaterialProperty mp_zTest;
    MaterialProperty mp_cullMode;

    MaterialProperty mp_stencilMask;
    MaterialProperty mp_stencilCompare;

    MaterialEditor m_materialEditor;

    readonly static string kw_blendTexture = "BLEND_TEXTURE";
    readonly static string kw_automaticBlend = "CUSTOM_VERTEXPOSITION";
    readonly static string kw_fog = "FOG";
    readonly static string kw_normalmap = "NORMALMAP";
    readonly static string kw_specular = "SPECULAR";
    readonly static string kw_cutOff = "CUTOFF";
    readonly static string kw_emissiveBlink = "EMISSIVEBLINK";

    private GUISkin editorSkin;
    private readonly static string editorSkinPath = "Assets/Engine/Shaders/Editor/GUISkin/MaterialEditorSkin.guiskin";

    private bool m_stencilParticles = false;

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
        mp_basicColor = FindProperty("_BasicColor", props);
        mp_saturatedColor = FindProperty("_SaturatedColor", props);
        mp_mainTex = FindProperty("_MainTex", props);
        mp_colorRamp = FindProperty("_ColorRamp", props);
        mp_noiseTex = FindProperty("_NoiseTex", props);
        mp_noisePanning = FindProperty("_NoisePanning", props);
        mp_emissionSaturation = FindProperty("_EmissionSaturation", props);
        mp_opacitySaturation = FindProperty("_OpacitySaturation", props);
        mp_colorMultiplier = FindProperty("_ColorMultiplier", props);
        mp_dissolveStep = FindProperty("_DissolveStep", props);
        mp_panning = FindProperty("_Panning", props);
        mp_tintColor = FindProperty("_TintColor", props);
        mp_emissivePower = FindProperty("_EmissivePower", props);
        mp_alphaBlendOffset = FindProperty("_ABOffset", props);

        /// Toggle Material Properties

        mp_enableColorRamp = FindProperty("_EnableColorRamp", props);
        mp_enableColorVertex = FindProperty("_EnableColorVertex", props);
//        mp_enableAutomaticPanning = FindProperty("_EnableAutomaticPanning", props);
        mp_enableEmissivePower = FindProperty("_EnableEmissivePower", props);
        mp_enableExtendedParticles = FindProperty("_EnableExtendedParticles", props);
        mp_enableDissolve = FindProperty("_EnableAlphaDissolve", props);
        mp_enableNoiseTexture = FindProperty("_EnableNoiseTexture", props);
        mp_enableNoiseTextureEmission = FindProperty("_EnableNoiseTextureEmission", props);
        mp_enableNoiseTextureAlpha = FindProperty("_EnableNoiseTextureAlpha", props);
        mp_enableNoiseTextureDissolve = FindProperty("_EnableNoiseTextureDissolve", props);
        mp_enableNoiseUVChannel = FindProperty("_EnableNoiseUV", props);

        /// Enum Material PProperties

        mp_blendMode = FindProperty("BlendMode", props);
        mp_srcBlend = FindProperty("_SrcBlend", props);
        mp_dstBlend = FindProperty("_DstBlend", props);
        mp_zTest = FindProperty("_ZTest", props);
        mp_cullMode = FindProperty("_Cull", props);


        if (m_stencilParticles)
        {
            mp_stencilMask = FindProperty("_StencilMask", props);
            mp_stencilCompare = FindProperty("_Comp", props);
        }
    }

    private bool featureSet(MaterialProperty feature, string label)
    {
        EditorGUILayout.BeginVertical(editorSkin.customStyles[2]);
        m_materialEditor.ShaderProperty(feature, label);
        EditorGUILayout.EndVertical();

        return feature.floatValue > 0.0f;
    }

    private static string[] blendModes =
    {
        "BLENDMODE_ADDITIVE",
        "BLENDMODE_SOFTADDITIVE",
        "BLENDMODE_ADDITIVEDOUBLE",
        "BLENDMODE_ALPHABLEND",
        "BLENDMODE_ADDITIVEALPHABLEND",
        "BLENDMODE_PREMULTIPLY",
        //añado multiply
        "BLENDMODE_MULTIPLY"
    };

    public static void setBlendMode(Material material, int blendMode)
    {
        material.SetFloat("_BlendMode", blendMode);
        material.DisableKeyword("BLENDMODE_ADDITIVE");
        material.DisableKeyword("BLENDMODE_SOFTADDITIVE");
        material.DisableKeyword("BLENDMODE_ADDITIVEDOUBLE");
        material.DisableKeyword("BLENDMODE_ALPHABLEND");
        material.DisableKeyword("BLENDMODE_ADDITIVEALPHABLEND");
        material.DisableKeyword("BLENDMODE_PREMULTIPLY");
        material.DisableKeyword("BLENDMODE_MULTIPLY");

        material.EnableKeyword(blendModes[blendMode]);
        material.SetFloat("BlendMode", (float)blendMode);

        switch (blendMode)
        {
            case 0:                                                         //Additive
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
                Debug.Log("Blend mode additive");
                break;

            case 1:                                                         //Soft Additive
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcColor);
                Debug.Log("Blend mode soft additive");
                break;

            case 2:                                                         //Additive Double
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
                Debug.Log("Blend mode additive double");
                break;

            case 3:                                                         //Alpha blend
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                Debug.Log("Blend mode alpha blend");
                break;

            case 4:                                                         //Additive Alpha blend
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                Debug.Log("Blend mode additive alpha blend");
                break;

            case 5:                                                         //Premultiply
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                Debug.Log("Blend mode premultiply");
                break;
            //I add the multiply module
            case 6:                                                         //Just multiply
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.DstColor);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                Debug.Log("Blend mode multiply");
                break;
        }
    }

    private static string[] validKeyWords =
    {
        "EMISSIVEPOWER",
        "DISSOLVE_ENABLED",
        "COLOR_DOUBLETINT",
        "COLOR_RAMP",
        "COLOR_TINT",
        "APPLY_RGB_COLOR_VERTEX",
        "AUTOMATICPANNING",
        "EXTENDED_PARTICLES"
    };


    public static void changeMaterial(Material mat, Shader _newShader, int blendMode)
    {
        int rQueue = mat.renderQueue;
        mat.shader = _newShader;

        List<string> mlist = new List<string>();

        foreach (string keyWord in validKeyWords)
        {
            if (mat.IsKeywordEnabled(keyWord))
            {
                mlist.Add(keyWord);
            }
        }
        if (mat.IsKeywordEnabled("DISSOLVE"))
        {
            mlist.Add(validKeyWords[1]);
        }

        mat.shaderKeywords = null;
        setBlendMode(mat, blendMode);
        mat.renderQueue = rQueue;

        foreach (string keyWord in mlist)
        {
            mat.EnableKeyword(keyWord);
        }
    }


    private static void setExtendedParticles(Material mat, bool enable)
    {
        mat.SetFloat("_EnableExtendedParticles", enable ? 1.0f: 0.0f);

        if (enable)
        {
            mat.EnableKeyword("EXTENDED_PARTICLES");
        }
        else
        {
            mat.DisableKeyword("EXTENDED_PARTICLES");
        }
    }

    private static string[] dissolveKeywords =
    {
        "DISSOLVE_NONE",
        "DISSOLVE_ENABLED",
    };

    private static void setDissolve(Material mat, bool dissolve)
    {
        mat.DisableKeyword("DISSOLVE_NONE");
        mat.DisableKeyword("DISSOLVE_ENABLED");
        mat.DisableKeyword("DISSOLVE_EXTENDED");

        mat.EnableKeyword(dissolveKeywords[dissolve ? 1: 0]);
    }


    private static string[] colorSourceKeywords =
    {
        "COLOR_RAMP",
        "COLOR_TINT"
    };

    private static ColorSource getColorSource(Material mat)
    {
        if (mat.IsKeywordEnabled(colorSourceKeywords[0]))
        {
            return ColorSource.TextureRamp;
        }
        else if (mat.IsKeywordEnabled(colorSourceKeywords[1]))
        {
            return ColorSource.OneColor;
        }

        return ColorSource.TwoColors;
    }

    private static void setColorSource(Material mat, ColorSource col)
    {
        foreach (string kw in colorSourceKeywords)
        {
            mat.DisableKeyword(kw);
        }

        switch (col)
        {
            case ColorSource.TwoColors:
                break;
            case ColorSource.TextureRamp:
                mat.EnableKeyword(colorSourceKeywords[0]);
                break;
            case ColorSource.OneColor:
                mat.EnableKeyword(colorSourceKeywords[1]);
                break;
        }
    }

    private static string[] postEffectKeywords =
    {
        "NOISE_TEXTURE",
        "FLOWMAP"
    };


    private static PostEffect getPostEffect(Material mat)
    {
        if (mat.IsKeywordEnabled(postEffectKeywords[0]))
        {
            return PostEffect.Noise;
        }
        else if (mat.IsKeywordEnabled(postEffectKeywords[1]))
        {
            return PostEffect.FlowMap;
        }
        return PostEffect.None;
    }

    private static void setPostEffect(Material mat, PostEffect col)
    {
        foreach (string kw in postEffectKeywords)
        {
            mat.DisableKeyword(kw);
        }

        switch (col)
        {
            case PostEffect.None:
                break;
            case PostEffect.Noise:
                mat.EnableKeyword(postEffectKeywords[0]);
                break;
            case PostEffect.FlowMap:
                mat.EnableKeyword(postEffectKeywords[1]);
                break;
        }
    }



    private static bool getDissolve(Material mat)
    {
        return mat.IsKeywordEnabled(dissolveKeywords[1]);
    }

    /// <summary>
    /// Draw the inspector.
    /// </summary>
    /// 
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        m_materialEditor = materialEditor;
        Material material = materialEditor.target as Material;

        m_stencilParticles = material.shader.name.Contains("(Stencil)");

        IniEditorSkin();
        FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly

        GUILayout.BeginHorizontal(editorSkin.customStyles[2]);
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Transparent particles shader", editorSkin.customStyles[2]);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical(editorSkin.customStyles[2]);
        materialEditor.ShaderProperty(mp_blendMode, Styles.blendModeText);
        EditorGUILayout.EndVertical();

        int blendMode = (int)mp_blendMode.floatValue;
        if (EditorGUI.EndChangeCheck())
        {
            Debug.Log("Blend Mode: " + blendMode);
            setBlendMode(material, blendMode);
        }

        if (blendMode == 4)
        {
            materialEditor.ShaderProperty(mp_alphaBlendOffset, Styles.alphaBlendOffsetText);
        }

/*
        bool isExtended = mp_enableExtendedParticles.floatValue > 0.5f ? true : false;

        EditorGUILayout.BeginVertical(editorSkin.customStyles[2]);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(Styles.particlesText, GUILayout.Width(70));
//        if (GUILayout.Button(Styles.standardParticlesText, editorSkin.customStyles[isExtended ? 2: 0]))
        if (GUILayout.Button(Styles.standardParticlesText))
        {
                setExtendedParticles(material, false);
            isExtended = false;
        }
//        if (GUILayout.Button(Styles.extendedParticlesText, editorSkin.customStyles[isExtended ? 0 : 2]))
        if (GUILayout.Button(Styles.extendedParticlesText))
        {
                setExtendedParticles(material, true);
            isExtended = true;
        }
        //        m_materialEditor.ShaderProperty(feature, label);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
*/
        if(featureSet(mp_enableExtendedParticles, Styles.enableExtendedParticlesText))
        {
            materialEditor.TextureProperty(mp_mainTex, Styles.mainTexText);

            {
                Vector4 tem = mp_panning.vectorValue;
                Vector2 p1 = new Vector2(tem.x, tem.y);
                p1 = EditorGUILayout.Vector2Field(Styles.panningText, p1);
                //            materialEditor.ShaderProperty(mp_panning, Styles.panningText);
                tem.x = p1.x; tem.y = p1.y;
                mp_panning.vectorValue = tem;
            }

            featureSet(mp_enableColorVertex, Styles.enableColorVertexText);

            mp_enableDissolve.floatValue = getDissolve(material) ? 1.0f : 0.0f;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(editorSkin.customStyles[2]);
            m_materialEditor.ShaderProperty(mp_enableDissolve, Styles.enableDissolveText);
            EditorGUILayout.EndVertical();
            int dissolve = (int)mp_enableDissolve.floatValue;
            if (EditorGUI.EndChangeCheck())
            {
                setDissolve(material, dissolve > 0);
            }

            if (dissolve != 0)
            {
                EditorGUILayout.HelpBox(Styles.dissolveTipText, MessageType.Info);

                Vector4 tem = mp_dissolveStep.vectorValue;
                Vector2 p1 = new Vector2(tem.x, tem.y);

                p1.x = EditorGUILayout.Slider(Styles.dissolveStepMinText, p1.x, 0.0f, 1.0f);
                p1.y = EditorGUILayout.Slider(Styles.dissolveStepMaxText, p1.y, 0.0f, 1.0f);
                //                p1 = EditorGUILayout.Vector2Field(Styles.dissolveStepText, p1);
                //            materialEditor.ShaderProperty(mp_panning, Styles.panningText);
                tem.x = p1.x; tem.y = p1.y;
                mp_dissolveStep.vectorValue = tem;
//                materialEditor.ShaderProperty(mp_dissolveStep, Styles.dissolveStepText);
            }

            EditorGUILayout.BeginVertical(editorSkin.customStyles[2]);
            ColorSource col = getColorSource(material);
            ColorSource ncol = (ColorSource)EditorGUILayout.EnumPopup(Styles.colorSourceText, col);
            EditorGUILayout.EndVertical();

            if (col != ncol)
            {
                setColorSource(material, ncol);
            }

            if (ncol == ColorSource.TextureRamp)
            {
                materialEditor.ShaderProperty(mp_colorRamp, Styles.colorRampText);
                materialEditor.ShaderProperty(mp_colorMultiplier, Styles.colorMultiplierText);
            }
            else if (ncol == ColorSource.OneColor)
            {
                materialEditor.ShaderProperty(mp_basicColor, Styles.basicColorText);
            }
            else
            {
                materialEditor.ShaderProperty(mp_basicColor, Styles.basicColorText);
                materialEditor.ShaderProperty(mp_saturatedColor, Styles.saturatedColorText);
                materialEditor.ShaderProperty(mp_colorMultiplier, Styles.colorMultiplierText);
            }

            materialEditor.ShaderProperty(mp_emissionSaturation, Styles.emissionSaturationText);
            materialEditor.ShaderProperty(mp_opacitySaturation, Styles.opacitySaturationText);


            EditorGUILayout.BeginVertical(editorSkin.customStyles[2]);
            PostEffect post = getPostEffect(material);
            PostEffect npost = (PostEffect)EditorGUILayout.EnumPopup(Styles.postEffectText, post);
            EditorGUILayout.EndVertical();

            if (post != npost)
            {
                setPostEffect(material, npost);
            }


            if (npost == PostEffect.Noise)
            {
                //                if (featureSet(mp_enableNoiseUVChannel, Styles.enableNoiseUVchannelText))
                materialEditor.ShaderProperty(mp_enableNoiseUVChannel, Styles.enableNoiseUVchannelText);
                if (mp_enableNoiseUVChannel.floatValue > 0.0f)
                {
                    materialEditor.TextureProperty(mp_noiseTex, Styles.noiseTextureText, true);
                }
                else
                {
                    materialEditor.TextureProperty(mp_noiseTex, Styles.noiseTextureText, false);
                }

                {
                    Vector4 tem = mp_noisePanning.vectorValue;
                    Vector2 p1 = new Vector2(tem.x, tem.y);
                    p1 = EditorGUILayout.Vector2Field(Styles.panningText, p1);
                    //            materialEditor.ShaderProperty(mp_panning, Styles.panningText);
                    tem.x = p1.x; tem.y = p1.y;
                    mp_noisePanning.vectorValue = tem;
                }

                materialEditor.ShaderProperty(mp_enableNoiseTextureEmission, Styles.noiseTextureEmissionText);
                materialEditor.ShaderProperty(mp_enableNoiseTextureAlpha, Styles.noiseTextureAlphaText);
                materialEditor.ShaderProperty(mp_enableNoiseTextureDissolve, Styles.noiseTextureDissolveText);
            }
            else if (npost == PostEffect.FlowMap)
            {
                materialEditor.ShaderProperty(mp_enableNoiseUVChannel, Styles.enableFlowMapUVchannelText);
                if (mp_enableNoiseUVChannel.floatValue > 0.0f)
                {
                    materialEditor.TextureProperty(mp_noiseTex, Styles.flowMapTextureText, true);
                }
                else
                {
                    materialEditor.TextureProperty(mp_noiseTex, Styles.flowMapTextureText, false);
                }

                Vector4 tem = mp_noisePanning.vectorValue;
                tem.x = EditorGUILayout.FloatField(Styles.flowMapSpeedText, tem.x);
                tem.y = EditorGUILayout.FloatField(Styles.flowMapIntensityText, tem.y);
                //            materialEditor.ShaderProperty(mp_panning, Styles.panningText);
                mp_noisePanning.vectorValue = tem;
            }
        }
        else
        {
            materialEditor.ShaderProperty(mp_tintColor, Styles.tintColorText);
            materialEditor.ShaderProperty(mp_mainTex, Styles.mainTexText);

//            if (featureSet(mp_enableAutomaticPanning, Styles.enableAutomaticPanningText))
//            {
                Vector4 tem = mp_panning.vectorValue;
                Vector2 p1 = new Vector2(tem.x, tem.y);
                p1 = EditorGUILayout.Vector2Field(Styles.panningText, p1);
                //            materialEditor.ShaderProperty(mp_panning, Styles.panningText);
                tem.x = p1.x; tem.y = p1.y;
                mp_panning.vectorValue = tem;
//            }

            if (featureSet(mp_enableEmissivePower, Styles.enableEmissivePowerText))
            {
                materialEditor.ShaderProperty(mp_emissivePower, Styles.emissivePowerText);
            }

        }

        featureSet(mp_zTest, Styles.zTestText);
        featureSet(mp_cullMode, Styles.zCullMode);

        EditorGUILayout.BeginHorizontal(editorSkin.customStyles[2]);
        EditorGUILayout.LabelField(Styles.renderQueueText);
        int renderQueue = EditorGUILayout.IntField(material.renderQueue);
        if (material.renderQueue != renderQueue)
        {
            material.renderQueue = renderQueue;
        }
        EditorGUILayout.EndHorizontal();

        if (m_stencilParticles)
        {
            featureSet(mp_stencilMask, Styles.stencilMaskText);
            featureSet(mp_stencilCompare, Styles.stencilCompareText);
        }

        if (GUILayout.Button("Log keywords", editorSkin.customStyles[2]))
        {
            //            material.shaderKeywords = null;
            DebugKeywords(material);
        }

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
    /// Seek for all oldest particle shaders and replace with current Transparent Particles Standard shader
    /// </summary>
    [MenuItem("Tools/Particles/Replace old particles shaders")]
    public static void ReplaceOldParticleShaders()
    {
        Debug.Log("Obtaining material list");

        Material[] materialList;
        AssetFinder.FindAssetInContent<Material>(Directory.GetCurrentDirectory() + "\\Assets", out materialList);

        Shader shader = Shader.Find("Hungry Dragon/Particles/Transparent particles standard");

        int sChanged = 0;

        for (int c = 0; c < materialList.Length; c++)
        {
            Material mat = materialList[c];
            // TransparentAdditive.shader
            if (mat.shader.name == "Hungry Dragon/Particles/Transparent Additive")
            {
                changeMaterial(mat, shader, 0);
                setExtendedParticles(mat, false);
                sChanged++;
            }
            // TransparentSoftAdditive.shader
            if (mat.shader.name == "Hungry Dragon/Particles/Transparent Soft Additive")
            {
                changeMaterial(mat, shader, 1);
                setExtendedParticles(mat, false);
                sChanged++;
            }
            // TransparentAdditiveDouble.shader
            else if (mat.shader.name == "Hungry Dragon/Particles/Transparent Additive Double")
            {
                changeMaterial(mat, shader, 2);
                setExtendedParticles(mat, false);
                sChanged++;
            }
            // TransparentAlphaBlend.shader
            else if (mat.shader.name == "Hungry Dragon/Particles/Transparent Alpha Blend")
            {
                changeMaterial(mat, shader, 3);
                setExtendedParticles(mat, false);
                sChanged++;
            }
            // TransparentAdditiveAlphaBlend.shader
            else if (mat.shader.name == "Hungry Dragon/Particles/Transparent Additive Alpha Blend")
            {
                changeMaterial(mat, shader, 4);
                setExtendedParticles(mat, false);
                sChanged++;
            }
            // TransparentParticlesAdditive.shader
            else if (mat.shader.name == "Hungry Dragon/Particles/Transparent Particles Additive")
            {
                changeMaterial(mat, shader, 0);
                setExtendedParticles(mat, true);
                sChanged++;
            }
            // TransparentParticlesAlphaBlend.shader
            else if (mat.shader.name == "Hungry Dragon/Particles/Transparent Particles Alpha Blend")
            {
                changeMaterial(mat, shader, 3);
                setExtendedParticles(mat, true);
                sChanged++;
            }
            // TransparentParticlesPremultiply.shader
            else if (mat.shader.name == "Hungry Dragon/Particles/Transparent Particles Premultiply")
            {
                changeMaterial(mat, shader, 5);
                setExtendedParticles(mat, true);
                sChanged++;
            }
            // TransparentDissolve.shader
            else if (mat.shader.name == "Hungry Dragon/Particles/Transparent Dissolve")
            {
                int blendMode = (int)mat.GetFloat("_BlendMode");
                changeMaterial(mat, shader, (blendMode == 1) ? 0: 3);
                setExtendedParticles(mat, true);
                setDissolve(mat, true);
                mat.SetFloat("Dissolve", 1);
                sChanged++;
            }

        }

        Debug.Log(sChanged + " materials changed");
    }

    /// <summary>
    /// Dumps material keywords
    /// </summary>
    [MenuItem("Tools/Dump material keywords")]
    public static void DumpMaterialKeywords()
    {
        Material mat = Selection.activeObject as Material;
        if (mat != null)
        {
            DebugKeywords(mat);
        }
        else
        {
            Debug.Log("Selected asset isn't a Material!");
        }
    }


    /// <summary>
    /// Fix png textures
    /// </summary>
    [MenuItem("Tools/Fix png")]
    public static void FixPng()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        Debug.Log("Asset path: " + path);


        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex != null)
        {

            Color[] colarray = tex.GetPixels();
            for (int c = 0; c < colarray.Length; c++)
            {
                Color tem = colarray[c];
                tem.g = tem.a;
                colarray[c] = tem;
            }

            tex.SetPixels(colarray);
            tex.Apply();

            byte[] pngarray = tex.EncodeToPNG();

            File.WriteAllBytes(path, pngarray);
        }
        else
        {
            Debug.Log("Selected asset isn't a texture");
        }
    }


}