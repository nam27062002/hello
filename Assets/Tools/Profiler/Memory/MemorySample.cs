using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Object = UnityEngine.Object;

public class MemorySample : AbstractMemorySample
{
    public class ObjectDetails
    {
        // in bytes
        public int MemSize { get; set; }
        public Object Obj { get; set; }
        public int InstanceID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        /// <summary>
        /// Constructor to be used when the object is available, its used typically when a sample is taken
        /// </summary>        
        public ObjectDetails(Object o, ESizeStrategy sizeStrategy)
        {
            Obj = o;
            Name = o.name;
            InstanceID = o.GetInstanceID();            
            Type = o.GetType().Name;
            CalculateSize(sizeStrategy);
        }

        /// <summary>
        /// Constructor to be used when these data are taken from a xml containing a sample
        /// </summary>        
        public ObjectDetails(string type, int instanceId, string name, int memSize)
        {
            Obj = null;
            InstanceID = instanceId;
            Name = name;
            Type = type;
            MemSize = memSize;
        }

        public void CalculateSize(ESizeStrategy sizeStrategy)
        {
            if (Obj != null)
            {				
                MemSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(Obj);

                if (sizeStrategy != ESizeStrategy.Profiler)
                {
					if(Obj is Texture) 
					{
						MemSize = (int)CalculateTextureSizeBytes(Obj as Texture, sizeStrategy);
					} 
					else if(Obj is AnimationClip) 
					{
						MemSize = (int)(MemSize * 0.3384f);
					} 
					else if(Obj is GameObject) 
					{
						MemSize = (int)(MemSize * 0.622f);
					} 
					else if(Obj is Transform) 
					{
						MemSize = (int)(MemSize * 0.7f);
					} 
					else if(Obj is ParticleSystem) 
					{
						MemSize = (int)(MemSize * 0.8f);
					}
					else if(Obj is ParticleSystemRenderer) 
					{
						MemSize = (int)(MemSize * 0.72f);
					}

                }               
            }
        }			      

        private static int GetBitsPerPixel(TextureFormat format)
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
                case TextureFormat.ETC2_RGBA1:
                    return 1;
                case TextureFormat.ETC2_RGBA8:
                    return 8;
                case TextureFormat.BGRA32://	 Format returned by iPhone camera
                    return 32;
            }
            return 0;
        }			       

        private static int CalculateTextureSizeBytes(Texture tTexture)
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

        private static float CalculateTextureSizeBytes(Texture tTexture, ESizeStrategy sizeStrategy)
        {           
            int tWidth = tTexture.width;
            int tHeight = tTexture.height;
            if (tTexture is Texture2D)
            {
                Texture2D tTex2D = tTexture as Texture2D;
				float bitsPerPixel = GetBitsPerPixel(tTexture, tTex2D.format, sizeStrategy);

                int mipMapCount = tTex2D.mipmapCount;
                int mipLevel = 1;
                float tSize = 0;
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
				float bitsPerPixel = GetBitsPerPixel(tTexture, tTex2D.format, sizeStrategy);
                int mipMapCount = 10;
                int mipLevel = 1;
                float tSize = 0;
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
				float bitsPerPixel = GetBitsPerPixel(tTexture, tCubemap.format, sizeStrategy);
                return (tWidth * tHeight * 6 * bitsPerPixel) / 8f;
            }
            return 0f;
        }

		private static bool IsPowerOfTwo(int x)
		{
			return (x != 0) && ((x & (x - 1)) == 0);
		}

		private static float GetBitsPerPixel(Texture texture, TextureFormat format, ESizeStrategy sizeStrategy) 
		{
			float returnValue = GetBitsPerPixel(format);

			if (texture.width == texture.height && IsPowerOfTwo(texture.width)) 
			{			
				// 1024x1024 non compressed textures tke up all the space according to unity profiler
				if(texture.width == 1024 && !IsACompressedFormat(format))
					return returnValue;
				
				switch (sizeStrategy)
				{
					case ESizeStrategy.DeviceHalf:
						returnValue /= 4;
						break;

					case ESizeStrategy.DeviceQuarter:
						returnValue /= 16;
						break;
				}
			}

			return returnValue;
		}

		private static List<TextureFormat> CompressedFormats = new List<TextureFormat>
		{
			TextureFormat.PVRTC_RGB2,
			TextureFormat.PVRTC_RGBA2,
			TextureFormat.PVRTC_RGB4,
			TextureFormat.PVRTC_RGBA4,
			TextureFormat.ETC_RGB4,
			TextureFormat.ETC_RGB4,
			TextureFormat.ETC2_RGBA8
		};

		private static bool IsACompressedFormat(TextureFormat format)
		{
			return CompressedFormats.Contains(format);
		}

    } 		
       
    private Dictionary<string, List<ObjectDetails>> Objects { get; set; }

    private List<Type> UNIQUE_TYPES = new List<Type>()
    {        
        typeof(AnimationClip),
        typeof(Animator),
        typeof(AudioClip),
        typeof(Avatar),
        typeof(Material),
        typeof(Mesh),
        typeof(Shader),
        typeof(Sprite),
        typeof(Texture),
        typeof(Texture2D)
    };

    public MemorySample(string name, ESizeStrategy sizeStrategy)
    {
        Clear();

        Name = name;
        SizeStrategy = sizeStrategy;        
    }

    public override void Clear()
    {
        TypeGroups_Clear();
        Name = null;      
    }   

    public void AddObject(Object o)
    {
        if (o != null)
        {          
            Type type = o.GetType();
            AddGeneric(type.Name, o);
        }
    }    

    private void AddGeneric(string typeName, Object o)
    {
        if (Objects == null)
        {
            Objects = new Dictionary<string, List<ObjectDetails>>();
        }
        
        if (!Objects.ContainsKey(typeName))
        {
            Objects.Add(typeName, new List<ObjectDetails>());
        }      

		ObjectDetails oDetails = FindObjectDetails(typeName, o.GetInstanceID());
		if ((oDetails == null))
        {
           	oDetails = new ObjectDetails(o, SizeStrategy);
            Objects[typeName].Add(oDetails);
        }
    }

    private void AddGeneric(string typeName, int objectId, string objectName, int memSize)
    {
        if (Objects == null)
        {
            Objects = new Dictionary<string, List<ObjectDetails>>();
        }

        if (!Objects.ContainsKey(typeName))
        {
            Objects.Add(typeName, new List<ObjectDetails>());
        }
        
        ObjectDetails oDetails = FindObjectDetails(typeName, objectId);
        if (oDetails == null)
        {
            oDetails = new ObjectDetails(typeName, objectId, objectName, memSize);
            Objects[typeName].Add(oDetails);
        }            
    }

    public void Analyze()
    {
        if (Objects != null)
        {
            foreach (KeyValuePair<string, List<ObjectDetails>> pair in Objects)
            {
                SortObjectDetailsList(pair.Value);                
            }
        }       
    }

    private void SortObjectDetailsList(List<ObjectDetails> list)
    {
        list.Sort
        (
            delegate (ObjectDetails details1, ObjectDetails details2) 
            {
                // They are sorted in descent order by their size and alphabetically if they have the same size
                int returnValue = details2.MemSize - details1.MemSize;
                if (returnValue == 0)
                {
                    returnValue = string.Compare(details1.Name, details2.Name);
                }

                return returnValue;               
            }
        );
    }

    public long GetMemorySizePerType(string type)
    {
        return GetMemorySizePerTypeInObjects(type, Objects);
    }

    public long GetMemorySizePerTypeGroup(string type)
    {
        return GetMemorySizePerTypeInObjects(type, TypeGroups_Objects);        
    }

    private long GetMemorySizePerTypeInObjects(string type, Dictionary<string, List<ObjectDetails>> objects)
    {
        long returnValue = 0;        
        if (objects != null && objects.ContainsKey(type))
        {
            int count = objects[type].Count;
            for (int i = 0; i < count; i++)
            {
                returnValue += objects[type][i].MemSize;
            }
        }

        return returnValue;
    }

    public override long GetTotalMemorySize()
    {
        long returnValue = 0;
        if (Objects != null)
        {
            foreach (KeyValuePair<string, List<ObjectDetails>> pair in Objects)
            {
                returnValue += GetMemorySizePerType(pair.Key);
            }
        }

        return returnValue;
    }
    
    private ObjectDetails FindObjectDetails(string type, Object o)
    {
        if (Objects != null && Objects.ContainsKey(type))
        {
            foreach (ObjectDetails details in Objects[type])
            {                
                if (details.Obj == o) return details;
            }
        }

        return null;
    }

    private ObjectDetails FindObjectDetails(string type, int objectId)
    {
        if (Objects != null && Objects.ContainsKey(type))
        {
            foreach (ObjectDetails details in Objects[type])
            {
                if (details.InstanceID == objectId) return details;
            }
        }

        return null;
    }

    #region type_groups
    // This region is responsible for classifying the sample in type groups, for example: Texture2D and Sprite are assigned to the same group "Textures".
    // This way the sample will be easier to read

    public const string TYPE_GROUPS_OTHER = "Other";

    private Dictionary<string, List<string>> TypeGroups { get; set; }

    private Dictionary<string, List<ObjectDetails>> TypeGroups_Objects { get; set; }    

    public void TypeGroups_Clear()
    {
        TypeGroups = null;

        if (TypeGroups_Objects != null)
        {
            TypeGroups_Objects.Clear();
        }
    }

    public void TypeGroups_Apply(Dictionary<string, List<string>> typeGroups)
    {
        TypeGroups_Clear();

        TypeGroups = TypeGroups;

        if (Objects != null)
        {
            if (TypeGroups_Objects == null)
            {
                TypeGroups_Objects = new Dictionary<string, List<ObjectDetails>>();
            }

            List<string> typesProcessed = new List<string>();
            List<ObjectDetails> objectDetailsList;
            int objectsCount;
            if (typeGroups != null)
            {
                string typeToProcess;
                int typeGroupsCount;
                foreach (KeyValuePair<string, List<string>> pair in typeGroups)
                {
                    objectDetailsList = new List<ObjectDetails>();
                    TypeGroups_Objects.Add(pair.Key, objectDetailsList);
                    
                    typeGroupsCount = pair.Value.Count;
                    for (int i = 0; i < typeGroupsCount; i++)
                    {
                        typeToProcess = pair.Value[i];
                        if (Objects.ContainsKey(typeToProcess))
                        {
                            typesProcessed.Add(typeToProcess);

                            objectsCount = Objects[typeToProcess].Count;
                            for (int j = 0; j < objectsCount; j++)
                            {
                                objectDetailsList.Add(Objects[typeToProcess][j]);
                            }
                        }
                    }                
                                        
                    SortObjectDetailsList(objectDetailsList);                    
                }

                // Loops through all types so the ones that haven't been included in any type group will be assigned to TYPE_GROUPS_OTHER type group
                objectDetailsList = new List<ObjectDetails>();
                TypeGroups_Objects.Add(TYPE_GROUPS_OTHER, objectDetailsList);
                foreach (KeyValuePair<string, List<ObjectDetails>> pair in Objects)
                {
                    if (!typesProcessed.Contains(pair.Key))
                    {
                        objectsCount = pair.Value.Count;
                        for (int i = 0; i < objectsCount; i++)
                        {
                            objectDetailsList.Add(pair.Value[i]);
                        }
                    }
                }
                
                SortObjectDetailsList(objectDetailsList);                
            }            
        }
    }

    public List<ObjectDetails> TypeGroups_GetObjectDetails(string typeGroup)
    {
        List<ObjectDetails> returnValue = null;
        if (TypeGroups_Objects != null && TypeGroups_Objects.ContainsKey(typeGroup))
        {
            returnValue = TypeGroups_Objects[typeGroup];
        }

        return returnValue;
    }    
    #endregion

    #region size
    protected override void Size_Recalculate()
    {
        // Size has to be calculated again
        if (Objects != null)
        {
            int count;
            foreach (KeyValuePair<string, List<ObjectDetails>> pair in Objects)
            {
                count = pair.Value.Count;
                for (int i = 0; i < count; i++)
                {
                    if (pair.Value[i].Obj != null)
                    {
                        pair.Value[i].CalculateSize(SizeStrategy);
                    }
                }

                SortObjectDetailsList(pair.Value);
            }
        }

        if (TypeGroups_Objects != null)
        {
            // These lists need to be sorted again since the size might have affected to the order                    
            foreach (KeyValuePair<string, List<ObjectDetails>> pair in TypeGroups_Objects)
            {
                // We don't need to recalculate the size because it was already done above since Objects and TypeGroups_Objects store the same
                // objects           
                SortObjectDetailsList(pair.Value);
            }
        }
    }
    #endregion

    #region xml    
    private long Xml_AddObjectDetailsList(XmlDocument xmlDoc, XmlNode rootNode, List<ObjectDetails> list, string listName, bool includeDetails)
    {
        XmlNode itemNode = xmlDoc.CreateElement(listName);
        rootNode.AppendChild(itemNode);

        long memSize = 0;
        XmlNode node;
        XmlAttribute attribute;
        int count = list.Count;
        for (int i = 0; i < count; i++)
        {
            memSize += list[i].MemSize;

            if (includeDetails)
            {
                node = xmlDoc.CreateElement(list[i].Type);
                node.InnerText = list[i].Name;
                itemNode.AppendChild(node);

                attribute = xmlDoc.CreateAttribute(XML_PARAM_ID);
                attribute.Value = "" + list[i].InstanceID;
                node.Attributes.Append(attribute);                

                attribute = xmlDoc.CreateAttribute(XML_PARAM_SIZE);
                attribute.Value = FormatSizeString(list[i].MemSize);
                node.Attributes.Append(attribute);

                attribute = xmlDoc.CreateAttribute(XML_PARAM_SIZE_RAW);
                attribute.Value = "" + list[i].MemSize;
                node.Attributes.Append(attribute);
            }
        }

        attribute = xmlDoc.CreateAttribute(XML_PARAM_SIZE);
        attribute.Value = FormatSizeString(memSize);
        itemNode.Attributes.Append(attribute);

        attribute = xmlDoc.CreateAttribute(XML_PARAM_SIZE_RAW);
        attribute.Value = "" + memSize;
        itemNode.Attributes.Append(attribute);

        attribute = xmlDoc.CreateAttribute(XML_PARAM_AMOUNT);
        attribute.Value = count + "";
        itemNode.Attributes.Append(attribute);

        return memSize;
    }

    /// <summary>
    /// Returns a <c>XmlNode</c> that contains all assets stored in this sample.
    /// </summary>
    /// <param name="typeGroups">If <c>null</c> the assets are classified by their type. It a non <c>null</c> value is passed then the types of the assets are classified
    /// in the typeGroups passed as a parameter</param>
    /// <returns></returns>
    public override XmlNode ToXML(XmlDocument xmlDoc = null, XmlNode rootNode = null, Dictionary<string, List<string>> typeGroups=null)
    {
        Dictionary<string, List<ObjectDetails>> objects;
        if (typeGroups != null)
        {
            TypeGroups_Apply(typeGroups);
            objects = TypeGroups_Objects;
        }
        else
        {
            objects = Objects;
        }

        // Header
        if (xmlDoc == null)
        {
            xmlDoc = new XmlDocument();
        }        

        XmlNode thisRootNode = xmlDoc.CreateElement(XML_PARAM_SAMPLE);        
        if (rootNode != null)
        {
            rootNode.AppendChild(thisRootNode);
        }
        else
        {
            xmlDoc.AppendChild(thisRootNode);
        }

        XmlAttribute attribute = xmlDoc.CreateAttribute(XML_PARAM_NAME);
        attribute.Value = Name;
        thisRootNode.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute(XML_PARAM_SIZE_STRATEGY);
        attribute.Value = SizeStrategy.ToString();
        thisRootNode.Attributes.Append(attribute);

        long totalMemory = 0;
        if (objects != null)
        {
            long memSize;
            foreach (KeyValuePair<string, List<ObjectDetails>> typeList in objects)
            {
                memSize = Xml_AddObjectDetailsList(xmlDoc, thisRootNode, typeList.Value, typeList.Key, typeList.Key != TYPE_GROUPS_OTHER);
                totalMemory += memSize;
            }
        }

        // Total memory taken up by all assets is set as an attribute of the header
        attribute = xmlDoc.CreateAttribute(XML_PARAM_SIZE);
        attribute.Value = FormatSizeString(totalMemory);
        thisRootNode.Attributes.Append(attribute);

        attribute = xmlDoc.CreateAttribute(XML_PARAM_SIZE_RAW);
        attribute.Value = "" + totalMemory;
        thisRootNode.Attributes.Append(attribute);

        return xmlDoc;
    }

    public override void FromXML(XmlNode xml)
    {
        if (xml != null)
        {            
            XmlAttributeCollection attributes = xml.Attributes;
            if (attributes != null)
            {
                Name = attributes[XML_PARAM_NAME].InnerText;
                string sizeStrategy = attributes[XML_PARAM_SIZE_STRATEGY].InnerText;
                if (!string.IsNullOrEmpty(sizeStrategy))
                {
                    SizeStrategy = (ESizeStrategy)Enum.Parse(typeof(ESizeStrategy), sizeStrategy);
                }
            }
            
            foreach (XmlNode node in xml.ChildNodes)
            {
                FromXMLChild(node);
            }
        }
    }

    private void FromXMLChild(XmlNode node)
    {
        if (node.ChildNodes.Count != 0)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                FromXMLChild(child);
            }
        }
        else if (node.ParentNode != null)
        {
            XmlAttributeCollection attributes = node.ParentNode.Attributes;
            if (attributes != null)
            {                
                string sizeAsString = attributes[XML_PARAM_SIZE_RAW].InnerText;
                string idAsString = attributes[XML_PARAM_ID].InnerText;
                string type = node.ParentNode.Name;
                string name = node.InnerText;

                int size = int.Parse(sizeAsString);
                int id = int.Parse(idAsString);
                AddGeneric(type, id, name, size);                
            }            
        }
    }
    #endregion
}
