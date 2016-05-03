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
[RequireComponent(typeof(ShowHideAnimator))]
public class HUDNeedBiggerDragon : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private float m_duration = 1f;	// Seconds

	// External refs
	private Text m_text = null;
	private ShowHideAnimator m_anim;

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
		m_anim = GetComponent<ShowHideAnimator>();
		m_text = GetComponentInChildren<Text>();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Start hidden!
		m_anim.ForceHide(false, false);	// Don't disable! We still want to receive events ^_^
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<DragonTier>(GameEvents.BIGGER_DRAGON_NEEDED, OnBiggerDragonNeeded);
	}
	
	/// <summary>
	/// The component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<DragonTier>(GameEvents.BIGGER_DRAGON_NEEDED, OnBiggerDragonNeeded);
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Update timer
		if(m_timer > 0f) {
			m_timer -= Time.deltaTime;

			// Timer has finished? Hide
			if(m_timer <= 0f) {
				m_anim.Hide(true, false);	// Don't disable after animation! We still want to receive events ^_^
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
	private void OnBiggerDragonNeeded(DragonTier _requiredTier) {
		// Init text!
		if(_requiredTier == DragonTier.COUNT) {
			m_text.text = Localization.Localize("TID_FEEDBACK_NEED_BIGGER_DRAGON");
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
			m_text.text = Localization.Localize("TID_FEEDBACK_NEED_TIER_DRAGON", replacement);
		}

		// Play the anim!
		m_anim.Show();

		// Reset timer!
		m_timer = m_duration;
	}
}
