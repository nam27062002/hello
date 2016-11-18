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
	public enum CurrencyIcon {
		SC,
		PC,
		NONE
	}
	
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
	/// <param name="_icon">The curency icon to be displayed.</param>
	public void SetAmount(string _amountText, CurrencyIcon _icon) {
		// Skip if amount text is not set
		if(m_amountText == null) return;

		// Attach icon string
		switch(_icon) {
			case CurrencyIcon.NONE: {
				// Nothing to do
			} break;

			case CurrencyIcon.SC: {
				_amountText = UIConstants.TMP_SPRITE_SC + _amountText;
			} break;

			case CurrencyIcon.PC: {
				_amountText = UIConstants.TMP_SPRITE_PC + _amountText;
			} break;
		}

		// Set text
		m_amountText.text = _amountText;
	}

	/// <summary>
	/// Initialize the button with the given amount and currency.
	/// The amount will be formatted according to current localization settings.
	/// </summary>
	/// <param name="_amount">The amount to be displayed.</param>
	/// <param name="_icon">The curency icon to be displayed.</param>
	public void SetAmount(float _amount, CurrencyIcon _icon) {
		// Format number and call the string method
		string amountString = StringUtils.FormatNumber(_amount, 0);
		SetAmount(amountString, _icon);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}