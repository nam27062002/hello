// PopupPauseMapTab.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Map tab of the in-game pause popup.
/// </summary>
public class PopupPauseMapTab : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Separator("Map Area")]
	[SerializeField] private ShowHideAnimator m_lockGroupAnim = null;
	[SerializeField] private Animator m_lockIconAnim = null;

	[Separator("Info Area")]
	[SerializeField] private Localizer m_upgradeInfoText = null;
	[SerializeField] private ShowHideAnimator m_upgradeAreaAnim = null;
	[SerializeField] private ShowHideAnimator m_infoAreaAnim = null;

	// Buttons
	[Separator("Price Tags")]
	[SerializeField] private ShowHideAnimator m_scButtonAnim = null;
	[SerializeField] private TextMeshProUGUI m_scPriceText = null;
	[Space]
	[SerializeField] private ShowHideAnimator m_pcButtonAnim = null;
	[SerializeField] private TextMeshProUGUI m_pcPriceText = null;

	// FX
	[Separator("FX")]
	[SerializeField] private ParticleSystem m_upgradeFX = null;

	// Internal
	private DefinitionNode m_def = null;

	// Cache some data
	private int m_pricePC = 0;
	private int m_priceSC = 0;
	private bool m_isMaxed = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Initialize with current map definition
		UpdateDefinition();

		// Subscribe to external events
		Messenger.AddListener<int>(GameEvents.PROFILE_MAP_UPGRADED, OnMapUpgraded);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		RefreshLockIcon();
		RefreshInfoSection();
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<int>(GameEvents.PROFILE_MAP_UPGRADED, OnMapUpgraded);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Gets the map upgrade definition corresponding to the user's current upgrade level.
	/// </summary>
	private void UpdateDefinition() {
		// Get current upgrade definition and store it
		m_def = DefinitionsManager.SharedInstance.GetDefinition("map_upgrade_" + UsersManager.currentUser.mapLevel);
		Debug.Assert(m_def != null, "Map Upgrade definition for level " + UsersManager.currentUser.mapLevel + " wasn't found!", this);

		// Cache some data
		m_priceSC = m_def.GetAsInt("upgradePriceSC");
		m_pricePC = m_def.GetAsInt("upgradePriceHC");
		m_isMaxed = (m_priceSC <= 0 && m_pricePC <= 0);	// If no upgrades are available for purchase, map is maxed out
	}

	/// <summary>
	/// Refresh lock icon.
	/// </summary>
	private void RefreshLockIcon() {
		// Skip if definition not valid
		if(m_def == null) return;

		// Lock icon
		m_lockGroupAnim.Set(UsersManager.currentUser.mapLevel <= 0);
	}

	/// <summary>
	/// Refresh info section.
	/// </summary>
	private void RefreshInfoSection() {
		m_upgradeInfoText.Localize(m_def.Get("tidDesc"));
		m_upgradeAreaAnim.Set(!m_isMaxed);
		m_infoAreaAnim.Set(m_isMaxed);
		if(!m_isMaxed) {
			// Price tags
			m_scPriceText.text = UIConstants.GetIconString(m_priceSC, UIConstants.IconType.COINS, UIConstants.IconAlignment.LEFT);
			m_pcPriceText.text = UIConstants.GetIconString(m_pricePC, UIConstants.IconType.PC, UIConstants.IconAlignment.LEFT);

			// Buttons visibility
			m_scButtonAnim.Set(m_priceSC > 0 && m_pricePC <= 0);
			m_pcButtonAnim.Set(m_pricePC > 0);	// Regardless of SC price
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The upgrade with SC button has been pressed.
	/// </summary>
	public void OnUpgradeWithSC() {
		// Ignore if definition not valid
		if(m_def == null) return;

		// Validate it's actually a SC upgrade
		int costSC = m_def.GetAsInt("upgradePriceSC");
		if(costSC <= 0) return;

		// Make sure we have enough PC to remove the mission
		if(UsersManager.currentUser.coins >= costSC) {
			// Do it!
			UsersManager.currentUser.AddCoins(-costSC);
			UsersManager.currentUser.UpgradeMap();
			PersistenceManager.Save();
		} else {
			// Open shop popup
			//PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);

			// Currency popup / Resources flow disabled for now
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_SC_NOT_ENOUGH"), new Vector2(0.5f, 0.33f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}

	/// <summary>
	/// The upgrade with PC button has been pressed.
	/// </summary>
	public void OnUpgradeWithPC() {
		// Ignore if definition not valid
		if(m_def == null) return;

		// Validate it's actually a PC upgrade
		int costPC = m_def.GetAsInt("upgradePriceHC");
		if(costPC <= 0) return;

		// Make sure we have enough PC to remove the mission
		if(UsersManager.currentUser.pc >= costPC) {
			// Do it!
			UsersManager.currentUser.AddPC(-costPC);
			UsersManager.currentUser.UpgradeMap();
			PersistenceManager.Save();
		} else {
			// Open shop popup
			//PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);

			// Currency popup / Resources flow disabled for now
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PC_NOT_ENOUGH"), new Vector2(0.5f, 0.33f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}

	/// <summary>
	/// The map has been upgraded.
	/// </summary>
	/// <param name="_newLevel">New map level.</param>
	public void OnMapUpgraded(int _newLevel) {
		// Aux vars
		float lockRefreshDelay = 0.15f;

		// Get new map definition
		UpdateDefinition();

		// If it's the first upgrade, show nice unlock animation
		if(_newLevel == 1) {
			// Launch unlock anim
			m_lockIconAnim.SetTrigger("unlock");
			lockRefreshDelay = 1.3f;	// Longer delay
		}

		// Trigger FX
		m_upgradeFX.Stop();
		m_upgradeFX.Clear();
		m_upgradeFX.Play();

		// Refresh info fast
		DOVirtual.DelayedCall(0.15f, RefreshInfoSection);

		// Refresh lock icon after some delay (to sync with animation)
		DOVirtual.DelayedCall(lockRefreshDelay, RefreshLockIcon);
	}

	/// <summary>
	/// Lock icon has been tapped.
	/// </summary>
	public void OnLockTap() {
		// Play simpa animation
		m_lockIconAnim.SetTrigger("bounce");
	}
}