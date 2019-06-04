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
	[SerializeField] private AssetsDownloadFlow m_assetsDownloadFlow = null;
	public AssetsDownloadFlow assetsDownloadFlow {
		get { return m_assetsDownloadFlow; }
	}
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
    private const string LAB_MUSIC = "hd_lab_music";
    private int m_labMusicCount = 0;
	
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

				// Check for assets download
                Downloadables.Handle acquiredDragonHandle = HDAddressablesManager.Instance.GetHandleForSpecialDragon(_acquiredDragonSku);
                if (!acquiredDragonHandle.IsAvailable())
                {
                    // Initialize download flow with handle for ALL and check for popups
                    m_assetsDownloadFlow.InitWithHandle(HDAddressablesManager.Instance.GetHandleForAllDownloadables());
                    m_assetsDownloadFlow.OpenPopupIfNeeded(AssetsDownloadFlow.Context.PLAYER_BUYS_SPECIAL_DRAGON);
                }

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
        // if ( InstanceManager.musicController.Ambience_ToPlay.music_key != LAB_MUSIC )
        if (m_labMusicCount <= 0)
        {
            m_labMusicCount++;
            InstanceManager.musicController.Ambience_Play(LAB_MUSIC, gameObject);
        }

		// Trigger intro popup?
		if(!Prefs.GetBoolPlayer(PopupLabIntro.DISPLAYED_KEY)) {
			PopupManager.EnqueuePopup(PopupLabIntro.PATH);
		}

		// Trigger lab unlocked popup?
		if(PopupLabUnlocked.Check()) {
			PopupLabUnlocked labPopup = PopupManager.EnqueuePopup(PopupLabUnlocked.PATH).GetComponent<PopupLabUnlocked>();
			labPopup.Init(MenuScreen.LAB_DRAGON_SELECTION);
		}

		// If we have a dragon selection pending, do it now!
		if (!string.IsNullOrEmpty(m_pendingToSelectDragon)) {
            InstanceManager.menuSceneController.SetSelectedDragon(m_pendingToSelectDragon);
            m_pendingToSelectDragon = string.Empty;
        }

		// Do a first refresh
		InitWithDragon(InstanceManager.menuSceneController.selectedDragonData, false);

		// OTA: Show the general download progress bar
		Downloadables.Handle allContentHandle = HDAddressablesManager.Instance.GetHandleForAllDownloadables();
        m_assetsDownloadFlow.InitWithHandle(allContentHandle);

	} 

	/// <summary>
	/// The show animation has finished.
	/// </summary>
    public void OnShowPostAnimation() {
		// Subscribe to external events
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.AddListener<DragonDataSpecial, DragonDataSpecial.Stat>(MessengerEvents.SPECIAL_DRAGON_STAT_UPGRADED, OnStatUpgraded);

		// If the season has finished, go to the league screen
		MenuScreen prevScreen = InstanceManager.menuSceneController.transitionManager.prevScreen;
		if (prevScreen != MenuScreen.LAB_LEAGUES && prevScreen != MenuScreen.LAB_MISSIONS) {
			if (HDLiveDataManager.league.season.state == HDSeasonData.State.PENDING_REWARDS) {
				// Clear popups and go to leagues screen
				PopupManager.Clear(true);
                InstanceManager.menuSceneController.GoToScreen(MenuScreen.LAB_LEAGUES, true);
            }
        }
    }

	/// <summary>
	/// The screen is about to hide.
	/// </summary>
	public void OnHidePreAnimation() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.RemoveListener<DragonDataSpecial, DragonDataSpecial.Stat>(MessengerEvents.SPECIAL_DRAGON_STAT_UPGRADED, OnStatUpgraded);
	}

	/// <summary>
	/// Back button has been pressed.
	/// </summary>
	public void OnBackButton() {
		// Make sure we are allowed to change screen (prevent spamming)
		// [AOC] Resolves issue HDK-4255 among others
		if(!InstanceManager.menuSceneController.transitionManager.transitionAllowed) return;
        
		// Stop lab music
        m_labMusicCount--;
        InstanceManager.musicController.Ambience_Stop(LAB_MUSIC, gameObject);
        
		// Go back to default mode
		SceneController.SetMode(SceneController.Mode.DEFAULT);
        HDLiveDataManager.instance.SwitchToQuest();
        InstanceManager.menuSceneController.GoToScreen(MenuScreen.DRAGON_SELECTION);
    }

	/// <summary>
	/// The play button has been pressed.
	/// </summary>
	public void OnPlayButton() {
		// Prevent spamming
		if(!InstanceManager.menuSceneController.transitionManager.transitionAllowed) return;

        // Get assets download handle for current dragon
        string currentDragonSku = UsersManager.currentUser.currentSpecialDragon;
        Downloadables.Handle currentDragonHandle = HDAddressablesManager.Instance.GetHandleForSpecialDragon(currentDragonSku);
        if (!currentDragonHandle.IsAvailable())
        {
            // Resources not available, which means we need to download ALL
            m_assetsDownloadFlow.InitWithHandle(HDAddressablesManager.Instance.GetHandleForAllDownloadables());

            // If needed, show assets download popup
            m_assetsDownloadFlow.OpenPopupByState(PopupAssetsDownloadFlow.PopupType.ANY);

            // Don't move to next screen
            return;
        }


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
		// Make sure we are the active screen
		if(InstanceManager.menuSceneController.currentScreen != MenuScreen.LAB_DRAGON_SELECTION) return;

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