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
	[SerializeField] private GameObject m_unlockWithAddBtn = null;

	// FX
	[Separator("FX")]
	[SerializeField] private ParticleSystem m_unlockFX = null;

	// Internal
	private long m_unlockPricePC = 0;
	private bool m_wasUnlocked = false;	// Lock state in last frame, used to track end of timer

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
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Initialize internal vars
		m_wasUnlocked = isUnlocked;

		// Find out unlock price
		m_unlockPricePC = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings").GetAsLong("miniMapHCCost");

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

			// Ad revive - turn off when offline
			if(m_unlockWithAddBtn != null) {
                m_unlockWithAddBtn.SetActive(Application.internetReachability != NetworkReachability.NotReachable && FeatureSettingsManager.AreAdsEnabled);
			}
		} else {
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