using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Object = UnityEngine.Object;

public class MemorySample
{
    public class TextureDetails : IEquatable<TextureDetails>
    {
        public bool isCubeMap;
        public int memSize; // in bytes
        public Texture texture;
        public TextureFormat format;
        public int mipMapCount;
        public List<Object> FoundInMaterials = new List<Object>();
        public List<Object> FoundInRenderers = new List<Object>();
        public List<Object> FoundInAnimators = new List<Object>();
        public List<Object> FoundInScripts = new List<Object>();
        public List<Object> FoundInGraphics = new List<Object>();
        public bool isSky;
        public bool instance;
        public bool isgui;
      
        public bool Equals(TextureDetails other)
        {
            return texture != null && other.texture != null &&
                texture.GetNativeTexturePtr() == other.texture.GetNativeTexturePtr();
        }

        public override int GetHashCode()
        {
            return (int)texture.GetNativeTexturePtr();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TextureDetails);
        }
    };

    public class MeshDetails
    {
        public int memSize; // in bytes
        
        private Mesh mesh;
        public Mesh Mesh
        {
            get { return mesh; }
            set
            {
                mesh = value;
                memSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(mesh);
            }
        }

        public List<GameObject> FoundInGos = new List<GameObject>();
        public List<MeshFilter> FoundInMeshFilters = new List<MeshFilter>();
        public List<SkinnedMeshRenderer> FoundInSkinnedMeshRenderer = new List<SkinnedMeshRenderer>();

        public bool instance;

        public MeshDetails()
        {
            instance = false;
        }
    };

    private List<TextureDetails> ActiveTextures { get; set; }
    private List<MeshDetails> ActiveMeshDetails { get; set; }

    // In bytes
    private long TotalTextureMemory { get; set; }

    // In bytes
    private long TotalMeshMemory { get; set; }

    public MemorySample()
    {
        Reset();
    }

    public void Reset()
    {
        if (ActiveTextures == null)
        {
            ActiveTextures = new List<TextureDetails>();
        }
        else
        {
            ActiveTextures.Clear();
        }

        TotalTextureMemory = 0;

        if (ActiveMeshDetails == null)
        {
            ActiveMeshDetails = new List<MeshDetails>();
        }
        else
        {
            ActiveMeshDetails.Clear();
        }

        TotalMeshMemory = 0;
    }

    public void AddTexture(Texture t)
    {
        // Makes sure that the texture hasn't been added yet to avoid duplicated textures
        TextureDetails tDetails = GetTextureDetail(t);
        if (!ActiveTextures.Contains(tDetails))
        {
            ActiveTextures.Add(tDetails);
        }
    }

    public void AddMesh(Mesh m)
    {
        // Makes sure that the mesh hasn't been added yet to avoid duplicated meshes
        MeshDetails mDetails = FindMeshDetails(m);
        if (mDetails == null)
        {
            mDetails = new MeshDetails();
            mDetails.Mesh = m;
            ActiveMeshDetails.Add(mDetails);
        }
    }

    public void CalculateStats()
    {
        TotalTextureMemory = 0;
        if (ActiveTextures != null)
        {
            // Sorts textures by size
            ActiveTextures.Sort(delegate (TextureDetails details1, TextureDetails details2) { return details2.memSize - details1.memSize; });

            foreach (TextureDetails tTextureDetails in ActiveTextures)
            {
                TotalTextureMemory += tTextureDetails.memSize;
            }
        }

        TotalMeshMemory = 0;
        if (ActiveMeshDetails != null)
        {
            foreach (MeshDetails tMeshDetails in ActiveMeshDetails)
            {
                TotalMeshMemory += tMeshDetails.memSize;
            }
        }
    }

    private const string PARAM_ASSETS = "Assets";
    private const string PARAM_TEXTURES = "Textures";
    private const string PARAM_TEXTURE = "Texture";
    private const string PARAM_MESHES = "Meshes";
    private const string PARAM_MESH = "Mesh";
    private const string PARAM_SIZE = "size";
    private const string PARAM_AMOUNT = "amount";

    public XmlNode ToXML()    
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode rootNode = xmlDoc.CreateElement(PARAM_ASSETS);
        xmlDoc.AppendChild(rootNode);


        //
        // Textures
        //
        XmlNode itemNode = xmlDoc.CreateElement(PARAM_TEXTURES);
        rootNode.AppendChild(itemNode);        
        XmlAttribute attribute = xmlDoc.CreateAttribute(PARAM_SIZE);
        attribute.Value = FormatSizeString(TotalTextureMemory);
        itemNode.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute(PARAM_AMOUNT);
        attribute.Value = ((ActiveTextures != null) ? ActiveTextures.Count : 0) +"";
        itemNode.Attributes.Append(attribute);        

        if (ActiveTextures != null)
        {
            XmlNode node;
            int count = ActiveTextures.Count;
            for (int i = 0; i < count; i++)
            {
                node = xmlDoc.CreateElement(PARAM_TEXTURE);
                attribute = xmlDoc.CreateAttribute(PARAM_SIZE);
                attribute.Value = FormatSizeString(ActiveTextures[i].memSize);
                node.Attributes.Append(attribute);
                node.InnerText = ActiveTextures[i].texture.name;
                itemNode.AppendChild(node);
            }
        }

        //
        // Meshes
        //
        itemNode = xmlDoc.CreateElement(PARAM_MESHES);
        rootNode.AppendChild(itemNode);                
        attribute = xmlDoc.CreateAttribute(PARAM_SIZE);
        attribute.Value = FormatSizeString(TotalMeshMemory);
        itemNode.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute(PARAM_AMOUNT);
        attribute.Value = ((ActiveMeshDetails != null) ? ActiveMeshDetails.Count : 0) + "";
        itemNode.Attributes.Append(attribute);
        
        /*if (ActiveMeshDetails != null)
        {
            XmlNode node;
            int count = ActiveMeshDetails.Count;
            for (int i = 0; i < count; i++)
            {
                node = xmlDoc.CreateElement(PARAM_MESH);
                attribute = xmlDoc.CreateAttribute(PARAM_SIZE);
                attribute.Value = FormatSizeString(ActiveMeshDetails[i].memSize);
                node.Attributes.Append(attribute);
                node.InnerText = ActiveMeshDetails[i].Mesh.name;
                itemNode.AppendChild(node);
            }
        }*/

        //xmlDoc.Save("test-doc.xml");
        return xmlDoc;
    }

    private string FormatSizeString(long bytes)
    {
        float memSizeKB = bytes / 1024.0f;
        if (memSizeKB < 1024f) return "" + memSizeKB + "k";
        else
        {
            float memSizeMB = ((float)memSizeKB) / 1024.0f;
            return memSizeMB.ToString("0.00") + "Mb";
        }
    }

    private TextureDetails FindTextureDetails(Texture tTexture)
    {
        foreach (TextureDetails tTextureDetails in ActiveTextures)
        {
            if (tTextureDetails.texture == tTexture) return tTextureDetails;
        }
        return null;

    }

    private TextureDetails GetTextureDetail(Texture tTexture)
    {
        TextureDetails tTextureDetails = FindTextureDetails(tTexture);
        if (tTextureDetails == null)
        {
            tTextureDetails = new TextureDetails();
            tTextureDetails.texture = tTexture;
            tTextureDetails.isCubeMap = tTexture is Cubemap;

            int memSize = CalculateTextureSizeBytes(tTexture);

            TextureFormat tFormat = TextureFormat.RGBA32;
            int tMipMapCount = 1;
            if (tTexture is Texture2D)
            {
                tFormat = (tTexture as Texture2D).format;
                tMipMapCount = (tTexture as Texture2D).mipmapCount;
            }
            if (tTexture is Cubemap)
            {
                tFormat = (tTexture as Cubemap).format;
                memSize = 8 * tTexture.height * tTexture.width;
            }
            if (tTexture is Texture2DArray)
            {
                tFormat = (tTexture as Texture2DArray).format;
                tMipMapCount = 10;
            }

            tTextureDetails.memSize = memSize;
            tTextureDetails.format = tFormat;
            tTextureDetails.mipMapCount = tMipMapCount;

        }

        return tTextureDetails;
    }

    private int GetBitsPerPixel(TextureFormat format)
    {
        switch (format)
        {
            case TextureFormat.Alpha8: //	 Alpha-only texture format.
                return 8;
            case TextureFormat.ARGB4444: //	 A 16 bits/pixel texture format. Texture stores color with an alpha channel.
                return 16;
            case TextureFormat.RGBA4444: //	 A 16 bits/pixel texture format.
                return 16;
            case TextureFormat.RGB24:   // A color texture format.
                return 24;
            case TextureFormat.RGBA32:  //Color with an alpha channel texture format.
                return 32;
            case TextureFormat.ARGB32:  //Color with an alpha channel texture format.
                return 32;
            case TextureFormat.RGB565:  //	 A 16 bit color texture format.
                return 16;
            case TextureFormat.DXT1:    // Compressed color texture format.
                return 4;
            case TextureFormat.DXT5:    // Compressed color with alpha channel texture format.
                return 8;
            /*
			case TextureFormat.WiiI4:	// Wii texture format.
			case TextureFormat.WiiI8:	// Wii texture format. Intensity 8 bit.
			case TextureFormat.WiiIA4:	// Wii texture format. Intensity + Alpha 8 bit (4 + 4).
			case TextureFormat.WiiIA8:	// Wii texture format. Intensity + Alpha 16 bit (8 + 8).
			case TextureFormat.WiiRGB565:	// Wii texture format. RGB 16 bit (565).
			case TextureFormat.WiiRGB5A3:	// Wii texture format. RGBA 16 bit (4443).
			case TextureFormat.WiiRGBA8:	// Wii texture format. RGBA 32 bit (8888).
			case TextureFormat.WiiCMPR:	//	 Compressed Wii texture format. 4 bits/texel, ~RGB8A1 (Outline alpha is not currently supported).
				return 0;  //Not supported yet
			*/
            case TextureFormat.PVRTC_RGB2://	 PowerVR (iOS) 2 bits/pixel compressed color texture format.
                return 2;
            case TextureFormat.PVRTC_RGBA2://	 PowerVR (iOS) 2 bits/pixel compressed with alpha channel texture format
                return 2;
            case TextureFormat.PVRTC_RGB4://	 PowerVR (iOS) 4 bits/pixel compressed color texture format.
                return 4;
            case TextureFormat.PVRTC_RGBA4://	 PowerVR (iOS) 4 bits/pixel compressed with alpha channel texture format
                return 4;
            case TextureFormat.ETC_RGB4://	 ETC (GLES2.0) 4 bits/pixel compressed RGB texture format.
                return 4;
            case TextureFormat.ATC_RGB4://	 ATC (ATITC) 4 bits/pixel compressed RGB texture format.
                return 4;
            case TextureFormat.ATC_RGBA8://	 ATC (ATITC) 8 bits/pixel compressed RGB texture format.
                return 8;
            case TextureFormat.BGRA32://	 Format returned by iPhone camera
                return 32;
#if !UNITY_5
			case TextureFormat.ATF_RGB_DXT1://	 Flash-specific RGB DXT1 compressed color texture format.
			case TextureFormat.ATF_RGBA_JPG://	 Flash-specific RGBA JPG-compressed color texture format.
			case TextureFormat.ATF_RGB_JPG://	 Flash-specific RGB JPG-compressed color texture format.
			return 0; //Not supported yet  
#endif
        }
        return 0;
    }

    private int CalculateTextureSizeBytes(Texture tTexture)
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
        if (tTexture is Texture2DArray)
        {
            Texture2DArray tTex2D = tTexture as Texture2DArray;
            int bitsPerPixel = GetBitsPerPixel(tTex2D.format);
            int mipMapCount = 10;
            int mipLevel = 1;
            int tSize = 0;
            while (mipLevel <= mipMapCount)
            {
                tSize += tWidth * tHeight * bitsPerPixel / 8;
                tWidth = tWidth / 2;
                tHeight = tHeight / 2;
                mipLevel++;
            }
            return tSize * ((Texture2DArray)tTex2D).depth;
        }
        if (tTexture is Cubemap)
        {
            Cubemap tCubemap = tTexture as Cubemap;
            int bitsPerPixel = GetBitsPerPixel(tCubemap.format);
            return tWidth * tHeight * 6 * bitsPerPixel / 8;
        }
        return 0;
    }

    private MeshDetails FindMeshDetails(Mesh tMesh)
    {
        foreach (MeshDetails tMeshDetails in ActiveMeshDetails)
        {
            if (tMeshDetails.Mesh.GetInstanceID() == tMesh.GetInstanceID()) return tMeshDetails;
        }
        return null;

    }
}
