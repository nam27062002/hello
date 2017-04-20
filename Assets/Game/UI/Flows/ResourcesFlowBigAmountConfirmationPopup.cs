// ResourcesFlowBigAmountConfirmationPopup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/04/2017.
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
public class ResourcesFlowBigAmountConfirmationPopup : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/ResourcesFlow/PF_PopupBigAmountConfirmation";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Localizer m_messageText = null;
	[SerializeField] private TextMeshProUGUI m_amountText = null;

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
	/// <param name="_pcAmount">Amount of PC to spend.</param>
	public void Init(long _pcAmount) {
		// Create price tag text
		string priceTag = UIConstants.GetIconString(_pcAmount, UserProfile.Currency.HARD, UIConstants.IconAlignment.LEFT);

		// Set message text
		m_messageText.Localize("TID_RESOURCES_FLOW_BIG_AMOUNT_CONFIRMATION_MESSAGE", priceTag);

		// Set amount text
		m_amountText.text = priceTag;
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