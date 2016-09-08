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
	[SerializeField] private Text m_priceText = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Set price
		DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, Egg.SKU_PREMIUM_EGG);
		m_priceText.text = StringUtils.FormatNumber(eggDef.GetAsInt("pricePC"));
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The button has been pressed.
	/// </summary>
	public void OnBuyEgg() {
		// SFX
		AudioManager.instance.PlayClip("audio/sfx/UI/hsx_ui_button_select");

		// Get price and start purchase flow
		DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, Egg.SKU_PREMIUM_EGG);
		long pricePC = eggDef.GetAsLong("pricePC");
		if(UsersManager.currentUser.pc >= pricePC) {
			// Perform transaction
			UsersManager.currentUser.AddPC(-pricePC);
			PersistenceManager.Save();

			// Create a new egg instance
			Egg purchasedEgg = Egg.CreateFromDef(eggDef);
			purchasedEgg.ChangeState(Egg.State.READY);	// Already ready for collection!

			// Start open egg flow
			InstanceManager.sceneController.GetComponent<MenuScreensController>().StartOpenEggFlow(purchasedEgg);
		} else {
			// Open PC shop popup
			//PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);

			// Currency popup / Resources flow disabled for now
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PC_NOT_ENOUGH"), new Vector2(0.5f, 0.33f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}
}