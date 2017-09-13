using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public abstract class SocialUtils
{
    public abstract string GetPlatformNameTID();    

    public abstract void Init(SocialPlatformManager.GameSocialListener listener);

    public abstract string GetSocialID();

    public abstract string GetAccessToken();

    public abstract string GetUserName();

    public abstract bool IsLoggedIn();
    
    public void GetProfileInfo(Action<string> onGetName, Action<Texture2D> onGetImage)
    {
        string socialID = GetSocialID();
        string profileName = SocialProfileName;
        bool imageAlreadyRetrieved = false;

        if (!string.IsNullOrEmpty(profileName))
        {
            onGetName(profileName);

            Util.StartCoroutineWithoutMonobehaviour("LoadCachedProfileImage", LoadCachedProfileImage(delegate (Texture2D cachedImage) {
                if (cachedImage != null && !imageAlreadyRetrieved)
                {
                    onGetImage(cachedImage);
                }
            }));
        }

        GetProfileInfo(delegate (Dictionary<string, string> profileInfo)
        {
            if (profileInfo != null && profileInfo.ContainsKey("name"))
            {
                SocialProfileName = profileInfo["name"];
                onGetName(profileInfo["name"]);
            }
            else
            {
                onGetName(null);
            }
        });

        GetProfilePicture(socialID, delegate (Texture2D profileImage)
        {
            if (profileImage != null)
            {
                imageAlreadyRetrieved = true;
                File.WriteAllBytes(GetCachedProfileImagePath(), profileImage.EncodeToPNG());
                onGetImage(profileImage);
            }
            else
            {
                onGetImage(null);
            }
        });        
    }

    public abstract void GetProfileInfo(Action<Dictionary<string, string>> onGetProfileInfo);
    
    public void GetProfilePicture(string socialID, Action<Texture2D> onGetProfilePicture, int width = 256, int height = 256)
    {
        ExtendedGetProfilePicture(socialID, onGetProfilePicture, OnProfilePictureReady, width, height);        
    }    

    protected abstract void ExtendedGetProfilePicture(string socialID, Action<Texture2D> onGetProfilePicture, Action<string, Texture2D, Action<Texture2D>> callback, int width = 256, int height = 256);

    private void OnProfilePictureReady(string socialID, Texture2D picture, Action<Texture2D> callback)
    {        
        // send back the picture.
        callback(picture);
    }

    private IEnumerator LoadCachedProfileImage(Action<Texture2D> onLoaded)
    {
        WWW cachedImageLoader = new WWW(GetCachedProfileImagePath(true));

        yield return cachedImageLoader;

        Texture2D cachedImage = new Texture2D(256, 256);

        if (cachedImageLoader.error == null)
        {
            cachedImageLoader.LoadImageIntoTexture(cachedImage);
            onLoaded(cachedImage);
        }
        else
        {
            LogWarning("SocialUtils (LoadCachedProfileImage) :: LoadCachedImage failed: " + cachedImageLoader.error);
            onLoaded(null);
        }

    }

    private string GetCachedProfileImagePath(bool wwwPath = false)
    {
        return string.Format("{0}{1}/ProfileImg.bytes", (wwwPath ? "file://" : ""), Application.temporaryCachePath);
    }

    private string SocialProfileName
    {
        get { return PersistencePrefs.Social_ProfileName; }
        set { PersistencePrefs.Social_ProfileName = value; }
    }

    #region log
    private string LOG_CHANNEL = "Social";
    protected void Log(string msg)
    {
        msg = LOG_CHANNEL + msg;
        Debug.Log(msg);
    }

    protected void LogWarning(string msg)
    {
        Debug.LogWarning(LOG_CHANNEL + msg);
    }

    protected void LogError(string msg)
    {
        Debug.LogError(LOG_CHANNEL + msg);
    }
    #endregion
}