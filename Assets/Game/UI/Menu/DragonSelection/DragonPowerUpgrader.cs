// LabStatUpgrader.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Standalone widget to control the logic of upgrading a Special Dragon's power.
/// </summary>
public class DragonPowerUpgrader : MonoBehaviour {

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed references
    [SerializeField] private Image m_icon = null;
    [SerializeField] private Localizer m_priceText = null;
    [SerializeField] private RectTransform m_feedbackAnchor = null;
    [SerializeField] private ShowHideAnimator m_showHide = null;

    // Internal data
    private DragonDataSpecial m_dragonData = null;


    // Internal references

    // Tooltip

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
	/// Initialization.
	/// </summary>
	private void Awake()
    {

        // Subscribe to external events
        Messenger.AddListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_LEVEL_UPGRADED, OnDragonLevelUpgraded);

        // Make sure we're displaying the right info
        // [AOC] Delay by one frame to do it when the object is actually enabled
        UbiBCN.CoroutineManager.DelayedCallByFrames(
            () => { Refresh(false); },
            1
        );
    }


    public void OnShowPreAnimation()
    {
        Refresh(false);
    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
	/// Initialize with the given dragon data pre-defined stat type.
	/// </summary>
	/// <param name="_dragonData">Dragon data to be used.</param>
	public void InitFromData(DragonDataSpecial _dragonData)
    {
        // Store data references
        // Check for invalid params
        if (_dragonData == null)
        {
            m_dragonData = null;
            this.gameObject.SetActive(false);

            // Nothing else to do if dragon is not valid
            return;
        }
        else
        {
            m_dragonData = _dragonData;
            this.gameObject.SetActive(true);
        }

        // Update visuals
        Refresh(false);
    }

    /// <summary>
	/// Update visuals with current data.
	/// </summary>
	/// <param name="_animate">Trigger animations?</param>
	public void Refresh(bool _animate)
    {

        // Nothing to do if either dragon or stat data are not valid
        if (m_dragonData == null) return;

        // Hide button if the next upgrade doesn´t unlock a new power
        if (m_dragonData.IsUnlockingNewPower())
        {
            m_showHide.Show(true);
        }
        else
        {
            m_showHide.Hide(true);
            return;
        }



        // Refresh upgrade price
        if (m_priceText != null)
        {

            // Find the price of the next power upgrade 
            Price upgradePrice = m_dragonData.GetNextPowerUpgradePrice();

            // Disable localizer and just show the price
            m_priceText.enabled = false;

            if (upgradePrice != null )
            {
                m_priceText.text.text = UIConstants.GetIconString(
                    upgradePrice.Amount.ToString(),
                    UIConstants.GetCurrencyIcon(upgradePrice.Currency),
                    UIConstants.IconAlignment.LEFT
                );
            }

        }

        // Get the next power definition
        DefinitionNode nextPower = m_dragonData.GetNextPowerUpgrade();

        // Refresh power icon
        if (m_icon != null)
        {
            m_icon.sprite = Resources.Load<Sprite>(UIConstants.POWER_ICONS_PATH + nextPower.Get("icon"));
        }

    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
	/// The upgrade button has been pressed!
	/// </summary>
	public void OnUpgradeButton()
    {
        // Nothing to do if either dragon or stat data are not valid
        if (m_dragonData == null ) return;

        // If next level is not unlocking a new power, something went wrong
        if (!m_dragonData.IsUnlockingNewPower()) return;


        Downloadables.Handle allContentHandle = HDAddressablesManager.Instance.GetHandleForAllDownloadables();

        if (!allContentHandle.IsAvailable())
        {
            // Get the download flow from the parent lab screen
            AssetsDownloadFlow assetsDownloadFlow = InstanceManager.menuSceneController.GetScreenData(MenuScreen.LAB_DRAGON_SELECTION).
                                                        ui.GetComponent<LabDragonSelectionScreen>().assetsDownloadFlow;

            assetsDownloadFlow.InitWithHandle(allContentHandle);
            PopupAssetsDownloadFlow popup = assetsDownloadFlow.OpenPopupByState(PopupAssetsDownloadFlow.PopupType.ANY,
                                                                                AssetsDownloadFlow.Context.PLAYER_CLICKS_ON_UPGRADE_SPECIAL);
            // Abort the upgrade
            return;
        }

        // Get the price of the current upgrade
        Price upgradePrice = m_dragonData.GetNextPowerUpgradePrice();

        // Launch transaction
        if (upgradePrice != null)
        {
            ResourcesFlow purchaseFlow = new ResourcesFlow("UPGRADE_SPECIAL_DRAGON_STAT");
            purchaseFlow.OnSuccess.AddListener(OnUpgradePurchaseSuccess);
            purchaseFlow.Begin(
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
    private void OnUpgradePurchaseSuccess(ResourcesFlow _flow)
    {
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

        // Do it
        // Visuals will get refreshed when receiving the SPECIAL_DRAGON_STAT_UPGRADED event
        m_dragonData.UpgradePower();
    }

    /// <summary>
    /// A special dragon level has been upgraded.
    /// </summary>
    /// <param name="_dragonData">Target dragon data.</param>
    /// <param name="_stat">Target stat.</param>
    private void OnDragonLevelUpgraded(DragonDataSpecial _dragonData)
    {
        // Refresh visuals regardles of the stat (we might get locked)
        Refresh(true);
    }

}