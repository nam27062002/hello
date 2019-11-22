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

    // Internal
    private bool transactionInProgress = false;

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

        // Avoid multiple purchases when spamming the buy button
        if (transactionInProgress)
            return;

        if (InstanceManager.menuSceneController.transitionManager.transitionAllowed)
        {
			transactionInProgress = true;

			// Get price and start purchase flow
			DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, Egg.SKU_PREMIUM_EGG);

            // Build the purchase flow
            ResourcesFlow purchaseFlow = new ResourcesFlow("BUY_EGG");
			purchaseFlow.confirmationPopupBehaviour = ResourcesFlow.ConfirmationPopupBehaviour.IGNORE_THRESHOLD;    // [AOC] Because the buy egg button is pretty close to the "play" button, accidental taps can happen. Force confirmation popup regardless of the egg price.
            purchaseFlow.OnSuccess.AddListener(OnPurchaseSuccess);
            purchaseFlow.OnFinished.AddListener(OnPurchaseFinished);
            purchaseFlow.Begin(
				eggDef.GetAsLong("pricePC"),
				UserProfile.Currency.HARD,
				HDTrackingManager.EEconomyGroup.BUY_EGG,
				eggDef
			);
        }
	}


    /// <summary>
    /// To be called when the egg purchase was successfully completed.
    /// It will give the egg to the player.
    /// </summary>
    private void OnPurchaseSuccess (ResourcesFlow _flow)
    {
            // Play sound!
            AudioController.Play("UI_Buy");

            // Create a new egg instance
            Egg purchasedEgg = Egg.CreateFromDef(_flow.itemDef);
            purchasedEgg.ChangeState(Egg.State.READY);  // Already ready for collection!

            // Start open egg flow
            InstanceManager.menuSceneController.StartOpenEggFlow(purchasedEgg);
    }


    /// <summary>
    /// To be called when the egg purchase has finished. Failed or successful, it doesnt matter.
    /// </summary>
    private void OnPurchaseFinished (ResourcesFlow _flow)
    {
        transactionInProgress = false;
    }
}