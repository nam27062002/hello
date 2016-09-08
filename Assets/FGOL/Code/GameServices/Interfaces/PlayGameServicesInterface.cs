#if GOOGLE_PLAY_SERVICES
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using UnityEngine;

public class PlayGameServicesInterface : GameServicesInterface
{
	private bool m_lastGPGPSLoginFailed
	{
		get
		{
			return PlayerPrefs.GetInt("GPGS_LOGIN_FAILED", 0) == 1;
		}
		set
		{
			PlayerPrefs.SetInt("GPGS_LOGIN_FAILED", value ? 1 : 0);
		}
	}

    public override void Init()
    {
#if !PRODUCTION
        PlayGamesPlatform.DebugLogEnabled = true;
#endif
    }

    public override void Authenticate(Action<bool> onAuthCallback, bool force)
    {
		//	Callback that clears saved silent login flags on success
		Action<bool> forwardCallback = (bool success) =>
		{
			if (success)
			{
				//	Clear flag
				m_lastGPGPSLoginFailed = false;
			}
			
			//	Forward callback to game
			if (onAuthCallback != null)
			{ 
				onAuthCallback(success);
			}
		};

		if (force)
		{
			//	Already tried silent login and force is on - now do UI attempt
			PlayGamesPlatform.Instance.Authenticate(forwardCallback, false);
		}
		else if (!m_lastGPGPSLoginFailed)
		{
			//	First login - check if we can log in silently
			PlayGamesPlatform.Instance.Authenticate(success => 
			{
				if (success)
				{
					forwardCallback(true);
				}
				else
				{
					//	Silent request failed - try and do it with UI
					m_lastGPGPSLoginFailed = true;
					PlayGamesPlatform.Instance.Authenticate(forwardCallback, false);
				}
			}, true);
		}
		else
		{
			//	Just do a callback back so game knows
			forwardCallback(false);
		}
	}

	public override bool IsLoggedIn()
    {
        return PlayGamesPlatform.Instance.IsAuthenticated();
    }

    public override void ShowLockedAchievement(string achievementID)
    {
        FGOL.Assert.Fatal(m_achievements.ContainsKey(achievementID), "Achievement must be registered");

        string id = m_achievements[achievementID].ID.GooglePlayServicesID;

        if (!string.IsNullOrEmpty(id))
        {
            if(PlayGamesPlatform.Instance.IsAuthenticated())
            {
                PlayGamesPlatform.Instance.ReportProgress(id, 0, delegate(bool success)
                {
                    if(!success)
                    {
                        Debug.LogWarning("PlayGameServicesInterface (ShowLockedAchievement) :: Failed to show locked achievement with ID: " + id);
                    }
                });
            }
        }
    }

    public override void UnlockAchievement(string achievementID)
    {
        FGOL.Assert.Fatal(m_achievements.ContainsKey(achievementID), "Achievement must be registered");
        FGOL.Assert.Fatal(m_achievements[achievementID].Type == AchievementType.Standard, "This method is only for standard achievements");

        string id = m_achievements[achievementID].ID.GooglePlayServicesID;

        if(!string.IsNullOrEmpty(id))
        {
            if(PlayGamesPlatform.Instance.IsAuthenticated())
            {
                PlayGamesPlatform.Instance.ReportProgress(id, 100, delegate(bool success)
                {
                    if(!success)
                    {
                        Debug.LogWarning("PlayGameServicesInterface (UnlockAchievement) :: Failed to unlock achievement with ID: " + id);
                    }
                });
            }
        }
    }

    public override void UpdateAchievement(string achievementID, float previousPercentage, float currentPercentage)
    {
        FGOL.Assert.Fatal(m_achievements.ContainsKey(achievementID), "Achievement must be registered");
        FGOL.Assert.Fatal(m_achievements[achievementID].Type == AchievementType.Progress, "This method is only for progress achievements");
        FGOL.Assert.Fatal(currentPercentage > previousPercentage, "Current percentage must be greater then previous!");

        string id = m_achievements[achievementID].ID.GooglePlayServicesID;

        if(!string.IsNullOrEmpty(id))
        {
            int totalSteps = m_achievements[achievementID].Steps;
            int previousSteps = (int)Math.Floor(previousPercentage * totalSteps);
            int currentSteps = (int)Math.Floor(currentPercentage * totalSteps);

            int updatedSteps = currentSteps - previousSteps;

            if(PlayGamesPlatform.Instance.IsAuthenticated())
            {
                PlayGamesPlatform.Instance.IncrementAchievement(id, updatedSteps, delegate(bool success)
                {
                    if(!success)
                    {
                        Debug.LogWarning("PlayGameServicesInterface (UpdateAchievement) :: Failed to update progress on achievement with ID: " + id);
                    }
                });
            }
        }
    }

    public override void ShowAchievements()
    {
        if(PlayGamesPlatform.Instance.IsAuthenticated())
        {
            PlayGamesPlatform.Instance.ShowAchievementsUI();
        }
    }

    public override void PostScore(string leaderboardID, long score)
    {
        FGOL.Assert.Fatal(m_leaderboards.ContainsKey(leaderboardID), "Leaderboard must be registered");

        string id = m_leaderboards[leaderboardID].GooglePlayServicesID;

        if(!string.IsNullOrEmpty(id))
        {
            if(PlayGamesPlatform.Instance.IsAuthenticated())
            {
                PlayGamesPlatform.Instance.ReportScore(score, id, delegate(bool success)
                {
                    if(!success)
                    {
                        Debug.LogWarning("PlayGameServicesInterface (PostScore) :: Failed to post score to leaderboard with ID: " + id);
                    }
                });
            }
        }
    }

    public override void ShowLeaderboard(string leaderboardID = null)
    {
        FGOL.Assert.Fatal(string.IsNullOrEmpty(leaderboardID) || m_leaderboards.ContainsKey(leaderboardID), "Leaderboard must be registered");

        if(PlayGamesPlatform.Instance.IsAuthenticated())
        {
            if(string.IsNullOrEmpty(leaderboardID))
            {
                PlayGamesPlatform.Instance.ShowLeaderboardUI();
            }
            else
            {
                string id = m_leaderboards[leaderboardID].GooglePlayServicesID;

                if(!string.IsNullOrEmpty(id))
                {
                    PlayGamesPlatform.Instance.ShowLeaderboardUI(id);
                }
            }
        }
    }

#if !PRODUCTION
    public override void ResetAchievements()
    {
        Debug.LogError("Android Achievements cannot be reset this way");
    }
#endif

}
#endif
