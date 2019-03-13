using SimpleJSON;
using System.Collections.Generic;

public class AddressablesCatalogGroup 
{
    private const string ATT_ID = "id";
    private const string ATT_ASSET_BUNDLES = "assetBundles";

    public string Id { get; private set; }

    public List<string> AssetBundleIds { get; set; }

    public AddressablesCatalogGroup()
    {
        AssetBundleIds = new List<string>();
    }

    public void Reset()
    {
        Id = null;
        AssetBundleIds.Clear();
    }

    public void Load(JSONNode data)
    {
        Reset();

        if (data != null)
        {
            Id = data[ATT_ID];
            UbiListUtils.JSONArrayToList(data[ATT_ASSET_BUNDLES].AsArray, AssetBundleIds, true);            
        }
    }

    public JSONClass ToJSON()
    {        
        JSONClass data = new JSONClass();
        data[ATT_ID] = Id;
        data.Add(ATT_ASSET_BUNDLES, UbiListUtils.ListToJSONArray(AssetBundleIds));

        return data;
    }       
}
