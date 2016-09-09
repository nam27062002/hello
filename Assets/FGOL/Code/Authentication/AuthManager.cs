﻿using FGOL.Authentication;
using FGOL.Server;
using FGOL.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

//TODO Check what happens when social login succeeds and auth fails and we try to see if we are already authenticated. We prob want to clear login credentials on failed auth

public class AuthManager : AutoGeneratedSingleton<AuthManager>
{
    public enum LoginState
    {
        LoggedIn,
        LoggedInFriendsPermissionNeeded,
        PreviouslyLoggedIn,
        NeverLoggedIn
    }
    
    #region Private Members
    private User.LoginType[] m_authPrecedence = new User.LoginType[]
    {
        User.LoginType.Facebook,
        User.LoginType.Weibo,
    };

    public User.LoginType FirstAuthPrecedent
    {
        get { return m_authPrecedence[0];  }
    }

    private bool m_upgradeAvailable = false;
#endregion

#region Public Properties
    public bool upgradeAvailable
    {
        get { return m_upgradeAvailable; }
    }
#endregion

#region Public Methods
    public void LoadUser()
    {
        if(Authenticator.Instance.User == null)
        {
            Authenticator.Instance.User = new User();
        }

        Authenticator.Instance.User.Load();
    }
    

    public bool IsAuthenticated(User.LoginType loginType, PermissionType[] permissions = null)
    {
        loginType = loginType == User.LoginType.Default ? SocialManagerUtilities.GetLoginTypeFromSocialNetwork(SocialManager.GetSelectedSocialNetwork()) : loginType;
        SocialFacade.Network socialNetwork = SocialManagerUtilities.GetSocialNetworkFromLoginType(loginType);

        return IsAuthenticated(socialNetwork);
    }

    public bool IsAuthenticated(SocialFacade.Network socialNetwork, PermissionType[] permissions = null)
    {
        socialNetwork = socialNetwork == SocialFacade.Network.Default ? SocialManagerUtilities.GetSocialNetworkFromLoginType(GetAuthenticatedNetwork(permissions)) : socialNetwork;
        return Authenticator.Instance.User != null && Authenticator.Instance.User.IsSessionValid() && IsNetworkAuthenticated(socialNetwork, permissions);
    }

    public bool IsPreviouslyAuthenticated()
    {
        User user = Authenticator.Instance.User;

        return user != null && user.loginCredentials.Count > 0;
    }

    public bool IsNetworkPreviouslyAuthenticated(User.LoginType loginType)
    {
        loginType = loginType == User.LoginType.Default ? SocialManagerUtilities.GetLoginTypeFromSocialNetwork(SocialManager.GetSelectedSocialNetwork()) : loginType;
        User user = Authenticator.Instance.User;

        return user != null && user.loginCredentials.ContainsKey(loginType);
    }
    
    //The below functions are used just to check user is logged into social network 
    public LoginState GetNetworkLoginState(User.LoginType loginType)
    {
        loginType = loginType == User.LoginType.Default ? SocialManagerUtilities.GetLoginTypeFromSocialNetwork(SocialManager.GetSelectedSocialNetwork()) : loginType;
        LoginState state = LoginState.NeverLoggedIn;
        
        if (IsNetworkAuthenticated(loginType, new PermissionType[] { PermissionType.Basic }))
        {
            PermissionType[] grantedPermissions = GetGrantedPermissions(loginType);
            
            //If we don't have the friends permission
            if (Array.IndexOf(grantedPermissions, PermissionType.Friends) == -1)
            {
                state = LoginState.LoggedInFriendsPermissionNeeded;
            }
            //Otherwise we have both
            else
            {
                state = LoginState.LoggedIn;
            }
        }
        else
        {
            if (Authenticator.Instance.User.loginCredentials.ContainsKey(loginType))
            {
                state = LoginState.PreviouslyLoggedIn;
            }
        }
        
        return state;
    }

    public User.LoginType GetAuthenticatedNetwork(PermissionType[] permissions)
    {
        User.LoginType networkAuthenticated = User.LoginType.Default;

        foreach(User.LoginType loginType in m_authPrecedence)
        {
            if(IsNetworkAuthenticated(loginType, permissions))
            {
                networkAuthenticated = loginType;
                break;
            }
        }

        return networkAuthenticated;
    }

    public User.LoginType[] GetAuthenticatedNetworks()
    {
        List<User.LoginType> loginTypes = new List<User.LoginType>();

        foreach(User.LoginType loginType in m_authPrecedence)
        {
            if(IsNetworkAuthenticated(loginType))
            {
                loginTypes.Add(loginType);
            }
        }

        return loginTypes.ToArray();
    }
    
    public PermissionType[] GetGrantedPermissions(User.LoginType loginType)
    {
        loginType = loginType == User.LoginType.Default ? SocialManagerUtilities.GetLoginTypeFromSocialNetwork(SocialManager.GetSelectedSocialNetwork()) : loginType;
        PermissionType[] grantedPermissions = new PermissionType[0];
        
        SocialFacade.Network socialNetwork = SocialManagerUtilities.GetSocialNetworkFromLoginType(loginType);

        User user = Authenticator.Instance.User;

        if(user != null && user.loginCredentials.ContainsKey(loginType))
        {
            if(user.loginCredentials[loginType].isAccessTokenValid && SocialFacade.Instance.IsLoggedIn(socialNetwork))
            {
                grantedPermissions = SocialFacade.Instance.GetGrantedPermissions(socialNetwork);
            }
        }

        return grantedPermissions;
    }

    public bool IsNetworkAuthenticated(User.LoginType loginType, PermissionType[] permissions =null)
    {
        loginType = loginType == User.LoginType.Default ? SocialManagerUtilities.GetLoginTypeFromSocialNetwork(SocialManager.GetSelectedSocialNetwork()) : loginType;
        bool authenticated = false;

        SocialFacade.Network socialNetwork = SocialManagerUtilities.GetSocialNetworkFromLoginType(loginType);

        User user = Authenticator.Instance.User;

        if(user != null && user.loginCredentials.ContainsKey(loginType))
        {
            authenticated = user.loginCredentials[loginType].isAccessTokenValid && SocialFacade.Instance.IsLoggedIn(socialNetwork);

            if(authenticated && permissions != null)
            {
                authenticated &= CheckPermissions(permissions, SocialFacade.Instance.GetGrantedPermissions(socialNetwork));
            }
        }

        return authenticated;
    }

    public bool IsNetworkAuthenticated(SocialFacade.Network socialNetwork, PermissionType[] permissions = null)
    {
        socialNetwork = socialNetwork == SocialFacade.Network.Default ? SocialManager.GetSelectedSocialNetwork() : socialNetwork;
        return IsNetworkAuthenticated(SocialManagerUtilities.GetLoginTypeFromSocialNetwork(socialNetwork), permissions);
    }

    public User.LoginType[] NetworksAuthenticated(PermissionType[] permissions)
    {
        List<User.LoginType> authenticatedNetworks = new List<User.LoginType>();

        foreach(User.LoginType loginType in m_authPrecedence)
        {
            if(IsNetworkAuthenticated(loginType, permissions))
            {
                authenticatedNetworks.Add(loginType);
            }
        }

        return authenticatedNetworks.ToArray();
    }

    public void Login(User.LoginType loginType, PermissionType[] permissions, Action<Error, PermissionType[], bool> onLogin)
    {
        Log("Login");
        loginType = loginType == User.LoginType.Default ? SocialManagerUtilities.GetLoginTypeFromSocialNetwork(SocialManager.GetSelectedSocialNetwork()) : loginType;
        SocialLogin(SocialManagerUtilities.GetSocialNetworkFromLoginType(loginType), permissions, false, delegate(Error loginError, PermissionType[] grantedPermissions)
        {
            Log("OnLogin " + loginError);
            if(loginError == null)
            {
                Log("OnLogin no error");
                if (CheckPermissions(new PermissionType[] { PermissionType.Basic }, grantedPermissions))
                {
                    Log("Auth");
                    Auth(loginType, grantedPermissions, onLogin, false);
                }
                else
                {
                    LogWarning("AuthManager (SocialLogin) :: PermissionError - Basic Permissions were not granted");
                    onLogin(new PermissionError(), grantedPermissions, false);
                }
            }
            else
            {
                LogWarning("AuthManager (Login) :: Login Failed with error - " + loginError);
                onLogin(loginError, null, false);
            }
        });
    }

    public void Authenticate(PermissionType[] permissions, Action<Error, PermissionType[], bool> onAuth, bool silent = false)
    {
		User.LoginType loginType =  SocialManagerUtilities.GetLoginTypeFromSocialNetwork(SocialManager.GetSelectedSocialNetwork());

		Action<Error, PermissionType[]> callback = delegate(Error loginError, PermissionType[] grantedPermissions)
		{
			if(loginError == null)
			{
				if (CheckPermissions(new PermissionType[] { PermissionType.Basic }, grantedPermissions))
				{
					Auth(loginType, grantedPermissions, onAuth, false);
				}
				else
				{
					LogWarning("AuthManager (SocialLogin) :: PermissionError - Basic Permissions were not granted");
					onAuth(new PermissionError(), grantedPermissions, false);
				}
			}
			else
			{
				LogWarning("AuthManager (Login) :: Login Failed with error - " + loginError);
				onAuth(loginError, null, false);
			}
		};

		SocialLogin(SocialManagerUtilities.GetSocialNetworkFromLoginType(loginType), permissions, silent, callback);
    }

    public void SocialLogout(User.LoginType loginType)
    {
        loginType = loginType == User.LoginType.Default ? SocialManagerUtilities.GetLoginTypeFromSocialNetwork(SocialManager.GetSelectedSocialNetwork()) : loginType;
        SocialFacade.Instance.Logout(SocialManagerUtilities.GetSocialNetworkFromLoginType(loginType));
    }

    public void Logout(User.LoginType loginType, Action<Error> onLogout = null)
    {
        loginType = loginType == User.LoginType.Default ? SocialManagerUtilities.GetLoginTypeFromSocialNetwork(SocialManager.GetSelectedSocialNetwork()) : loginType;
        User.LoginType[] remainingNetworks = GetAuthenticatedNetworks();

        if(remainingNetworks.Length == 1 && Array.IndexOf<User.LoginType>(remainingNetworks, loginType) != -1)
        {
            Authenticator.Instance.Logout(delegate(Error error)
            {
                SocialLogout(loginType);

                //We are no longer Removing the credentials from the vector, simply invalidate the data so we keep track if the user has previously logged on. 
				Authenticator.Instance.User.loginCredentials[loginType].Invalidate( );
                Authenticator.Instance.User.sessionToken = null;
                Authenticator.Instance.User.sessionExpiry = 0;

                onLogout(error);

				//	Let the rest of the game knows
				FGOL.Events.EventManager.Instance.TriggerEvent(Events.OnUserLoggedOut, null);
            });
        }
        else
        {
            SocialLogout(loginType);
            onLogout(null);

			//	Let the rest of the game knows
			FGOL.Events.EventManager.Instance.TriggerEvent(Events.OnUserLoggedOut, null);
        }
    }
#endregion

#region Private Methods
    private void SocialLogin(SocialFacade.Network network, PermissionType[] permissions, bool silent, Action<Error, PermissionType[]> onLogin)
    {
        network = network == SocialFacade.Network.Default ? SocialManagerUtilities.GetSocialNetworkFromLoginType(GetAuthenticatedNetwork(permissions)) : network;
        SocialFacade facade = SocialFacade.Instance;

        if(IsNetworkAuthenticated(network, permissions))
        {
            facade.RefreshAuthentication(network, delegate(bool success){
                if (success)
                {
                    onLogin(null, facade.GetGrantedPermissions(network));
                }
                else
                {
                    onLogin(new AuthenticationError("Authentication refresh failed!"), null);
                }
            });
        }
        else if(!silent)
        {
            facade.Login(network, permissions, delegate(bool loggedIn)
            {
                Log("OnLogIn " + loggedIn);
                if(loggedIn)
                {
					PermissionType[] grantedPermissions = facade.GetGrantedPermissions(network);
                    //[DGR] ANALYTICS no support added yet
                    //bool allGranted = CheckPermissions(permissions, grantedPermissions);

                    onLogin(null, grantedPermissions);

                    /*
                    //[DGR] ANALYTICS no support added yet
					// Report analytics
					if(network == SocialFacade.Network.Facebook)
					{
						HSXAnalyticsManager.Instance.SocialPermissionsAskedResult("Facebook", permissions, allGranted);
					}
					if(allGranted)
					{
						HSXAnalyticsManager.Instance.SocialLogin(network.ToString());
                    }
                    */
				}
                else
                {
                    LogWarning("AuthManager (SocialLogin) :: LoginError - Login failed with network: " + network);
                    onLogin(new AuthenticationError("Login failed with network: " + network, ErrorCodes.LoginError), null);
                }
            });
        }
        else if (silent)
        {
            if (facade.IsLoggedIn(network))
            {
                facade.RefreshAuthentication(network, delegate (bool success) {
                    if (success)
                    {
                        PermissionType[] grantedPermissions = facade.GetGrantedPermissions(network);

                        if (CheckPermissions(permissions, grantedPermissions))
                        {
                            onLogin(null, grantedPermissions);
                        }
                        else
                        {
                            onLogin(new PermissionError(null, ErrorCodes.PermissionError), null);
                        }
                    }
                    else
                    {
                        onLogin(new AuthenticationError("Authentication refresh failed!"), null);
                    }
                });
            }
            else
            {
                onLogin(new AuthenticationError(null, ErrorCodes.LoginError), null);
            }
        }
        else
        {
            onLogin(new AuthenticationError(null, ErrorCodes.LoginError), null);
        }
    }

    private void Auth(User.LoginType loginType, PermissionType[] grantedPermissions, Action<Error, PermissionType[], bool> onAuth, bool silent)
    {
        loginType = loginType == User.LoginType.Default ? SocialManagerUtilities.GetLoginTypeFromSocialNetwork(SocialManager.GetSelectedSocialNetwork()) : loginType;
        SocialFacade.Network socialNetwork = SocialManagerUtilities.GetSocialNetworkFromLoginType(loginType);

        User.LoginCredentials loginCredentials = new User.LoginCredentials(
            SocialFacade.Instance.GetSocialID(socialNetwork),
            SocialFacade.Instance.GetAccessToken(socialNetwork),
            -1,
            grantedPermissions
        );

        Action< Authenticator.AuthResult> populateUserCredentials = delegate (Authenticator.AuthResult result)
        {
            User currentUser = Authenticator.Instance.User;
            currentUser.ID = result.fgolID;
            currentUser.cloudSaveLocation = result.cloudSaveLocation;
            currentUser.cloudSaveBucket = result.cloudSaveBucket;
            currentUser.sessionToken = result.sessionToken;
            currentUser.sessionExpiry = result.sessionExpiry;

            currentUser.cloudCredentials.values.Clear();

            if (result.cloudCredentials != null)
            {
                foreach (KeyValuePair<string, object> pair in result.cloudCredentials)
                {
                    currentUser.cloudCredentials.values.Add(pair.Key, pair.Value as string);
                }
            }

            currentUser.cloudCredentials.expiry = result.cloudCredentialsExpiry;

            loginCredentials.expiry = result.socialExpiry;

            currentUser.loginCredentials[loginType] = loginCredentials;
            currentUser.cloudSaveAvailable = result.cloudSaveAvailable;

            currentUser.Save();

			//	Notify rest of the game user is updated
			FGOL.Events.EventManager.Instance.TriggerEvent(Events.OnUserLoggedIn, currentUser);
        };

        Log("Authenticate");
        Authenticator.Instance.Authenticate(Authenticator.Instance.User.ID, loginCredentials, loginType, delegate(Error error, Authenticator.AuthResult result)
        {
            Log("OnAuthenticated " + error);
            if(error == null)
            {
                m_upgradeAvailable = result.upgradeAvailable;

                Log("result.authState = " + result.authState);
                switch(result.authState)
                {
                    case Authenticator.AuthState.Authenticated:
                        //Check if we have logged in with a different account
                        if(!string.IsNullOrEmpty(Authenticator.Instance.User.ID) && Authenticator.Instance.User.ID != result.fgolID)
                        {
                            Log("OnAuth con UserAuthError");
                            onAuth(new UserAuthError("Different User Authenticated"), grantedPermissions, false);
                        }
                        else
                        {
                            populateUserCredentials(result);

                            //Auth with same user we can continue 
                            Log("OnAuth  Success");
                            onAuth(null, grantedPermissions, result.cloudSaveAvailable);
                            if (!silent)
                            {
                                /*
                                //[DGR] PUSH No support added yet
                                HSXServer.Instance.GetPushNotificationsStatus(delegate (Error statusError, bool pushEnabled)
                                {
                                    if (statusError == null)
                                    {
                                        NotificationManager.notificationsEnabled = pushEnabled;
                                    }
                                });
                                */
                            }
                        }
                        break;
                    case Authenticator.AuthState.NewSocialLogin:
                        populateUserCredentials(result);
                        onAuth(null, grantedPermissions, result.cloudSaveAvailable);
                        break;
                    case Authenticator.AuthState.NewUser:
                        populateUserCredentials(result);
                        onAuth(null, grantedPermissions, result.cloudSaveAvailable);
                        break;
                    default:
                        //TODO determine better error message and figure out how we will handle this case
                        onAuth(new AuthenticationError("Unknown Auth Status"), grantedPermissions, false);
                        break;
                }
            }
            else
            {
                LogWarning("AuthManager (Auth) :: Failed to authenticated with error - " + error);
                onAuth(error, grantedPermissions, false);
            }
        });
    }

    private bool CheckPermissions(PermissionType[] required, PermissionType[] granted)
    {
        bool permissionsGranted = true;

        foreach(PermissionType permission in required)
        {
            permissionsGranted &= Array.IndexOf<PermissionType>(granted, permission) >= 0;
        }

        return permissionsGranted;
    }
    #endregion

    #region log
    private const string PREFIX = "AuthManager:";

    private void Log(string message)
    {
        Debug.Log(PREFIX + message);
        Facebook.Unity.FacebookLogger.Info(PREFIX + message);
    }

    private void LogWarning(string message)
    {
        Log(message);
        Debug.LogWarning(PREFIX + message);
    }

    private void LogError(string message)
    {
        Log(message);
        Debug.LogError(PREFIX + message);
    }
    #endregion
}