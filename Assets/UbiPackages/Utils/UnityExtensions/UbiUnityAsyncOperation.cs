using UnityEngine;

public class UbiUnityAsyncOperation : UbiAsyncOperation
{
    private AsyncOperation m_asyncOperation;

    public UbiUnityAsyncOperation(AsyncOperation asyncOperation)
    {
        m_asyncOperation = asyncOperation;
    }

    public bool allowSceneActivation
    {
        get
        {
            return (m_asyncOperation == null) ? false : m_asyncOperation.allowSceneActivation;
        }

        set
        {
            if (m_asyncOperation != null)
            {
                m_asyncOperation.allowSceneActivation = value;
            }
        }
    }

    public bool isDone
    {
        get
        {
            return (m_asyncOperation == null) ? true : m_asyncOperation.isDone;
        }        
    }

    public float progress
    {
        get
        {
            return (m_asyncOperation == null) ? 1f : m_asyncOperation.progress;            
        }
    }    
}
