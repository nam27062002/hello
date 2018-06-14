// GlobalEventsScreenActivePanel.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Panel corresponding to an active global event.
/// </summary>
public class GlobalEventsPanelTeaser : GlobalEventsPanel {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float EVENT_COUNTDOWN_UPDATE_INTERVAL = 1f;	// Seconds
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private TextMeshProUGUI m_timerText = null;
	[SerializeField] private Image m_icon;
	[SerializeField] private TextMeshProUGUI m_text;
	[SerializeField] private GlobalEventsRewardInfo m_rewardInfo;


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	public void OnEnable() {
		// Program periodic update call
		InvokeRepeating("UpdatePeriodic", 0f, EVENT_COUNTDOWN_UPDATE_INTERVAL);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	public void OnDisable() {
		// Cancel periodic call
		CancelInvoke();
	}

	/// <summary>
	/// Called periodically.
	/// </summary>
	private void UpdatePeriodic() {
		// Just in case
		if(GlobalEventManager.currentEvent == null) return;

		// Update timer
		double remainingSeconds = GlobalEventManager.currentEvent.remainingTime.TotalSeconds;
		m_timerText.text = TimeUtils.FormatTime(
			System.Math.Max(0, remainingSeconds),	// Never show negative time!
			TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
			4
		);

		// [AOC] Manage timer end when this panel is active
		if(remainingSeconds <= 0) {
			GlobalEventManager.RequestCurrentEventState();	// This should change the active panel
		}
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	override public void Refresh() {
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, GlobalEventManager.currentEvent.bonusDragonSku);
		m_text.text = def.GetLocalized("tidName");

		GlobalEvent evt = GlobalEventManager.currentEvent;
		m_icon.sprite = Resources.Load<Sprite>(UIConstants.DISGUISE_ICONS_PATH + evt.bonusDragonSku + "/icon_disguise_0");	// Default skin

		m_rewardInfo.rewardSlot = evt.topContributorsRewardSlot;

		// Force a first update on the timer
		UpdatePeriodic();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}