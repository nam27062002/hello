public class AssetBundlesOpRequest : UbiAsyncOperation
{    
    public AssetBundlesOp Op { get; set; }

    private AssetBundlesOp.OnDoneCallback OnDone;

    private bool m_isDone;
    
    public AssetBundlesOp.EResult Result { get; set; }
    public object Data { get; set; }
    public T GetData<T>() 
    {
        return (Data == null) ? default(T): (T)Data;
    }

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
        Result = AssetBundlesOp.EResult.None;
        Data = null;
        OnDone = onDone;
    }

    public void Setup(AssetBundlesOp op, AssetBundlesOp.OnDoneCallback onDone)
    {
        Setup(onDone);
        Op = op;        
    }    

    public void NotifyResult(AssetBundlesOp.EResult result, object data)
    {
        Result = result;
        Data = data;

        isDone = true;   
        if (OnDone != null)
        {
            OnDone(result, data);
        }
    }

    public virtual void Cancel()
    {
        if (Op != null)
        {
            Op.Cancel();
        }
    }
}
