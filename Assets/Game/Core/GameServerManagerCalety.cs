/// <summary>
/// This class is responsible for implementing the <c>GameServerManager</c>interface by using Calety.
/// </summary>

using FGOL.Authentication;
using FGOL.Server;
using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;
public class GameServerManagerCalety : GameServerManager
{
    #region listener
    /// Listener /////////////////////////////////////////////////////////////
    private class GameSessionDelegate : GameSessionManager.GameSessionListener
    {
        const string tag = "GameSessionDelegate";

        public bool m_logged = false;

        // WAITING FLAGS
        public bool m_waitingLoginResponse = false;
        public bool m_waitingGetUniverse = false;
        public bool m_waitingMergeResponse = false;

        public SimpleJSON.JSONClass m_lastRecievedUniverse = null;
        public int m_saveDataCounter = 0;

        private Action<string, int> m_onResponse;

        public GameSessionDelegate(Action<string, int> onResponse)
        {
            m_onResponse = onResponse;
        }

        // Triggers when user was succesfully logged into our server
        public override void onLogInToServer()
        {
            Debug.TaggedLog(tag, "onLogInToServer");
            m_waitingLoginResponse = false;            
            if (!m_logged)
            {
                m_logged = true;
                Messenger.Broadcast<bool>(GameEvents.LOGGED, m_logged);
            }

            if (m_onResponse != null)
            {
                m_onResponse(null, 200);
            }
        }

        public override void onLogInFailed()
        {
            Debug.TaggedLog(tag, "onLogInToServer");
            m_waitingLoginResponse = false;

            if (m_onResponse != null)
            {
                m_onResponse(null, 401);
            }
        } 

        // Triggers when logout from our server is called
        public override void onLogOutFromServer()
        {
            Debug.TaggedLog(tag, "onLogOutFromServer");
            m_waitingLoginResponse = false;
            if (m_logged)
            {
                m_logged = false;
                Messenger.Broadcast<bool>(GameEvents.LOGGED, m_logged);
            }

            if (m_onResponse != null)
            {
                m_onResponse(null, 200);
            }
            // no problem, continue playing
        }

        // The GC login is finished after receiving GC token
        public override void onGameCenterAuthenticationFinished()
        {
            Debug.TaggedLog(tag, "onGameCenterAuthenticationFinished");
        }

        // Trying to use GC functionality without being authenticated previously
        public override void onGameCenterNotAuthenticatedException()
        {
            Debug.TaggedLog(tag, "onGameCenterNotAuthenticatedException");
        }

        // The user cancelled the GC login or iOS is returning a cancelled GC state
        public override void onGameCenterAuthenticationCancelled()
        {
            Debug.TaggedLog(tag, "onGameCenterAuthenticationCancelled");
        }

        // There are merge conflicts and asks to show the merging popup
        public override void onMergeShowPopupNeeded()
        {
            m_waitingMergeResponse = false;
            Debug.TaggedLog(tag, "onMergeShowPopupNeeded");
        }

        // Probably not needed anywhere, but useful for test cases (actually implemented in unit tests)
        public override void onMergeSucceeded()
        {
            m_waitingMergeResponse = false;
            Debug.TaggedLog(tag, "onMergeSucceeded");
        }

        // Probably not needed anywhere, but useful for test cases (actually implemented in unit tests)
        public override void onMergeFailed()
        {
            m_waitingMergeResponse = false;
            Debug.TaggedLog(tag, "onMergeFailed");
        }

        // The user has requested a password to do a cross platform merge
        public override void onMergeXPlatformPass(string pass)
        {
            Debug.TaggedLog(tag, "onMergeXPlatformPass " + pass);
        }

        // Probably not needed anywhere, but useful for test cases (actually implemented in unit tests)
        public override void onMergeXPlatformSucceeded(string platform, string platformUserId)
        {
            Debug.TaggedLog(tag, "onMergeXPlatformSucceeded");
        }

        // Probably not needed anywhere, but useful for test cases (actually implemented in unit tests)
        public override void onMergeXPlatformFailed()
        {
            Debug.TaggedLog(tag, "onMergeXPlatformFailed");
        }

        // Notify the game that a new version of the app is released. Show a popup that redirects to the store.
        public override void onNewAppVersionNeeded()
        {
            Debug.TaggedLog(tag, "onNewAppVersionNeeded");
        }

        // Notify the game that a new version of the app is released. Show a popup that redirects to the store.
        public override void onUserBlackListed()
        {
            Debug.TaggedLog(tag, "onUserBlackListed");
        }

        // Returns current achievements state
        public override void onGetAchievementsInfo(Dictionary<string, GameCenterManager.GameCenterAchievement> kAchievementsInfo)
        {
            Debug.TaggedLog(tag, "onGetAchievementsInfo");
        }

        // Returns current leaderboard score
        public override void onGetLeaderboardScore(string strLeaderboardSKU, int iScore, int iRank)
        {
            Debug.TaggedLog(tag, "onGetLeaderboardScore");
        }

        // Some API needs a restart of the game
        public override void onRequestGameReset()
        {
            Debug.TaggedLog(tag, "onRequestGameReset");
        }                    

        void ResetWaitingFlags()
        {
            m_waitingLoginResponse = false;
            m_waitingGetUniverse = false;
            m_waitingMergeResponse = false;
        }               
    }

    /// End Listener //////////////////////////////////////////////////////////
    #endregion

    private GameSessionDelegate m_delegate;
    
    protected override void ExtendedConfigure()
    {        
        CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");

        // Init server game details
        ServerManager.ServerConfig kServerConfig = new ServerManager.ServerConfig();

        if (settingsInstance != null)
        {
            kServerConfig.m_strServerURL = settingsInstance.m_strLocalServerURL[settingsInstance.m_iBuildEnvironmentSelected];
            kServerConfig.m_strServerPort = settingsInstance.m_strLocalServerPort[settingsInstance.m_iBuildEnvironmentSelected];
            kServerConfig.m_strServerDomain = settingsInstance.m_strLocalServerDomain[settingsInstance.m_iBuildEnvironmentSelected];
            kServerConfig.m_eBuildEnvironment = (CaletyConstants.eBuildEnvironments)settingsInstance.m_iBuildEnvironmentSelected;

            kServerConfig.m_strClientVersion = settingsInstance.GetClientBuildVersion();
        }

#if UNITY_EDITOR
        kServerConfig.m_strClientPlatformBuild = "editor";
#elif UNITY_ANDROID
	    kServerConfig.m_strClientPlatformBuild = "android";
#elif UNITY_IOS
	    kServerConfig.m_strClientPlatformBuild = "ios";
#endif
        kServerConfig.m_strServerApplicationSecretKey = "avefusilmagnifica";

        ServerManager.SharedInstance.Initialise(ref kServerConfig);

        m_delegate = new GameSessionDelegate(Commands_OnResponse);
        GameSessionManager.SharedInstance.SetListener(m_delegate);

        Commands_Init();

        //[DGR] Extra api calls which are needed by Dragon but are not defined in Calety. Maybe they could be added in Calety when it supports offline mode
        CaletyExtensions_Init();
    }

    public override void Ping(Action<Error> callback)
    {
        Commands_PrepareToRunCommand(ECommand.Ping, null,
            delegate (Error error, Dictionary<string, object> response)
            {
                callback(error);
            }
        );
    }

    public override void LogInToServerThruPlatform(string platformId, string platformUserId, string platformToken, Action<Error, Dictionary<string, object>> callback)
    {
        Log("LogInToServerThruPlatform");

        //if (!m_delegate.m_logged)
        {
            if (!m_delegate.m_waitingLoginResponse)
            {
                m_delegate.m_logged = false;
                m_delegate.m_waitingLoginResponse = true;

                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("platformId", platformId);
                parameters.Add("platformUserId", platformUserId);
                parameters.Add("platformToken", platformToken);
                Commands_PrepareToRunCommand(ECommand.Login, parameters, callback);                
            }
        }       
    }

    public override void LogOut(Action<Error> callback)
    {
        Commands_PrepareToRunCommand(ECommand.Logout, null,
            delegate(Error error, Dictionary<string, object> response)
            {
                callback(error);
            }
        );
    }

    public override void GetServerTime(Action<Error, Dictionary<string, object>> callback)
    {        
        Commands_PrepareToRunCommand(ECommand.GetTime, null, callback);            
    }

    public override void GetPersistence(Action<Error, Dictionary<string, object>> callback)
    {
        Commands_PrepareToRunCommand(ECommand.GetPersistence, null, callback);        
    }

    public override void SetPersistence(string persistence, Action<Error, Dictionary<string, object>> callback)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("persistence", persistence);        
        Commands_PrepareToRunCommand(ECommand.SetPersistence, parameters, callback);        
    }

    public override void UpdateSaveVersion(bool prelimUpdate, Action<Error, Dictionary<string, object>> callback)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("fgolID", GameSessionManager.SharedInstance.GetUID());
        parameters.Add("prelimUpdate", prelimUpdate.ToString());        
        Commands_PrepareToRunCommand(ECommand.UpdateSaveVersion, parameters, callback);
    }

    private bool IsLogged()
    {
        return m_delegate.m_logged;
    }

    #region commands
    /// <summary>
    /// Max amount of retries when an authorization error is got after sending a command. If this happens then Auth is tried to sent before resending the command that caused the error
    /// </summary>
    private const int COMMANDS_MAX_AUTH_RETRIES = 1;

    private enum ECommand
    {
        None,
        Ping,
        Login,
        Logout,
        GetTime,
        GetPersistence,
        SetPersistence,
        UpdateSaveVersion
    }
    
    public delegate void BeforeCommandComplete(Error error);    
    //public delegate void AfterCommand(Command command, Dictionary<string, string> parameters, Error error, Dictionary<string, object> result, Action<Error, Dictionary<string, object>> callback, int retries);
   
    private ECommand Commands_CurrentCommand { get; set; }  
    private Action<Error, Dictionary<string, object>> Commands_CurrentCallback { get; set; }

    private void Commands_Init()
    {                
        Commands_Reset();
    }

    private void Commands_Reset()
    {
        Commands_CurrentCommand = ECommand.None;
        Commands_CurrentCallback = null;
    }

    private bool Commands_RequiresAuth(ECommand command)
    {
        return false;
    }

    private void Commands_BeforeCommand(ECommand command, Dictionary<string, string> parameters, BeforeCommandComplete callback)
    {
        Log("BeforeCommand " + command + " requires auth = " + Commands_RequiresAuth(command));
        Error error = null;

        if (Authenticator.Instance.Token != null)
        {
            parameters["deviceToken"] = Authenticator.Instance.Token.ToString();
        }

        parameters["version"] = Globals.GetApplicationVersion();
        parameters["platform"] = Globals.GetPlatform().ToString();

        Action<Error> onAuthed = delegate (Error authError)
        {
            if (authError == null)
            {
                string sessionToken = Authenticator.Instance.User.sessionToken;

                if (sessionToken != null)
                {
                    parameters["fgolID"] = Authenticator.Instance.User.ID;
                    parameters["socialID"] = SocialFacade.Instance.GetSocialIDFromHighestPrecedenceNetwork();
                    parameters["sessionToken"] = sessionToken;
                }
                else
                {
                    error = new AuthenticationError("Invalid Session Token");
                }
            }

            callback(error);
        };

        //ClaimedRewardReceiptCommand is a special command that is a Normal command but needs authentication
        // [DGR] Not needed yet
        //bool checkAuth = command.Name == ClaimedRewardReceiptCommand && Authenticator.Instance.User != null && !string.IsNullOrEmpty(Authenticator.Instance.User.ID);
        bool checkAuth = false;        
        if (Commands_RequiresAuth(command) || checkAuth)
        {
            Log("IsAuthenticated = " + AuthManager.Instance.IsAuthenticated(User.LoginType.Default));
            if (AuthManager.Instance.IsAuthenticated(User.LoginType.Default))
            {
                onAuthed(null);
            }
            else
            {
                LogWarning("(BeforeCommand) :: No authed trying to reauthenticated before command");

                //Try and silently authenticate and continue with request
                AuthManager.Instance.Authenticate(new PermissionType[] { PermissionType.Basic }, delegate (Error authError, PermissionType[] grantedPermissions, bool cloudSaveAvailable)
                {
                    onAuthed(authError);
                }, true);
            }
        }
        else
        {
            Log("Continue");
            callback(null);
        }
    }

    private void Commands_AfterCommand(ECommand command, Dictionary<string, string> parameters, Error error, Dictionary<string, object> result, Action<Error, Dictionary<string, object>> callback, int retries)
    {                       
        //Try and recover from an auth error                    
        if (error != null && error.GetType() == typeof(AuthenticationError) && retries < COMMANDS_MAX_AUTH_RETRIES && command != ECommand.Login)
        {
            //Invalidate the session in an attempt to force re-auth
            if (Authenticator.Instance.User != null)
            {
                Log("(AfterCommand) :: Invalidating session");
                Authenticator.Instance.User.InvalidateSession();
            }

            Log(string.Format("(AfterCommand) :: Auth Error Retrying ({0})", retries));
            Commands_PrepareToRunCommand(command, parameters, callback, ++retries);
        }
        else
        {
            Log("Commander Callback :: " + command);
            callback(error, result);
        }                             
    }

    private void Commands_PrepareToRunCommand(ECommand command, Dictionary<string, string> parameters, Action<Error, Dictionary<string, object>> callback, int retries=0)
    {
        Log("PrepareToRunCommand " + command);
        //Make sure we have a valid parameters object as before or after command callbacks may modify it
        if (parameters == null)
        {
            parameters = new Dictionary<string, string>();
        }

        BeforeCommandComplete runCommand = delegate(Error beforeError)
        {
            if (beforeError == null)
            {
                Commands_RunCommand(command, parameters, delegate (Error error, Dictionary<string, object> result)                    
                {
                    Commands_AfterCommand(command, parameters, error, result, callback, retries);                        
                });
            }
            else
            {
                callback(beforeError, null);
            }
        };

        Commands_BeforeCommand(command, parameters, runCommand);                           
    }   
    
    private void Commands_RunCommand(ECommand command, Dictionary<string, string> parameters, Action<Error, Dictionary<string, object>> callback)
    {
        Log("RunCommand " + command + " CurrentCommand = " + Commands_CurrentCommand);
        // Commands have to be executed one by one since we're not using actions on server side
        if (Commands_CurrentCommand == ECommand.None)
        {
            Commands_CurrentCommand = command;
            Commands_CurrentCallback = callback;

            switch (Commands_CurrentCommand)
            {
                case ECommand.Ping:
                {
                    // [DGR] SERVER: To change for an actual request to the server
                    Commands_OnResponse(null, 200);
                }
                break;

                case ECommand.Login:
                {
                    Log("Command Login");
                    CaletyExtensions_LogInToServerThruPlatform(parameters["platformId"], parameters["platformUserId"], parameters["platformToken"]);
                }
                break;

                case ECommand.Logout:
                {
                    GameSessionManager.SharedInstance.LogOutFromServer(false);                        
                }
                break;

                case ECommand.GetTime:
                {
                    // [DGR] SERVER: To change for an actual request to the server
                    Commands_OnResponse(null, 200);                        
                }
                break;

                case ECommand.GetPersistence:
                {
                    CaletyExtensions_GetPersistence();
                }
                break;

                case ECommand.SetPersistence:
                {
                    CaletyExtensions_SetPersistence(parameters["persistence"]);
                }
                break;

                case ECommand.UpdateSaveVersion:
                {
                    // [DGR] SERVER: To change for an actual request to the server
                    Commands_OnResponse(null, 200);
                }
                break;
            }
        }
        else
        {
            LogError("GameServerManagerCalety error: command " + command + " can't be executed because command " + Commands_CurrentCommand + " is still being processed.");
        }
    }

    private void Commands_OnExecuteCommandDone(Error error, Dictionary<string, object> result)
    {
        // Current command needs to be reseted before calling the callback because we're already done with it command and the callback might trigger another command so we need to reset it first
        // since only one command can be handled simultaneously
        Action<Error, Dictionary<string, object>> callback = Commands_CurrentCallback;
        Commands_Reset();        
        if (callback != null)
        {
            callback(error, result);
        }        
    }

    private void Commands_OnResponse(string responseData, int statusCode)
    {
        Error error = null;
        Dictionary<string, object> response = null;

        int status = (int)Math.Floor(statusCode / 100.0);

        switch (status)
        {
            case 2: // Respose is ok
            {
                // [DGR] SERVER: Receive response
                //error = new InvalidServerResponseError("(WRONG RESPONSE FORMAT) " + responseData);
                //error = new UserAuthError("test");

                    /*if (response.DataAsText != null && response.DataAsText != "")
                    {
                        Dictionary<string, object> result = Json.Deserialize(response.DataAsText) as Dictionary<string, object>;
                        if (result != null)
                        {
                            if (result.ContainsKey("response"))
                            {
                                Dictionary<string, object> expectedResult = result["response"] as Dictionary<string, object>;

                                if (expectedResult != null)
                                {
                                    callback(null, expectedResult);
                                }
                                else
                                {
                                    error = new InvalidServerResponseError("(WRONG RESPONSE FORMAT) " + response.DataAsText);
                                    LogError(url, error);
                                    callback(error, null);
                                }
                            }
                            else if (result.ContainsKey("error"))
                            {
                                Dictionary<string, object> errorJson = result["error"] as Dictionary<string, object>;

                                string errorMessage = errorJson["message"] as string;
                                string errorName = errorJson["name"] as string;

                                //TODO do we still need status?
                                //string errorStatus = errorJson.ContainsKey("status") ? errorJson["status"] as string : string.Empty;

                                ErrorCodes errorCode = ErrorCodes.UnknownError;

                                try
                                {
                                    int errorCodeRaw = errorJson.ContainsKey("code") ? Convert.ToInt32(errorJson["code"]) : -1;

                                    object parsedCode = Enum.ToObject(typeof(ErrorCodes), errorCodeRaw);

                                    if (Enum.IsDefined(typeof(ErrorCodes), parsedCode))
                                    {
                                        errorCode = (ErrorCodes)parsedCode;
                                    }
                                }
                                catch (Exception) { }

                                Dictionary<string, object> errorData = null;

                                if (errorJson.ContainsKey("data"))
                                {
                                    errorData = errorJson["data"] as Dictionary<string, object>;
                                }

                                error = new ServerInternalError(errorMessage, errorName, errorCode);

                                bool logError = true;

                                if (errorName != null)
                                {
                                    switch (errorName)
                                    {
                                        case "AuthError":
                                            error = new AuthenticationError(errorMessage, errorCode);
                                            break;
                                        case "CompatibilityError":
                                            error = new CompatibilityError(errorMessage, errorCode, errorData);
                                            break;
                                        case "UploadDisallowedError":
                                            error = new UploadDisallowedError(errorMessage, errorCode);
                                            logError = false;
                                            break;
                                        case "UserError":
                                            error = new UserAuthError(errorMessage, errorCode);
                                            break;
                                    }
                                }

                                if (logError)
                                {
                                    LogError(url, error);
                                }

                                callback(error, null);
                            }
                            else if (result.ContainsKey("maintenance"))
                            {
                                error = new MaintenanceError();

                                LogError(url, error);
                                callback(error, null);
                            }
                            else
                            {
                                error = new InvalidServerResponseError("(WRONG FORMAT) " + response.DataAsText);
                                LogError(url, error);
                                callback(error, null);
                            }
                        }
                        else
                        {
                            error = new InvalidServerResponseError("(NOT JSON) " + response.DataAsText);
                            LogError(url, error);
                            callback(error, null);
                        }
                    }
                    else
                    {
                        error = new InvalidServerResponseError("(EMPTY RESPONSE)");

                        LogError(url, error);
                        callback(error, null);
                    }*/
                }
            break;
            case 4:
            {
                error = new ClientConnectionError("Status code: " + statusCode);                
            }
            break;
            case 5:
            {
                error = new ServerConnectionError("Status code: " + statusCode);                
            }
            break;
            default:
            {
                error = new UnknownError("Status code: " + statusCode);                
            }
            break;
        }
        
        if (error == null)
        {
            switch (Commands_CurrentCommand)
            {
                case ECommand.Login:
                {
                    // [DGR] SERVER: Receive these parameters from server
                    response = new Dictionary<string, object>();
                    response["fgolID"] = GameSessionManager.SharedInstance.GetUID();
                    response["sessionToken"] = GameSessionManager.SharedInstance.GetUserToken();
                    response["authState"] = Authenticator.AuthState.Authenticated.ToString(); //(Authenticator.AuthState)Enum.Parse(typeof(Authenticator.AuthState), response["authState"] as string);                        
                    response["upgradeAvailable"] = false.ToString(); // response.ContainsKey("upgradeAvailable") && Convert.ToBoolean(response["upgradeAvailable"]);
                    response["cloudSaveAvailable"] = false.ToString(); //Convert.ToBoolean(response["cloudSaveAvailable"]);
                }
                break;

                case ECommand.GetTime:
                case ECommand.UpdateSaveVersion:
                {
                    // [DGR] SERVER: Receive these parameters from server
                    response = new Dictionary<string, object>();
                    response["dateTime"] = Globals.GetUnixTimestamp() + "";
                    response["unixTimestamp"] = Globals.GetUnixTimestamp();
                }
                break;

                default:
                {
                    // [DGR] SERVER: Receive these parameters from server
                    response = new Dictionary<string, object>();
                    response["response"] = responseData;                    
                }
                break;
            }
        }
        else
        {
            LogWarning(Commands_CurrentCommand, error);
        }

        Commands_OnExecuteCommandDone(error, response);        
    }
    #endregion

    #region calety_extensions
    private void CaletyExtensions_Init()
    {
        NetworkManager.SharedInstance.RegistryEndPoint("/api/persistence/get", NetworkManager.EPacketEncryption.E_ENCRYPTION_AES_NONE, new int[] { 200, 500 }, CaletyExtensions_OnGetPersistenceResponse);
        NetworkManager.SharedInstance.RegistryEndPoint("/api/persistence/set", NetworkManager.EPacketEncryption.E_ENCRYPTION_AES_NONE, new int[] { 200, 500, 400 }, CaletyExtensions_OnSetPersistenceResponse);
    }    

    private bool CaletyExtensions_OnGetPersistenceResponse(string strResponse, string strCmd, int iResponseCode)
    {
        // [DGR] Server: Default universe
        if (strResponse == "{}")
        {
            strResponse = "{\"version\":\"0.1.1\"}";
        }

        Commands_OnResponse(strResponse, iResponseCode);       
        return true;
    }

    public bool CaletyExtensions_OnSetPersistenceResponse(string strResponse, string strCmd, int iResponseCode)
    {
        Log("OnSetPersistenceResponse statusCode=" + iResponseCode);
        Commands_OnResponse(strResponse, iResponseCode);       
        return true;
    }

    private void CaletyExtensions_LogInToServerThruPlatform(string platformId, string platformUserId, string platformToken)
    {
        Log("CaletyExtensions_LogInToServerThruPlatform");
        ServerManager.SharedInstance.SetNetworkPlatform(platformId);
        ServerManager.SharedInstance.Server_SendAuth(platformUserId, platformToken);
    }

    private void CaletyExtensions_GetPersistence()
    {
        if (IsLogged())
        {
            Dictionary<string, string> kParams = new Dictionary<string, string>();
            kParams["uid"] = GameSessionManager.SharedInstance.GetUID();
            kParams["token"] = GameSessionManager.SharedInstance.GetUserToken();
            ServerManager.SharedInstance.SendCommand("/api/persistence/get", kParams);
        }
    }

    private void CaletyExtensions_SetPersistence(string persistence)
    {
        if (IsLogged())
        {
            Dictionary<string, string> kParams = new Dictionary<string, string>();
            kParams["uid"] = GameSessionManager.SharedInstance.GetUID();
            kParams["token"] = GameSessionManager.SharedInstance.GetUserToken();
            kParams["universe"] = persistence;
            ServerManager.SharedInstance.SendCommand("/api/persistence/set", kParams);
        }
    }
    #endregion

    #region log
    private const string LOG_CHANNEL = "[GameServerManagerCalety]";
    private void LogWarning(ECommand command, Error error, Exception e = null)
    {
        Facebook.Unity.FacebookLogger.Info(String.Format("{0} Error when sending command {1}: {2}: {3} ({4})", LOG_CHANNEL, command, error.GetType().Name, error.message, error.code));
        /*Debug.LogWarning(String.Format("{0} Error when sending command {1}: {2}: {3} ({4})", LOG_CHANNEL, command, error.GetType().Name, error.message, error.code));        
        if (e != null)
        {
            Debug.LogWarning(e);
        }*/

    }

    private void Log(string message)
    {
        //Debug.Log(String.Format("{0} {1}", LOG_CHANNEL, message));
        Facebook.Unity.FacebookLogger.Info(String.Format("{0} {1}", LOG_CHANNEL, message));
    }

    private void LogWarning(string message)
    {
        //Debug.LogWarning(String.Format("{0} {1}", LOG_CHANNEL, message));    
        Facebook.Unity.FacebookLogger.Info(String.Format("{0} {1}", LOG_CHANNEL, message));
    }

    private void LogError(string message)
    {
        //Debug.LogError(String.Format("{0} {1}", LOG_CHANNEL, message));
        Facebook.Unity.FacebookLogger.Info(String.Format("{0} {1}", LOG_CHANNEL, message));
    }
    #endregion
}

