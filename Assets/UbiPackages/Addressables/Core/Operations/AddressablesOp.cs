public abstract class AddressablesOp
{
    public delegate void OnDoneCallback(AddressablesOp op);

    public abstract bool IsDone { get; }

    public abstract float Progress { get; }

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
        m_asset = null;
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
            if (IsDone && m_onDone != null)
            {
                m_onDone(this);
            }
        }
    }
}
