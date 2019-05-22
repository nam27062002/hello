using System.Collections.Generic;

/// <summary>
/// This class is responsible for unloading a list of asset bundles and loading another list of asset bundles
/// </summary>
public class UnloadAndLoadAssetBundlesOp : UbiAsyncOperation
{
    private enum EStep
    {
        None,
        UnloadingPreviousAssetBundleList,
        LoadingNextAssetBundleList,
        Done
    };

    private EStep m_step;
    private EStep Step
    {
        get { return m_step; }
        set
        {
            m_step = value;

            switch (m_step)
            {
                case EStep.None:
                case EStep.Done:
                    Request = null;
                    AssetBundleIdsToLoad = null;
                    AssetBundleIdsToUnload = null;
                    break;

                case EStep.UnloadingPreviousAssetBundleList:
                    if (AssetBundleIdsToUnload != null && AssetBundleIdsToUnload.Count > 0)
                    {
                        AssetBundlesManager.Instance.UnloadAssetBundleList(AssetBundleIdsToUnload);
                    }

                    Step = EStep.LoadingNextAssetBundleList;
                    break;

                case EStep.LoadingNextAssetBundleList:
                    if (AssetBundleIdsToLoad == null || AssetBundleIdsToLoad.Count == 0)
                    {
                        Step = EStep.Done;
                    }
                    else
                    {
                        Request = AssetBundlesManager.Instance.LoadAssetBundleList(AssetBundleIdsToLoad, null, true);
                    }
                    break;
            }
        }
    }

    private AssetBundlesOpRequest Request { get; set; }

    private List<string> AssetBundleIdsToUnload { get; set; }
    private List<string> AssetBundleIdsToLoad { get; set; }

    public void Setup(List<string> rawAssetBundleIdsToUnload, List<string> rawAssetBundleIdsToLoad)
    {
        Step = EStep.None;
        
        List<string> assetBundleIdsToStay = null;

        // Only the asset bundles that are not required by rawAssetBundleIdsToLoad need to be unloaded
        List<string> assetBundleIdsToUnload;
        UbiListUtils.SplitIntersectionAndDisjoint(rawAssetBundleIdsToUnload, rawAssetBundleIdsToLoad, out assetBundleIdsToStay, out assetBundleIdsToUnload);

        List<string> assetBundleIdsToLoad;
        UbiListUtils.SplitIntersectionAndDisjoint(rawAssetBundleIdsToLoad, assetBundleIdsToStay, out assetBundleIdsToStay, out assetBundleIdsToLoad);

        AssetBundleIdsToUnload = assetBundleIdsToUnload;
        AssetBundleIdsToLoad = assetBundleIdsToLoad;

        Step = EStep.UnloadingPreviousAssetBundleList;
    }

    public bool allowSceneActivation { get; set; }

    public bool isDone
    {
        get { return Step == EStep.Done; }
    }

    public float progress
    {
        get
        {
            float returnValue = 0f;

            switch (Step)
            {
                case EStep.LoadingNextAssetBundleList:
                    if (Request != null)
                    {
                        returnValue = Request.progress;
                    }
                    break;

                case EStep.Done:
                    returnValue = 1f;
                    break;
            }

            return returnValue;
        }
    }

    public void Cancel()
    {
        Step = EStep.None;
    }

    public void Update()
    {
        switch (Step)
        {
            case EStep.LoadingNextAssetBundleList:
                if (Request != null && Request.isDone)
                {
                    Step = EStep.Done;
                }
                break;
        }
    }
}
