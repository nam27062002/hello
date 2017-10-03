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

    public override void GetProfileInfoFromPlatform(Action<ProfileInfo> onGetProfileInfo)
    {
        if (FB.IsInitialized && FB.IsLoggedIn)
        {
            ProfileInfo profileInfo = null;

            FB.API("/me?fields=id,first_name,last_name,gender,email,age_range", HttpMethod.GET, delegate (IGraphResult response)
            {                
                if (string.IsNullOrEmpty(response.Error))
                {
                    profileInfo = new ProfileInfo();

                    foreach (KeyValuePair<string, object> pair in response.ResultDictionary)
                    {
                        if (pair.Key == "age_range")
                        {
                            Dictionary<string, object> age_range = pair.Value as Dictionary<string, object>;
                            if (age_range != null)
                            {
                                int min = 0;
                                int max = 0;

                                string key = "min";                                
                                if (age_range.ContainsKey(key))
                                {
                                    min = Convert.ToInt32(age_range[key]);
                                }

                                key = "max";
                                if (age_range.ContainsKey(key))
                                {                                    
                                    max = Convert.ToInt32(age_range[key]);
                                }                                

                                int ageAsInt = min;
                                if (max > 0)
                                {
                                    if (min > 0)
                                    {
                                        ageAsInt = (min + max) / 2;
                                    }
                                    else
                                    {
                                        ageAsInt = max;
                                    }
                                }

                                int birthday = GameServerManager.SharedInstance.GetEstimatedServerTime().Year - ageAsInt;

                                //profileInfo.Add("age_range_min", min + "");
                                //profileInfo.Add("age_range_max", max + "");                                
                                profileInfo.YearOfBirth = birthday;
                            }
                        }
                        else
                        {
                            string value = pair.Value as string;
                            if (value != null)
                            {
                                profileInfo.SetValueAsString(pair.Key, value);
                            }
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

    protected override void ExtendedGetProfilePicture(string socialID, Action<Texture2D, bool> onGetProfilePicture, int width = 256, int height = 256)
    {
        string url = string.Format("/{0}/picture?width={1}&height={2}&redirect=false&type=normal", socialID, width, height);

        if (FeatureSettingsManager.IsDebugEnabled)
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
                            if (FeatureSettingsManager.IsDebugEnabled)
                                Log("SocialUtilsFb :: (GetProfilePicture) Profile image " + pictureInfo["url"] + " requested");

                            ImageRequest request = new ImageRequest();
                            request.Get(pictureInfo["url"] as string, delegate (Error error, Texture2D texture)
                            {
                                if (error == null)
                                {
                                    onGetProfilePicture(texture, false);                                    
                                }
                                else
                                {
                                    if (FeatureSettingsManager.IsDebugEnabled)
                                        Log("SocialUtilsFb :: (GetProfilePicture) Error getting image: " + error);

                                    // no need to fire the callback because there is nothing to cache.
                                    onGetProfilePicture(null, true);
                                }
                            });
                        }
                        else
                        {
                            if (FeatureSettingsManager.IsDebugEnabled)
                                Log("SocialUtilsFb :: (GetProfilePicture) Invalid image!");

                            onGetProfilePicture(null, false);
                        }
                    }
                    else
                    {
                        if (FeatureSettingsManager.IsDebugEnabled)
                            LogWarning("SocialUtilsFb :: (GetProfilePicture) Invalid json response: " + result.RawResult);

                        onGetProfilePicture(null, true);
                    }
                }
                else
                {
                    if (FeatureSettingsManager.IsDebugEnabled)
                        LogWarning("SocialUtilsFb :: (GetProfilePicture) Invalid json response: " + result.RawResult);

                    onGetProfilePicture(null, true);
                }
            }
            else
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                    LogWarning("SocialUtilsFb :: (GetProfilePicture) Error getting facebook profile picture: " + (result.Error != null ? result.Error : "NONE"));

                onGetProfilePicture(null, true);
            }
        });
    }
}