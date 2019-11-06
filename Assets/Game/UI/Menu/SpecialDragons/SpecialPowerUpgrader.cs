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

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Standalone widget to control the logic of upgrading a Special Dragon's power.
/// </summary>
public class SpecialPowerUpgrader : ISpecialDragonUpgrader {

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed references
    [SerializeField] private Image m_icon = null;
    
    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
	/// Initialization.
	/// </summary>
	protected override void Awake()
    {
		// Call parent
		base.Awake();

        // Make sure we're displaying the right info
        // [AOC] Delay by one frame to do it when the object is actually enabled
        UbiBCN.CoroutineManager.DelayedCallByFrames(
            () => { Refresh(false); },
            1
        );
    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
	/// Update visuals with current data.
	/// </summary>
	/// <param name="_animate">Trigger animations?</param>
	public override void Refresh(bool _animate)
    {
        // Nothing to do if either dragon or stat data are not valid
        if (m_dragonData == null) return;

        // Hide button if the next upgrade doesn´t unlock a new power
        if (m_dragonData.IsUnlockingNewPower())
        {
			if(_animate) {
				m_showHide.RestartShow();
			} else {
				m_showHide.Show(false);
			}
        }
        else
        {
            m_showHide.Hide(false);
            return;
        }

		// Refresh upgrade price
		RefreshPrice();

        // Get the next power definition
        DefinitionNode nextPower = m_dragonData.GetNextPowerUpgrade();

        // Refresh power icon
        if (m_icon != null)
        {
            m_icon.sprite = Resources.Load<Sprite>(UIConstants.POWER_ICONS_PATH + nextPower.Get("icon"));
        }
    }

	/// <summary>
	/// Get the price for this upgrade.
	/// </summary>
	/// <returns>The price of this upgrade.</returns>
	public override Price GetPrice() {
		return m_dragonData.GetNextPowerUpgradePrice();
	}

	/// <summary>
	/// Check the non-generic conditions needed for this upgrader to upgrade.
	/// </summary>
	/// <returns>Whether the upgrader can upgrade or not.</returns>
	public override bool CanUpgrade() {
		// If next level is not unlocking a new power, something went wrong
		if(!m_dragonData.IsUnlockingNewPower()) return false;

		// All checks passed!
		return true;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
    /// The upgrade purchase has been successful.
    /// </summary>
    /// <param name="_flow">The Resources Flow that triggered the event.</param>
    protected override void OnUpgradePurchaseSuccess(ResourcesFlow _flow) {
		// Let parent do its job
		base.OnUpgradePurchaseSuccess(_flow);

		// Do it
		// Visuals will get refreshed when receiving the SPECIAL_DRAGON_STAT_UPGRADED event
		m_dragonData.UpgradePower();
    }
}