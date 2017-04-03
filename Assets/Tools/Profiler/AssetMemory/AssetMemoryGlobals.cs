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
        Other
    };

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
    }
}
