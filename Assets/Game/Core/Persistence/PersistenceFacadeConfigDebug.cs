public class PersistenceFacadeConfigDebug : PersistenceFacadeConfig
{
    private static string LOCAL_DRAGON_SKU = "dragon_crocodile";
    private static string CLOUD_DRAGON_SKU = "dragon_classic";

    public enum EUserCaseId
    {
        Production,

		Launch_Local_NotFound_Cloud_NoConnection,                                    // tested
        Launch_Local_NotFound_Error_FullDisk_Cloud_NoConnection,                     // tested
		Launch_Local_Error_LoadPermission_Cloud_NoConnection,                        // tested
		Launch_Local_NeverLoggedIn_Cloud_NoConnection,                               // tested
        Launch_Local_LoggedIn_Cloud_NoConnection,                                    // tested
        Launch_Local_LoggedInAndIncentivised_Cloud_NoConnection,                     // tested
        Launch_Local_LoggedIn_Cloud_Error_NotLoggedInServer,                         // tested
        Launch_Local_NeverLoggedIn_Cloud_Error_NotLoggedInSocial,                    // tested
        Launch_Local_NeverLoggedIn_Cloud_Error_GetPersistence,                       // tested
        Launch_Local_NeverLoggedIn_Cloud_More,                                       // tested
        Launch_Local_NeverLoggedIn_Error_FullDisk_Cloud_More,                        // tested
        Launch_Local_NeverLoggedIn_Cloud_Less,                                       // tested
        Launch_Local_NeverLoggedIn_Cloud_Equal,                                      // tested
        Launch_Local_NeverLoggedIn_Cloud_Less_Error_Upload,                          // tested        
        Launch_Local_NeverLoggedIn_Cloud_Less_Social_Account_With_Progress,          // tested
		Launch_Local_NeverLoggedIn_Cloud_Corrupted,                                  // tested: popups hardcoded
		Launch_Local_NeverLoggedIn_Cloud_Corrupted_Error_Upload,                     // tested
		Launch_Local_NeverLoggedIn_Cloud_Corrupted_Error_Merge,                      // tested
		Launch_Local_Corrupted_Cloud_NoConnection,                                   // tested
		Launch_Local_Corrupted_Cloud_Ok,                                             // tested
		Launch_Local_Corrupted_Cloud_Corrupted,                                      // tested     

		Settings_Local_NeverLoggedIn_Cloud_NoConnection,                             // tested
		Settings_Local_LoggedIn_Cloud_NoConnection,                                  // tested
		Settings_Local_LoggedInAndIncentivised_Cloud_NoConnection,                   // tested
		Settings_Local_LoggedIn_Cloud_Error_NotLoggedInServer,                       // tested
		Settings_Local_LoggedIn_Cloud_Error_NotLoggedInSocial,                       // tested
		Settings_Local_NeverLoggedIn_Cloud_Error_GetPersistence,                     // tested
		Settings_Local_NeverLoggedIn_Cloud_More,                                     // tested
		Settings_Local_NeverLoggedIn_Error_FullDisk_Cloud_More,                      // tested
		Settings_Local_NeverLoggedIn_Cloud_Less,                                     // tested
		Settings_Local_NeverLoggedIn_Cloud_Equal,                                    // tested
		Settings_Local_NeverLoggedIn_Cloud_Less_Error_Upload,                        // tested
        Settings_Local_NeverLoggedIn_Cloud_Less_Social_Account_With_Progress,        // tested
		Settings_Local_NeverLoggedIn_Cloud_Corrupted,                                // tested
		Settings_Local_NeverLoggedIn_Cloud_Corrupted_Error_Upload,                   // tested
		Settings_Local_NeverLoggedIn_Cloud_Corrupted_Error_Merge,                    // tested
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
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.LoggedIn);
				LocalDriverDebug.IsPermissionErrorEnabled = true;
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_NoConnection:
				// Game loads with the persistence defined below 
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, LOCAL_DRAGON_SKU);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Launch_Local_LoggedIn_Cloud_NoConnection:
				// A popup giving the login reward is shown to the user
				// When accepted game loads with the persistence defined below althugh socialState has changed to LoggedIn
				// and the reward was added to the profile
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.LoggedIn, LOCAL_DRAGON_SKU);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Launch_Local_LoggedInAndIncentivised_Cloud_NoConnection:				
				// Game loads with the persistence defined below 
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.LoggedInAndIncentivised, LOCAL_DRAGON_SKU);
				CloudDriverDebug.IsConnectionEnabled = false;
			break;

			case EUserCaseId.Launch_Local_LoggedIn_Cloud_Error_NotLoggedInServer:
                // A popup showing the reward because of loging in is open.
                // Game loads with the persistence defined below + the reward (15 pc)
                LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.LoggedIn, LOCAL_DRAGON_SKU);
				CloudDriverDebug.IsLogInServerEnabled = false;
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Error_NotLoggedInSocial:
                // Game loads with the persistence defined below 
                LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, LOCAL_DRAGON_SKU);
				CloudDriverDebug.IsLogInSocialEnabled = false;
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Error_GetPersistence:
                // A popup showing the reward because of loging in is open.
                // Game loads with the persistence defined below + the reward (15 pc). The user is logged in to the social network.		
                LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, LOCAL_DRAGON_SKU);
				CloudDriverDebug.IsGetPersistenceEnabled = false;
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_More:
                // The merge popup is shown to let the user choose between local and cloud persistence. The game continues to load in background
                // The incentivised social reward popup is shown regardless the persistence chosen by the user.
                // If the user chooses local persistence or just closes the popup then the game continues with the local progress.
                // If the user chooses cloud persistence then the game is reloaded with that progress
				LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, LOCAL_DRAGON_SKU, 10);
				CloudDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.LoggedIn, CLOUD_DRAGON_SKU, 100);
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Error_FullDisk_Cloud_More:
                // The merge popup is shown to let the user choose between local and cloud persistence. The game continues to load in background
                // The incentivised social reward popup is shown regardless the persistence chosen by the user.
                // If the user chooses local persistence or just closes the popup then the game continues with the local progress.
                // If the user chooses cloud persistence then a popup notifying about an error when trying to save is shown repeatedly until the problem is solved and then the game is reloaded with that progress
                SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_More);
				LocalDriverDebug.isFullDiskErrorEnabled = true;
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less:
                // Game loads with the local persistence.
                // The incentivised social reward popup is shown.
                LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, LOCAL_DRAGON_SKU, 100);
				CloudDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.LoggedIn, CLOUD_DRAGON_SKU, 10);
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Equal:
                // Game loads with the local persistence.
                // The incentivised social reward popup is shown.
                LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, LOCAL_DRAGON_SKU, 100);
				CloudDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, CLOUD_DRAGON_SKU, 100);				
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less_Error_Upload:
                // A popup notifying about an error when accessing cloud save is shown. The game continues to load in background. Two buttons:
                // Continue: to keep playing with local persistence. The incentivised social reward popup is shown.
                // Retry: to try to sync with cloud save again. Keeps showing this popup until the error doesn't happen or the user chooses 'Continue'. If the error gets fixed then the game continues with
                // local persistence because it's more than cloud persistence
                SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less);
				CloudDriverDebug.IsUploadPersistenceEnabled = false;
			break;            

            case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less_Social_Account_With_Progress:
                // A popup notifying the user that the social account already has some progress is shown. Do you want to load the remote progress? Two buttons:
                // Cancel: The user keeps playing with local progress but it's not connected to the social network
                // Ok: The game is reloaded with the cloud progress (account id is changed to the account id linked to that social account and relogin)                
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less);
				CloudDriverDebug.IsMergeEnabled = true;
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Corrupted:
                // A popup notifying about corrupted cloud persistence is shown. The game continues to load in background. Two buttons:
                // Continue: to keep playing with local peristence. The incentivised social reward popup is show. The user is logged in the social network.
                // Upload: to override cloud persistence with local persistence. After clicking a new popup confirming that the cloud was overriden is shown. After this popup the incentivised social reward popup is shown
                LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, LOCAL_DRAGON_SKU);
				CloudDriverDebug.PersistenceAsString = GetPersistenceCorrupted();
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Corrupted_Error_Upload:
                // A popup notifying about cloud persistence corrupted is shown. The game continues to load in background. Two buttons:
                // Continue: to keep playing with local peristence. The incentivised social reward popup is show.
                // Upload: to override cloud persistence with local persistence. After clicking a new popup shows an error because coud save was inaccessible until it gets fixed. Once it's fixed coud save overriden popup is shown.
                LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, LOCAL_DRAGON_SKU);
				CloudDriverDebug.PersistenceAsString = GetPersistenceCorrupted();
				CloudDriverDebug.IsUploadPersistenceEnabled = false;
			break;

			case EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Corrupted_Error_Merge:    
                // A popup notifying an error when syncing is shown (code 1: not allowed to use cloud persistence because it's corrupted). One button:
                // Ok: When clicked the user keeps playing with local progress not logged in the social platform            
                LocalDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.NeverLoggedIn, LOCAL_DRAGON_SKU);
				CloudDriverDebug.PersistenceAsString = GetPersistenceCorrupted();
				CloudDriverDebug.IsMergeEnabled = true;
			break;

			case EUserCaseId.Launch_Local_Corrupted_Cloud_NoConnection:
                // A popup lettign the user know that cloud couldn't be retrieved is shown. Once closed a popup letting the user know that local persistence is corrupted is shown.
				// If the user chooses to reset local persistence then the game is loaded with the default persistence.
				// If the user chooses to connect to cloud then a popup notifying that there's no connection is shown.
				// When this popup is closed the popup that lets the user choose between connecting to cloud and resetting is shown again
				LocalDriverDebug.PersistenceAsString = GetPersistenceCorrupted();
                CloudDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.LoggedInAndIncentivised, CLOUD_DRAGON_SKU);
                CloudDriverDebug.IsConnectionEnabled = false;
                LocalDriverDebug.Prefs_SocialId = "userId";
            break;

			case EUserCaseId.Launch_Local_Corrupted_Cloud_Ok:
				// A popup letting the user know that local persistence is corrupted but it was overriden by the cloud persistence is shown.
                // When the user clicks on "Ok" button the game continues to load with the cloud persistence                				
				LocalDriverDebug.PersistenceAsString = GetPersistenceCorrupted();
				CloudDriverDebug.PersistenceAsString = GetPersistence(UserProfile.ESocialState.LoggedInAndIncentivised, CLOUD_DRAGON_SKU);
                LocalDriverDebug.Prefs_SocialId = "userId";
            break;

			case EUserCaseId.Launch_Local_Corrupted_Cloud_Corrupted:
                // A popup letting the user know that both local end cloud persistence are corrupted is shown.                
                // When reset button is clicked game loads with both local and cloud persistences resetted to default
                LocalDriverDebug.PersistenceAsString = GetPersistenceCorrupted();
				CloudDriverDebug.PersistenceAsString = GetPersistenceCorrupted();
                LocalDriverDebug.Prefs_SocialId = "userId";
            break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_NoConnection:
                // A popup showing no connection message is shown. Once the user can connect the incentivised social reward popup is shown.
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
                CloudDriverDebug.NeedsToIgnoreSycnFromLaunch = true;                                
            break;

			case EUserCaseId.Settings_Local_LoggedIn_Cloud_Error_NotLoggedInSocial:
				// No error popup is shown because the social network should show the error popup
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Error_NotLoggedInSocial);
                CloudDriverDebug.NeedsToIgnoreSycnFromLaunch = true;
            break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Error_GetPersistence:
                // A popup notifying an error when accessing to the cloud is shown. Two buttons:
                // Continue: The user keeps playing with local persistence. The incentivised social reward popup is shown and the user is logged in to the social network.
                // Retry: A new attempt is performed.
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Error_GetPersistence);
                CloudDriverDebug.NeedsToIgnoreSycnFromLaunch = true;
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
                CloudDriverDebug.NeedsToIgnoreSycnFromLaunch = true;
            break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Error_FullDisk_Cloud_More:
                // The merge popup is shown to let the user choose between local and cloud persistence.
                // The incentivised social reward popup is shown regardless the persistence chosen by the user.
                // If the user chooses local persistence or just closes the popup then the game continues with the local progress.
                // If the user chooses cloud persistence then a popup notifying about an error when trying to save is shown repeatedly until the problem is solved and then the game is reloaded with that progress                
                SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Error_FullDisk_Cloud_More);
                CloudDriverDebug.NeedsToIgnoreSycnFromLaunch = true;
            break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Less:
				// When login button is clicked a popup showing the login reward is shown and
				// cloud and local are in sync with local persistence
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less);
                CloudDriverDebug.NeedsToIgnoreSycnFromLaunch = true;
            break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Equal:
				// When login button is clicked a popup showing the login reward is shown and
				// cloud and local are in sync with local persistence
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Equal);
                CloudDriverDebug.NeedsToIgnoreSycnFromLaunch = true;
            break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Less_Error_Upload:
				// When login button is clicked a popup showing an error because an error
				// when saving the local progress in the cloud occured is shown. If the error is fixed
				// or the user decides to dismiss the error then a popup showing the login reward is shown. 
				// Persistences are in sync only if the error was fixed, otherwise they will get
				// synced automatically when the error is fixed.
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less_Error_Upload);
                CloudDriverDebug.NeedsToIgnoreSycnFromLaunch = true;
            break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Less_Social_Account_With_Progress:
                // A popup notifying the user that the social account already has some progress is shown. Do you want to load the remote progress? Two buttons:
                // Cancel: The user keeps playing with local progress but it's not connected to the social network
                // Ok: The game is reloaded with the cloud progress (account id is changed to the account id linked to that social account and relogin)                
                SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Less_Social_Account_With_Progress);
                CloudDriverDebug.NeedsToIgnoreSycnFromLaunch = true;
            break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Corrupted:
                // When login button is clicked a popup notifies the user that cloud persistence 
                // is corrupted. If the user choose to ignore the problem then cloud won't be
                // in sync and uploading local persistence will be disabled. 
                // If the user decides to override cloud persistence with local persistence then
                // we do so and local and cloud will be in sync if everything goes ok.
                // After the popup the login reward is given to the user.				
                SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Corrupted);
                CloudDriverDebug.NeedsToIgnoreSycnFromLaunch = true;
            break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Corrupted_Error_Upload:
				// When login button is clicked a popup notifies the user that cloud persistence 
				// is corrupted. When the user decides to override cloud persistence with local persistence a
				// popup notifying an error when writing in cloud is shown until the problem is fixed
				// or the user decides to ignore the probem.
				// The login reward is given to the user after this popup.
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Corrupted_Error_Upload);
                CloudDriverDebug.NeedsToIgnoreSycnFromLaunch = true;
            break;

			case EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_Corrupted_Error_Merge:
				// When login button is clicked a popup notifies the user that cloud persistence 
				// is corrupted. Local persistence can't override cloud persistence because there's a social
				// merge to solve. This is notified to the user with a popup that let her continue playing
				// locally, which means that the social login hasn't been completed so login reward is not given
				// to the user.				
				SetupUserCaseId(EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Corrupted_Error_Merge);
                CloudDriverDebug.NeedsToIgnoreSycnFromLaunch = true;
            break;
		}
	}   

	private string GetPersistenceCorrupted()
	{
		return "{\"userProfile\":{\"sc\":\"10000:0\",\"pc\":\"0:0\",\"timestamp\":\"09 / 06 / 2017 07:31:01\"},\"User\":{\"NumGameLoops\":0,\"deviceName\":\"BCDTDAVIDGERM\",\"modifiedTime\":1504683061}";
	}

	private string GetPersistence(UserProfile.ESocialState socialState, string initialDragonSku=null, int timePlayed=0)
	{   
        return PersistenceUtils.GetDefaultDataFromProfile("", initialDragonSku, socialState.ToString(), timePlayed).ToString();
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
