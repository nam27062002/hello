/// <summary>
/// This class is responsible for handling any Hungry Dragon related stuff needed for tracking. It uses Calety Tracking support to send tracking events
/// </summary>

using UnityEngine;
public class HDTrackingManager
{
    // Singleton ///////////////////////////////////////////////////////////
    private static bool IsEnabled = true;

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

    public virtual void NotifyStartRound(int playerProgress)
    {

    }

    /// <summary>
    /// This method is called when the user finishes a round
    /// </summary>
    /// <param name="playerProgress">An int value that sums up the user's progress</param>
    public virtual void NotifyEndRound(int playerProgress)
    {

    }
    #endregion    

    #region log
    private const bool LOG_USE_COLOR = true;
    private const string LOG_CHANNEL = "[HDTrackingManager] ";
    private const string LOG_CHANNEL_COLOR = "<color=cyan>" + LOG_CHANNEL;

    public static void Log(string msg)
    {        
        if (LOG_USE_COLOR)
        {
            Debug.Log(LOG_CHANNEL_COLOR + msg + " </color>");
        }
        else
        {
            Debug.Log(LOG_CHANNEL + msg);
        }
    }

    public static void LogError(string msg)
    {
        Debug.LogError("[HDTrackingManager] " + msg);
    }
    #endregion
}

