﻿/// <summary>
/// This class is responsible for handling any Hungry Dragon related stuff needed for tracking. It uses Calety Tracking support to send tracking events
/// </summary>
/// 

using System.Collections.Generic;
using UnityEngine;

public class HDTrackingManager
{   
    private static HDTrackingManager smInstance = null;

    public static HDTrackingManager Instance
    {
        get
        {
            if (smInstance == null)
            {
                if (FeatureSettingsManager.instance.IsTrackingEnabled)                
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

    public virtual void Init() {}
    public virtual void Destroy() {}

    public virtual string GetTrackingID() { return null; }
    public virtual string GetDNAProfileID() { return null;  }

    //////////////////////////////////////////////////////////////////////////

    public enum EEconomyGroup
    {
		UNKNOWN = -1,

        REMOVE_MISSION,
        SKIP_MISSION,
        UNLOCK_MAP,
        REVIVE,
        UNLOCK_DRAGON,
        BUY_EGG,
        SKIP_EGG_INCUBATION,
        ACQUIRE_DISGUISE,
		SHOP_PC_PACK,
		SHOP_COINS_PACK,
		SHOP_OFFER_PACK,
        SHOP_PROMOTED_IAP,
		SHOP_KEYS_PACK,
        NOT_ENOUGH_RESOURCES,
        INCENTIVISE_SOCIAL_LOGIN,       // Used when the user logs in social platform
        CHEAT,                          // Use this if the currency comes from a cheat so it won't be tracked
        REWARD_CHEST,
        REWARD_GLOBAL_EVENT,
		REWARD_LIVE_EVENT,
        REWARD_MISSION,                 
        REWARD_RUN,                     // Used when the user gets something such as soft currency during a run
		REWARD_AD,						// Reward given by watching an ad
		REWARD_PREREG,					// Reward given from pre-registration
        PET_DUPLICATED,                 // Used when the user gets some reward instead of a pet because the user already has that pet
        SHOP_EXCHANGE,                  // Used when the user exchanges a currency into any other currency such as HC into SC, HC into keys or real money into HC

		GLOBAL_EVENT_KEYS_RESET,		// At the end of the event keys are reset back to 0
		GLOBAL_EVENT_REFUND,            // Used when adding a score to the global event is not possible and the HC spent to duplicate the score needs to be refunded
		GLOBAL_EVENT_BONUS,				// Spend a key to duplicate score registered to a global event at the end of the run

		TOURNAMENT_ENTRY,			    // Tournament Support

        CUSTOMER_SUPPORT,               // Reward received via customer support tool
        SHOP_PURCHASE_RESUMED           // Reward given when resuming a purchase that was interrupted
    };

	public enum EFunnels
	{
		LOAD_GAME = 0
	};

	public enum EActionsMission
	{		
		new_immediate,
		skip_pay,
		skip_ad,
		new_pay,
		new_ad,
		new_mix,
		new_wait,
		done
	};

	public enum EEventMultiplier
	{
		none,
		golden_key,
		ad,
		hc_payment
	};

    public static string EconomyGroupToString(EEconomyGroup group)
    {
        return group.ToString();
    }

	public static EEconomyGroup StringToEconomyGroup(string _str) {
		try {
			return (EEconomyGroup)System.Enum.Parse(typeof(EEconomyGroup), _str);
		} catch(System.ArgumentException) {
			return EEconomyGroup.UNKNOWN;
		}
	}

    // Tracking related data stored in persistence.
    public TrackingPersistenceSystem TrackingPersistenceSystem { get; set; }
    
    public HDTrackingManager()
    {
        SaveOfflineUnsentEventsEnabled = true;
    }

    public virtual void GoToGame() {}
    public virtual void GoToMenu() {}

    public virtual void Update()
    {        
    }

    public bool SaveOfflineUnsentEventsEnabled;

    private float SaveOfflineUnsentEventLastTimestamp;

    public void SaveOfflineUnsentEvents()
    {
        if (SaveOfflineUnsentEventsEnabled)
        {
            float now = Time.realtimeSinceStartup;
            if (now - SaveOfflineUnsentEventLastTimestamp >= FeatureSettingsManager.instance.TrackingStoreUnsentMinTime)
            {
                SaveOfflineUnsentEventLastTimestamp = now;
                SaveOfflineUnsentEventsExtended();
            }
        }
    }

    protected virtual void SaveOfflineUnsentEventsExtended() {}

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


    public enum EMarketingIdFrom
    {
        FirstLoading,
        Settings
    };

    /// <summary>
    /// Notifies marketing id.
    /// </summary>
    /// <param name="from">Where this method is called from</param>
    public virtual void Notify_MarketingID(EMarketingIdFrom from) {}

    /// <summary>
    /// Called when the user starts a round.
    /// </summary>
    /// <param name="dragonXp">Xp of the dragon chosen by the user to play the current round.</param>
    /// <param name="dragonProgression">Progression of the current dragon. It's calculated the same way as playerProgression is but it's done for the dragon chosen by the user to play this round</param>
    /// <param name="dragonSkin">Sku of the skin chosen by the user to play the current round.</param>
    /// <param name="pets">List with the track ids of the pets equipped to play the current round. Null if no pets are equipped.</param>    
    public virtual void Notify_RoundStart(int dragonXp, int dragonProgression, string dragonSkin, List<string> pets) {}

    /// <summary>
    /// Called when the user finishes a round (because of death and not survive or because of quit game).    
    /// </summary>    
    /// <param name="dragonXp">Xp of the dragon chosen by the user to play the current round.</param>
    /// <param name="deltaXp">Dragon xp gained during the whole round.</param>    
    /// <param name="dragonProgression">Progression of the current dragon. It's calculated the same way as playerProgression is but it's done for the dragon chosen by the user to play this round</param>
    /// <param name="timePlayed">Time (in seconds) spent on the round.</param>
    /// <param name="score">Score made in the round.</param>    
    /// <param name="chestsFound">Amount of chests found during the round.</param>
    /// <param name="eggFound">Amount of eggs found during the round.</param>
    /// <param name="highestMultiplier">Highest score multiplier got during the round.</param>
    /// <param name="highestBaseMultiplier">Highest base score multiplier got during the round.</param>
    /// <param name="furyRushNb">Amount of times fury rush has been triggered during the round.</param>
    /// <param name="superFireRushNb">Amount of times superfury rush has been triggered during the round.</param>
    /// <param name="hcRevive">Amount of times the user paid with HC spent to revive the dragon.</param>
    /// <param name="adRevive">Amount of times the user paid by watching an ad to revive her dragon druring the round.</param>
    /// <param name="scGained">Amount of soft currency gained during the round.</param>
    /// <param name="hcGained">Amount of hard currency gained during the round.</param>
	/// <param name="boostTime">Amount of time the player was using boost during the round in seconds.</param>
    /// <param name="mapUsage">Numer of time the player opened the map.</param>
    public virtual void Notify_RoundEnd(int dragonXp, int deltaXp, int dragonProgression, int timePlayed, int score, 
        int chestsFound, int eggFound, float highestMultiplier, float highestBaseMultiplier, int furyRushNb, int superFireRushNb, int hcRevive, int adRevive,
        int scGained, int hcGained, float boostTime, int mapUsage) {}

    /// <summary>
    /// Called when a run finished (because of death or quit game). Remember that a round is composed of at least one run, but it can have more than one if after a run
    /// because of death the user decides to revive so a new run is started.
    /// </summary>
    /// <param name="dragonXp">Xp of the dragon chosen by the user to play the current round.</param>
    /// <param name="timePlayed">Time (in seconds) spent on the round so far.</param>
    /// <param name="score">Current score in the round.</param>
    /// <param name="deathType">Reason why the run ended.</param>
    /// <param name="deathSource">Some death types might have a source such as the sku of the entity that killed the user's dragon, otherwise it must be null.</param>
    /// <param name="deathCoordinates">Coordinates of the map where the user's dragon was when the run ended.</param>
    public virtual void Notify_RunEnd(int dragonXp, int timePlayed, int score, string deathType, string deathSource, Vector3 deathCoordinates) {}

    /// <summary>
    /// Called when the user opens the app store
    /// </summary>
    public virtual void Notify_StoreVisited() {}

    public virtual void Notify_IAPStarted() {}

    /// <summary>
    /// /// Called when the user completed an in app purchase.    
    /// </summary>
    /// <param name="storeTransactionID">transaction ID returned by the platform</param>
    /// <param name="houstonTransactionID">transaction ID returned by houston</param>
    /// <param name="itemID">ID of the item purchased</param>
    /// <param name="promotionType">Promotion type if there is one, otherwise <c>null</c></param>
    /// <param name="moneyCurrencyCode">Code of the currency that the user used to pay for the item</param>
    /// <param name="moneyPrice">Price paid by the user in her currency</param>
    /// <param name="moneyUSD">Price paid by the user in cents of dollar</param>
    /// <param name="isOffer"><c>true</c> if it's an offer. <c>false</c> otherwise</param>
    public virtual void Notify_IAPCompleted(string storeTransactionID, string houstonTransactionID, string itemID, string promotionType, string moneyCurrencyCode, float moneyPrice, int moneyUSD, bool isOffer) {}

    /// <summary>
    /// Called when the user completed a purchase by using game resources (either soft currency or hard currency)
    /// </summary>
    /// <param name="economyGroup">ID used to identify the type of item the user has bought. Example UNLOCK_DRAGON</param>
    /// <param name="itemID">ID used to identify the item that the user bought. Example: sku of the dragon unlocked</param>
    /// <param name="promotionType">Promotion type if there is one, otherwise <c>null</c></param>
    /// <param name="moneyCurrencyCode">Currency type used</param>
    /// <param name="moneyPrice">Amount of the currency paid</param>
    /// <param name="amountBalance">Amount of this currency after the transaction was performed</param>
    public virtual void Notify_PurchaseWithResourcesCompleted(EEconomyGroup economyGroup, string itemID, string promotionType, 
        UserProfile.Currency moneyCurrencyCode, int moneyPrice, int amountBalance) {}

    /// <summary>
    /// Called when the user earned some resources
    /// </summary>
    /// <param name="economyGroup">ID used to identify the type of item the user has earned. Example UNLOCK_DRAGON</param>        
    /// <param name="moneyCurrencyCode">Currency type earned</param>
    /// <param name="amountDelta">Amount of the currency earned</param>
    /// <param name="amountBalance">Amount of this currency after the transaction was performed</param>
    public virtual void Notify_EarnResources(EEconomyGroup economyGroup, UserProfile.Currency moneyCurrencyCode, int amountDelta, int amountBalance) {}


    /// <summary>
    /// Called when the user clicks on the button to request a customer support ticked
    /// </summary>
    public virtual void Notify_CustomerSupportRequested() {}

    /// <summary>
    /// Called when an ad has been requested by the user. 
    /// <param name="adType">Ad Type.</param>
    /// <param name="rewardType">Type of reward given for watching the ad.</param>
    /// <param name="adIsAvailable"><c>true</c>c> if the ad is available, <c>false</c> otherwise.</param>
    /// <param name="provider">Ad Provider. Optional.</param>    
    /// </summary>
    public virtual void Notify_AdStarted(string adType, string rewardType, bool adIsAvailable, string provider=null) {}

    /// <summary>
    /// Called when the ad requested by the user has finished
    /// <param name="adType">Ad Type.</param>    
    /// <param name="adIsLoaded"><c>true</c>c> if the ad was effectively viewed, <c>false</c> otherwise.</param>
    /// <param name="maxReached"><c>true</c> if the user has reached the limit of ad viewing authorized by the app. Used for reward ads</param>
    /// <param name="adViewingDuration">Duration in seconds of the ad viewing.</param>
    /// <param name="provider">Ad Provider. Optional.</param>    
    /// </summary>
    public virtual void Notify_AdFinished(string adType, bool adIsLoaded, bool maxReached, int adViewingDuration=0, string provider=null) {}

    /// <summary>
    /// Called when Menu has been loaded
    /// </summary>
    public virtual void Notify_MenuLoaded() {}

    /// <summary>
    /// The game has reached a step in the loading funnel according to the sequence defined for Calety
    /// </summary>
    /// <param name="_step">Step to notify.</param>
    public virtual void Notify_Calety_Funnel_Load(FunnelData_Load.Steps _step) {}

    /// <summary>
    /// The game has reached a step in the loading funnel according to the sequence defined for Razolytics 
    /// </summary>
    /// <param name="_step">Step to notify.</param>
    public virtual void Notify_Razolytics_Funnel_Load(FunnelData_LoadRazolytics.Steps _step) {}    
    
    /// <summary>
    /// The game has reached a step in the firts user experience funnel.
    /// </summary>
    /// <param name="_step">Step to notify.</param>
    public virtual void Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps _step) {}

    /// <summary>
    /// The user has logged in the social platform.
    /// </summary>    
    public virtual void Notify_SocialAuthentication() {}

    /// <summary>
    /// Notifies the consent popup display.
    /// </summary>
    public virtual void Notify_ConsentPopupDisplay(bool _sourceSettings) { }

    /// <summary>
    /// Notifies the consent popup accept.
    /// </summary>
    /// <param name="_age">Age.</param>
    /// <param name="_enableAnalytics">If set to <c>true</c> enable analytics.</param>
    /// <param name="_enableMarketing">If set to <c>true</c> enable marketing.</param>
    /// <param name="_modVersion">Mod version.</param>
    /// <param name="_duration">Duration.</param>
    public virtual void Notify_ConsentPopupAccept(int _age, bool _enableAnalytics, bool _enableMarketing, string _modVersion, int _duration) {}


    /// <summary>
    /// The user has closed the legal popup.
    /// </summary>
    public virtual void Notify_LegalPopupClosed(int duration, bool hasBeenAccepted) {}

	/// <summary>
	/// The user got a new Pet!
	/// </summary>
	/// <param name="_sku">Pet sku.</param>
	/// <param name="_source">Where did the Pet come from?.</param>
	public virtual void Notify_Pet(string _sku, string _source) {}

    /// <summary>
    /// Notifies the dragon unlocked.
    /// </summary>
    /// <param name="dragon_sku">Dragon sku.</param>
	/// <param name="order">Dragon oder</param>
    public virtual void Notify_DragonUnlocked( string dragon_sku, int order ) {}

    /// <summary>
    /// Notifies the loading gameplay started.
    /// </summary>
	public virtual void Notify_LoadingGameplayStart(){}

	/// <summary>
	/// Notifies the loading gameplay end.
	/// </summary>
	/// <param name="loading_duration">Loading duration in seconds.</param>
	public virtual void Notify_LoadingGameplayEnd(  float loading_duration ){}

    /// <summary>
    /// Notifies the loading area start.
    /// </summary>
    /// <param name="original_area">Original area.</param>
    /// <param name="destination_area">Destination area.</param>
    public virtual void Notify_LoadingAreaStart( string original_area, string destination_area ){}

    /// <summary>
    /// Notifies the loading area end.
    /// </summary>
    /// <param name="original_area">Original area.</param>
    /// <param name="destination_area">Destination area.</param>
	/// <param name="area_loading_duration">Duration in seconds.</param>
	public virtual void Notify_LoadingAreaEnd( string original_area, string destination_area, float area_loading_duration ){}

	/// <summary>
	/// The player has opened an info popup.
	/// </summary>
	/// <param name="_popupName">Name of the opened popup. Prefab name.</param>
	/// <param name="_action">How was this popup opened? One of "automatic", "info_button" or "settings".</param>
	public virtual void Notify_InfoPopup(string _popupName, string _action) {}

	/// <summary>
	/// Notifies the missions.
	/// </summary>
	/// <param name="_mission">Mission.</param>
	/// <param name="_action">Action.</param>
	public virtual void Notify_Missions(Mission _mission, EActionsMission _action) {}

	/// <summary>
	/// Notifies the settings open. When settings popup opens
	/// </summary>
	public virtual void Notify_SettingsOpen(){}

	/// <summary>
	/// Notifies the settings close. When settings popup closed
	/// </summary>
	public virtual void Notify_SettingsClose(){}

    /// <summary>
    /// Notifies the start of performance track every X seconds
    /// </summary>
    public virtual void Notify_StartPerformanceTracker() {}

    /// <summary>
    /// Notifies the stop of performance track every X seconds
    /// </summary>
    public virtual void Notify_StopPerformanceTracker() {}


    public virtual void Notify_GlobalEventRunDone(int _eventId, string _eventType, int _runScore, int _score, EEventMultiplier _mulitplier) {}

	public virtual void Notify_GlobalEventReward(int _eventId, string _eventType, int _rewardTier, int _score, bool _topContributor) {}

	public virtual void Notify_Hacker(){}

    public enum EPopupSurveyAction
    {
        Yes,
        No,
        Later
    };

    public virtual void Notify_PopupSurveyShown(EPopupSurveyAction action) {}        

    public enum EPopupUnsupportedDeviceAction
    {
        Shown,
        Quit,
        Leave2HSE
    };

    public virtual void Notify_PopupUnsupportedDeviceAction(EPopupUnsupportedDeviceAction action) {}
    public virtual void Notify_DeviceStats() { }

    public virtual void Notify_HungryLetterCollected() {}

    public virtual void Notify_Crash(bool isFatal, string errorType, string errorMessage) {}

    /// <summary>
    /// Notifies that the user has seen an offer.
    /// </summary>
    /// <param name="onDemand"><c>true</c> the user has requested to see the offer by clicking on UI.<c>false</c> the user is prompted with the offer automatically.</param>
    /// <param name="itemID">Id of the item offered to the user, typically the sku of the item in shopPacksDefinitions.</param>
    public virtual void Notify_OfferShown(bool onDemand, string itemID) {}

    public virtual void Notify_EggOpened() {}

    /// <summary>
    /// Called when the user clicks on tournament button on main screen
    /// <param name="tournamentSku">Sku of the currently available tournament</param>
    /// </summary>
    public virtual void Notify_TournamentClickOnMainScreen(string tournamentSku) {}

    /// <summary>
    /// Called when the user clicks on next button on tournament description screen
    /// </summary>
    /// <param name="tournamentSku">Sku of the currently available tournament</param>
    public virtual void Notify_TournamentClickOnNextOnDetailsScreen(string tournamentSku) {}

    /// <summary>
    /// Called when the user clickes on enter tournament button
    /// </summary>
    /// <param name="tournamentSku">Sku of the currently available tournament</param>
    /// <param name="currency"><c>NONE</c> if the tournament is for free, otherwise the currency name used to enter the tournament</param>
    public virtual void Notify_TournamentClickOnEnter(string tournamentSku, UserProfile.Currency currency) {}

    public enum ERateThisAppResult
    {
        Yes,
        No,
        Later
    };

    public virtual void Notify_RateThisApp(ERateThisAppResult result, int dragonProgression) {}
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
        Debug.LogError(LOG_CHANNEL + msg);
    }
	#endregion
}

