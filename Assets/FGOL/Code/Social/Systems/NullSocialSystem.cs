//[DGR] No support added yet
//using Definitions;
using System;
using UnityEngine;

public class NullSocialSystem : ISocialSystem
{
    //[DGR] No support added yet
    //private Bank.CurrencyType m_loginRewardType = Bank.CurrencyType.Gems;
    // private int m_loginRewardAmount = 10;
    
    public void Init(SocialSaveSystem socialSaveSystem)
    {
    }

    public bool IsUser()
    {
        return false;
    }

    public bool IsLoggedIn(PermissionType[] permissions = null)
    {
        return false;
    }

    public void Login(Action<bool> onComplete, bool syncSave, bool repeatAsk = false)
    {
        onComplete(false);
    }

    public void Authenticate(Action onComplete = null)
    {
        onComplete();
    }

    public void AskForPublishPermission(Action<bool> onPermissionGranted)
    {
        onPermissionGranted(false);
    }

    public void LogOut(Action onLogout)
    {
        onLogout();
    }

    public void IncentiviseLogin(Action onComplete = null)
    {
        if (onComplete != null)
        {
            onComplete();
        }
    }

    public void GetProfileInfo(Action<string> onGetName, Action<Texture2D> onGetImage)
    {
        onGetName(null);
        onGetImage(null);
    }

    public void InviteFriends(Action<int> onInviteFriends = null)
    {
        onInviteFriends(-1);
    }

    private void OnGameDBLoaded(Enum eventType, object[] args)
    {
        /*
        //[DGR] No support added yet
        GameDB gameDB = GameDataManager.Instance.gameDB;

        if (gameDB != null)
        {
            PlayerInitData data = gameDB.GetItem<PlayerInitData>(PlayerInitData.KeyPlayer);

            if (data != null)
            {
                m_loginRewardType = data.facebookLoginRewardType;
                m_loginRewardAmount = data.facebookLoginReward;
            }
            else
            {
                Debug.LogWarning("WeiboSocialSystem (OnGameDBLoaded) :: PlayerInitData is not available!");
            }
        }
        else
        {
            Debug.LogWarning("WeiboSocialSystem (OnGameDBLoaded) :: GameDB is not available!");
        }
        */
    }
}
