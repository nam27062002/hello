using System;
public class PersistenceLocalDriver
{
	public bool IsLoadedInGame { get; set; }

	// Amount of updates that the local persistence is ahead of the cloud persistence
	public int UpdatesAheadOfCloud { get; set; }

	public PersistenceData Data { get; set; }

	public UserProfile UserProfile { get; set; }

	public PersistenceLocalDriver()
	{
		string dataName = PersistencePrefs.ActiveProfileName;        
		Data = new PersistenceData(dataName);
		Reset();
	}

    public void Destroy()
    {
    }

    protected virtual void Reset()
	{
		IsLoadedInGame = false;
		UpdatesAheadOfCloud = 0;

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
		Data.Load();
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
		Data.Save();
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
		int rewardAmount = PersistenceFacade.Rules_GetPCAmountToIncentivizeSocial();

		Action onRewardCollected = delegate ()
        {
            // Gives the reward    
            UserProfile.EarnCurrency(UserProfile.Currency.HARD, (ulong)rewardAmount, false, HDTrackingManager.EEconomyGroup.INCENTIVISE_SOCIAL_LOGIN);

            // Mark it as already rewarded
            UserProfile.SocialState = UserProfile.ESocialState.LoggedInAndInventivised;            
            Save(onDone);            
        };

        PersistenceFacade.Popups_OpenLoginComplete(rewardAmount, onRewardCollected);        
	}

	public void NotifyUserHasLoggedIn(Action onDone)
	{
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

	public void Update()
	{
		;
	}	
}
