// ResultsScreenStepSkinUnlocked.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/09/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepSkinUnlocked : ResultsScreenSequenceStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Space]
	[SerializeField] private MenuDragonLoader m_preview = null;
	[SerializeField] private DragControlRotation m_dragControl = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_skinNameText = null;
	[SerializeField] private PowerIcon m_powerIcon = null;
	[SerializeField] private MultiCurrencyButton m_purchaseButton = null;
	[SerializeField] private ShowHideAnimator m_shareButtonGroup = null;

	// Internal
	private List<DefinitionNode> m_skinsToProcess = new List<DefinitionNode>();
	private int m_processedSkins = 0;
	
	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {
		// Only if there actually are skins to display
		return m_skinsToProcess.Count > 0;
	}

	/// <summary>
	/// Init this step.
	/// </summary>
	override protected void DoInit() {
		// Only for CLASSIC dragons!
		Debug.Assert(DragonManager.CurrentDragon.type == IDragonData.Type.CLASSIC, "ONLY FOR CLASSIC DRAGONS!");

		// Aux vars
	    DragonProgression progression = (DragonManager.CurrentDragon as DragonDataClassic).progression;

		// Gather levels before and after the run
		// Consider cheats!
		int initialLevel = RewardManager.dragonInitialLevel;
		int finalLevel = progression.level;
		if(CPResultsScreenTest.testEnabled) {
			// Initial level and delta
			float initialLevelRaw = Mathf.Lerp(0, progression.maxLevel, CPResultsScreenTest.xpInitialDelta);
			initialLevel = Mathf.FloorToInt(initialLevelRaw);

			// Special case for last level (should only happen with delta >= 1f)
			if(initialLevel >= progression.maxLevel) {
				initialLevel = progression.maxLevel;
			}

			// Do the same with the final level and delta
			float finalLevelRaw = Mathf.Lerp(0, progression.maxLevel, CPResultsScreenTest.xpFinalDelta);
			finalLevel = Mathf.FloorToInt(finalLevelRaw);

			// Special case for last level (should only happen with delta >= 1f)
			if(finalLevel >= progression.maxLevel) {
				finalLevel = progression.maxLevel;
			}
		}

		// If we haven't level up during the game, it's impossible to have unlocked any skin
		if(initialLevel == finalLevel) {
			return;
		}

		// Find out all skins unlocked in this run
		List<DefinitionNode> allSkins = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", DragonManager.CurrentDragon.def.sku);
		DefinitionsManager.SharedInstance.SortByProperty(ref allSkins, "unlockLevel", DefinitionsManager.SortType.NUMERIC);
		for(int i = 0; i < allSkins.Count; ++i) {
			// Skip if unlockLevel is 0 (default skin)
			int unlockLevel = allSkins[i].GetAsInt("unlockLevel");
			if(unlockLevel <= 0) continue;

			// Skip if already owned
			if(UsersManager.currentUser.wardrobe.GetSkinState(allSkins[i].sku) == Wardrobe.SkinState.OWNED) continue;

			// Check unlock level vs level before starting the game and level after the game
			if(unlockLevel > initialLevel && unlockLevel <= finalLevel) {
				// This skin has been unlocked during this run!
				// Add it to the list
				m_skinsToProcess.Add(allSkins[i]);
			}
		}

		// Reset processed skins counter
		m_processedSkins = 0;
	}

	/// <summary>
	/// Launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Load data of current skin
		DefinitionNode def = m_skinsToProcess[m_processedSkins];

		// Skin name
		m_skinNameText.text = def.GetLocalized("tidName");

		// Skin preview
		m_preview.LoadDragon(def.Get("dragonSku"), def.sku);
		m_preview.dragonInstance.allowAltAnimations = false;	// [AOC] Disable weird alt animations for now

		// Power
		string powerSku = def.GetAsString("powerup");
		DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, powerSku);
		m_powerIcon.InitFromDefinition(powerDef, false);	// [AOC] Powers are not locked anymore

		// Price
		float priceSC = def.GetAsFloat("priceSC");
		float pricePC = def.GetAsFloat("priceHC");
		if(pricePC > priceSC) {
			m_purchaseButton.SetAmount(pricePC, UserProfile.Currency.HARD);
		} else {
			m_purchaseButton.SetAmount(priceSC, UserProfile.Currency.SOFT);
		}

		// Start with purchase button hidden
		m_purchaseButton.GetComponent<ShowHideAnimator>().ForceHide(false);

		// Start with share button hidden
		m_shareButtonGroup.ForceHide(false);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Hide animation has finished.
	/// </summary>
	override protected void OnSequenceFinished() {
		// Mark current skin as processed
		m_processedSkins++;

		// If there are still skins to be displayed, relaunch the screen
		// Otherwise mark step as finished
		if(m_processedSkins < m_skinsToProcess.Count) {
			Launch();
		} else {
			// Save!
			PersistenceFacade.instance.Save_Request(true);
			OnFinished.Invoke();
		}
	}

	/// <summary>
	/// The button to purchase current skin has been pressed.
	/// </summary>
	public void OnPurchaseSkinButton() {
		// Aux vars
		DefinitionNode def = m_skinsToProcess[m_processedSkins];

		// All checks passed, get price and currency
		long priceSC = def.GetAsLong("priceSC");
		long pricePC = def.GetAsLong("priceHC");
		bool isPC = pricePC > priceSC;

		// Perform transaction
		// Get price and start purchase flow
		ResourcesFlow purchaseFlow = new ResourcesFlow("ACQUIRE_DISGUISE");
		purchaseFlow.OnSuccess.AddListener(OnPurchaseSuccess);
		if(isPC) {
			purchaseFlow.Begin(pricePC, UserProfile.Currency.HARD, HDTrackingManager.EEconomyGroup.ACQUIRE_DISGUISE, def);
		} else {
			purchaseFlow.Begin(priceSC, UserProfile.Currency.SOFT, HDTrackingManager.EEconomyGroup.ACQUIRE_DISGUISE, def);
		}
	}

	/// <summary>
	/// The purchase resources flow has finished successfully.
	/// </summary>
	/// <param name="_flow">The flow that triggered the callback.</param>
	private void OnPurchaseSuccess(ResourcesFlow _flow) {
		// Acquire and equip the skin!
		// [AOC] Unless testing!
		if(!CPResultsScreenTest.testEnabled) {
			// Acquire it!
			UsersManager.currentUser.wardrobe.SetSkinState(_flow.itemDef.sku, Wardrobe.SkinState.OWNED);

			// Immediately equip it!
			UsersManager.currentUser.EquipDisguise(DragonManager.CurrentDragon.def.sku, _flow.itemDef.sku, true);
		}

		// Throw out some fireworks!
		m_controller.scene.LaunchConfettiFX(true);

		// Hide the button to prevent spamming
		m_purchaseButton.GetComponent<ShowHideAnimator>().ForceHide();

		// Show share button
		m_shareButtonGroup.ForceShow();
	}

	/// <summary>
	/// The share button has been pressed.
	/// </summary>
	public void OnShareButton() {
		// Initialize and open share screen
		ShareScreenDragon shareScreen = ShareScreensManager.GetShareScreen("skin_acquired") as ShareScreenDragon;
		shareScreen.Init(
			"skin_acquired",
			SceneController.GetMainCameraForCurrentScene(),
			IDragonData.CreateFromSkin(m_skinsToProcess[m_processedSkins].sku),    // Create a sample dragon data object to initialize the share screen
			true,
			null
		);

		shareScreen.TakePicture();
	}

	/// <summary>
	/// Callback to rescale particles from the animation sequence.
	/// </summary>
	public void PreviewScaledFinished()
	{
		ParticleScaler[] scalers = m_preview.GetComponentsInChildren<ParticleScaler>();
		for( int i = 0;i<scalers.Length; ++i )
		{
			scalers[i].DoScale();
		}
	}
}