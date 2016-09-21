//[DGR] No support added yet
//using Definitions;
using FGOL.Authentication;
using FGOL.Events;
using FGOL.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WeiboSocialSystem : ISocialSystem
{
    // [DGR] No support added yet
    /*
    private SocialSaveSystem m_socialSaveSystem = null;    
    private Bank.CurrencyType m_loginRewardType = Bank.CurrencyType.Gems;
    private int m_loginRewardAmount = 10;
    */


    public void Init(SocialSaveSystem socialSaveSystem)
    {
        // [DGR] RULES: No support added yet
        //EventManager.Instance.RegisterEvent(Events.OnGameDBLoaded, OnGameDBLoaded);

        // [DGR] No support added yet
        //m_socialSaveSystem = socialSaveSystem;
    }

    public bool IsUser()
    {
        return AuthManager.Instance.IsNetworkPreviouslyAuthenticated(User.LoginType.Weibo);
    }

    public bool IsLoggedIn(PermissionType[] permissions = null)
    {
        //permissions are ignored for ol' weibo
        return AuthManager.Instance.IsNetworkAuthenticated(User.LoginType.Weibo, permissions);
    }

    public void Login(Action<bool> onComplete, bool syncSave, bool repeatAsk = false)
    {
        bool cloudSavePreviouslyAvailable = Authenticator.Instance.User != null && Authenticator.Instance.User.cloudSaveAvailable;
        PermissionType[] perms = new PermissionType[] { PermissionType.Friends };
        AuthManager.Instance.Login(User.LoginType.Weibo, perms, delegate (Error error, PermissionType[] grantedPermissions, bool cloudSaveAvailable)
        {
            if (error == null)
            {
                Debug.Log(string.Format("WeiboSocialSystem (WeiboLogin) :: Weibo Login successful! CS avail {0}, enabled {1} ", cloudSaveAvailable, SaveFacade.Instance.cloudSaveEnabled));

                if (syncSave)
                {
                    if (cloudSaveAvailable || SaveFacade.Instance.cloudSaveEnabled)
					{
						if (onComplete != null)
						{
							onComplete(true);
						}

						SaveFacade.Instance.cloudSaveEnabled = true;
						SaveFacade.Instance.ClearError();
						SaveFacade.Instance.GoToSaveLoaderState();
					}
					else
					{
                        SaveFacade.Instance.CloudSaveEnableConfirmation(User.LoginType.Weibo, true, 
						()=> 
						{
							if (onComplete != null)
							{
								onComplete(true);
							}
						},
						delegate ()
						{
							IncentiviseLogin(delegate(){
								if (onComplete != null)
								{
									onComplete(true);
                                }
                            });
                        });
                    }
                }
                else
                {
                    IncentiviseLogin(delegate () {
                        if (onComplete != null)
                        {
                            onComplete(true);
                        }
                    });
                }
            }
            else
            {
                Debug.LogWarning("WeiboSocialSystem (WeiboLogin) :: Error logging in  - " + error);

                if (!repeatAsk)
                {
                    if (error.GetType() == typeof(UserAuthError))
                    {
                        if ((SaveFacade.Instance.cloudSaveEnabled || cloudSaveAvailable || cloudSavePreviouslyAvailable) && syncSave)
                        {
                            Debug.LogWarning("WeiboSocialSystem (WeiboLogin) :: Switching account with cloud save enabled!");

                            /*
                            //[DGR] No support added yet
                            MessageBoxPopup.MessageBoxConfig config = new MessageBoxPopup.MessageBoxConfig();
                            config.titleText = "TID_SAVE_WARN_CLOUD_SWITCH_NAME";
							config.messageText = "TID_SAVE_POPUP_WARN_CLOUD_SWITCH_NETWORK_DESC";
                            config.messageArgs = new object[] { SocialFacade.GetLocalizedNetworkName(SocialManager.GetSelectedSocialNetwork()),
								SocialFacade.GetLocalizedNetworkName(SocialFacade.Network.Weibo)};
							config.backButtonMode = MessageBoxPopup.MessageBoxConfig.BackButtonMode.cancel;
                            config.onConfirm = delegate ()
							{
                                User.Clear();
								Authenticator.Instance.User = new User();
								if (onComplete != null)
								{
									onComplete(true);
								}
								SaveFacade.Instance.cloudSaveEnabled = true;
                                SaveFacade.Instance.ClearError();
                                SaveFacade.Instance.GoToSaveLoaderState();
                            };

                            config.onCancel = delegate ()
                            {
                                MessageBoxPopup.MessageBoxConfig cancelConfig = new MessageBoxPopup.MessageBoxConfig();
                                cancelConfig.titleText = "TID_SAVE_ERROR_CLOUD_DISABLED_NAME";
								cancelConfig.messageText = "TID_SAVE_POPUP_ERROR_CLOUD_SAVE_DISABLED_DESC";
								cancelConfig.messageArgs = new object[] { SocialFacade.GetLocalizedNetworkName(SocialFacade.Network.Weibo)};
								cancelConfig.cancelEnabled = false;
								cancelConfig.backButtonMode = MessageBoxPopup.MessageBoxConfig.BackButtonMode.confirm;
                                cancelConfig.onConfirm = delegate (){
                                    AuthManager.Instance.SocialLogout(User.LoginType.Weibo);

                                    if (onComplete != null)
                                    {
                                        onComplete(false);
                                    }
                                };

                                MessageBoxPopup.OpenMessageBox(cancelConfig);
                            };

                            MessageBoxPopup.OpenMessageBox(config);
                            */
                        }
                        else
                        {
                            Debug.LogWarning("WeiboSocialSystem (WeiboLogin) :: Wrong social account!");

                            /*
                            //[DGR] No support added yet
                            MessageBoxPopup.MessageBoxConfig config = new MessageBoxPopup.MessageBoxConfig();

                            config.titleText = "TID_SOCIAL_ERROR_WRONG_ACCOUNT_NAME";
							config.messageText = "TID_SOCIAL_ERROR_WRONG_ACCOUNT_DESC";
							config.messageArgs = new object[] { SocialFacade.GetLocalizedNetworkName(SocialFacade.Network.Weibo)};
                            config.onConfirm = delegate ()
                            {
                                AuthManager.Instance.SocialLogout(User.LoginType.Weibo);

                                if (onComplete != null)
                                {
                                    onComplete(false);
                                }
                            };
							
							config.backButtonMode = MessageBoxPopup.MessageBoxConfig.BackButtonMode.confirm;
                            config.cancelEnabled = false;
                            config.modal = false;
                            MessageBoxPopup.OpenMessageBox(config);
                            */
                        }
                    }
                    else
                    {
                    /*
                    //[DGR] No support added yet
                    MessageBoxPopup.MessageBoxConfig config = new MessageBoxPopup.MessageBoxConfig();

                    config.cancelEnabled = false;

                    config.titleText = "TID_SOCIAL_LOGIN_FAILED_NAME";
                    config.messageText = "TID_SOCIAL_LOGIN_FAILED_DESC";
                    config.messageArgs = new object[] { SocialFacade.GetLocalizedNetworkName(SocialFacade.Network.Weibo)};
                    config.onConfirm = delegate ()
                    {
                        AuthManager.Instance.SocialLogout(User.LoginType.Weibo);

                        if (onComplete != null)
                        {
                            onComplete(false);
                        }
                    };
                    config.backButtonMode = MessageBoxPopup.MessageBoxConfig.BackButtonMode.confirm;

                    MessageBoxPopup.OpenMessageBox(config);
                    */
                }
            }
            else if (onComplete != null)
            {
                onComplete(false);
            }
        }
    });
}

public void Authenticate(Action onComplete = null)
{
    Debug.Log("WeiboSocialSystem (AuthenticateWeibo) :: Authenticating Weibo!");

    if (!AuthManager.Instance.IsNetworkAuthenticated(User.LoginType.Weibo))
    {
        AuthManager.Instance.Authenticate(Authenticator.Instance.User.loginCredentials[User.LoginType.Weibo].permissions, delegate (Error error, PermissionType[] grantedPermissions, bool cloudSaveAvailable)
        {
            if (error != null && error.GetType() == typeof(UserAuthError))
            {
                /*
                //[DGR] No support added yet
                MessageBoxPopup.MessageBoxConfig config = new MessageBoxPopup.MessageBoxConfig();

                config.titleText = "TID_SOCIAL_ERROR_WRONG_ACCOUNT_NAME";
                config.messageText = "TID_SOCIAL_ERROR_WRONG_ACCOUNT_DESC";
                config.messageArgs = new object[]{SocialFacade.GetLocalizedNetworkName(SocialFacade.Network.Weibo)};
                config.onConfirm = delegate ()
                {
                    Debug.Log("WeiboSocialSystem (AuthenticateWeibo) :: Wrong Weibo account - " + error);

                    if (onComplete != null)
                    {
                        onComplete();
                    }
                };

                config.backButtonMode = MessageBoxPopup.MessageBoxConfig.BackButtonMode.confirm;
                config.cancelEnabled = false;
                config.modal = false;
                MessageBoxPopup.OpenMessageBox(config);
                */
            }
            else
            {
                Debug.Log("WeiboSocialSystem (AuthenticateWeibo) :: Weibo Auth Complete - " + error);

                if (onComplete != null)
                {
                    onComplete();
                }
            }
        }, true);
    }
    else
    {
        if (onComplete != null)
        {
            onComplete();
        }
    }
}

public void AskForPublishPermission(Action<bool> onPermissionGranted)
{
    Action onFailedPermissions = delegate (){
        /*
        //[DGR] No support added yet
        MessageBoxPopup.MessageBoxConfig config = new MessageBoxPopup.MessageBoxConfig();

        config.cancelEnabled = false;

        config.titleText = "STRING_WeiboPOST_ERROR_SHAREPERMISSION_TITLE";
        config.messageText = "STRING_WeiboPOST_ERROR_SHAREPERMISSION";
        config.backButtonMode = MessageBoxPopup.MessageBoxConfig.BackButtonMode.confirm;
        config.onConfirm = delegate ()
        {
            AuthManager.Instance.SocialLogout(User.LoginType.Weibo);
            onPermissionGranted(false);
        };

        MessageBoxPopup.OpenMessageBox(config);
        */
    };

    AuthManager.Instance.Login(User.LoginType.Weibo, new PermissionType[] { PermissionType.Publish }, delegate (Error error, PermissionType[] grantedPermissions, bool cloudSaveAvailable){
        if (error == null)
        {
            bool permissionGranted = grantedPermissions != null && Array.IndexOf<PermissionType>(grantedPermissions, PermissionType.Publish) >= 0;

            if (permissionGranted)
            {
                onPermissionGranted(true);
            }
            else
            {
                onFailedPermissions();
            }
        }
        else
        {
            if (error.GetType() == typeof(UserAuthError))
            {
                Debug.LogWarning("WeiboSocialSystem (AskForPublishPermission) :: Wrong social account!");

                /*
                //[DGR] No support added yet
                MessageBoxPopup.MessageBoxConfig config = new MessageBoxPopup.MessageBoxConfig();

                config.titleText = "TID_SOCIAL_ERROR_WRONG_ACCOUNT_NAME";
                config.messageText = "TID_SOCIAL_ERROR_WRONG_ACCOUNT_DESC";
                config.messageArgs = new object[] { SocialFacade.GetLocalizedNetworkName(SocialFacade.Network.Weibo)};
                config.onConfirm = delegate ()
                {
                    AuthManager.Instance.SocialLogout(User.LoginType.Weibo);

                    onPermissionGranted(false);
                };
                config.backButtonMode = MessageBoxPopup.MessageBoxConfig.BackButtonMode.confirm;
                config.cancelEnabled = false;
                config.modal = false;
                MessageBoxPopup.OpenMessageBox(config);
                */
            }
            else
        {
            onFailedPermissions();
        }
    }
});
}

public void LogOut(Action onLogout)
{
AuthManager.Instance.Logout(User.LoginType.Weibo, delegate (Error error)
{
    if (error != null)
    {
        Debug.LogWarning("OptionsPopup (OnWeiboToggle) :: Error logging out  - " + error);
    }
    onLogout();
});
}

public void IncentiviseLogin(Action onComplete = null)
{
#if WEIBO
if (!m_socialSaveSystem.WasSocialSystemIncentivised(SocialFacade.Network.Weibo))
{
    if (AuthManager.Instance.IsNetworkAuthenticated(User.LoginType.Weibo, new PermissionType[] { PermissionType.Basic, PermissionType.Friends }))
    {
        m_socialSaveSystem.SetSocialSystemIncentivised(SocialFacade.Network.Weibo);

        string currencySymbol = "";

        switch (m_loginRewardType)
        {
            case Bank.CurrencyType.Coins:
                currencySymbol = "[COINS]";
                App.Instance.Bank.AddCoins(m_loginRewardAmount);
                break;
            case Bank.CurrencyType.Gems:
                currencySymbol = "[GEMS]";
                App.Instance.Bank.AddGems(m_loginRewardAmount);
                break;
            case Bank.CurrencyType.Spins:
                currencySymbol = "[SPINS]";
                App.Instance.Bank.AddSpins(m_loginRewardAmount);
                break;
        }

        SaveFacade.Instance.Save(m_socialSaveSystem.name, true);

        // Report to analytics
        HSXAnalyticsManager.Instance.CurrencyEarned("LoginReward", "Weibo", m_loginRewardType.ToString(), m_loginRewardAmount);

        MessageBoxPopup.MessageBoxConfig config = new MessageBoxPopup.MessageBoxConfig();
        config.titleText = "TID_SOCIAL_LOGIN_COMPLETE_NAME";
        config.messageText = "TID_SOCIAL_LOGIN_COMPLETE_DESC";
        config.messageArgs = new object[] { m_loginRewardAmount, currencySymbol }; //TODO need to replace both value and symbol
        config.onConfirm = delegate ()
        {
            if (onComplete != null)
            {
                onComplete();
            }
        };
        config.backButtonMode = MessageBoxPopup.MessageBoxConfig.BackButtonMode.confirm;
        config.cancelEnabled = false;

        MessageBoxPopup.OpenMessageBox(config);
    }
    else if (onComplete != null)
    {
        onComplete();
    }
}
else
{
    if (onComplete != null)
    {
        onComplete();
    }
}
#else
if (onComplete != null)
{
    onComplete();
}	
#endif
}

public void GetProfileInfo(Action<string> onGetName, Action<Texture2D> onGetImage)
{
string socialID = SocialManager.Instance.GetSocialID(SocialFacade.Network.Weibo);

string WeiboProfileName = PlayerPrefs.GetString(GetProfileNameKey(socialID), null);

bool imageAlreadyRetrieved = false;

if (!string.IsNullOrEmpty(WeiboProfileName))
{
    onGetName(WeiboProfileName);

    Util.StartCoroutineWithoutMonobehaviour("LoadCachedProfileImage", LoadCachedProfileImage(socialID, delegate(Texture2D cachedImage){
        if (cachedImage != null && !imageAlreadyRetrieved)
        {
            onGetImage(cachedImage);
        }
    }));
}

SocialManagerUtilities.CheckConnectionAuth(delegate (SocialManagerUtilities.ConnectionState state)
{
    if (state == SocialManagerUtilities.ConnectionState.OK)
    {
        SocialFacade.Instance.GetProfileInfo(SocialFacade.Network.Weibo, delegate (Dictionary<string, string> profileInfo){
            if (profileInfo != null && profileInfo.ContainsKey("name"))
            {
                PlayerPrefs.SetString(GetProfileNameKey(socialID), profileInfo["name"]);
                PlayerPrefs.Save();

                onGetName(profileInfo["name"]);
            }
            else
            {
                onGetName(null);
            }
        });

        SocialFacade.Instance.GetProfilePicture(SocialFacade.Network.Weibo, socialID, delegate (Texture2D profileImage){
            if (profileImage != null)
            {
                imageAlreadyRetrieved = true;
                File.WriteAllBytes(GetCachedProfileImagePath(socialID), profileImage.EncodeToPNG());
                onGetImage(profileImage);
            }
            else
            {
                onGetImage(null);
            }
        });
    }
    else
    {
        onGetName(null);
        onGetImage(null);
    }
});
}

public void InviteFriends(Action<int> onInviteFriends = null)
{
SocialManagerUtilities.CheckConnectionAuth(delegate (SocialManagerUtilities.ConnectionState state){
    if (state == SocialManagerUtilities.ConnectionState.OK)
    {
        /*
        //[DGR] No support added yet
        GameSettings settings = GameDataManager.Instance.gameDB.GetItem<GameSettings>(GameSettings.KeySettings);

        if (settings != null)
        {
            SocialFacade.Instance.InviteFriends(SocialFacade.Network.Weibo, "TODO", "TODO", settings.facebookShareUrl, settings.facebookShareImageUrl, delegate (string requestID, string[] friends)
            {
                Debug.Log("WeiboSocialSystem (InviteFriends) :: Invite Complete");

                if (onInviteFriends != null)
                {
                    onInviteFriends(0);
                }
            });
        }
        else if (onInviteFriends != null)
        {
            onInviteFriends(-2);
        }*/
    }
    else
    {
        if (onInviteFriends != null)
        {
            onInviteFriends(-1);
        }
    }
});
}

    // [DGR] RULES: No support added yet
    /*
    private void OnGameDBLoaded(Enum eventType, object[] args)
    {
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
    }
    */

    private IEnumerator LoadCachedProfileImage(string socialID, Action<Texture2D> onLoaded)
    {
        WWW cachedImageLoader = new WWW(GetCachedProfileImagePath(socialID, true));

        yield return cachedImageLoader;

        Texture2D cachedImage = new Texture2D(256, 256);

        if (cachedImageLoader.error == null)
        {
            cachedImageLoader.LoadImageIntoTexture(cachedImage);
            onLoaded(cachedImage);
        }
        else
        {
            Debug.LogWarning("WeiboSocialSystem (LoadCachedProfileImage) :: LoadCachedImage failed: " + cachedImageLoader.error);
            onLoaded(null);
        }
    }

    private string GetCachedProfileImagePath(string socialID, bool wwwPath = false)
    {
        return string.Format("{0}{1}/{2}-WBProfileImg.bytes", (wwwPath ? "file://" : ""), Application.temporaryCachePath, socialID);
    }

    private string GetProfileNameKey(string socialID)
    {
        return string.Format("{0}-WBProfileName", socialID);
    }
}
