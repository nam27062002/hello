// MissionPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Pill representing a single mission popup.
/// Used both in the pause menu and in the level selection menu, be careful when
/// doing changes in any of them!
/// </summary>
public class MissionPill : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private const string TID_SKIP_FREE = "TID_MISSIONS_SKIP_FREE";
	private const string TID_SKIP_PARTIAL = "TID_MISSIONS_SKIP_PARTIAL";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Setup
	[Separator("Setup")]
	[HideEnumValues(false, true)]
	[SerializeField] private Mission.Difficulty m_missionDifficulty = Mission.Difficulty.EASY;
	[SerializeField] private bool m_showProgressForSingleRunMissions = false;	// It doesn't make sense to show progress for single-run missions in the menu

	// References - keep references to objects that are often accessed
	[Separator("Active State")]
	[SerializeField] private GameObject m_activeObj = null;
	[Space]
	[SerializeField] private BaseIcon m_missionIcon = null;
    [SerializeField] private TextMeshProUGUI m_missionDescriptionText = null;
	[SerializeField] private TextMeshProUGUI m_rewardText = null;
	[SerializeField] private GameObject m_missionCompletedObj = null;
	[Space]
	[SerializeField] private GameObject m_progressGroup = null;
	[SerializeField] private TextMeshProUGUI m_progressText = null;
	[SerializeField] private Slider m_progressBar = null;
	[Space]
	[SerializeField] private Localizer m_removeCostText = null;
	[SerializeField] private GameObject m_removeFreeButton = null;
	[SerializeField] private GameObject m_removePaidButton = null;
    [Space]
    [SerializeField] private GameObject m_targetZone = null;
	[SerializeField] private Localizer m_targetZoneText = null;
	[SerializeField] private Localizer m_targetDragonText = null;
	
	[Separator("Cooldown State")]
	[SerializeField] private GameObject m_cooldownObj = null;
	[Space]
	[SerializeField] private Localizer m_cooldownInfoText = null;
	[SerializeField] private TextMeshProUGUI m_cooldownTimeText = null;
	[SerializeField] private Slider m_cooldownBar = null;
	[Space]
	[SerializeField] private Localizer m_cooldownSkipFreeText = null;
	[SerializeField] private Localizer m_cooldownSkipPaidText = null;

	[Separator("Other Refs")]
	[SerializeField] private Localizer m_difficultyText = null;

	// Data
	private Mission m_mission = null;
	public Mission mission {
		get {
			m_mission = MissionManager.GetMission(m_missionDifficulty);
			return m_mission;
		}
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	private void Awake() {
		// Check required references
		Debug.Assert(m_cooldownObj != null, "Required reference!");
		Debug.Assert(m_activeObj != null, "Required reference!");

		// Subscribe to external events
		Messenger.AddListener<Mission>(MessengerEvents.MISSION_REMOVED, OnMissionRemoved);
		Messenger.AddListener<Mission, Mission.State, Mission.State>(MessengerEvents.MISSION_STATE_CHANGED, OnMissionStateChanged);
		if(FeatureSettingsManager.IsControlPanelEnabled) {
			Messenger.AddListener(MessengerEvents.DEBUG_REFRESH_MISSION_INFO, DEBUG_OnRefreshMissionInfo);
		}
		Messenger.AddListener<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, OnEventFinished);
	}

	/// <summary>
	/// Default destructor.
	/// </summary>
	private void OnDestroy() {
		if (ApplicationManager.IsAlive) {
			// Unsubscribe from external events
			Messenger.RemoveListener<Mission> (MessengerEvents.MISSION_REMOVED, OnMissionRemoved);
			Messenger.RemoveListener<Mission, Mission.State, Mission.State> (MessengerEvents.MISSION_STATE_CHANGED, OnMissionStateChanged);
			if (FeatureSettingsManager.IsControlPanelEnabled) {
				Messenger.RemoveListener (MessengerEvents.DEBUG_REFRESH_MISSION_INFO, DEBUG_OnRefreshMissionInfo);
			}
			Messenger.RemoveListener<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, OnEventFinished);
		}
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Detect hot language changes
		Broadcaster.AddListener(BroadcastEventType.LANGUAGE_CHANGED, this);

		// Make sure we're up to date
		Refresh();
	}
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.LANGUAGE_CHANGED:
            {
                OnLanguageChanged();
            }break;
        }
    }
    

	private void OnDisable() {
		// Only detect hot language changes while active
		Broadcaster.RemoveListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Update time-dependant fields
		if(mission != null) {
            if (mission.state == Mission.State.COOLDOWN) {
                RefreshCooldownTimers();
            }
            if(mission.updated) {
                Refresh();
            }
		}
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initializes the pill using the given mission. Will overwrite the missionDifficulty
	/// property.
	/// </summary>
	/// <param name="_mission">The mission to displayed in this pill.</param>
	public void InitFromMission(Mission _mission) {
		// Easy!
		m_mission = _mission;
		if(m_mission != null) m_missionDifficulty = m_mission.difficulty;
		Refresh();
	}

	/// <summary>
	/// Update the pill with the data from the target mission.
	/// </summary>
	public void Refresh() {
		// Make sure mission is valid
		if(mission == null) return;

		// Select which object should be visible
		m_cooldownObj.SetActive(m_mission.state == Mission.State.COOLDOWN || m_mission.state == Mission.State.ACTIVATION_PENDING);
		m_activeObj.SetActive(m_mission.state == Mission.State.ACTIVE);

		// Update visuals
		switch(m_mission.state) {
			case Mission.State.COOLDOWN: 			RefreshCooldown(); 			break;
			case Mission.State.ACTIVATION_PENDING: 	RefreshActivationPending(); break;
			case Mission.State.ACTIVE: 				RefreshActive(); 			break;
		}

		// Shared stuff
		// Shared mission difficulty text
		RefreshDifficulty(m_difficultyText, true);

        // Refresh zone
        RefreshZone(m_targetZone, m_targetZoneText, mission.objective.zone);

        mission.updated = false;
	}

	/// <summary>
	/// Refresh active state object.
	/// </summary>
	private void RefreshActive() {
		// Mission description
		m_missionDescriptionText.text = m_mission.objective.GetDescription();

		// Progress
		// Optionally hide progress for singlerun missions
		bool show = !m_mission.objective.singleRun || m_showProgressForSingleRunMissions;
		m_progressGroup.SetActive(show);
		if(show) {
			m_progressText.text = m_mission.objective.GetProgressString();
			m_progressBar.value = m_mission.objective.progress;
		}

        UIConstants.IconType icon = UIConstants.IconType.NONE;
        switch (m_mission.reward.currency) {
            case UserProfile.Currency.SOFT:             icon = UIConstants.IconType.COINS;              break;
            case UserProfile.Currency.GOLDEN_FRAGMENTS: icon = UIConstants.IconType.GOLDEN_FRAGMENTS;   break;
        }
		// Reward
		m_rewardText.text = UIConstants.GetIconString(m_mission.reward.amount, icon, UIConstants.IconAlignment.LEFT);

		// Remove cost
		// [AOC] The pill might not have it (e.g. in-game pill)
		if(m_removeCostText!= null) {
			m_removeCostText.Localize(m_removeCostText.tid, StringUtils.FormatNumber(m_mission.removeCostPC));
		}

		// Check if this mission is complete
		if(m_missionCompletedObj != null) m_missionCompletedObj.SetActive(m_mission.objective.isCompleted);

        // Get the icon definition
        string iconSku = m_mission.def.GetAsString("icon");

        // The BaseIcon component will load the proper image or 3d model according to iconDefinition.xml
        m_missionIcon.LoadIcon(iconSku);
        m_missionIcon.gameObject.SetActive(true);



		// With
		// [AOC] TODO!! Feature not yet implemented, use a fixed text for now
		if(m_targetDragonText != null) {
			m_targetDragonText.Localize("TID_MISSIONS_WITH_ANY_DRAGON");
		}

		// Difficulty
		RefreshDifficulty(m_difficultyText, true);

		// Buttons
		RefreshRemovePayButtons();
	}

	/// <summary>
	/// 
	/// </summary>
	private void RefreshRemovePayButtons()
	{
		// Check if ads availables to skip mission
		bool canPayWithAds = CanPayRemoveMissionWithAds();

		// Don't allow removing during tutorial
		bool ftux = false;
		if(m_mission != null && m_mission.def != null) {
			ftux = m_mission.def.sku.Contains("ftux");
		}

		if(m_removeFreeButton != null) {
			m_removeFreeButton.SetActive(!ftux && canPayWithAds);
		}

		if(m_removePaidButton != null) {
			m_removePaidButton.SetActive(!ftux && !canPayWithAds);
		}
	}

	/// <summary>
	/// Determines whether the user can pay to remove this mission with ads.
	/// </summary>
	/// <returns><c>true</c> if the user can pay to remove the mission with ads; otherwise, <c>false</c>.</returns>
	private bool CanPayRemoveMissionWithAds()
	{
		bool checkVideoIsReady = true;
		DefinitionNode _def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
        if (_def != null)
        {
			int usesPerDay = _def.GetAsInt("dailyAdsRemoveMissions");
			if ( usesPerDay > 0 )
			{
				// Check remaining uses
				if ( GameServerManager.SharedInstance.GetEstimatedServerTime() >= UsersManager.currentUser.dailyRemoveMissionAdTimestamp )
				{
					UsersManager.currentUser.dailyRemoveMissionAdTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTime().AddDays(1);
					UsersManager.currentUser.dailyRemoveMissionAdUses = 0;
				}
				if (UsersManager.currentUser.dailyRemoveMissionAdUses >= usesPerDay) 
				{
					checkVideoIsReady = false;
				}
			}
        }

		bool ret = false;
        if ( checkVideoIsReady && GameAds.adsAvailable) 
        {
			ret = true;
        }
       
		return ret;
	}

	/// <summary>
	/// Refresh the cooldown state object.
	/// </summary>
	private void RefreshCooldown() {
		// Update the timers
		RefreshCooldownTimers();

		// Info text
		m_cooldownInfoText.Localize("TID_MISSIONS_NEXT_MISSION_IN");

		// Cooldown bar
		m_cooldownBar.gameObject.SetActive(true);

		// Difficulty
		RefreshDifficulty(m_difficultyText, true);

        // TODO: A more efficient way to access the button
        if (!FeatureSettingsManager.AreAdsEnabled) {
            GameObject skipAdButton = m_cooldownObj.FindObjectRecursive("ButtonSkipAd");
            if (skipAdButton != null) {
                skipAdButton.SetActive(false);
            }
        }

		// Skip with ad button
		if(m_cooldownSkipFreeText != null) {
			// If the remaining time is lower than skip time, don't put time at all
			if(m_mission.cooldownRemaining.TotalSeconds < Mission.SECONDS_SKIPPED_WITH_AD) {
				m_cooldownSkipFreeText.Localize(TID_SKIP_FREE);
			} else {
				m_cooldownSkipFreeText.Localize(
					TID_SKIP_PARTIAL,
					StringUtils.FormatNumber(Mission.SECONDS_SKIPPED_WITH_AD/60f, 0)
				);
			}
		}
	}

	/// <summary>
	/// Refresh the timers part of the cooldown. Optimized to be called every frame.
	/// </summary>
	private void RefreshCooldownTimers() {
        // Since cooldown must be refreshed every frame, keep the reference to the objects rather than finding them every time
        // Cooldown remaining time
        if (m_cooldownTimeText != null) {
            double seconds = m_mission.cooldownRemaining.TotalSeconds;
            m_cooldownTimeText.text = TimeUtils.FormatTime(seconds, (seconds < 60f)? TimeUtils.EFormat.ABBREVIATIONS : TimeUtils.EFormat.DIGITS, 3);
        }

		// Cooldown bar
		if(m_cooldownBar != null) m_cooldownBar.normalizedValue = m_mission.cooldownProgress;

		// Skip cost
		// [AOC] The pill might not have it (e.g. in-game pill)
		if(m_cooldownSkipPaidText != null) {
			m_cooldownSkipPaidText.Localize(m_cooldownSkipPaidText.tid, StringUtils.FormatNumber(m_mission.skipCostPC));
		}
	}

	/// <summary>
	/// Refresh the activation pending state object.
	/// </summary>
	private void RefreshActivationPending() {
		// Info text
		m_cooldownInfoText.Localize("TID_MISSIONS_ACTIVATION_PENDING");

		// Cooldown remaining time
		m_cooldownTimeText.text = "";

		// Cooldown bar
		m_cooldownBar.gameObject.SetActive(false);

		// Skip cost - shouldn't exist in ACTIVATION_PENDING state, but just in case
		// [AOC] The pill might not have it (e.g. in-game pill)
		if(m_cooldownSkipPaidText != null) m_cooldownSkipPaidText.Localize("");

		// Difficulty
		RefreshDifficulty(m_difficultyText, true);
	}

	/// <summary>
	/// Refreshes the difficulty info.
	/// </summary>
	/// <param name="_loc">Localizer to be updated. If <c>null</c> process will be skipped.</param>
	/// <param name="_applyColor">Whether to change the color of the text to match the color of the difficulty based on content values.</param>
	private void RefreshDifficulty(Localizer _loc, bool _applyColor) {
		// Check params
		if(_loc == null) return;

		// Get difficulty definition
		DefinitionNode difficultyDef = MissionManager.GetDifficultyDef(m_missionDifficulty);
		if(difficultyDef == null) return;
			
		// Set text
		_loc.Localize(difficultyDef.GetAsString("tidName"));

		// Set color (if requested)
		if(_applyColor) {
			if(_loc.text != null) _loc.text.color = difficultyDef.GetAsColor("color");
		}
	}


    /// <summary>
    /// Refreshes the zone info
    /// </summary>
    /// <param name="_targetZone">Zone info group</param>
    /// <param name="_targetZoneText">Localizer of the zone</param>
    /// <param name="_zoneSku">Sku of the zone as defined in zoneTriggersDefinitions</param>
    private void RefreshZone(GameObject _targetZone, Localizer _targetZoneText, string _zoneSku)
    {
        // Safety check
        if (_targetZone == null || _targetZoneText == null)
            return;

        // Try to find the zone in the content
        DefinitionNode zoneDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.ZONE_TRIGGERS, "sku", _zoneSku);

        _targetZone.SetActive(true);
        _targetZoneText.gameObject.SetActive(true);

        if (string.IsNullOrEmpty(_zoneSku))
        {
            // Where
            _targetZoneText.Localize("TID_MISSIONS_ALL_MAP");

        }
        else
        {
            // Show the localized name of the zone
            _targetZoneText.Localize(zoneDef.GetAsString("tidName"));
        }

        return;

    }


    //------------------------------------------------------------------//
    // CALLBACKS														//
    //------------------------------------------------------------------//
    /// <summary>
    /// Callback for the remove mission button.
    /// </summary>
    public void OnRemoveMission() {
	// Ignore if mission not initialized
	if(m_mission == null) return;

	// Start purchase flow
	ResourcesFlow purchaseFlow = new ResourcesFlow("REMOVE_MISSION");
	purchaseFlow.OnSuccess.AddListener(
		(ResourcesFlow _flow) => {
			// Just do it
			HDTrackingManager.Instance.Notify_Missions(m_mission, HDTrackingManager.EActionsMission.skip_pay);
			MissionManager.RemoveMission(m_missionDifficulty);
            PersistenceFacade.instance.Save_Request();
        }
	);
	purchaseFlow.Begin((long)m_mission.removeCostPC, UserProfile.Currency.HARD, HDTrackingManager.EEconomyGroup.REMOVE_MISSION, m_mission.def);
	}

	/// <summary>
	/// Callback for the remove mission button with ads.
	/// </summary>
	public void OnFreeRemoveMission(){
		if(m_mission == null) return;

		// Ignore if offline
		if(DeviceUtilsManager.SharedInstance.internetReachability == NetworkReachability.NotReachable) {
			// Show some feedback
			UIFeedbackText.CreateAndLaunch(
				LocalizationManager.SharedInstance.Localize("TID_AD_ERROR"), 
				new Vector2(0.5f, 0.33f), 
				this.GetComponentInParent<Canvas>().transform as RectTransform
			);
			return;
		}

		// Show video ad!
		PopupAdBlocker.LaunchAd(true, GameAds.EAdPurpose.REMOVE_MISSION, OnVideoRewardCallback);
	}

	void OnVideoRewardCallback( bool done )
	{
		if ( done )
		{
			HDTrackingManager.Instance.Notify_Missions(m_mission, HDTrackingManager.EActionsMission.skip_ad);

			UsersManager.currentUser.dailyRemoveMissionAdUses++;
			MissionManager.RemoveMission(m_missionDifficulty);
            PersistenceFacade.instance.Save_Request();
        }
	}

	/// <summary>
	/// Ad popup has been closed.
	/// </summary>
	private void OnRemoveMissionAdClosed() {		
		HDTrackingManager.Instance.Notify_Missions(m_mission, HDTrackingManager.EActionsMission.skip_ad);
			
		UsersManager.currentUser.dailyRemoveMissionAdUses++;
		MissionManager.RemoveMission(m_missionDifficulty);
        PersistenceFacade.instance.Save_Request();
    }

	/// <summary>
	/// The skip time with ad button has been pressed.
	/// </summary>
	public void OnSkipTimeWithAd() {
		if(m_mission == null) return;

		// Check "daily" limit (not actually daily)
		bool canSkipWithAd = true;
		DefinitionNode _def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
		if(_def != null) {
			int usesPerDay = _def.GetAsInt("maxAdsSkipMissions", 0);
			double cooldownHours = _def.GetAsDouble("cooldownAdsSkipMissions", 24);
			if(usesPerDay > 0) {
				// If timestamp has passed, reset ad count
				DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
				if(serverTime >= UsersManager.currentUser.skipMissionAdTimestamp) {
					UsersManager.currentUser.skipMissionAdTimestamp = serverTime.AddHours(cooldownHours);
					UsersManager.currentUser.skipMissionAdUses = 0;
				}

				// Check remaining uses
				if(UsersManager.currentUser.skipMissionAdUses >= usesPerDay) {
					// Limit reached!
					canSkipWithAd = false;

					// Show error message
					TimeSpan remainingTime = UsersManager.currentUser.skipMissionAdTimestamp - serverTime;
					UIFeedbackText errorText = UIFeedbackText.CreateAndLaunch(
						LocalizationManager.SharedInstance.Localize("TID_AD_LIMIT_ERROR", TimeUtils.FormatTime(remainingTime.TotalSeconds, TimeUtils.EFormat.WORDS, 1)), 
						new Vector2(0.5f, 0.33f), 
						PopupManager.canvas.transform as RectTransform
					);
					errorText.text.color = UIConstants.ERROR_MESSAGE_COLOR;
				}
			}
		}

		// Show Ad!
		if(canSkipWithAd) {
			PopupAdBlocker.LaunchAd(true, GameAds.EAdPurpose.SKIP_MISSION_COOLDOWN, OnSkipTimeAdClosed);
		}
	}

	/// <summary>
	/// Ad popup has been closed.
	/// </summary>
	private void OnSkipTimeAdClosed(bool _success) {
		// Do it!
		if(_success) {
			UsersManager.currentUser.skipMissionAdUses++;
			MissionManager.SkipMission(m_missionDifficulty, Mission.SECONDS_SKIPPED_WITH_AD, true, false);
	        PersistenceFacade.instance.Save_Request();

			Refresh();
		}
    }

	/// <summary>
	/// Callback for the skip mission button.
	/// </summary>
	public void OnSkipMission() {
		// Ignore if mission not initialized
		if(m_mission == null) return;

        // Start purchase flow        
        ResourcesFlow purchaseFlow = new ResourcesFlow("SKIP_MISSION");
        purchaseFlow.OnSuccess.AddListener(
			(ResourcesFlow _flow) => {
				// Just do it
				MissionManager.SkipMission(m_missionDifficulty, -1f, false, true);
                PersistenceFacade.instance.Save_Request();
            }
		);
		purchaseFlow.Begin((long)m_mission.skipCostPC, UserProfile.Currency.HARD, HDTrackingManager.EEconomyGroup.SKIP_MISSION, m_mission.def);
	}

	/// <summary>
	/// A mission has been removed. If it matches the difficulty of this pill, refresh 
	/// with the new mission data.
	/// </summary>
	/// <param name="_newMission">The new mission replacing the one removed.</param>
	private void OnMissionRemoved(Mission _newMission) {
		if(_newMission.difficulty == m_missionDifficulty) {
			m_mission = _newMission;
			Refresh();
		}
		else
		{
			RefreshRemovePayButtons();
		}
	}

	/// <summary>
	/// A mission state has changed. If it matches the difficulty of this pill,
	/// refresh visuals.
	/// </summary>
	/// <param name="_mission">The mission that has finished its cooldown.</param>
	/// <param name="_oldState">The previous state of the mission.</param>
	/// <param name="_newState">The new state of the mission.</param>
	private void OnMissionStateChanged(Mission _mission, Mission.State _oldState, Mission.State _newState) {
		// Is it this mission?
		if(m_mission == _mission) {
			Refresh();
		}
	}

	/// <summary>
	/// The language has been changed.
	/// </summary>
	private void OnLanguageChanged() {
		// Just update all the info
		Refresh();
	}

	private void OnEventFinished(int _eventId, HDLiveDataManager.ComunicationErrorCodes _error) {
		Refresh();
	}

	/// <summary>
	/// Force a refresh.
	/// </summary>
	private void DEBUG_OnRefreshMissionInfo() {
		m_mission = MissionManager.GetMission(m_missionDifficulty);
		Refresh();
	}
}
