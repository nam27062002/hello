﻿namespace FGOL.Save.SaveStates
{
    public enum SpaceRequirementResult
    {
        PathUnavailable,
        OutOfDiskSpace,
        OK
    }

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

    public enum SyncState
    {
        Successful,
        UpgradeNeeded,
        PermissionError,
        Corrupted,
        Inaccessible,
        Error
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