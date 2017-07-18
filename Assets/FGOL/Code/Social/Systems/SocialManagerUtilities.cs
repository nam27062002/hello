using FGOL.Server;
using FGOL.Authentication;
using System;

public class SocialManagerUtilities
{
    public enum ConnectionState
    {
        OK,
        NoConnection,
        NoAuth
    }

    public static void CheckConnectionAuth(Action<ConnectionState> onValidConnection)
    {
        PermissionType[] permissions = new PermissionType[] { PermissionType.Basic, PermissionType.Friends };

        GameServerManager.SharedInstance.CheckConnection(
		(Error error, GameServerManager.ServerResponse response) => {
            if (error == null)
            {
                Debug.Log("SocialManagerUtilities (CheckConnectionAuth) :: Internet connection available");

                if (AuthManager.Instance.IsAuthenticated(User.LoginType.Default, permissions))
                {
                    Debug.Log("SocialManagerUtilities (CheckConnectionAuth) :: User authenticated");

                    onValidConnection(ConnectionState.OK);
                }
                else if (AuthManager.Instance.IsPreviouslyAuthenticated())
                {
                    AuthManager.Instance.Authenticate(permissions, delegate (Error authError, PermissionType[] grantedPermissions, bool saveInCloudAvailable) {
                        if (authError == null)
                        {
                            onValidConnection(ConnectionState.OK);
                        }
                        else
                        {
                            Debug.LogWarning("SocialManagerUtilities (CheckConnectionAuth) :: Unable to authenticated user - " + error);
                            onValidConnection(ConnectionState.NoAuth);
                        }
                    }, true);
                }
                else
                {
                    onValidConnection(ConnectionState.NoAuth);
                }
            }
            else
            {
                Debug.LogWarning("SocialManagerUtilities (CheckConnectionAuth) :: No internet connection - " + error);
                onValidConnection(ConnectionState.NoConnection);
            }
        });
    }

    public static SocialFacade.Network GetSocialNetworkFromLoginType(User.LoginType network)
    {
        SocialFacade.Network socialNetwork = SocialFacade.Network.Default;

        switch (network)
        {
            case User.LoginType.Facebook:
                socialNetwork = SocialFacade.Network.Facebook;
                break;
            case User.LoginType.Weibo:
                socialNetwork = SocialFacade.Network.Weibo;
                break;
            case User.LoginType.GameCenter:
                socialNetwork = SocialFacade.Network.GameCenter;
                break;
        }

        return socialNetwork;
    }

    public static User.LoginType GetLoginTypeFromSocialNetwork(SocialFacade.Network network)
    {
        User.LoginType type = User.LoginType.Default;

        switch (network)
        {
            case SocialFacade.Network.Facebook:
                type = User.LoginType.Facebook;
                break;
            case SocialFacade.Network.Weibo:
                type = User.LoginType.Weibo;
                break;
            case SocialFacade.Network.GameCenter:
                type = User.LoginType.GameCenter;
                break;
        }

        return type;
    }

	public static GeoLocation.Location GetGeoLocationFromSocialNetwork(SocialFacade.Network network)
	{
		switch(network)
		{
		case SocialFacade.Network.Weibo:
			return GeoLocation.Location.China;
		default:
			return GeoLocation.Location.Default;
		}
	}
    public static string GetPrefixedSocialID(string userID)
    {
        string prefixedUserID = userID;

        switch (SocialManager.GetSelectedSocialNetwork())
        {
            case SocialFacade.Network.Facebook:
                prefixedUserID = "FB_" + userID;
                break;
            case SocialFacade.Network.Weibo:
                prefixedUserID = "WB_" + userID;
                break;
        };

        return prefixedUserID;
    }

    public static string RemovePrefixSocialID(string userID)
    {
        switch (SocialManager.GetSelectedSocialNetwork())
        {
            case SocialFacade.Network.Facebook:
                if(userID.StartsWith( "FB_") )
                {
                    return userID.Replace( "FB_", "" ) ;
                }                
                break;
            case SocialFacade.Network.Weibo:
                if (userID.StartsWith("WB_"))
                {
                    return userID.Replace("WB_", "");
                }                
                break;
        };

        return userID;
    }
}
