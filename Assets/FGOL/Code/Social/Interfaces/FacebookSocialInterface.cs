using Facebook.Unity;
using FGOL.Server;
using FGOL.ThirdParty.MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacebookSocialInterface : ISocialInterface
{
    private Action m_onInitialization = null;
    private bool m_isInited = false;

    public void Init()
    {
        m_isInited = false;

        FB.Init(
            delegate()
            {
                Debug.Log("Facebook :: Initialized");                

                m_isInited = true;
                if (m_onInitialization != null)
                {
                    m_onInitialization();
                }
            },
            delegate(bool isGameShown)
            {
                //TODO do we need to deal with this? This is fired when the game is hidden so we may need to pause or resume stuff!
            }
        );
    }

    public bool IsInited()
    {
        return m_isInited;
    }

    public void AppActivation()
    {
        Action appActivation = delegate ()
        {
            FB.ActivateApp();
        };

        if (FB.IsInitialized)
        {
            appActivation();
        }
        else
        {
            Debug.LogWarning("FacebookInterface :: Not initialized!");
            m_onInitialization += appActivation;
        }
    }

    public void Login(PermissionType[] permissions, Action<bool> onLogin)
    {
        //Check that read permissions aren't asked together with write permissions
        bool writePermissions = Array.IndexOf<PermissionType>(permissions, PermissionType.Publish) >= 0;

        Log("Login " + onLogin);
        if (!writePermissions || permissions.Length == 1)
        {
            Log("Login dentro");
            Action login = delegate ()
            {
                FacebookDelegate<ILoginResult> loginCallback = delegate(ILoginResult result)
                {
                    Log("loginCallback " + FB.IsLoggedIn);
                    if (FB.IsLoggedIn)
                    {
                        onLogin(true);
                    }
                    else
                    {
                        LogError("FacebookInterface :: (Login) Login not successful! Error - " + (result != null && result.Error != null ? result.Error : "NONE"));
                        onLogin(false);
                    }
                };

                if (writePermissions)
                {
                    Log("LogInWithPublishPermissions " + loginCallback);
                    FB.LogInWithPublishPermissions(GetLoginScope(permissions), loginCallback);
                }
                else
                {
                    Log("LogInWithReadPermissions " + loginCallback);
                    FB.LogInWithReadPermissions(GetLoginScope(permissions), loginCallback);
                }
            };

            Log("IsInitialized " + FB.IsInitialized);
            if (FB.IsInitialized)
            {
                Log("login()");
                login();
            }
            else
            {
                Debug.LogWarning("FacebookInterface :: Not initialized!");
                m_onInitialization += login;
            }
        }
        else
        {
            LogError("FacebookInterface (Login) :: Can't ask for write permissions with read permissions!");
            onLogin(false);
        }
    }

    public bool IsLoggedIn()
    {
        return FB.IsInitialized && FB.IsLoggedIn;
    }

    public void RefreshAuthentication(Action<bool> onRefresh)
    {
        if (FB.IsInitialized && FB.IsLoggedIn)
		{
			//from the facebook sdk page. QUOTE:
			//With the iOS and Android SDKs, long-lived tokens are used by default and should automatically be refreshed.

            onRefresh(true);
			onRefresh = null;
			/*
			Util.StartCoroutineWithoutMonobehaviour("RefreshFallback", RefreshFallback(15, onRefresh));

			FB.Mobile.RefreshCurrentAccessToken(delegate (IAccessTokenRefreshResult result){
                if (FB.IsLoggedIn || (result != null && string.IsNullOrEmpty(result.Error) && result.AccessToken != null))
                {
                    onRefresh(true);
					onRefresh = null;
                }
                else
                {
                    Debug.LogError("FacebookInterface (RefreshAuthentication) :: Failed to refresh token with error: " + (result != null ? result.Error : "UNKNOWN ERROR"));
                    onRefresh(false);
					onRefresh = null;
				}
            });
			*/
		}
        else
        {
            Debug.LogError("FacebookInterface (RefreshAuthentication) :: Failed to refresh token with error: FB not logged in or initialized");
            onRefresh(false);
			onRefresh = null;
		}
    }

	// Facebook activity crashes upon refreshing authentication when internet stability is low/not working
	// In order to fix it we set a timer for callback, refresh will fail after a given time to prevent unlimited load times
	private IEnumerator RefreshFallback(float delay, Action<bool> onRefresh)
	{
		while (delay > 0)
		{
			delay -= Time.deltaTime;
			yield return null;
		}

		if (onRefresh != null)
		{
			onRefresh(false);
			onRefresh = null;
		}
	}

	public PermissionType[] GetGrantedPermissions()
    {
        List<PermissionType> grantedPermissions = new List<PermissionType>();

        if (FB.IsInitialized && FB.IsLoggedIn)
        {
            foreach (string permission in AccessToken.CurrentAccessToken.Permissions)
            {
                switch (permission)
                {
                    case "public_profile":
                        grantedPermissions.Add(PermissionType.Basic);
                        break;
                    case "user_friends":                        
                        grantedPermissions.Add(PermissionType.Friends);
                        break;
                    case "publish_actions":
                        grantedPermissions.Add(PermissionType.Publish);
                        break;
                }
            }
        }

        return grantedPermissions.ToArray();
    }

    public void LogOut()
    {
        if(FB.IsInitialized && FB.IsLoggedIn)
        {
            FB.LogOut();
        }
    }

    public void GetProfileInfo(Action<Dictionary<string, string>> onGetProfileInfo)
    {
        if(FB.IsInitialized && FB.IsLoggedIn)
        {
            FB.API("/me", HttpMethod.GET, delegate(IGraphResult response)
            {
                if (string.IsNullOrEmpty(response.Error))
                {
                    Dictionary<string, string> profileInfo = new Dictionary<string, string>();

                    foreach(KeyValuePair<string, object> pair in response.ResultDictionary)
                    {
                        string value = pair.Value as string;

                        if(value != null)
                        {
                            profileInfo.Add(pair.Key, value);
                        }
                    }

                    onGetProfileInfo(profileInfo);
                }
                else
                {
                    onGetProfileInfo(null);
                }
            });
        }
        else
        {
            Debug.LogError("FacebookInterface (GetProfileInfo) :: FB not initiliazed or logged in!");
            onGetProfileInfo(null);
        }
    }

    public void InviteFriends(string title, string message, string appUrl, string imageUrl, Action<string, string[]> onFriendRequest)
    {
        //TODO support non mobile invite on non mobile platforms!
        FB.Mobile.AppInvite(new Uri(appUrl), new Uri(imageUrl), delegate (IAppInviteResult result){
            if (string.IsNullOrEmpty(result.Error))
            {
                onFriendRequest(null, null);
            }
            else
            {
                Debug.LogError("FacebookInterface :: (InviteFriends) Error - " + (result.Error != null ? result.Error : "NONE"));
            }
        });

        /*FB.AppRequest(message, null, null, null, null, "", title, delegate(IAppRequestResult result)
        {
            if(string.IsNullOrEmpty(result.Error))
            {
                if(result.ResultDictionary != null && result.ResultDictionary.ContainsKey("request"))
                {
					string[] friends = result.ResultDictionary.ContainsKey("to") ? Json.GetArray<string>(result.ResultDictionary["to"]) : null;
					onFriendRequest(result.ResultDictionary["request"] as string, friends);

					// Report anaylics
					HSXAnalyticsManager.Instance.InviteSent(friends);
                }
                else
                {
                    onFriendRequest(null, null);
                }
            }
            else
            {
                Debug.LogError("FacebookInterface :: (InviteFriends) Error - " + (result.Error != null ? result.Error : "NONE"));
                onFriendRequest(null, null);
            }
        });*/
		}

	public void GetFriends(Action<Dictionary<string, string>> onGetFriends)
    {
        if (FB.IsInitialized && FB.IsLoggedIn)
        {
            Dictionary<string, string> allFriends = new Dictionary<string, string>();

            Action<string> getFriends = null;

            getFriends = delegate (string url)
            {
                FB.API(url, HttpMethod.GET, delegate (IGraphResult result)
                {
                    if (result != null && string.IsNullOrEmpty(result.Error) && result.ResultDictionary != null)
                    {
                        if (result.ResultDictionary.ContainsKey("data"))
                        {
                            Dictionary<string, object>[] friends = Json.GetArray<Dictionary<string, object>>(result.ResultDictionary["data"]);

                            if (friends != null)
                            {
                                foreach (Dictionary<string, object> friend in friends)
                                {
                                    allFriends.Add(friend["id"] as string, friend["name"] as string);
                                }
                            }

                            if (result.ResultDictionary.ContainsKey("paging"))
                            {
                                Dictionary<string, object> paging = result.ResultDictionary["paging"] as Dictionary<string, object>;

                                if (paging.ContainsKey("next"))
                                {
                                    string nextUrl = paging["next"] as string;

                                    if (!string.IsNullOrEmpty(nextUrl))
                                    {
                                        getFriends("me/friends" + nextUrl.Substring(nextUrl.IndexOf("?")));
                                    }
                                    else
                                    {
                                        onGetFriends(allFriends.Count > 0 ? allFriends : null);
                                    }
                                }
                                else
                                {
                                    onGetFriends(allFriends.Count > 0 ? allFriends : null);
                                }
                            }
                            else
                            {
                                onGetFriends(allFriends.Count > 0 ? allFriends : null);
                            }
                        }
                        else
                        {
                            onGetFriends(null);
                        }
                    }
                    else
                    {
                        Debug.LogError("FacebookInterface :: (GetFriends) Error - " + (result.Error != null ? result.Error : "NONE") + " message - " + result.RawResult);
                        onGetFriends(null);
                    }
                });
            };

            getFriends("me/friends");
        }
        else
        {
            Debug.LogError("FacebookInterface (GetFriends) :: FB not initiliazed or logged in!");
            onGetFriends(null);
        }
    }

    public void GetProfilePicture(string socialID, Action<Texture2D> onGetProfilePicture, Action<string, Texture2D, Action<Texture2D>> callback, int width = 256, int height = 256)
    {
        string url = string.Format("/{0}/picture?width={1}&height={2}&redirect=false&type=normal", socialID, width, height);

        FB.API(url, HttpMethod.GET, delegate(IGraphResult result)
        {
            if(result != null && string.IsNullOrEmpty(result.Error))
            {
                if(result.ResultDictionary != null && result.ResultDictionary.ContainsKey("data"))
                {
                    Dictionary<string, object> pictureInfo = result.ResultDictionary["data"] as Dictionary<string, object>;

                    if(pictureInfo != null)
                    {
                        if(pictureInfo.ContainsKey("url") && (!pictureInfo.ContainsKey("is_silhouette") || !(bool)pictureInfo["is_silhouette"]))
                        {
                            ImageRequest request = new ImageRequest();
                            request.Get(pictureInfo["url"] as string, delegate(Error error, Texture2D texture)
                            {
                                if(error == null)
                                {
                                    callback(socialID, texture, onGetProfilePicture);
                                }
                                else
                                {
                                    Debug.Log("FacebookInterface :: (GetProfilePicture) Error getting image: " + error);
									// no need to fire the callback because there is nothing to cache.
									onGetProfilePicture(null);
                                }
                            });
                        }
                        else
                        {
                            Debug.Log("FacebookInterface :: (GetProfilePicture) Invalid image!");
                            onGetProfilePicture(null);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("FacebookInterface :: (GetProfilePicture) Invalid json response: " + result.RawResult);
                        onGetProfilePicture(null);
                    }
                }
                else
                {
                    Debug.LogWarning("FacebookInterface :: (GetProfilePicture) Invalid json response: " + result.RawResult);
                    onGetProfilePicture(null);
                }
            }
            else
            {
                Debug.LogWarning("FacebookInterface :: (GetProfilePicture) Error getting facebook profile picture: " + (result.Error != null ? result.Error : "NONE"));
                onGetProfilePicture(null);
            }
        });
    }

	public void Share(string url, string title, string description, string imageUrl, Action<bool> onShare)
	{
		if(FB.IsInitialized && FB.IsLoggedIn)
		{
			Util.StartCoroutineWithoutMonobehaviour("ShareFallback", ShareFallback(15, onShare));

			FB.ShareLink(new Uri(url), title, description, new Uri(imageUrl), delegate (IShareResult result) {
				if(result != null && !result.Cancelled && string.IsNullOrEmpty(result.Error))
				{
					if(onShare != null)
					{
						onShare(true);
						onShare = null;
					}
				}
				else
				{
					if(onShare != null)
					{
						Debug.Log("FacebookInterface (Share) :: Share not successful - Reason: " + ((result != null && result.Cancelled) ? "User cancelled" : (result != null ? result.Error : "Empty result")));
						onShare(false);
						onShare = null;
					}
				}
			});
		}
		else
		{
			if(onShare != null)
			{
				Debug.LogError("FacebookInterface (Share) :: FB not initiliazed or logged in!");
				onShare(false);
				onShare = null;
			}
		}
	}

	// Facebook activity crashes upon switching users while share dialogue is active: HSX-4162
	// In order to fix it we set a timer for callback, it will be called after X delay if user has switched accounts and FB activity crashed
	// It will call the share callback with false result to avoind infinite loading UI
	private IEnumerator ShareFallback(float delay, Action<bool> onShare)
	{
		while(delay > 0)
		{
			delay -= Time.deltaTime;
			yield return null;
		}

		if(onShare != null)
		{
			onShare(false);
			onShare = null;
		}
	}

	public void SharePicture(byte[] image, string imageName, string description, Dictionary<string, string> args, Action<bool> onShare)
    {
        if (FB.IsInitialized && FB.IsLoggedIn)
        {
            WWWForm formData = new WWWForm();
            formData.AddBinaryData("image", image, imageName);
            formData.AddField("caption", description);

            if (args != null)
            {
                foreach(KeyValuePair<string, string> pair in args)
                {
                    formData.AddField(pair.Key, pair.Value);
                }
            }

            FB.API("me/photos", Facebook.Unity.HttpMethod.POST, delegate (IGraphResult result){
                if (result != null && string.IsNullOrEmpty(result.Error))
                {
                    onShare(true);
                }
                else
                {
                    Debug.Log("FacebookInterface (SharePicture) :: Share not successful - Reason: " + (result != null && result.Error != null ? result.Error : "Empty result"));
                    onShare(false);
                }
            }, formData);
        }
        else
        {
            Debug.LogError("FacebookInterface (Share) :: FB not initiliazed or logged in!");
            onShare(false);
        }
    }

    public string GetSocialID()
    {
        return (FB.IsInitialized && FB.IsLoggedIn) ? AccessToken.CurrentAccessToken.UserId : null;
    }

    public string GetAccessToken()
    {
        return (FB.IsInitialized && FB.IsLoggedIn) ? AccessToken.CurrentAccessToken.TokenString : null;
    }

    private List<string> GetLoginScope(PermissionType[] permissions)
    {
        List<string> scopes = new List<string>();

        foreach(PermissionType permission in permissions)
        {
            switch(permission)
            {
                case PermissionType.Basic:
                    scopes.Add("public_profile");
                    break;
                case PermissionType.Friends:
                    scopes.Add("user_friends");
                    break;
                case PermissionType.Publish:
                    scopes.Add("publish_actions");
                    break;
            }
        }

        return scopes;
    }

    #region log
    private const string PREFIX = "FbSocialInterface:";

    private void Log(string message)
    {
        Debug.Log(PREFIX + message);        
    }

    private void LogWarning(string message)
    {     
        Debug.LogWarning(PREFIX + message);
    }

    private void LogError(string message)
    {        
        Debug.LogError(PREFIX + message);
    }
    #endregion
}