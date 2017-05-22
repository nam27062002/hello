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
                    Resources.UnloadUnusedAssets();
                    System.GC.Collect();
                    break;
            }

            mState = value;

            switch (mState)
            {
                case EState.UNLOADING_SCENES:
                    PrepareTasks(ScenesToUnload, ref mTasks, false);
                    break;

                case EState.LOADING_SCENES:
                    PrepareTasks(ScenesToLoad, ref mTasks, true);
                    break;

                case EState.DONE:                    
                    if (OnDone != null)
                    {
                        OnDone();
                    }
                    break;
            }
        }
    }

    private List<string> ScenesToUnload { get; set; }
    private List<string> ScenesToLoad { get; set; }

    private List<AsyncOperation> mTasks;
    private List<AsyncOperation> Tasks
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

    public SwitchAsyncScenes()
    {
        Reset();
        Tasks = new List<AsyncOperation>();
    }

    public void Reset()
    {
        State = EState.NONE;
        ScenesToUnload = null;
        ScenesToLoad = null;

        if (Tasks != null)
        {
            Tasks.Clear();            
        }

        OnDone = null;
    }

    public void Perform(List<string> scenesToUnload, List<string> scenesToLoad, bool delayActivationScenes, System.Action onDone=null)
    {
        ScenesToUnload = scenesToUnload;
        ScenesToLoad = scenesToLoad;
        DelayActivationScenes = delayActivationScenes;
        OnDone = onDone;
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

    private void PrepareTasks(List<string> scenes, ref List<AsyncOperation> loadingTasks, bool load)
    {
        if (loadingTasks == null)
        {
            loadingTasks = new List<AsyncOperation>();
        }

        if (scenes != null)
        {
            AsyncOperation loadingTask = null;
            for (int i = 0; i < scenes.Count && !string.IsNullOrEmpty(scenes[i]); i++)
            {
                if (load)
                {
                    loadingTask = SceneManager.LoadSceneAsync(scenes[i], LoadSceneMode.Additive);
                }
                else
                {
                    loadingTask = SceneManager.UnloadSceneAsync(scenes[i]);
                }

                if (DebugUtils.SoftAssert(loadingTask != null, "The scene " + scenes[i] + "  couldn't be found (probably mispelled or not added to Build Settings)"))
                {
                    loadingTasks.Add(loadingTask);
                }
            }
        }            
    }
}
