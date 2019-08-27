using System.Collections;
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

	public static bool IsDeviceTablet( float width, float height, float dpi )
    {
        float screenWidth = width / dpi;
        float screenHeight = height / dpi;
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
		
    	Debug.Log("Open store url " + url);
		
		if (!string.IsNullOrEmpty (url)) 
		{
			Application.OpenURL(url);
		}
    }
    /// <summary>
    /// Translates from standard iso name into Calety iso name
    /// </summary>
    /// <param name="iso"> Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
    /// <returns>Calety iso name: "en", "en", "es", "pt", "zh_cn", etc</returns>
    public static string StandardISOToCaletyISO(string iso)
    {
        switch (iso)
        {
            /*
            case "zh-CN":
                iso = "zh_cn";
                break;

            case "zh-TW":
                iso = "zh_tw";
                break;
            */
            default:
                string[] tokens = iso.Split('-');
                if (tokens.Length >= 1)
                {
                    iso = tokens[0];
                }
                break;
        }

        return iso;
    }
}
