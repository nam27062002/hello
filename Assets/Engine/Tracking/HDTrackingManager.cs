/// <summary>
/// This class is responsible for handling any Hungry Dragon related stuff needed for tracking. It uses Calety Tracking support to send tracking events
/// </summary>

using UnityEngine;
public class HDTrackingManager
{
    // Singleton ///////////////////////////////////////////////////////////
#if UNITY_EDITOR
    // Disabled on editor because ubimobile services crashes on editor when platform is set to iOS
    private static bool IsEnabled = false;
#else
    private static bool IsEnabled = true;
#endif

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

    public enum EEconomyGroup
    {
        REMOVE_MISSION,
        SKIP_MISSION,
        UNLOCK_MAP,
        REVIVE,
        UNLOCK_DRAGON,
        BUY_EGG,
        SKIP_EGG_INCUBATION,
        ACQUIRE_DISGUISE,
        SHOP_COINS_PACK,
        NOT_ENOUGH_RESOURCES
    };

    public static string EconomyGroupToString(EEconomyGroup group)
    {
        return group.ToString();
    }

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
    /// Called when the user opens the app store
    /// </summary>
    public virtual void Notify_StoreVisited() {}

    /// <summary>
    /// /// Called when the user completed an in app purchase.    
    /// </summary>
    /// <param name="storeTransactionID">transaction ID returned by the platform</param>
    /// <param name="houstonTransactionID">transaction ID returned by houston</param>
    /// <param name="itemID">ID of the item purchased</param>
    /// <param name="promotionType">Promotion type if there is one, otherwise <c>null</c></param>
    /// <param name="moneyCurrencyCode">Code of the currency that the user used to pay for the item</param>
    /// <param name="moneyPrice">Price paid by the user in her currency</param>
    public virtual void Notify_IAPCompleted(string storeTransactionID, string houstonTransactionID, string itemID, string promotionType, string moneyCurrencyCode, float moneyPrice) {}

    /// <summary>
    /// Called when the user completed a purchase by using game resources (either soft currency or hard currency)
    /// </summary>
    /// <param name="economyGroup">ID used to identify the type of item the user has bought. Example UNLOCK_DRAGON</param>
    /// <param name="itemID">ID used to identify the item that the user bought. Example: sku of the dragon unlocked</param>
    /// <param name="promotionType">Promotion type if there is one, otherwise <c>null</c></param>
    /// <param name="moneyCurrencyCode">Currency type used</param>
    /// <param name="moneyPrice">Amount of the currency paid</param>
    public virtual void Notify_PurchaseWithResourcesCompleted(EEconomyGroup economyGroup, string itemID, string promotionType, UserProfile.Currency moneyCurrencyCode, int moneyPrice) { }    
    #endregion

    #region log
    private const bool LOG_USE_COLOR = false;
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

