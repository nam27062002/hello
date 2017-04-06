// BuyEggButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/09/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Standard behaviour for all the "Buy Egg" buttons throughout the menu.
/// </summary>
public class BuyEggButton : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private TMPro.TextMeshProUGUI m_priceText = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Set price
		DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, Egg.SKU_PREMIUM_EGG);
		m_priceText.text = UIConstants.GetIconString(eggDef.GetAsLong("pricePC"), UIConstants.IconType.PC, UIConstants.IconAlignment.LEFT);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The button has been pressed.
	/// </summary>
	public void OnBuyEgg() {
		// Get price and start purchase flow
		DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, Egg.SKU_PREMIUM_EGG);
		ResourcesFlow purchaseFlow = new ResourcesFlow("BUY_EGG");
		purchaseFlow.OnSuccess.AddListener(
			(ResourcesFlow _flow) => {
				// Create a new egg instance
				Egg purchasedEgg = Egg.CreateFromDef(_flow.itemDef);
				purchasedEgg.ChangeState(Egg.State.READY);	// Already ready for collection!

				// Start open egg flow
				InstanceManager.sceneController.GetComponent<MenuScreensController>().StartOpenEggFlow(purchasedEgg);
			}
		);
		purchaseFlow.Begin(eggDef.GetAsLong("pricePC"), UserProfile.Currency.HARD, eggDef);
	}
}