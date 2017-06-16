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

        public delegate bool OnResponse(string response, int responseCode);
        private OnResponse m_onResponse;

        public bool IsNewAppVersionNeeded { get; set; }

        public GameSessionDelegate(OnResponse onResponse)
        {
            Debug.TaggedLog(tag, "GameSessionDelegate instantiated");
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
                JSONNode response = ServerManager.SharedInstance.GetServerAuthBConfig();
                string responseAsString = null;
                if (response != null)
                {
                    responseAsString = response.ToString();
                }

                m_onResponse(responseAsString, (IsNewAppVersionNeeded) ? 426 : 200);
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

            // An error is sent, just in case the client is waiting for a response for any command
            if (m_onResponse != null)
            {
                m_onResponse(null, 500);
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
        public override void onMergeShowPopupNeeded(CaletyConstants.PopupMergeType eType, JSONNode kLocalAccount, JSONNode kCloudAccount)
        {
            m_waitingMergeResponse = false;            
            Debug.TaggedLog(tag, "onMergeShowPopupNeeded");
        }

        public override void onShowMaintenanceMode()
        {         
            Debug.TaggedLog(tag, "onShowMaintenanceMode");
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
            IsNewAppVersionNeeded = true;
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

        kServerConfig.m_bIsNewVersionRestrictive = false;
        kServerConfig.m_bIsUsingHTTPS = false;

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
        Commands_EnqueueCommand(ECommand.Ping, null,
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
                // We need to logout before if already logged in
                if (GameSessionManager.SharedInstance.IsLogged())
                {
                    LogOut(null);
                }
                
                m_delegate.m_logged = false;
                m_delegate.m_waitingLoginResponse = true;
                m_delegate.IsNewAppVersionNeeded = false;

                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("platformId", platformId);
                parameters.Add("platformUserId", platformUserId);
                parameters.Add("platformToken", platformToken);
                Commands_EnqueueCommand(ECommand.Login, parameters, callback);                
            }
        }       
    }

    public override void LogOut(Action<Error> callback)
    {
        // The response is immediate. We don't want to treat it as a command because it could be trigger at any moment and we don't want it to mess with a command that is being processed
        GameSessionManager.SharedInstance.LogOutFromServer(false);
        if (callback != null)
        {
            callback(null);
        }       
    }

    public override void GetServerTime(Action<Error, Dictionary<string, object>> callback)
    {
        Commands_EnqueueCommand(ECommand.GetTime, null, callback);            
    }

    public override void GetPersistence(Action<Error, Dictionary<string, object>> callback)
    {
        Commands_EnqueueCommand(ECommand.GetPersistence, null, callback);        
    }

    public override void SetPersistence(string persistence, Action<Error, Dictionary<string, object>> callback)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("persistence", persistence);
        Commands_EnqueueCommand(ECommand.SetPersistence, parameters, callback);        
    }

    public override void UpdateSaveVersion(bool prelimUpdate, Action<Error, Dictionary<string, object>> callback)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("fgolID", GameSessionManager.SharedInstance.GetUID());
        parameters.Add("prelimUpdate", prelimUpdate.ToString());
        Commands_EnqueueCommand(ECommand.UpdateSaveVersion, parameters, callback);
    }

    public override void GetQualitySettings(Action<Error, Dictionary<string, object>> callback)     
    {
        Commands_EnqueueCommand(ECommand.GetQualitySettings, null, callback);
    }

    /// <summary>
    /// Uploads the quality settings information calculated by client to the server
    /// </summary>
    /// <param name="qualitySettings">Json in string format of the quality settings to upload</param>    
    public override void SetQualitySettings(string qualitySettings, Action<Error, Dictionary<string, object>> callback)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("qualitySettings", qualitySettings);
        Commands_EnqueueCommand(ECommand.SetQualitySettings, parameters, callback);
    }

    public override void SendPlayTest(bool silent, string playTestUserId, string trackingData, Action<Error, Dictionary<string, object>> callback)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("silent", silent.ToString());
        parameters.Add("playTestUserId", playTestUserId);
        parameters.Add("trackingData", trackingData);
        Commands_EnqueueCommand(ECommand.PlayTest, parameters, callback);                
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
        GetTime,
        GetPersistence,
        SetPersistence,
        UpdateSaveVersion,
        GetQualitySettings,
        SetQualitySettings,
        PlayTest        
    }

    private class Command
    {
        public ECommand Cmd { get; private set; }
        public Dictionary<string, string> Parameters { get; set; }
        public Action<Error, Dictionary<string, object>> Callback { get; private set; }

        public void Reset()
        {
            Cmd = ECommand.None;
            Parameters = null;
            Callback = null;
        }

        public void Setup(ECommand cmd, Dictionary<string, string> parameters, Action<Error, Dictionary<string, object>> callback)
        {
            Cmd = cmd;
            Parameters = parameters;
            Callback = callback;
        }
    }

    /// <summary>
    /// Pool of commands to reduce the impact in memory of sending commands to the server. Every time a <c>Command</c> object is needed we should check this pool first to get the object
    /// and once we're done we have to return the object to this pool
    /// </summary>
    private Queue<Command> Commands_Pool { get; set; }

    private Queue<Command> Commands_Queue { get; set; }
       
    private Command Commands_CurrentCommand { get; set; }         

    public delegate void BeforeCommandComplete(Error error);
    //public delegate void AfterCommand(Command command, Dictionary<string, string> parameters, Error error, Dictionary<string, object> result, Action<Error, Dictionary<string, object>> callback, int retries);

    private void Commands_Init()
    {
        Commands_Pool = new Queue<Command>();
        Commands_Queue = new Queue<Command>();        
    }

    private Command Commands_GetCommand()
    {
        Command returnValue = null;
        if (Commands_Pool.Count == 0)
        {
            returnValue = new Command();
        }
        else
        {
            returnValue = Commands_Pool.Dequeue();
        }

        return returnValue;
    }

    private void Commands_ReturnCommand(Command command)
    {
        command.Reset();

        if (Commands_Pool.Contains(command))
        {
            LogError("This command is already in the pool");
        }
        else
        {            
            Commands_Pool.Enqueue(command);
        }
    }

    private void Commands_EnqueueCommand(ECommand command, Dictionary<string, string> parameters, Action<Error, Dictionary<string, object>> callback)
    {
        Command cmd = Commands_GetCommand();
        cmd.Setup(command, parameters, callback);

        // If no command is being processed and the queue is empty then it's run immeditaly
        if (Commands_CurrentCommand == null && Commands_IsQueueEmpty())
        {
            Commands_CurrentCommand = cmd;
            Commands_PrepareToRunCommand(cmd);
        }
        else
        {
            Commands_Queue.Enqueue(cmd);
        }       
    }

    private Command Commands_DequeueCommand()
    {        
        return Commands_Queue.Dequeue();                
    }

    private bool Commands_IsQueueEmpty()
    {
        return Commands_Queue.Count == 0;
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

    private void Commands_AfterCommand(Command command, Error error, Dictionary<string, object> result, int retries)
    {                       
        //Try and recover from an auth error                    
        if (error != null && error.GetType() == typeof(AuthenticationError) && retries < COMMANDS_MAX_AUTH_RETRIES && command.Cmd != ECommand.Login)
        {
            //Invalidate the session in an attempt to force re-auth
            if (Authenticator.Instance.User != null)
            {
                Log("(AfterCommand) :: Invalidating session");
                Authenticator.Instance.User.InvalidateSession();
            }

            Log(string.Format("(AfterCommand) :: Auth Error Retrying ({0})", retries));
            Commands_PrepareToRunCommand(command, ++retries);
        }
        else if (command.Callback != null)
        {
            Log("Commander Callback :: " + command);
            command.Callback(error, result);
        }                             
    }

    private void Commands_PrepareToRunCommand(Command command, int retries=0)
    {
        Log("PrepareToRunCommand " + command.Cmd);       

        //Make sure we have a valid parameters object as before or after command callbacks may modify it
        if (command.Parameters == null)
        {
            command.Parameters = new Dictionary<string, string>();
        }

        BeforeCommandComplete runCommand = delegate(Error beforeError)
        {
            if (beforeError == null)
            {
                Commands_RunCommand(command, delegate (Error error, Dictionary<string, object> result)                    
                {
                    Commands_AfterCommand(command, error, result, retries);                        
                });
            }
            else if (command.Callback != null)
            {
                command.Callback(beforeError, null);
            }
        };

        Commands_BeforeCommand(command.Cmd, command.Parameters, runCommand);                           
    }   
    
    private void Commands_RunCommand(Command command, Action<Error, Dictionary<string, object>> callback)
    {
        Log("RunCommand " + command.Cmd + " CurrentCommand = " + Commands_CurrentCommand.Cmd);
        // Commands have to be executed one by one since we're not using actions on server side
        if (Commands_CurrentCommand == command)
        {
            Dictionary<string, string> parameters = command.Parameters;
                           
            switch (command.Cmd)
            {
                case ECommand.Ping:
                {                    
                    CaletyExtensions_Ping();
                }
                break;

                case ECommand.GetTime:
                {                 
                    CaletyExtensions_GetTime();
                }
                break;

                case ECommand.Login:
                {
                    Log("Command Login");
                    CaletyExtensions_LogInToServerThruPlatform(parameters["platformId"], parameters["platformUserId"], parameters["platformToken"]);
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

                case ECommand.GetQualitySettings:
                {
                    CaletyExtensions_GetQualitySettings();
                }
                break;

                case ECommand.SetQualitySettings:
                {
                    CaletyExtensions_SetQualitySettings(parameters["qualitySettings"]);
                }
                break;

                case ECommand.PlayTest:
                {
                    bool silent = (parameters["silent"].ToLower() == "true");
                    CaletyExtensions_SendPlayTest(silent, parameters["playTestUserId"], parameters["trackingData"]);
                }
                break;
            }
        }
        else
        {
            LogError("GameServerManagerCalety error: command " + command.Cmd + " can't be executed because command " + Commands_CurrentCommand.Cmd + " is still being processed.");
        }
    }

    private void Commands_OnExecuteCommandDone(Error error, Dictionary<string, object> result)
    {
        Action<Error, Dictionary<string, object>> callback = Commands_CurrentCommand.Callback;
        Commands_ReturnCommand(Commands_CurrentCommand);
        Commands_CurrentCommand = null;
        if (callback != null)
        {
            callback(error, result);
        }

        // If no command is being processed and there's a command enqueued then that command is processed
        // We need to verify that Commands_CurrentCommand is null because the callback called right above might have call another command
        if (Commands_CurrentCommand == null && !Commands_IsQueueEmpty())
        {
            Commands_CurrentCommand = Commands_DequeueCommand();
            Commands_PrepareToRunCommand(Commands_CurrentCommand);
        }
    }

    private bool Commands_OnResponse(string responseData, int statusCode)
    {
        Error error = null;
        Dictionary<string, object> response = null;

        // 426 code means that there's a new version of the application available. We simulate that the response was 200 (SUCCESS) because we don't want to force the
        // user to upgrade        
        bool upgradeAvailable = false;
        if (statusCode == 426)
        {
            statusCode = 200;
            upgradeAvailable = true;
        }

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
            JSONNode result = null;
            if (responseData != null)
            {
                result = SimpleJSON.JSON.Parse(responseData);                
            }

            switch (Commands_CurrentCommand.Cmd)
            {
                case ECommand.Login:
                {                   
                    // [DGR] SERVER: Receive these parameters from server
                    response = new Dictionary<string, object>();
                    response["fgolID"] = GameSessionManager.SharedInstance.GetUID();
                    response["sessionToken"] = GameSessionManager.SharedInstance.GetUserToken();
                    response["authState"] = Authenticator.AuthState.Authenticated.ToString(); //(Authenticator.AuthState)Enum.Parse(typeof(Authenticator.AuthState), response["authState"] as string);                        
                                                                                              //response["authState"] = Authenticator.AuthState.NewSocialLogin.ToString(); //(Authenticator.AuthState)Enum.Parse(typeof(Authenticator.AuthState), response["authState"] as string);                        
                    if (result != null)
                    {
                        string key = "upgradeAvailable";
                        response[key] = upgradeAvailable;

                        key = "cloudSaveAvailable";
                        response[key] = result.ContainsKey(key) && Convert.ToBoolean((object)result[key]);                        
                    }
                }
                break;

                case ECommand.GetTime:
                case ECommand.UpdateSaveVersion:
                {
                    int time = Globals.GetUnixTimestamp();

                    // Checks if the response from server can be interpreted
                    string key = "t";                
                    if (result != null && result.ContainsKey(key))
                    {
                        long timeAsLong = result[key].AsLong;
                        time = (int)(timeAsLong / 1000);                        
                    }

                    // [DGR] SERVER: Receive these parameters from server
                    response = new Dictionary<string, object>();
                    response["dateTime"] = time;
                    response["unixTimestamp"] = time;
                }
                break;

                case ECommand.GetQualitySettings:
                {                
                    response = new Dictionary<string, object>();
                    response["response"] = responseData;

                    // statusCode 204 means that the client has to upload its settings to the server
                    response["upLoadRequired"] = (statusCode == 204);                    
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
            LogWarning(Commands_CurrentCommand.Cmd, error);
        }

        Commands_OnExecuteCommandDone(error, response);
        return error == null;       
    }
    #endregion

    #region calety_extensions
    private static string COMMAND_PING = "/api/server/ping";
    private static string COMMAND_TIME = "/api/server/time";
    private static string COMMAND_GET_PERSISTENCE = "/api/persistence/get";
    private static string COMMAND_SET_PERSISTENCE = "/api/persistence/set";
    private static string COMMAND_GET_QUALITY_SETTINGS = "/api/adq/settings";
    private static string COMMAND_SET_QUALITY_SETTINGS = "/api/quality/upload";
    private static string COMMAND_PLAYTEST_A = "/api/playtest/a";
    private static string COMMAND_PLAYTEST_B = "/api/playtest/b";

    private void CaletyExtensions_Init()
    {
		// All codes need to be handled in order to be sure that the game will continue regardless the network error
		int[] codes = new int[] { 200, 204, 301, 302, 303, 304, 305, 306, 307, 400, 401, 402, 403, 404, 405, 406, 407, 408, 409, 410, 411, 412, 413, 414, 415, 416, 417, 426, 500, 501, 502, 503, 504, 505 };
        NetworkManager.SharedInstance.RegistryEndPoint(COMMAND_PING, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES_NONE, codes, CaletyExtensions_OnPing);
        NetworkManager.SharedInstance.RegistryEndPoint(COMMAND_TIME, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES_NONE, codes, CaletyExtensions_OnGetTime);
        NetworkManager.SharedInstance.RegistryEndPoint(COMMAND_GET_PERSISTENCE, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES_NONE, codes, CaletyExtensions_OnGetPersistenceResponse);
        NetworkManager.SharedInstance.RegistryEndPoint(COMMAND_SET_PERSISTENCE, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES_NONE, codes, CaletyExtensions_OnSetPersistenceResponse);
        NetworkManager.SharedInstance.RegistryEndPoint(COMMAND_GET_QUALITY_SETTINGS, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES_NONE, codes, CaletyExtensions_OnGetQualitySettingsResponse);
        NetworkManager.SharedInstance.RegistryEndPoint(COMMAND_SET_QUALITY_SETTINGS, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnSetQualitySettingsResponse);
        NetworkManager.SharedInstance.RegistryEndPoint(COMMAND_PLAYTEST_A, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnPlayTestResponse);
        NetworkManager.SharedInstance.RegistryEndPoint(COMMAND_PLAYTEST_B, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnPlayTestResponse);
    }    

    private bool CaletyExtensions_OnPing(string strResponse, string strCmd, int iResponseCode)
    {
        return Commands_OnResponse(strResponse, iResponseCode);        
    }

    private bool CaletyExtensions_OnGetTime(string strResponse, string strCmd, int iResponseCode)
    {
        return Commands_OnResponse(strResponse, iResponseCode);        
    }

    private bool CaletyExtensions_OnGetPersistenceResponse(string strResponse, string strCmd, int iResponseCode)
    {
        // [DGR] Server: Default universe
        if (strResponse == "{}")
        {
            SimpleJSON.JSONNode defaultJson = PersistenceManager.GetDefaultDataFromProfile(PersistenceProfile.DEFAULT_PROFILE);
            defaultJson.Add("version", FGOL.Save.SaveGameManager.Instance.Version);
            strResponse = defaultJson.ToString();
        }

        return Commands_OnResponse(strResponse, iResponseCode);                
    }

    private bool CaletyExtensions_OnSetPersistenceResponse(string strResponse, string strCmd, int iResponseCode)
    {
        Log("OnSetPersistenceResponse statusCode=" + iResponseCode);
        return Commands_OnResponse(strResponse, iResponseCode);        
    }

    private bool CaletyExtensions_OnGetQualitySettingsResponse(string strResponse, string strCmd, int iResponseCode)
    {
        Log("OnGetQualitySettingsResponse statusCode=" + iResponseCode);
        return Commands_OnResponse(strResponse, iResponseCode);
    }

    private bool CaletyExtensions_OnSetQualitySettingsResponse(string strResponse, string strCmd, int iResponseCode)
    {
        Log("OnSetQualitySettingsResponse statusCode=" + iResponseCode);
        return Commands_OnResponse(strResponse, iResponseCode);
    }

    private bool CaletyExtensions_OnPlayTestResponse(string strResponse, string strCmd, int iResponseCode)
    {
        Log("OnPlayTestResponse statusCode=" + iResponseCode);
        return Commands_OnResponse(strResponse, iResponseCode);
    }

    private void CaletyExtensions_LogInToServerThruPlatform(string platformId, string platformUserId, string platformToken)
    {        
        Log("CaletyExtensions_LogInToServerThruPlatform");        
        if (!string.IsNullOrEmpty(platformId))
        {
            ServerManager.SharedInstance.SetSocialPlatform(platformId);
        }

        ServerManager.SharedInstance.Server_SendAuth(platformUserId, platformToken);
    }    

    private void CaletyExtensions_GetPersistence()
    {
        if (IsLogged())
        {
            Dictionary<string, string> kParams = new Dictionary<string, string>();
            kParams["uid"] = GameSessionManager.SharedInstance.GetUID();
            kParams["token"] = GameSessionManager.SharedInstance.GetUserToken();
            ServerManager.SharedInstance.SendCommand(COMMAND_GET_PERSISTENCE, kParams);
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
            ServerManager.SharedInstance.SendCommand(COMMAND_SET_PERSISTENCE, kParams);
        }
    }

    private void CaletyExtensions_Ping()
    {        
        ServerManager.SharedInstance.SendCommand(COMMAND_PING);
    }

    private void CaletyExtensions_GetTime()
    {
        ServerManager.SharedInstance.SendCommand(COMMAND_TIME);
    }

    private void CaletyExtensions_GetQualitySettings()
    {        
        // The user is not required to be logged to request the quality settings for her device        
        ServerManager.SharedInstance.SendCommand(COMMAND_GET_QUALITY_SETTINGS);        
    }

    private void CaletyExtensions_SetQualitySettings(string qualitySettings)
    {
        // The user is required to be logged to set its quality settings to prevent anonymous users from messing with the quality settings of other users who have the same device model
        if (IsLogged())
        {
            Dictionary<string, string> kParams = new Dictionary<string, string>();
            kParams["uid"] = GameSessionManager.SharedInstance.GetUID();                        
            ServerManager.SharedInstance.SendCommand(COMMAND_SET_QUALITY_SETTINGS, kParams, qualitySettings);
        }
        else
        {
            LogError("SetQualitySettings require the user to be logged");
        }
    }    

    private void CaletyExtensions_SendPlayTest(bool silent, string playTestUserId, string trackingData)
    {
        string command = (silent) ? COMMAND_PLAYTEST_A : COMMAND_PLAYTEST_B;

        // This endpoint is anonymous but we need to send the playtest user id for tracking purposes
        Dictionary<string, string> kParams = new Dictionary<string, string>();
        kParams["uid"] = playTestUserId;        
        ServerManager.SharedInstance.SendCommand(command, kParams, trackingData);
    }
    #endregion

    #region log
    private const string LOG_CHANNEL = "[GameServerManagerCalety]";
    private void LogWarning(ECommand command, Error error, Exception e = null)
    {        
        Debug.LogWarning(String.Format("{0} Error when sending command {1}: {2}: {3} ({4})", LOG_CHANNEL, command, error.GetType().Name, error.message, error.code));        
        if (e != null)
        {
            Debug.LogWarning(e);
        }

    }

    private void Log(string message)
    {
        Debug.Log(String.Format("{0} {1}", LOG_CHANNEL, message));        
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning(String.Format("{0} {1}", LOG_CHANNEL, message));            
    }

    private void LogError(string message)
    {
        Debug.LogError(String.Format("{0} {1}", LOG_CHANNEL, message));        
    }
    #endregion
}

