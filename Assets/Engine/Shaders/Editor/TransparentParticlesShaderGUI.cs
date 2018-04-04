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
    /*
        public enum BlendMode
        {
            Opaque,
            Cutout,
            Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
        }
    */


    private static class Styles
    {
        readonly public static string basicColorText = "Basic Color";
        readonly public static string saturatedColorText = "Saturated Color";
        readonly public static string mainTexText = "Particle Texture";
        readonly public static string colorRampText = "Color Ramp";
        readonly public static string emissionSaturationText = "Emission Saturation";
        readonly public static string opacitySaturationText = "Opacity Saturation";
        readonly public static string colorMultiplierText = "Color Multiplier";
        readonly public static string enableDissolveText = "Enable Alpha Dissolve";
        readonly public static string enableColorRampText = "Enable Color Ramp";
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
    MaterialProperty mp_emissionSaturation;
    MaterialProperty mp_opacitySaturation;
    MaterialProperty mp_colorMultiplier;
    MaterialProperty mp_dissolveStep;
    MaterialProperty mp_panning;
    MaterialProperty mp_tintColor;
    MaterialProperty mp_emissivePower;
    MaterialProperty mp_alphaBlendOffset;

    /// <summary>
    /// Toggle Material Properties
    /// </summary>
    MaterialProperty mp_enableDissolve;
    MaterialProperty mp_enableColorRamp;
    MaterialProperty mp_enableColorVertex;
    MaterialProperty mp_enableAutomaticPanning;
    MaterialProperty mp_enableEmissivePower;
    MaterialProperty mp_enableExtendedParticles;
    MaterialProperty mp_enableRBGColorVertex;

    /// <summary>
    /// Enum Material PProperties
    /// </summary>

    MaterialProperty mp_blendMode;
    MaterialProperty mp_srcBlend;
    MaterialProperty mp_dstBlend;
    MaterialProperty mp_zTest;

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
        mp_enableAutomaticPanning = FindProperty("_EnableAutomaticPanning", props);
        mp_enableEmissivePower = FindProperty("_EnableEmissivePower", props);
        mp_enableExtendedParticles = FindProperty("_EnableExtendedParticles", props);
        mp_enableColorVertex = FindProperty("_EnableColorVertex", props);

        /// Enum Material PProperties

        mp_enableDissolve = FindProperty("Dissolve", props);
        mp_blendMode = FindProperty("BlendMode", props);
        mp_srcBlend = FindProperty("_SrcBlend", props);
        mp_dstBlend = FindProperty("_DstBlend", props);
        mp_zTest = FindProperty("_ZTest", props);

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
        "BLENDMODE_PREMULTIPLY"
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
        }
    }

    private static string[] validKeyWords =
    {
        "EMISSIVEPOWER",
        "DISSOLVE_NONE",
        "DISSOLVE_ENABLED",
        "DISSOLVE_EXTENDED",
        "COLOR_RAMP",
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


        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginVertical(editorSkin.customStyles[2]);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(Styles.particlesText, GUILayout.Width(70));
        if (GUILayout.Button(Styles.standardParticlesText))
        {
            setExtendedParticles(material, false);
        }
        if (GUILayout.Button(Styles.extendedParticlesText))
        {
            setExtendedParticles(material, true);
        }
        //        m_materialEditor.ShaderProperty(feature, label);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

//        featureSet(mp_enableExtendedParticles, Styles.enableExtendedParticlesText);

        if (mp_enableExtendedParticles.floatValue > 0.5f)
        {
            materialEditor.ShaderProperty(mp_mainTex, Styles.mainTexText);
            if (featureSet(mp_enableAutomaticPanning, Styles.enableAutomaticPanningText))
            {
                Vector4 tem = mp_panning.vectorValue;
                Vector2 p1 = new Vector2(tem.x, tem.y);
                p1 = EditorGUILayout.Vector2Field(Styles.panningText, p1);
                //            materialEditor.ShaderProperty(mp_panning, Styles.panningText);
                tem.x = p1.x; tem.y = p1.y;
                mp_panning.vectorValue = tem;
            }

            featureSet(mp_enableColorVertex, Styles.enableColorVertexText);

            if (featureSet(mp_enableDissolve, Styles.enableDissolveText))
            {
                Vector4 tem = mp_dissolveStep.vectorValue;
                Vector2 p1 = new Vector2(tem.x, tem.y);

                p1.x = EditorGUILayout.Slider(Styles.dissolveStepMinText, p1.x, -1.0f, 1.0f);
                p1.y = EditorGUILayout.Slider(Styles.dissolveStepMaxText, p1.y, -1.0f, 1.0f);
                //                p1 = EditorGUILayout.Vector2Field(Styles.dissolveStepText, p1);
                //            materialEditor.ShaderProperty(mp_panning, Styles.panningText);
                tem.x = p1.x; tem.y = p1.y;
                mp_dissolveStep.vectorValue = tem;
//                materialEditor.ShaderProperty(mp_dissolveStep, Styles.dissolveStepText);
            }
            materialEditor.ShaderProperty(mp_opacitySaturation, Styles.opacitySaturationText);

            if (featureSet(mp_enableColorRamp, Styles.enableColorRampText))
            {
                materialEditor.ShaderProperty(mp_colorRamp, Styles.colorRampText);
            }
            else
            {
                materialEditor.ShaderProperty(mp_basicColor, Styles.basicColorText);
                materialEditor.ShaderProperty(mp_saturatedColor, Styles.saturatedColorText);
            }

            materialEditor.ShaderProperty(mp_emissionSaturation, Styles.emissionSaturationText);
        }
        else
        {
            materialEditor.ShaderProperty(mp_tintColor, Styles.tintColorText);
            materialEditor.ShaderProperty(mp_mainTex, Styles.mainTexText);

            if (featureSet(mp_enableAutomaticPanning, Styles.enableAutomaticPanningText))
            {
                Vector4 tem = mp_panning.vectorValue;
                Vector2 p1 = new Vector2(tem.x, tem.y);
                p1 = EditorGUILayout.Vector2Field(Styles.panningText, p1);
                //            materialEditor.ShaderProperty(mp_panning, Styles.panningText);
                tem.x = p1.x; tem.y = p1.y;
                mp_panning.vectorValue = tem;
            }

            if (featureSet(mp_enableEmissivePower, Styles.enableEmissivePowerText))
            {
                materialEditor.ShaderProperty(mp_emissivePower, Styles.emissivePowerText);
            }

        }


        featureSet(mp_zTest, Styles.zTestText);

        EditorGUILayout.BeginHorizontal(editorSkin.customStyles[2]);
        EditorGUILayout.LabelField(Styles.renderQueueText);
        int renderQueue = EditorGUILayout.IntField(material.renderQueue);
        if (material.renderQueue != renderQueue)
        {
            material.renderQueue = renderQueue;
        }
        EditorGUILayout.EndHorizontal();



        if (GUILayout.Button("Log keywords", editorSkin.customStyles[2]))
        {
            //            material.shaderKeywords = null;
            DebugKeywords(material);
        }


        /*
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

                Vector4 tem = mp_Panning.vectorValue;
                Vector2 p1 = new Vector2(tem.x, tem.y);

                featureSet(mp_MainColor, Styles.mainColorText);
                if (mp_MainColor.floatValue == 0.0f)
                {
                    materialEditor.TextureProperty(mp_mainTexture, Styles.mainTextureText);
                    materialEditor.TextureProperty(mp_normalTexture, Styles.normalTextureText, false);

                    p1 = EditorGUILayout.Vector2Field("Panning:", p1);
                    tem.x = p1.x;
                    tem.y = p1.y;


                    bool normalMap = mp_normalTexture.textureValue != null as Texture;

                    if (EditorGUI.EndChangeCheck())
                    {
                        SetKeyword(material, kw_normalmap, normalMap);
                        EditorUtility.SetDirty(material);
                        Debug.Log("EnableNormalMap " + (normalMap));
                        //            DebugKeywords(material);
                    }


                    if (normalMap)
                    {
                        materialEditor.ShaderProperty(mp_normalStrength, Styles.normalStrengthText);
                    }
                }
                else
                {
                    materialEditor.ShaderProperty(mp_Color, Styles.colorText);
                }

                EditorGUI.BeginChangeCheck();

                if (featureSet(mp_EnableBlendTexture, Styles.enableBlendTextureText))
                {
                    materialEditor.TextureProperty(mp_blendTexture, Styles.blendTextureText);
                    p1.Set(tem.z, tem.w);
                    p1 = EditorGUILayout.Vector2Field("Panning:", p1);
                    tem.z = p1.x;
                    tem.w = p1.y;

                    materialEditor.ShaderProperty(mp_EnableAdditiveBlend, Styles.additiveBlendingText);
                    materialEditor.ShaderProperty(mp_EnableAutomaticBlend, Styles.automaticBlendingText);
                }


                mp_Panning.vectorValue = tem;

                if (featureSet(mp_EnableSpecular, Styles.enableSpecularText))
                {
                    materialEditor.ShaderProperty(mp_specularPower, Styles.specularPowerText);
        //            RotationDrawer.setColor(mp_secondLightColor.colorValue);
        //            RotationDrawer.setTargetPoint(mp_specularDirection.vectorValue.x, mp_specularDirection.vectorValue.y);
                    RotationDrawer.setSpecularPow(mp_specularPower.floatValue);

                    materialEditor.ShaderProperty(mp_specularDirection, Styles.specularDirText);
                }

                featureSet(mp_EnableFog, Styles.enableFogText);

                featureSet(mp_VertexcolorMode, Styles.vertexColorModeText);
                featureSet(mp_EmissionType, Styles.emissionTypeText);

        //        if (featureSet(mp_EnableEmissiveBlink, Styles.enableEmissiveBlink))
                switch((int)mp_EmissionType.floatValue)
                {
                    case 0:         //Emission none
                    default:
                        break;

                    case 1:         //Emission blink
                        materialEditor.ShaderProperty(mp_EmissivePower, Styles.emissivePowerText);
                        materialEditor.ShaderProperty(mp_BlinkTimeMultiplier, Styles.blinkTimeMultiplierText);
                        break;

                    case 2:         //Emission reflective
                        materialEditor.ShaderProperty(mp_reflectionMap, Styles.reflectionMapText);
        //                materialEditor.ShaderProperty(mp_reflectionColor, Styles.reflectionColorText);
                        materialEditor.ShaderProperty(mp_reflectionAmount, Styles.reflectionAmountText);
                        EditorGUILayout.HelpBox(Styles.reflectionAdviceText, MessageType.Info);                
                        break;

                    case 3:         //Lightmap contrast
                        materialEditor.ShaderProperty(mp_lightmapContrastIntensity, Styles.lightmapContrastIntensityText);
                        materialEditor.ShaderProperty(mp_lightmapContrastMargin, Styles.lightmapContrastMarginText);
                        materialEditor.ShaderProperty(mp_lightmapContrastPhase, Styles.lightmapContrastPhaseText);

                        break;

                }
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
        */
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

        }

        Debug.Log(sChanged + " materials changed");
    }

    /// <summary>
    /// Seek for all transparent scenary standard materials and disable keyword OPAQUEALPHA
    /// </summary>
    [MenuItem("Tools/Scenary/Seek for additive blending materials")]
    public static void SeekAdditiveBlending()
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

//            if (mat.shader.name == "Hungry Dragon/Scenary/Scenary Standard")
            if (mat.shader == shader)
            {
                if (mat.IsKeywordEnabled("ADDITIVE_BLEND"))
                {
                    Debug.Log("Material name:" + mat.name);
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
                mat.SetFloat("_EnableFog", 1.0f);
                mat.SetFloat("_EnableOpaqueAlpha", 1.0f);

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
                mat.SetFloat("_EnableBlendTexture", 1.0f);

                mat.EnableKeyword("FOG");
                mat.EnableKeyword("OPAQUEALPHA");
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