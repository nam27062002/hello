// IncubatorScreenController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global controller for the Incubator screen in the main menu.
/// </summary>
public class IncubatorScreenController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[SerializeField] private Button m_buyButton = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events.
		Messenger.AddListener<NavigationScreenSystem.ScreenChangedEvent>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnNavigationScreenChanged);

		// Price
		DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, "egg_dragon_titan");	// [AOC] TODO!! Single egg def
		m_buyButton.FindComponentRecursive<Text>("TextCost").text = StringUtils.FormatNumber(eggDef.GetAsInt("pricePC"));
	}

	/// <summary>
	/// Raises the disable event.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe to external events.
		Messenger.RemoveListener<NavigationScreenSystem.ScreenChangedEvent>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnNavigationScreenChanged);
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Start open flow on one of the eggs in the incubator, if possible.
	/// </summary>
	/// <returns>Whether the opening process was started or not.</returns>
	/// <param name="_egg">The egg to be opened.</param>
	public bool StartOpenEggFlow(Egg _egg) {
		// Just in case, shouldn't happen anything if there is no egg incubating or it is not ready
		if(_egg == null || _egg.state != Egg.State.READY) return false;

		// Try to reuse current egg view attached to the incubator
		EggController eggView = null;
		MenuScreensController screensController = InstanceManager.sceneController.GetComponent<MenuScreensController>();
		IncubatorScreenScene incubatorScene = screensController.GetScene((int)MenuScreens.INCUBATOR) as IncubatorScreenScene;
		for(int i = 0; i < EggManager.INVENTORY_SIZE; i++) {
			if(EggManager.inventory[i] == null) continue;
			if(EggManager.inventory[i].state == Egg.State.READY) {
				eggView = incubatorScene.eggAnchors[i].eggView;
				break;
			}
		}

		// Go to OPEN_EGG screen and start open flow
		OpenEggScreenController openEggScreen = screensController.GetScreen((int)MenuScreens.OPEN_EGG).GetComponent<OpenEggScreenController>();
		openEggScreen.StartFlow(_egg, eggView);
		screensController.GoToScreen((int)MenuScreens.OPEN_EGG);

		return true;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Navigation screen has changed (animation starts now).
	/// </summary>
	/// <param name="_event">Event data.</param>
	private void OnNavigationScreenChanged(NavigationScreenSystem.ScreenChangedEvent _event) {
		// Only if it comes from the main screen navigation system
		if(_event.dispatcher != InstanceManager.GetSceneController<MenuSceneController>().screensController) return;

		// If leaving this screen, remove "new" flag from eggs
		if(_event.fromScreenIdx == (int)MenuScreens.INCUBATOR) {
			for(int i = 0; i < EggManager.INVENTORY_SIZE; i++) {
				if(EggManager.inventory[i] != null) {
					EggManager.inventory[i].isNew = false;
				}
			}
		}
	}

	/// <summary>
	/// Buy egg button has been pressed.
	/// </summary>
	public void OnBuyEgg() {
		// SFX
		AudioManager.instance.PlayClip("audio/sfx/UI/hsx_ui_button_select");

		// Get price and start purchase flow
		DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, "egg_dragon_titan");	// [AOC] TODO!! Single egg def
		long pricePC = eggDef.GetAsLong("pricePC");
		if(UsersManager.currentUser.pc >= pricePC) {
			// Perform transaction
			UsersManager.currentUser.AddPC(-pricePC);
			PersistenceManager.Save();

			// Create a new egg instance
			Egg purchasedEgg = Egg.CreateFromDef(eggDef);
			purchasedEgg.ChangeState(Egg.State.READY);	// Already ready for collection!

			// Go to OPEN_EGG screen and start flow
			MenuScreensController screensController = InstanceManager.sceneController.GetComponent<MenuScreensController>();
			OpenEggScreenController openEggScreen = screensController.GetScreen((int)MenuScreens.OPEN_EGG).GetComponent<OpenEggScreenController>();
			screensController.GoToScreen((int)MenuScreens.OPEN_EGG);
			openEggScreen.StartFlow(purchasedEgg);
		} else {
			// Open PC shop popup
			//PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);

			// Currency popup / Resources flow disabled for now
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PC_NOT_ENOUGH"), new Vector2(0.5f, 0.33f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}
}