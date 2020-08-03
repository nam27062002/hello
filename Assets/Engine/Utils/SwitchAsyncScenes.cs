using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is responsible for encapsulating all stuff needed to switch scenes asynchronously
/// </summary>
public class SwitchAsyncScenes
{
    public enum EState
    {
        NONE,
        UNLOADING_SCENES,        
        LOADING_EXTRA_DEPENDENCIES,
        LOADING_SCENES,
        ACTIVATING_SCENES,
        DONE
    };

    private EState mState;
    private EState State
    {
        get
        {
            return mState;
        }

        set
        {
            switch (mState)
            {
                case EState.UNLOADING_SCENES:
                    if (ExtraDependenciesToUnload != null)
                    {
                        HDAddressablesManager.Instance.UnloadDependencyIdsList(ExtraDependenciesToUnload);
                    }

                    if (OnUnload != null)
                    {
						OnUnload();
                    }
                    Resources.UnloadUnusedAssets();
                    System.GC.Collect();
                    break;

                case EState.LOADING_EXTRA_DEPENDENCIES:
                    if (OnDependenciesDone != null)
                    {
                        OnDependenciesDone();
                    }
                    break;
            }

            mState = value;

            switch (mState)
            {                
                case EState.UNLOADING_SCENES:
                    PrepareTasks(ScenesToUnload, ref mTasks, false);
                    break;                

                case EState.LOADING_EXTRA_DEPENDENCIES:
                    if (ExtraDependenciesToLoad != null)
                    {
                        AsyncOp = HDAddressablesManager.Instance.LoadDependencyIdsListAsync(ExtraDependenciesToLoad);
                    }                    
                    break;

                case EState.LOADING_SCENES:
                    PrepareTasks(ScenesToLoad, ref mTasks, true);
                    break;

                case EState.DONE:                    
                    if (OnDone != null)
                    {
                        OnDone();
                    }

                    Reset();
                    break;
            }
        }
    }

    private List<string> ScenesToUnload { get; set; }
    private List<string> ScenesToLoad { get; set; }

    private List<string> ExtraDependenciesToUnload { get; set; }
    private List<string> ExtraDependenciesToLoad { get; set; }
    private AddressablesOp AsyncOp { get; set; }

    private List<AddressablesOp> mTasks;
    private List<AddressablesOp> Tasks
    {
        get
        {
            return mTasks;
        }

        set
        {
            mTasks = value;                 
        }
    }

    private bool DelayActivationScenes { get; set; }

    private System.Action OnDone { get; set; }
	private System.Action OnUnload { get; set; }
    private System.Action OnDependenciesDone { get; set; }

    public SwitchAsyncScenes()
    {
        Reset();
        Tasks = new List<AddressablesOp>();
    }

    public void Reset()
    {
        State = EState.NONE;
        ScenesToUnload = null;
        ScenesToLoad = null;
        ExtraDependenciesToUnload = null;
        ExtraDependenciesToLoad = null;
        AsyncOp = null;

        if (Tasks != null)
        {
            Tasks.Clear();            
        }

        OnDone = null;
        OnUnload = null;
        OnDependenciesDone = null;
    }

    public void Perform(List<string> scenesToUnload, List<string> scenesToLoad, bool delayActivationScenes, List<string> extraDependenciesToUnload=null, List<string> extraDependenciesToLoad=null, 
                        System.Action onDone=null, System.Action onUnload=null, System.Action onDependenciesDone=null)
    {
        Reset();
        ScenesToUnload = scenesToUnload;
        ScenesToLoad = scenesToLoad;
        DelayActivationScenes = delayActivationScenes;

        List<string> assetBundleIdsToStay = null;

        // Only the asset bundles that are not required by rawAssetBundleIdsToLoad need to be unloaded
        List<string> assetBundleIdsToUnload;
        UbiListUtils.SplitIntersectionAndDisjoint(extraDependenciesToUnload, extraDependenciesToLoad, out assetBundleIdsToStay, out assetBundleIdsToUnload);

        List<string> assetBundleIdsToLoad;
        UbiListUtils.SplitIntersectionAndDisjoint(extraDependenciesToLoad, assetBundleIdsToStay, out assetBundleIdsToStay, out assetBundleIdsToLoad);

        ExtraDependenciesToUnload = assetBundleIdsToUnload;
        ExtraDependenciesToLoad = assetBundleIdsToLoad;
        OnDone = onDone;
		OnUnload = onUnload;
        OnDependenciesDone = onDependenciesDone;
        State = EState.UNLOADING_SCENES;
    }

    public void Update()
    {
        switch (State)
        {
            case EState.UNLOADING_SCENES:
            {
                bool done = true;
                if (Tasks != null)
                {                    
                    for (int i = 0; i < Tasks.Count && done; i++)
                    {
                        if (!Tasks[i].isDone)
                        {
                            done = false;
                        }
                    }
                }

                if (done)
                {
                    State = EState.LOADING_EXTRA_DEPENDENCIES;
                }
            }
            break;

            case EState.LOADING_EXTRA_DEPENDENCIES:
            {
                if (AsyncOp == null || AsyncOp.isDone)
                {
                    State = EState.LOADING_SCENES;
                }
            }
            break;

            case EState.LOADING_SCENES:
            {
                bool done = true;
                if (Tasks != null)
                {
                    for (int i = 0; i < Tasks.Count && done; i++)
                    {
                        if (DelayActivationScenes)
                        {
                            done = Tasks[i].progress >= 0.9f;
                        }
                        else
                        {
                            done = Tasks[i].isDone;
                        }
                    }                    
                }

                if (done)
                {
                    if (DelayActivationScenes)
                    {
                        if (Tasks != null)
                        {
                            for (int i = 0; i < Tasks.Count; i++)
                            {
                                Tasks[i].allowSceneActivation = true;
                            }
                        }

                        State = EState.ACTIVATING_SCENES;
                    } 
                    else
                    {
                            State = EState.DONE;
                    }                                          
                }
            }
            break;

            case EState.ACTIVATING_SCENES:
            {
                bool done = true;
                if (Tasks != null)
                {
                    for (int i = 0; i < Tasks.Count && done; i++)
                    {
                        done = Tasks[i].isDone;
                    }
                }  
                
                if (done)
                {
                    State = EState.DONE;
                }              
            }
            break;
        }
    }

    private void PrepareTasks(List<string> scenes, ref List<AddressablesOp> loadingTasks, bool load)
    {
        if (loadingTasks == null)
        {
            loadingTasks = new List<AddressablesOp>();
        }

        if (scenes != null)
        {
            AddressablesOp loadingTask = null;
            for (int i = 0; i < scenes.Count && !string.IsNullOrEmpty(scenes[i]); i++)
            {
                if (load)
                {
                    loadingTask = HDAddressablesManager.Instance.LoadSceneAsync(scenes[i], null, LoadSceneMode.Additive);
                }
                else
                {
                    loadingTask = HDAddressablesManager.Instance.UnloadSceneAsync(scenes[i]);
                }

                if (DebugUtils.SoftAssert(loadingTask != null && loadingTask.Error == null, "The scene " + scenes[i] + "  couldn't be found (probably mispelled or not added to Build Settings)"))
                {
                    loadingTasks.Add(loadingTask);
                }
            }
        }            
    }
}
