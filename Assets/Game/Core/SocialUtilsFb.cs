using Facebook.Unity;
using FGOL.Server;
using System;
using System.Collections.Generic;
using UnityEngine;
public class SocialUtilsFb : SocialUtils
{   
    public override string GetPlatformNameTID()
    {
        return "TID_SOCIAL_FACEBOOK";
    }

    public override void Init(SocialPlatformManager.GameSocialListener listener)
    {        
        FacebookManager.SharedInstance.AddFacebookListener(listener);
        FacebookManager.SharedInstance.Initialise();
    }

    public override string GetSocialID()
    {
        return (FB.IsInitialized && FB.IsLoggedIn) ? AccessToken.CurrentAccessToken.UserId : null;
    }

    public override string GetAccessToken()
    {
        return (FB.IsInitialized && FB.IsLoggedIn) ? AccessToken.CurrentAccessToken.TokenString : null;
    }

    public override bool IsLoggedIn()
    {
        return FacebookManager.SharedInstance.IsLoggedIn();
    }

    public override void GetProfileInfo(Action<Dictionary<string, string>> onGetProfileInfo)
    {
        if (FB.IsInitialized && FB.IsLoggedIn)
        {
            FB.API("/me", HttpMethod.GET, delegate (IGraphResult response)
            {
                if (string.IsNullOrEmpty(response.Error))
                {
                    Dictionary<string, string> profileInfo = new Dictionary<string, string>();

                    foreach (KeyValuePair<string, object> pair in response.ResultDictionary)
                    {
                        string value = pair.Value as string;

                        if (value != null)
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
            if (FeatureSettingsManager.IsDebugEnabled)
                LogError("(GetProfileInfo) :: FB not initiliazed or logged in!");

            onGetProfileInfo(null);
        }
    }

    protected override void ExtendedGetProfilePicture(string socialID, Action<Texture2D> onGetProfilePicture, Action<string, Texture2D, Action<Texture2D>> callback, int width = 256, int height = 256)
    {
        string url = string.Format("/{0}/picture?width={1}&height={2}&redirect=false&type=normal", socialID, width, height);

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
                            ImageRequest request = new ImageRequest();
                            request.Get(pictureInfo["url"] as string, delegate (Error error, Texture2D texture)
                            {
                                if (error == null)
                                {
                                    callback(socialID, texture, onGetProfilePicture);
                                }
                                else
                                {
                                    if (FeatureSettingsManager.IsDebugEnabled)
                                        Log("SocialUtilsFb :: (GetProfilePicture) Error getting image: " + error);

                                    // no need to fire the callback because there is nothing to cache.
                                    onGetProfilePicture(null);
                                }
                            });
                        }
                        else
                        {
                            if (FeatureSettingsManager.IsDebugEnabled)
                                Log("SocialUtilsFb :: (GetProfilePicture) Invalid image!");

                            onGetProfilePicture(null);
                        }
                    }
                    else
                    {
                        if (FeatureSettingsManager.IsDebugEnabled)
                            LogWarning("SocialUtilsFb :: (GetProfilePicture) Invalid json response: " + result.RawResult);

                        onGetProfilePicture(null);
                    }
                }
                else
                {
                    if (FeatureSettingsManager.IsDebugEnabled)
                        LogWarning("SocialUtilsFb :: (GetProfilePicture) Invalid json response: " + result.RawResult);

                    onGetProfilePicture(null);
                }
            }
            else
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                    LogWarning("SocialUtilsFb :: (GetProfilePicture) Error getting facebook profile picture: " + (result.Error != null ? result.Error : "NONE"));

                onGetProfilePicture(null);
            }
        });
    }
}