public class PersistenceStates
{
    public enum LoadState
    {
        OK,
        NotFound,
        PermissionError,
        Corrupted,
        //VersionMismatch
    }

    public enum SaveState
    {
        OK,
        PermissionError,
        DiskSpace        
    }

    public enum ConflictState
    {
        RecommendLocal,
        RecommendCloud,
        UseLocal,
        UseCloud,
        UserDecision,
        Equal,
        LocalSaveCorrupt,
        CloudSaveCorrupt,
        LocalCorruptUpgradeNeeded
    }

    public enum ConflictResult
    {
        Local,
        Cloud,
        Dismissed
    }

	public enum ESyncResult
	{
		None,
		Success,

        // Syncer errors
		Error_Disabled,
		Error_Already_Syncing,

        // Local errors
        Error_Local_Load_NotFound,
        Error_Local_Load_Corrupted,
        Error_Local_Load_Permission,
        Error_Local_Save_Permission,
        Error_Local_Save_DiskSpace,

        // Cloud errors
        Error_Cloud_NotConnection,
        Error_Cloud_Social_NotLogged,
        Error_Cloud_Server_NotLogged,
        Error_Cloud_Server_Persistence,
        Error_Cloud_Server_MergeFailed,
        Error_Cloud_Server_MergeShowPopupNeeded,

        // Any other error
        Error_Default 
    };
}
