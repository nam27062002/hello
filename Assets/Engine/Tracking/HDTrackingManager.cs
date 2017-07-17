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
    /// <summary>
    /// Called when the application starts
    /// </summary>
    public virtual void Notify_ApplicationStart() {}

    /// <summary>
    /// Called when the application is closed
    /// </summary>
    public virtual void Notify_ApplicationEnd() {}

    /// <summary>
    /// Called when the application is paused
    /// </summary>
    public virtual void Notify_ApplicationPaused() {}

    /// <summary>
    /// Called when the application is resumed
    /// </summary>
    public virtual void Notify_ApplicationResumed() {}

    /// <summary>
    /// Called when the user starts a round
    /// </summary>
    public virtual void Notify_RoundStart() {}

    /// <summary>
    /// Called when the user finishes a round
    /// </summary>    
    public virtual void Notify_RoundEnd() {}

    /// <summary>
    /// Called when an in-app purchase (with real money) is completed
    /// </summary>
    public virtual void Notify_IAPCompleted(string storeTransactionID, string houstonTransactionID, string itemID, string promotionType) {}
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

