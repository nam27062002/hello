using System.Collections.Generic;
public class PersistenceCloudManagerDebug : PersistenceCloudManager
{
    public PersistenceCloudManagerDebug(string name)
    {
        Name = name;
    }

    public string Name { get; set; }

    public Queue<bool> Server_ForcedCheckConnection { get; set; }
    protected override void Server_CheckConnection()
    {
        if (Server_ForcedCheckConnection != null && Server_ForcedCheckConnection.Count > 0)
        {
            bool logged = Server_ForcedCheckConnection.Dequeue();
            Server_OnCheckedConnection(logged);
        }
        else
        {
            base.Server_CheckConnection();
        }
    }

    public Queue<bool> Server_ForcedLogIn { get; set; }

    protected override void Server_LogIn()
    {        
        if (Server_ForcedLogIn != null && Server_ForcedLogIn.Count > 0)
        {
            bool logged = Server_ForcedLogIn.Dequeue();
            Server_OnLoggedIn(logged);
        }
        else
        {
            base.Server_LogIn();
        }
    }

    public Queue<bool> Social_ForcedLogIn { get; set; }

    protected override void Social_LogIn()
    {
        if (Social_ForcedLogIn != null && Social_ForcedLogIn.Count > 0)
        {
            bool logged = Social_ForcedLogIn.Dequeue();
            Social_OnLoggedIn(logged);
        }
        else
        {
            base.Social_LogIn();
        }
    }
}