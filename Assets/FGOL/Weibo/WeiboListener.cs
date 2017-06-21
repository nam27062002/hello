using UnityEngine;
using System.Collections.Generic;
using FGOL.ThirdParty.MiniJSON;

public class WeiboListener : MonoBehaviour
{
    //we just get strings back from the platform
    public void OnLoginCompleteSuccess(string tokenDetails)
    {
        Debug.Log("Weibo :: Login Success with details: " + tokenDetails);
        Dictionary<string, object> deserialised = Json.Deserialize(tokenDetails) as Dictionary<string, object>;
		string refreshToken = null;
#if !UNITY_EDITOR
		string tokenID = deserialised["token"] as string;
        string expiry = deserialised["expiry"] as string;
        string uid = deserialised["uid"] as string;
		if (deserialised.ContainsKey("refreshToken"))
        {
            refreshToken = deserialised["refreshToken"] as string;
        }
#else
		//for editor only
		string tokenID = deserialised["access_token"] as string;
		int expiryTime = (int)Globals.GetUnixTimestamp() + System.Convert.ToInt32(deserialised["expires_in"]);
		string expiry = expiryTime.ToString();
        string uid = deserialised["uid"] as string;
#endif
		WeiboSocialInterface.WeiboTokenSave.SaveTokenDetails(tokenID, refreshToken, uid, expiry);
        WeiboSocialInterface.WeiboDelegate.OnLoginComplete(true);
        WeiboSocialInterface.WeiboDelegate.OnLoginComplete = null;
    }

    public void OnLoginCompleteFailed(string error)
    {
        Debug.Log("Weibo :: Login Failed with Error: " + error);
        WeiboSocialInterface.WeiboDelegate.OnLoginComplete(false);
        WeiboSocialInterface.WeiboDelegate.OnLoginComplete = null;
    }
}
