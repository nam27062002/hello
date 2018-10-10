// LabDragonSelectionScreen.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/09/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Screen controller for the Lab Dragon Selection screen.
/// </summary>
public class LabDragonSelectionScreen : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Localizer m_dragonNameText = null;
	[SerializeField] private Localizer m_dragonDescText = null;
	[Space]
	[SerializeField] private Localizer m_unlockInfoText = null;
	[Space]
	[SerializeField] private Localizer m_upgradeLockedInfoText = null;
	[SerializeField] private LabStatUpgrader[] m_stats = new LabStatUpgrader[0];
	[Space]
	[SerializeField] private GameObject m_loadingUI = null;

	// Cache some data
	private DragonDataSpecial m_dragonData = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Initialize unlock info text
		if(m_unlockInfoText != null) {
			DefinitionNode unlockTierDef = DefinitionsManager.SharedInstance.GetDefinition(
				DefinitionsCategory.DRAGON_TIERS, IDragonData.TierToSku(DragonDataSpecial.MIN_TIER_TO_UNLOCK)
			);

			m_unlockInfoText.Localize(
				m_unlockInfoText.tid,
				UIConstants.GetSpriteTag(unlockTierDef.GetAsString("icon"))
			);
		}

		// Initialize 3D scene
        LabDragonSelectionScene scene3d = InstanceManager.menuSceneController.transitionManager.GetScreenData(MenuScreen.LAB_DRAGON_SELECTION).scene3d as LabDragonSelectionScene;
		if(scene3d != null) {
			// Link loading UI - will be controlled by the 3D scene
			scene3d.loadingUI = m_loadingUI;
		}
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);

		// Do a first refresh
		InitWithDragon(InstanceManager.menuSceneController.selectedDragonData, false);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Launch the acquire animation!
	/// </summary>
	/// <param name="_acquiredDragonSku">Acquired dragon sku.</param>
	public void LaunchAcquireAnim(string _acquiredDragonSku) {
		// Program animation
		DOTween.Sequence()
			.AppendCallback(() => {
				// Lock all input
				Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, true);

				// Throw out some fireworks!
				InstanceManager.menuSceneController.dragonScroller.LaunchDragonPurchasedFX();

				// Trigger SFX
				AudioController.Play("hd_unlock_dragon");
			})
			.AppendInterval(1f)     // Add some delay before unlocking input to avoid issues when spamming touch (fixes issue https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-765)
			.AppendCallback(() => {
				// Unlock input
				Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, false);
			})
			.SetAutoKill(true)
			.Play();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh with data from a target dragon.
	/// </summary>
	/// <param name="_dragonData">Data to be used to initialize the dragon info.</param>
	/// <param name="_animate">Trigger animations?</param>
	private void InitWithDragon(IDragonData _dragonData, bool _animate) {
		// Store new dragon data
		m_dragonData = _dragonData as DragonDataSpecial;

		// Skip if dragon data is not valid
		if(m_dragonData == null) return;

		// Dragon name
		if(m_dragonNameText != null) {
			m_dragonNameText.Localize(m_dragonData.def.GetAsString("tidName"));
		}

		// Dragon desc
		if(m_dragonDescText != null) {
			m_dragonDescText.Localize(m_dragonData.def.GetAsString("tidDesc"));
		}

		// Dragon stats
		for(int i = 0; i < m_stats.Length; ++i) {
			m_stats[i].InitFromData(m_dragonData);
		}

		// First refresh
		Refresh(_animate);
	}

	/// <summary>
	/// Soft refresh of things that can change while the same dragon is selected.
	/// </summary>
	/// <param name="_animate">Trigger animations?</param>
	private void Refresh(bool _animate) {
		// Skip if dragon data is not valid
		if(m_dragonData == null) return;

		// Dragon description
		if(m_dragonDescText != null) {
			// Show only if dragon is not owned
			m_dragonDescText.gameObject.SetActive(!m_dragonData.isOwned);
		}

		// Upgrade locked info
		if(m_upgradeLockedInfoText != null) {
			// Show if no more upgrades are possible
			// [AOC] Unless the cause is because all stats are already maxed!
			DragonTier nextRequiredTier = m_dragonData.GetNextRequiredTier();
			bool show = !m_dragonData.CanUpgradeStats() && !m_dragonData.allStatsMaxed && nextRequiredTier != DragonTier.COUNT;
			m_upgradeLockedInfoText.gameObject.SetActive(show);

			// Update text
			if(show) {
				m_upgradeLockedInfoText.Localize(	// Unlock a %U0 dragon to keep upgrading %U1!
					m_upgradeLockedInfoText.tid,
                    UIConstants.GetSpriteTag(UIConstants.GetDragonTierIcon(nextRequiredTier)),
                 	m_dragonData.def.GetLocalized("tidName")
				);
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Back button has been pressed.
	/// </summary>
    public void OnBackButton() {
		SceneController.SetMode(SceneController.Mode.DEFAULT);
    }

	/// <summary>
	/// A new dragon has been selected.
	/// </summary>
	/// <param name="_sku">The sku of the selected dragon.</param>
	private void OnDragonSelected(string _sku) {
		// Get new dragon's data from the dragon manager and do the refresh logic
		InitWithDragon(DragonManager.GetDragonData(_sku), true);
	}
}