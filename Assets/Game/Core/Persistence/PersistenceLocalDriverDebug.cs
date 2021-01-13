public class PersistenceLocalDriverDebug : PersistenceLocalDriver
{
	public string PersistenceAsString { get; set; }

	public bool IsPermissionErrorEnabled { get; set; }
	public bool isFullDiskErrorEnabled { get; set; }

    private int LoadTimes { get; set; }
    private int SaveTimes { get; set; }

    public PersistenceLocalDriverDebug()
	{
		Reset();

        PersistenceAsString = null;
        IsPermissionErrorEnabled = false;
        isFullDiskErrorEnabled = false;
        LoadTimes = 0;
        SaveTimes = 0;
    }	

	protected override void ExtendedLoad()
	{
        LoadTimes++;
        if (LoadTimes == 2)
        {
            IsPermissionErrorEnabled = false;
        }

        if (IsPermissionErrorEnabled)
		{
			Data.LoadState = PersistenceStates.ELoadState.PermissionError;
		} 
		else if (string.IsNullOrEmpty (PersistenceAsString))
		{
			Data.LoadState = PersistenceStates.ELoadState.NotFound;
		} 
		else
		{
			Data.LoadFromString(PersistenceAsString);
		}
	}
    
    protected override void ExtendedSave()
	{
        SaveTimes++;

        if (SaveTimes == 4)
        {
            IsPermissionErrorEnabled = false;
            isFullDiskErrorEnabled = false;
        }

        if (IsPermissionErrorEnabled)
		{
			Data.SaveState = PersistenceStates.ESaveState.PermissionError;
		} 
		else if (isFullDiskErrorEnabled)
		{
			Data.SaveState = PersistenceStates.ESaveState.DiskSpace;
		} 
		else
		{
			Data.Systems_Save();
			PersistenceAsString = Data.ToString();
			Data.SaveState = PersistenceStates.ESaveState.OK;
		}
	}

    #region prefs
    private string mPrefsSocialId;

    public override string Prefs_SocialId
    {
        get { return mPrefsSocialId; }
        set { mPrefsSocialId = value; }
    }
    #endregion   
}
