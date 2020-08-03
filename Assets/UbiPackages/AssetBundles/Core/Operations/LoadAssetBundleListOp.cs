using System;
using System.Collections.Generic;

/// <summary>
/// Operation responsible for requesting to load a list of asset bundles. This operation is done after all asset bundles have been requested to be loaded and they all have been loaded successfully or
/// as soon as an asset bundle hasn't been able to be loaded
/// </summary>
public class LoadAssetBundleListOp : AssetBundlesOp
{
    private List<string> m_assetBundleIds;

    protected override void ExtendedReset()
    {
        m_assetBundleIds = null;
    }

    public void Setup(List<string> assetBundleIds, OnDoneCallback onDone)
    {
        base.Setup(onDone);

        m_assetBundleIds = assetBundleIds;        
    }

    protected override void ExtendedPerform()
    {
        if (m_assetBundleIds == null || m_assetBundleIds.Count == 0)
        {
            NotifySuccess(null);
        }        
    }

    protected override void ExtendedUpdate() 
    {
        int absLoaded = 0;
        
        AssetBundleHandle handle;
        EResult result = EResult.Success;
        AssetBundlesTypes.ELoadState loadState;
        int count = m_assetBundleIds.Count;
        for (int i = 0; i < count && result == EResult.Success; i++)
        {
            handle = AssetBundlesManager.Instance.GetAssetBundleHandle(m_assetBundleIds[i]);
            if (handle == null)
            {
                result = EResult.Error_AB_Handle_Not_Found;
            }
            else
            {
                loadState = handle.LoadState;
                switch (loadState)
                {
                    case AssetBundlesTypes.ELoadState.Loaded:
                        absLoaded++;
                        break;

                    case AssetBundlesTypes.ELoadState.Error:
                        result = EResult.Error_AB_Couldnt_Be_Loaded;
                        break;
                }
            }
        }

        if (result == EResult.Success)
        {
            if (absLoaded == count)
            {
                NotifySuccess(null);
            }            
        }
        else
        {
            NotifyError(result);
        }
    }

    protected override float ExtendedProgress
    {
        get
        {
            float soFar = 0;
            int count = m_assetBundleIds.Count;
            for (int i = 0; i < count; i++)
            {
                AssetBundleHandle handle = AssetBundlesManager.Instance.GetAssetBundleHandle(m_assetBundleIds[i]);                
                if (handle != null)
                {
                    soFar += AssetBundlesManager.Instance.Loader_GetProgress(handle.Id);                                   
                }
            }

            return (count == 0) ? 1f : soFar / count;
        }
    }    
}
