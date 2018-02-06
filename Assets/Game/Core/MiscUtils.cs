﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiscUtils {

	const string SUPPORT_EMAIL_ADDRESS = "hungrydragon-support@ubisoft.com";
	const string SUPPORT_EMAIL_SUBJECT = "Hungry Dragon Feedback";

	public static void SendFeedbackEmail(){
		// Open feedback link/email
		string version = GameSettings.internalVersion.ToString() + "("+ ServerManager.SharedInstance.GetRevisionVersion() +")";
		string track = HDTrackingManager.Instance.GetDNAProfileID();
		if ( !string.IsNullOrEmpty(track) )
		{
			version += " (" + track + ")";
		}
		string subject = SUPPORT_EMAIL_SUBJECT + " " + version;
        string mailSubject = WWW.EscapeURL(subject).Replace("+", "%20");

		Application.OpenURL("mailto:" + SUPPORT_EMAIL_ADDRESS + "?subject=" + mailSubject);// + "&body=" + body);
	}

	public static bool IsDeviceTablet()
    {
        float screenWidth = Screen.width / Screen.dpi;
        float screenHeight = Screen.height / Screen.dpi;
        float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));
        return diagonalInches > 6.5f;
    }

    public static void OpenAppInStore(string appId)
    {
		string url = null;
#if UNITY_IOS        
	    Application.OpenURL("itms-apps://itunes.apple.com/app/id" + appId);
#elif UNITY_ANDROID
        Application.OpenURL("market://details?id=" + appId);
#endif

		if (FeatureSettingsManager.IsDebugEnabled)
			Debug.Log("Open store url " + url);
		
		if (!string.IsNullOrEmpty (url)) 
		{
			Application.OpenURL(url);
		}
    }    
}
