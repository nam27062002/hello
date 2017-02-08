// MenuHUD.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller for the main menu HUD.
/// </summary>
public class ProfileCurrencyCounter : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Type {
		COINS,
		PC,
		GOLDEN_FRAGMENTS
	}

	public enum IconType {
		NONE,
		LEFT,
		RIGHT
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Setup
	[SerializeField] private Type m_type = Type.COINS;
	[SerializeField] private IconType m_iconType = IconType.RIGHT;	// Typical HUD top-right counter

	// References
	[Space]
	[SerializeField] private TextMeshProUGUI m_text = null;
	[SerializeField] private Animator m_anim = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Required references
		DebugUtils.Assert(m_text != null, "Required field!");	// Anim not required
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void OnEnable() {
		// Initialize text
		UpdateText();

		// Subscribe to external events
		switch(m_type) {
			case Type.COINS:	Messenger.AddListener<long, long>(GameEvents.PROFILE_COINS_CHANGED, OnAmountChanged);	break;
			case Type.PC:		Messenger.AddListener<long, long>(GameEvents.PROFILE_PC_CHANGED, OnAmountChanged);		break;
		}
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe to external events
		switch(m_type) {
			case Type.COINS:	Messenger.RemoveListener<long, long>(GameEvents.PROFILE_COINS_CHANGED, OnAmountChanged);	break;
			case Type.PC:		Messenger.RemoveListener<long, long>(GameEvents.PROFILE_PC_CHANGED, OnAmountChanged);		break;
		}
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Update the text with the current value from the profile.
	/// </summary>
	private void UpdateText() {
		// Select text and icon based on currency
		string text = "";
		string iconString = "";
		switch(m_type) {
			case Type.COINS: {
				text = StringUtils.FormatNumber(UsersManager.currentUser.coins);
				iconString = UIConstants.TMP_SPRITE_SC;
			} break;

			case Type.PC: {
				text = StringUtils.FormatNumber(UsersManager.currentUser.pc);
				iconString = UIConstants.TMP_SPRITE_PC;
			} break;

			case Type.GOLDEN_FRAGMENTS: {
				text = LocalizationManager.SharedInstance.Localize("TID_FRACTION", StringUtils.FormatNumber(EggManager.goldenEggFragments), StringUtils.FormatNumber(EggManager.goldenEggRequiredFragments));
				iconString = UIConstants.TMP_SPRITE_GOLDEN_EGG_FRAGMENT;
			} break;
		}

		// Apply to textfield based on icon type
		switch(m_iconType) {
			case IconType.NONE: {
				m_text.text = text;
			} break;

			case IconType.LEFT: {
				m_text.text = iconString + text;
			} break;

			case IconType.RIGHT: {
				m_text.text = text + iconString;
			} break;
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The amount of the target currency has changed in the profile.
	/// </summary>
	/// <param name="_oldAmount">Previous amount.</param>
	/// <param name="_newAmount">Current amount.</param>
	private void OnAmountChanged(long _oldAmount, long _newAmount) {
		// Update text
		UpdateText();

		// Launch anim
		if(m_anim != null) {
			m_anim.SetTrigger("start");
		}
	}
}