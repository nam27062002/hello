using System;
using System.Collections.Generic;
using UnityEngine;

interface ISocialInterface
{
    void Init();
    bool IsInited();
    void AppActivation();
    void Login(PermissionType[] permissions, Action<bool> onLogin);
    bool IsLoggedIn();

    void RefreshAuthentication(Action<bool> onRefresh);
    PermissionType[] GetGrantedPermissions();
    void LogOut();

    void GetProfileInfo(Action<Dictionary<string, string>> onGetProfileInfo);
    void InviteFriends(string title, string message, string appUrl, string imageUrl, Action<string, string[]> onFriendRequest);
    void GetFriends(Action<Dictionary<string, string>> onGetFriends);
    void GetProfilePicture(string socialID, Action<Texture2D> onGetProfilePicture, Action<string, Texture2D, Action<Texture2D>> callback, int width = 256, int height = 256);

    void Share(string url, string title, string description, string imageUrl, Action<bool> onShare);
    void SharePicture(byte[] image, string imageName, string description, Dictionary<string, string> args,  Action<bool> onShare);

    string GetAccessToken();
    string GetSocialID();
}
