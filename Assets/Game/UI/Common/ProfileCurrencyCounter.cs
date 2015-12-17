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
		PC
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Setup
	[SerializeField] private Type m_type = Type.COINS;

	// References
	[SerializeField] private Text m_text = null;
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
		switch(m_type) {
			case Type.COINS: {
				m_text.text = StringUtils.FormatNumber(UserProfile.coins);
			} break;

			case Type.PC: {
				m_text.text = StringUtils.FormatNumber(UserProfile.pc);
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