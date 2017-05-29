// ResourcesFlowBigAmountConfirmationPopup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
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
	[SerializeField] private Localizer m_buttonText = null;
	[SerializeField] private Toggle m_dontShowAgainToggle = null;

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
		// Format price
		string priceTag = StringUtils.FormatNumber(_pcAmount);

		// Set message text
		m_messageText.Localize(m_messageText.tid, priceTag);

		// Set amount text
		m_buttonText.Localize(m_buttonText.tid, priceTag);

		// Initialize toggle
		m_dontShowAgainToggle.isOn = !GameSettings.showBigAmountConfirmationPopup;	// Inverse
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

	/// <summary>
	/// The "Don't show again" toggle has been changed.
	/// </summary>
	/// <param name="_newValue">New value of the toggle.</param>
	public void OnDontShowAgainToggled(bool _newValue) {
		// Store settings
		GameSettings.showBigAmountConfirmationPopup = !_newValue;	// Inverse!
	}
}