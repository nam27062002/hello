using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;
using System;

public class EditorSocialPlatform : ISocialPlatform 
{
	EditorLocalUser mLocalUser = null;

	public EditorSocialPlatform()
	{
		mLocalUser = new EditorLocalUser();
	}
	

	public void Authenticate(Action<bool> callback) 
	{
		Authenticate(callback, false);
	}
	
	public void Authenticate(Action<bool> callback, bool silent) 
	{
		mLocalUser.Authenticate(callback);
	}
	
	public void Authenticate(ILocalUser unused, Action<bool> callback) 
	{
		Authenticate(callback, false);
	}
	
	public ILocalUser localUser 
	{
		get {
			return mLocalUser;
		}
	}
	
	public void LoadUsers(string[] userIDs, Action<IUserProfile[]> callback) 
	{
		if (callback != null) 
		{
			callback.Invoke(new IUserProfile[0]);
		}
	}
	
	public void ReportProgress(string achievementID, double progress, Action<bool> callback) 
	{
		callback.Invoke(true);
	}
	
	public void LoadAchievementDescriptions(Action<IAchievementDescription[]> callback) 
	{
		if (callback != null) 
		{
			callback.Invoke(new IAchievementDescription[0]);
		}
	}
	
	
	public void LoadAchievements(Action<IAchievement[]> callback) {
		if (callback != null) 
		{
			callback.Invoke(new IAchievement[0]);
		}
	}
	
	public IAchievement CreateAchievement () 
	{
		return new EditorAchievement();
	}
	
	public void ReportScore (long score, string board, Action<bool> callback) 
	{
		if (callback != null)
		{
			callback.Invoke(true);
		}
	}
	
	public void LoadScores(string leaderboardID, Action<IScore[]> callback) 
	{
		if (callback != null) {
			callback.Invoke(new IScore[0]);
		}
	}
	
	public ILeaderboard CreateLeaderboard () 
	{
		return null;
	}
	
	public void ShowAchievementsUI () 
	{
		
	}
	
	public void ShowLeaderboardUI () 
	{
	
	}
	
	public void LoadFriends (ILocalUser user, Action<bool> callback) 
	{
		if (callback != null) {
			callback.Invoke(false);
		}
	}
	
	public void LoadScores (ILeaderboard board, Action<bool> callback) 
	{
		
		if (callback != null) 
		{
			callback.Invoke(false);
		}
	}
	
	public bool GetLoading (ILeaderboard board) 
	{
		return false;
	}
	
}
