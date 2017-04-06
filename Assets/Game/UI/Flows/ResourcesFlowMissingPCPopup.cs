// ResourcesFlowMissingPCPopup.cs
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
public class ResourcesFlowMissingPCPopup : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/ResourcesFlow/PF_PopupMissingPC";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed Members
	[SerializeField] private PopupCurrencyShopPill m_recommendedPackPill = null;

	// Events
	public UnityEvent OnRecommendedPackPurchased = new UnityEvent();
	public UnityEvent OnGoToShop = new UnityEvent();
	public UnityEvent OnCancel = new UnityEvent();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		m_recommendedPackPill.OnPurchaseSuccess.AddListener(OnPillPurchaseSuccess);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		m_recommendedPackPill.OnPurchaseSuccess.RemoveListener(OnPillPurchaseSuccess);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given data.
	/// </summary>
	/// <param name="_coinsToBuy">Amount of coins to buy.</param>
	/// <param name="_pricePC">PC price of the coins to buy.</param>
	public void Init(DefinitionNode _recommendedPackDef) {
		// Initialize recommended pack
		m_recommendedPackPill.InitFromDef(_recommendedPackDef);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// More offers button has been pressed.
	/// </summary>
	public void OnMoreOffersButton() {
		// Managed externally
		OnGoToShop.Invoke();
	}

	/// <summary>
	/// Cancel button has been pressed.
	/// </summary>
	public void OnCancelButton() {
		// Managed externally
		OnCancel.Invoke();
	}

	/// <summary>
	/// The purchase flow on the pill has sucessfully ended.
	/// </summary>
	/// <param name="_pill">The pill that triggered the event</param>
	private void OnPillPurchaseSuccess(PopupCurrencyShopPill _pill) {
		// Notify listeners
		OnRecommendedPackPurchased.Invoke();
	}
}