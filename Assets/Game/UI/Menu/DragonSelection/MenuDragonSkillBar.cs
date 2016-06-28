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
using System.Collections.Generic;

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
	// [SkuList(DefinitionsCategory.DRAGON_SKILLS)]
	public string m_skillSku;

	[Header("References")]
	public GameObject m_levelsParent;	// game object containing all the toggles to show the levels
	private List<Toggle> m_levelToggles = new List<Toggle>();
	public Localizer m_labelText;
	public Text m_valueText;
	public Button m_levelUpButton;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() 
	{
		int i = 1;
		Toggle levelToggle = null;
		do
		{
			levelToggle = null;
			Transform child = m_levelsParent.transform.FindChild( "Level_" + i );
			if ( child != null )
			{
				levelToggle = child.GetComponent<Toggle>();
				if ( levelToggle != null )
				{
					m_levelToggles.Add( levelToggle );
					i++;
				}
			}

		}while( levelToggle != null );
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
		// Get dragon data
		DragonData dragonData = DragonManager.GetDragonData(_sku);
		if(dragonData == null) return;

		// Get skill data
		DragonSkill skillData = dragonData.GetSkill(m_skillSku);
		if(skillData == null) return;

		// Label
		m_labelText.Localize(skillData.def.Get("tidName"));

		// Level Values
		for( int i = 0; i<m_levelToggles.Count; i++ )
		{
			m_levelToggles[i].isOn = i < skillData.level;
		}

		// Text
		// [AOC] TODO!! May depend on skill type
		switch(skillData.def.sku) {
			default: {
				m_valueText.text = StringUtils.FormatNumber(skillData.value, 2);
			} break;
		}

		// Level up button
		m_levelUpButton.interactable = skillData.CanUnlockNextLevel();
		Text buttonText = m_levelUpButton.FindComponentRecursive<Text>("Text");
		if(skillData.level == skillData.lastLevel) {
            buttonText.text = LocalizationManager.SharedInstance.Localize("TID_MAX");
		} else {
			buttonText.text = StringUtils.FormatNumber(skillData.nextLevelUnlockPrice);
		}
	}

	/// <summary>
	/// Level up the current skill.
	/// </summary>
	public void LevelUp() {
		// Get skill data
		DragonSkill skillData = DragonManager.currentDragon.GetSkill(m_skillSku);

		// Play Sound
		AudioManager.instance.PlayClip("audio/sfx/UI/hsx_ui_button_select");

		// Enough resources?
		if(UserProfile.coins < skillData.nextLevelUnlockPrice) {
			// Show currency shop
			//PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);

			// Currency popup / Resources flow disabled for now
            UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_SC_NOT_ENOUGH"), new Vector2(0.5f, 0.33f), this.GetComponentInParent<Canvas>().transform as RectTransform);
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
