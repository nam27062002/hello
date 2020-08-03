using System.Collections.Generic;

/// <summary>
/// This class is responsible for unloading an asset bundles group and loading an asset bundles group
/// </summary>
public class UnloadAndLoadAssetBundlesGroupOp : UnloadAndLoadAssetBundlesOp
{    
    public void Setup(string groupToUnloadId, string groupToLoadId, bool isGroupToLoadMandatory=true, System.Action onUnloadDone=null)
    {        
        List<string> groupToUnloadDependencyIds = null;
        List<string> groupToLoadDependencyIds = null;
        List<string> assetBundleIdsToStay = null;

        AssetBundlesGroup group = AssetBundlesManager.Instance.GetAssetBundlesGroup(groupToUnloadId);
        if (group != null)
        {
            groupToUnloadDependencyIds = group.AssetBundleIds;
        }

        group = AssetBundlesManager.Instance.GetAssetBundlesGroup(groupToLoadId);
        if (group != null)
        {
            groupToLoadDependencyIds = group.AssetBundleIds;
        }

        // Only the asset bunldes that are not required by groupToLoadId need to be unloaded
        List<string> assetBundleIdsToUnload;
        UbiListUtils.SplitIntersectionAndDisjoint(groupToUnloadDependencyIds, groupToLoadDependencyIds, out assetBundleIdsToStay, out assetBundleIdsToUnload);

        List<string> assetBundleIdsToLoad;
        UbiListUtils.SplitIntersectionAndDisjoint(groupToLoadDependencyIds, assetBundleIdsToStay, out assetBundleIdsToStay, out assetBundleIdsToLoad);

        List<string> mandatoryIdsToLoad = (isGroupToLoadMandatory) ? assetBundleIdsToLoad : null;        
        Setup(assetBundleIdsToUnload, assetBundleIdsToLoad, mandatoryIdsToLoad, onUnloadDone);        
    }    
}
