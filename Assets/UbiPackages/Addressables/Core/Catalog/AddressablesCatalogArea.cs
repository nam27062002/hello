using SimpleJSON;
using System.Collections.Generic;

public class AddressablesCatalogArea 
{
    private const string ATT_ID = "id";
    private const string ATT_ASSET_BUNDLES = "assetBundles";

    public string Id { get; private set; }

    public List<string> AssetBundleIds { get; set; }

    public AddressablesCatalogArea()
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
            LoadAssetBundlesList(data[ATT_ASSET_BUNDLES].AsArray);
        }
    }

    public JSONClass ToJSON()
    {        
        JSONClass data = new JSONClass();
        data[ATT_ID] = Id;
        data.Add(ATT_ASSET_BUNDLES, AssetBundlesListToJSON());

        return data;
    }

    public void LoadAssetBundlesList(JSONArray entries)
    {
        if (entries != null)
        {
            int count = entries.Count;
            string abName;
            for (int i = 0; i < count; ++i)
            {
                abName = entries[i];

                abName = abName.Trim();
                // Makes sure that there's no duplicates
                if (!string.IsNullOrEmpty(abName) && !AssetBundleIds.Contains(abName))
                {
                    AssetBundleIds.Add(abName);
                }
            }
        }
    }

    private JSONArray AssetBundlesListToJSON()
    {
        JSONArray data = new JSONArray();
        int count = AssetBundleIds.Count;
        for (int i = 0; i < count; i++)
        {
            if (!string.IsNullOrEmpty(AssetBundleIds[i]))
            {
                data.Add(AssetBundleIds[i]);
            }
        }

        return data;
    }
}
