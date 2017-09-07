using FGOL.Server;
using System;
public class PersistenceCloudManager
{
    public void Init()
    {
        Social_Init();
        Load_Init();
        Cloud_Init();
    }

    #region load    
    private enum ELoadStep
    {        
        CheckingConnection,
        LoggingToServer,
        LoggingToSocial,
        GettingPersistence,
        Done
    }

    private ELoadStep m_LoadStep;
    private ELoadStep Load_Step
    {
        get
        {
            return m_LoadStep;
        }

        set
        {
            m_LoadStep = value;

            switch (m_LoadStep)
            {
                case ELoadStep.CheckingConnection:
                    Server_CheckConnection();
                    break;

                case ELoadStep.LoggingToServer:
                    if (Server_IsLoggedIn())
                    {
                        Load_Step = Load_NextStep(Load_Step);
                    }
                    else
                    {
                        Server_LogIn();
                    }
                    break;

                case ELoadStep.LoggingToSocial:
                    if (Social_IsLoggedIn())
                    {
                        Load_Step = Load_NextStep(Load_Step);
                    }
                    else if (Load_IsFromLauncher)
                    {
                        // Launcher requires sylent login so if it's not logged then it's just ignored
                        Load_OnCompleted(false, null);
                    }
                    else
                    {
                        Social_LogIn();
                    }
                    break;

                case ELoadStep.GettingPersistence:
                    Server_GetPersistence();
                    break;
            }
        }
    }

    private bool Load_IsFromLauncher { get; set; }
    private bool Load_IsSilent { get; set; }
    private Action Load_OnDoneListener { get; set; }

    private void Load_Init()
    {
        Load_Step = ELoadStep.Done;
        Load_OnDoneListener = null;
        Load_IsFromLauncher = false;
        Load_IsSilent = false;
    }

    public void Load(bool isFromLauncher, bool isSilent, Action onDone)
    {
        Load_IsFromLauncher = isFromLauncher;
        Load_IsSilent = isSilent;
        Load_OnDoneListener = onDone;
        Load_Step = ELoadStep.CheckingConnection;                
    }

    private void Load_OnCompleted(bool success, string persistence)
    {
        Cloud_State = (success) ? ECloudState.Available : ECloudState.NotAvailable;
        Cloud_IsSaveAllowed = success;

        if (!string.IsNullOrEmpty(persistence))
        {
            Cloud_LoadPersistenceFromString(persistence);
        }

        if (Load_OnDoneListener != null)
        {
            Load_OnDoneListener();
        }

        Load_Init();
    }  
    
    private ELoadStep Load_NextStep(ELoadStep step)
    {
        ELoadStep returnValue = ELoadStep.Done;
        switch (step)
        {
            case ELoadStep.CheckingConnection:
                returnValue = ELoadStep.LoggingToServer;
                break;

            case ELoadStep.LoggingToServer:
                returnValue = ELoadStep.LoggingToSocial;
                break;

            case ELoadStep.LoggingToSocial:
                returnValue = ELoadStep.GettingPersistence;
                break;

            case ELoadStep.GettingPersistence:
                returnValue = ELoadStep.Done;
                break;
        }

        return returnValue;
    }              
    #endregion

    #region cloud
    public enum ECloudState
    {
        Syncing,
        Available,
        NotAvailable
    }

    public ECloudState Cloud_State { get; set; }

    public PersistenceData Cloud_Persistence { get; set; }

    public void Cloud_Save(GameServerManager.ServerCallback callback)
    {
        if (Cloud_IsAvailable() && Cloud_IsSaveAllowed && Cloud_Persistence.LoadState == PersistenceStates.LoadState.OK)
        {
            GameServerManager.SharedInstance.SetPersistence(Cloud_Persistence.ToString(), callback);
        }
    }

    private void Cloud_Init()
    {
        Cloud_State = ECloudState.NotAvailable;
        Cloud_IsSaveAllowed = false;
        Cloud_Persistence = null;
    }

    public bool Cloud_IsAvailable()
    {
        return Cloud_State == ECloudState.Available;
    }

    public bool Cloud_IsSaveAllowed { get; set; }

    public void Cloud_Override(string persistence, GameServerManager.ServerCallback callback)
    {
        Cloud_LoadPersistenceFromString(persistence);
        Cloud_Save(callback);        
    }

    private void Cloud_LoadPersistenceFromString(string persistence)
    {
        /*
        if (Cloud_Persistence == null)
        {
            Cloud_Persistence = new PersistenceData(PersistenceFacade.SharedInstance.LocalPersistence_ActiveProfileID());
        }        

        Cloud_Persistence.LoadFromString(persistence);   
        */     
    }
	#endregion

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
        if (Load_Step == ELoadStep.CheckingConnection)
        {
            if (isConnected)
            {
                Load_Step = Load_NextStep(Load_Step);
            }
            else
            {
                Action onDone = delegate ()
                {
                    Load_OnCompleted(false, null);
                };

                if (Load_IsSilent)
                {
                    //PersistenceFacade.SharedInstance.Popups_OpenErrorConnection(onDone);
                }
                else
                {
                    onDone();
                }
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
        if (Load_Step == ELoadStep.LoggingToServer)
        {
            if (logged)
            {
                Load_Step = Load_NextStep(Load_Step);
            }
            else
            {
                Action onDone = delegate ()
                {
                    Load_OnCompleted(false, null);                    
                };

                if (Load_IsSilent)
                {
                    //PersistenceFacade.SharedInstance.Popups_OpenErrorConnection(onDone);
                }     
                else
                {
                    onDone();
                }           
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
        }

        Load_OnCompleted(success, persistence);     
    }
    #endregion

    #region social
    private void Social_Init()
    {
        SocialPlatformManager.SharedInstance.Init();
    }

    protected virtual void Social_LogIn()
    {        
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
        if (Load_Step == ELoadStep.LoggingToSocial)
        {
            if (logged)
            {
                Load_Step = Load_NextStep(Load_Step);
            }
            else
            {
                Load_OnCompleted(false, null);
            }
        }
    }

    public virtual bool Social_IsLoggedIn()
    {
        return SocialPlatformManager.SharedInstance.IsLoggedIn();
    }
    #endregion    
}