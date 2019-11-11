// ISpecialDragonUpgrader.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/11/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Standalone widget to control the logic of upgrading a Special Dragon's Stat.
/// </summary>
public abstract class ISpecialDragonUpgrader : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] protected TextMeshProUGUI m_priceText = null;
	[SerializeField] protected RectTransform m_feedbackAnchor = null;
    [SerializeField] protected ShowHideAnimator m_showHide = null;
	public ShowHideAnimator showHide {
		get { return m_showHide; }
	}

	// Internal data
	protected DragonDataSpecial m_dragonData = null;

	// Prevent spamming upgrade buttons
	protected static ResourcesFlow s_activeResourcesFlow = null;
	protected float m_antiSpamTimer = 0f;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// Subscribe to external events
        Messenger.AddListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_LEVEL_UPGRADED, OnDragonLevelUpgraded);


    }

	/// <summary>
	/// Update loop.
	/// </summary>
	protected virtual void Update() {
		// Update anti-spam timer
		if(m_antiSpamTimer > 0f) {
			m_antiSpamTimer -= Time.unscaledDeltaTime;
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected virtual void OnDestroy() {
        // Subscribe to external events
        Messenger.RemoveListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_LEVEL_UPGRADED, OnDragonLevelUpgraded);
    }

    //------------------------------------------------------------------------//
    // ABSTRACT AND VIRTUAL METHODS											  //
    //------------------------------------------------------------------------//
    /// <summary>
	/// Initialize with the given dragon data.
	/// </summary>
	/// <param name="_dragonData">Dragon data to be used.</param>
	public virtual void InitFromData(DragonDataSpecial _dragonData) {
		// Store data references
		m_dragonData = _dragonData;
		this.gameObject.SetActive(m_dragonData != null);
	}

	/// <summary>
	/// Update visuals with current data.
	/// </summary>
	/// <param name="_animate">Trigger animations?</param>
	public abstract void Refresh(bool _animate);

	/// <summary>
	/// Get the price for this upgrade.
	/// </summary>
	/// <returns>The price of this upgrade.</returns>
	public abstract Price GetPrice();

	/// <summary>
	/// Check the non-generic conditions needed for this upgrader to upgrade.
	/// </summary>
	/// <returns>Whether the upgrader can upgrade or not.</returns>
	public abstract bool CanUpgrade();

	/// <summary>
	/// Refresh the price tag of the upgrade.
	/// </summary>
	protected virtual void RefreshPrice() {
		// Checks
		if(m_priceText == null) return;

		// Get price
		Price upgradePrice = GetPrice();
		if(upgradePrice == null) return;

		// Disable localizer and just show the price
		m_priceText.text = UIConstants.GetIconString(
			StringUtils.FormatNumber(System.Convert.ToInt64(upgradePrice.Amount)),
			UIConstants.GetCurrencyIcon(upgradePrice.Currency),
			UIConstants.IconAlignment.LEFT
		);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The upgrader is about to show.
	/// </summary>
	public void OnShowPreAnimation() {
		// In case this is enabled by an external component, make sure the visibility is right
		Refresh(false);
	}

	/// <summary>
	/// The upgrade button has been pressed!
	/// </summary>
	public virtual void OnUpgradeButton() {
		// Nothing to do if there is a transaction already in process
		// Resolves issue HDK-6454 (spamming button)
		if(s_activeResourcesFlow != null) return;

		// Prevent spamming
		if(m_antiSpamTimer > 0f) return;

		// Nothing to do if either dragon data is not valid
		if(m_dragonData == null) return;

		// Custom checks
		if(!CanUpgrade()) return;


        // OTA: we prevent the upgrade if the asset bundles are not downloaded
        // because some of the upgraded dragons need the downloadable content
        Downloadables.Handle allContentHandle = HDAddressablesManager.Instance.GetHandleForAllDownloadables();
        if (!allContentHandle.IsAvailable())
        {
            // Get the download flow from the parent lab screen
            AssetsDownloadFlow assetsDownloadFlow = InstanceManager.menuSceneController.GetScreenData(MenuScreen.DRAGON_SELECTION).
                                                        ui.GetComponent<MenuDragonScreenController>().assetsDownloadFlow;

            assetsDownloadFlow.InitWithHandle(allContentHandle);
            PopupAssetsDownloadFlow popup = assetsDownloadFlow.OpenPopupByState(PopupAssetsDownloadFlow.PopupType.ANY,
                                                                                AssetsDownloadFlow.Context.PLAYER_CLICKS_ON_UPGRADE_SPECIAL);
            // Abort the upgrade
            return;
        }

		// Get the price of the current upgrade
		Price upgradePrice = GetPrice();

        // Launch transaction
        if (upgradePrice != null)
        {
            s_activeResourcesFlow = new ResourcesFlow("UPGRADE_SPECIAL_DRAGON_STAT");
			s_activeResourcesFlow.OnSuccess.AddListener(OnUpgradePurchaseSuccess);
			s_activeResourcesFlow.OnFinished.AddListener(OnUpgradePurchaseFinished);
			s_activeResourcesFlow.Begin(
                System.Convert.ToInt64(upgradePrice.Amount),
                upgradePrice.Currency,
                HDTrackingManager.EEconomyGroup.UNKNOWN, //HDTrackingManager.EEconomyGroup.SPECIAL_DRAGON_UPGRADE,
                null
            );
        }
	}

	/// <summary>
	/// The upgrade purchase has been successful.
	/// </summary>
	/// <param name="_flow">The Resources Flow that triggered the event.</param>
	protected virtual void OnUpgradePurchaseSuccess(ResourcesFlow _flow) {
		// Tracking
		// [AOC] TODO!!
		//HDTrackingManager.Instance.Notify_DragonUnlocked(dragonData.def.sku, dragonData.GetOrder());

		// Show a nice feedback animation
		// "Level up!"
		UIFeedbackText feedbackText = UIFeedbackText.CreateAndLaunch(
			LocalizationManager.SharedInstance.Localize("TID_FEEDBACK_LEVEL_UP"),
			m_feedbackAnchor,
			GameConstants.Vector2.zero,
			m_feedbackAnchor
		);

		Canvas c = feedbackText.GetComponentInParent<Canvas>();
		feedbackText.transform.SetParent(c.transform);
		feedbackText.transform.SetAsLastSibling();

		// Trigger SFX
		AudioController.Play("hd_reward_golden_fragments");
	}

	/// <summary>
	/// A resources flow has finished.
	/// </summary>
	/// <param name="_flow">The flow that triggered the event.</param>
	private void OnUpgradePurchaseFinished(ResourcesFlow _flow) {
		// Should be the same flow that was triggered, but check just in case
		if(_flow != s_activeResourcesFlow) return;

		// Clear flow reference to allow for another flow to be started
		s_activeResourcesFlow = null;

		// Start anti-spam timer
		m_antiSpamTimer = 1f;
	}

	/// <summary>
	/// A special dragon level has been upgraded.
	/// </summary>
	/// <param name="_dragonData">Target dragon data.</param>
	protected virtual void OnDragonLevelUpgraded(DragonDataSpecial _dragonData) {
		// Refresh visuals regardles of the stat (we might get locked)
		Refresh(false);
	}
}