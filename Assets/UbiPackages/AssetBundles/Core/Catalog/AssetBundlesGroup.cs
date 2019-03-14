using SimpleJSON;
using System.Collections.Generic;

public class AssetBundlesGroup
{
    private const string ATT_ID = "id";
    private const string ATT_ASSET_BUNDLES = "assetBundles";

    public string Id { get; private set; }

    public List<string> AssetBundleIds { get; set; }

    public AssetBundlesGroup()
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
            AssetBundleIds = UbiListUtils.JSONArrayToList(data[ATT_ASSET_BUNDLES].AsArray, true);
        }
    }

    public void ExpandDependencies()
    {
        AssetBundleIds = AssetBundlesManager.Instance.GetDependenciesIncludingSelfList(AssetBundleIds);
    }    

    public JSONClass ToJSON()
    {
        JSONClass data = new JSONClass();
        data[ATT_ID] = Id;
        data.Add(ATT_ASSET_BUNDLES, UbiListUtils.ListToJSONArray(AssetBundleIds));

        return data;
    }
    
    public void Setup(string id, List<string> assetBundleIds)
    {
        Reset();

        Id = id;
        UbiListUtils.AddRange(AssetBundleIds, assetBundleIds, false, true);
    }    

    public float GetMbLeftToDownload()
    {
        return AssetBundlesManager.Instance.GetAssetBundleListMbLeftToDownload(AssetBundleIds);
    }

    public float GetMbDownloadedSoFar()
    {
        return AssetBundlesManager.Instance.GetAssetBundleListMbDownloadedSoFar(AssetBundleIds);
    }

    public float GetTotalMb()
    {
        return AssetBundlesManager.Instance.GetAssetBundleListTotalMb(AssetBundleIds);
    }
}
