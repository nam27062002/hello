using System;

public class NullGameServicesInterface : GameServicesInterface
{
    public override void Init()
    {
    }

    public override void Authenticate(Action<bool> onAuthCallback, bool force)
    {
        onAuthCallback(false);
    }

    public override bool IsLoggedIn()
    {
        return false;
    }

    public override void ShowLockedAchievement(string achievementID)
    {
    }

    public override void UnlockAchievement(string achievementID)
    {
    }

    public override void UpdateAchievement(string achievementID, float previousPercentage, float currentPercentage)
    {
    }

    public override void ShowAchievements()
    {
    }

    public override void PostScore(string leaderboardID, long score)
    {
    }

    public override void ShowLeaderboard(string leaderboardID = null)
    {
    }

#if !PRODUCTION
    public override void ResetAchievements()
    {
        Debug.LogError("you're in the editor aren't you? silly billy");
    }
#endif

}
