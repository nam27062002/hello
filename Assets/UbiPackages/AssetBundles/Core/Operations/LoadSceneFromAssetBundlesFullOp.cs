using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This operation loads a scene from an asset bundle, if this asset bundle or any dependencies haven't been downloaded or loaded yet then this operation resolves that 
/// before trying to load the resource.
/// </summary>
public class LoadSceneFromAssetBundlesFullOp : LoadResourceFromAssetBundlesFullOp
{
    private LoadSceneMode m_loadSceneMode;

    public void Setup(string assetBundleId, string sceneName, LoadSceneMode loadSceneMode, OnDoneCallback onDone)
    {
        m_loadSceneMode = loadSceneMode;
        base.Setup(assetBundleId, sceneName, onDone);
    }

    protected override void LoadResourceFromAssetBundle(AssetBundle assetBundle, string resourceName, OnDoneCallback onDone)
    {
        AssetBundlesManager.Instance.LoadSceneFromAssetBundleAsync(assetBundle, resourceName, m_loadSceneMode, onDone);
    }
}
