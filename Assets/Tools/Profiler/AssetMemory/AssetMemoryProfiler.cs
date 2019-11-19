using SimpleJSON;
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

    private Dictionary<string, List<AssetMemoryGlobals.GoExtended>> GosPerLabel;

    public List<string> Labels { get; set; }

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

        if (Labels != null)
        {
            Labels.Clear();
        }

        if (GosPerLabel != null)
        {
            GosPerLabel.Clear();
        }
    }

    public void AddGo(GameObject go, string label="", string path="")
    {
        if (Gos == null)
        {
            Gos = new List<AssetMemoryGlobals.GoExtended>();
        }

        // Makes sure that go hasn't been registered yet
        if (!ContainsGo(go))
        {
            AssetInformationStruct info = new AssetInformationStruct(go, go.name, AssetMemoryGlobals.EAssetType.Other, path, UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(go));
            AssetMemoryGlobals.GoExtended goEx= new AssetMemoryGlobals.GoExtended(go, label, info);
            Gos.Add(goEx);

            AnalyzeObject(go, ref info);

            if (Labels == null)
            {
                Labels = new List<string>();
            }

            if (!Labels.Contains(label))
            {
                Labels.Add(label);
            }

            if (GosPerLabel == null)
            {
                GosPerLabel = new Dictionary<string, List<AssetMemoryGlobals.GoExtended>>();
            }

            if (!GosPerLabel.ContainsKey(label))
            {
                GosPerLabel.Add(label, new List<AssetMemoryGlobals.GoExtended>());
            }

            GosPerLabel[label].Add(goEx);
        }       
    }

    public void RemoveGo(GameObject go)
    {
        if (Gos != null)
        {
            AssetMemoryGlobals.GoExtended goEx = GetGoExtended(go);
            if (goEx != null)
            {
                if (GosPerLabel != null && GosPerLabel.ContainsKey(goEx.Label))
                {
                    GosPerLabel[goEx.Label].Remove(goEx);
                    if (GosPerLabel[goEx.Label].Count == 0)
                    {
                        GosPerLabel.Remove(goEx.Label);
                        Labels.Remove(goEx.Label);
                    }
                }

                Gos.Remove(goEx);
            }
        }        
    }
        
    public void MoveGoToLabel(GameObject go, string label)
    {
        if (GosPerLabel != null)
        {            
            foreach (KeyValuePair<string, List<AssetMemoryGlobals.GoExtended>> pair in GosPerLabel)
            {
                if (pair.Key != label)
                {
                    if (Label_ContainsGo(pair.Key, go))
                    {
                        Label_RemoveGo(pair.Key, go);
                    }
                }
            }

            if (!Label_ContainsGo(label, go))
            {
                AddGo(go, label);
            }
        }        
    }

    private bool Label_ContainsGo(string label, GameObject go)
    {
        bool returnValue = false;

        if (GosPerLabel != null && GosPerLabel.ContainsKey(label))
        {
            List<AssetMemoryGlobals.GoExtended> gos = GosPerLabel[label];            
            if (gos != null)
            {
                int count = gos.Count;
                for (int i = 0; i < count && !returnValue; i++)
                {
                    if (gos[i].Contains(go))
                    {
                        returnValue = true;
                    }
                }
            }
        }

        return returnValue;
    }

    private void Label_RemoveGo(string label, GameObject go)
    {
        if (GosPerLabel != null && GosPerLabel.ContainsKey(label))
        {
            List<AssetMemoryGlobals.GoExtended> gos = GosPerLabel[label];
            if (gos != null)
            {                
                for (int i = 0; i < gos.Count;)
                {
                    if (gos[i].Contains(go))
                    {
                        gos[i].RemoveGo(go);                        
                    }

                    if (gos[i].Go == go)
                    {                        
                        RemoveGo(go);                     
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }
    }        

    public List<AssetMemoryGlobals.GoExtended> GetGosPerLabel(string label)
    {
        List<AssetMemoryGlobals.GoExtended> returnValue = null;
        if (GosPerLabel != null && GosPerLabel.ContainsKey(label))
        {
            returnValue = GosPerLabel[label];
        }

        return returnValue;
    }

    public void AddTexture(GameObject go, Texture texture, string label, string name)
    {
        if (texture != null)
        {
            if (Gos == null)
            {
                Gos = new List<AssetMemoryGlobals.GoExtended>();
            }

            AssetInformationStruct info = new AssetInformationStruct(go, name, AssetMemoryGlobals.EAssetType.Other, null, 0);
            AssetMemoryGlobals.GoExtended goEx = new AssetMemoryGlobals.GoExtended(null, label, info);
            Gos.Add(goEx);

            if (GosPerLabel == null)
            {
                GosPerLabel = new Dictionary<string, List<AssetMemoryGlobals.GoExtended>>();
            }

            if (Labels == null)
            {
                Labels = new List<string>();
            }

            if (!Labels.Contains(label))
            {
                Labels.Add(label);
            }

            if (!GosPerLabel.ContainsKey(label))
            {
                GosPerLabel.Add(label, new List<AssetMemoryGlobals.GoExtended>());
            }

            GosPerLabel[label].Add(goEx);

            AddTexture(go, texture, "RenderTexture", ref info);            
        }
    }

    private void AddTexture(GameObject go, Texture texture, string subtype, ref AssetInformationStruct info)
    {        
        if (texture != null)
        {
            int size = 0;
#if UNITY_EDITOR
            size = CalculateTextureSizeBytes(texture);
#else
            size = UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(texture);
#endif           
            AssetInformationStruct diffuse = new AssetInformationStruct(go, texture.name, AssetMemoryGlobals.EAssetType.Texture, GetAssetPath(texture), size);            
            info.AddChild(diffuse);
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

    /// <summary>
    /// Returns the memory size taken up by the game objects registered regardless their asset type
    /// </summary>
    /// <param name="label">If <c>null</c> then all game objects are taken into consideration. A non null value returns the memory size taken up only by game objects labelled with <c>label</c></param>
    /// <returns></returns>
    public long GetSize(string label=null)
    {
        long returnValue = 0;
        if (Gos != null)
        {
            List<string> assetPaths = new List<string>();

            List<AssetMemoryGlobals.GoExtended> gos = null;
            if (label == null)
            {
                gos = Gos;
            }
            else if (GosPerLabel != null && GosPerLabel.ContainsKey(label))
            {
                gos = GosPerLabel[label];
            }

            if (gos != null)
            {
                int count = gos.Count;
                for (int i = 0; i < count; i++)
                {
                    returnValue += gos[i].Info.GetSize(assetPaths);
                }
            }
        }

        return returnValue;
    }

    public long GetSizePerType(AssetMemoryGlobals.EAssetType type, string label=null)
    {
        long returnValue = 0;
        if (Gos != null)
        {
            List<string> assetPaths = new List<string>();

            List<AssetMemoryGlobals.GoExtended> gos = null;
            if (label == null)
            {
                gos = Gos;
            }
            else if (GosPerLabel != null && GosPerLabel.ContainsKey(label))
            {
                gos = GosPerLabel[label];
            }

            if (gos != null)
            {
                int count = gos.Count;
                for (int i = 0; i < count; i++)
                {
                    returnValue += gos[i].Info.GetSizePerType(type, assetPaths);
                }
            }
        }

        return returnValue;
    }

    public Dictionary<string, long> GetDetailedSizePerType(AssetMemoryGlobals.EAssetType type, string label = null)
    {
        Dictionary<string, long> returnValue = new Dictionary<string, long>();
        if (Gos != null)
        {            
            List<AssetMemoryGlobals.GoExtended> gos = null;
            if (label == null)
            {
                gos = Gos;
            }
            else if (GosPerLabel != null && GosPerLabel.ContainsKey(label))
            {
                gos = GosPerLabel[label];
            }

            if (gos != null)
            {
                int count = gos.Count;
                for (int i = 0; i < count; i++)
                {
                    gos[i].Info.GetDetailedSizePerType(type, returnValue);                    
                }
            }
        }

        return returnValue;
    }

    private void AnalyzeObject(GameObject go, ref AssetInformationStruct info)
    {
        AnalyzeAnimations(go, ref info);
        AnalyzeMeshes(go, ref info);

#if UNITY_EDITOR
        AnalyzeTextureFromEditor(go, ref info);
        //AnalyzeTextureFromShadersSettings(go, ref info);
#else
        AnalyzeTextureFromShadersSettings(go, ref info);        
#endif

        ParticleSystem p = go.GetComponent<ParticleSystem>();
        if (p != null)
        {
            AssetInformationStruct aa = new AssetInformationStruct(go, p.name, AssetMemoryGlobals.EAssetType.ParticleSystem,
                null, UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(p));
            info.AddChild(aa);
        }

        /*foreach (Transform child in go.GetComponentInChildren<Transform>())
        {
            AssetInformationStruct cc = new AssetInformationStruct(child.gameObject, child.gameObject.name, AssetMemoryGlobals.EAssetType.Other, "", UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(child.gameObject));
            AnalyzeObject(child.gameObject, ref cc);
            info.AddChild(cc);
        }*/
    }
		

#if UNITY_EDITOR
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
                        AddTexture(go, texture, subtype, ref info);                        
                    }
                }
            }                       
        }
    }
#endif
   
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
                    AddTexture(go, texture, texDef, ref info);
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
                    AssetInformationStruct aa = new AssetInformationStruct(go, anim.name, AssetMemoryGlobals.EAssetType.Animation, 
                        GetAssetPath(anim), UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(anim));                    
                    info.AddChild(aa);
                }
            }
        }

        Animation[] anims = go.GetComponents<Animation>();
        foreach (Animation anim in anims)
        {                
            AssetInformationStruct aa = new AssetInformationStruct(go, anim.name, AssetMemoryGlobals.EAssetType.Animation, 
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
                AssetInformationStruct aa = new AssetInformationStruct(go, mrender.sharedMesh.name, AssetMemoryGlobals.EAssetType.Mesh,
                    GetAssetPath(mrender.sharedMesh), UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(mrender.sharedMesh));                
                info.AddChild(aa);                
            }
        }

        SkinnedMeshRenderer[] renders = go.GetComponents<SkinnedMeshRenderer>();
        foreach (var mrender in renders)
        {
            if (mrender.sharedMesh != null)
            {
                AssetInformationStruct aa = new AssetInformationStruct(go, mrender.sharedMesh.name, AssetMemoryGlobals.EAssetType.Mesh,
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
            case TextureFormat.ETC2_RGBA8:// ATC (ATITC) 8 bits/pixel compressed RGB texture format.
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
