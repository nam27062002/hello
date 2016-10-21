using FGOL.ThirdParty.MiniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NullSocialInterface : ISocialInterface
{
    public void Init(){ }

    public bool IsInited()
    {
        return false;
    }

    public void AppActivation(){ }

    public void Login(PermissionType[] permissions, Action<bool> onLogin)
    {
        onLogin(false);
    }

    public bool IsLoggedIn()
    {
        return false;
    }

    public void RefreshAuthentication(Action<bool> onRefresh)
    {
        onRefresh(false);
    }

    public PermissionType[] GetGrantedPermissions()
    {
        return new PermissionType[0];
    }

    public void LogOut(){ }

    public void GetProfileInfo(Action<Dictionary<string, string>> onGetProfileInfo)
    {
        onGetProfileInfo(null);
    }

    public void InviteFriends(string title, string message, string appUrl, string imageUrl, Action<string, string[]> onFriendRequest)
    {
        onFriendRequest(null, null);
    }

    public void GetFriends(Action<Dictionary<string, string>> onGetFriends)
    {
        onGetFriends(null);
    }

    public void GetProfilePicture(string socialID, Action<Texture2D> onGetProfilePicture, Action<string, Texture2D, Action<Texture2D>> callback, int width = 256, int height = 256)
    {
		// no need to fire the callback because there is nothing to cache.
		onGetProfilePicture(null);
    }

    public void Share(string url, string title, string description, string imageUrl, Action<bool> onShare)
    {
        onShare(false);
    }

    public void SharePicture(byte[] image, string imageName, string description, Dictionary<string, string> args, Action<bool> onShare)
    {
        onShare(false);
    }

    public string GetSocialID()
    {
        return null;
    }

    public string GetAccessToken()
    {
        return null;
    }
}