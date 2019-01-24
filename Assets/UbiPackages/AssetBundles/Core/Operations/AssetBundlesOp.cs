/// <summary>
/// Superclass for all asset bundle operations
/// </summary>
public abstract class AssetBundlesOp
{    
    public enum EResult
    {
        Success,
        Error_AB_Handle_Not_Found,      // No asset bundle handle found for an id
        Error_AB_Couldnt_Be_Loaded,     // There was a problem when loading an asset bundle from disk
        Error_Asset_Not_Found_In_AB,    // No asset with the name specified was found in an asset bundle
        Error_AB_Is_Not_A_Scene_Bundle, // This error arises when trying to load a scene from a non scene asset bundle
        Error_AB_Is_Not_Loaded          // This error arises when trying to unload an asset bundle that hasn't been loaded
    };

    public delegate void OnDoneCallback(EResult result, object data);

    private OnDoneCallback m_onDone;
    public OnDoneCallback OnDone
    {
        get { return m_onDone; }
        set { m_onDone = value; }
    }

    private bool m_isPerforming;
    public bool IsPerforming
    {        
        get { return m_isPerforming; }
        private set { m_isPerforming = value;  }
    }

    public void Reset()
    {
        m_onDone = null;
        IsPerforming = false;

        ExtendedReset();
    }

    protected virtual void ExtendedReset() {}

    public void Perform()
    {
        if (!IsPerforming)
        {
            IsPerforming = true;
            ExtendedPerform();
        }
    }

    protected abstract void ExtendedPerform();

    public void Update()
    {
        if (IsPerforming)
        {
            ExtendedUpdate();
        }
    }

    protected virtual void ExtendedUpdate() {}

    protected void NotifySuccess(object data)
    {
        NotifyOnDone(EResult.Success, data);
    }

    protected void NotifyError(EResult result)
    {
        NotifyOnDone(result, null);
    }

    protected void NotifyOnDone(EResult result, object data)
    {
        IsPerforming = false;

        if (OnDone != null)
        {
            OnDone(result, data);            
        }
    }
}
