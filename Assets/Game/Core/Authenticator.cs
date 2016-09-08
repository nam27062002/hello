/// <summary>
/// This class is responsible for offering the interface for all stuff related to authentication. This interface satisfies the requirements of the code taken from HSX in order to make the integration
/// easier. This class also hides its implementation, so we could have different implementations for this class and we could decide in the implementation of the method <c>SharedInstance</c> 
/// which one to use. 
/// </summary>
/// 

using FGOL.Authentication;
using FGOL.Server;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Authenticator
{
    #region singleton
    // Singleton ///////////////////////////////////////////////////////////
    private static Authenticator s_pInstance = null;

    public static Authenticator Instance
    {
        get
        {
            if (s_pInstance == null)
            {
                // Calety implementation is used
                s_pInstance = new AuthenticatorCalety();
                s_pInstance.Init();
            }

            return s_pInstance;
        }
    }
    #endregion

    public enum AuthState
    {
        NewUser,
        NewSocialLogin,
        Authenticated,
        Unknown,
        Error
    }

    public class AuthResult
    {
        public string fgolID;
        public string cloudSaveLocation;
        public string cloudSaveBucket;
        public string sessionToken;
        public int sessionExpiry;
        public Dictionary<string, object> cloudCredentials;
        public int cloudCredentialsExpiry;
        public int socialExpiry;
        public AuthState authState;
        public bool upgradeAvailable;
        public bool cloudSaveAvailable;
    }

    private DeviceToken m_deviceToken = null;
    public DeviceToken Token
    {
        get { return m_deviceToken; }
    }

    private User m_user = null;
    public User User
    {
        get { return m_user; }
        set { m_user = value; }
    }

    protected void Init()
    {
        s_pInstance.m_deviceToken = new DeviceToken();
        ExtendedInit();
    }
    protected virtual void ExtendedInit() { }

    public void CheckConnection(Action<Error> callback)
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            ExtendedCheckConnection(callback);
        }
        else
        {
            Debug.Log("Authenticator (CheckConnection) :: InternetReachability NotReachable");
            callback(new ClientConnectionError("InternetReachability NotReachable", ErrorCodes.ClientConnectionError));
        }
    }
    protected virtual void ExtendedCheckConnection(Action<Error> callback) { }
    public virtual void Authenticate(string fgolID, User.LoginCredentials credentials, User.LoginType network, Action<Error, AuthResult> callback) { }    
    public virtual void Logout(Action<Error> callback) { }
    public virtual void UpdateSaveVersion(bool preliminary, Action<Error, int> onUpdate) { }
    public virtual void CheckGameVersion(Action<bool> onCheckComplete) { }
    public virtual void GetServerTime(Action<Error, string, int> onGetServerTime) { }
}
