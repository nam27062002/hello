using System;
public class PersistenceCloudDriverDebug : PersistenceCloudDriver
{
	public string PersistenceAsString { get; set; }
	public bool IsConnectionEnabled { get; set; }
	public bool IsLogInServerEnabled { get; set; }
	public bool IsLogInSocialEnabled { get; set; }
	public bool IsMergeEnabled { get; set; }
	public bool IsGetPersistenceEnabled { get; set; }
	public bool IsUploadPersistenceEnabled { get; set; }

    private bool mNeedsToIgnoreSyncFromLaunch;

    private int ConnectionTimes { get; set; }
    private int UploadTimes { get; set; }

    public bool NeedsToIgnoreSycnFromLaunch
    {
        get
        {
            return mNeedsToIgnoreSyncFromLaunch;
        }

        set
        {
            mNeedsToIgnoreSyncFromLaunch = value;
            if (mNeedsToIgnoreSyncFromLaunch)
            {
                IsConnectionEnabled = false;
            }
        }
    }

    protected override void ExtendedReset()
	{
		PersistenceAsString = "{}";
		IsConnectionEnabled = true;
		IsLogInServerEnabled = true;
		IsLogInSocialEnabled = true;
		IsMergeEnabled = false;
		IsGetPersistenceEnabled = true;
		IsUploadPersistenceEnabled = true;
        NeedsToIgnoreSycnFromLaunch = false;
        ConnectionTimes = 0;        
    }

	protected override void Syncer_ExtendedCheckConnection(Action<bool> onDone)
	{
        ConnectionTimes++;

        if (ConnectionTimes == 4)
            IsConnectionEnabled = true;
        
        if (!Syncer_IsAppInit && mNeedsToIgnoreSyncFromLaunch)
        {
            IsConnectionEnabled = true;
        }

		if (onDone != null)
		{
			onDone(IsConnectionEnabled);
		}
	}

	protected override void Syncer_ExtendedLogInServer(Action<bool> onDone)
	{
		if (onDone != null)
		{
			onDone(IsLogInServerEnabled);
		}
	}

	protected override void Syncer_ExtendedLogInSocial(Action<SocialPlatformManager.ELoginResult, string> onDone)
	{
		if (onDone != null)
		{
			if (!IsLogInSocialEnabled)
			{
				onDone(SocialPlatformManager.ELoginResult.Error, null);
			}
			else if (IsMergeEnabled)
			{
				// We need to do it only once because the flow might make the game
				// reload so we don't want this to be sent again
				IsMergeEnabled = false;				
				onDone(SocialPlatformManager.ELoginResult.MergeLocalOrOnlineAccount, PersistenceAsString);
			}
			else
			{
				onDone(SocialPlatformManager.ELoginResult.Ok, null);
			}
		}
	}

	protected override void Syncer_ExtendedGetPersistence(Action<bool> onDone)
	{
		if (IsGetPersistenceEnabled)
		{
			Data.LoadFromString(PersistenceAsString);	
		} 

		if (onDone != null)
		{
			onDone(IsGetPersistenceEnabled);
		}
	}

	protected override void Upload_Perform (string persistence, Action<bool> onDone)
	{
        UploadTimes++;
        if (UploadTimes == 2)
        {
            IsUploadPersistenceEnabled = true;
        }

        if (IsUploadPersistenceEnabled)
		{
			PersistenceAsString = persistence;
			Data.LoadFromString (persistence);            
		}

        if (onDone != null)
		{
			onDone(IsUploadPersistenceEnabled);
		}
	}          
}
