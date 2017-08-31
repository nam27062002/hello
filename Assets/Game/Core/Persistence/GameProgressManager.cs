/// <summary>
/// Abstract class for persistence manager. It's used to be able to sue different implementations. The actual one (PersistenceManagerImp) and another (PersistenceManagerDebug)
/// make all flow testing easier.
/// </summary>
public abstract class GameProgressManager
{
    public virtual void Init() {}

    public PersistenceData LocalProgress_Data { get; set; }
    public abstract PersistenceData LocalProgress_Load(string id);
    public abstract void LocalProgress_ResetToDefault(string id, SimpleJSON.JSONNode deafultProfile);
    public abstract PersistenceStates.SaveState LocalProgress_SaveToDisk();    

    public virtual void Systems_RegisterSystem(PersistenceSystem system) {}
    public virtual void Systems_UnregisterSystem(PersistenceSystem system) {}

    //public virtual bool Cloud_
}
