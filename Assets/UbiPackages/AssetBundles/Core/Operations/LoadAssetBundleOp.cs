using UnityEngine;

/// <summary>
/// Loads an asset bundle from disk
/// </summary>
public class LoadAssetBundleOp : AssetBundlesOp
{
    private AssetBundleHandle m_handle;
    private AssetBundleCreateRequest m_request;

    protected override void ExtendedReset()
    {
        m_handle = null;
        m_request = null;
    }

    public void Setup(AssetBundleHandle handle, OnDoneCallback onDone)
    {
        base.Setup(onDone);

        m_handle = handle;        
    }    

    protected override void ExtendedPerform()
    {
        m_request = AssetBundle.LoadFromFileAsync(m_handle.LocalPath);        
    }

    protected override void ExtendedUpdate()
    {        
        if (m_request!= null && m_request.isDone)
        {
            AssetBundle ab = m_request.assetBundle;
            m_handle.OnLoaded(ab);
            if (ab == null)
            {
                NotifyError(AssetBundlesOp.EResult.Error_AB_Couldnt_Be_Loaded);
            }
            else
            {
                NotifySuccess(ab);
            }
        }        
    }
}
