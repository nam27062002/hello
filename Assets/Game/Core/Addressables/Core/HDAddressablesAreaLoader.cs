using System.Collections.Generic;

/// <summary>
/// This class is responsible for loading all dependencies requried to load a game area. HD is dividen in several game areas: menu, village, castle dark. 
/// When loading a new area all dependencies required by the previous area and not required by the new area are unloaded.
/// </summary>
public class HDAddressablesAreaLoader : UbiAsyncOperation
{
    private const float PERCENT_PROGRESS_FOR_DEPENDENCIES = 0.2f;

    private enum EStep
    {
        None,
        DealingWithDependencies,
        LoadingScene,
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
                case EStep.LoadingScene:                 
                    m_loadingSceneOp = HDAddressablesManager.Instance.LoadSceneAsync(m_sceneToLoad);
                    break;
            }
        }
    }

    private AddressablesOp m_loadingSceneOp;
    private UnloadAndLoadAssetBundlesOp m_dependenciesOp;
    private string m_sceneToLoad;

    // Not implemented yet
    public bool allowSceneActivation { get; set; }

    private bool LoadAddressablesInLoading { get; set; }

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
                case EStep.DealingWithDependencies:
                    returnValue = m_dependenciesOp.progress * PERCENT_PROGRESS_FOR_DEPENDENCIES;
                    break;

                case EStep.LoadingScene:
                    returnValue = PERCENT_PROGRESS_FOR_DEPENDENCIES + m_dependenciesOp.progress * (1 - PERCENT_PROGRESS_FOR_DEPENDENCIES);
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
        Reset();
    }    

    public void Reset()
    {
        if (m_dependenciesOp != null)
        {
            m_dependenciesOp.Cancel();
        }

        if (m_loadingSceneOp != null)
        {
            m_loadingSceneOp.Cancel();
            m_loadingSceneOp = null;
        }

        m_sceneToLoad = null;

        Step = EStep.None;

        LoadAddressablesInLoading = false;
    }

    public void Setup(AddressablesBatchHandle handle, string sceneToLoad, bool loadAddressablesInLoading=false)
    {
        Reset();

        if (m_dependenciesOp == null)
        {
            m_dependenciesOp = new UnloadAndLoadAssetBundlesOp();
        }

        List<string> currentDependencyIdsLoaded = new List<string>();
        HDAddressablesManager.Instance.FillWithLoadedAssetBundleIdList(currentDependencyIdsLoaded);

        LoadAddressablesInLoading = loadAddressablesInLoading;

        m_dependenciesOp.Setup(currentDependencyIdsLoaded, handle.DependencyIds, handle.MandatoryDependencyIds, OnUnloadABDone);
        m_sceneToLoad = sceneToLoad;        

        Step = EStep.DealingWithDependencies;
    }

    private void OnUnloadABDone()
    {
        if (LoadAddressablesInLoading)
        {
            LoadingScreen.LoadAddressables();
        }
    }

    public void Update()
    {
        switch (Step)
        {
            case EStep.DealingWithDependencies:
                m_dependenciesOp.Update();
                if (m_dependenciesOp.isDone)
                {
                    Step = EStep.LoadingScene;
                }
                break;

            case EStep.LoadingScene:
                if (m_loadingSceneOp.isDone)
                {
                    Step = EStep.Done;
                }
                break;
        }
    }
}
