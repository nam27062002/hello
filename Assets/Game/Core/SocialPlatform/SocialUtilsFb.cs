using Facebook.Unity;
using System;
using System.Collections.Generic;
public class SocialUtilsFb : SocialUtils
{
    // Social Listener //////////////////////////////////////////////////////
    private  class GameSocialListener : FacebookManager.FacebookListenerBase
    {
        const string TAG = "GameSocialListener";
        private SocialPlatformManager m_manager;

        public GameSocialListener(SocialPlatformManager manager)
        {
            m_manager = manager;
        }

        public override void onLogInCompleted()
        {
            Debug.TaggedLog(TAG, "onLogInCompleted");
            m_manager.OnSocialPlatformLogin();
        }

        public override void onLogInCancelled()
        {
            Debug.TaggedLog(TAG, "onLogInCancelled");
            m_manager.OnSocialPlatformLoginFailed();
        }

        public override void onLogInFailed()
        {
            Debug.TaggedLog(TAG, "onLogInFailed");
            m_manager.OnSocialPlatformLoginFailed();
        }

        public override void onLogOut()
        {
            m_manager.OnSocialPlatformLogOut();
            Debug.TaggedLog(TAG, "onLogOut");
        }

        public override void onPublishCompleted()
        {
            Debug.TaggedLog(TAG, "onPublishCompleted");
        }

        public override void onPublishFailed()
        {
            Debug.TaggedLog(TAG, "onPublishFailed");
        }
			
        public override void onLikesReceived(bool bIsLiked)
        {
            Debug.TaggedLog(TAG, "onLikesReceived");
        }

        public override void onPostsReceived()
        {
            Debug.TaggedLog(TAG, "onPostsReceived");
        }
    }
    //////////////////////////////////////////////////////////////////////////
    
    public SocialUtilsFb() : base(EPlatform.Facebook)
    {
    }

    public override string GetPlatformNameTID()
    {
        return "TID_SOCIAL_FACEBOOK";
    }

    public override void Init(SocialPlatformManager manager)
    {        
        GameSocialListener listener = new GameSocialListener(manager);
        FacebookManager.SharedInstance.AddFacebookListener(listener);
        FacebookManager.SharedInstance.Initialise();
    }

    public override string GetSocialID()
    {
        return FacebookManager.SharedInstance.UserID;
    }

    public override string GetAccessToken()
    {
        return (FB.IsInitialized && FB.IsLoggedIn) ? AccessToken.CurrentAccessToken.TokenString : null;
    }

    public override string GetUserName()
    {
        return FacebookManager.SharedInstance.UserName;
    }

    public override bool IsLoggedIn()
    {
        return FacebookManager.SharedInstance.IsLoggedIn();
    }

	public override bool IsLogInTimeoutEnabled()
	{
		#if UNITY_ANDROID
		return true;
		#else
		return false;
		#endif
	}

    public override void GetProfileInfoFromPlatform(Action<ProfileInfo> onGetProfileInfo)
    {
        if (FB.IsInitialized && FB.IsLoggedIn)
        {
            ProfileInfo profileInfo = null;

            FB.API("/me?fields=id,first_name,last_name", HttpMethod.GET, delegate (IGraphResult response)
            {                
                if (string.IsNullOrEmpty(response.Error))
                {
                    profileInfo = new ProfileInfo();

                    foreach (KeyValuePair<string, object> pair in response.ResultDictionary)
                    {                        
                        string value = pair.Value as string;
                        if (value != null)
                        {
                            profileInfo.SetValueAsString(pair.Key, value);
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
            LogError("(GetProfileInfo) :: FB not initiliazed or logged in!");
            onGetProfileInfo(null);
        }
    }

    protected override void ExtendedGetProfilePicture(string socialID, string storagePath, Action<bool> onGetProfilePicture, int width = 256, int height = 256)
    {
        string url = string.Format("/{0}/picture?width={1}&height={2}&redirect=false&type=normal", socialID, width, height);
        
        LogWarning("SocialUtilsFb :: (GetProfilePicture) gettint URL " + url);

        FB.API(url, HttpMethod.GET, delegate (IGraphResult result)
        {
            if (result != null && string.IsNullOrEmpty(result.Error))
            {
                if (result.ResultDictionary != null && result.ResultDictionary.ContainsKey("data"))
                {
                    Dictionary<string, object> pictureInfo = result.ResultDictionary["data"] as Dictionary<string, object>;

                    if (pictureInfo != null)
                    {                        
                        if (pictureInfo.ContainsKey("url") && (!pictureInfo.ContainsKey("is_silhouette") || !(bool)pictureInfo["is_silhouette"]))
                        {                            
                            Log("SocialUtilsFb :: (GetProfilePicture) Profile image " + pictureInfo["url"] + " requested");                           

                            UnityEngine.Events.UnityAction<bool, string, long> onThisDone = delegate(bool success,  string key, long size)
                            {
                                onGetProfilePicture(success);
                            };

                            NetworkManager.SharedInstance.DownloadFile("profilePic_" + socialID, pictureInfo["url"] as string, FileUtils.GetDeviceStoragePath(storagePath, CaletyConstants.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED), onThisDone);
                        }
                        else
                        {                            
                            FileUtils.RemoveFileInDeviceStorage(storagePath, CaletyConstants.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);                            
                            
                            Log("SocialUtilsFb :: (GetProfilePicture) Invalid image!");

                            onGetProfilePicture(true);
                        }
                    }
                    else
                    {                        
                        LogWarning("SocialUtilsFb :: (GetProfilePicture) Invalid json response: " + result.RawResult);
                        onGetProfilePicture(true);
                    }
                }
                else
                {                    
                    LogWarning("SocialUtilsFb :: (GetProfilePicture) Invalid json response: " + result.RawResult);
                    onGetProfilePicture(true);
                }
            }
            else
            {                
                LogWarning("SocialUtilsFb :: (GetProfilePicture) Error getting facebook profile picture: " + (result.Error != null ? result.Error : "NONE"));
                onGetProfilePicture(true);
            }
        });
    }   
}