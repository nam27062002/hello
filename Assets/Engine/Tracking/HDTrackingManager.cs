/// <summary>
/// This class is responsible for handling any Hungry Dragon related stuff needed for tracking. It uses Calety Tracking support to send tracking events
/// </summary>

using UnityEngine;
public class HDTrackingManager
{
    // Singleton ///////////////////////////////////////////////////////////
    private static bool IsEnabled = false;

    private static HDTrackingManager smInstance = null;

    public static HDTrackingManager Instance
    {
        get
        {
            if (smInstance == null)
            {
                if (IsEnabled)
                {
                    smInstance = new HDTrackingManagerImp();
                }
                else
                {
                    smInstance = new HDTrackingManager();
                }
            }

            return smInstance;
        }
    }
    //////////////////////////////////////////////////////////////////////////
        
    public TrackingSaveSystem TrackingSaveSystem { get; set; }
            
    public virtual void Update()
    {        
    }
    
    #region notify
    public virtual void NotifyStartSession()
    {        
    }

    public virtual void NotifyEndSession()
    {       
    }
    #endregion    

    #region log
    public static void Log(string msg)
    {
        Debug.Log("<color=cyan>[HDTrackingManager] " + msg + " </color>");
    }

    public static void LogError(string msg)
    {
        Debug.LogError("[HDTrackingManager] " + msg);
    }
    #endregion
}

