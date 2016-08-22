// HUDXPBar.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller for a xp bar in the debug hud.
/// </summary>
public class HUDXPBar : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	[SerializeField] private Slider m_bar;
	[SerializeField] private Text m_levelXpText;
	[SerializeField] private Text m_totalXpText;
	[SerializeField] private Text m_gameXpText;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		Messenger.AddListener<string, bool>(GameEvents.DEBUG_SETTING_CHANGED, OnDebugSettingChanged);

		// Only show if allowed!
		this.gameObject.SetActive(DebugSettings.Get(DebugSettings.SHOW_XP_BAR));
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string, bool>(GameEvents.DEBUG_SETTING_CHANGED, OnDebugSettingChanged);
	}

	/// <summary>
	/// Keep values updated
	/// </summary>
	private void Update() {
		// Only if player is alive
		if(InstanceManager.player != null) {
			// Aux vars
			DragonProgression progression = InstanceManager.player.data.progression;
			Range xpRange = progression.xpRange;

			// Bar value
			m_bar.minValue = xpRange.min;
			m_bar.maxValue = xpRange.max;
			m_bar.value = progression.xp;

			// Special case on last level
			if(progression.isMaxLevel) {
				m_bar.minValue = m_bar.minValue - 1;	// Trick to show the bar full
			}

			// Text
			// Special case on last level
			if(progression.isMaxLevel) {
				m_levelXpText.text = String.Format("{0}/{1} (lv. {2}/{3})",
					StringUtils.FormatNumber(progression.xp, 0),
					StringUtils.FormatNumber(Mathf.Ceil(xpRange.max), 0),
					StringUtils.FormatNumber(progression.level + 1),
					StringUtils.FormatNumber(progression.lastLevel + 1)
				);
			} else {
				m_levelXpText.text = String.Format("{0}/{1} (lv. {2}/{3})",
					StringUtils.FormatNumber(progression.xp - xpRange.min, 0),
                    StringUtils.FormatNumber(Mathf.Ceil(xpRange.distance), 0),
                    StringUtils.FormatNumber(progression.level + 1),
                    StringUtils.FormatNumber(progression.lastLevel + 1)
				);
			}

			// Total XP text
			m_totalXpText.text = String.Format("Total: {0} xp", StringUtils.FormatNumber(progression.xp, 0));

			// Game earned XP text
			m_gameXpText.text = String.Format("Game: {0} xp", StringUtils.FormatNumber(RewardManager.xp, 0));
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	private void OnDebugSettingChanged(string _id, bool _newValue) {
		// XP bar setting?
		if(_id == DebugSettings.SHOW_XP_BAR) {
			this.gameObject.SetActive(_newValue);
		}
	}
}
