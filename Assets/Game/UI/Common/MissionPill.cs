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
public class MissionPill : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Setup
	[Separator]
	[HideEnumValues(false, true)]
	[SerializeField] private Mission.Difficulty m_missionDifficulty = Mission.Difficulty.EASY;
	[SerializeField] private bool m_showProgressForSingleRunMissions = false;	// It doesn't make sense to show progress for single-run missions in the menu
	
	// References - keep references to objects that are often accessed
	[Separator]
	[SerializeField] private GameObject m_cooldownObj = null;
	[SerializeField] private GameObject m_activeObj = null;

	// Cooldown group
	private TextMeshProUGUI m_cooldownText = null;
	private Slider m_cooldownBar = null;
	private TextMeshProUGUI m_skipCostText = null;	// Optional

	// Data
	private Mission m_mission = null;
	public Mission mission {
		get {
			if(m_mission == null) {
				m_mission = MissionManager.GetMission(m_missionDifficulty);
			}
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

		// Find other references
		// [AOC] Since cooldown must be refreshed every frame, keep the reference to the objects rather than finding them every time
		m_cooldownText = m_cooldownObj.FindComponentRecursive<TextMeshProUGUI>("CooldownTimeText");
		m_cooldownBar = m_cooldownObj.FindComponentRecursive<Slider>("CooldownBar");
		m_skipCostText = m_cooldownObj.FindComponentRecursive<TextMeshProUGUI>("TextCost");

		// Subscribe to external events
		Messenger.AddListener<Mission>(GameEvents.MISSION_REMOVED, OnMissionRemoved);
		Messenger.AddListener<Mission, Mission.State, Mission.State>(GameEvents.MISSION_STATE_CHANGED, OnMissionStateChanged);
	}

	/// <summary>
	/// Default destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Mission>(GameEvents.MISSION_REMOVED, OnMissionRemoved);
		Messenger.RemoveListener<Mission, Mission.State, Mission.State>(GameEvents.MISSION_STATE_CHANGED, OnMissionStateChanged);
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Detect hot language changes
		Messenger.AddListener(EngineEvents.LANGUAGE_CHANGED, OnLanguageChanged);

		// Make sure we're up to date
		Refresh();
	}

	private void OnDisable() {
		// Only detect hot language changes while active
		Messenger.RemoveListener(EngineEvents.LANGUAGE_CHANGED, OnLanguageChanged);
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Update time-dependant fields
		if(mission != null && mission.state == Mission.State.COOLDOWN) {
			RefreshCooldownTimers();
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
		RefreshDifficulty(this.FindComponentRecursive<Localizer>("DifficultyTextTitle"), true);
	}

	/// <summary>
	/// Refresh active state object.
	/// </summary>
	private void RefreshActive() {
		// Mission description
		m_activeObj.FindComponentRecursive<TextMeshProUGUI>("MissionText").text = m_mission.objective.GetDescription();

		// Progress
		// Optionally hide progress for singlerun missions
		bool show = !m_mission.def.GetAsBool("singleRun") || m_showProgressForSingleRunMissions;
		m_activeObj.FindObjectRecursive("ProgressGroup").SetActive(show);
		if(show) {
			m_activeObj.FindComponentRecursive<Localizer>("ProgressText").Localize("TID_FRACTION", m_mission.objective.GetCurrentValueFormatted(), m_mission.objective.GetTargetValueFormatted());
			m_activeObj.FindComponentRecursive<Slider>("ProgressBar").value = m_mission.objective.progress;
		}

		// Reward
		m_activeObj.FindComponentRecursive<TextMeshProUGUI>("RewardText").text = UIConstants.GetIconString(m_mission.rewardCoins, UIConstants.IconType.COINS, UIConstants.IconAlignment.LEFT);

		// Remove cost
		// [AOC] The pill might not have it (e.g. in-game pill)
		TextMeshProUGUI removeCostText = m_activeObj.FindComponentRecursive<TextMeshProUGUI>("TextCost");
		if(removeCostText != null) removeCostText.text = UIConstants.GetIconString(m_mission.removeCostPC, UIConstants.IconType.PC, UIConstants.IconAlignment.LEFT);

		// Check if this mission is complete
		GameObject completedObj = m_activeObj.FindObjectRecursive("CompletedMission");
		if (completedObj != null) completedObj.SetActive(m_mission.objective.isCompleted);

		// Change Icon
		GameObject iconBoxObj = m_activeObj.FindObjectRecursive("IconBox");
		if (iconBoxObj != null) {
			Image img = iconBoxObj.FindObjectRecursive("Image").GetComponent<Image>();
			Sprite spr = Resources.Load<Sprite>(m_mission.def.GetAsString("icon"));
			img.sprite = spr;
		}

		// Where
		// [AOC] TODO!! Feature not yet implemented, use a fixed text for now
		Localizer whereText = m_activeObj.FindComponentRecursive<Localizer>("TextPlaceValue");
		if(whereText != null) {
			whereText.Localize("TID_MISSIONS_WHERE_ANY_LEVEL");
		}

		// With
		// [AOC] TODO!! Feature not yet implemented, use a fixed text for now
		Localizer withText = m_activeObj.FindComponentRecursive<Localizer>("TextWithValue");
		if(withText != null) {
			withText.Localize("TID_MISSIONS_WITH_ANY_DRAGON");
		}

		// Difficulty
		RefreshDifficulty(m_activeObj.FindComponentRecursive<Localizer>("DifficultyText"), true);

		RefreshRemovePayButtons();
	}

	private void RefreshRemovePayButtons()
	{
		GameObject watchAd = m_activeObj.FindObjectRecursive("ButtonWatchAd");
		GameObject removeButton = m_activeObj.FindObjectRecursive("ButtonRemoveMission");
		if ( watchAd != null && removeButton != null){
			
			bool canPayWithAds = CanPayRemoveMissionWithAds();
			// Check if ads availables to skip mission
			watchAd.SetActive( canPayWithAds );
			removeButton.SetActive( !canPayWithAds );
		}
	}

	/// <summary>
	/// Determines whether the yser can pay to remove this mission with ads.
	/// </summary>
	/// <returns><c>true</c> if the user can pay to remove the mission with ads; otherwise, <c>false</c>.</returns>
	private bool CanPayRemoveMissionWithAds()
	{
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			return false;
		}

		DefinitionNode _def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
        if (_def != null)
        {
			int usesPerDay = _def.GetAsInt("dailyAdsRemoveMissions");
			if ( usesPerDay > 0 )
			{
				// Check remaining uses
				if ( DateTime.UtcNow >= UsersManager.currentUser.dailyRemoveMissionAdTimestamp )
				{
					UsersManager.currentUser.dailyRemoveMissionAdTimestamp = DateTime.UtcNow.AddDays(1);
					UsersManager.currentUser.dailyRemoveMissionAdUses = 0;
				}
				if (UsersManager.currentUser.dailyRemoveMissionAdUses >= usesPerDay) 
				{
					return false;
				}
			}
        }
       
		return true;
	}

	/// <summary>
	/// Refresh the cooldown state object.
	/// </summary>
	private void RefreshCooldown() {
		// Update the timers
		RefreshCooldownTimers();

		// Difficulty
		RefreshDifficulty(m_cooldownObj.FindComponentRecursive<Localizer>("DifficultyText"), true);
	}

	/// <summary>
	/// Refresh the timers part of the cooldown. Optimized to be called every frame.
	/// </summary>
	private void RefreshCooldownTimers() {
		// Since cooldown must be refreshed every frame, keep the reference to the objects rather than finding them every time
		// Cooldown remaining time
		if(m_cooldownText != null) m_cooldownText.text = TimeUtils.FormatTime(m_mission.cooldownRemaining.TotalSeconds, TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 3);

		// Cooldown bar
		if(m_cooldownBar != null) m_cooldownBar.normalizedValue = m_mission.cooldownProgress;

		// Skip cost
		// [AOC] The pill might not have it (e.g. in-game pill)
		if(m_skipCostText != null) m_skipCostText.text = UIConstants.GetIconString(m_mission.skipCostPC, UIConstants.IconType.PC, UIConstants.IconAlignment.LEFT);
	}

	/// <summary>
	/// Refresh the activation pending state object.
	/// </summary>
	private void RefreshActivationPending() {
		// Info text
		m_cooldownObj.FindComponentRecursive<TextMeshProUGUI>("CooldownInfoText").text = LocalizationManager.SharedInstance.Localize("TID_MISSIONS_ACTIVATION_PENDING");

		// Cooldown remaining time
		m_cooldownText.text = "";

		// Cooldown bar
		m_cooldownBar.normalizedValue = 1f;

		// Skip cost - shouldn't exist in ACTIVATION_PENDING state, but just in case
		// [AOC] The pill might not have it (e.g. in-game pill)
		if(m_skipCostText != null) m_skipCostText.text = "";

		// Difficulty
		RefreshDifficulty(m_cooldownObj.FindComponentRecursive<Localizer>("DifficultyText"), true);
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
				MissionManager.RemoveMission(m_missionDifficulty);
				PersistenceManager.Save();
			}
		);
		purchaseFlow.Begin((long)m_mission.removeCostPC, UserProfile.Currency.HARD, m_mission.def);
	}

	/// <summary>
	/// Callback for the remove mission button with ads.
	/// </summary>
	public void OnFreeRemoveMission(){
		if ( m_mission == null ) return;

		PopupController popup = PopupManager.OpenPopupInstant(PopupAdRevive.PATH);
		popup.OnClosePostAnimation.AddListener(OnAdClosed);
	}

	private void OnAdClosed() {

		UsersManager.currentUser.dailyRemoveMissionAdUses++;
		MissionManager.RemoveMission(m_missionDifficulty);
		PersistenceManager.Save();
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
				MissionManager.SkipMission(m_missionDifficulty);
				PersistenceManager.Save();
			}
		);
		purchaseFlow.Begin((long)m_mission.skipCostPC, UserProfile.Currency.HARD, m_mission.def);
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
}
