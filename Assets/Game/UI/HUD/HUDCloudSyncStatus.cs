// HUDCloudSyncStatus.cs
// Hungry Dragon
// 
// Created by David Germade on 12nd September 2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

using System;
using UnityEngine;

/// <summary>
/// This class is responsible for handling the widget shown in hud in different
/// screens to let the user know whether or not the local persistence is in sync with the cloud persistence
/// Possible states:
/// 1) Invisible: Cloud Save feature not available for this player (i.e. Underage)
/// 2) Incentivise: The player hasn't logged in the social platform since she installed the game.
/// 3) Not logged in: The player has logged in to the social platform at some point, but logged out afterwards
/// 4) Not in sync
/// 5) In sync
/// </summary>
public class HUDCloudSyncStatus : MonoBehaviour
{
	//------------------------------------------------------------------------//
	// CONSTANTS AND STATIC MEMBERS											  //
	//------------------------------------------------------------------------//
	private enum State {
		CLOUD_SAVE_DISABLED,
		NEVER_LOGGED_IN,
		PREVIOUSLY_LOGGED_IN,
		LOGGED_IN_NOT_SYNCHED,
		LOGGED_IN_SYNCHED
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private GameObject m_root;
	[SerializeField] private GameObject m_syncSuccessIcon = null;
	[SerializeField] private GameObject m_syncFailedSprite = null;
	[SerializeField] private GameObject m_loginRewardRoot = null;
	[SerializeField] private Localizer m_loginRewardText = null;

	private State m_state = State.NEVER_LOGGED_IN;
	private State m_lastDisplayedState = State.NEVER_LOGGED_IN;

	// Spam prevention and other control vars
	private bool m_isPopupOpen = false;
	private float m_spamPreventionTimer = 0f;
	private bool IsClickEnabled {
		get { return m_spamPreventionTimer <= 0f; }
		set {
			// Reset spam prevention
			if(value) {
				m_spamPreventionTimer = 1f;
			} else {
				m_spamPreventionTimer = 0f;
			}
		}
	}

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake()
    {
		// It shouldn't be visible by default
		m_state = State.CLOUD_SAVE_DISABLED;
        RefreshView(true);
        
		// Subscribe to external events
        Messenger.AddListener<bool>(MessengerEvents.PERSISTENCE_SYNC_CHANGED, OnSyncChange);        

		// Simulate a sync status change for initialization
        OnSyncChange(PersistenceFacade.instance.Sync_IsSynced);

		// Initialize some static visual elements
		if(m_loginRewardText != null) {
			PersistenceFacade.Texts_LocalizeIncentivizedSocial(m_loginRewardText);
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
    void OnDestroy()
    {
        Messenger.RemoveListener<bool>(MessengerEvents.PERSISTENCE_SYNC_CHANGED, OnSyncChange);
    }

	/// <summary>
	/// Called every frame.
	/// </summary>
    void Update()
    {
		RefreshState();
        RefreshView(false);

		// Update spam prevention
		if(m_spamPreventionTimer > 0f) {
			m_spamPreventionTimer -= Time.unscaledDeltaTime;
		}
    }

	/// <summary>
	/// 
	/// </summary>
	public void RefreshState() {
		// Is Cloud Save enabled for this player?
		// [AOC] IsCloudSaveAllowed returns false when unable to login with the server, so I don't think it's usable in this case
		//bool isEnabled = PersistenceFacade.instance.IsCloudSaveAllowed && PersistenceFacade.instance.IsCloudSaveEnabled;
		bool isEnabled = SocialPlatformManager.SharedInstance.GetIsEnabled() && PersistenceFacade.instance.IsCloudSaveEnabled;
		if(!isEnabled) {
			// No! Nothing else to check
			m_state = State.CLOUD_SAVE_DISABLED;
			return;
		}		

        PersistenceLocalDriver localDriver = PersistenceFacade.instance.LocalDriver;
        if (string.IsNullOrEmpty(localDriver.Prefs_SocialPlatformKey) && localDriver.Prefs_SocialImplicitMergeState != PersistenceCloudDriver.EMergeState.Ok) { 		
		    m_state = State.NEVER_LOGGED_IN;
	    } else {
		    bool isCurrentlyLoggedIn = PersistenceFacade.instance.CloudDriver.IsLoggedIn;
			// Is currently logged in?
			if (isCurrentlyLoggedIn) {
				// Yes! Check sync status
				if (PersistenceFacade.instance.Sync_IsSynced) {
					m_state = State.LOGGED_IN_SYNCHED;
				} else {
					m_state = State.LOGGED_IN_NOT_SYNCHED;
				}
			} else {
				// No!
				m_state = State.PREVIOUSLY_LOGGED_IN;
			}
		}		
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_force"></param>
	private void RefreshView(bool _force) {
		// Skip if state hasn't changed
		if(m_state == m_lastDisplayedState && !_force) return;

		// Store last displayed state
		m_lastDisplayedState = m_state;

		// Root
		if(m_root != null) {
			m_root.SetActive(m_state != State.CLOUD_SAVE_DISABLED);
		}

		// If disabled, nothing else to do
		if(m_state == State.CLOUD_SAVE_DISABLED) return;

		// Sync status icons
		if(m_syncSuccessIcon != null) {
			m_syncSuccessIcon.SetActive(m_state == State.LOGGED_IN_SYNCHED);
		}

		if(m_syncFailedSprite != null) {
			m_syncFailedSprite.SetActive(m_state == State.LOGGED_IN_NOT_SYNCHED);
		}

		// Login Incentivise reward
		// Is incentivised login enabled?
		if(m_loginRewardRoot != null) {
			bool incentivisedLoginEnabled = FeatureSettingsManager.instance.IsIncentivisedLoginEnabled();
			m_loginRewardRoot.SetActive(m_state == State.NEVER_LOGGED_IN && incentivisedLoginEnabled);
		}
    }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="onSyncDone"></param>
	private void Sync_FromSettings(Action onSyncDone)
	{
		PersistenceFacade.Popups_OpenLoadingPopup();

		// Uses the same social platform that is currently in usage since the user can not change social platforms
		// by clicking this icon
		PersistenceFacade.instance.Sync_FromSettings(SocialPlatformManager.SharedInstance.CurrentPlatform_GetId(), PersistenceCloudDriver.ESyncMode.Lite, onSyncDone);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called by the player when the user clicks on this widget
	/// </summary>
	public void OnIconClick() {
		// Avoid spamming
		if(!IsClickEnabled) return;
		IsClickEnabled = false;

		// Depends on state
		switch(m_state) {
			case State.CLOUD_SAVE_DISABLED: {
				return;	// Shouldn't happen
			}       

			case State.NEVER_LOGGED_IN:
			case State.PREVIOUSLY_LOGGED_IN: {
				// Open Settings popup at the Save tab
				PopupController popup = PopupManager.LoadPopup(PopupSettings.PATH);
				PopupSettings settingsPopup = popup.GetComponent<PopupSettings>();
				settingsPopup.SetActiveTab(PopupSettings.Tab.SAVE);
				popup.Open();
			} break;

			case State.LOGGED_IN_SYNCHED: {
				// Show sync status popup
				if(!m_isPopupOpen) {
					m_isPopupOpen = true;

					Action onConfirm = delegate () {
						// Force a sync (if enabled)
						if(PersistenceFacade.instance.IsCloudSaveEnabled) {
							Sync_FromSettings(OnSyncDone);
						} else {
							m_isPopupOpen = false;
						}
					};

					Action onCancel = delegate () {
						m_isPopupOpen = false;
					};

					PersistenceFacade.Popups_OpenCloudSync(onConfirm, onCancel);
				}
			} break;

			case State.LOGGED_IN_NOT_SYNCHED: {
				// Directly trigger the Cloud Sync flow
				Sync_FromSettings(OnSyncDone);
			} break;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="synced"></param>
	private void OnSyncChange(bool synced) {
		// Refresh state and view
		RefreshState();
		RefreshView(true);
	}

	/// <summary>
	/// Cloud sync has finishehd.
	/// </summary>
	private void OnSyncDone() {
		m_isPopupOpen = false;
		PersistenceFacade.Popups_CloseLoadingPopup();
	}
}