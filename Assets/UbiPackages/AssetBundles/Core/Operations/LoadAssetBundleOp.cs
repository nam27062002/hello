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

    public AssetBundleHandle GetHandle()
    {
        return m_handle;
    }

    protected override void ExtendedPerform()
    {        
        m_request = AssetBundle.LoadFromFileAsync(m_handle.Path);        
    }

    protected override void ExtendedUpdate()
    {        
        if (m_request!= null && m_request.isDone)
        {
            AssetBundle ab = m_request.assetBundle;

            if (m_handle.LoadState == AssetBundlesTypes.ELoadState.Loading)
            {                
                m_handle.OnLoaded(ab);
                if (ab == null)
                {
                    NotifyError(EResult.Error_AB_Couldnt_Be_Loaded);
                }
                else
                {
                    NotifySuccess(ab);
                }
            }
            else
            {
                if (ab != null)
                {
                    ab.Unload(true);
                    NotifyError(EResult.Canceled);
                }
            }
        }        
    }

    protected override void UpdateAllowSceneActivation(bool value)
    {                
        if (m_request != null)
        {
            m_request.allowSceneActivation = value;
        }        
    }

    protected override float ExtendedProgress
    {
        get
        {            
            return (m_request == null) ? 0f : m_request.progress;
        }
    }
}
