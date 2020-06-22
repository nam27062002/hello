#if UNITY_EDITOR

using SimpleJSON;
using UnityEngine;

public class PersistenceTest
{
    public const string PREFS_LOCAL_USER_ID = "PersistenceTest_LocalUserId";

    public enum EProgress
    {
        Empty,
        P2,
        P1
    };

    public enum EUserId
    {
        None,
        U1,
        U2
    };
    
    public enum EExplicitPlatformState
    {
        None,
        LoggedInWhenQuit,
        LoggedOutWhenQuit
    };

    public enum EImplicitMergeResponse
    {
        None,
        Ok,
        OkForce,
        Error,
        Conflict
    };

    public EUserId Start_LocalUserId { get; set; }
    public EProgress Start_LocalProgress { get; set; }
    public PersistenceCloudDriver.EMergeState Start_MergeState { get; set; }
    public EExplicitPlatformState Start_ExplicitPlatformState { get; set; }
    public EUserId Start_CloudUserId { get; set; }
    public EProgress Start_CloudProgress { get; set; }

    public EImplicitMergeResponse ImplicitMergeResponse { get; set; }
    public EImplicitMergeResponse ImplicitMergeResponse2 { get; set; }

    public EUserId End_LocalUserId { get; set; }
    public EProgress End_LocalProgress { get; set; }
    public PersistenceCloudDriver.EMergeState End_MergeState { get; set; }
    public EExplicitPlatformState End_ExplicitPlatformState { get; set; }
    public UserProfile.ESocialState End_SocialState { get; set; }
    public PersistenceCloudDriver.ESyncMode End_SyncModeAtLaunch { get; set; }

    private UserProfile m_userProfile;
    private UserProfile UserProfile
    {
        get
        {
            if (m_userProfile == null)
            {
                m_userProfile = new UserProfile();
            }

            return m_userProfile;
        }
    }


    private PersistenceLocalDriver m_localDriver;
    private PersistenceLocalDriver LocalDriver
    {
        get
        {
            if (m_localDriver == null)
            {
                m_localDriver = new PersistenceLocalDriver();
                m_localDriver.Data.Systems_RegisterSystem(UserProfile);
            }

            return m_localDriver;
        }
    }

    private static SocialUtils.EPlatform EXPLICIT_PLATFORM = SocialUtils.EPlatform.SIWA;
    private static string EXPLICIT_PLATFORM_STRING = EXPLICIT_PLATFORM.ToString();

    private static string END_POINT_AUTH_B = "/api/auth/b";
    private static string END_POINT_LOGIN = "login";
    private static string END_POINT_MERGE = "/api/merge/c";
    private static string END_POINT_PERSISTENCE_GET = "/api/persistence/get";

    private bool HasAppBeenFirstLaunched { get; set; }

    public PersistenceCloudDriver.ESyncMode SyncModeAtLaunch { get; set; }

    public PersistenceTest(EUserId startLocalUserId, EProgress startLocalProgress, PersistenceCloudDriver.EMergeState startMergeState,
        EExplicitPlatformState startExplicitPlatformState, EUserId startCloudUserId, EProgress startCloudProgress, EImplicitMergeResponse implicitMergeResponse, EImplicitMergeResponse implicitMergeResponse2,
        EUserId endLocalUserId, EProgress endLocalProgress, PersistenceCloudDriver.EMergeState endMergeState, EExplicitPlatformState endExplicitPlatformState, UserProfile.ESocialState endSocialState,
        PersistenceCloudDriver.ESyncMode endSyncModeAtLaunch)
    {
        Start_LocalUserId = startLocalUserId;
        Start_LocalProgress = startLocalProgress;
        Start_MergeState = startMergeState;
        Start_ExplicitPlatformState = startExplicitPlatformState;
        Start_CloudUserId = startCloudUserId;
        Start_CloudProgress = startCloudProgress;

        ImplicitMergeResponse = implicitMergeResponse;
        ImplicitMergeResponse2 = implicitMergeResponse2;

        End_LocalUserId = endLocalUserId;
        End_LocalProgress = endLocalProgress;
        End_MergeState = endMergeState;
        End_ExplicitPlatformState = endExplicitPlatformState;
        End_SocialState = endSocialState;
        End_SyncModeAtLaunch = endSyncModeAtLaunch;
    }

    public void PrepareLocal()
    {
        HasAppBeenFirstLaunched = false;

        // If definitions are not loaded, do it now
        if (!Application.isPlaying)
        {
            ContentManager.InitContent(true, false);
            MissionManager.ResetSingleton();
            UsersManager.ResetSingleton();
            DragonManager.ResetSingleton();
            DragonManager.SetupUser(UserProfile);
        }

        string token = ((int)(Start_LocalUserId)).ToString();
        PlayerPrefs.SetString(GameSessionManager.KEY_ANONYMOUS_PLATFORM_USER_ID, token);

        JSONNode persistence = EProgressToPersistence(Start_LocalProgress);
        LocalDriver.Override(persistence.ToString(), null);

        LocalDriver.Prefs_SocialImplicitMergeState = Start_MergeState;

        switch (Start_ExplicitPlatformState)
        {
            case EExplicitPlatformState.None:
                LocalDriver.Prefs_SocialPlatformKey = "";
                LocalDriver.Prefs_SocialWasLoggedInWhenQuit = false;
                break;

            default:
                LocalDriver.Prefs_SocialPlatformKey = EXPLICIT_PLATFORM_STRING;
                LocalDriver.Prefs_SocialWasLoggedInWhenQuit = Start_ExplicitPlatformState == PersistenceTest.EExplicitPlatformState.LoggedInWhenQuit;
                break;
        }
    }

    private void PrepareImplicitMerge(EImplicitMergeResponse implicitMergeResponse, DummyNetworkManager networkManager)
    {
        string cmd = END_POINT_MERGE;
        int code = 200;
        string response = "{}";

        switch (implicitMergeResponse)
        {
            case EImplicitMergeResponse.OkForce:
                code = 204;
                break;

            case EImplicitMergeResponse.Error:
                code = 400;
                break;

            case EImplicitMergeResponse.Conflict:
                code = 300;
                int localUserId = EUserIdToUserId(Start_LocalUserId);
                int cloudUserId = EUserIdToUserId(Start_CloudUserId);
                JSONNode json = new JSONClass();
                JSONNode jsonLocal = new JSONClass();
                JSONNode jsonCloud = new JSONClass();
                JSONNode jsonMappings = new JSONClass();
                json.Add(localUserId.ToString(), jsonLocal);
                json.Add(cloudUserId.ToString(), jsonCloud);
                JSONNode cloudProgress = EProgressToPersistence(Start_CloudProgress);
                jsonCloud.Add("profile", cloudProgress);
                jsonCloud.Add("mappings", jsonMappings);
                jsonMappings.Add("game", cloudUserId.ToString());// "ff44dcab-94f6-4e96-98fd-44a69a575386");
                jsonMappings.Add("dna", "fake_dna_profileID_test_00");

                //response = "{\"" + localUserId.ToString() + "\":{},\"" + cloudUserId.ToString() + "\":{\"mappings\": {\"game\": \"ff44dcab-94f6-4e96-98fd-44a69a575386\",\"dna\": \"fake_dna_profileID_test_00\"}}}";
                response = json.ToString();
                break;
        }

        networkManager.SetForcedResponse(cmd, code, response);
    }

    public bool HasPassed()
    {
        PersistenceLocalDriver localDriver = PersistenceFacade.instance.LocalDriver;

        return CheckUserId(End_LocalUserId) && CheckProgress(End_LocalProgress) &&
            localDriver.Prefs_SocialImplicitMergeState == End_MergeState &&
            CheckExplicitPlatformState(End_ExplicitPlatformState) &&
            UsersManager.currentUser.SocialState == End_SocialState &&
            !PersistenceFacade.instance.Sync_IsSyncing &&
            End_SyncModeAtLaunch == SyncModeAtLaunch;
    }

    public void OnAppLaunched()
    {
        SyncModeAtLaunch = PersistenceCloudDriver.ESyncMode.None;

        DummyNetworkManager networkManager = NetworkManager.SharedInstance as DummyNetworkManager;
        if (networkManager != null)
        {
            networkManager.ResetResponses();
            networkManager.OnResponseSent = OnResponseSent;

            if (!HasAppBeenFirstLaunched)
            {
                HasAppBeenFirstLaunched = true;

                Messenger.AddListener(MessengerEvents.DEFINITIONS_LOADED, OnContentLoaded);
            }

            int localUserId = (int)Start_LocalUserId;
            string token = PlayerPrefs.GetString(GameSessionManager.KEY_ANONYMOUS_PLATFORM_USER_ID);
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("Test can't be performed because the token is missing");
            }
            else
            {
                localUserId = int.Parse(token);
            }

            string cmd = END_POINT_AUTH_B;
            string responseString = "{\"uid\":\"" + localUserId + "\"}";
            networkManager.SetForcedResponse(cmd, 200, responseString);

            cmd = END_POINT_LOGIN;
            responseString = "{ \"data\":{ \"res\":{ \"token\":\"" + token + "\",\"previousSessions\":15},\"pid\":0,\"region\":\"--\",\"sid\":15} }";
            networkManager.SetForcedResponse(cmd, 200, responseString);            
        }
    }

    public void OnContentLoaded()
    {
        Messenger.RemoveListener(MessengerEvents.DEFINITIONS_LOADED, OnContentLoaded);

        DummyNetworkManager networkManager = NetworkManager.SharedInstance as DummyNetworkManager;
        if (networkManager != null)
        {
            PrepareImplicitMerge(ImplicitMergeResponse, networkManager);

            string cmd = END_POINT_PERSISTENCE_GET;
            JSONNode json = EProgressToPersistence(Start_CloudProgress);            
            networkManager.SetForcedResponse(cmd, 200, json.ToString());            
        }
    }

    private void OnResponseSent(string endPoint)
    {
        DummyNetworkManager networkManager = NetworkManager.SharedInstance as DummyNetworkManager;
        if (networkManager != null)
        {
            if (endPoint == END_POINT_MERGE)
            {
                if (ImplicitMergeResponse2 != EImplicitMergeResponse.None)
                {
                    PrepareImplicitMerge(ImplicitMergeResponse2, networkManager);


                    if (ImplicitMergeResponse2 == EImplicitMergeResponse.OkForce)
                    {
                        string cmd = END_POINT_PERSISTENCE_GET;
                        JSONNode json = EProgressToPersistence(EProgress.Empty);
                        networkManager.SetForcedResponse(cmd, 200, json.ToString());
                    }
                }
            }
        }
    }

    private int EUserIdToUserId(EUserId userId)
    {
        return (int)userId;
    }

    private string EProgressToCurrentDragonSku(EProgress progress)
    {
        JSONNode returnValue = null;

        switch (progress)
        {
            case EProgress.Empty:
                returnValue = "dragon_baby";
                break;

            case EProgress.P1:
                returnValue = "dragon_crocodile";
                break;

            case EProgress.P2:
                returnValue = "dragon_classic";
                break;
        }

        return returnValue;
    }

    private JSONNode EProgressToPersistence(EProgress progress)
    {
        JSONNode returnValue = null;

        string currentDragonSku = EProgressToCurrentDragonSku(progress);
        switch (progress)
        {
            case EProgress.Empty:
                returnValue = PersistenceUtils.GetDefaultDataFromProfile();
                break;

            case EProgress.P1:
                returnValue = PersistenceUtils.GetDefaultDataFromProfile("", currentDragonSku, UserProfile.ESocialState.NeverLoggedIn.ToString(), 1000);
                break;

            case EProgress.P2:
                returnValue = PersistenceUtils.GetDefaultDataFromProfile("", currentDragonSku, UserProfile.ESocialState.NeverLoggedIn.ToString(), 10000);
                break;
        }

        return returnValue;
    }

    private bool CheckUserId(EUserId userId)
    {
        return GameSessionManager.SharedInstance.GetUID() == EUserIdToUserId(userId).ToString();
    }

    private bool CheckProgress(EProgress progress)
    {
        string currentDragonSku = EProgressToCurrentDragonSku(progress);
        return UsersManager.currentUser.CurrentDragon == currentDragonSku;
    }

    private bool CheckExplicitPlatformState(EExplicitPlatformState platformState)
    {
        bool returnValue = false;

        PersistenceLocalDriver localDriver = PersistenceFacade.instance.LocalDriver;
        switch (platformState)
        {
            case EExplicitPlatformState.None:
                returnValue = string.IsNullOrEmpty(localDriver.Prefs_SocialPlatformKey) && !localDriver.Prefs_SocialWasLoggedInWhenQuit;
                break;

            default:
                returnValue = localDriver.Prefs_SocialPlatformKey == EXPLICIT_PLATFORM_STRING;
                if (returnValue)
                {
                    returnValue = (platformState == EExplicitPlatformState.LoggedInWhenQuit) ? localDriver.Prefs_SocialWasLoggedInWhenQuit : !localDriver.Prefs_SocialWasLoggedInWhenQuit;
                }
                break;
        }

        return returnValue;
    }
}
#endif