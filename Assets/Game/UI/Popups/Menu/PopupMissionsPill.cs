// PopupMissionsPill.cs
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

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Pill representing a single mission for the temp popup.
/// </summary>
public class PopupMissionsPill : MonoBehaviour {
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
	[SerializeField] private GameObject m_lockedObj = null;
	[SerializeField] private GameObject m_cooldownObj = null;
	[SerializeField] private GameObject m_activeObj = null;

	// Cooldown group
	private Text m_cooldownText = null;
	private Slider m_cooldownBar = null;
	private Text m_skipCostText = null;	// Optional

	// Data
	private Mission m_mission = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	private void Awake() {
		// Check required references
		Debug.Assert(m_lockedObj != null, "Required reference!");
		Debug.Assert(m_cooldownObj != null, "Required reference!");
		Debug.Assert(m_activeObj != null, "Required reference!");

		// Find other references
		// [AOC] Since cooldown must be refreshed every frame, keep the reference to the objects rather than finding them every time
		m_cooldownText = m_cooldownObj.FindComponentRecursive<Text>("CooldownTimeText");
		m_cooldownBar = m_cooldownObj.FindComponentRecursive<Slider>("CooldownBar");
		m_skipCostText = m_cooldownObj.FindComponentRecursive<Text>("TextCost");

		// Subscribe to external events
		Messenger.AddListener<Mission>(GameEvents.MISSION_REMOVED, OnMissionRemoved);
		Messenger.AddListener<Mission>(GameEvents.MISSION_COOLDOWN_FINISHED, OnMissionCooldownFinished);
	}

	/// <summary>
	/// Default destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Mission>(GameEvents.MISSION_REMOVED, OnMissionRemoved);
		Messenger.RemoveListener<Mission>(GameEvents.MISSION_COOLDOWN_FINISHED, OnMissionCooldownFinished);
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure we're up to date
		Refresh();
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Update time-dependant fields
		if(m_mission.state == Mission.State.COOLDOWN) {
			RefreshCooldown();
		}
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Update the pill with the data from the target mission.
	/// </summary>
	public void Refresh() {
		// Get mission
		if(m_mission == null) {
			m_mission = MissionManager.GetMission(m_missionDifficulty);
			if(m_mission == null) return;
		}

		// Select which object should be visible
		m_lockedObj.SetActive(m_mission.state == Mission.State.LOCKED);
		m_cooldownObj.SetActive(m_mission.state == Mission.State.COOLDOWN || m_mission.state == Mission.State.ACTIVATION_PENDING);
		m_activeObj.SetActive(m_mission.state == Mission.State.ACTIVE);

		// Update visuals
		switch(m_mission.state) {
			case Mission.State.LOCKED: 				RefreshLocked(); 			break;
			case Mission.State.COOLDOWN: 			RefreshCooldown(); 			break;
			case Mission.State.ACTIVATION_PENDING: 	RefreshActivationPending(); break;
			case Mission.State.ACTIVE: 				RefreshActive(); 			break;
		}
	}

	/// <summary>
	/// Refresh active state object.
	/// </summary>
	private void RefreshActive() {
		// Mission description
		m_activeObj.FindComponentRecursive<Text>("MissionText").text = m_mission.objective.GetDescription();

		// Progress
		// Optionally hide progress for singlerun missions
		bool show = !m_mission.def.Get<bool>("singleRun") || m_showProgressForSingleRunMissions;
		m_activeObj.FindObjectRecursive("ProgressGroup").SetActive(show);
		if(show) {
			m_activeObj.FindComponentRecursive<Text>("ProgressText").text = System.String.Format("{0}/{1}", m_mission.objective.GetCurrentValueFormatted(), m_mission.objective.GetTargetValueFormatted());
			m_activeObj.FindComponentRecursive<Slider>("ProgressBar").value = m_mission.objective.progress;
		}

		// Reward
		m_activeObj.FindComponentRecursive<Text>("RewardText").text = StringUtils.FormatNumber(m_mission.rewardCoins);

		// Remove cost
		// [AOC] The pill might not have it (e.g. in-game pill)
		Text removeCostText = m_activeObj.FindComponentRecursive<Text>("TextCost");
		if(removeCostText != null) removeCostText.text = StringUtils.FormatNumber(m_mission.removeCostPC);

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
	}

	/// <summary>
	/// Refresh the cooldown state object.
	/// </summary>
	private void RefreshCooldown() {
		// Since cooldown must be refreshed every frame, keep the reference to the objects rather than finding them every time
		// Cooldown remaining time
		m_cooldownText.text = TimeUtils.FormatTime(m_mission.cooldownRemaining.TotalSeconds, TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 3);

		// Cooldown bar
		m_cooldownBar.normalizedValue = m_mission.cooldownProgress;	// [AOC] Reverse bar

		// Skip cost
		// [AOC] The pill might not have it (e.g. in-game pill)
		if(m_skipCostText != null) m_skipCostText.text = StringUtils.FormatNumber(m_mission.skipCostPC);
	}

	/// <summary>
	/// Refresh the activation pending state object.
	/// </summary>
	private void RefreshActivationPending() {
		// Info text
		m_cooldownObj.FindComponentRecursive<Text>("CooldownInfoText").text = Localization.Localize("Mission will be available for next game!");	// [AOC] HARDCODED!!

		// Cooldown remaining time
		m_cooldownText.text = "";

		// Cooldown bar
		m_cooldownBar.normalizedValue = 1f;	// [AOC] Reverse bar

		// Skip cost - shouldn't exist in ACTIVATION_PENDING state, but just in case
		// [AOC] The pill might not have it (e.g. in-game pill)
		if(m_skipCostText != null) m_skipCostText.text = "";
	}

	/// <summary>
	/// Refresh the locked state object.
	/// </summary>
	private void RefreshLocked() {
		// Text
		int dragonsOwned = DragonManager.GetDragonsByLockState(DragonData.LockState.OWNED).Count;
		int remainingDragonsToUnlock = MissionManager.dragonsToUnlock[(int)m_missionDifficulty] - dragonsOwned;
		if(remainingDragonsToUnlock == 1) {
			m_lockedObj.FindComponentRecursive<Text>("LockedText").text = Localization.Localize("LOCKED!\nOwn 1 more dragon to unlock this mission");	// [AOC] HARDCODED!!
		} else {
			m_lockedObj.FindComponentRecursive<Text>("LockedText").text = Localization.Localize("LOCKED!\nOwn %U0 more dragons to unlock this mission", StringUtils.FormatNumber(remainingDragonsToUnlock));	// [AOC] HARDCODED!!
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

		// Make sure we have enough PC to remove the mission
		int costPC = m_mission.removeCostPC;
		if(UserProfile.pc >= costPC) {
			// Do it!
			UserProfile.AddPC(-costPC);
			MissionManager.RemoveMission(m_missionDifficulty);
			PersistenceManager.Save();
		} else {
			// Open shop popup
			PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);
		}
	}

	/// <summary>
	/// Callback for the skip mission button.
	/// </summary>
	public void OnSkipMission() {
		// Ignore if mission not initialized
		if(m_mission == null) return;

		// Make sure we have enough PC to remove the mission
		int costPC = m_mission.skipCostPC;
		if(UserProfile.pc >= costPC) {
			// Do it!
			UserProfile.AddPC(-costPC);
			MissionManager.SkipMission(m_missionDifficulty);
			PersistenceManager.Save();
		} else {
			// Open shop popup
			PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);
		}
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
	}

	/// <summary>
	/// A mission cooldown has finished. If it matches the difficulty of this pill,
	/// refresh visuals. Works both for skipped missions and real cooldown timer endings.
	/// </summary>
	/// <param name="_mission">The mission that has finished its cooldown.</param>
	private void OnMissionCooldownFinished(Mission _mission) {
		if(_mission.difficulty == m_missionDifficulty) {
			Refresh();
		}
	}
}
