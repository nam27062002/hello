using UnityEngine;

/// <summary>
/// This class contains a set of constants to be used by the asset memory module
/// </summary>
public class AssetMemoryGlobals
{
    public enum EAssetType
    {
        Texture,
        Animation,
        Mesh,
        ParticleSystem,
        Other
    };

    public class AssetMemoryRawData
    {
        public string Name { get; set; }
        public long Bytes { get; set; }

        public AssetMemoryRawData(string name, long bytes)
        {
            Name = name;
            Bytes = bytes;
        }
    }

    public class GoExtended
    {
        public GameObject Go { get; set; }
        public string Label { get; set; }
        public AssetInformationStruct Info { get; set; }

        public GoExtended (GameObject go, string label, AssetInformationStruct info)
        {
            Go = go;
            Label = label;
            Info = info;
        }  
        
        public bool Contains(GameObject go)
        {
            return Go == go || (Info != null && Info.Contains(go));
        }

        public void RemoveGo(GameObject go)
        {
            if (Info != null && Info.Contains(go))
            {
                Info.RemoveGo(go);
            }
        }
    }
}
