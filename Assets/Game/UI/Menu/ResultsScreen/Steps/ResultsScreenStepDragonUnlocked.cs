// ResultsScreenStepDragonUnlocked.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/09/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepDragonUnlocked : ResultsScreenSequenceStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Space]
	[SerializeField] private MenuDragonLoader m_preview = null;
	[SerializeField] private DragControlRotation m_dragControl = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_dragonNameText = null;
	[SerializeField] private Image m_dragonTierIcon = null;
	[SerializeField] private GameObject m_newPreysInfo = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_healthText = null;
	[SerializeField] private TextMeshProUGUI m_energyText = null;
	[SerializeField] private TextMeshProUGUI m_speedText = null;
	[Space]
	[SerializeField] private ShowHideAnimator m_shareButtonAnim = null;
	[SerializeField] private ShowHideAnimator m_purchaseButtonsAnim = null;
	[SerializeField] private UIDragonPriceSetup m_scPriceSetup = null;
	[SerializeField] private UIDragonPriceSetup m_hcPriceSetup = null;

	// Internal
	private IDragonData m_dragonData = null;
	private bool m_dragonUnlocked = false;
	
	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {
		// Has next dragon been unlocked during this run?
		return m_dragonUnlocked;
	}

	/// <summary>
	/// Init this step.
	/// </summary>
	override protected void DoInit() {
		// Get next dragon's data
		m_dragonData = DragonManager.GetNextDragonData(DragonManager.currentDragon.def.sku);

		// Has next dragon been unlocked during this run?
		// IMpossible if there is no next dragon
		m_dragonUnlocked = false;
		if(m_dragonData != null) {
			bool isLocked = m_dragonData.isLocked;
			bool wasLocked = RewardManager.nextDragonLocked;
			if(CPResultsScreenTest.testEnabled) {
				// Testing!
				isLocked = CPResultsScreenTest.xpFinalDelta < 1f;
				wasLocked = CPResultsScreenTest.nextDragonLocked;
			}
			m_dragonUnlocked = wasLocked && !isLocked;
		}

		// If a new dragon was unlocked, tell the menu to show the dragon unlocked screen first!
		if(m_dragonUnlocked) {
			GameVars.unlockedDragonSku = m_dragonData.def.sku;						
		}
	}

	/// <summary>
	/// Launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Just in case
		if(m_dragonData == null) {
			OnFinished.Invoke();
			return;
		}

		// Aux vars
		DefinitionNode def = m_dragonData.def;

		// Dragon name
		m_dragonNameText.text = def.GetLocalized("tidName");

		// Dragon preview
		m_preview.LoadDragon(def.sku);
		m_preview.dragonInstance.allowAltAnimations = false;	// [AOC] Disable weird alt animations for now

		// Tier info
		if(m_dragonTierIcon != null) m_dragonTierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, m_dragonData.tierDef.GetAsString("icon"));

		// If the unlocked dragon is of different tier as the dragon used to unlocked it, show 'new preys' banner
		if(m_newPreysInfo != null) {
			DefinitionNode previousDragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, def.GetAsString("previousDragonSku"));
			if(previousDragonDef != null && previousDragonDef.Get("tier") != m_dragonData.tierDef.sku) {
				m_newPreysInfo.SetActive(true);
			} else {
				m_newPreysInfo.SetActive(false);
			}
		}

		// Stats
		if(m_healthText != null) m_healthText.text = StringUtils.FormatNumber(m_dragonData.maxHealth, 0);
		if(m_energyText != null) m_energyText.text = StringUtils.FormatNumber(m_dragonData.baseEnergy, 0);
		if(m_speedText != null) m_speedText.text = StringUtils.FormatNumber(m_dragonData.maxSpeed * 10f, 0);	// x10 to show nicer numbers

		// Update sc unlock button
		if(m_scPriceSetup != null) {
			// [AOC] UIDragonPriceSetup makes it easy for us!
			m_scPriceSetup.InitFromData(m_dragonData, UserProfile.Currency.SOFT);
		}

		// Update hc unlock button
		if(m_hcPriceSetup != null) {
			// [AOC] UIDragonPriceSetup makes it easy for us!
			m_hcPriceSetup.InitFromData(m_dragonData, UserProfile.Currency.HARD);
		}

		// Start with buttons hidden
		if(m_purchaseButtonsAnim != null) {
			m_purchaseButtonsAnim.ForceHide(false);
		}

		if(m_shareButtonAnim != null) {
			m_shareButtonAnim.ForceHide(false);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The button to unlock the new dragon with SC has been pressed.
	/// </summary>
	public void OnUnlockWithSC() {
		// Let MenuDragonUnlockClassicDragon handle it
		MenuDragonUnlockClassicDragon.UnlockWithSC(m_dragonData, OnUnlockSuccess);
	}

	/// <summary>
	/// The button to unlock the new dragon with PC has been pressed.
	/// </summary>
	public void OnUnlockWithPC() {
		// Let MenuDragonUnlockClassicDragon handle it
		MenuDragonUnlockClassicDragon.UnlockWithPC(m_dragonData, OnUnlockSuccess);
	}

	/// <summary>
	/// The unlock resources flow has been successful.
	/// </summary>
	/// <param name="_flow">The flow that triggered the event.</param>
	private void OnUnlockSuccess(ResourcesFlow _flow) {
		// [AOC] If testing, undo dragon acquisition (feelin' dirty)
		if(CPResultsScreenTest.testEnabled) {
			m_dragonData.ResetLoadedData();
			PersistenceFacade.instance.Save_Request();
		}

		// Throw out some fireworks!
		m_controller.scene.LaunchConfettiFX(true);

		// Hide the button to prevent spamming
		if(m_purchaseButtonsAnim != null) {
			m_purchaseButtonsAnim.ForceHide();
		}

		// Show share button
		if(m_shareButtonAnim != null) {
			m_shareButtonAnim.ForceShow();
		}
	}

	/// <summary>
	/// The share button has been pressed.
	/// </summary>
	public void OnShareButton() {
		// Initialize and open share screen
		ShareScreenDragon shareScreen = ShareScreensManager.GetShareScreen("dragon_acquired") as ShareScreenDragon;
		shareScreen.Init(
			"dragon_acquired",
			SceneController.GetMainCameraForCurrentScene(),
			m_dragonData,
			true,
			null
		);
		shareScreen.TakePicture();
	}

	/// <summary>
	/// Dragon info button has been pressed.
	/// </summary>
	public void OnDragonInfoButton() {
		// Tracking
		string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupDragonInfo.PATH);
		HDTrackingManager.Instance.Notify_InfoPopup(popupName, "info_button");

		// Open the dragon info popup initialized with the unlocked dragon info
		PopupController popup = PopupManager.LoadPopup(PopupDragonInfo.PATH);
		PopupDragonInfo dragonInfoPopup = popup.GetComponent<PopupDragonInfo>();
		dragonInfoPopup.Init(m_dragonData);
		popup.Open();
	}
}