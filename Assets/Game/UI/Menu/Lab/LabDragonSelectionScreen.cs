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
	[Separator("References")]
	[SerializeField] private Localizer m_dragonNameText = null;
	[SerializeField] private Localizer m_dragonDescText = null;
	[Space]
	[SerializeField] private Localizer m_unlockInfoText = null;
	[Space]
    [SerializeField] private LabDragonBar m_dragonExpBar = null;
    [SerializeField] private Localizer m_upgradeLockedInfoText = null;
	[SerializeField] private LabStatUpgrader[] m_stats = new LabStatUpgrader[0];
	[Space]
	[SerializeField] private GameObject m_loadingUI = null;
	[Separator("Config")]
	[Tooltip("Use it to sync with animation")]
	[SerializeField] private float m_dragonChangeInfoDelay = 0.15f;

    // Use it to automatically select a specific dragon upon entering this screen
    // If the screen is already the active one, the selection will be applied the next time the screen is entered from a different screen
    private string m_pendingToSelectDragon = string.Empty;
    public string pendingToSelectDragon {
        set { m_pendingToSelectDragon = value; }
    }

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
		Messenger.AddListener<DragonDataSpecial, DragonDataSpecial.Stat>(MessengerEvents.SPECIAL_DRAGON_STAT_UPGRADED, OnStatUpgraded);

		// Do a first refresh
		InitWithDragon(InstanceManager.menuSceneController.selectedDragonData, false);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.RemoveListener<DragonDataSpecial, DragonDataSpecial.Stat>(MessengerEvents.SPECIAL_DRAGON_STAT_UPGRADED, OnStatUpgraded);
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
				LabDragonSelectionScene scene = InstanceManager.menuSceneController.transitionManager.GetScreenData(MenuScreen.LAB_DRAGON_SELECTION).scene3d.GetComponent<LabDragonSelectionScene>();
				scene.LaunchDragonPurchasedFX();
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

        // Dragon exp and stats
        m_dragonExpBar.BuildFromDragonData(m_dragonData);
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
	/// The screen is about to be displayed.
	/// </summary>
	public void OnShowPreAnimation() {
        InstanceManager.musicController.Ambience_Play("hd_lab_music", gameObject);
    
		// Trigger intro popup?
		if(!Prefs.GetBoolPlayer(PopupLabIntro.DISPLAYED_KEY)) {
			PopupManager.OpenPopupAsync(PopupLabIntro.PATH);
		}

        // If we have a dragon selection pending, do it now!
        if (!string.IsNullOrEmpty(m_pendingToSelectDragon)) {
            InstanceManager.menuSceneController.SetSelectedDragon(m_pendingToSelectDragon);
            m_pendingToSelectDragon = string.Empty;
        }
    } 

	/// <summary>
	/// The show animation has finished.
	/// </summary>
    public void OnShowPostAnimation() {
        HDSeasonData m_season = HDLiveDataManager.league.season;
        if (m_season.state == HDSeasonData.State.PENDING_REWARDS) {
            InstanceManager.menuSceneController.GoToScreen(MenuScreen.LAB_LEAGUES, true);
        }
    }

    /// <summary>
    /// Back button has been pressed.
    /// </summary>
    public void OnBackButton() {
        // Stop lab music
        InstanceManager.musicController.Ambience_Stop("hd_lab_music", gameObject);
        
		// Go back to default mode
		SceneController.SetMode(SceneController.Mode.DEFAULT);
        HDLiveDataManager.instance.SwitchToQuest();
    }

	/// <summary>
	/// The play button has been pressed.
	/// </summary>
	public void OnPlayButton() {
		// Go to the special missions screen
		// If the leagues tutorial has not yet been triggered, go to the leagues screen instead
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.LEAGUES_INFO)) {
			InstanceManager.menuSceneController.GoToScreen(MenuScreen.LAB_LEAGUES);
		} else {
			InstanceManager.menuSceneController.GoToScreen(MenuScreen.LAB_MISSIONS);
		}
	}

	/// <summary>
	/// A new dragon has been selected.
	/// </summary>
	/// <param name="_sku">The sku of the selected dragon.</param>
	private void OnDragonSelected(string _sku) {
		// [AOC] Add some delay to sync with UI animation
		UbiBCN.CoroutineManager.DelayedCall(
			() => {
				// Get new dragon's data from the dragon manager and do the refresh logic
				InitWithDragon(DragonManager.GetDragonData(_sku), true);
			}, m_dragonChangeInfoDelay
		);
	}

	/// <summary>
	/// A dragon stat has been upgraded.
	/// </summary>
	private void OnStatUpgraded(DragonDataSpecial _dragonData, DragonDataSpecial.Stat _stat) {
		// Let's just refresh for now
		Refresh(true);
        m_dragonExpBar.AddLevel();
	}

	/// <summary>
	/// Info button has been pressed.
	/// </summary>
	public void OnInfoButton() {
		// Skip if dragon data is not valid
		if(m_dragonData == null) return;

		// Tracking
		string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupSpecialDragonInfo.PATH);
		HDTrackingManager.Instance.Notify_InfoPopup(popupName, "info_button");

		// Open the dragon info popup and initialize it with the current dragon's data
		PopupSpecialDragonInfo popup = PopupManager.OpenPopupInstant(PopupSpecialDragonInfo.PATH).GetComponent<PopupSpecialDragonInfo>();
		popup.Init(m_dragonData);
	}
}