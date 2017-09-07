using FGOL.Server;
using SimpleJSON;
public class PersistenceSyncOpCloud : PersistenceSyncOp
{    
	protected override void ExtendedPerform()
	{
        Step = EStep.CheckingConnection;        
	}

	protected override void ExtendedDiscard() {}

    protected override void ExtendedReset()
    {
        Step = EStep.None;        
    }

    protected override PersistenceStates.ESyncResult ExtendedCalculateResult()
    {
        PersistenceStates.ESyncResult returnValue = Result;
        if (returnValue == PersistenceStates.ESyncResult.Success && Data != null)
        {
            returnValue = base.ExtendedCalculateResult();
        }

        return returnValue;
    }

    private enum EStep
    {
        None,
        CheckingConnection,
        LoggingToServer,
        LoggingToSocial,
        GettingPersistence,
        Done
    }

    private EStep mStep;
    private EStep Step
    {
        get
        {
            return mStep;
        }

        set
        {
            switch (mStep)
            {
                case EStep.LoggingToSocial:                
                    break;
            }

            mStep = value;

            switch (mStep)
            {
                case EStep.CheckingConnection:
                    Server_CheckConnection();
                    break;

                case EStep.LoggingToServer:
                    if (Server_IsLoggedIn())
                    {
                        Step = GetNextStep(Step);
                    }
                    else
                    {
                        Server_LogIn();
                    }
                    break;

                case EStep.LoggingToSocial:
                    Social_AddMergeListeners();

                    Social_IsLogInReady = true;
                    Social_MergeState = ESocialMergeState.Waiting;

                    if (Social_IsLoggedIn())
                    {                        
                        Social_MergeState = ESocialMergeState.Succeeded;                        
                    }
                    else if (!IsSilent) // If it's silent then no explicit login should be done                                           
                    {                        
                        Social_LogIn();                        
                    }
                    break;                

                case EStep.GettingPersistence:
                    Server_GetPersistence();
                    break;

                case EStep.Done:
                    OnPerformDone();
                    break;
            }
        }
    }

    private EStep GetNextStep(EStep step)
    {
        EStep returnValue = EStep.Done;
        switch (step)
        {
            case EStep.CheckingConnection:
                returnValue = EStep.LoggingToServer;
                break;

            case EStep.LoggingToServer:
                returnValue = EStep.LoggingToSocial;
                break;

            case EStep.LoggingToSocial:
                returnValue = EStep.GettingPersistence;
                break;

            case EStep.GettingPersistence:
                returnValue = EStep.Done;
                break;
        }

        return returnValue;
    }

    public override void Update()
    {
        switch (Step)
        {
            case EStep.LoggingToSocial:
                bool isLoggedIn = Social_IsLoggedIn();
                if (Social_IsLogInReady && 
                    (Social_MergeState != ESocialMergeState.Waiting || !isLoggedIn))               
                {
                    if (isLoggedIn)
                    {
                        if (Social_MergeState == ESocialMergeState.Failed)
                        {
                            // If merge fails then no persistence can be retrieved
                            OnDone(PersistenceStates.ESyncResult.Error_Cloud_Server_MergeFailed);
                        }
                        else if (Social_MergeState == ESocialMergeState.ShowPopupNeeeded)
                        {
                            OnDone(PersistenceStates.ESyncResult.Error_Cloud_Server_MergeShowPopupNeeded);
                        }
                        else
                        {
                            Step = GetNextStep(Step);
                        }
                    }    
                    else
                    {
                        OnDone(PersistenceStates.ESyncResult.Error_Cloud_Social_NotLogged);
                    }                                     
                }
                break;
        }
    }

    private void OnDone(PersistenceStates.ESyncResult result)
    {        
        Result = result;
        OnPerformDone();
    }

    #region server
    protected virtual void Server_CheckConnection()
    {
        GameServerManager.SharedInstance.Configure();
        GameServerManager.SharedInstance.CheckConnection((Error _error, GameServerManager.ServerResponse _response) => {
            Server_OnCheckedConnection(_error == null);            
        });
    }

    protected void Server_OnCheckedConnection(bool isConnected)
    {
        if (Step == EStep.CheckingConnection)
        {
            if (isConnected)
            {
                Step = GetNextStep(Step);
            }
            else
            {
                OnDone(PersistenceStates.ESyncResult.Error_Cloud_NotConnection);                
            }
        }
    }

    protected virtual void Server_LogIn()
    {
        GameServerManager.SharedInstance.Auth((Error _error, GameServerManager.ServerResponse _response) => {
            Server_OnLoggedIn(GameServerManager.SharedInstance.IsLoggedIn());
        });
    }

    protected void Server_OnLoggedIn(bool logged)
    {        
        if (Step == EStep.LoggingToServer)
        {
            if (logged)
            {
                Step = GetNextStep(Step);
            }
            else
            {
                OnDone(PersistenceStates.ESyncResult.Error_Cloud_Server_NotLogged);                
            }
        }
    }

    protected virtual bool Server_IsLoggedIn()
    {        
       return GameServerManager.SharedInstance.IsLoggedIn();
    }

    protected virtual void Server_GetPersistence()
    {
        GameServerManager.SharedInstance.GetPersistence(Server_OnGetPersistence);
    }

    protected void Server_OnGetPersistence(Error error, GameServerManager.ServerResponse response)
    {
        bool success = error == null;
        string persistence = null;
        
        if (success)
        {
            persistence = response["response"] as string;
            Data.LoadFromString(persistence);
            OnDone(PersistenceStates.ESyncResult.Success);
        }
        else
        {
            OnDone(PersistenceStates.ESyncResult.Error_Cloud_Server_Persistence);
        }                        
    }
    #endregion

    #region social
    private enum ESocialMergeState
    {
        Waiting,
        Succeeded,
        Failed,
        ShowPopupNeeeded
    }

    private ESocialMergeState Social_MergeState { get; set; }
    private bool Social_IsLogInReady { get; set; }

    private void Social_Init()
    {
        SocialPlatformManager.SharedInstance.Init();        
    }

    protected virtual void Social_LogIn()
    {
        Social_IsLogInReady = false;
        Messenger.AddListener<bool>(GameEvents.SOCIAL_LOGGED, Social_OnLoggedInHelper);
        SocialPlatformManager.SharedInstance.Login();
    }

    private void Social_OnLoggedInHelper(bool logged)
    {
        Messenger.RemoveListener<bool>(GameEvents.SOCIAL_LOGGED, Social_OnLoggedInHelper);
        Social_OnLoggedIn(logged);
    }

    protected void Social_OnLoggedIn(bool logged)
    {
        if (Step == EStep.LoggingToSocial)
        {
            Social_IsLogInReady = true;            
        }
    }

    public virtual bool Social_IsLoggedIn()
    {
        return SocialPlatformManager.SharedInstance.IsLoggedIn();
    }

    private void Social_AddMergeListeners()
    {
        Messenger.AddListener(GameEvents.MERGE_SUCCEEDED, Social_OnMergeSucceeded);
        Messenger.AddListener(GameEvents.MERGE_FAILED, Social_OnMergeFailed);
        Messenger.AddListener<CaletyConstants.PopupMergeType, JSONNode, JSONNode>(GameEvents.MERGE_SHOW_POPUP_NEEDED, Social_OnMergeShowPopupNeeded);
    }

    private void Social_RemoveMergeListeners()
    {
        Messenger.RemoveListener(GameEvents.MERGE_SUCCEEDED, Social_OnMergeSucceeded);
        Messenger.RemoveListener(GameEvents.MERGE_FAILED, Social_OnMergeFailed);
        Messenger.RemoveListener<CaletyConstants.PopupMergeType, JSONNode, JSONNode>(GameEvents.MERGE_SHOW_POPUP_NEEDED, Social_OnMergeShowPopupNeeded);
    }

    private void Social_OnMergeSucceeded()
    {
        Social_MergeState = ESocialMergeState.Succeeded;
    }

    private void Social_OnMergeFailed()
    {
        Social_MergeState = ESocialMergeState.Failed;
    }

    private void Social_OnMergeShowPopupNeeded(CaletyConstants.PopupMergeType eType, JSONNode kLocalAccount, JSONNode kCloudAccount)
    {
        Social_MergeState = ESocialMergeState.ShowPopupNeeeded;

        // Loads the cloud data from here
        Data.LoadFromString(kCloudAccount.ToString());
    }
    #endregion
}

