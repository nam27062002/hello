using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class loads/unloads a scene asynchronously from an asset bundle that needs to be already downloaded and loaded. Use <c>LoadResourceFromAssetBundlesFullOp</c> class instead if you don't want to have to deal
/// with downloading and loading asset bundles.
/// </summary>
public class LoadSceneFromAssetBundleOp : AssetBundlesOp
{
    private bool m_isLoad;
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

    public void SetupLoad(AssetBundle assetBundle, string sceneName, LoadSceneMode loadSceneMode, OnDoneCallback onDone)
    {
        Setup(true, assetBundle, sceneName, loadSceneMode, onDone);
    }

    public void SetupUnload(AssetBundle assetBundle, string sceneName, OnDoneCallback onDone)
    {
        Setup(false, assetBundle, sceneName, LoadSceneMode.Single, onDone);
    }

    private void Setup(bool isLoad, AssetBundle assetBundle, string sceneName, LoadSceneMode loadSceneMode, OnDoneCallback onDone)
    {
        base.Setup(onDone);

        m_isLoad = isLoad;
        m_assetBundle = assetBundle;
        m_sceneName = sceneName;
        m_loadSceneMode = loadSceneMode;        
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
                if (m_isLoad)
                {
                    m_operation = SceneManager.LoadSceneAsync(m_sceneName, m_loadSceneMode);
                }
                else
                {
                    m_operation = SceneManager.UnloadSceneAsync(m_sceneName);
                }
                
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

    protected override void UpdateAllowSceneActivation(bool value)
    {
        if (m_operation != null)
        {
            m_operation.allowSceneActivation = value;
        }
    }

    protected override float ExtendedProgress
    {
        get { return (m_operation == null) ? 0f : m_operation.progress; }
    }
}
