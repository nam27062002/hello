//#define SKIP_DEFINES	// Uncomment for editing

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_ANDROID || SKIP_DEFINES
public class PlatformUtilsAndroidImpl: PlatformUtils
{
	private string AndroidGetCountryCode()
	{
		string result = "";
        try
        {
            AndroidJavaClass localeClass = new AndroidJavaClass("java.util.Locale");
            AndroidJavaObject locale = localeClass.CallStatic<AndroidJavaObject>("getDefault");
            result = locale.Call<string>("getCountry");
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidGetCountryCode LOCALE:" + e.Message);
        }
		Debug.Log("AndroidGetCountryCode () returned : " + result);
		return result != null? result: "";
	}

	public override string GetCountryCode()
	{
		if (Application.platform == RuntimePlatform.Android )
			return AndroidGetCountryCode();
		return "US";
	}
	
	public override void GetTokens(){}

	public override string FormatPrice( float price, string currencyLocale )
	{
		//TODO:
		return "";
	}

	public override void ShareImage(string filename, string caption)
	{
		Debug.Log ("Trying to share " + filename);
#if !UNITY_EDITOR || SKIP_DEFINES
		// [AOC] Updated version, supports multiple file sharing
		using(AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
		using(AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent")) {
			// Aux vars
			AndroidJavaObject currentActivity = GetCurrentActivity();

			// Initialize intent parameters
			intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
			//intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), "");	// Not using subject field for now
			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), caption);

			// Instantiate the object Uri class pointing to the file's path
			AndroidJavaObject fileObject = new AndroidJavaObject("java.io.File", filename);
			AndroidJavaClass fileProviderClass = new AndroidJavaClass("androidx.core.content.FileProvider");
			AndroidJavaObject uriObject = fileProviderClass.CallStatic<AndroidJavaObject>("getUriForFile", currentActivity, Application.identifier + ".provider", fileObject);

			// attach file
			intentObject.Call<AndroidJavaObject>("setType", "image/png");
			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
			intentObject.Call<AndroidJavaObject>("addFlags", intentClass.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION"));

			// finally start application with our intent
			// [AOC] Create custom chooser intent to avoid showing the "Always" and "Only Once" button
			bool customChooser = true;
			if(customChooser) {
				AndroidJavaObject chooserObject = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, LocalizationManager.SharedInstance.Localize("TID_GEN_SHARE_WITH"));
				currentActivity.Call("startActivity", chooserObject);
			} else {
				currentActivity.Call("startActivity", intentObject);
			}
		}
#endif
	}

	public override void MakeToast(string text, bool longDuration)
	{
#if !UNITY_EDITOR || SKIP_DEFINES
		AndroidJavaClass toastInterfaceClass = new AndroidJavaClass("com.ubisoft.utils.ToastInterface");
		toastInterfaceClass.CallStatic("makeToast", GetCurrentActivity(), text, longDuration);
#endif
	}

#if !UNITY_EDITOR || SKIP_DEFINES
	private AndroidJavaObject GetCurrentActivity()
	{
		//instantiate the class UnityPlayer
		AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		
		//instantiate the object currentActivity
		AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

		return currentActivity;
	}
	#endif

	public override string GetTrackingId()
	{
#if !UNITY_EDITOR || SKIP_DEFINES
		AndroidJavaClass up = new AndroidJavaClass  ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject> ("currentActivity");
		AndroidJavaClass client = new AndroidJavaClass ("com.ubisoft.utils.PlatformUtils");
		string adInfo = client.CallStatic<string> ("getTrackingId",currentActivity);

		return adInfo;
		#else
		return "Using editor";
		#endif
	}

	public override void askPermissions(){
		//string[] permissions = "android.permission.WRITE_EXTERNAL_STORAGE".Split(new char[]{' '});
#if !UNITY_EDITOR || SKIP_DEFINES
		AndroidJavaClass up = new AndroidJavaClass  ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject> ("currentActivity");
		AndroidJavaClass client = new AndroidJavaClass ("com.ubisoft.utils.PlatformUtils");
		client.CallStatic ("requestPermissions",currentActivity);

#endif
	}

	public override bool arePermissionsGranted(){
#if !UNITY_EDITOR || SKIP_DEFINES
		AndroidJavaClass up = new AndroidJavaClass  ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject> ("currentActivity");
		AndroidJavaClass client = new AndroidJavaClass ("com.ubisoft.utils.PlatformUtils");
		return client.CallStatic<bool> ("arePermissionsGranted",currentActivity);
#else
		return true;
#endif
	}
	
	public static int GetSDKLevel()
	{
		int _sdkLevel = 18;
#if (UNITY_ANDROID && !UNITY_EDITOR) || SKIP_DEFINES
		IntPtr _class = AndroidJNI.FindClass("android/os/Build$VERSION");
		IntPtr _fieldId = AndroidJNI.GetStaticFieldID( _class, "SDK_INT", "I");
		_sdkLevel = AndroidJNI.GetStaticIntField( _class, _fieldId);
#endif
		return _sdkLevel;
	}
	
	public override long getFreeDataSpaceAvailable(  )
	{ 
		if ( Application.platform == RuntimePlatform.Android )
		{
			using (AndroidJavaObject statFs = new AndroidJavaObject( "android.os.StatFs", Application.persistentDataPath))
			{
				if ( GetSDKLevel() < 18 )	// If minor that 4.3 use old code
				{
					long bytesAvailable = ((long)statFs.Call<int>("getBlockSize")) * ((long)statFs.Call<int>("getAvailableBlocks"));
					return bytesAvailable;
				}
				else
				{
					return statFs.Call<long>("getAvailableBytes");
				}
			}
		}
		else
		{
			return 1 * 1024 * 1024 * 1024;	// 1Gb

			// Find Drive Info
/*
			DriveInfo[] allDrives = DriveInfo.GetDrives();
			string rootDir = Directory.GetDirectoryRoot( Application.persistentDataPath );
			foreach( DriveInfo di in allDrives)
			{
				if ( di.RootDirectory.Name.Equals( rootDir) )
				{
					return di.AvailableFreeSpace;
				}
			}
*/		}

		return 0;
	}


    override public bool ApplicationExists(String applicationURI)
    {
        bool ret = false;
#if !UNITY_EDITOR || SKIP_DEFINES
        try
        {
            AndroidJavaObject currentActivity = GetCurrentActivity();
            AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            try
            {
                object[] args = new object[] { applicationURI, 1 };
                AndroidJavaObject packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", args);
                ret = true;
            }
            catch (Exception e)
            {
                Debug.Log("Exception getPackageInfo: " + e.Message);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Exception ApplicationExists: " + e.Message);
        }
#endif
        return ret;
    }
}
#endif
