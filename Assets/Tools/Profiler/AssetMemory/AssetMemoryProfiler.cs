﻿using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// This class is responsible for returning a report with statistics about the footprint in memory of a set of game objects.
/// Two game objects that share the same assets won't double the total amount of memory consumed.
/// </summary>
public class AssetMemoryProfiler
{
    /// <summary>
    /// List of game objects to take into consideration for the analysis.
    /// </summary>
    public List<AssetMemoryGlobals.GoExtended> Gos;

    private bool ContainsGo(GameObject go)
    {
        return GetGoExtended(go) != null;
    }

    private AssetMemoryGlobals.GoExtended GetGoExtended(GameObject go)
    {
        AssetMemoryGlobals.GoExtended returnValue = null;
        if (Gos != null)
        {
            int count = Gos.Count;
            for (int i = 0; i < count && returnValue == null; i++)
            {
                if (Gos[i].Go == go)
                {
                    returnValue = Gos[i];
                }
            }
        }

        return returnValue;
    }

    public void Reset()
    {
        if (Gos != null)
        {
            Gos.Clear();
        }
    }

    public void AddGo(GameObject go, string label, string path="")
    {
        if (Gos == null)
        {
            Gos = new List<AssetMemoryGlobals.GoExtended>();
        }

        // Makes sure that go hasn't been registered yet
        if (!ContainsGo(go))
        {
            AssetInformationStruct info = new AssetInformationStruct(go.name, AssetMemoryGlobals.EAssetType.Other, path, UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(go));
            AssetMemoryGlobals.GoExtended goEx= new AssetMemoryGlobals.GoExtended(go, label, info);
            Gos.Add(goEx);

            AnalyzeObject(go, ref info);
        }
    }

    public void RemoveGo(GameObject go)
    {
        if (Gos != null)
        {
            AssetMemoryGlobals.GoExtended goEx = GetGoExtended(go);
            if (goEx != null)
            {
                Gos.Remove(goEx);
            }
        }
    }

    public long GetGoSize(GameObject go)
    {
        long returnValue = 0;
        AssetMemoryGlobals.GoExtended goEx = GetGoExtended(go);
        if (goEx != null)
        {
            returnValue = goEx.Info.GetSize();
        }

        return returnValue;
    }

    public long GetGoSizePerType(GameObject go, AssetMemoryGlobals.EAssetType type)
    {
        long returnValue = 0;
        AssetMemoryGlobals.GoExtended goEx = GetGoExtended(go);
        if (goEx != null)
        {
            returnValue = goEx.Info.GetSizePerType(type);
        }

        return returnValue;
    }

    public long GetSize()
    {
        long returnValue = 0;
        if (Gos != null)
        {
            List<string> assetPaths = new List<string>();

            int count = Gos.Count;
            for (int i = 0; i < count; i++)
            {
                returnValue += Gos[i].Info.GetSize(assetPaths);
            }
        }

        return returnValue;
    }

    public long GetSizePerType(AssetMemoryGlobals.EAssetType type)
    {
        long returnValue = 0;
        if (Gos != null)
        {
            List<string> assetPaths = new List<string>();

            int count = Gos.Count;
            for (int i = 0; i < count; i++)
            {
                returnValue += Gos[i].Info.GetSizePerType(type, assetPaths);
            }
        }

        return returnValue;
    }

    private void AnalyzeObject(GameObject go, ref AssetInformationStruct info)
    {
        AnalyzeAnimations(go, ref info);
        AnalyzeMeshes(go, ref info);

#if UNITY_EDITOR
        //AnalyzeTextureFromEditor(go, ref info);
        AnalyzeTextureFromShadersSettings(go, ref info);
#else
        AnalyzeTextureFromShadersSettings(go, ref info);        
#endif

        foreach (Transform child in go.GetComponentInChildren<Transform>())
        {
            AssetInformationStruct cc = new AssetInformationStruct(child.gameObject.name, AssetMemoryGlobals.EAssetType.Other);
            AnalyzeObject(child.gameObject, ref cc);
            info.AddChild(cc);
        }
    }

    private void AnalyzeTextureFromEditor(GameObject go, ref AssetInformationStruct info)
    {
        Shader shader;
        Texture texture;
        string subtype;
        int propertiesCount;
        Renderer[] renderers = go.GetComponents<Renderer>();
        foreach (Renderer ren in renderers)
        {
            if (ren.sharedMaterial != null)
            {
                shader = ren.sharedMaterial.shader;

                propertiesCount = UnityEditor.ShaderUtil.GetPropertyCount(shader);
                for (int i = 0; i < propertiesCount; i++)                    
                {
                    if (UnityEditor.ShaderUtil.GetPropertyType(shader, i) == UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        subtype = UnityEditor.ShaderUtil.GetPropertyName(shader, i);
                        texture = ren.sharedMaterial.GetTexture(subtype);
                        if (texture != null)
                        {
                            AssetInformationStruct diffuse = new AssetInformationStruct();
                            diffuse.Name = texture.name;
                            diffuse.Type = AssetMemoryGlobals.EAssetType.Texture;
                            diffuse.Subtype = subtype;
                            diffuse.Path = GetAssetPath(texture);

#if UNITY_EDITOR
                            diffuse.Size = CalculateTextureSizeBytes(texture);
#else
						    diffuse.Size = UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(texture);
#endif

                            info.AddChild(diffuse);
                        }
                    }
                }
            }                       
        }
    }
   
    private void AnalyzeTextureFromShadersSettings(GameObject go, ref AssetInformationStruct info)
    {
        Renderer[] renderers = go.GetComponents<Renderer>();
        List<string> textureNames;        
        foreach (Renderer ren in renderers)
        {
            if (ren.sharedMaterial != null)
            {
                textureNames = ShadersSettings_GetTextureNames(ren.sharedMaterial.shader.name);
                foreach (var texDef in textureNames)
                {
                    Texture texture = ren.sharedMaterial.GetTexture(texDef);
                    if (texture != null)
                    {
                        AssetInformationStruct diffuse = new AssetInformationStruct();
                        diffuse.Name = texture.name;
                        diffuse.Type = AssetMemoryGlobals.EAssetType.Texture;
                        diffuse.Subtype = texDef;
                        diffuse.Path = GetAssetPath(texture);

#if UNITY_EDITOR
                        diffuse.Size = CalculateTextureSizeBytes(texture);
#else
						diffuse.Size = UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(texture);
#endif

                        info.AddChild(diffuse);
                    }
                }

            }
        }

        //Resources.UnloadUnusedAssets();
    }   

    private void AnalyzeAnimations(GameObject go, ref AssetInformationStruct info)
    {
        Animator[] animators = go.GetComponents<Animator>();
        foreach (Animator animator in animators)
        {
            if (animator.runtimeAnimatorController != null)
            {
                foreach (AnimationClip anim in animator.runtimeAnimatorController.animationClips)
                {
                    AssetInformationStruct aa = new AssetInformationStruct(anim.name, AssetMemoryGlobals.EAssetType.Animation, 
                        GetAssetPath(anim), UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(anim));                    
                    info.AddChild(aa);
                }
            }
        }

        Animation[] anims = go.GetComponents<Animation>();
        foreach (Animation anim in anims)
        {                
            AssetInformationStruct aa = new AssetInformationStruct(anim.name, AssetMemoryGlobals.EAssetType.Animation, 
                anim.name, UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(anim));            
            info.AddChild(aa);                       
        }

    }

    private void AnalyzeMeshes(GameObject go, ref AssetInformationStruct info)
    {
        MeshFilter[] meshes = go.GetComponents<MeshFilter>();

        foreach (var mrender in meshes)
        {
            if (mrender.sharedMesh != null)
            {
                AssetInformationStruct aa = new AssetInformationStruct(mrender.sharedMesh.name, AssetMemoryGlobals.EAssetType.Mesh,
                    GetAssetPath(mrender.sharedMesh), UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(mrender.sharedMesh));                
                info.AddChild(aa);                
            }
        }

        SkinnedMeshRenderer[] renders = go.GetComponents<SkinnedMeshRenderer>();
        foreach (var mrender in renders)
        {
            if (mrender.sharedMesh != null)
            {
                AssetInformationStruct aa = new AssetInformationStruct(mrender.sharedMesh.name, AssetMemoryGlobals.EAssetType.Mesh,
                    GetAssetPath(mrender.sharedMesh), UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(mrender.sharedMesh));               
                info.AddChild(aa);                
            }
        }
    }

    private string GetAssetPath(UnityEngine.Object go)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.GetAssetPath(go);
#else
	    return "/PathUnknown/" + go.name;
#endif
    }

    public static int CalculateTextureSizeBytes(Texture tTexture)
    {
        int tWidth = tTexture.width;
        int tHeight = tTexture.height;
        if (tTexture is Texture2D)
        {
            Texture2D tTex2D = tTexture as Texture2D;
            int bitsPerPixel = GetBitsPerPixel(tTex2D.format);
            int mipMapCount = tTex2D.mipmapCount;
            int mipLevel = 1;
            int tSize = 0;
            while (mipLevel <= mipMapCount)
            {
                tSize += tWidth * tHeight * bitsPerPixel / 8;
                tWidth = tWidth / 2;
                tHeight = tHeight / 2;
                mipLevel++;
            }
            return tSize;
        }

        if (tTexture is Cubemap)
        {
            Cubemap tCubemap = tTexture as Cubemap;
            int bitsPerPixel = GetBitsPerPixel(tCubemap.format);
            return tWidth * tHeight * 6 * bitsPerPixel / 8;
        }
        return 0;
    }

    public static int GetBitsPerPixel(TextureFormat format)
    {
        switch (format)
        {
            case TextureFormat.Alpha8: // Alpha-only texture format.
                return 8;
            case TextureFormat.ARGB4444: // A 16 bits/pixel texture format. Texture stores color with an alpha channel.
                return 16;
            case TextureFormat.RGB24:   // A color texture format.
                return 24;
            case TextureFormat.RGBA32:  //Color with an alpha channel texture format.
                return 32;
            case TextureFormat.ARGB32:  //Color with an alpha channel texture format.
                return 32;
            case TextureFormat.RGB565:  // A 16 bit color texture format.
                return 16;
            case TextureFormat.DXT1:    // Compressed color texture format.
                return 4;
            case TextureFormat.DXT5:    // Compressed color with alpha channel texture format.
                return 8;
            /*
	        case TextureFormat.WiiI4: // Wii texture format.
	        case TextureFormat.WiiI8: // Wii texture format. Intensity 8 bit.
	        case TextureFormat.WiiIA4: // Wii texture format. Intensity + Alpha 8 bit (4 + 4).
	        case TextureFormat.WiiIA8: // Wii texture format. Intensity + Alpha 16 bit (8 + 8).
	        case TextureFormat.WiiRGB565: // Wii texture format. RGB 16 bit (565).
	        case TextureFormat.WiiRGB5A3: // Wii texture format. RGBA 16 bit (4443).
	        case TextureFormat.WiiRGBA8: // Wii texture format. RGBA 32 bit (8888).
	        case TextureFormat.WiiCMPR: // Compressed Wii texture format. 4 bits/texel, ~RGB8A1 (Outline alpha is not currently supported).
	        return 0; //Not supported yet
	        */
            case TextureFormat.PVRTC_RGB2:// PowerVR (iOS) 2 bits/pixel compressed color texture format.
                return 2;
            case TextureFormat.PVRTC_RGBA2:// PowerVR (iOS) 2 bits/pixel compressed with alpha channel texture format
                return 2;
            case TextureFormat.PVRTC_RGB4:// PowerVR (iOS) 4 bits/pixel compressed color texture format.
                return 4;
            case TextureFormat.PVRTC_RGBA4:// PowerVR (iOS) 4 bits/pixel compressed with alpha channel texture format
                return 4;
            case TextureFormat.ETC_RGB4:// ETC (GLES2.0) 4 bits/pixel compressed RGB texture format.
                return 4;
            case TextureFormat.ATC_RGB4:// ATC (ATITC) 4 bits/pixel compressed RGB texture format.
                return 4;
            case TextureFormat.ATC_RGBA8:// ATC (ATITC) 8 bits/pixel compressed RGB texture format.
                return 8;
            case TextureFormat.BGRA32:// Format returned by iPhone camera
                return 32;
                //case TextureFormat. ATF_RGB_DXT1:// Flash-specific RGB DXT1 compressed color texture format.
                //case TextureFormat.ATF_RGBA_JPG:// Flash-specific RGBA JPG-compressed color texture format.
                //case TextureFormat.ATF_RGB_JPG:// Flash-specific RGB JPG-compressed color texture format.
                //return 0; //Not supported yet
        }

        return 0;
    }

#region shaders_settings
    private const string SHADERS_SETTINGS_FILE = "shadersSettings";
    private const string SHADER_SETTINGS_ATT_ID = "id";
    private const string SHADER_SETTINGS_ATT_PROPERTIES = "properties";

    private static Dictionary<string, List<string>> m_shadersSettingsProperties;

    private static string ShadersSettings_GetFileNameFullPath()
    {
        return Application.dataPath + "/Resources/Profiler/" + SHADERS_SETTINGS_FILE + ".json";
    }

    public static void ShadersSettings_Reset()
    {
        if (m_shadersSettingsProperties != null)
        {
            m_shadersSettingsProperties.Clear();
        }
    }

    private static void ShadersSettings_LoadFromFile()
    {
        ShadersSettings_Reset();

        if (m_shadersSettingsProperties == null)
        {
            m_shadersSettingsProperties = new Dictionary<string, List<string>>();
        }

        TextAsset textAsset = (TextAsset)Resources.Load("Profiler/" + SHADERS_SETTINGS_FILE, typeof(TextAsset)); ;
        if (textAsset == null)
        {
            Debug.LogError("Could not load text asset " + SHADERS_SETTINGS_FILE);
        }
        else
        {            
            JSONNode data = JSONNode.Parse(textAsset.text);
            if (data != null)
            {
                // Spawners
                if (data.ContainsKey(SHADERS_SETTINGS_FILE))
                {
                    JSONArray shaders = data[SHADERS_SETTINGS_FILE] as JSONArray;
                    if (shaders != null)
                    {
                        string propertiesList;
                        string[] tokens;
                        List<string> properties;
                        int count = shaders.Count;                       
                        for (int i = 0; i < count; i++)
                        {
                            propertiesList = shaders[i][SHADER_SETTINGS_ATT_PROPERTIES];
                            properties = new List<string>();
                            if (propertiesList != null)
                            {
                                tokens = propertiesList.Split(',');
                                for (int j = 0; j < tokens.Length; j++)
                                {
                                    properties.Add(tokens[j]);
                                }
                            }

                            m_shadersSettingsProperties.Add(shaders[i][SHADER_SETTINGS_ATT_ID], properties);
                        }
                    }
                }
            }
        }        
    }

    public static void ShadersSettings_SaveToFile(Dictionary<string, List<string>> data)
    {        
        // Create new object, initialize and return it
        JSONClass jsonCollection = new JSONClass();        
        JSONArray root = new JSONArray();
        if (data != null)
        {
            foreach (KeyValuePair<string, List<string>> pair in data)
            {
                root.Add(ShadersSettings_ToEntryJSON(pair.Key, pair.Value));
            }
        }

        jsonCollection.Add(SHADERS_SETTINGS_FILE, root);

        string content = jsonCollection.ToString();
        ShadersSettings_SaveToFile(content);

        // Cache is deleted so the new information will be taken into consideration
        ShadersSettings_Reset();
    }

    private static JSONNode ShadersSettings_ToEntryJSON(string id, List<string> properties)
    {
        string propertiesString = "";
        if (properties != null)
        {
            int count = properties.Count;          
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    propertiesString += ",";
                }

                propertiesString += properties[i];
            }
        }

        JSONNode returnValue = new JSONClass();
        returnValue.Add(SHADER_SETTINGS_ATT_ID, id);
        returnValue.Add(SHADER_SETTINGS_ATT_PROPERTIES, propertiesString);
        return returnValue;
    }

    private static void ShadersSettings_SaveToFile(string content)
    {
        File.WriteAllText(ShadersSettings_GetFileNameFullPath(), content);
    }

    private static List<string> ShadersSettings_GetTextureNames(string shaderName)
    {
        if (m_shadersSettingsProperties == null)
        {
            ShadersSettings_LoadFromFile();
        }

        return (m_shadersSettingsProperties.ContainsKey(shaderName)) ? m_shadersSettingsProperties[shaderName] : null;               
    }
#endregion
}
