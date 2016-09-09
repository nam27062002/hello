using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;

public class GameCenterServicesInterface : GameServicesInterface
{
    private bool m_isLoggedIn = false;

    public override void Init()
    {
    }

    public override void Authenticate(Action<bool> onAuthCallback, bool force)
    {
        GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
        Social.Active.Authenticate(Social.localUser, delegate(bool loggedIn)
        {
            m_isLoggedIn = loggedIn;
            onAuthCallback(loggedIn);
        });
    }

    public override bool IsLoggedIn()
    {
        return m_isLoggedIn;
    }

    public override void ShowLockedAchievement(string achievementID)
    {
        FGOL.Assert.Fatal(m_achievements.ContainsKey(achievementID), "Achievement must be registered");

        string id = m_achievements[achievementID].ID.GameCenterID;

        if(!string.IsNullOrEmpty(id))
        {
            if(m_isLoggedIn)
            {
                // [DGR] ACHIEVEMENTS Not supported yet
                //GKAchievementReporter.ReportAchievement(id, 0, true);
            }
        }
    }

    public override void UnlockAchievement(string achievementID)
    {
        FGOL.Assert.Fatal(m_achievements.ContainsKey(achievementID), "Achievement must be registered");
        FGOL.Assert.Fatal(m_achievements[achievementID].Type == AchievementType.Standard, "This method is only for standard achievements");

        string id = m_achievements[achievementID].ID.GameCenterID;

        if(!string.IsNullOrEmpty(id))
        {
            if(m_isLoggedIn)
            {
                // [DGR] ACHIEVEMENTS Not supported yet
                //GKAchievementReporter.ReportAchievement(id, 100, true);
            }
        }
    }

    public override void UpdateAchievement(string achievementID, float previousPercentage, float currentPercentage)
    {
        FGOL.Assert.Fatal(m_achievements.ContainsKey(achievementID), "Achievement must be registered");
        FGOL.Assert.Fatal(m_achievements[achievementID].Type == AchievementType.Progress, "This method is only for progress achievements");
        FGOL.Assert.Fatal(currentPercentage > previousPercentage, "Current percentage must be greater then previous!");

        string id = m_achievements[achievementID].ID.GameCenterID;

        if(!string.IsNullOrEmpty(id))
        {
            if(m_isLoggedIn)
            {
                // [DGR] ACHIEVEMENTS Not supported yet
                //GKAchievementReporter.ReportAchievement(id, currentPercentage, true);
            }
        }
    }

    public override void ShowAchievements()
    {
        if(m_isLoggedIn)
        {
            Social.Active.ShowAchievementsUI();
        }
    }

    public override void PostScore(string leaderboardID, long score)
    {
        FGOL.Assert.Fatal(m_leaderboards.ContainsKey(leaderboardID), "Leaderboard must be registered");

        string id = m_leaderboards[leaderboardID].GameCenterID;

        if(!string.IsNullOrEmpty(id))
        {
            if(m_isLoggedIn)
            {
                Social.Active.ReportScore(score, id, delegate(bool success)
                {
                    if(!success)
                    {
                        Debug.LogWarning("GameCenterServicesInterface (PostScore) :: Failed to post score to leaderboard with ID: " + id);
                    }
                });
            }
        }
    }

    public override void ShowLeaderboard(string leaderboardID = null)
    {
        FGOL.Assert.Fatal(string.IsNullOrEmpty(leaderboardID) || m_leaderboards.ContainsKey(leaderboardID), "Leaderboard must be registered");

        if(m_isLoggedIn)
        {
            Social.Active.ShowLeaderboardUI();
        }
    }

#if !PRODUCTION
    public override void ResetAchievements()
    {
        GameCenterPlatform.ResetAllAchievements(HandleAchievementReset);
    }

    private void HandleAchievementReset(bool status)
    {
        if (status)
        {
            Debug.LogError("Achievements should have been reset");
        }
        else
        {
            Debug.LogError("Achievements have failed to reset");
        }
    }
#endif
}
