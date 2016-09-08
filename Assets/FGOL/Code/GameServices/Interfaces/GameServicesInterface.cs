using System;
using System.Collections.Generic;

public abstract class GameServicesInterface
{
    public enum AchievementType
    {
        Standard,
        Progress
    }

    [System.Serializable]
    public struct AchievementIDDef
    {
        public string GooglePlayServicesID;
        public string GameCenterID;
        public string GameCircleID;

        public AchievementIDDef(string googlePlayServicesID, string gameCenterID, string gameCircleID)
        {
            GooglePlayServicesID = googlePlayServicesID;
            GameCenterID = gameCenterID;
            GameCircleID = gameCircleID;
        }
    }

    [System.Serializable]
    public struct AchievementDef
    {
        public AchievementIDDef ID;
        public AchievementType Type;
        public int Steps;

        public AchievementDef(AchievementIDDef id, AchievementType type, int steps = 0)
        {
            ID = id;
            Type = type;
            Steps = 0;

            if (Type == AchievementType.Progress)
            {
                if(steps == 0)
                {
                    Steps = 100;
                }
            }
        }
    }

    public struct LeaderboardDef
    {
        public string GooglePlayServicesID;
        public string GameCenterID;
        public string GameCircleID;
    }

    protected Dictionary<string, AchievementDef> m_achievements = new Dictionary<string, AchievementDef>();
    protected Dictionary<string, LeaderboardDef> m_leaderboards = new Dictionary<string, LeaderboardDef>();

    public void RegisterAchievement(string lookupName, AchievementDef def)
    {
        if (!m_achievements.ContainsKey(lookupName))
        {
            m_achievements.Add(lookupName, def);
        }
    }

    public void RegisterLeaderboard(string lookupName, LeaderboardDef def)
    {
		if(!m_leaderboards.ContainsKey(lookupName))
		{
			m_leaderboards.Add(lookupName, def);
		}
		else
		{
			m_leaderboards[lookupName] = def;
		}
    }

    public abstract void Init();
    public abstract void Authenticate(Action<bool> onAuthCallback, bool force);
    public abstract bool IsLoggedIn();
    public abstract void ShowLockedAchievement(string achievementID);
    public abstract void UnlockAchievement(string achievementID);
    public abstract void UpdateAchievement(string achievementID, float previousPercentage, float currentPercentage);
    public abstract void ShowAchievements();
    public abstract void PostScore(string leaderboardID, long score);
    public abstract void ShowLeaderboard(string leaderboardID = null);

#if !PRODUCTION
    public abstract void ResetAchievements();
#endif
}