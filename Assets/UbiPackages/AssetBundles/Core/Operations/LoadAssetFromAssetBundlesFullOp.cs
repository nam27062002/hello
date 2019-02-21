using UnityEngine;

/// <summary>
/// This operation loads an asset from an asset bundle, if this asset bundle or any dependencies haven't been downloaded or loaded yet then this operation resolves that 
/// before trying to load the resource.
/// </summary>
public class LoadAssetFromAssetBundlesFullOp : LoadResourceFromAssetBundlesFullOp
{
    protected override AssetBundlesOpRequest LoadResourceFromAssetBundle(AssetBundle assetBundle, string resourceName, OnDoneCallback onDone)
    {
        return AssetBundlesManager.Instance.LoadAssetFromAssetBundleAsync(assetBundle, resourceName, onDone, true);
    }    
}
