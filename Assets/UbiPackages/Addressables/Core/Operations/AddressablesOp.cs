public abstract class AddressablesOp : UbiAsyncOperation
{
    public delegate void OnDoneCallback(AddressablesOp op);

    public abstract bool isDone { get; }

    private bool m_allowSceneActivation = true;
    public bool allowSceneActivation
    {
        get { return m_allowSceneActivation; }

        set
        {
            m_allowSceneActivation = value;
            UpdateAllowSceneActivation(m_allowSceneActivation);
        }
    }

    protected virtual void UpdateAllowSceneActivation(bool value) { }

    public float progress
    {
        get
        {
            float returnValue = ExtendedProgress;
            if (isDone)
            {
                returnValue = 1f;
            }
            else if (returnValue >= 1f)
            {
                returnValue = 0.99f;
            }

            if (!allowSceneActivation && returnValue > 0.9f)
            {
                returnValue = 0.9f;
            }

            return returnValue;
        }
    }

    protected abstract float ExtendedProgress { get; }     

    public virtual AddressablesError Error
    {
        get;
        private set;
    }

    private object m_asset;

    public virtual T GetAsset<T>()
    {
        return (m_asset == null) ? default(T) : (T)m_asset;
    }

    public void Setup(AddressablesError error, object asset)
    {
        Error = error;
        m_asset = asset;
    }    

    private OnDoneCallback m_onDone;

    public OnDoneCallback OnDone
    {
        get
        {
            return m_onDone;
        }

        set
        {
            m_onDone = value;
            if (isDone && m_onDone != null)
            {
                m_onDone(this);
            }
        }
    }
}
