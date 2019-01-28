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

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Setup
	[SerializeField] private UserProfile.Currency m_currency = UserProfile.Currency.SOFT;
	[SerializeField] private UIConstants.IconAlignment m_alignment = UIConstants.IconAlignment.RIGHT;	// Typical HUD top-right counter

	// References
	[Space]
	[SerializeField] private TextMeshProUGUI m_text = null;
	[SerializeField] private Animator m_anim = null;

	// More setup
	[Space]
	[SerializeField] private Color m_colorWhenMinReached = Color.red;
	[SerializeField] private Color m_colorWhenMaxReached = Color.green;

	// Internal
	private NumberTextAnimator m_textAnim = null;
	private Color m_defaultColor = Color.white;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Required references
		DebugUtils.Assert(m_text != null, "Required field!");	// Anim not required

		// Store original color
		m_defaultColor = m_text.color;

		// If the text has a number animator linked, store it and assign it a custom text setter
		m_textAnim = m_text.GetComponent<NumberTextAnimator>();
		if(m_textAnim != null) {
			m_textAnim.CustomTextSetter = TextAnimatorSetter;
			m_textAnim.OnFinished.AddListener(OnTextAnimFinished);
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void OnEnable() {
		// Initialize text
		UpdateText(false);

		// Subscribe to external events
		Messenger.AddListener<UserProfile.Currency, long, long>(MessengerEvents.PROFILE_CURRENCY_CHANGED, OnAmountChanged);
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe to external events
		Messenger.RemoveListener<UserProfile.Currency, long, long>(MessengerEvents.PROFILE_CURRENCY_CHANGED, OnAmountChanged);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Clear text anim custom setter
		if(m_textAnim != null) {
			m_textAnim.CustomTextSetter = null;
			m_textAnim.OnFinished.RemoveListener(OnTextAnimFinished);
		}
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Update the text with the current value from the profile.
	/// </summary>
	/// <param name="_animate">Whether to animate the number or not.</param>
	private void UpdateText(bool _animate) {
		// Apply target amount based on currency
		long amount = UsersManager.currentUser.GetCurrency(m_currency);

		// Using a text animator?
		if(m_textAnim != null) {
			m_textAnim.SetValue(amount, _animate);
		} else {
			SetText(amount);
		}

		// Launch normal animation as well
		if(_animate && m_anim != null) {
			m_anim.SetTrigger( GameConstants.Animator.START );
		}
	}

	/// <summary>
	/// Set the text immediately with the given amount.
	/// </summary>
	/// <param name="_amount">Amount to be displayed.</param>
	private void SetText(long _amount) {
		// Aux vars
		UserProfile.CurrencyData currencyData = UsersManager.currentUser.GetCurrencyData(m_currency);

		// Select text and icon based on currency
		// UIConstants does the job for us
		switch(m_currency) {
			case UserProfile.Currency.SOFT:
			case UserProfile.Currency.HARD: 
            case UserProfile.Currency.GOLDEN_FRAGMENTS: {
				m_text.text = UIConstants.GetIconString(_amount, m_currency, m_alignment);
			} break;

			case UserProfile.Currency.KEYS: {
				m_text.text = UIConstants.GetIconString(
					LocalizationManager.SharedInstance.Localize(
						"TID_FRACTION", 
						StringUtils.FormatNumber(_amount), 
						StringUtils.FormatNumber(currencyData.max)
					),
					UIConstants.IconType.KEYS, m_alignment
				);
			} break;
		}

		// Apply proper color based on limits
		if(_amount < currencyData.min) {
			m_text.color = m_colorWhenMinReached;
		} else if(currencyData.max > 0 && _amount > currencyData.max) {
			m_text.color = m_colorWhenMaxReached;
		} else {
			m_text.color = m_defaultColor;
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The amount of the target currency has changed in the profile.
	/// </summary>
	/// <param name="_currency">Which currency?</param>
	/// <param name="_oldAmount">Previous amount.</param>
	/// <param name="_newAmount">Current amount.</param>
	private void OnAmountChanged(UserProfile.Currency _currency, long _oldAmount, long _newAmount) {
		// If currency matches, update text
		if(_currency == m_currency) {
			UpdateText(true);
		}
	}

	/// <summary>
	/// Custom text setter for a number text animator.
	/// </summary>
	/// <param name="_textAnim">The text animator that triggered the event.</param>
	private void TextAnimatorSetter(NumberTextAnimator _textAnim) {
		// Call internal text setter with animator's current value
		SetText(m_textAnim.currentValue);
	}

	/// <summary>
	/// The text animator has finished.
	/// </summary>
	private void OnTextAnimFinished() {
	}
}