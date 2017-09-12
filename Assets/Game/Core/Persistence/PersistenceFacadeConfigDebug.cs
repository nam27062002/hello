public class PersistenceFacadeConfigDebug : PersistenceFacadeConfig
{
    public enum EUserCaseId
    {
        Production,

		Launch_Local_NotFound_Cloud_NoConnection,
		Launch_Local_NotFound_Error_FullDisk_Cloud_NoConnection,
		Launch_Local_Error_LoadPermission_Cloud_NoConnection,
		Launch_Local_NeverLoggedIn_Cloud_NoConnection,
		Launch_Local_LoggedIn_Cloud_NoConnection,
		Launch_Local_LoggedInAndIncentivised_Cloud_NoConnection,
		Launch_Local_LoggedIn_Cloud_Error_NotLoggedInServer,
		Launch_Local_NeverLoggedIn_Cloud_Error_NotLoggedInSocial,
		Launch_Local_NeverLoggedIn_Cloud_Error_GetPersistence,
		Launch_Local_NeverLoggedIn_Cloud_More,
		Launch_Local_NeverLoggedIn_Error_FullDisk_Cloud_More,
		Launch_Local_NeverLoggedIn_Cloud_Less,
		Launch_Local_NeverLoggedIn_Cloud_Equal,
		Launch_Local_NeverLoggedIn_Cloud_Less_Error_Upload,
		Launch_Local_NeverLoggedIn_Cloud_Less_Error_Merge,
		Launch_Local_NeverLoggedIn_Cloud_Corrupted,
		Launch_Local_NeverLoggedIn_Cloud_Corrupted_Error_Upload,
		Launch_Local_NeverLoggedIn_Cloud_Corrupted_Error_Merge,
		Launch_Local_Corrupted_Cloud_NoConnection,
		Launch_Local_Corrupted_Cloud_Ok,
		Launch_Local_Corrupted_Cloud_Corrupted,

		Settings_Local_NeverLoggedIn_Cloud_NoConnection,
		Settings_Local_LoggedIn_Cloud_NoConnection,
		Settings_Local_LoggedInAndIncentivised_Cloud_NoConnection,
		Settings_Local_LoggedIn_Cloud_Error_NotLoggedInServer,
		Settings_Local_LoggedIn_Cloud_Error_NotLoggedInSocial,
		Settings_Local_NeverLoggedIn_Cloud_Error_GetPersistence,
		Settings_Local_NeverLoggedIn_Cloud_More,
		Settings_Local_NeverLoggedIn_Error_FullDisk_Cloud_More,
		Settings_Local_NeverLoggedIn_Cloud_Less,
		Settings_Local_NeverLoggedIn_Cloud_Equal,
		Settings_Local_NeverLoggedIn_Cloud_Less_Error_Upload,
		Settings_Local_NeverLoggedIn_Cloud_Less_Error_Merge,
		Settings_Local_NeverLoggedIn_Cloud_Corrupted,
		Settings_Local_NeverLoggedIn_Cloud_Corrupted_Error_Upload,
		Settings_Local_NeverLoggedIn_Cloud_Corrupted_Error_Merge,
    };

    private EUserCaseId UserCaseId { get; set;  }   

    public PersistenceFacadeConfigDebug(EUserCaseId id)
    {
        UserCaseId = id;
		Setup();
    }   

    protected override void Setup()
	{        
		if (UserCaseId == EUserCaseId.Production)
		{
			base.Setup ();
		} 
		else
		{
			LocalDriver = new PersistenceLocalDriverDebug();
			CloudDriver = new PersistenceCloudDriverDebug();
			CloudDriver.Setup(LocalDriver);

			SetupUserCaseId(UserCaseId);
		}
    }

	private void SetupUserCaseId(EUserCaseId caseId)
	{
		switch (caseId)
		{
			case EUserCaseId.Launch_Local_NotFound_Cloud_NoConnection:
				// Default profile is loaded
				LocalDriverDebug.PersistenceAsString = null;
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Launch_Local_NotFound_Error_FullDisk_Cloud_NoConnection:
				// A popup showing full disk error is shown until it's fixed. Once fixed default persistence is loaded
				SetupUserCaseId(EUserCaseId.Launch_Local_NotFound_Cloud_NoConnection);
				LocalDriverDebug.isFullDiskErrorEnabled = true;
			break;

			case EUserCaseId.Launch_Local_Error_LoadPermission_Cloud_NoConnection:
				// A popup showing permission error is shown until it's fixed. Once fixed the persistence defined below is loaded
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.LoggedIn, 10);
				LocalDriverDebug.IsPermissionErrorEnabled = true;
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_NoConnection:
				// Game loads with the persistence defined below 
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, 10);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Launch_Local_LoggedIn_Cloud_NoConnection:
				// A popup giving the login reward is shown to the user
				// When accepted game loads with the persistence defined below althugh socialState has changed to LoggedIn
				// and the reward was added to the profile
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.LoggedIn, 10);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Launch_Local_LoggedInAndIncentivised_Cloud_NoConnection:				
				// Game loads with the persistence defined below 
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.LoggedInAndInventivised, 10);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Launch_Local_LoggedIn_Cloud_Error_NotLoggedInServer:								
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.LoggedIn, 10);
				CloudDriverDebug.IsLogInServerEnabled = false;
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Error_NotLoggedInSocial:				
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, 10);
				CloudDriverDebug.IsLogInSocialEnabled = false;
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Error_GetPersistence:				
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, 10);
				CloudDriverDebug.IsGetPersistenceEnabled = false;
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_More:								
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, 10);
				CloudDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.LoggedIn, 100);
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Error_FullDisk_Cloud_More:
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_More);
				LocalDriverDebug.isFullDiskErrorEnabled = true;
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less:
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, 100);
				CloudDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.LoggedIn, 10);
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Equal:
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, 100);
				CloudDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, 10);				
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less_Error_Upload:
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less);
				CloudDriverDebug.IsUploadPersistenceEnabled = false;
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less_Error_Merge:
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less);
				CloudDriverDebug.IsMergeEnabled = true;
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Corrupted:
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, 100);
				CloudDriverDebug.PersistenceAsString = GetPersistenceCorrupted();
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Corrupted_Error_Upload:
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, 100);
				CloudDriverDebug.PersistenceAsString = GetPersistenceCorrupted();
				CloudDriverDebug.IsUploadPersistenceEnabled = false;
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Corrupted_Error_Merge:
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, 100);
				CloudDriverDebug.PersistenceAsString = GetPersistenceCorrupted();
				CloudDriverDebug.IsMergeEnabled = true;
			break;

			case EUserCaseId.Launch_Local_Corrupted_Cloud_NoConnection:
				// A popup letting the user know that local persistence is corrupted is shown.
				// If the user chooses to reset local persistence then the game is loaded with the default persistence.
				// If the user chooses to connect to cloud then a popup notifying that there's no connection is shown.
				// When this popup is closed the popup that lets the user choose between connecting to cloud and resetting
				// is shown again
				LocalDriverDebug.PersistenceAsString = GetPersistenceCorrupted();
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Launch_Local_Corrupted_Cloud_Ok:
				// A popup letting the user know that local persistence is corrupted is shown.
				// If the user chooses to reset local persistence then the game is loaded with the default persistence.
				// If the user chooses to connect to cloud then the game loads with the cloud persistence				
				LocalDriverDebug.PersistenceAsString = GetPersistenceCorrupted();
				CloudDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.LoggedInAndInventivised, 100);				
			break;

			case EUserCaseId.Launch_Local_Corrupted_Cloud_Corrupted:
				// A popup letting the user know that local persistence is corrupted is shown.
				// If the user chooses to reset local persistence then the game is loaded with the default persistence.
				// If the user chooses to connect to cloud then another popup notifying that cloud persistence is corrupted too is shown
				// When reset button is clicked game loads with both local and cloud persistences resetted to default
				LocalDriverDebug.PersistenceAsString = GetPersistenceCorrupted();
				CloudDriverDebug.PersistenceAsString = GetPersistenceCorrupted();
			break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_NoConnection:
				// A popup showing no connection message is shown
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_NoConnection);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Settings_Local_LoggedIn_Cloud_NoConnection:
				// A popup showing no connection message is shown
				SetupUserCaseId(EUserCaseId.Launch_Local_LoggedIn_Cloud_NoConnection);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Settings_Local_LoggedInAndIncentivised_Cloud_NoConnection:
				// A popup showing no connection message is shown
				SetupUserCaseId(EUserCaseId.Launch_Local_LoggedInAndIncentivised_Cloud_NoConnection);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Settings_Local_LoggedIn_Cloud_Error_NotLoggedInServer:
				// A popup showing no connection message is shown
				SetupUserCaseId(EUserCaseId.Launch_Local_LoggedIn_Cloud_Error_NotLoggedInServer);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Settings_Local_LoggedIn_Cloud_Error_NotLoggedInSocial:
				// No error popup is shown
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Error_NotLoggedInSocial);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Error_GetPersistence:
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Error_GetPersistence);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_More:
				// When login button is clicked a popup prompting the user to choose between
				// local and cloud persistences is shown. After this popup another popup notifying
				// the login rewad has been granted is shown. 
				// If the user chooses local then cloud persistence is overriden with local persistence. 
				// If cloud is chosen then the game gets reloaded and cloud persistence is shown
				// If the user decides to dismiss the question then local and cloud persistences will keep
				// out of sync and the user will continue with local persistence
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_More);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Error_FullDisk_Cloud_More:
				// Full disk message is shown until it's fixed.
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Error_FullDisk_Cloud_More);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Less:
				// When login button is clicked a popup showing the login reward is shown and
				// cloud and local are in sync with local persistence
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Equal:
				// When login button is clicked a popup showing the login reward is shown and
				// cloud and local are in sync with local persistence
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Equal);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Less_Error_Upload:
				// When login button is clicked a popup showing an error because an error
				// when saving the local progress in the cloud occured is shown. If the error is fixed
				// or the user decides to dismiss the error then a popup showing the login reward is shown. 
				// Persistences are in sync only if the error was fixed, otherwise they will get
				// synced automatically when the error is fixed.
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less_Error_Upload);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Less_Error_Merge:
				// When login button is clicked a popup is shown to let the user choose
				// between local persistence and cloud persistence because of a merge conflict. 
				// If local persistence is chosen then the user won't be logged in social anymore 
				// so no login reward will be granted.
				// If cloud persistence is chosen then the game gets reloaded with the cloud persistence
				// Login reward is granted before reloading.
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less_Error_Merge);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Corrupted:
				// When login button is clicked a popup notifies the user that cloud persistence 
				// is corrupted. If the user choose to ignore the problem then cloud won't be
				// in sync and uploading local persistence will be disabled. 
				// If the user decides to override cloud persistence with local persistence then
				// we do so and if everything goes ok local and cloud will be in sync.
				// After the popup the login reward is given to the user.				
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Corrupted);
				CloudDriverDebug.IsConnectionEnabled = false;				
			break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Corrupted_Error_Upload:
				// When login button is clicked a popup notifies the user that cloud persistence 
				// is corrupted. When the user decides to override cloud persistence with local persistence a
				// popup notifying an error when writing in cloud is shown until the problem is fixed
				// or the user decides to ignore the problem				
				// After the popup the login reward is given to the user.				
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Corrupted_Error_Upload);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Corrupted_Error_Merge:
				// When login button is clicked a popup notifies the user that cloud persistence 
				// is corrupted. Local persistence can't override cloud persistence because there's a social
				// merge to solve. This is notified to the user with a popup that let her continue playing
				// locally, which means that the social login hasn't been completed so login reward is not given
				// to the user.				
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Corrupted_Error_Merge);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;
		}
	}   

	private string GetPersistenceCorrupted()
	{
		return "{\"userProfile\":{\"sc\":\"10000:0\",\"pc\":\"0:0\",\"timestamp\":\"09 / 06 / 2017 07:31:01\"},\"User\":{\"NumGameLoops\":0,\"deviceName\":\"BCDTDAVIDGERM\",\"modifiedTime\":1504683061}";
	}

	private string GetPersistence(UserProfile.ESocialState socialState, int gameLoops)
	{
		return "{\"userProfile\":{\"sc\":\"10000:0\",\"socialState\":\"" + socialState.ToString() 
			+ "\"},\"User\":{\"NumGameLoops\":" + gameLoops + "}}";
	}
    
	private PersistenceLocalDriverDebug LocalDriverDebug 
	{
		get 
		{
			return LocalDriver as PersistenceLocalDriverDebug;
		}
	}

	private PersistenceCloudDriverDebug CloudDriverDebug 
	{
		get 
		{
			return CloudDriver as PersistenceCloudDriverDebug;
		}
	}
}
