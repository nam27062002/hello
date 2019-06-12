using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper for loading a level
/// </summary>
public class LevelLoader
{    
    
    public enum EState
    {
        None,
        UnloadingPrevAreaScenes,
        UnloadingPrevAreaDependencies,
        LoadingNextAreaDependencies,
        LoadingNextAreaScenes,
        WaitingToActivateNextAreaScences,
        ActivatingNextAreaScenes,
        Done
    }

    private EState m_state;

    private EState State
    {
        get
        {
            return m_state;
        }

        set
        {
            switch (m_state)
            {
                case EState.UnloadingPrevAreaDependencies:
                    Resources.UnloadUnusedAssets();
                    System.GC.Collect();
                    break;
            }

            EState preVState = m_state;
            m_state = value;

            if (OnChangeState != null)
            {
                OnChangeState(preVState, m_state);
            }

            // ActivatingNextAreaScenes state reuses the same tasks as LoadingNextAreaScenes state
            if (State != EState.ActivatingNextAreaScenes && State != EState.WaitingToActivateNextAreaScences)
            {
                m_tasks = null;
            }

            switch (m_state)
            {
                case EState.UnloadingPrevAreaScenes:
                    if (m_loadLevelHandle.NeedsToUnloadPrevAreaScenes())
                    {
                        m_tasks = m_loadLevelHandle.UnloadPrevAreaScenes();
                    }                                        
                    break;

                case EState.UnloadingPrevAreaDependencies:
                    if (m_loadLevelHandle.NeedsToUnloadPrevAreaDependencies())
                    {
                        m_loadLevelHandle.UnloadPrevAreaDependencies();
                    }

                    State = EState.LoadingNextAreaDependencies;
                    break;

                case EState.LoadingNextAreaDependencies:
                    if (m_loadLevelHandle.NeedsToLoadNextAreaDependencies())
                    {
                        m_tasks = new List<AddressablesOp>();
                        m_tasks.Add(m_loadLevelHandle.LoadNextAreaDependencies());
                    }                    
                    break;

                case EState.LoadingNextAreaScenes:
                    if (m_loadLevelHandle.NeedsToLoadNextAreaScenes())
                    {
                        if (m_loadSync)
                        {
                            m_loadLevelHandle.LoadNextAreaScenes();                            
                        }
                        else
                        {
                            m_tasks = m_loadLevelHandle.LoadNextAreaScenesAsync();
                            SetAllowSceneActivation(m_tasks, false);
                        }
                    }
                    break;

                case EState.ActivatingNextAreaScenes:
                    // Reuses the same tasks as the previous state   
                    if (!m_loadSync)
                    {
                        SetAllowSceneActivation(m_tasks, true);
                    }
                    break;
            }
        }
    }

    public delegate void OnChangeStateCallback(EState prevState, EState newState);
    private OnChangeStateCallback OnChangeState;

    private string m_prevArea;
    private string m_nextArea;

    private List<string> m_realSceneNamesToLoad;
    private List<string> m_realSceneNamesToUnload;

    private HDAddressablesManager.Ingame_SwitchAreaHandle m_loadLevelHandle;

    private List<AddressablesOp> m_tasks;

    private bool m_loadSync;

    private List<string> m_dependencyIdsToStayInMemory;

    public LevelLoader(string prevArea, string nextArea)
    {
        m_prevArea = prevArea;
        m_nextArea = nextArea;

        AddressablesBatchHandle handle = HDAddressablesManager.Instance.GetAddressablesAreaBatchHandle(GameSceneController.NAME);
        m_dependencyIdsToStayInMemory = handle.DependencyIds;
    }

    public void Reset()
    {
        if (m_realSceneNamesToLoad != null)
        {
            m_realSceneNamesToLoad.Clear();
        }

        if (m_realSceneNamesToUnload != null)
        {
            m_realSceneNamesToUnload.Clear();
        }
        
        State = EState.None;
        m_loadSync = false;
        OnChangeState = null;
    }   

    public void AddRealSceneNameToLoad(string realSceneName)
    {
        if (m_realSceneNamesToLoad == null)
        {
            m_realSceneNamesToLoad = new List<string>();
        }

        if (!m_realSceneNamesToLoad.Contains(realSceneName))
        {
            m_realSceneNamesToLoad.Add(realSceneName);
        }
    }

    public void AddRealSceneNameListToLoad(List<string> realSceneNames)
    {
        if (m_realSceneNamesToLoad == null)
        {
            m_realSceneNamesToLoad = new List<string>();
        }

        UbiListUtils.AddRange(m_realSceneNamesToLoad, realSceneNames, false, true);
    }


    public void AddRealSceneNameToUnload(string realSceneName)
    {
        if (m_realSceneNamesToUnload == null)
        {
            m_realSceneNamesToUnload = new List<string>();
        }

        if (!m_realSceneNamesToUnload.Contains(realSceneName))
        {
            m_realSceneNamesToUnload.Add(realSceneName);
        }
    }

    public void AddRealSceneNameListToUnload(List<string> realSceneNames)
    {
        if (m_realSceneNamesToUnload == null)
        {
            m_realSceneNamesToUnload = new List<string>();
        }

        UbiListUtils.AddRange(m_realSceneNamesToUnload, realSceneNames, false, true);
    }
    
    public void Perform(bool loadSync, OnChangeStateCallback onChangeState = null)
    {        
        m_loadSync = loadSync;
        OnChangeState = onChangeState;

        // Retreives the dependencies (typically the ones that player's dragon and pets depend on) that shouldn't be unloaded between areas
        m_loadLevelHandle = HDAddressablesManager.Instance.Ingame_SwitchArea(m_prevArea, m_nextArea, m_realSceneNamesToUnload, m_realSceneNamesToLoad, m_dependencyIdsToStayInMemory);        
        State = EState.UnloadingPrevAreaScenes;        
    }

    public void Unload()
    {
        if (m_loadLevelHandle != null)
        {
            m_loadLevelHandle.UnloadNextAreaDependencies();
        }
    }

    public void Update()
    {
        bool needsToChangeState = GetStateProgress(true) >= 1f;       

        // We know that all scenes are loaded because they've been loaded synchronously
        if (State == EState.LoadingNextAreaScenes && m_loadSync)
        {
            needsToChangeState = true;
        }

        switch (State)
        {
            case EState.UnloadingPrevAreaScenes:
                if (needsToChangeState)
                {
                    State = EState.UnloadingPrevAreaDependencies;
                }
                break;

            case EState.UnloadingPrevAreaDependencies:
                if (needsToChangeState)
                {
                    State = EState.LoadingNextAreaDependencies;
                }
                break;

            case EState.LoadingNextAreaDependencies:
                if (needsToChangeState)
                {
                    State = EState.LoadingNextAreaScenes;
                }
                break;

            case EState.LoadingNextAreaScenes:
                if (needsToChangeState)
                {
                    State = (m_loadSync) ? EState.Done : EState.WaitingToActivateNextAreaScences;
                }
                break;

            case EState.ActivatingNextAreaScenes:
                if (needsToChangeState)
                {
                    State = EState.Done;
                }
                break;
        }
    }       

    public float GetProgress()
    {
        float returnValue = 0f;
        switch (State)
        {
            case EState.Done:
                returnValue = 1f;
                break;

            case EState.LoadingNextAreaScenes:
            case EState.WaitingToActivateNextAreaScences:
            case EState.ActivatingNextAreaScenes:
                returnValue = GetStateProgress(false);
                break;
        }

        return returnValue;        
    }

    private float GetStateProgress(bool correctProgress)
    {        
        float returnValue = 1f;
        if (m_tasks != null)
        {
            returnValue = 0f;
            int count = m_tasks.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    // When allowSceneActivation is set to false then progress is stopped at 0.9. The isDone is then maintained at false. When allowSceneActivation is set to true isDone can complete.
                    if (m_tasks[i].allowSceneActivation || !correctProgress)
                    {
                        returnValue += m_tasks[i].progress;
                    }
                    else
                    {
                        returnValue += m_tasks[i].progress / 0.9f;
                    }
                }

                returnValue /= count;
            }
        }

        return returnValue;
    }

    private void SetAllowSceneActivation(List<AddressablesOp> tasks, bool value)
    {
        if (tasks != null)
        {
            int count = tasks.Count;
            for (int i = 0; i < count; i++)
            {
                if (tasks[i] != null)
                {
                    tasks[i].allowSceneActivation = value;
                }
            }
        }
    }

    public bool IsLoadingNextAreaScenes()
    {
        return State >= EState.LoadingNextAreaScenes;
    }

    public bool IsReadyToActivateNextAreaScenes()
    {
        return State == EState.WaitingToActivateNextAreaScences;
    }

    public void ActivateNextAreaScenes()
    {
        if (State == EState.WaitingToActivateNextAreaScences)
        {
            State = EState.ActivatingNextAreaScenes;
        }
    }

    public bool IsDone()
    {
        return State == EState.Done;
    }
}
