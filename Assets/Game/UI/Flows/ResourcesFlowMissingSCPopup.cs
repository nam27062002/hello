// ResourcesFlowMissingCoinsPopup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar popup to the ResourcesFlow.
/// </summary>
public class ResourcesFlowMissingSCPopup : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Economy/PF_PopupMissingSC";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private CurrencyButton m_pcButton = null;
	[SerializeField] private TextMeshProUGUI m_missingCoinsText = null;

	// Events
	public UnityEvent OnAccept = new UnityEvent();
	public UnityEvent OnCancel = new UnityEvent();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given data.
	/// </summary>
	/// <param name="_coinsToBuy">Amount of coins to buy.</param>
	/// <param name="_pricePC">PC price of the coins to buy.</param>
	public void Init(long _coinsToBuy, long _pricePC) {
		// Set missing coins text
		m_missingCoinsText.text = UIConstants.GetIconString(_coinsToBuy, UIConstants.IconType.COINS, UIConstants.IconAlignment.LEFT);

		// Set PC cost text
		m_pcButton.SetAmount(_pricePC, UserProfile.Currency.HARD);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Suggested pack button has been pressed.
	/// </summary>
	public void OnAcceptButton() {
		// Managed Externally
		OnAccept.Invoke();
	}

	/// <summary>
	/// Cancel button has been pressed.
	/// </summary>
	public void OnCancelButton() {
		// Managed Externally
		OnCancel.Invoke();
	}
}