/// <summary>
/// This class is responsible for managing persistence load/save. It stores the user's local game progress which is stored on device
/// </summary>
using FGOL.Save;
using FGOL.Server;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LocalPersistenceUtils
{
    // Path where the data files are stored
    public static string saveDir
    {
        // Hidden file directory of Unity, see https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
        get { return Application.persistentDataPath; }
    }

    /// <summary>
    /// Get the default data stored in the given profile prefab.
    /// </summary>
    /// <returns>The data from profile.</returns>
    /// <param name="_profileName">The name of the profile to be loaded.</param>
    public static SimpleJSON.JSONClass GetDefaultDataFromProfile(string _profileName = "")
    {
        SimpleJSON.JSONClass _returnValue = null;

        bool _useDefaultProfile = string.IsNullOrEmpty(_profileName) || _profileName == PersistenceProfile.DEFAULT_PROFILE;
        if (_useDefaultProfile)
        {
            // The default profile is created from rules
            DefinitionNode _def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "initialSettings");
            if (_def != null)
            {
                string _sc = _def.Get("softCurrency");
                string _pc = _def.Get("hardCurrency");
                string _initialDragonSku = _def.Get("initialDragonSKU");

                _returnValue = new SimpleJSON.JSONClass();

                // User Profile: sc, pc, currentDragon
                SimpleJSON.JSONClass _userProfile = new SimpleJSON.JSONClass();
                _userProfile.Add("sc", _sc);
                _userProfile.Add("pc", _pc);
                _userProfile.Add("keys", 3);	// [AOC] HARDCODED!!
                _userProfile.Add("currentDragon", _initialDragonSku);
                _userProfile.Add("currentLevel", "level_0");	// Only one level now
                _returnValue.Add("userProfile", _userProfile);

                // Dragons array
                SimpleJSON.JSONArray _dragons = new SimpleJSON.JSONArray();

                // Initial dragon
                SimpleJSON.JSONClass _dragon = new SimpleJSON.JSONClass();
                _dragon.Add("sku", _initialDragonSku);
                _dragon.Add("owned", "true");
                _dragons.Add(_dragon);

                _returnValue.Add("dragons", _dragons);
            }
        }
        else
        {
            // Other profiles are loaded from resources
            TextAsset defaultProfile = Resources.Load<TextAsset>(PersistenceProfile.RESOURCES_FOLDER + _profileName);
            if (defaultProfile != null)
            {
                _returnValue = SimpleJSON.JSON.Parse(defaultProfile.text) as SimpleJSON.JSONClass;
            }
        }

        return _returnValue;
    }

    /// <summary>
	/// Given the name of a profile, obtain the full path of its associated persistence file.
	/// </summary>
	/// <param name="_profileName">The name of the profile whose path we want.</param>
	private static string GetPersistenceFilePath(string _profileName = "")
    {
        return saveDir + "/" + _profileName + ".sav";
    }

    /// <summary>
	/// Obtain the list of current saved games.
	/// </summary>
	/// <returns>The list of saved games in the persistence dir.</returns>
	public static string[] GetSavedGamesList()
    {
        // C# makes it easy for us
        DirectoryInfo dirInfo = new DirectoryInfo(saveDir);
        FileInfo[] files = dirInfo.GetFiles();

        // Strip filename from full file path
        List<string> fileNames = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Name.EndsWith("sav"))
            {
                fileNames.Add(Path.GetFileNameWithoutExtension(files[i].Name));
            }
        }

        return fileNames.ToArray();
    }

    /// <summary>
    /// Load the game persistence for a specific profile into a new JSON object.
    /// </summary>
    /// <returns>The to JSON where the data is loaded, <c>null</c> if an error ocurred.</returns>
    /// <param name="_profileName">The name of the profile to be loaded.</param>	
    public static SimpleJSON.JSONClass LoadToObject(string _profileName = "")
    {
        // From https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
        SimpleJSON.JSONClass data = null;
        string path = GetPersistenceFilePath(_profileName);
        if (File.Exists(path))
        {
            //[DGR] Persistence with FGOL technology           
            SaveData _saveData = new SaveData(_profileName);
            _saveData.Load();
            string profileJSONStr = _saveData.ToString();
            data = SimpleJSON.JSON.Parse(profileJSONStr) as SimpleJSON.JSONClass;
        }
        else
        {
            // No saved games found for the given profile, try to load the profile data            
            Debug.Log("No saved games were found, loading profile " + _profileName);

            // Load profile and use it as initial data
            data = GetDefaultDataFromProfile(_profileName);
            if (data == null)
            {
                Debug.Log("Profile " + _profileName + " couldn't be found, starting from 0");
            }
        }

        return data;
    }

    /// <summary>
    /// Deletes local persistence file.
    /// The game should be reloaded afterwards.
    /// </summary>
    /// <param name="_profileName">The name of the profile to be cleared.</param>
    public static void Clear(string _profileName = null)
    {
        // Delete persistence file
        string path = GetPersistenceFilePath(_profileName);
        File.Delete(path);

        // Create a new save file with the default data from the profile
        SimpleJSON.JSONClass data = GetDefaultDataFromProfile(_profileName);
        if (data != null)
        {
            SaveFromObject(_profileName, data);
        }
    }

    /// <summary>
	/// Determines if a given profile name has a persistence file attached to it.
	/// </summary>
	/// <returns><c>true</c> if a saved game exists for the specified _profileName; otherwise, <c>false</c>.</returns>
	/// <param name="_profileName">The name of the profile we want to check.</param>
	public static bool HasSavedGame(string _profileName)
    {
        return File.Exists(GetPersistenceFilePath(_profileName));
    }

    /// <summary>
    /// Save a given data object to persistence file. This method should be called only by the editor.
    /// </summary>
    /// <param name="_profileName">The name of the profile to be saved.</param>
    /// <param name="_data">The data object to be saved.</param>
    public static void SaveFromObject(string _profileName, SimpleJSON.JSONClass _data)
    {
        // The file needs to be encrypted and compressed in order to make it work as it will work on production
        PersistenceData _saveData = new PersistenceData(_profileName);
        _saveData.Merge(_data.ToString());
        _saveData.Save();
    }    
}

public class PersistenceManagerImp : GameProgressManager
{
    //m_version needs to reflect the minimum version on the server and the minimum support version of the app
    // [DGR] Version number set here so it will be accessible immediately, which is needed when the persistence profiles editor is used with the game off
    //private string m_version = "0.1.1";

    public override void Init()
    {
        LocalProgress_Init();
        Systems_Init();         
    }    
    
    #region local_progress    

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

    public override PersistenceData LocalProgress_Load(string id)
    {
        LocalProgress_IsSaveEnabled = false;        
        LocalProgress_Data = new PersistenceData(id);        
        LocalProgress_Data.Load();

        Action loadSystems = delegate ()
        {            
            if (LocalProgress_Data.LoadState == PersistenceStates.LoadState.OK)
            {
                Systems_Load(LocalProgress_Data);                                
            }

            // Save is disabled if the local progress load state is not right
            LocalProgress_IsSaveEnabled = LocalProgress_Data.LoadState == PersistenceStates.LoadState.OK;
        };

        //Check for valid results and enable saving if we are in a valid state!
        if (LocalProgress_Data.LoadState == PersistenceStates.LoadState.OK)
        {
            Systems_Load(LocalProgress_Data);                        
        }

        // Save is disabled if the local progress load state is not right
        LocalProgress_IsSaveEnabled = LocalProgress_Data.LoadState == PersistenceStates.LoadState.OK;

        return LocalProgress_Data;
    }

    public override void LocalProgress_ResetToDefault(string id, SimpleJSON.JSONNode defaultProfile)
    {        
        LocalProgress_Data = new PersistenceData(id);                
        LocalProgress_Data.Merge(defaultProfile.ToString());

        // Default persistence is always OK
        LocalProgress_Data.LoadState = PersistenceStates.LoadState.OK;

        // Loads the systems         
        Systems_Load(LocalProgress_Data);        
        
        // Saves the default persistence if everything went ok
        LocalProgress_IsSaveEnabled = LocalProgress_Data.LoadState == PersistenceStates.LoadState.OK;
        if (LocalProgress_IsSaveEnabled)
        {
            LocalProgress_SaveToDisk();
        }        
    }

    public override PersistenceStates.SaveState LocalProgress_SaveToDisk()            
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

    public override void Systems_RegisterSystem(PersistenceSystem system)
    {
        Systems_Catalog.Add(system.name, system);
    }

    public override void Systems_UnregisterSystem(PersistenceSystem system)
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
