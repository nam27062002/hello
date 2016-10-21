using System;
using System.Collections.Generic;
using UnityEngine;

public class EditorDummySocialInterface : ISocialInterface
{
    private bool m_isLoggedIn = false;
    private bool m_isInited = false;
    private string m_networkName = "";
    public EditorDummySocialInterface(string networkName)
    {
        m_networkName = networkName;
    }

    public void Init()
    {
        m_isInited = true;
    }

    public bool IsInited()
    {
        return m_isInited;
    }

    public void AppActivation(){ }

    public void Login(PermissionType[] permissions, Action<bool> onLogin)
    {
        m_isLoggedIn = true;
        onLogin(m_isLoggedIn);
    }

    public bool IsLoggedIn()
    {
        return m_isLoggedIn;
    }

    public void RefreshAuthentication(Action<bool> onRefresh)
    {
        onRefresh(m_isLoggedIn);
    }

    public PermissionType[] GetGrantedPermissions()
    {
        return m_isLoggedIn ? new PermissionType[] { PermissionType.Basic, PermissionType.Friends, PermissionType.Publish } : new PermissionType[0];
    }

    public void LogOut()
    {
        m_isLoggedIn = false;
    }

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
        int socialID = PlayerPrefs.GetInt(m_networkName + "DummySocialID", -1);

        if (socialID == -1)
        {            
            UnityEngine.Random.InitState(System.Environment.TickCount);
            socialID = UnityEngine.Random.Range(0, Int32.MaxValue);
            PlayerPrefs.SetInt(m_networkName + "DummySocialID", socialID);
        }

        return socialID.ToString();
    }

    public string GetAccessToken()
    {
        return "DEBUGTOKEN";
    }
}