public class PersistenceLocalDriverDebug : PersistenceLocalDriver
{
	public string PersistenceAsString { get; set; }

	public bool IsPermissionErrorEnabled { get; set; }
	public bool isFullDiskErrorEnabled { get; set; }

	public PersistenceLocalDriverDebug()
	{
		Reset();	
	}

	protected override void ExtendedReset()
	{
		PersistenceAsString = null;
		IsPermissionErrorEnabled = false;
		isFullDiskErrorEnabled = false;
	}

	protected override void ExtendedLoad()
	{
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

    /*
	private int KeyPressed { get; set; }
	// UNGTK
	public override void OnKeyPressed()
	{
		KeyPressed++;

		if (KeyPressed == 2)
		{
			IsPermissionErrorEnabled = false;
			isFullDiskErrorEnabled = false;
		}
	}
    */
}
