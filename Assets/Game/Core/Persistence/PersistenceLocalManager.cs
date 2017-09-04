/// <summary>
/// This class is responsible for managing persistence load/save. It stores the user's local game progress which is stored on device
/// </summary>
using FGOL.Server;
using System;
using System.Collections.Generic;
public class PersistenceLocalManager
{
    //m_version needs to reflect the minimum version on the server and the minimum support version of the app
    // [DGR] Version number set here so it will be accessible immediately, which is needed when the persistence profiles editor is used with the game off
    //private string m_version = "0.1.1";

    public void Init()
    {
        LocalProgress_Init();
        Systems_Init();         
    }

    #region local_progress    

    public PersistenceData LocalProgress_Data { get; set; }

    /// <summary>
    /// Whether or not a game progress can be saved locally. It's used to keep I/O operations in sync, so saving locally will be disabled while a loading/saving operation
    /// is already being performed.
    /// </summary>
    private bool LocalProgress_IsSaveEnabled { get; set; }    

    private void LocalProgress_Init()
    {
        LocalProgress_Data = null;
        LocalProgress_IsSaveEnabled = false;
    }

    public virtual PersistenceData LocalProgress_Load(string id)
    {
        LocalProgress_IsSaveEnabled = false;        
        LocalProgress_Data = new PersistenceData(id);        
        LocalProgress_Data.Load();
        
        //Check for valid results and enable saving if we are in a valid state!
        if (LocalProgress_Data.LoadState == PersistenceStates.LoadState.OK)
        {
            Systems_Load(LocalProgress_Data);                        
        }

        // Save is disabled if the local progress load state is not right
        LocalProgress_IsSaveEnabled = LocalProgress_Data.LoadState == PersistenceStates.LoadState.OK;

        return LocalProgress_Data;
    }

    public void LocalProgress_ResetToDefault(string id, SimpleJSON.JSONNode defaultProfile)
    {        
        PersistenceData defaultData = new PersistenceData(id);
        defaultData.Merge(defaultProfile.ToString());
        
        LocalProgress_Override(defaultData);                
    }

    public void LocalProgress_Override(PersistenceData data)
    {
        LocalProgress_Data = data;

        // Loads the systems         
        Systems_Load(LocalProgress_Data);

        // Saves the default persistence if everything went ok
        LocalProgress_IsSaveEnabled = LocalProgress_Data.LoadState == PersistenceStates.LoadState.OK;
        if (LocalProgress_IsSaveEnabled)
        {
            LocalProgress_SaveToDisk();
        }
    }

    public virtual PersistenceStates.SaveState LocalProgress_SaveToDisk()            
    {
        PersistenceStates.SaveState state = PersistenceStates.SaveState.Disabled;        
        if (LocalProgress_Data != null && LocalProgress_IsSaveEnabled)
        {
            Systems_Save();
            state = LocalProgress_Data.Save();
        }

        return state;
    }
    #endregion

    #region systems
    private Dictionary<string, PersistenceSystem> Systems_Catalog { get; set; }

    private void Systems_Init()
    {
        Systems_Catalog = new Dictionary<string, PersistenceSystem>();
    }

    private void Systems_Clear()
    {
        foreach (KeyValuePair<string, PersistenceSystem> pair in Systems_Catalog)
        {
            pair.Value.Reset();
        }
    }

    private void Systems_Load(PersistenceData data)
    {
        if (data.LoadState == PersistenceStates.LoadState.OK)
        {
            try
            {
                Systems_Clear();

                foreach (KeyValuePair<string, PersistenceSystem> pair in Systems_Catalog)
                {
                    pair.Value.data = data;
                    pair.Value.Load();
                }
            }
            catch (CorruptedSaveException)
            {
                data.LoadState = PersistenceStates.LoadState.Corrupted;
            }
        }        
    }    

    private PersistenceStates.LoadState Systems_Upgrade(PersistenceData data, out bool upgraded)
    {
        PersistenceStates.LoadState state = PersistenceStates.LoadState.OK;

        upgraded = false;

        try
        {
            Systems_Clear();

            foreach (KeyValuePair<string, PersistenceSystem> pair in Systems_Catalog)
            {
                pair.Value.data = data;
                if (pair.Value.Upgrade())
                {
                    upgraded = true;
                }
            }
        }
        catch (CorruptedSaveException)
        {
            state = PersistenceStates.LoadState.Corrupted;
        }

        return state;
    }

    public void Systems_RegisterSystem(PersistenceSystem system)
    {
        Systems_Catalog.Add(system.name, system);
    }

    public void Systems_UnregisterSystem(PersistenceSystem system)
    {
        if (system != null && Systems_Catalog.ContainsKey(system.name))
        {
            Systems_Catalog.Remove(system.name);
        }        
    }

    private void Systems_Save()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("(Systems_Save) :: Saving all save systems!");
        }

        foreach (KeyValuePair<string, PersistenceSystem> pair in Systems_Catalog)
        {
            pair.Value.data = LocalProgress_Data;
            pair.Value.Save();            
        }
    }
    #endregion    

    #region log
    private const string LOG_CHANNEL = "[PersistenceManagerImp]:";

    private void Log(string msg)
    {
        Debug.Log(LOG_CHANNEL + msg);
    }
    #endregion 
}
