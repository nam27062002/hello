﻿public class AssetBundlesOpRequest : UbiAsyncOperation
{    
    public AssetBundlesOp Op { get; set; }

    private AssetBundlesOp.OnDoneCallback OnDone;

    private bool m_isDone;

    public bool isDone
    {
        get
        {
            return m_isDone;
        }

        private set
        {
            m_isDone = value;
        }
    }

    public virtual bool allowSceneActivation
    {
        get
        {
            return (Op == null) ? true : Op.AllowSceneActivation;
        }

        set
        {
            if (Op != null)
            {
                Op.AllowSceneActivation = value;
            }
        }
    }

    public virtual float progress
    {
        get
        {
            if (Op == null)
            {
                return (isDone) ? 1f : 0f;
            }
            else
            {
                return Op.Progress;
            }            
        }        
    }    

    public void Setup(AssetBundlesOp.OnDoneCallback onDone)
    {
        OnDone = onDone;
    }

    public void Setup(AssetBundlesOp op, AssetBundlesOp.OnDoneCallback onDone)
    {
        Setup(onDone);
        Op = op;        
    }    

    public void NotifyResult(AssetBundlesOp.EResult result, object data)
    {
        isDone = true;   
        if (OnDone != null)
        {
            OnDone(result, data);
        }
    }
}
