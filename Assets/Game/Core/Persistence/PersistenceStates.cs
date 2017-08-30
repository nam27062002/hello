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
}
