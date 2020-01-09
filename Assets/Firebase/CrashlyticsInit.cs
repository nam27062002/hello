using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Crashlytics;

public static class CrashlyticsInit {

    public static bool isInitialized
    {
        get; private set;
    }

	// Use this for initialization
	public static void initialise() {
        isInitialized = false;

#if UNITY_ANDROID
        int androidAPILevel = PlatformUtilsAndroidImpl.GetSDKLevel();
        if (androidAPILevel < 21)  //
        {
            Debug.Log("API level " + androidAPILevel + " detected. Avoiding Crashlytics init.");
            return;
        }
#endif


        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                //   app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
                isInitialized = true;
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    public static void OnCrashlyticsLogException()
    {

        try
        {
            throw new Exception("Firebase Crashlytics Test new custom exception!!!!");
        }
        catch (Exception e)
        {
            Crashlytics.LogException(e);
        }

        Debug.Log("Firebase Crashlytics Test new custom exception!!!!");
    }

    public static void OnCrashlyticsLog()
    {
        Crashlytics.Log("Firebase Crashlytics Test log!!!!!!!!!");
        Debug.Log("Firebase Crashlytics Test log!!!!!!!!!");
    }


}
