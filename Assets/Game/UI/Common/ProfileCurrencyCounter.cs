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
using System.Text;

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

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Setup
	[SerializeField] private Type m_type = Type.COINS;
	[SerializeField] private UIConstants.IconAlignment m_alignment = UIConstants.IconAlignment.RIGHT;	// Typical HUD top-right counter

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
		// UIConstants does the job for us
		switch(m_type) {
			case Type.COINS: {
				m_text.text = UIConstants.GetIconString(
					UsersManager.currentUser.coins,
					UIConstants.IconType.COINS, m_alignment
				);
			} break;

			case Type.PC: {
				m_text.text = UIConstants.GetIconString(
					UsersManager.currentUser.pc,
					UIConstants.IconType.PC, m_alignment
				);
			} break;

			case Type.GOLDEN_FRAGMENTS: {
				m_text.text = UIConstants.GetIconString(
					LocalizationManager.SharedInstance.Localize(
						"TID_FRACTION", 
						StringUtils.FormatNumber(EggManager.goldenEggFragments), 
						StringUtils.FormatNumber(EggManager.goldenEggRequiredFragments)
					),
					UIConstants.IconType.GOLDEN_FRAGMENTS, m_alignment
				);

				// Hide if all golden eggs have been collected (but don't disable, otherwise it will never again be enabled!)
				this.gameObject.ForceGetComponent<CanvasGroup>().alpha = EggManager.allGoldenEggsCollected ? 0f : 1f;
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