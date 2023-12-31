﻿using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for storing memori related information of a single asset
/// </summary>
public class AssetInformationStruct
{
    static bool IsTypeUnique(AssetMemoryGlobals.EAssetType type)
    {
        // Assets of these types have to be counted only once
        return type == AssetMemoryGlobals.EAssetType.Animation || type == AssetMemoryGlobals.EAssetType.Mesh || type == AssetMemoryGlobals.EAssetType.Texture;
    }

    static void CalculateSize(AssetInformationStruct info, ref long size, ref List<string> assetPaths)
    {
        if (!IsTypeUnique(info.Type) || assetPaths.IndexOf(info.Path) == -1)
        {
            size += info.Size;
            if (!string.IsNullOrEmpty(info.Path) && IsTypeUnique(info.Type))
            {
                assetPaths.Add(info.Path);
            }
        }

        if (info.Children != null)
        {
            foreach (AssetInformationStruct child in info.Children)
            {
                CalculateSize(child, ref size, ref assetPaths);
            }
        }
    }


    static void CalculateSizePerType(AssetInformationStruct info, AssetMemoryGlobals.EAssetType type, ref long size, ref List<string> assetPaths)
    {
        if (info.Type == type &&
            (!IsTypeUnique(info.Type) || assetPaths.IndexOf(info.Path) == -1))            
        {
            size += info.Size;
            if (!string.IsNullOrEmpty(info.Path) && IsTypeUnique(info.Type))
            {
                assetPaths.Add(info.Path);
            }
        }       

        if (info.Children != null)
        {
            foreach (AssetInformationStruct child in info.Children)
            {
                CalculateSizePerType(child, type, ref size, ref assetPaths);
            }
        }
    }

    static void CalculateDetailedSizePerType(AssetInformationStruct info, AssetMemoryGlobals.EAssetType type, ref Dictionary<string, long> assets)
    {
        if (info.Type == type && IsTypeUnique(info.Type) && !assets.ContainsKey(info.Path))
        {            
            if (string.IsNullOrEmpty(info.Path))
            {
                string emptyKey = "Empty";
                if (!assets.ContainsKey(emptyKey))
                {
                    assets.Add(emptyKey, 0);                                                    
                }

                assets[emptyKey] += info.Size;
            }
            else
            {
                assets.Add(info.Path, info.Size);
            }
        }

        if (info.Children != null)
        {
            foreach (AssetInformationStruct child in info.Children)
            {
                CalculateDetailedSizePerType(child, type, ref assets);
            }
        }
    }

    public GameObject Go { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public AssetMemoryGlobals.EAssetType Type { get; set; }    
    public string Subtype { get; set; }

    public long Size { get; set; }

    public List<AssetInformationStruct> Children { get; set; }    

    public AssetInformationStruct(GameObject go, string name, AssetMemoryGlobals.EAssetType type, string path="", long size=0)
    {
        Reset();
        Go = go;
        Name = name;
        Type = type;        
        Path = path;
        Size = size;
    }    

    private void Reset()
    {
        Name = "";
        Path = "";        
        Type = AssetMemoryGlobals.EAssetType.Other;
        Subtype = "";
        Size = 0;
    }

    public void AddChild(AssetInformationStruct child)
    {
        if (Children == null)
        {
            Children = new List<AssetInformationStruct>();
        }

        Children.Add(child);
    }

    /// <summary>
    /// Size including child objects
    /// </summary>
    public long GetSize(List<string> assetPaths=null)
    {        
        long value = 0;
        if (assetPaths == null)
        {
            assetPaths = new List<string>();
        }

        CalculateSize(this, ref value, ref assetPaths);

        return value;      
    }

    /// <summary>
    /// Size per type
    /// </summary>
    public long GetSizePerType(AssetMemoryGlobals.EAssetType type, List<string> assetPaths=null)
    {
        long value = 0;
        if (assetPaths == null)
        {
            assetPaths = new List<string>();
        }

        CalculateSizePerType(this, type, ref value, ref assetPaths);

        return value;
    }    

    public void GetDetailedSizePerType(AssetMemoryGlobals.EAssetType type, Dictionary<string, long> assets)
    {
        CalculateDetailedSizePerType(this, type, ref assets);
    }

    public bool Contains(GameObject go)
    {
        bool returnValue = Go == go;
        if (!returnValue && Children != null)
        {
            int count = Children.Count;
            for (int i = 0; i < count && !returnValue; i++)
            {
                returnValue = Children[i].Contains(go);
            }
        }

        return returnValue;
    }

    public void RemoveGo(GameObject go)
    {
        if (Children != null)
        {
            int count = Children.Count;
            for (int i = 0; i < Children.Count;)
            {
                if (Children[i].Contains(go))
                {
                    Children[i].RemoveGo(go);
                }

                if (Children[i].Go == go)
                {
                    Children.RemoveAt(i);
                }
                else
                {
                    i++;
                }                
            }
        }
    }
}
