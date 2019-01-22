using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class loads a scene asynchronously from an asset bundle that needs to be already downloaded and loaded. Use <c>LoadResourceFromAssetBundlesFullOp</c> class instead if you don't want to have to deal
/// with downloading and loading asset bundles. Actually this class is used as a helper for <c>LoadResourceFromAssetBundlesFullOp</c>.
/// </summary>
public class LoadSceneFromAssetBundleOp : AssetBundlesOp
{
    private AssetBundle m_assetBundle;
    private string m_sceneName;
    private LoadSceneMode m_loadSceneMode;
    private AsyncOperation m_operation;

    protected override void ExtendedReset()
    {
        m_assetBundle = null;
        m_sceneName = null;
        m_operation = null;
    }

    public void Setup(AssetBundle assetBundle, string sceneName, LoadSceneMode loadSceneMode, OnDoneCallback onDone)
    {
        m_assetBundle = assetBundle;
        m_sceneName = sceneName;
        m_loadSceneMode = loadSceneMode;
        OnDone = onDone;
    }

    protected override void ExtendedPerform()
    {
        if (m_assetBundle == null)
        {
            NotifyError(EResult.Error_AB_Couldnt_Be_Loaded);
        }
        else
        {
            if (m_assetBundle.isStreamedSceneAssetBundle)
            {
                m_operation = SceneManager.LoadSceneAsync(m_sceneName, m_loadSceneMode);
                if (m_operation == null)
                {
                    NotifyError(EResult.Error_Asset_Not_Found_In_AB);
                }                
            }
            else
            {
                NotifyError(EResult.Error_AB_Is_Not_A_Scene_Bundle);
            }            
        }
    }

    protected override void ExtendedUpdate()
    {
        if (m_operation.isDone)
        {
            NotifySuccess(null);
        }
    }
}
