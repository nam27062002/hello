using System;
using FGOL.Save;
using FGOL.Server;
public class TrackingSaveSystem : SaveSystem
{
    // Tracking user ID generated upon first time session is started, uses GUID as we don't have server at this point
    public string UserID { get; set; }

    // Counter of sessions since installation
    public int SessionCount;				

    public TrackingSaveSystem()
    {
        m_systemName = "Tracking";
        Reset();
    }

    public override void Reset()
    {
        UserID = "";
        SessionCount = 0;
    }

    public override void Load()
    {
        try
        {
            UserID =        GetString("UserID");
            SessionCount =  GetInt("SessionCount", 0);
        }
        catch (Exception e)
        {
            Debug.LogError("TrackingSaveSystem (Load) :: Exception - " + e);
            throw new CorruptedSaveException(e);
        }
    }

    public override void Save()
    {
        SetString("UserID", UserID);
        SetInt("SessionCount", SessionCount);
    }

    public override bool Upgrade()
    {
        return false;
    }

    public override void Downgrade()
    {
    }
}