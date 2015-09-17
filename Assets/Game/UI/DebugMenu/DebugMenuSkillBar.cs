// DebugMenuSkillBar.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/09/2015.
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
/// Simple controller to display the state of a skill from the current dragon in the debug menu.
/// </summary>
public class DebugMenuSkillBar : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	[Header("Data")]
	public DragonSkill.EType m_skillType;

	[Header("References")]
	public Slider m_bar;
	public Text m_labelText;
	public Text m_valueText;
	public Button m_levelUpButton;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// First update
	/// </summary>
	private void Start() {
		// Subscribe to external events
		Messenger.AddListener(DebugMenuDragonSelector.EVENT_DRAGON_CHANGED, Refresh);
		Messenger.AddListener(DebugMenuSimulate.EVENT_SIMULATION_FINISHED, Refresh);
		
		// Do a first refresh
		Refresh();
	}
	
	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener(DebugMenuDragonSelector.EVENT_DRAGON_CHANGED, Refresh);
		Messenger.RemoveListener(DebugMenuSimulate.EVENT_SIMULATION_FINISHED, Refresh);
	}

	/// <summary>
	/// Refresh with data from currently selected dragon.
	/// </summary>
	public void Refresh() {
		// Get skill data
		DragonSkill skillData = DragonManager.currentDragonData.GetSkill(m_skillType);

		// Label
		//m_labelTxt.text = Localization.Localize(skillData.tidName);
		m_labelText.text = skillData.type.ToString();

		// Bar value
		m_bar.minValue = 0;
		m_bar.maxValue = DragonSkill.NUM_LEVELS - 1;
		m_bar.value = skillData.level;	// [0..N-1] bar starts empty and should be filled when we're at level 5
			
		// Text
		// [AOC] TODO!! Depends on skill type
		switch(skillData.type) {
			case DragonSkill.EType.BITE:
			case DragonSkill.EType.BOOST:
			case DragonSkill.EType.FIRE:
				m_valueText.text = String.Format("{0}", StringUtils.FormatNumber(skillData.value, 2));
				break;
			case DragonSkill.EType.SPEED:
				m_valueText.text = String.Format("{0}", StringUtils.FormatNumber(skillData.value, 0));
				break;
		}

		// Level up button
		m_levelUpButton.interactable = skillData.CanUnlockNextLevel();
		Text m_text = m_levelUpButton.FindSubObject("Text").GetComponent<Text>();
		if(skillData.level == skillData.lastLevel) {
			m_text.text = "Max";	// [AOC] HARDCODED!!
		} else {
			m_text.text = String.Format("▲{0}", StringUtils.FormatNumber(skillData.nextLevelUnlockPrice));	// [AOC] HARDCODED!!
		}
	}

	public void LevelUp() {
		// Get skill data
		DragonSkill skillData = DragonManager.currentDragonData.GetSkill(m_skillType);

		// Just do it! ^_^
		skillData.UnlockNextLevel();

		// Refresh data
		Refresh();

		// Save persistence
		PersistenceManager.Save();
	}
}
