public class PersistenceStates
{
    public enum ELoadState
    {
        OK,
        NotFound,
        PermissionError,
        Corrupted,        
    }

    public enum ESaveState
    {
        OK,
        PermissionError,
        DiskSpace        
    }

    public enum EConflictState
    {
        RecommendLocal,
        RecommendCloud,
        UseLocal,
        UseCloud,
        UserDecision,
        Equal        
    }

    public enum EConflictResult
    {
        Local,
        Cloud,
        Dismissed
    }

	public enum ESyncResult
	{
		Ok,
		ErrorLogging,
		ErrorSyncing, // Log In successfully but local couldn't be sent to cloud
		NeedsToReload
	};
}
