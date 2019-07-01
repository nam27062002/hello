using FGOL.Save;
using System;
using System.IO;
public class PersistenceLocalDriver
{
	public bool IsLoadedInGame { get; set; }

	// Amount of updates that the local persistence is ahead of the cloud persistence
	public int UpdatesAheadOfCloud { get; set; }

    private PersistenceData mData;
	public PersistenceData Data
    {
        get { return mData; }
        set
        {
            mData = value;
                        
            string key = (mData == null) ? null : mData.Key;      
            if (string.IsNullOrEmpty(key))
            {
                key = PersistenceProfile.DEFAULT_PROFILE;
            }
            SavePaths_Generate(key);
        }
    }

	public UserProfile UserProfile { get; set; }

    public TrackingPersistenceSystem TrackingPersistenceSystem { get; set; }    

	public PersistenceLocalDriver()
	{
        string dataName = PersistencePrefs.ActiveProfileName;
        Data = new PersistenceData(dataName);
		Reset();
	}


    public void Destroy()
    {
    }

    public void Reset()
	{
		IsLoadedInGame = false;
		UpdatesAheadOfCloud = 0;

        if (Data != null)
        {
            Data.Reset();
        }

		ExtendedReset();
	}

	protected virtual void ExtendedReset() {}    

    public void Load(Action onDone)
	{
		ExtendedLoad();
		OnLoadDone(onDone);
	}

	protected virtual void ExtendedLoad()
	{
        // Migration code
        // Move Default.sav to Devault_0.sav
        string dataName = PersistencePrefs.ActiveProfileName;
        string oldPath = SaveUtilities.GetSavePath(dataName);
        string newPath = SaveUtilities.GetSavePath(dataName + "_0");
        if ( File.Exists( oldPath ) && !File.Exists(newPath) )
        {
            Data.Load( oldPath );    
            if ( Data.LoadState == PersistenceStates.ELoadState.OK )
            {
                Data.Save( newPath );
                // if save if ok then delete file
                if (SavePaths_Verify(newPath))
                {
                    File.Delete( oldPath );
                }
            }
        }


        int currentIndex = SavePaths_LatestIndex;        
        int latestIndex = currentIndex;
        string savePath;

        bool anyCorrupted = false;

        // Tries different paths until no paths are left or one of them contains a file that exists and it's not corrupted
        for (int i = 0; i < SAVE_PATHS_COUNT; i++)
        {            
            savePath = SavePaths_GetPathAtIndex(currentIndex);
            Data.Load(savePath);

            // This variable states that at least a file was found but it was corrupted so the corresponding popup can be shown if no valid file is found
            if (Data.LoadState == PersistenceStates.ELoadState.Corrupted)
            {
                anyCorrupted = true;
            }

            // Checks if it's a valid one, if so then it
            if (SavePaths_IsAValidLoadState(Data.LoadState))            
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                {
                    PersistenceFacade.Log("File at index " + currentIndex + " with path " + savePath + " has been loaded with state " + Data.LoadState);
                }

                if (currentIndex != latestIndex)
                {
                    SavePaths_LatestIndex = currentIndex;
                }

                anyCorrupted = false;

                // No more iterations are needed
                break;
            }
            else
            {                
                if (FeatureSettingsManager.IsDebugEnabled)
                    PersistenceFacade.LogWarning("File at index " + currentIndex + " with path " + savePath + " is not valid: " + Data.LoadState);

                // Updates the index for the next iteration
                currentIndex = SavePaths_GetPreviousIndexToIndex(currentIndex);
            }
        }    
        
        // We need to mark Data as Corrupted because no valid file was found and we know that at least a file exists but it was corrupted so we need to let the user know
        if (anyCorrupted)
        {
            Data.LoadState = PersistenceStates.ELoadState.Corrupted;
        }            
    }

	protected void OnLoadDone(Action onDone)
	{
		switch (Data.LoadState)
		{
			case PersistenceStates.ELoadState.PermissionError:
				// Tries to reload
				Action onConfirm = delegate() 
				{
					Load(onDone);
				};

				// Notifies that no permissions to load occurred
				PersistenceFacade.Popups_OpenLocalLoadPermissionError(onConfirm);				
			break;

			case PersistenceStates.ELoadState.NotFound:
				OverrideWithDefault(onDone);
			break;

			case PersistenceStates.ELoadState.OK:
				ProcessSocialState(onDone);
			break;

			default:
				if (onDone != null)
				{
					onDone();
				}
			break;
		}
	}

	public void Override(string persistence, Action onDone)
	{               
        Data.LoadFromString(persistence);

        // Overrides all files except the one that is going to be overridden by Save(onDone) below
        // They all need to be overridden becuase their values are not valid anymore, otherwise the user could be able to load an old persistence
        // if the latest file got corrupted
        if (SAVE_PATHS_MULTIPLE_ENABLED)
        {
            int index = SavePaths_GetNextIndexToLatestIndex();

            string savePath;

            // all files have to be overridden because the current value is not valid anymore
            for (int i = 0; i < SAVE_PATHS_COUNT; i++)
            {
                if (i != index)
                {
                    savePath = SavePaths_GetPathAtIndex(i);
                    Data.Save(savePath);
                }
            }
        }

        // Saves the new local persistence. 
		Save(onDone);
	}

	public void OverrideWithDefault(Action onDone)
	{        
		// Default persistence is loaded
		SimpleJSON.JSONClass defaultPersistence = PersistenceUtils.GetDefaultDataFromProfile();
		Override(defaultPersistence.ToString(), onDone);
	}

	public void Save(Action onDone)
	{        
        // Makes sure that the persistence that is about to be saved is a valid one
        if (Data.LoadState == PersistenceStates.ELoadState.OK)
		{
			ExtendedSave();
			OnSaveDone(onDone);
		} 
		else if (FeatureSettingsManager.IsDebugEnabled)
		{
			PersistenceFacade.LogError(" Data is not OK so it won't be saved");
			if (onDone != null)
			{
				onDone();
			}
		}
	}

	protected virtual void ExtendedSave()
	{
        // It's not saved in the latest path in order to prevent it from gettomg corrupted if something goes wrong        
        int index = SavePaths_GetNextIndexToLatestIndex();
        string savePath = SavePaths_GetPathAtIndex(index);
        Data.Save(savePath);        

        if (SAVE_PATHS_MULTIPLE_ENABLED)
        {            
            if (Data.SaveState == PersistenceStates.ESaveState.OK)
            {
                // Loads it again to make sure it's been saved correctly
                if (SavePaths_Verify(savePath))
                {
                    // Since the file has been loaded successfully we can consider it the latest valid one
                    SavePaths_LatestIndex = index;

                    if (FeatureSettingsManager.IsDebugEnabled)
                        PersistenceFacade.Log("Local persistence successfully saved to " + savePath + " with state = " + Data.SaveState);
                }
                else
                {
                    Data.SaveState = PersistenceStates.ESaveState.Corrupted;

                    if (FeatureSettingsManager.IsDebugEnabled)
                        PersistenceFacade.LogError("Error when saving local persistence to " + savePath + " LoadState = " + SavePaths_Data.LoadState);                    
                }
            }            
        }
	}

	private void OnSaveDone(Action onDone)
	{
		Action onRetry = delegate()
		{
			Save(onDone);
		};

		switch (Data.SaveState)
		{
			case PersistenceStates.ESaveState.DiskSpace:
				PersistenceFacade.Popups_OpenLocalSaveDiskOutOfSpaceError(onRetry);
			break;

			case PersistenceStates.ESaveState.PermissionError:
				PersistenceFacade.Popups_OpenLocalSavePermissionError(onRetry);
			break;

            case PersistenceStates.ESaveState.Corrupted:
                PersistenceFacade.Popups_OpenLocalSaveCorruptedError(onRetry, onDone);
                break;

			default:
				if (IsLoadedInGame)
				{
					UpdatesAheadOfCloud++;
				}

				if (onDone != null)
				{					
					onDone();
				}

			break;
		}
	}

	public void ProcessSocialState (Action onDone)
	{
		// Checks if the user hasn't collected the LoggedIn reward
		if (UserProfile.SocialState == UserProfile.ESocialState.LoggedIn)
		{
			ShowLogInReward(onDone);
		}
		else if (onDone != null)
		{
			onDone();
		}
	}

	private void ShowLogInReward(Action onDone)
	{
        if (FeatureSettingsManager.instance.IsIncentivisedLoginEnabled())
        {
            int rewardAmount = PersistenceFacade.Rules_GetPCAmountToIncentivizeSocial();

            Action onRewardCollected = delegate ()
            {
                // Gives the reward    
                UserProfile.EarnCurrency(UserProfile.Currency.HARD, (ulong)rewardAmount, false, HDTrackingManager.EEconomyGroup.INCENTIVISE_SOCIAL_LOGIN);

                // Mark it as already rewarded
                UserProfile.SocialState = UserProfile.ESocialState.LoggedInAndIncentivised;
                Save(onDone);
            };

            PersistenceFacade.Popups_OpenLoginComplete(rewardAmount, onRewardCollected);
        }
        else
        {
            // Mark it as already rewarded
            UserProfile.SocialState = UserProfile.ESocialState.LoggedInAndIncentivised;
            Save(onDone);
        }
	}

	public void NotifyUserHasLoggedIn(string socialPlatformKey, string socialId, Action onDone)
	{
        Prefs_SocialPlatformKey = socialPlatformKey;
        Prefs_SocialId = socialId;
        PersistencePrefs.Social_WasLoggedInWhenQuit = true;

        if (TrackingPersistenceSystem != null)
        {            
            TrackingPersistenceSystem.SetSocialParams(socialPlatformKey, socialId);
        }

		// Checks if it's the first time the user logs in, if so then socialState has to be updated
		if (UserProfile != null && UserProfile.SocialState == UserProfile.ESocialState.NeverLoggedIn)
		{
			UserProfile.SocialState = UserProfile.ESocialState.LoggedIn;
			Save(onDone);
		} 
		else if (onDone != null)
		{
			onDone();
		}
	}

    public void Update() {}

    #region SavePaths
    private const bool SAVE_PATHS_MULTIPLE_ENABLED = true;
    private const int SAVE_PATHS_COUNT = (SAVE_PATHS_MULTIPLE_ENABLED) ? 2 : 1;

    private string[] mSavePaths;        

    private void SavePaths_Generate(string key)
    {        
        if (mSavePaths == null)
        {
            mSavePaths = new string[SAVE_PATHS_COUNT];
        }

        string name;
        for (int i = 0; i  < SAVE_PATHS_COUNT; i++)
        {
            name = key + "_" + i;
            mSavePaths[i] = SaveUtilities.GetSavePath(name);
        }
    }

    private int SavePaths_LatestIndex
    {
        get { return (SAVE_PATHS_MULTIPLE_ENABLED) ? PersistencePrefs.SavePathsLatestIndex : 0; }
        set { PersistencePrefs.SavePathsLatestIndex = value; }
    }

    private int SavePaths_GetNextIndexToLatestIndex()
    {
        return (SavePaths_LatestIndex + 1) % SAVE_PATHS_COUNT;
    }

    private int SavePaths_GetPreviousIndexToIndex(int index)
    {
        int returnValue = index - 1;
        if (returnValue < 0)
        {
            returnValue = SAVE_PATHS_COUNT - 1;
        }

        return returnValue;
    }

    private string SavePaths_GetLatestPath
    {
        get { return SavePaths_GetPathAtIndex(SavePaths_LatestIndex); }
    }

    private string SavePaths_GetPathAtIndex(int index)
    {
        return (mSavePaths == null) ? null : mSavePaths[index]; 
    }

    private bool SavePaths_IsAValidLoadState(PersistenceStates.ELoadState state)
    {
        return state != PersistenceStates.ELoadState.Corrupted && state != PersistenceStates.ELoadState.NotFound;
    }

    private PersistenceData SavePaths_Data { get; set; }
    private bool SavePaths_Verify(string path)
    {
        if (SavePaths_Data == null)
        {
            // We need to use the same key as Data so it can be decrypted
            SavePaths_Data = new PersistenceData(Data.Key);
        }
        else
        {
            SavePaths_Data.Reset();
        }

        SavePaths_Data.Load(path);
        return SavePaths_Data.LoadState == PersistenceStates.ELoadState.OK;
    }
    #endregion

    #region prefs
    public virtual string Prefs_SocialPlatformKey
    {
        get { return PersistencePrefs.Social_PlatformKey; }
        set { PersistencePrefs.Social_PlatformKey = value; }
    }

    public virtual string Prefs_SocialId
    {
        get { return PersistencePrefs.Social_Id; }
        set { PersistencePrefs.Social_Id = value; }
    }
    #endregion
}
