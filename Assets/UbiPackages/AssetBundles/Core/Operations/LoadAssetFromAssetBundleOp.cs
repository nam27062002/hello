using UnityEngine;

/// <summary>
/// This class loads an asset asynchronously from an asset bundle that needs to be already downloaded and loaded. Use <c>LoadResourceFromAssetBundlesFullOp</c> class instead if you don't want to have to deal
/// with downloading and loading asset bundles. Actually this class is used as a helper for <c>LoadResourceFromAssetBundlesFullOp</c>.
/// </summary>
public class LoadAssetFromAssetBundleOp : AssetBundlesOp
{   
    private AssetBundle m_assetBundle;
    private string m_assetName;
    private AssetBundleRequest m_request;

    protected override void ExtendedReset()
    {
        m_assetBundle = null;
        m_assetName = null;
        m_request = null;
    }

    public void Setup(AssetBundle assetBundle, string assetName, OnDoneCallback onDone)
    {
        base.Setup(onDone);
        m_assetBundle = assetBundle;
        m_assetName = assetName;        
    }

    protected override void ExtendedPerform()
    {
        if (m_assetBundle == null)
        {
            NotifyError(EResult.Error_AB_Couldnt_Be_Loaded);
        }
        else
        {            
            m_request = m_assetBundle.LoadAssetAsync<object>(m_assetName);
            if (m_request == null)
            {
                NotifyError(EResult.Error_Asset_Not_Found_In_AB);
            }
        }
    }

    protected override void ExtendedUpdate()
    {
        if (m_request.isDone)
        {
            object asset = m_request.asset;
            if (asset != null)
            {               
                NotifySuccess(asset);                
            }
            else
            {
                NotifyError(EResult.Error_Asset_Not_Found_In_AB);
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
        get { return (m_request == null) ? 0f : m_request.progress; }
    }
}
