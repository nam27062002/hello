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
	[SerializeField] private ShowHideAnimator m_purchaseButtonsAnim = null;
	[SerializeField] private Localizer m_hcPriceText = null;
	[SerializeField] private Localizer m_scPriceText = null;

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
		if(m_scPriceText != null) {
			// Set text
			m_scPriceText.Localize(
				m_scPriceText.tid,
				StringUtils.FormatNumber(m_dragonData.def.GetAsLong("unlockPriceCoins"))
			);
		}

		// Update hc unlock button
		if(m_hcPriceText != null) {
			// Set text
			m_hcPriceText.Localize(
				m_hcPriceText.tid,
				StringUtils.FormatNumber(m_dragonData.def.GetAsLong("unlockPricePC"))
			);
		}

		// Start with buttons hidden
		if(m_purchaseButtonsAnim != null) {
			m_purchaseButtonsAnim.ForceHide(false);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The button to unlock the new dragon with SC has been pressed.
	/// </summary>
	public void OnUnlockWithSC() {
		// [AOC] TODO!! Try to reuse MenuDragonUnlockClassicDragon somehow

		// Make sure we can
		// [AOC] Unless testing!
		if(!CPResultsScreenTest.testEnabled) {
			if(!MenuDragonUnlockClassicDragon.CheckUnlockWithSC(m_dragonData)) return;
		}

		// [AOC] From 1.18 on, don't trigger the missing SC flow for dragon 
		//		 purchases (we are displaying the HC button next to it)
		// Check whether we have enough SC
		long priceSC = m_dragonData.def.GetAsLong("unlockPriceCoins");
		if(priceSC > UsersManager.currentUser.coins) {
			// Not enough SC! Show a message
			UIFeedbackText.CreateAndLaunch(
				LocalizationManager.SharedInstance.Localize("TID_SC_NOT_ENOUGH"),   // [AOC] TODO!! Improve text?
				GameConstants.Vector2.center,
				this.GetComponentInParent<Canvas>().transform as RectTransform,
				"NotEnoughSCError"
			);
		} else {
			// There shouldn't be any problem to perform the transaction, do
			// it via a ResourcesFlow to avoid duplicating code / missing steps
			ResourcesFlow purchaseFlow = new ResourcesFlow(MenuDragonUnlockClassicDragon.UNLOCK_WITH_SC_RESOURCES_FLOW_NAME);
			purchaseFlow.OnSuccess.AddListener(OnUnlockSuccess);
			purchaseFlow.Begin(
				priceSC,
				UserProfile.Currency.SOFT,
				HDTrackingManager.EEconomyGroup.UNLOCK_DRAGON,
				m_dragonData.def
			);
		}
	}

	/// <summary>
	/// The button to unlock the new dragon with PC has been pressed.
	/// </summary>
	public void OnUnlockWithPC() {
		// [AOC] TODO!! Try to reuse MenuDragonUnlockClassicDragon somehow

		// Make sure we can
		// [AOC] Unless testing!
		if(!CPResultsScreenTest.testEnabled) {
			if(!MenuDragonUnlockClassicDragon.CheckUnlockWithPC(m_dragonData)) return;
		}

		// Get price and start purchase flow
		ResourcesFlow purchaseFlow = new ResourcesFlow(MenuDragonUnlockClassicDragon.UNLOCK_WITH_HC_RESOURCES_FLOW_NAME);
		purchaseFlow.OnSuccess.AddListener(OnUnlockSuccess);
		purchaseFlow.Begin(
			m_dragonData.def.GetAsLong("unlockPricePC"),
			UserProfile.Currency.HARD,
			HDTrackingManager.EEconomyGroup.UNLOCK_DRAGON,
			m_dragonData.def
		);
	}

	/// <summary>
	/// The unlock resources flow has been successful.
	/// </summary>
	/// <param name="_flow">The flow that triggered the event.</param>
	private void OnUnlockSuccess(ResourcesFlow _flow) {
		// Just acquire target dragon!
		// [AOC] Unless testing!
		if(!CPResultsScreenTest.testEnabled) {
			m_dragonData.Acquire();

			HDTrackingManager.Instance.Notify_DragonUnlocked(m_dragonData.def.sku, m_dragonData.GetOrder());
		}

		// Save!
		PersistenceFacade.instance.Save_Request(true);

		// Throw out some fireworks!
		m_controller.scene.LaunchConfettiFX(true);

		// Hide the button to prevent spamming
		if(m_purchaseButtonsAnim != null) {
			m_purchaseButtonsAnim.ForceHide();
		}

		// Same with tap to continue
		m_tapToContinue.ForceHide();

		// Continue with the animation after some delay
		UbiBCN.CoroutineManager.DelayedCall(() => {
			m_sequence.Play();
		}, 1f);
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