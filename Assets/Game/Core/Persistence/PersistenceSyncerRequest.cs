using System;

public class PersistenceSyncerRequest
{
	public PersistenceSyncerRequest()
	{
		Reset();
	}

	public PersistenceCloudDriver.ESyncMode Mode { get; set; }
	public PersistenceCloudDriver.EErrorMode ErrorMode { get; set; }
	public bool IsAppInit { get; set; }
	public SocialUtils.EPlatform PlatformId { get; set; }
	public SocialPlatformManager.ELoginResult LogInSocialResult { get; set; }
	public Action<PersistenceStates.ESyncResult, PersistenceStates.ESyncResultDetail> OnSyncDone { get; set; }

	/// <summary>
	/// When <c>true</c> the local user server Id is forced to be linked to the social user Id used for merging accounts
	/// </summary>
	public bool ForceMerge { get; set; }	

	private PersistenceComparator mComparator;
	public PersistenceComparator Comparator
	{
		get
		{
			if (mComparator == null)
			{
				mComparator = new HDPersistenceComparator();
			}

			return mComparator;
		}
	}

	public bool NeedsToShowCloudOverridenPopup { get; set; }

	public float Timer
	{
		get; set;
	}

	public void Reset()
	{
		Comparator.Reset();
		
		IsAppInit = false;
		Mode = PersistenceCloudDriver.ESyncMode.Full;
		ErrorMode = PersistenceCloudDriver.EErrorMode.Verbose;
		PlatformId = SocialUtils.EPlatform.None;
		OnSyncDone = null;
		ForceMerge = false;
		LogInSocialResult = SocialPlatformManager.ELoginResult.Error;
		NeedsToShowCloudOverridenPopup = false;
		Timer = 0f;
	}
}