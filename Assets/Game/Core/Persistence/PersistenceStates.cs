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
        Error_Cloud_Not_Connection,

        // Any other error
        Error_Default 
    };
}
