// CurrencyButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/11/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

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
/// Auxiliar component to quickly access popular components used in a currency button.
/// </summary>
public class CurrencyButton : AnimatedButton {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private TextMeshProUGUI m_amountText = null;
	public TextMeshProUGUI amountText {
		get { return m_amountText; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the button with the given amount text (already formatted) and currency.
	/// </summary>
	/// <param name="_amount">The amount to be displayed.</param>
	/// <param name="_currency">The curency to be displayed.</param>
	public void SetAmount(string _amountText, UserProfile.Currency _currency) {
		// Skip if amount text is not set
		if(m_amountText == null) return;

		// Set text (UIConstants makes it easy for us!)
		m_amountText.text = UIConstants.GetIconString(_amountText, UIConstants.GetCurrencyIcon(_currency), UIConstants.IconAlignment.LEFT);
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
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}