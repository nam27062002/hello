using UnityEngine;

/// <summary>
/// This operation loads a resource (asset or scene) from an asset bundle, if this asset bundle or any dependencies haven't been downloaded or loaded yet then this operation resolves that 
/// before trying to load the resource.
/// </summary>
public abstract class LoadResourceFromAssetBundlesFullOp : AssetBundlesOp
{
    public static bool EarlyExit(string assetBundleId, string resourceName, OnDoneCallback onDone)
    {
        bool returnValue = false;
        EResult result = EResult.Success;
        object data = null;

        AssetBundleHandle handle = AssetBundlesManager.Instance.GetAssetBundleHandle(assetBundleId);
        if (handle == null)
        {
            result = EResult.Error_AB_Handle_Not_Found;
            returnValue = true;
        }
        else if (handle.IsLoaded())
        {
            AssetBundle assetBundle = handle.AssetBundle;
            if (assetBundle == null)
            {
                AssetBundlesManager.Log_AssertABIsNullButIsLoaded(assetBundleId);

                result = EResult.Error_AB_Couldnt_Be_Loaded;
                returnValue = true;
            }
        }

        if (returnValue && onDone != null)
        {
            onDone(result, data);
        }

        return returnValue;
    }


    private string m_assetBundleId;
    private string m_resourceName;    

    public void Setup(string assetBundleId, string resourceName, OnDoneCallback onDone)
    {
        m_assetBundleId = assetBundleId;
        m_resourceName = resourceName;        
        OnDone = onDone;
    }
        
    protected override void ExtendedPerform()
    {
        if (!EarlyExit(m_assetBundleId, m_resourceName, NotifyOnDone))
        {            
            // EarlyExit guarantees handle won't be null
            AssetBundleHandle handle = AssetBundlesManager.Instance.GetAssetBundleHandle(m_assetBundleId);
            if (handle.IsLoaded())
            {
                LoadResourceFromAssetBundle(handle.AssetBundle, m_resourceName, NotifyOnDone);                
            }
            else 
            {
                // Needs to load the asset bundle before loading the asset
                AssetBundlesManager.Instance.LoadAssetBundleAndDependencies(m_assetBundleId, OnAssetBundleLoaded);
            }
        }                
    }

    private void OnAssetBundleLoaded(EResult result, object data)
    {
        if (result == EResult.Success)
        {
            // EarlyExit guarantees handle won't be null
            AssetBundleHandle handle = AssetBundlesManager.Instance.GetAssetBundleHandle(m_assetBundleId);
            if (handle.IsLoaded())
            {
                LoadResourceFromAssetBundle(handle.AssetBundle, m_resourceName, NotifyOnDone);                
            }
            else
            {
                AssetBundlesManager.Log_AssertABIsNotLoadedButCallbackWasSuccessful(m_assetBundleId);
            }            
        }
        else
        {
            NotifyError(result);
        }
    }

    protected abstract void LoadResourceFromAssetBundle(AssetBundle assetBundle, string resourceName, OnDoneCallback onDone);    
}
