// MenuDragonSkillBar.cs
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
public class MenuDragonSkillBar : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	[Header("Data")]
	[SkuList(Definitions.Category.DRAGON_SKILLS)]
	public string m_skillSku;

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
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, Refresh);
		
		// Do a first refresh
		Refresh(InstanceManager.GetSceneController<MenuSceneController>().selectedDragon);
	}
	
	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, Refresh);
	}

	/// <summary>
	/// Refresh with data from currently selected dragon.
	/// </summary>
	/// <param name="_sku">The _sku of the selected dragon</param>
	public void Refresh(string _sku) {
		// Get skill data
		DragonSkill skillData = DragonManager.GetDragonData(_sku).GetSkill(m_skillSku);

		// Label
		m_labelText.text = skillData.def.GetLocalized("tidName");

		// Bar value
		m_bar.minValue = 0;
		m_bar.maxValue = DragonSkill.NUM_LEVELS - 1;
		m_bar.value = skillData.level;	// [0..N-1] bar starts empty and should be filled when we're at level 5
			
		// Text
		// [AOC] TODO!! May depend on skill type
		switch(skillData.def.sku) {
			default:
				m_valueText.text = String.Format("{0}", StringUtils.FormatNumber(skillData.value, 2));
				break;
		}

		// Level up button
		m_levelUpButton.interactable = skillData.CanUnlockNextLevel();
		Text m_text = m_levelUpButton.FindTransformRecursive("Text").GetComponent<Text>();
		if(skillData.level == skillData.lastLevel) {
			m_text.text = "Max";	// [AOC] HARDCODED!!
		} else {
			m_text.text = String.Format("{0}", StringUtils.FormatNumber(skillData.nextLevelUnlockPrice));	// [AOC] HARDCODED!!
		}
	}

	/// <summary>
	/// Level up the current skill.
	/// </summary>
	public void LevelUp() {
		// Get skill data
		DragonSkill skillData = DragonManager.currentDragon.GetSkill(m_skillSku);

		// Enough resources?
		if(UserProfile.coins < skillData.nextLevelUnlockPrice) {
			// Show currency shop
			PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);
		} else {
			// Perform transaction
			UserProfile.AddCoins(-skillData.nextLevelUnlockPrice);

			// Do it! ^_^
			skillData.UnlockNextLevel();

			// Refresh data
			Refresh(DragonManager.currentDragon.def.sku);

			// Save persistence
			PersistenceManager.Save();
		}
	}
}
