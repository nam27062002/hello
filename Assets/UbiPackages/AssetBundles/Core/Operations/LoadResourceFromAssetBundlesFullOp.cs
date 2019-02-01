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

    private AssetBundlesOpRequest m_request;

    private enum EStep
    {
        None,
        LoadingAssetBundle,
        LoadingResourceFromAssetBundle,
        Done
    };
    
    private EStep Step { get; set; }

    protected override void ExtendedReset()
    {
        Step = EStep.None;
    }

    public void Setup(string assetBundleId, string resourceName, OnDoneCallback onDone)
    {
        Setup(onDone);

        m_assetBundleId = assetBundleId;
        m_resourceName = resourceName;                
        Step = EStep.None;
    }
        
    protected override void ExtendedPerform()
    {
        if (!EarlyExit(m_assetBundleId, m_resourceName, NotifyOnDone))
        {            
            // EarlyExit guarantees handle won't be null
            AssetBundleHandle handle = AssetBundlesManager.Instance.GetAssetBundleHandle(m_assetBundleId);
            if (handle.IsLoaded())
            {
                LoadResourceFromAssetBundleInternal(handle.AssetBundle);                
            }
            else 
            {
                // Needs to load the asset bundle before loading the asset
                Step = EStep.LoadingAssetBundle;                
                SetRequest(AssetBundlesManager.Instance.LoadAssetBundleAndDependencies(m_assetBundleId, OnAssetBundleLoaded, true));
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
                LoadResourceFromAssetBundleInternal(handle.AssetBundle);                
            }
            else
            {
                result = EResult.Error_Internal;
                AssetBundlesManager.Log_AssertABIsNotLoadedButCallbackWasSuccessful(m_assetBundleId);
            }            
        }
        
        if (IsResultError(result))
        {
            NotifyOnDoneInternal(result, null);            
        }
    }

    protected abstract AssetBundlesOpRequest LoadResourceFromAssetBundle(AssetBundle assetBundle, string resourceName, OnDoneCallback onDone);    

    private void LoadResourceFromAssetBundleInternal(AssetBundle assetBundle)
    {
        Step = EStep.LoadingResourceFromAssetBundle;
        SetRequest(LoadResourceFromAssetBundle(assetBundle, m_resourceName, NotifyOnDoneInternal));        
    }    

    private void NotifyOnDoneInternal(EResult result, object data)
    {
        Step = EStep.Done;
        NotifyOnDone(result, data);
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
            float returnValue = 0f;
            switch (Step)
            {
                case EStep.None:
                    break;

                case EStep.LoadingAssetBundle:
                case EStep.LoadingResourceFromAssetBundle:
                    if (m_request != null)
                    {
                        returnValue = m_request.progress;                        
                    }

                    if (Step == EStep.LoadingResourceFromAssetBundle)
                    {
                        returnValue += 1.0f;
                    }

                    returnValue /= 2f;
                    break;
                

                case EStep.Done:
                    returnValue = 1f;
                    break;
            }            

            return returnValue;
        }
    }

    private void SetRequest(AssetBundlesOpRequest request)
    {
        m_request = request;
        if (m_request != null && Step == EStep.LoadingResourceFromAssetBundle)
        {
            m_request.allowSceneActivation = AllowSceneActivation;
        }
    }
}
