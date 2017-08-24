/// <summary>
/// This class is responsible for simulating different cases for debug purposes
/// </summary>
public class PersistenceManagerDebug : GameProgressManager
{
    public class Forced
    {
        public int Times { get; set; }

        public void Reset()
        {
            Times = 0;
        }
    }

    public PersistenceManagerDebug(string name, PersistenceManagerImp manager)
    {
        Name = name;
        Manager = manager;

        Reset();
    }

    public string Name { get; set; }

    private PersistenceManagerImp Manager;

    private bool NeedsToForceLoadState { get; set; }

    private PersistenceStates.LoadState m_forcedLoadState;
    public PersistenceStates.LoadState ForcedLoadState
    {
        get { return m_forcedLoadState; }
        set
        {
            NeedsToForceLoadState = true;
            m_forcedLoadState = value;
        }
    }

    private bool NeedsToForceSaveState { get; set; }

    private PersistenceStates.SaveState m_forcedSaveState;
    public PersistenceStates.SaveState ForcedSaveState
    { 
        get { return m_forcedSaveState; }
        set
        {
            NeedsToForceSaveState = true;
            m_forcedSaveState = value;
        }
    }

    private void Reset()
    {
        NeedsToForceLoadState = false;
        NeedsToForceSaveState = false;
    }
    
    public override PersistenceStates.LoadState LocalProgress_Load()
    {
        PersistenceStates.LoadState returnValue;
        if (NeedsToForceLoadState)
        {
            returnValue = ForcedLoadState;
        }
        else
        {
            returnValue = Manager.LocalProgress_Load();
        }
        
        return returnValue;
    }

    public override PersistenceStates.SaveState LocalProgress_SaveToDisk()
    {
        PersistenceStates.SaveState returnValue;
        if (NeedsToForceSaveState)
        {
            returnValue = ForcedSaveState;
        }
        else
        {
            returnValue = Manager.LocalProgress_SaveToDisk();
        }

        return returnValue;
    }

    public override PersistenceStates.LoadState LocalProgress_ResetToDefault()
    {
        return Manager.LocalProgress_ResetToDefault();
    }
}
