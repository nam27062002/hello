#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

public class PersistenceTester
{        
    private static Dictionary<int, PersistenceTest> sm_tests;

    private static void InitTests()
    {
        if (sm_tests == null)
        {
            sm_tests = new Dictionary<int, PersistenceTest>();

            PersistenceTest test;
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.Empty, PersistenceCloudDriver.EMergeState.None,
                PersistenceTest.EExplicitPlatformState.None, PersistenceTest.EUserId.None, PersistenceTest.EProgress.Empty, PersistenceTest.EImplicitMergeResponse.Error, PersistenceTest.EImplicitMergeResponse.None,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.Empty, PersistenceCloudDriver.EMergeState.None, PersistenceTest.EExplicitPlatformState.None, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Full);
            sm_tests.Add(1, test);

            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.Empty, PersistenceCloudDriver.EMergeState.None,
                PersistenceTest.EExplicitPlatformState.None, PersistenceTest.EUserId.None, PersistenceTest.EProgress.Empty, PersistenceTest.EImplicitMergeResponse.Ok, PersistenceTest.EImplicitMergeResponse.None,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.Empty, PersistenceCloudDriver.EMergeState.Ok, PersistenceTest.EExplicitPlatformState.None, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Full);
            sm_tests.Add(2, test);

            // Use Case: 3. Implicit Conflict after reinstalling the game / Restore
            // Result: Prompts Merge conflict popup
            // User Action: Hit 'Restore' button
            // Extra: Test disconnecting network before hitting "Restore" button
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.Empty, PersistenceCloudDriver.EMergeState.None,
                PersistenceTest.EExplicitPlatformState.None, PersistenceTest.EUserId.U1, PersistenceTest.EProgress.P1, PersistenceTest.EImplicitMergeResponse.Conflict, PersistenceTest.EImplicitMergeResponse.None,
                PersistenceTest.EUserId.U1, PersistenceTest.EProgress.P1, PersistenceCloudDriver.EMergeState.Ok, PersistenceTest.EExplicitPlatformState.None, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Lite);
            sm_tests.Add(3, test);

            // Use Case: 4. Implicit Conflict after reinstalling the game / Keep (Ok) 
            // Result: Prompts Merge conflict popup
            // User Action: Hit 'Keep' button
            // Extra: Test disconnecting network before hitting "Keep" button
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.Empty, PersistenceCloudDriver.EMergeState.None,
                PersistenceTest.EExplicitPlatformState.None, PersistenceTest.EUserId.U1, PersistenceTest.EProgress.P1, PersistenceTest.EImplicitMergeResponse.Conflict, PersistenceTest.EImplicitMergeResponse.OkForce,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.Empty, PersistenceCloudDriver.EMergeState.Ok, PersistenceTest.EExplicitPlatformState.None, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Full);
            sm_tests.Add(4, test);

            // Use Case: 5. Implicit Conflict after reinstalling the game / Keep (Error) / Confirm
            // Result: Prompts Merge conflict popup
            // User Action: Hit 'Keep' button
            // Result: Prompt Error when keeping local progress. Do you want to continue?
            // User Action: Hit 'Ok' button
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.Empty, PersistenceCloudDriver.EMergeState.None,
                PersistenceTest.EExplicitPlatformState.None, PersistenceTest.EUserId.U1, PersistenceTest.EProgress.P1, PersistenceTest.EImplicitMergeResponse.Conflict, PersistenceTest.EImplicitMergeResponse.Error,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.Empty, PersistenceCloudDriver.EMergeState.Failed, PersistenceTest.EExplicitPlatformState.None, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Full);
            sm_tests.Add(5, test);

            // Use Case: 6. Implicit login has already been done successfully and local persistence is ahead
            // Result: Persistences are synchronised without calling merge/c            
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Ok,
                PersistenceTest.EExplicitPlatformState.None, PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceTest.EImplicitMergeResponse.None, PersistenceTest.EImplicitMergeResponse.None,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Ok, PersistenceTest.EExplicitPlatformState.None, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Lite);
            sm_tests.Add(6, test);
            
            // Use Case: 7. Implicit login has already been done successfully and cloud persistence is ahead
            // Result: Persistences are synchronised without calling merge/c. Prompt Sync persistence popup
            // User Action: Hit 'Local' button
            // Result: Game continues
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Ok,
                PersistenceTest.EExplicitPlatformState.None, PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P1, PersistenceTest.EImplicitMergeResponse.None, PersistenceTest.EImplicitMergeResponse.None,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Ok, PersistenceTest.EExplicitPlatformState.None, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Lite);
            sm_tests.Add(7, test);

            // Use Case: 8. Implicit login has already been done successfully and cloud persistence is ahead
            // Result: Persistences are synchronised without calling merge/c. Prompt Sync persistence popup
            // User Action: Hit 'Cloud' button
            // Result: Game is restarted
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Ok,
                PersistenceTest.EExplicitPlatformState.None, PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P1, PersistenceTest.EImplicitMergeResponse.None, PersistenceTest.EImplicitMergeResponse.None,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P1, PersistenceCloudDriver.EMergeState.Ok, PersistenceTest.EExplicitPlatformState.None, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Lite);
            sm_tests.Add(8, test);

            // Use Case: 9. Implicit login has already been done successfully and persistences are synchronised
            // Result: Game loads without friction
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Ok,
                PersistenceTest.EExplicitPlatformState.None, PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceTest.EImplicitMergeResponse.None, PersistenceTest.EImplicitMergeResponse.None,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Ok, PersistenceTest.EExplicitPlatformState.None, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Lite);
            sm_tests.Add(9, test);

            // Use Case: 10. No previous explicit login. Implicit login has failed
            // Result: No sync and cloud save disabled            
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Failed,
                PersistenceTest.EExplicitPlatformState.None, PersistenceTest.EUserId.U1, PersistenceTest.EProgress.P1, PersistenceTest.EImplicitMergeResponse.None, PersistenceTest.EImplicitMergeResponse.None,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Failed, PersistenceTest.EExplicitPlatformState.None, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.None);
            sm_tests.Add(10, test);

            // Use Case: 11. Implicit login hasn't been done and the user is explicitly logged in
            // User Action: Wait until the game logs in to the DNA silently
            // Result: Logged in to SIWA and cloud save synced            
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.None,
                PersistenceTest.EExplicitPlatformState.LoggedInWhenQuit, PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceTest.EImplicitMergeResponse.Ok, PersistenceTest.EImplicitMergeResponse.None,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Ok, PersistenceTest.EExplicitPlatformState.LoggedInWhenQuit, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Full);
            sm_tests.Add(11, test);

            // Use Case: 12. Implicit login hasn't been done and the user has been explicitly logged in but she's not anymmore
            // Result: Not logged in to SIWA and cloud save synced (because of DNA)   
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.None,
                PersistenceTest.EExplicitPlatformState.LoggedOutWhenQuit, PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceTest.EImplicitMergeResponse.Ok, PersistenceTest.EImplicitMergeResponse.None,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Ok, PersistenceTest.EExplicitPlatformState.LoggedOutWhenQuit, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Full);
            sm_tests.Add(12, test);

            // Use Case: 13. The user is explicitly logged in. Implicit login hasn't been done and when it's done there's a conflict
            // Result: No conflict is show, it's marked as failed and cloud save enabled because of the explicit platform          
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.None,
                PersistenceTest.EExplicitPlatformState.LoggedInWhenQuit, PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceTest.EImplicitMergeResponse.Conflict, PersistenceTest.EImplicitMergeResponse.None,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Failed, PersistenceTest.EExplicitPlatformState.LoggedInWhenQuit, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Full);
            sm_tests.Add(13, test);

            // Use Case: 14. Implicit login hasn't been done and the user has been explicitly logged in but she's not anymmore. Implicit login causes a conflict
            // Result: Not logged in to SIWA. Cloud save disabled        
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.None,
                PersistenceTest.EExplicitPlatformState.LoggedOutWhenQuit, PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceTest.EImplicitMergeResponse.Conflict, PersistenceTest.EImplicitMergeResponse.None,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Failed, PersistenceTest.EExplicitPlatformState.LoggedOutWhenQuit, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Full);
            sm_tests.Add(14, test);

            // Use Case: 15. Implicit login has been done and the user has been explicitly logged in but she's not anymmore. 
            // Result: Not logged in to SIWA. Cloud save enabled        
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Ok,
                PersistenceTest.EExplicitPlatformState.LoggedOutWhenQuit, PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceTest.EImplicitMergeResponse.None, PersistenceTest.EImplicitMergeResponse.None,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Ok, PersistenceTest.EExplicitPlatformState.LoggedOutWhenQuit, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Lite);
            sm_tests.Add(15, test);

            // Use Case: 16. Corrupt local progress. User wasn't not explicitly logged in last time she played
            // Result: Prompts popup notifying local progress was recovered from corruption. Game loads. Not logged in to SIWA
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.PCORRUPTED, PersistenceCloudDriver.EMergeState.Ok,
                PersistenceTest.EExplicitPlatformState.LoggedOutWhenQuit, PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceTest.EImplicitMergeResponse.None, PersistenceTest.EImplicitMergeResponse.None,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Ok, PersistenceTest.EExplicitPlatformState.LoggedOutWhenQuit, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Lite);
            sm_tests.Add(16, test);

            // Use Case: 17. Corrupt local progress. User was explicitly logged in last time she played
            // Result: Prompts popup notifying local progress was recovered from corruption. Game loads. Logged in to SIWA
            test = new PersistenceTest(PersistenceTest.EUserId.U2, PersistenceTest.EProgress.PCORRUPTED, PersistenceCloudDriver.EMergeState.Ok,
                PersistenceTest.EExplicitPlatformState.LoggedInWhenQuit, PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceTest.EImplicitMergeResponse.None, PersistenceTest.EImplicitMergeResponse.None,
                PersistenceTest.EUserId.U2, PersistenceTest.EProgress.P2, PersistenceCloudDriver.EMergeState.Ok, PersistenceTest.EExplicitPlatformState.LoggedInWhenQuit, UserProfile.ESocialState.NeverLoggedIn,
                PersistenceCloudDriver.ESyncMode.Full);
            sm_tests.Add(17, test);
        }
    }

    private const string PREF_CURRENT_TEST_ID = "PersistenceTester_CurrentTestId";    

    private PersistenceTest CurrentTest
    {
        get
        {
            return sm_tests.ContainsKey(CurrentTestId) ? sm_tests[CurrentTestId] : null;            
        }
    }

    public int CurrentTestId
    {
        get
        {
            return PlayerPrefs.GetInt(PREF_CURRENT_TEST_ID, 0);
        }

        set
        {
            PlayerPrefs.SetInt(PREF_CURRENT_TEST_ID, value);
            PlayerPrefs.Save();
        }
    }

    public PersistenceTester()
    {
        InitTests();

        PersistenceTest currentTest = CurrentTest;
        if (currentTest != null)
        {
            NetworkManager.DUMMY = true;
            DNASocialPlatformManager.DUMMY = true;
        }
    }

    public void BeginTest(int testId)
	{
        if (testId == 0)
        {
            CurrentTestId = testId;
        }
        else if (sm_tests.ContainsKey(testId))
        {
            PersistenceTest test = sm_tests[testId];

            // Prepare the test
            test.PrepareLocal();

            CurrentTestId = testId;

            // Restart the game
            if (Application.isPlaying)
            {
                ApplicationManager.instance.NeedsToRestartFlow = true;
            }
        }
        else
        {
            Debug.LogError("No test found for id " + testId);
        }
	}

    public bool HasCurrentTestPassed()
    {
        return IsATestRunning() && CurrentTest.HasPassed();
    }

    private bool IsATestRunning()
    {
        return CurrentTest != null;
    }    

    public void OnAppLaunched()
	{        
        PersistenceTest currentTest = CurrentTest;
        if (currentTest != null)
        {
            currentTest.OnAppLaunched();
        }
    }

    public void OnSyncModeAtLaunch(PersistenceCloudDriver.ESyncMode mode)
    {
        PersistenceTest currentTest = CurrentTest;
        if (currentTest != null)
        {
            currentTest.SyncModeAtLaunch = mode;
        }
    }
}
#endif
