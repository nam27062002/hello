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

public class FacebookSocialSystem : ISocialSystem
{
    private SocialSaveSystem m_socialSaveSystem = null;
    
    public void Init(SocialSaveSystem socialSaveSystem)
    {        
        m_socialSaveSystem = socialSaveSystem;
    }
    
    public bool IsUser()
    {
        return AuthManager.Instance.IsNetworkPreviouslyAuthenticated(User.LoginType.Facebook);
    }

    public bool IsLoggedIn(PermissionType[] permissions = null)
    {
        if (permissions == null)
        {
            permissions = new PermissionType[] { PermissionType.Basic };
        }

        return AuthManager.Instance.IsNetworkAuthenticated(User.LoginType.Facebook, permissions);
    }

    public void Login(Action<bool> onComplete, bool syncSave, bool repeatAsk = false)
    {
        Log("FacebookSocialSystem Login");
        PermissionType[] requiredPermissions = new PermissionType[] { PermissionType.Basic, PermissionType.Friends };

        bool cloudSavePreviouslyAvailable = Authenticator.Instance.User != null && Authenticator.Instance.User.cloudSaveAvailable;

        AuthManager.Instance.Login(User.LoginType.Facebook, requiredPermissions, delegate (Error error, PermissionType[] grantedPermissions, bool cloudSaveAvailable) {            
            if (error == null)
            {
                Log("AuthManager.Login no error");
                bool friendsPermissionGranted = grantedPermissions != null && Array.IndexOf<PermissionType>(grantedPermissions, PermissionType.Friends) >= 0;                
                if (friendsPermissionGranted)
                {
                    Log(string.Format("FacebookSocialSystem (FacebookLogin) :: Facebook Login successful! CS avail {0}, enabled {1} ", cloudSaveAvailable, SaveFacade.Instance.cloudSaveEnabled));

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
                            SaveFacade.Instance.CloudSaveEnableConfirmation(User.LoginType.Facebook, true, 
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
                    if (!repeatAsk)
                    {
                        SocialFacade.Network network = SocialFacade.Network.Facebook;
                        PersistenceManager.Popups_OpenLoginIncomplete(network, m_socialSaveSystem.WasSocialSystemIncentivised(network), Rules_GetPCAmountToIncentivizeSocial(),
                            delegate ()
                            {
                                Login(onComplete, true, true);
                            },
                            delegate ()
                            {
                                if (onComplete != null)
                                {
                                    onComplete(AuthManager.Instance.IsNetworkAuthenticated(User.LoginType.Facebook, new PermissionType[] { PermissionType.Basic }));
                                }
                            }
                        );
                                              
                        LogWarning("FacebookSocialSystem (FacebookLogin) :: Incomplete login: no permission for retrieving friends given!");
                    }
                    else if (onComplete != null)
                    {
                        onComplete(AuthManager.Instance.IsNetworkAuthenticated(User.LoginType.Facebook, new PermissionType[] { PermissionType.Basic }));
                    }
                }
            }
            else
            {
                LogWarning("FacebookSocialSystem (FacebookLogin) :: Error logging in  - " + error);

                if (!repeatAsk)
                {
                    if (error.GetType() == typeof(UserAuthError))
                    {
                        if ((SaveFacade.Instance.cloudSaveEnabled || cloudSaveAvailable || cloudSavePreviouslyAvailable) && syncSave)
                        {
                            LogWarning("FacebookSocialSystem (FacebookLogin) :: Switching account with cloud save enabled!");

                            PersistenceManager.Popups_OpenSwitchingUserWithCloudSaveEnabled(SocialManager.GetSelectedSocialNetwork(), SocialFacade.Network.Facebook,
                                delegate ()
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
                                },
                                delegate () // OnCancel
                                {
                                    // The user has decided not to log in with a different account so she will just be logged out.
                                    // A message lets the user know that is shown
                                    PersistenceManager.Popups_OpenNoCloudSaveEnabledAnymore(SocialFacade.Network.Facebook,
                                        delegate () {
                                            AuthManager.Instance.SocialLogout(User.LoginType.Facebook);

                                            if (onComplete != null)
                                            {
                                                onComplete(false);
                                            }
                                        }
                                    );                                    
                                }
                            );                           
                        }
                        else
                        {
                            LogWarning("FacebookSocialSystem (FacebookLogin) :: Wrong social account!");                            
                            PersistenceManager.Popups_OpenLoginErrorWrongSocialAccount(SocialFacade.Network.Facebook,
                                delegate ()
                                {
                                    AuthManager.Instance.SocialLogout(User.LoginType.Facebook);

                                    if (onComplete != null)
                                    {
                                        onComplete(false);
                                    }
                                }
                            );                            
                        }
                    }
                    else
                    {        
                        // [DGR] It's a generic error. A popup with a generic error message is shown and the user is allowed to keep playing                                      
                        PersistenceManager.Popups_OpenLoginGenericError(SocialFacade.Network.Facebook, 
                            delegate ()
                            {
                                AuthManager.Instance.SocialLogout(User.LoginType.Facebook);

                                if (onComplete != null)
                                {
                                    onComplete(false);
                                }
                            }
                        );
                        LogWarning("FacebookSocialSystem (FacebookLogin) :: Generic login error");                       
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
        Log("FacebookSocialSystem (AuthenticateFacebook) :: Authenticating Facebook!");

        if (!AuthManager.Instance.IsNetworkAuthenticated(User.LoginType.Facebook))
        {
            AuthManager.Instance.Authenticate(Authenticator.Instance.User.loginCredentials[User.LoginType.Facebook].permissions, delegate (Error error, PermissionType[] grantedPermissions, bool cloudSaveAvailable)
            {
                if (error != null && error.GetType() == typeof(UserAuthError))
                {                    
                    PersistenceManager.Popups_OpenLoginErrorWrongSocialAccount(SocialFacade.Network.Facebook,
                        delegate ()
                        {
                            Log("FacebookSocialSystem (AuthenticateFacebook) :: Wrong facebook account - " + error);
                            if (onComplete != null)
                            {
                                onComplete();
                            }
                        }
                    );                                       
                }
                else
                {
                    Log("FacebookSocialSystem (AuthenticateFacebook) :: Facebook Auth Complete - " + error);
                    if (onComplete != null)
                    {
                        onComplete();
                    }
                }
            }, true);
        }
        else
        {
            Log("OnAuthenticated Success " + onComplete);
            if (onComplete != null)
            {
                onComplete();
            }
        }
    }

    public void AskForPublishPermission(Action<bool> onPermissionGranted)
    {
        Action onFailedPermissions = delegate () {
            PersistenceManager.Popups_OpenPublishPermissionFailed(
                delegate ()
                {
                    AuthManager.Instance.SocialLogout(User.LoginType.Facebook);
                    onPermissionGranted(false);
                }
            );                       
        };

        AuthManager.Instance.Login(User.LoginType.Facebook, new PermissionType[] { PermissionType.Publish }, delegate (Error error, PermissionType[] grantedPermissions, bool cloudSaveAvailable){
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
                    LogWarning("FacebookSocialSystem (AskForPublishPermission) :: Wrong social account!");
                    PersistenceManager.Popups_OpenLoginErrorWrongSocialAccount(SocialFacade.Network.Facebook,
                        delegate ()
                        {
                            AuthManager.Instance.SocialLogout(User.LoginType.Facebook);

                            onPermissionGranted(false);
                        }
                    );                                        
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
        AuthManager.Instance.Logout(User.LoginType.Facebook, delegate (Error error)
        {
            if (error != null)
            {
                LogWarning("OptionsPopup (OnFacebookToggle) :: Error logging out  - " + error);
            }

            onLogout();
        });
    }

    public void IncentiviseLogin(Action onComplete = null)
    {
#if FACEBOOK               
        if (!m_socialSaveSystem.WasSocialSystemIncentivised(SocialFacade.Network.Facebook))
        {
            if (AuthManager.Instance.IsNetworkAuthenticated(User.LoginType.Facebook, new PermissionType[] { PermissionType.Basic, PermissionType.Friends }))
            {
                m_socialSaveSystem.SetSocialSystemIncentivised(SocialFacade.Network.Facebook);
                
                // Gives the reward
                int rewardAmount = Rules_GetPCAmountToIncentivizeSocial();
                UsersManager.currentUser.AddPC(rewardAmount);
                // [DGR] All systems are saves in order to save the reward too
                //SaveFacade.Instance.Save(m_socialSaveSystem.name, true);
                SaveFacade.Instance.Save(null, true);

                // [DGR] ANALITICS Not supported yet
                // Report to analytics
                //HSXAnalyticsManager.Instance.CurrencyEarned("LoginReward", "Facebook", m_loginRewardType.ToString(), m_loginRewardAmount);
                PersistenceManager.Popups_OpenLoginComplete(rewardAmount,
                    delegate ()
                    {
                        if (onComplete != null)
                        {
                            onComplete();
                        }
                    }
                );
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
        string socialID = SocialManager.Instance.GetSocialID(SocialFacade.Network.Facebook);

        string facebookProfileName = PlayerPrefs.GetString(GetProfileNameKey(socialID), null);

        bool imageAlreadyRetrieved = false;

        if (!string.IsNullOrEmpty(facebookProfileName))
        {
            onGetName(facebookProfileName);

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
                SocialFacade.Instance.GetProfileInfo(SocialFacade.Network.Facebook, delegate (Dictionary<string, string> profileInfo){
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

                SocialFacade.Instance.GetProfilePicture(SocialFacade.Network.Facebook, socialID, delegate (Texture2D profileImage){
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
                //[DGR] FRIENDS No support added yet
                /*
                GameSettings settings = GameDataManager.Instance.gameDB.GetItem<GameSettings>(GameSettings.KeySettings);

                if (settings != null)
                {
                    SocialFacade.Instance.InviteFriends(SocialFacade.Network.Facebook, "TODO", "TODO", settings.facebookShareUrl, settings.facebookShareImageUrl, delegate (string requestID, string[] friends)
                    {
                        Log("FacebookSocialSystem (InviteFriends) :: Invite Complete");

                        if (onInviteFriends != null)
                        {
                            onInviteFriends(0);
                        }
                    });
                }
                else if (onInviteFriends != null)
                {
                    onInviteFriends(-2);
                }
                */
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

    private int Rules_GetPCAmountToIncentivizeSocial()    
    {
        return PersistenceManager.Rules_GetPCAmountToIncentivizeSocial();        
    }
    
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
            LogWarning("FacebookSocialSystem (LoadCachedProfileImage) :: LoadCachedImage failed: "+ cachedImageLoader.error);
            onLoaded(null);
        }

    }

    private string GetCachedProfileImagePath(string socialID, bool wwwPath = false)
    {
        return string.Format("{0}{1}/{2}-FBProfileImg.bytes", (wwwPath ? "file://" : ""), Application.temporaryCachePath, socialID);
    }

    private string GetProfileNameKey(string socialID)
    {
        return string.Format("{0}-FBProfileName", socialID);
    }

    #region log
    private const string PREFIX = "FbSocialSystem:";
    private void Log(string message)
    {
        Debug.Log(PREFIX + message);
        Facebook.Unity.FacebookLogger.Info(PREFIX  + message);        
    }

    private void LogWarning(string message)
    {
        Log(message);
        //Debug.LogWarning(PREFIX + message);
    }
    #endregion
}
