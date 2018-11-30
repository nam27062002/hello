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
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize with the given dragon data.
	/// </summary>
	/// <param name="_data">Dragon data.</param>
	/// <param name="_currency">Currency. Only SC and HC supported for now.</param>
	public void InitFromData(IDragonData _data, UserProfile.Currency _currency) {
		// Final price
		if(priceText != null) {
			priceText.text = UIConstants.GetIconString(
				_data.GetPriceModified(_currency), 
				_currency, 
				UIConstants.IconAlignment.LEFT
			);
		}

		// Previous price - only show if there is a discount
		bool discountActive = _data.HasPriceModifier(_currency);
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
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}