// MultiCurrencyButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar component to quickly setup a multi-currency button setup.
/// </summary>
public class MultiCurrencyButton : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private CurrencyButton[] m_buttons = new CurrencyButton[(int)UserProfile.Currency.COUNT];
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the button with the given amount text (already formatted) and currency.
	/// Will show/hide buttons according to target currency.
	/// </summary>
	/// <param name="_amount">The amount to be displayed.</param>
	/// <param name="_currency">The curency to be displayed.</param>
	public void SetAmount(string _amountText, UserProfile.Currency _currency, string _previousAmountText = null) {
		// Refresh text for the corresponding button
		int currencyIdx = (int)_currency;
		if(m_buttons[currencyIdx] != null) {
			m_buttons[currencyIdx].SetAmount(_amountText, _currency, _previousAmountText);
		}

		// Show/Hide buttons
		SetButtonsVisibility(_currency);
	}

	/// <summary>
	/// Initialize the button with the given amount and currency.
	/// The amount will be formatted according to current localization settings.
	/// </summary>
	/// <param name="_amount">The amount to be displayed.</param>
	/// <param name="_currency">The curency to be displayed.</param>
	public void SetAmount(float _amount, UserProfile.Currency _currency) {
		// Format number and call the string method
		string amountString = StringUtils.FormatNumber(_amount, 0);
		SetAmount(amountString, _currency);
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Toggle buttons on/off based on target currency.
	/// </summary>
	/// <param name="_currency">Currency to be displayed.</param>
	private void SetButtonsVisibility(UserProfile.Currency _currency) {
		// Check all buttons
		bool show = false;
		for(int i = 0; i < m_buttons.Length; i++) {
			// Skip unset buttons
			if(m_buttons[i] == null) continue;

			// Show only if it matches target currency
			show = (i == (int)_currency);
			// [AOC] TODO!! Animator doesn't always work, specially when calling this during the Awake() function. Disable for now.
			/*if(m_buttons[i].animator != null) {
				m_buttons[i].animator.Set(show);
			} else {
				m_buttons[i].gameObject.SetActive(show);
			}*/
			m_buttons[i].gameObject.SetActive(show);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}