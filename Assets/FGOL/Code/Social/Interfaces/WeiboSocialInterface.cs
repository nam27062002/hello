using BestHTTP;
using FGOL.Server;
using FGOL.ThirdParty.MiniJSON;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

class WeiboRequest : Request
{
    public void Get(string url, Method method, Dictionary<string, string> parameters, byte[] data, Action<Error, byte[]> callback)
    {
        Run(url, method, null, parameters, data, delegate (Error error, HTTPResponse response)
        {
            if (error == null)
            {
                if (response != null)
                {
                    switch (response.StatusCode)
                    {
                        case 200:
                            callback(null, response.Data);
                            break;
                        default:
                            callback(new UnknownError(string.Format("Failed to download with status code {0} from url: {1}", response.StatusCode, url)), null);
                            break;
                    }
                }
                else
                {
                    callback(new UnknownError("Failed to download from url: " + url), null);
                }
            }
            else
            {
                callback(error, null);
            }
        });
    }
}


public class WeiboDelegate
{
    public Action<bool> OnLoginComplete;
}

public class WeiboSocialInterface : ISocialInterface
{
	private static string HSW_KEY = "2041231630";
	private static string HSW_SECRET = "a5b4bdfbf0de2a76d9393dce6eb2655c";
	private static string HSW_REDIRECT = "https://hsx-auth-geo.fgol.mobi/";

	public static WeiboDelegate WeiboDelegate;
    public static WeiboTokenSaver WeiboTokenSave = null;
    private Weibo.Unity.IWeibo m_interface = null;
    private GameObject m_weiboGameObject = null;
    // [DGR] No support added yet
    //private WeiboListener m_weiboListener = null;

    private string m_userDisplayName = "Me";

    public void Init()
    {
        Debug.Log("WeiboLogin init");
#if UNITY_ANDROID && !UNITY_EDITOR
        m_interface = new Weibo.Unity.WeiboAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
        m_interface = new Weibo.Unity.WeiboIOS();
#else
		m_interface = new Weibo.Unity.WeiboDummy();
#endif

        WeiboTokenSave = new WeiboTokenSaver();
        if (m_weiboGameObject == null)
        {
            WeiboDelegate = new WeiboDelegate();
            m_weiboGameObject = new GameObject("WeiboGameObject");
            // [DGR] No support added yet
            //m_weiboListener = m_weiboGameObject.AddComponent<WeiboListener>();
            GameObject.DontDestroyOnLoad(m_weiboGameObject);
        }
        m_interface.Init(HSW_KEY, HSW_SECRET, HSW_REDIRECT);
    }


    public void AppActivation()
    {
        //do something I guess?
    }

    public void Login(PermissionType[] permissions, Action<bool> onLogin)
    {
        Debug.Log("WeiboLogin Login");
        WeiboDelegate.OnLoginComplete = onLogin;
        m_interface.Login();
    }

    public bool IsLoggedIn()
    {
        if (WeiboTokenSave != null)
        {
            return WeiboTokenSave.Token != null;
        }
        else
        {
            Debug.LogError("WeiboTokenSave Not Initialised");
            return false;
        }
    }

    public string GetAccessToken()
    {
        return WeiboTokenSave.Token;
    }

    public string GetSocialID()
    {
        return WeiboTokenSave.UserID;
    }

    public void RefreshAuthentication(Action<bool> onRefresh)
    {
        Debug.Log("WeiboSocialInterface RefreshAuthentication");
        if (WeiboTokenSave.RefreshToken != null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["client_id"] = HSW_KEY;
            parameters["client_secret"] = HSW_SECRET;
            parameters["grant_type"] = "refresh_token";
            parameters["redirect_uri"] = HSW_REDIRECT;
            parameters["refresh_token"] = WeiboTokenSave.RefreshToken;

            WeiboRequest refreshAuthReq = new WeiboRequest();
            refreshAuthReq.Get("https://api.weibo.com/oauth2/access_token", Request.Method.POST, parameters, null, delegate (FGOL.Server.Error error, byte[] response)
            {
                if (error == null)
                {
                    string responseString = Encoding.UTF8.GetString(response, 0, response.Length);
                    Debug.Log("Response from Weibo Token Refresh: " + responseString);
                    Dictionary<string, object> result = Json.Deserialize(responseString) as Dictionary<string, object>;
                    if (result != null && result.ContainsKey("access_token"))
                    {
                        string token = result["access_token"] as string;
                        string userID = result["uid"] as string;

                        string tokenExpiry = null;
                        try
                        {
                            int tokenExpiryInt = Convert.ToInt32(result["expires_in"]);
                            tokenExpiry = tokenExpiryInt.ToString();
                        }
                        catch (Exception)
                        {
                            Debug.LogError("Couldn't parse Weibo token expiry time. Not an int32.");
                        }

                        string tokenRefresh = null;
                        if (result.ContainsKey("refresh_token"))
                        {
                            tokenRefresh = result["refresh_token"] as string;
                        }
                        WeiboTokenSave.SaveTokenDetails(token, tokenRefresh, userID, tokenExpiry);
                        onRefresh(true);
                    }
                    else if (result == null)
                    {
                        Debug.Log("Weibo Token Refresh result was not valid json: " + responseString);
                        onRefresh(false);
                    }
                    else
                    {
                        Debug.Log("Weibo Token Refresh failed with response: " + responseString);
                        onRefresh(false);
                    }
                }
                else
                {
                    Debug.Log("Weibo Token Refresh Failed " + error.message);
                    onRefresh(false);
                }

            });
        }
        else
        {
            Debug.Log("Weibo has no token for auth refresh. Fail only if current token is expired");
			if (WeiboTokenSave.TokenExpiry != null)
			{
				try
				{
					int tokenExpiry = System.Convert.ToInt32(WeiboTokenSave.TokenExpiry);
					if (tokenExpiry > Globals.GetUnixTimestamp())
					{

						Debug.Log("Weibo cannot refresh token. Current token is still valid.");
						//token expiry null, assume expired
						onRefresh(true);
					}
					else
					{
						Debug.LogError("Weibo cannot refresh token. Current token is also expired. AuthFail.");
						onRefresh(false);
					}
				}
				catch (Exception)
				{
					Debug.LogError("Weibo cannot refresh token. Current token expiry is not an int? AuthFail");

					//token expiry not an int?
					onRefresh(false);
				}
			}
			else
			{
				Debug.LogError("Weibo cannot refresh token. Current token expiry is not set. AuthFail.");
				//token expiry null, assume expired
				onRefresh(false);
			}
        }
    }

    public PermissionType[] GetGrantedPermissions()
    {
        List<PermissionType> grantedPermissions = new List<PermissionType>();
        grantedPermissions.Add(PermissionType.Basic);
        grantedPermissions.Add(PermissionType.Friends);
        grantedPermissions.Add(PermissionType.Publish);
        return grantedPermissions.ToArray();
    }

    public void LogOut()
    {
        if (!IsLoggedIn())
        {
            Debug.LogWarning("WeiboSocialInterface: Already Logged Out");
        }
        else
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["access_token"] = GetAccessToken();

            WeiboRequest friendsInfo = new WeiboRequest();
            friendsInfo.Get("https://api.weibo.com/oauth2/revokeoauth2", Request.Method.POST, parameters, null, delegate (FGOL.Server.Error error, byte[] response)
            {
                if (error == null)
                {
                    Debug.Log("WeiboSocialInterface: Successfully logged out");
                }
                else
                {
                    Debug.Log("WeiboSocialInterface: Failed to log out: " + error.message);
                }
                //we still delete the details anyway, lol
                WeiboTokenSave.DeleteTokenDetails();
            });
        }
    }

    public void GetProfileInfo(Action<Dictionary<string, string>> onGetProfileInfo)
    {
        if (IsLoggedIn())
        {
            Dictionary<string, string> results = null;
            string url = string.Format("https://api.weibo.com/2/users/show.json?uid={0}&source={1}&access_token={2}", GetSocialID(), HSW_KEY, GetAccessToken());
            WeiboRequest friendsInfo = new WeiboRequest();
            friendsInfo.Get(url, Request.Method.GET, null, null, delegate (FGOL.Server.Error error, byte[] response)
            {
                if (error == null)
                {
                    string responseString = Encoding.UTF8.GetString(response, 0, response.Length);
                    Debug.Log("Weibo :: OnProfileRequestComplete Success with Data: " + responseString);
                    Dictionary<string, object> deserialised = Json.Deserialize(responseString) as Dictionary<string, object>;
                    if (deserialised.ContainsKey("name"))
                    {
                        results = new Dictionary<string, string>();

						//	Cache name so we don't have to query server every time
						m_userDisplayName = deserialised["name"] as string;
						results["name"] = m_userDisplayName;
						results["id"] = deserialised["idstr"] as string;
                    }
                    else
                    {
                        if (deserialised.ContainsKey("error"))
                        {
                            string jsonError = deserialised["error"] as string;
                            Debug.LogError("Weibo :: OnProfileRequestComplete Failed: " + jsonError);
                        }
                        else
                        {
                            Debug.LogError("Weibo :: OnProfileRequestComplete Failed: " + responseString);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Weibo :: OnProfileRequestComplete Failed url: " + url + " Error:" + error.message);
                }
                onGetProfileInfo(results);
            });
        }
        else
        {
            onGetProfileInfo(null);
        }
    }

    public void InviteFriends(string title, string message, string appUrl, string imageUrl, Action<string, string[]> onFriendRequest)
    {
        //this will not work on weibo without a lot of work and UI
        onFriendRequest(null, null);
    }

    public void GetFriends(Action<Dictionary<string, string>> onGetFriends)
    {
        if (IsLoggedIn())
        {
            string url = string.Format("https://api.weibo.com/2/friendships/friends.json?uid={0}&source={1}&access_token={2}", GetSocialID(), HSW_KEY, GetAccessToken());
            WeiboRequest friendsInfo = new WeiboRequest();
            friendsInfo.Get(url, Request.Method.GET, null, null, delegate (FGOL.Server.Error error, byte[] response)
            {
                if (error == null)
                {
                    string responseString = Encoding.UTF8.GetString(response, 0, response.Length);

                    Debug.Log("Weibo :: Friends Request Returned: " + responseString);
                    Dictionary<string, object> deserialised = Json.Deserialize(responseString) as Dictionary<string, object>;
                    if (deserialised.ContainsKey("users"))
                    {
                        Dictionary<string, string> userData = new Dictionary<string, string>();
                        List<object> friends = deserialised["users"] as List<object>;
                        foreach (object fr in friends)
                        {
                            Dictionary<string, object> friendDict = fr as Dictionary<string, object>;
                            if (friendDict != null && friendDict.ContainsKey("id") && friendDict.ContainsKey("name"))
                            {
                                string friendID = friendDict["idstr"] as string;
                                string friendName = friendDict["name"] as string;
                                userData.Add(friendID, friendName);
                            }
                            else
                            {
                                Debug.LogWarning("Weibo :: OnFriendsRequestComplete data had missing data: " + responseString);
                            }
                        }

                        onGetFriends(userData);
                    }
                    else
                    {
                        Debug.LogError("Weibo :: OnFriendsRequestComplete data doesn't contain ssers: "+ responseString);
                        onGetFriends(null);
                    }
                }
                else
                {
                    Debug.LogError("Weibo :: OnFriendsRequestComplete error " + error.message);
                    onGetFriends(null);
                }
            });
        }
        else
        {
            onGetFriends(null);
        }
    }


    public void GetProfilePicture(string socialID, Action<Texture2D> onGetProfilePicture, Action<string, Texture2D, Action<Texture2D>> callback, int width = 256, int height = 256)
    {
        if (IsLoggedIn())
        {
            string url = string.Format("https://api.weibo.com/2/users/show.json?uid={0}&source={1}&access_token={2}", socialID, HSW_KEY, GetAccessToken());
            WeiboRequest imageInfo = new WeiboRequest();
            imageInfo.Get(url, Request.Method.GET, null, null, delegate (FGOL.Server.Error error, byte[] response)
            {
                if (error == null)
                {
                    string responseString = Encoding.UTF8.GetString(response, 0, response.Length);
                    Debug.Log("Weibo :: OnProfileRequestComplete Success with Data: " + responseString);
                    Dictionary<string, object> deserialised = Json.Deserialize(responseString) as Dictionary<string, object>;
                    if (deserialised != null)
                    {
                        if (deserialised.ContainsKey("avatar_large"))
                        {
                            string imageUrl = deserialised["avatar_large"] as string;
                            Debug.Log("Weibo :: OnProfileRequestComplete, URL is: " + imageUrl);
                            WeiboRequest imageRequest = new WeiboRequest();
                            imageRequest.Get(imageUrl, Request.Method.GET, null, null, delegate (FGOL.Server.Error imageReqError, byte[] imageReqResponse)
                            {
                                if (error == null)
                                {
                                    Debug.Log("Weibo :: OnProfileRequestComplete, URL is: " + imageUrl);

									//	Weibo can serve gifs, so first try like that
									Texture2D texture = null;
									texture = GifTextureFactory.CreateTexture(imageReqResponse);
									if (texture == null)
									{
                                    	texture = new UnityEngine.Texture2D(0, 0, UnityEngine.TextureFormat.ARGB32, false);
                                    	texture.LoadImage(imageReqResponse);
									}

                                    callback(socialID, texture, onGetProfilePicture);
                                }
                                else
                                {
                                    Debug.Log("Weibo :: Image Couldn't be downloaded, URL: " + imageUrl + " Error: " + imageReqError.message);
									// no need to fire the callback because there is nothing to cache.
									onGetProfilePicture(null);
                                }
                            });
                        }
                        else
                        {
                            Debug.LogError("Weibo :: (GetProfilePicture) No Large Image in response: " + responseString);
                            onGetProfilePicture(null);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Error from Weibo GetProfilePicture: " + error.message);
                    onGetProfilePicture(null);
                }
            });
        }
        else
        {
            onGetProfilePicture(null);
        }
    }


    public void Share(string url, string title, string description, string imageUrl, Action<bool> onShare)
    {
        if (IsLoggedIn())
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["source"] = HSW_KEY;
            parameters["status"] = description + " " + url;
            parameters["access_token"] = GetAccessToken();
            WeiboRequest x = new WeiboRequest();
			x.Get("https://api.weibo.com/2/statuses/update.json", Request.Method.POST, parameters, null, delegate (FGOL.Server.Error error, byte[] response)
			{
				if (error == null && response != null)
				{
#if !PRODUCTION
					string responseString = Encoding.Unicode.GetString(response, 0, response.Length);
					Debug.Log("Response from Weibo SharePicture: " + responseString);
#endif
					onShare(true);
				}
				else
				{
#if !PRODUCTION
					Debug.Log("Error from Weibo SharePicture: " + error.message);
#endif
					onShare(false);
				}
			});
		}
		else
		{
			onShare(false);
		}
	}

    public void SharePicture(byte[] image, string imageName, string description, Dictionary<string, string> args, Action<bool> onShare)
    {
        if (IsLoggedIn())
        {
			OnRequestFinishedDelegate onRequestDone = delegate (HTTPRequest request, HTTPResponse response)
			{
				switch (request.State)
				{
					case HTTPRequestStates.Finished:
#if !PRODUCTION
						if (response != null)
						{
							Debug.Log("Response from Weibo SharePicture: " + response.DataAsText);
						}
#endif
						onShare(true);
						break;
					case HTTPRequestStates.Error:
					case HTTPRequestStates.Aborted:
					case HTTPRequestStates.ConnectionTimedOut:
					case HTTPRequestStates.TimedOut:
#if !PRODUCTION
						Debug.Log("Failed sharepicture: " + request.State.ToString());
						if (response != null)
						{
							Debug.Log("Failed sharepicture: " + response.StatusCode + " - " + response.Data + ": " + response.DataAsText);
						}
#endif
						onShare(false);
						break;

					default:
						//either queued, or waiting or processing, or updaitng
						//no WORRIES
						break;
				}
			};

			HTTPRequest req = new HTTPRequest(new Uri("https://upload.api.weibo.com/2/statuses/upload.json"), HTTPMethods.Post, onRequestDone);
			req.AddBinaryData("pic", image);
			req.AddField("source", HSW_KEY);
			req.AddField("status", description);
			req.AddField("access_token", GetAccessToken());
			req.ConnectTimeout = TimeSpan.FromSeconds(30f);
			req.Timeout = TimeSpan.FromSeconds(30f);
			req.DisableCache = true;
			req.IsCookiesEnabled = true;
			req.Send();
        }
        else
        {
            onShare(false);
        }
    }
}