// ResourcesFlowMissingCoinsPopup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/10/2018
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
public class ResourcesFlowMissingGFPopup : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Economy/PF_PopupMissingGF";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Localizer m_missingAmountText = null;
	[SerializeField] private Localizer m_openEggButtonText = null;

	// Events
	public UnityEvent OnFinish = new UnityEvent();

	// Internal
	private long m_buyEggPricePC = 0;
	private DefinitionNode m_buyEggDef = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get egg price from content
		m_buyEggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, Egg.SKU_PREMIUM_EGG);
		m_buyEggPricePC = m_buyEggDef.GetAsLong("pricePC");
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given data.
	/// </summary>
	/// <param name="_gfToBuy">Amount of golden fragments to buy.</param>
	public void Init(long _gfToBuy) {
		// Set missing amount text
		m_missingAmountText.Localize(
			m_missingAmountText.tid,
			StringUtils.FormatNumber(_gfToBuy)
		);

		// Set PC cost text
		m_openEggButtonText.Localize(
			m_openEggButtonText.tid,
			StringUtils.FormatNumber(m_buyEggPricePC)
		);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The buy egg button has been pressed.
	/// </summary>
	public void OnBuyEggButton() {
		// Launch purchase egg Flow
		ResourcesFlow purchaseFlow = new ResourcesFlow("BUY_EGG");
		purchaseFlow.OnSuccess.AddListener(
			(ResourcesFlow _flow) => {
				// Cancel parent resources flow (we can't control the output once gone to another screen)
				OnFinish.Invoke();

				// Play sound!
				AudioController.Play("UI_Buy");

				// Create a new egg instance
				Egg purchasedEgg = Egg.CreateFromDef(_flow.itemDef);
				purchasedEgg.ChangeState(Egg.State.READY);  // Already ready for collection!

				// Start open egg flow
				InstanceManager.menuSceneController.StartOpenEggFlow(purchasedEgg);
			}
		);
		purchaseFlow.Begin(m_buyEggPricePC, UserProfile.Currency.HARD, HDTrackingManager.EEconomyGroup.BUY_EGG, m_buyEggDef);
	}

	/// <summary>
	/// Cancel button has been pressed.
	/// </summary>
	public void OnCancelButton() {
		// Managed Externally
		OnFinish.Invoke();
	}
}