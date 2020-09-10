using UnityEditor;
using UnityEngine;

public class EditorPersistenceCloudMenu : MonoBehaviour
{
    private const string CLOUD_MENU = EditorPersistenceMenu.PERSISTENCE_MENU + "/" + "Cloud";

    private const string CLOUD_PLATFORM_MENU = CLOUD_MENU + "/Platform";
    private const string CLOUD_PLATFORM_NONE = CLOUD_PLATFORM_MENU + "/None";
    private const string CLOUD_PLATFORM_FACEBOOK = CLOUD_PLATFORM_MENU + "/Facebook";
    private const string CLOUD_PLATFORM_WEIBO = CLOUD_PLATFORM_MENU + "/Weibo";
    private const string CLOUD_PLATFORM_SIWA = CLOUD_PLATFORM_MENU + "/SIWA";
    private const string CLOUD_PLATFORM_DNA = CLOUD_PLATFORM_MENU + "/DNA";

    private const string CLOUD_LOGGED_IN_WHEN_QUIT_MENU = CLOUD_MENU + "/LoggedInWhenQuit";
    private const string CLOUD_LOGGED_IN_WHEN_QUIT_TRUE = CLOUD_LOGGED_IN_WHEN_QUIT_MENU + "/True";
    private const string CLOUD_LOGGED_IN_WHEN_QUIT_FALSE = CLOUD_LOGGED_IN_WHEN_QUIT_MENU + "/False";

    private const string CLOUD_IMPLICIT_MERGE_STATE_MENU = CLOUD_MENU + "/ImplicitMergeState";
    private const string CLOUD_IMPLICIT_MERGE_STATE_NONE = CLOUD_IMPLICIT_MERGE_STATE_MENU + "/None";
    private const string CLOUD_IMPLICIT_MERGE_STATE_OK = CLOUD_IMPLICIT_MERGE_STATE_MENU + "/Ok";
    private const string CLOUD_IMPLICIT_MERGE_STATE_FAILED = CLOUD_IMPLICIT_MERGE_STATE_MENU + "/Failed";

    private static PersistenceLocalDriver s_persistenceLocalDriver = new PersistenceLocalDriver();

    #region platform
    private static SocialUtils.EPlatform Platform
    {
        get
        {            
            SocialUtils.EPlatform platformId = SocialUtils.KeyToEPlatform(s_persistenceLocalDriver.Prefs_SocialPlatformKey);
            bool isPlatformSupported = SocialPlatformManager.IsSocialPlatformIdSupportedByDevicePlatform(platformId);
            if (!isPlatformSupported)
            {
                platformId = SocialUtils.EPlatform.None;
            }

            return platformId;
        }

        set
        {
            string platformKey = (value == SocialUtils.EPlatform.None) ? "" : SocialUtils.EPlatformToKey(value);
            s_persistenceLocalDriver.Prefs_SocialPlatformKey = platformKey;

            // Call Update() to make it save prefs
            s_persistenceLocalDriver.Update(); 
        }
    }    

    [MenuItem(CLOUD_PLATFORM_NONE)]
    public static void Platform_SetNone()
    {
        Platform = SocialUtils.EPlatform.None;
    }

    [MenuItem(CLOUD_PLATFORM_NONE, true)]
    public static bool Platform_SetNoneValidate()
    {
        Menu.SetChecked(CLOUD_PLATFORM_NONE, Platform == SocialUtils.EPlatform.None);
        return true;
    }

    [MenuItem(CLOUD_PLATFORM_FACEBOOK)]
    public static void Platform_SetFacebook()
    {
        Platform = SocialUtils.EPlatform.Facebook;
    }

    [MenuItem(CLOUD_PLATFORM_FACEBOOK, true)]
    public static bool Platform_SetFacebookValidate()
    {
        Menu.SetChecked(CLOUD_PLATFORM_FACEBOOK, Platform == SocialUtils.EPlatform.Facebook);
        return true;
    }

    [MenuItem(CLOUD_PLATFORM_WEIBO)]
    public static void Platform_SetWeibo()
    {
        Platform = SocialUtils.EPlatform.Weibo;
    }

    [MenuItem(CLOUD_PLATFORM_WEIBO, true)]
    public static bool Platform_SetWeiboValidate()
    {
        Menu.SetChecked(CLOUD_PLATFORM_WEIBO, Platform == SocialUtils.EPlatform.Weibo);
        return true;
    }

    [MenuItem(CLOUD_PLATFORM_SIWA)]
    public static void Platform_SetSIWA()
    {
        Platform = SocialUtils.EPlatform.SIWA;
    }

    [MenuItem(CLOUD_PLATFORM_SIWA, true)]
    public static bool Platform_SetSIWAValidate()
    {
        Menu.SetChecked(CLOUD_PLATFORM_SIWA, Platform == SocialUtils.EPlatform.SIWA);
        return true;
    }

    [MenuItem(CLOUD_PLATFORM_DNA)]
    public static void Platform_SetDNA()
    {
        Platform = SocialUtils.EPlatform.DNA;
    }

    [MenuItem(CLOUD_PLATFORM_DNA, true)]
    public static bool Platform_SetDNAValidate()
    {
        Menu.SetChecked(CLOUD_PLATFORM_DNA, Platform == SocialUtils.EPlatform.DNA);
        return true;
    }
    #endregion

    #region loggedInWhenQuit
    private static bool LoggedInWhenQuit
    {
        get
        {
            return s_persistenceLocalDriver.Prefs_SocialWasLoggedInWhenQuit;
        }

        set
        {
            s_persistenceLocalDriver.Prefs_SocialWasLoggedInWhenQuit = value;

            // Call Update() to make it save prefs
            s_persistenceLocalDriver.Update();
        }
    }

    [MenuItem(CLOUD_LOGGED_IN_WHEN_QUIT_TRUE)]
    public static void LoggedInWhenQuit_SetTrue()
    {
        LoggedInWhenQuit = true;
    }

    [MenuItem(CLOUD_LOGGED_IN_WHEN_QUIT_TRUE, true)]
    public static bool LoggedInWhenQuit_SetTrueValidate()
    {
        Menu.SetChecked(CLOUD_LOGGED_IN_WHEN_QUIT_TRUE, LoggedInWhenQuit);
        return true;
    }

    [MenuItem(CLOUD_LOGGED_IN_WHEN_QUIT_FALSE)]
    public static void LoggedInWhenQuit_SetFalse()
    {
        LoggedInWhenQuit = false;
    }

    [MenuItem(CLOUD_LOGGED_IN_WHEN_QUIT_FALSE, true)]
    public static bool LoggedInWhenQuit_SetFalseValidate()
    {
        Menu.SetChecked(CLOUD_LOGGED_IN_WHEN_QUIT_FALSE, !LoggedInWhenQuit);
        return true;
    }
    #endregion

    #region implicitMerge
    private static PersistenceCloudDriver.EMergeState ImplicitMergeState
    {
        get
        {
            return s_persistenceLocalDriver.Prefs_SocialImplicitMergeState;
        }

        set
        {
            s_persistenceLocalDriver.Prefs_SocialImplicitMergeState = value;

            // Call Update() to make it save prefs
            s_persistenceLocalDriver.Update();
        }
    }

    [MenuItem(CLOUD_IMPLICIT_MERGE_STATE_NONE)]
    public static void ImplicitMerge_SetNone()
    {
        ImplicitMergeState = PersistenceCloudDriver.EMergeState.None;
    }

    [MenuItem(CLOUD_IMPLICIT_MERGE_STATE_NONE, true)]
    public static bool ImplicitMerge_SetNoneValidate()
    {
        Menu.SetChecked(CLOUD_IMPLICIT_MERGE_STATE_NONE, ImplicitMergeState == PersistenceCloudDriver.EMergeState.None);
        return true;
    }

    [MenuItem(CLOUD_IMPLICIT_MERGE_STATE_OK)]
    public static void ImplicitMerge_SetOk()
    {
        ImplicitMergeState = PersistenceCloudDriver.EMergeState.Ok;
    }

    [MenuItem(CLOUD_IMPLICIT_MERGE_STATE_OK, true)]
    public static bool ImplicitMerge_SetOkValidate()
    {
        Menu.SetChecked(CLOUD_IMPLICIT_MERGE_STATE_OK, ImplicitMergeState == PersistenceCloudDriver.EMergeState.Ok);
        return true;
    }

    [MenuItem(CLOUD_IMPLICIT_MERGE_STATE_FAILED)]
    public static void ImplicitMerge_SetFailed()
    {
        ImplicitMergeState = PersistenceCloudDriver.EMergeState.Failed;
    }

    [MenuItem(CLOUD_IMPLICIT_MERGE_STATE_FAILED, true)]
    public static bool ImplicitMerge_SetFailedValidate()
    {
        Menu.SetChecked(CLOUD_IMPLICIT_MERGE_STATE_FAILED, ImplicitMergeState == PersistenceCloudDriver.EMergeState.Failed);
        return true;
    }
    #endregion
}