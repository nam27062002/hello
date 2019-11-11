// GoalsScreenMapPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// UI controller for the map upgrading.
/// Reused in both the goals screen and the in-game map popup.
/// </summary>
public class MapUpgradeController : MonoBehaviour, IBroadcastListener {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    private const string TID_MAP_FREE_UNLOCK_MESSAGE = "TID_MAP_FREE_UNLOCK_MESSAGE";

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed references
    [Separator("Animators")]
	[SerializeField] private ShowHideAnimator m_lockedGroupAnim = null;
	[SerializeField] private ShowHideAnimator m_unlockedGroupAnim = null;


    [Separator("Objects")]
	[SerializeField] private TextMeshProUGUI m_pcPriceText = null;
	[SerializeField] private TextMeshProUGUI m_timerText = null;
	[SerializeField] private GameObject m_unlockWithAdBtn = null;
    [SerializeField] private GameObject m_unlockWithPcBtn = null;
    [SerializeField] private GameObject m_unlockFreeBtn = null;
    [SerializeField] private Localizer m_unlockFreeDescription = null;


    // FX
    [Separator("FX")]
	[SerializeField] private ParticleSystem m_unlockFX = null;

	// Internal
	private long m_unlockPricePC = 0;
	private bool m_wasUnlocked = false;	// Lock state in last frame, used to track end of timer

    // Cached
    private ShowHideAnimator m_unlockFreeBtnAnimator;

	// [AOC] If the map timer runs out during the game, we let the player enjoy the unlocked map for the whole run
	private bool isUnlocked {
		get { return UsersManager.currentUser.mapUnlocked; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.PROFILE_MAP_UNLOCKED, this);

        m_unlockFreeBtnAnimator = m_unlockFreeBtn.GetComponent<ShowHideAnimator>();
    }

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Initialize internal vars
		m_wasUnlocked = isUnlocked;

		// Find out unlock price
		m_unlockPricePC = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings").GetAsLong("miniMapHCCost");

        // Instead of using events and broadcasting, pass a delegate to the remove ads controller
        UsersManager.currentUser.removeAds.refreshMapPill = Refresh;

		// Refresh visuals!
		Refresh(false);
	}

	/// <summary>
	/// Update loop, called every frame.
	/// </summary>
	private void Update() {
		// If unlocked, refresh timer
		if(isUnlocked) {
			m_wasUnlocked = true;
			RefreshTimer();
		}

		// First frame it's locked after the timer run out, refresh (show unlock buttons)
		else if(m_wasUnlocked) {
			m_wasUnlocked = false;
			Refresh(true);

			// Notify other UI elements
			Broadcaster.Broadcast(BroadcastEventType.UI_MAP_EXPIRED);
		}
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
        // Remove the delegate
        UsersManager.currentUser.removeAds.refreshMapPill = null;
    }

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.PROFILE_MAP_UNLOCKED, this);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.PROFILE_MAP_UNLOCKED:
            {
                OnMapUnlocked();
            }break;
        }
    }

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the pill with info of the current map upgrade.
	/// </summary>
	/// <param name="_animate">Whether to animate or not (useful for initialization).</param>
	public void Refresh(bool _animate) {
		// Aux vars
		bool isLocked = !isUnlocked;

		// Groups visibility
		if(m_lockedGroupAnim != null) m_lockedGroupAnim.ForceSet(isLocked, _animate);
		if(m_unlockedGroupAnim != null) m_unlockedGroupAnim.ForceSet(!isLocked, _animate);

		// Depending on lock state
		if(isLocked) {
			// Price tag
			if(m_pcPriceText != null) {
				m_pcPriceText.text = UIConstants.GetIconString(m_unlockPricePC, UserProfile.Currency.HARD, UIConstants.IconAlignment.LEFT);
			}

            // Check if remove ads feature is active
            bool removeAds = UsersManager.currentUser.removeAds.IsActive;

			// Ad unlock - turn off when offline
			if(m_unlockWithAdBtn != null) {
                m_unlockWithAdBtn.SetActive(DeviceUtilsManager.SharedInstance.internetReachability != NetworkReachability.NotReachable && FeatureSettingsManager.AreAdsEnabled && !removeAds);
			}

            // Unlock map without ads
            if (m_unlockFreeBtn != null)
            {
                m_unlockFreeBtnAnimator.ForceSet(removeAds && UsersManager.currentUser.removeAds.IsMapRevealAvailable(), true);

                if (m_unlockFreeDescription != null)
                {
                    int coolDownSeconds = UsersManager.currentUser.removeAds.mapRevealDurationSecs;
                    string cooldownFormatted = TimeUtils.FormatTime(coolDownSeconds, TimeUtils.EFormat.WORDS_WITHOUT_0_VALUES, 2);
                    m_unlockFreeDescription.Localize(TID_MAP_FREE_UNLOCK_MESSAGE, cooldownFormatted);
                }
            }



        }
        else {
			// Timer
			RefreshTimer();
		}
	}

	/// <summary>
	/// Update the timer text.
	/// </summary>
	private void RefreshTimer() {
		// Check required stuff
		if(m_timerText == null) return;

		// Countdown format
		TimeSpan timeToReset = UsersManager.currentUser.mapResetTimestamp - GameServerManager.SharedInstance.GetEstimatedServerTime();
		double seconds = System.Math.Max(timeToReset.TotalSeconds, 0d);	// Never go negative!
		m_timerText.text = TimeUtils.FormatTime(seconds, TimeUtils.EFormat.DIGITS, 3, TimeUtils.EPrecision.HOURS, true);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The unlock with ad button has been pressed.
	/// </summary>
	public void OnUnlockWithAd() {
		// Ignore if map is already unlocked (probably spamming button)
		if(isUnlocked) return;

		// Show video ad!
		PopupAdBlocker.LaunchAd(true, GameAds.EAdPurpose.UPGRADE_MAP, OnVideoRewardCallback);
	}


    /// <summary>
    /// The unlock with free button (remove ads feature) has been pressed.
    /// </summary>
    public void OnUnlockForFree()
    {
        // Ignore if map is already unlocked (probably spamming button)
        if (isUnlocked) return;

        // If map reveal is not ready, get out. 
        if (!UsersManager.currentUser.removeAds.UseMapReveal()) return;

        // Unlock the map
        UsersManager.currentUser.UnlockMap(UsersManager.currentUser.removeAds.mapRevealDurationSecs);
        PersistenceFacade.instance.Save_Request();
    }


    void OnVideoRewardCallback(bool done){
		if ( done ){            
            UsersManager.currentUser.UnlockMap();
            PersistenceFacade.instance.Save_Request();
            Track_UnlockMap(HDTrackingManager.EUnlockType.video_ads);
        }
	}

	/// <summary>
	/// The unlock with PC button has been pressed.
	/// </summary>
	public void OnUnlockWithPC() {
		// Ignore if map is already unlocked (probably spamming button)
		if(isUnlocked) return;

        // Start purchase flow        
        ResourcesFlow purchaseFlow = new ResourcesFlow("UNLOCK_MAP");
        purchaseFlow.OnSuccess.AddListener(
			(ResourcesFlow _flow) => {                
				// Just do it
				UsersManager.currentUser.UnlockMap();
                PersistenceFacade.instance.Save_Request();
                Track_UnlockMap(HDTrackingManager.EUnlockType.HC);
            }
		);
		purchaseFlow.Begin(m_unlockPricePC, UserProfile.Currency.HARD, HDTrackingManager.EEconomyGroup.UNLOCK_MAP, null);
	}

    private void Track_UnlockMap(HDTrackingManager.EUnlockType unlockType) {
        HDTrackingManager.ELocation location = (FlowManager.IsInGameScene()) ? HDTrackingManager.ELocation.game_play : HDTrackingManager.ELocation.main_menu;
        HDTrackingManager.Instance.Notify_UnlockMap(location, unlockType);
    }	

	/// <summary>
	/// The map has been upgraded.
	/// </summary>
	public void OnMapUnlocked() {
		// Trigger FX
		if(m_unlockFX != null) {
			m_unlockFX.Stop();
			m_unlockFX.Clear();
			m_unlockFX.Play();
		}

		// Refresh info after some delay (to sync with animation)
		//DOVirtual.DelayedCall(0.25f, Refresh);
		Refresh(true);
	}
}
