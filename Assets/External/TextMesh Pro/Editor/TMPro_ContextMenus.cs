﻿// Copyright (C) 2014 - 2016 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;


namespace TMPro.EditorUtilities
{

    public class TMP_ContextMenus : Editor
    {

        private static Texture m_copiedTexture;

        private static Material m_copiedProperties;
        private static Material m_copiedAtlasProperties;


        // Add a Context Menu to the Texture Editor Panel to allow Copy / Paste of Texture.
        [MenuItem("CONTEXT/Texture/Copy", false, 2000)]
        static void CopyTexture(MenuCommand command)
        {
            m_copiedTexture = command.context as Texture;
        }


        // Add a Context Menu to allow easy duplication of the Material.
        //[MenuItem("CONTEXT/MaterialComponent/Duplicate Material", false)]
        [MenuItem("CONTEXT/Material/Duplicate Material", false)]
        static void DuplicateMaterial(MenuCommand command)
        {
            // Get the type of text object
            // If material is not a base material, we get material leaks...

            Material source_Mat = (Material)command.context;
            if (!EditorUtility.IsPersistent(source_Mat))
            {
                Debug.LogWarning("Material is an instance and cannot be converted into a permanent asset.");
                return;
            }


            string assetPath = AssetDatabase.GetAssetPath(source_Mat).Split('.')[0];

            Material duplicate = new Material(source_Mat);

            // Need to manually copy the shader keywords
            duplicate.shaderKeywords = source_Mat.shaderKeywords;

            AssetDatabase.CreateAsset(duplicate, AssetDatabase.GenerateUniqueAssetPath(assetPath + ".mat"));

            // Assign duplicate Material to selected object (if one is)
            if (Selection.activeGameObject != null)
            {
                TMP_Text textObject = Selection.activeGameObject.GetComponent<TMP_Text>();
                if (textObject != null)
                {
                    textObject.fontSharedMaterial = duplicate;
                }
                else
                {
                    TMP_SubMesh subMeshObject = Selection.activeGameObject.GetComponent<TMP_SubMesh>();

                    if (subMeshObject != null)
                        subMeshObject.sharedMaterial = duplicate;
                    else
                    {
                        TMP_SubMeshUI subMeshUIObject = Selection.activeGameObject.GetComponent<TMP_SubMeshUI>();

                        if (subMeshUIObject != null)
                            subMeshUIObject.sharedMaterial = duplicate;
                    }
                }
            }
        }


        //[MenuItem("CONTEXT/MaterialComponent/Copy Material Properties", false)]
        [MenuItem("CONTEXT/Material/Copy Material Properties", false)]
        static void CopyMaterialProperties(MenuCommand command)
        {
            Material mat = null;
            if (command.context.GetType() == typeof(Material))
                mat = (Material)command.context;
            else
            {
                mat = Selection.activeGameObject.GetComponent<CanvasRenderer>().GetMaterial();
            }

            m_copiedProperties = new Material(mat);

            m_copiedProperties.shaderKeywords = mat.shaderKeywords;

            m_copiedProperties.hideFlags = HideFlags.DontSave;
        }


        // PASTE MATERIAL
        //[MenuItem("CONTEXT/MaterialComponent/Paste Material Properties", false)]
        [MenuItem("CONTEXT/Material/Paste Material Properties", false)]
        static void PasteMaterialProperties(MenuCommand command)
        {

            if (m_copiedProperties == null)
            {
                Debug.LogWarning("No Material Properties to Paste. Use Copy Material Properties first.");
                return;
            }

            Material mat = null;
            if (command.context.GetType() == typeof(Material))
                mat = (Material)command.context;
            else
            {
                mat = Selection.activeGameObject.GetComponent<CanvasRenderer>().GetMaterial();
            }

            Undo.RecordObject(mat, "Paste Material");

            ShaderUtilities.GetShaderPropertyIDs(); // Make sure we have valid Property IDs
            if (mat.HasProperty(ShaderUtilities.ID_GradientScale))
            {
                // Preserve unique SDF properties from destination material.
                m_copiedProperties.SetTexture(ShaderUtilities.ID_MainTex, mat.GetTexture(ShaderUtilities.ID_MainTex));
                m_copiedProperties.SetFloat(ShaderUtilities.ID_GradientScale, mat.GetFloat(ShaderUtilities.ID_GradientScale));
                m_copiedProperties.SetFloat(ShaderUtilities.ID_TextureWidth, mat.GetFloat(ShaderUtilities.ID_TextureWidth));
                m_copiedProperties.SetFloat(ShaderUtilities.ID_TextureHeight, mat.GetFloat(ShaderUtilities.ID_TextureHeight));
            }

            EditorShaderUtilities.CopyMaterialProperties(m_copiedProperties, mat);

            // Copy ShaderKeywords from one material to the other.
            mat.shaderKeywords = m_copiedProperties.shaderKeywords;

            // Let TextMeshPro Objects that this mat has changed.
            TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(true, mat);
        }


        // Enable Resetting of Material properties without losing unique properties of the font atlas.
        //[MenuItem("CONTEXT/MaterialComponent/Reset", false, 2100)]
        [MenuItem("CONTEXT/Material/Reset", false, 2100)]
        static void ResetSettings(MenuCommand command)
        {

            Material mat = null;
            if (command.context.GetType() == typeof(Material))
                mat = (Material)command.context;
            else
            {
                mat = Selection.activeGameObject.GetComponent<CanvasRenderer>().GetMaterial();
            }


            //Material mat = (Material)command.context;
            Undo.RecordObject(mat, "Reset Material");

            Material tmp_mat = new Material(mat.shader);

            ShaderUtilities.GetShaderPropertyIDs(); // Make sure we have valid Property IDs
            if (mat.HasProperty(ShaderUtilities.ID_GradientScale))
            {
                // Copy unique properties of the SDF Material over to the temp material.  
                tmp_mat.SetTexture(ShaderUtilities.ID_MainTex, mat.GetTexture(ShaderUtilities.ID_MainTex));
                tmp_mat.SetFloat(ShaderUtilities.ID_GradientScale, mat.GetFloat(ShaderUtilities.ID_GradientScale));
                tmp_mat.SetFloat(ShaderUtilities.ID_TextureWidth, mat.GetFloat(ShaderUtilities.ID_TextureWidth));
                tmp_mat.SetFloat(ShaderUtilities.ID_TextureHeight, mat.GetFloat(ShaderUtilities.ID_TextureHeight));
                tmp_mat.SetFloat(ShaderUtilities.ID_StencilID, mat.GetFloat(ShaderUtilities.ID_StencilID));
                tmp_mat.SetFloat(ShaderUtilities.ID_StencilComp, mat.GetFloat(ShaderUtilities.ID_StencilComp));

                mat.CopyPropertiesFromMaterial(tmp_mat);

                // Reset ShaderKeywords
                mat.shaderKeywords = new string[0]; // { "BEVEL_OFF", "GLOW_OFF", "UNDERLAY_OFF" };
            }
            else
            {
                mat.CopyPropertiesFromMaterial(tmp_mat);
            }

            DestroyImmediate(tmp_mat);

            TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(true, mat);
        }



        //This function is used for debugging and fixing potentially broken font atlas links.
        [MenuItem("CONTEXT/Material/Copy Atlas", false, 2000)]
        static void CopyAtlas(MenuCommand command)
        {
            Material mat = command.context as Material;

            m_copiedAtlasProperties = new Material(mat);
            m_copiedAtlasProperties.hideFlags = HideFlags.DontSave;
        }


        // This function is used for debugging and fixing potentially broken font atlas links
        [MenuItem("CONTEXT/Material/Paste Atlas", false, 2001)]
        static void PasteAtlas(MenuCommand command)
        {
            Material mat = command.context as Material;

            if (m_copiedAtlasProperties != null)
            {
                Undo.RecordObject(mat, "Paste Texture");

                ShaderUtilities.GetShaderPropertyIDs(); // Make sure we have valid Property IDs
                mat.mainTexture = m_copiedAtlasProperties.mainTexture;
                mat.SetFloat(ShaderUtilities.ID_GradientScale, m_copiedAtlasProperties.GetFloat(ShaderUtilities.ID_GradientScale));
                mat.SetFloat(ShaderUtilities.ID_TextureWidth, m_copiedAtlasProperties.GetFloat(ShaderUtilities.ID_TextureWidth));
                mat.SetFloat(ShaderUtilities.ID_TextureHeight, m_copiedAtlasProperties.GetFloat(ShaderUtilities.ID_TextureHeight));
            }
            else if (m_copiedTexture != null)
            {
                Undo.RecordObject(mat, "Paste Texture");

                mat.mainTexture = m_copiedTexture;
            }

            //DestroyImmediate(m_copiedAtlasProperties);
        }


        // Context Menus for TMPro Font Assets
        //This function is used for debugging and fixing potentially broken font atlas links.
        [MenuItem("CONTEXT/TMP_FontAsset/Extract Atlas", false, 2000)]
        static void ExtractAtlas(MenuCommand command)
        {
            TMP_FontAsset font = command.context as TMP_FontAsset;
            Texture2D tex = Instantiate(font.material.mainTexture) as Texture2D;

            string fontPath = AssetDatabase.GetAssetPath(font);
            string texPath = Path.GetDirectoryName(fontPath) + "/" + Path.GetFileNameWithoutExtension(fontPath) + " Atlas.png";
            Debug.Log(texPath);
            // Saving File for Debug
            var pngData = tex.EncodeToPNG();
            File.WriteAllBytes(texPath, pngData);

            AssetDatabase.Refresh();
            DestroyImmediate(tex);
        }


        // Context Menus for TMPro Sprite Assets
        //This function is used for debugging and fixing potentially broken Sprite Asset material links.
        //[MenuItem("CONTEXT/TMP_SpriteAsset/Update Sprite Material", false, 2000)]
        //static void UpdateMaterial(MenuCommand command)
        //{
        //    TMP_SpriteAsset spriteAsset = command.context as TMP_SpriteAsset;

        //    ShaderUtilities.GetShaderPropertyIDs();

        //    if (spriteAsset.material == null)
        //    {
        //        // Add a new material
        //        Shader shader = Shader.Find("TMPro/Sprite");
        //        Material tempMaterial = new Material(shader);
        //        tempMaterial.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);

        //        spriteAsset.material = tempMaterial;
        //        tempMaterial.hideFlags = HideFlags.HideInHierarchy;
        //        AssetDatabase.AddObjectToAsset(tempMaterial, spriteAsset);


        //    }
        //    else
        //    {
        //        // Update the existing material
        //        DestroyImmediate(spriteAsset.material, true);

        //        Shader shader = Shader.Find("TMPro/Sprite");
        //        Material tempMaterial = new Material(shader);
        //        tempMaterial.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);

                
        //        tempMaterial.hideFlags = HideFlags.HideInHierarchy;
        //        AssetDatabase.AddObjectToAsset(tempMaterial, spriteAsset);
        //        spriteAsset.material = tempMaterial;

        //        //AssetDatabase.Refresh();
        //    }

        //    AssetDatabase.SaveAssets();
        //    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(spriteAsset));  // Re-import font asset to get the new updated version.
        //    AssetDatabase.Refresh();

        //}

    }
}