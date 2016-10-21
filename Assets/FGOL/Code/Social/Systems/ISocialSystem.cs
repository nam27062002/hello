using FGOL.Authentication;
using FGOL.Events;
using FGOL.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public interface ISocialSystem
{
    void Init(SocialSaveSystem socialSaveSystem);    
    bool IsUser();
    bool IsLoggedIn(PermissionType[] permissions = null);
    void Login(Action<bool> onComplete, bool syncSave, bool repeatAsk = false);
    void Authenticate(Action onComplete = null);
    void AskForPublishPermission(Action<bool> onPermissionGranted);
    void LogOut(Action onLogout);
    void IncentiviseLogin(Action onComplete = null);
    void GetProfileInfo(Action<string> onGetName, Action<Texture2D> onGetImage);
    void InviteFriends(Action<int> onInviteFriends = null);
}
