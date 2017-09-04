public class PersistenceStates
{
    public enum LoadState
    {
        OK,
        NotFound,
        PermissionError,
        Corrupted,
        VersionMismatch
    }

    public enum SaveState
    {
        OK,
        PermissionError,
        DiskSpace,
        Disabled
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
}
