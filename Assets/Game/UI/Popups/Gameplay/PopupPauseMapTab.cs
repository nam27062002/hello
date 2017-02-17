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
	[SerializeField] private GameObject m_mapView = null;
	[SerializeField] private ShowHideAnimator m_lockGroupAnim = null;
	[SerializeField] private Animator m_lockIconAnim = null;

	[Separator("Info Area")]
	[SerializeField] private Localizer m_descriptionText = null;
	[SerializeField] private ShowHideAnimator m_upgradeAreaAnim = null;
	[SerializeField] private ShowHideAnimator m_infoAreaAnim = null;

	// Buttons
	[Separator("Price Tags")]
	[SerializeField] private ShowHideAnimator m_scButtonAnim = null;
	[SerializeField] private TextMeshProUGUI m_scPriceText = null;
	[Space]
	[SerializeField] private ShowHideAnimator m_pcButtonAnim = null;
	[SerializeField] private TextMeshProUGUI m_pcPriceText = null;

	// Internal
	private DefinitionNode m_def = null;

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
		Refresh();
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
	}

	/// <summary>
	/// Refresh the pill with info of the current map upgrade.
	/// </summary>
	public void Refresh() {
		// Skip if definition not valid
		if(m_def == null) return;

		// Description
		m_descriptionText.Localize(m_def.Get("tidDesc"));

		// Price tags
		int priceSC = m_def.GetAsInt("upgradePriceSC");
		m_scPriceText.text = UIConstants.IconString(priceSC, UIConstants.IconType.COINS, UIConstants.IconAlignment.LEFT);

		int pricePC = m_def.GetAsInt("upgradePriceHC");
		m_pcPriceText.text = UIConstants.IconString(pricePC, UIConstants.IconType.PC, UIConstants.IconAlignment.LEFT);

		// Buttons visibility
		m_scButtonAnim.Set(priceSC > 0 && pricePC <= 0);
		m_pcButtonAnim.Set(pricePC > 0);	// Regardless of SC price
		//m_noPriceAnim.Set(priceSC <= 0 && pricePC <= 0);
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
		// Get new map definition
		UpdateDefinition();

		// Trigger FX
		/*m_upgradeFX.Stop();
		m_upgradeFX.Clear();
		m_upgradeFX.Play();

		// Image animation (to hide sprite swap)
		m_mapImage.transform.DOKill(true);
		m_mapImage.transform.DOScale(0f, 0.15f).SetEase(Ease.InBack).SetLoops(2, LoopType.Yoyo).SetAutoKill(true);*/

		// Refresh info after some delay (to sync with animation)
		DOVirtual.DelayedCall(0.15f, Refresh);
	}
}