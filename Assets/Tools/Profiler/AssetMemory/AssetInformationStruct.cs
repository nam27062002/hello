using System.Collections.Generic;

/// <summary>
/// This class is responsible for storing memori related information of a single asset
/// </summary>
public class AssetInformationStruct
{
    static void CalculateSize(AssetInformationStruct info, ref long size, ref List<string> assetPaths)
    {
        if (assetPaths.IndexOf(info.Path) == -1)
        {
            size += info.Size;
            if (!string.IsNullOrEmpty(info.Path))
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
        if (assetPaths.IndexOf(info.Path) == -1 && info.Type == type)
        {
            size += info.Size;
            if (!string.IsNullOrEmpty(info.Path))
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

    public string Name { get; set; }
    public string Path { get; set; }
    public AssetMemoryGlobals.EAssetType Type { get; set; }    
    public string Subtype { get; set; }

    public long Size { get; set; }

    public List<AssetInformationStruct> Children { get; set; }

    public AssetInformationStruct()
    {
        Reset();
    }

    public AssetInformationStruct(string name, AssetMemoryGlobals.EAssetType type, string path="", long size=0)
    {
        Reset();
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
}
