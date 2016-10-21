using UnityEngine;
using System.Collections;
using System;

#if AMAZON

public class AmazonGameServicesInterface : GameServicesInterface
{
    // Supported features
    private const bool m_supportsLeaderboards = true;
    private const bool m_supportsAchievements = true;
    private const bool m_supportsWhisperSync = false;

    private Action<bool> m_lastAuthenticateCallback = null;

    public override void Init()
    {
        AGSClient.ServiceReadyEvent += OnGameServicesReady;
        AGSClient.ServiceNotReadyEvent += OnGameServicesNotReady;
        AGSLeaderboardsClient.SubmitScoreCompleted += OnLeaderboardsScoreCompleted;
        AGSAchievementsClient.UpdateAchievementCompleted += OnAchievementsUpdateCompleted;

        //  Make new game object
        GameObject amazonGameManager = new GameObject();
        GameObject.DontDestroyOnLoad(amazonGameManager);
        amazonGameManager.ForceGetComponent<GameCircleManager>();
    }

    #region CALLBACKS

    private void TriggerAuthCallback(bool success)
    {
        if (m_lastAuthenticateCallback != null)
        {
            m_lastAuthenticateCallback(success);
            m_lastAuthenticateCallback = null;
        }
    }

    private void OnGameServicesReady()
    {
        Debug.Log("Amazon game services are ready!");
        TriggerAuthCallback(true);
    }

    private void OnGameServicesNotReady(string error)
    {
        Debug.Log("Amazon game services are not ready! " + error);
        TriggerAuthCallback(false);
    }

    private void OnLeaderboardsScoreCompleted(AGSSubmitScoreResponse response)
    {
        Debug.Log("AGS: Score submitteed" + response.leaderboardId);
    }

    private void OnAchievementsUpdateCompleted(AGSUpdateAchievementResponse response)
    {
        Debug.Log("AGS: Achievement updated" + response.achievementId);
    }

    #endregion

    public override void Authenticate(Action<bool> onAuthCallback, bool force)
    {
        m_lastAuthenticateCallback = onAuthCallback;
        AGSClient.Init(m_supportsLeaderboards, m_supportsAchievements, m_supportsWhisperSync);
    }

    public override bool IsLoggedIn()
    {
        return AGSClient.IsServiceReady();
    }

    public override void PostScore(string leaderboardID, long score)
    {
        if (IsLoggedIn())
        {
            AGSLeaderboardsClient.SubmitScore(m_leaderboards[leaderboardID].GameCircleID, score);
        }
    }

#if !PRODUCTION
    public override void ResetAchievements()
    {
        Debug.LogError("Amazon achievements can't be reset!");
    }
#endif

    public override void ShowAchievements()
    {
        if (IsLoggedIn())
        {
            AGSAchievementsClient.ShowAchievementsOverlay();
        }
    }

    public override void ShowLeaderboard(string leaderboardID = null)
    {
        if (IsLoggedIn())
        {
            AGSLeaderboardsClient.ShowLeaderboardsOverlay();
        }
    }

    public override void ShowLockedAchievement(string achievementID)
    {
        string id = m_achievements[achievementID].ID.GameCircleID;
        AGSAchievementsClient.UpdateAchievementProgress(id, 0);
    }

    public override void UnlockAchievement(string achievementID)
    {
        string id = m_achievements[achievementID].ID.GameCircleID;
        AGSAchievementsClient.UpdateAchievementProgress(id, 100);
    }

    public override void UpdateAchievement(string achievementID, float previousPercentage, float currentPercentage)
    {
        string id = m_achievements[achievementID].ID.GameCircleID;
        AGSAchievementsClient.UpdateAchievementProgress(id, currentPercentage);
    }
}

#endif