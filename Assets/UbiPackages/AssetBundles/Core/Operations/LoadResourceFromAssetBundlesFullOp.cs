using System.Collections.Generic;
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

        AssetBundlesManager abManager = AssetBundlesManager.Instance;
        AssetBundleHandle handle = abManager.GetAssetBundleHandle(assetBundleId);
        if (handle == null)
        {
            result = EResult.Error_AB_Handle_Not_Found;
            returnValue = true;
        }
        else
        {
            List<string> abIds = abManager.GetDependenciesIncludingSelf(assetBundleId);
            if (abManager.IsAssetBundleListAvailable(abIds) && abManager.IsAssetBundleListLoaded(abIds))
            {
                AssetBundle assetBundle = handle.AssetBundle;
                if (assetBundle == null)
                {
                    AssetBundlesManager.Log_AssertABIsNullButIsLoaded(assetBundleId);

                    result = EResult.Error_AB_Couldnt_Be_Loaded;
                    returnValue = true;
                }
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
        DownloadingAssetBundleAndDependencies,
        LoadingAssetBundleAndDependencies,
        LoadingResourceFromAssetBundle,
        Done
    };

    private EStep m_step;
    private EStep Step
    {
        get
        {
            return m_step;
        }

        set
        {
            m_step = value;

            AssetBundlesManager abManager = AssetBundlesManager.Instance;
            AssetBundleHandle handle = abManager.GetAssetBundleHandle(m_assetBundleId);
            List<string> abIds = abManager.GetDependenciesIncludingSelf(m_assetBundleId);

            switch (m_step)
            {
                case EStep.DownloadingAssetBundleAndDependencies:
                    if (abManager.IsAssetBundleListAvailable(abIds))
                    {
                        Step = EStep.LoadingAssetBundleAndDependencies;
                    }
                    else
                    {
                        SetRequest(AssetBundlesManager.Instance.DownloadAssetBundleAndDependencies(m_assetBundleId, OnAssetBundleDownloaded, true));
                    }
                    break;

                case EStep.LoadingAssetBundleAndDependencies:
                    if (abManager.IsAssetBundleListLoaded(abIds))
                    {
                        Step = EStep.LoadingResourceFromAssetBundle;
                    }
                    else
                    {
                        // Needs to load the asset bundle and its dependencies before loading the asset                        
                        SetRequest(AssetBundlesManager.Instance.LoadAssetBundleAndDependencies(m_assetBundleId, OnAssetBundleLoaded, true));
                    }
                    break;

                case EStep.LoadingResourceFromAssetBundle:
                    SetRequest(LoadResourceFromAssetBundle(handle.AssetBundle, m_resourceName, NotifyOnDoneInternal));
                    break;
            }
        }
    }

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
            Step = EStep.DownloadingAssetBundleAndDependencies;
        }                
    }
    
    private void OnAssetBundleDownloaded(EResult result, object data)
    {
        if (result == EResult.Success)
        {
            Step = EStep.LoadingAssetBundleAndDependencies;
        }

        if (IsResultError(result))
        {
            NotifyOnDoneInternal(result, null);
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
                Step = EStep.LoadingResourceFromAssetBundle;
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

                case EStep.LoadingAssetBundleAndDependencies:
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
