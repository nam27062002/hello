
public abstract class HDLiveDataController {
    private string m_type;
    public string type { get { return m_type; } }

    public abstract void CleanData();

    public abstract bool ShouldSaveData();
    public abstract SimpleJSON.JSONNode SaveData();
    public abstract void LoadData(SimpleJSON.JSONNode _data, bool _fromCache);
}
