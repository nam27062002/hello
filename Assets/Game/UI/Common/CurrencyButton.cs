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
	[Space]
	[SerializeField] private TextMeshProUGUI m_amountText = null;
	public TextMeshProUGUI amountText {
		get { return m_amountText; }
	}

    [SerializeField] private Text m_amountTextUnity = null;     // [AOC] Alternative version for Unity textfields, useful for real-currency
    public Text amountTextUnity
    {
        get { return m_amountTextUnity; }
    }

    [Space]

    [SerializeField] private TextMeshProUGUI m_previousAmountText = null;
    public TextMeshProUGUI previousAmountText
    {
        get { return m_previousAmountText; }
    }

    [SerializeField] private Text m_previousAmountTextUnity = null;     // [AOC] Alternative version for Unity textfields, useful for real-currency
    public Text previousAmountTextTextUnity
    {
        get { return m_previousAmountTextUnity; }
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
    public void SetAmount(string _amountText, UserProfile.Currency _currency, string _previousAmountText = null)
    {
        // Set text (UIConstants makes it easy for us!)
        string text = UIConstants.GetIconString(_amountText, UIConstants.GetCurrencyIcon(_currency), UIConstants.IconAlignment.LEFT);

        // Update any defined textfield
        if (m_amountText != null)
        {
            m_amountText.text = text;
        }

        if (m_amountTextUnity != null)
        {
            m_amountTextUnity.text = text;
        }

        // Price before discount (Text mesh pro component)
        if (m_previousAmountText != null)
        { 
            if (!string.IsNullOrEmpty(_previousAmountText))
            {
                m_previousAmountText.gameObject.SetActive(true);
                m_previousAmountText.text = _previousAmountText;
            }
            else
            {
                m_previousAmountText.gameObject.SetActive(false);
            }
        }

        // Price before discount (unity text component)
        if (m_previousAmountTextUnity != null)
        {
            if (!string.IsNullOrEmpty(_previousAmountText))
            {
                m_previousAmountTextUnity.gameObject.SetActive(true);
                m_previousAmountTextUnity.text = _previousAmountText;
            }
            else
            {
                m_previousAmountTextUnity.gameObject.SetActive(false);
            }
        }
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