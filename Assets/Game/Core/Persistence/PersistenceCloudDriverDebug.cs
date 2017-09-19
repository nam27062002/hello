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

	protected override void ExtendedReset()
	{
		PersistenceAsString = "{}";
		IsConnectionEnabled = true;
		IsLogInServerEnabled = true;
		IsLogInSocialEnabled = true;
		IsMergeEnabled = false;
		IsGetPersistenceEnabled = true;
		IsUploadPersistenceEnabled = true;
	}

	protected override void Syncer_ExtendedCheckConnection(Action<bool> onDone)
	{
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
				// reload so we don't want this to be send again
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

    /*
	private int KeysAmount { get; set; }
	public override void OnKeyPressed()
	{
		KeysAmount++;
		IsConnectionEnabled = true;

		if (KeysAmount == 2)
			IsUploadPersistenceEnabled = true;
	}
    */
}
