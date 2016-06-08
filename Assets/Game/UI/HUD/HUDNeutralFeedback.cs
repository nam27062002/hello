// HUDNeedBiggerDragon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/12/2015.
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
/// Simple controller for a feedback in the hud telling you need a bigger dragon.
/// </summary>
public class HUDNeutralFeedback : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private float m_needBiggerSharkDuration = 1f;	// Seconds
	[SerializeField] private float m_survivalBonusDuration = 1f;	// Seconds

	// External refs
	public GameObject m_needBiggerDragonObject;
	public GameObject m_survivalUpdatedObject;

	enum ShowingEvent
	{
		NONE,
		BIGGER_DRAGON,
		SURVIVAL_BONUS,
	}

	ShowingEvent m_showingEvent = ShowingEvent.NONE;

	private Text m_biggerDragon_NeedText = null;
	private Text m_biggerDragon_DragonText = null;
	private Image m_biggerDragonIcon = null;
	private ShowHideAnimator m_biggerSharkAnim;
	private ShowHideAnimator m_survivalBonusAnim;

	// Logic
	private float m_timer = 0f;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external refs
		m_biggerSharkAnim = m_needBiggerDragonObject.GetComponent<ShowHideAnimator>();
		m_biggerDragon_NeedText = m_needBiggerDragonObject.transform.FindChild("Needtext").GetComponent<Text>();
		m_biggerDragon_DragonText = m_needBiggerDragonObject.transform.FindChild("Dragontext").GetComponent<Text>();
		m_biggerDragonIcon = m_needBiggerDragonObject.GetComponentInChildren<Image>();

		m_survivalBonusAnim = m_survivalUpdatedObject.GetComponent<ShowHideAnimator>();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() 
	{
		HideBiggerDragon();
		HideSurvivalBonus();
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<DragonTier>(GameEvents.BIGGER_DRAGON_NEEDED, OnBiggerDragonNeeded);
		Messenger.AddListener(GameEvents.SURVIVAL_BONUS_ACHIEVED, OnNewSurvivalBonus);
	}
	
	/// <summary>
	/// The component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<DragonTier>(GameEvents.BIGGER_DRAGON_NEEDED, OnBiggerDragonNeeded);
		Messenger.RemoveListener(GameEvents.SURVIVAL_BONUS_ACHIEVED, OnNewSurvivalBonus);
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() 
	{
		if ( m_timer > 0 )
		{
			m_timer -= Time.deltaTime;
			// Timer has finished? Hide
			if(m_timer <= 0f) 
			{
				HideCurrent();
			}
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A mission has been completed.
	/// </summary>
	/// <param name="_requiredTier">The required tier. DragonTier.COUNT if not defined.</param>
	private void OnBiggerDragonNeeded(DragonTier _requiredTier) 
	{
		if ( m_showingEvent != ShowingEvent.BIGGER_DRAGON )
		{
			HideCurrent();
			ShowBiggerDragon( _requiredTier );
			m_showingEvent = ShowingEvent.BIGGER_DRAGON;
		}
	}


	private void ShowBiggerDragon(DragonTier _requiredTier)
	{
		// Init text!
		/*if(_requiredTier == DragonTier.COUNT) {
			m_biggerSharkText.text = Localization.Localize("TID_FEEDBACK_NEED_BIGGER_DRAGON");
			m_biggerSharkIcon.enabled = false
		} else {
			// [AOC] TEMP!! While we don't use tier icons, use different text colors per tier
			string colorCode = "";
			switch(_requiredTier) {
				case DragonTier.TIER_0: colorCode = "#ff7fff";	break;
				case DragonTier.TIER_1: colorCode = "#7fff00";	break;
				case DragonTier.TIER_2: colorCode = "#ffff00";	break;
				case DragonTier.TIER_3: colorCode = "#ff7f00";	break;
				case DragonTier.TIER_4: colorCode = "#ff0000";	break;
			}

			// Get required tier definition
			DefinitionNode tierDef = DefinitionsManager.GetDefinitionByVariable(DefinitionsCategory.DRAGON_TIERS, "order", ((int)_requiredTier).ToString());
			string replacement = tierDef.GetLocalized("tidName");
			if(!string.IsNullOrEmpty(colorCode)) {
				replacement = "<color=" + colorCode + ">" + replacement + "</color>";
			}
			m_biggerSharkText.text = Localization.Localize("TID_FEEDBACK_NEED_TIER_DRAGON", replacement);
		}*/

		// TODO: we'll add all the icons into a font and we'll print the icons as a character.
		if(_requiredTier == DragonTier.COUNT) {
			m_biggerDragonIcon.enabled = false;
		} else {
			m_biggerDragonIcon.enabled = true;

			string path = "UI/Menu/Graphics/tiers/";
			switch(_requiredTier) {
				case DragonTier.TIER_0: path += "icon_xs";	break;
				case DragonTier.TIER_1: path += "icon_s";	break;
				case DragonTier.TIER_2: path += "icon_m";	break;
				case DragonTier.TIER_3: path += "icon_l";	break;
				case DragonTier.TIER_4: path += "icon_xl";	break;
			}

			m_biggerDragonIcon.sprite = Resources.Load<Sprite>(path);
		}

		// Play the anim!
		m_biggerSharkAnim.Show();

		// Reset timer!
		m_timer = m_needBiggerSharkDuration;
	}

	private void HideBiggerDragon()
	{
		m_biggerSharkAnim.Hide(true, false);	// Don't disable after animation! We still want to receive events ^_^
	}

	private void OnNewSurvivalBonus() 
	{
		if ( m_showingEvent != ShowingEvent.SURVIVAL_BONUS )
		{
			HideCurrent();
			ShowSurvivalBonus();
			m_showingEvent = ShowingEvent.SURVIVAL_BONUS;
		}
	}

	private void ShowSurvivalBonus()
	{
		m_survivalBonusAnim.Show();
		m_timer = m_survivalBonusDuration;
	}

	private void HideSurvivalBonus()
	{
		m_survivalBonusAnim.Hide(true, false);	// Don't disable after animation! We still want to receive events ^_^
	}

	private void HideCurrent()
	{
		switch( m_showingEvent )
		{
			case ShowingEvent.BIGGER_DRAGON:
			{
				HideBiggerDragon();
			}break;
			case ShowingEvent.SURVIVAL_BONUS:
			{
				HideSurvivalBonus();
			}break;
		}
		m_showingEvent = ShowingEvent.NONE;
	}
}
