
public abstract class HDLiveDataController {
    //---[Attributes]-----------------------------------------------------------
    protected string m_type;
    public string type { get { return m_type; } }

    protected bool m_dataLoadedFromCache = false;
    public bool isDataFromCache { get { return m_dataLoadedFromCache; } }

    protected bool m_active = false;
    public bool isActive { get { return m_active; } }

    protected bool m_isFinishPending = false;


    //---[Methods]--------------------------------------------------------------
    public void DeleteCache() {
        CleanData();
        if (CacheServerManager.SharedInstance.HasKey(m_type)) {
            CacheServerManager.SharedInstance.DeleteKey(m_type);
        }
        m_dataLoadedFromCache = false;
    }

    public abstract void Activate();
    public abstract void Deactivate();
    public abstract void ActivateLaterMods();
    public abstract void DeactivateLaterMods();
    public abstract void ApplyDragonMods();

    public abstract void CleanData();
    public abstract bool ShouldSaveData();
    public abstract SimpleJSON.JSONNode SaveData();

    public abstract bool IsFinishPending();
    public abstract void LoadDataFromCache();
    public abstract void LoadData(SimpleJSON.JSONNode _data);

    public abstract void OnLiveDataResponse();
}
