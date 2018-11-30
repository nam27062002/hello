// UIPriceSetup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/11/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to easily setup a price button for a dragon.
/// </summary>
[Serializable]
public class UIDragonPriceSetup {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public Localizer actionText = null;
	public TextMeshProUGUI priceText = null;
	public TextMeshProUGUI previousPriceText = null;
	[Space]
	[SerializeField] private GameObject[] m_toActivateOnDiscount = new GameObject[0];
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize with the given dragon data.
	/// </summary>
	/// <param name="_data">Dragon data.</param>
	/// <param name="_currency">Currency. Only SC and HC supported for now.</param>
	public void InitFromData(IDragonData _data, UserProfile.Currency _currency) {
		// Aux vars
		bool discountActive = _data.HasPriceModifier(_currency);

		// Final price
		if(priceText != null) {
			priceText.text = UIConstants.GetIconString(
				_data.GetPriceModified(_currency), 
				_currency, 
				UIConstants.IconAlignment.LEFT
			);
		}

		// Previous price - only show if there is a discount
		if(previousPriceText != null) {
			previousPriceText.text = StringUtils.FormatNumber(_data.GetPrice(_currency));
			previousPriceText.gameObject.SetActive(discountActive);
		}

		// Action text
		if(actionText != null) {
			if(discountActive) {
				actionText.Localize("TID_DRAGON_GET_NOW_DISCOUNT");
			} else {
				actionText.Localize("TID_DRAGON_GET_NOW");
			}
		}

		// Discount decos
		for(int i = 0; i < m_toActivateOnDiscount.Length; ++i) {
			if(m_toActivateOnDiscount[i] != null) {
				m_toActivateOnDiscount[i].SetActive(discountActive);
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}