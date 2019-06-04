// MenuDragonSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

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
/// Main controller of the dragon selection screen.
/// </summary>
public class MenuDragonScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private MenuDragonLockIcon m_lockIcon = null;
	[Space]
	[SerializeField] private float m_initialDelay = 1f;
	[SerializeField] private float m_scrollDuration = 1f;
	[SerializeField] private float m_unlockAnimDuration = 1f;
	[SerializeField] private float m_unlockAnimFinalPauseDuration = 1f;
	[Space]
	[SerializeField] private NavigationShowHideAnimator[] m_toHideOnUnlockAnim = null;
    [SerializeField] private NavigationShowHideAnimator[] m_toHideOnTeaseAnim = null;
	[Space]
	[SerializeField] private AssetsDownloadFlow m_assetsDownloadFlow = null;
	public AssetsDownloadFlow assetsDownloadFlow {
		get { return m_assetsDownloadFlow; }
	}

	// Public properties
	private bool m_isAnimating = false;
	private bool isAnimating {
		get { return m_isAnimating; }
	}

	// Use it to automatically select a specific dragon upon entering this screen
	// If the screen is already the active one, the selection will be applied the next time the screen is entered from a different screen
	private string m_pendingToSelectDragon = string.Empty;
	public string pendingToSelectDragon {
		set { m_pendingToSelectDragon = value; }
	}

	// Internal vars
	private MenuScreen m_goToScreen = MenuScreen.NONE;
	private IDragonData m_dragonToTease = null;
	private IDragonData m_dragonToReveal = null;

    private bool m_showPendingTransactions = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	private void Awake() {
		// Subscribe to external events.
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnTransitionStarted);
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnTransitionEnd);
	}

	void Start(){
		if ( HDLiveDataManager.instance.ShouldRequestMyLiveData() )
		{
			HDLiveDataManager.instance.RequestMyLiveData();
		}
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
        m_showPendingTransactions = false;

        // Check whether we need to move to another screen
        // Check order is relevant!
        m_goToScreen = MenuScreen.NONE;

		// Check pending rewards
		if(UsersManager.currentUser.rewardStack.Count > 0 && false) {
			m_goToScreen = MenuScreen.PENDING_REWARD;
			return;
		}
	}

	/// <summary>
	/// Raises the disable event.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe to external events.
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnTransitionStarted);
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnTransitionEnd);
    }

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Do we have a screen change pending?
        if(m_goToScreen != MenuScreen.NONE && InstanceManager.menuSceneController.transitionManager.transitionAllowed) {
			// Which screen?
			switch(m_goToScreen) {
				case MenuScreen.EVENT_REWARD: {
					EventRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.EVENT_REWARD).ui.GetComponent<EventRewardScreen>();
					scr.StartFlow();
				} break;

				case MenuScreen.PENDING_REWARD: {
					PendingRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.PENDING_REWARD).ui.GetComponent<PendingRewardScreen>();
					scr.StartFlow(true);
				} break;
			}

			// Clear open and queued popups and go to target screen!
			PopupManager.Clear(true);
			InstanceManager.menuSceneController.GoToScreen(m_goToScreen);

			// Clear var
			m_goToScreen = MenuScreen.NONE;
			return;
		}

		// Cheat for simulating dragon unlock
		#if UNITY_EDITOR
		if(Input.GetKeyDown(KeyCode.U)) {
			int order = DragonManager.currentDragon.def.GetAsInt("order");
			List<IDragonData> dragonsByOrder = DragonManager.GetDragonsByOrder(IDragonData.Type.CLASSIC);
			if(order < dragonsByOrder.Count - 1) {	// Exclude if playing with last dragon
				IDragonData nextDragonData = dragonsByOrder[order + 1];
				if(nextDragonData != null) {
					InstanceManager.menuSceneController.dragonSelector.SetSelectedDragon(DragonManager.currentDragon.def.sku);
					DOVirtual.DelayedCall(1f, () => { LaunchUnlockAnim(nextDragonData.def.sku, m_initialDelay, m_scrollDuration, true); });
				}
			}
		}
		#endif

		//-----
		if(!m_isAnimating) { 	// Not while animating!
			if (!InstanceManager.menuSceneController.dragonScroller.cameraAnimator.isTweening) {
				if (m_dragonToReveal != null) {
					LaunchRevealAnim(m_dragonToReveal.def.sku);
				} else if (m_dragonToTease != null) {
					LaunchTeaseAnim(m_dragonToTease.def.sku);
				}

				// Launch only one animation at a time (this will do it)
				m_dragonToReveal = null;
				m_dragonToTease = null;
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Launch the unlock animation! Dragon acquired via HC
	/// </summary>
	/// <param name="_unlockedDragonSku">Unlocked dragon sku.</param>
	/// <param name="_initialDelay">Initial delay before launching the unlock animation.</param>
	/// <param name="_scrollDuration">Use it to sync with scrolling to target dragon.</param>
	/// <param name="_gotoDragonUnlockScreen">Whether to go the DragonUnlockScreen after the animation or not.</param>
	public void LaunchUnlockAnim(string _unlockedDragonSku, float _initialDelay, float _scrollDuration, bool _gotoDragonUnlockScreen) {
		// Program lock animation sequence
		DOTween.Sequence()
			.AppendCallback(() => {
				// Toggle animating mode
				SetAnimationFlag(true, true);

				// Disable normal behaviour
				m_lockIcon.GetComponent<MenuShowConditionally>().enabled = false;
				if(_unlockedDragonSku == InstanceManager.menuSceneController.selectedDragon) {
					// Target dragon is already selected, make sure lock icon is visible!
					m_lockIcon.GetComponent<ShowHideAnimator>().ForceShow();
				}
			})
			.AppendInterval(Mathf.Max(0.1f, _initialDelay))	// Avoid 0 duration
			.AppendCallback(() => {
				// Navigate to target dragon (should be next dragon)
				InstanceManager.menuSceneController.dragonSelector.SetSelectedDragon(_unlockedDragonSku);
			})
			.AppendInterval(Mathf.Max(0.1f, _scrollDuration))	// Sync with dragon scroll duration. Avoid 0 duration, otherwise lock animator gets broken
			.AppendCallback(() => {
				// Clean screen
				// Don't disable elements, otherwise they won't be enabled on the next screen change!
				for(int i = 0; i < m_toHideOnUnlockAnim.Length; i++) {
					m_toHideOnUnlockAnim[i].ForceHide(true, false);

					// If the element has a ShowConditionally component, disable to override its behaviour
					MenuShowConditionally[] showConditionally = m_toHideOnUnlockAnim[i].GetComponentsInChildren<MenuShowConditionally>();
					for(int j = 0; j < showConditionally.Length; ++j) {
						showConditionally[j].enabled = false;
					}
				}
				InstanceManager.menuSceneController.hud.animator.ForceHide(true, false);

				// Show icon unlock animation
				m_lockIcon.GetComponent<ShowHideAnimator>().ForceShow();
				m_lockIcon.view.LaunchUnlockAnim();

				// Trigger SFX
				AudioController.Play("hd_unlock_dragon");
			})
			.AppendInterval(m_unlockAnimDuration)
			.AppendCallback(() => {
				// Restore lock icon to the idle state (otherwise default values will get corrupted when deactivating the object)
				m_lockIcon.GetComponent<MenuShowConditionally>().enabled = true;
				m_lockIcon.GetComponent<ShowHideAnimator>().ForceHide(true, false);
			})
			.AppendInterval(m_unlockAnimFinalPauseDuration)
			.AppendCallback(() => {
				// Put lock icon back to its original position
				m_lockIcon.view.StopAllAnims();

				// Restore all hidden items
				for(int i = 0; i < m_toHideOnUnlockAnim.Length; i++) {
					// Leave them hidden if changing screens!
					if(!_gotoDragonUnlockScreen) {
						m_toHideOnUnlockAnim[i].Show(true);
					}

					// Re-enable all disabled ShowConditionally components
					MenuShowConditionally[] showConditionally = m_toHideOnUnlockAnim[i].GetComponentsInChildren<MenuShowConditionally>();
					for(int j = 0; j < showConditionally.Length; ++j) {
						showConditionally[j].enabled = true;
						showConditionally[j].OnDragonSelected(_unlockedDragonSku);
					}
				}

				// Restore HUD
				InstanceManager.menuSceneController.hud.animator.ForceShow(true);

				// Throw out some fireworks!
				InstanceManager.menuSceneController.dragonScroller.LaunchDragonPurchasedFX();
			})
			.AppendInterval(0.1f)
			.AppendCallback(() => {
				// Navigate to dragon unlock screen if required
				if(_gotoDragonUnlockScreen) {
					InstanceManager.menuSceneController.GoToScreen(MenuScreen.DRAGON_UNLOCK);
				}
			})
			.AppendInterval(0.5f)	// Add some delay before unlocking input to avoid issues when spamming touch (fixes issue https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-765)
			.AppendCallback(() => {
				// Toggle animating mode
				SetAnimationFlag(false, !_gotoDragonUnlockScreen);  // Particular case when the first M dragon has been acquired in the Results Screen!
			})
			.SetAutoKill(true)
			.Play();
	}

	/// <summary>
	/// Launch the acquire animation! Dragon acquired via XP+SC
	/// </summary>
	/// <param name="_acquiredDragonSku">Acquired dragon sku.</param>
	public void LaunchAcquireAnim(string _acquiredDragonSku) {
		// Program animation
		DOTween.Sequence()
			.AppendCallback(() => {
				// Toggle animating mode
				SetAnimationFlag(true, true);

				// Throw out some fireworks!
				InstanceManager.menuSceneController.dragonScroller.LaunchDragonPurchasedFX();

				// Trigger SFX
				AudioController.Play("hd_unlock_dragon");
			})
			.AppendInterval(1f)		// Add some delay before unlocking input to avoid issues when spamming touch (fixes issue https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-765)
			.AppendCallback(() => {
				// Check for new tease/reveals
				bool pendingReveals = CheckPendingReveals();

				// Toggle animating mode
				SetAnimationFlag(false, !pendingReveals);   // Only allow post actions if there are no pending reveals

				// OTA: Check if the dragon is downloaded
				Downloadables.Handle acquiredDragonHandle = HDAddressablesManager.Instance.GetHandleForClassicDragon(_acquiredDragonSku);
				if(!acquiredDragonHandle.IsAvailable()) {
					// Initialize download flow with handle for ALL and check for popups
					m_assetsDownloadFlow.InitWithHandle(HDAddressablesManager.Instance.GetHandleForAllDownloadables());

                    // this case will never be triggered by the Sparks acquisition, so we know
                    // that if we reach this case, its because the player acquired a medium (or bigger) dragon
                    m_assetsDownloadFlow.OpenPopupIfNeeded(AssetsDownloadFlow.Context.PLAYER_BUYS_NOT_DOWNLOADED_DRAGON);
				}
			})
			.SetAutoKill(true)
			.Play();
	}

	/// <summary>
	/// Launch the tease animation!
	/// </summary>
	/// <param name="_teaseDragonSku">Teased dragon sku.</param>
	private void LaunchTeaseAnim(string _teaseDragonSku) {
		// Aux vars
		MenuDragonSlot slot = InstanceManager.menuSceneController.dragonScroller.GetDragonSlot(_teaseDragonSku);
		IDragonData dragonData = DragonManager.GetDragonData(_teaseDragonSku);

		DOTween.Sequence()
			.AppendCallback(() => {
				// Toggle animating mode
				SetAnimationFlag(true, true);

				InstanceManager.menuSceneController.hud.animator.ForceHide(true, false);
				for(int i = 0; i < m_toHideOnUnlockAnim.Length; i++) {
					m_toHideOnUnlockAnim[i].ForceHide(true, false);
				}

                for (int i = 0; i < m_toHideOnTeaseAnim.Length; i++) {
                    m_toHideOnTeaseAnim[i].ForceHide(true, false);
                }

				// Do not desactivate to allow async loading
				slot.animator.ForceHide(false, false);
			})
			.AppendInterval(0.1f)	// Avoid 0 duration
			.AppendCallback(() => {
				InstanceManager.menuSceneController.dragonScroller.FocusDragon(_teaseDragonSku, true);
			})
			.AppendInterval(1f)	// Avoid 0 duration
			.AppendCallback(() => {
				MenuDragonPreview preview = InstanceManager.menuSceneController.dragonScroller.GetDragonPreview(_teaseDragonSku);
				preview.equip.EquipDisguiseShadow();
				preview.allowAltAnimations = false;

				dragonData.Tease();	// [AOC] Mark as teased before actually showing it, otherwise the slot will auto-hide itself again!
				slot.animator.ForceShow(true);

				// SFX
				AudioController.Play(UIConstants.GetDragonTierSFX(dragonData.tier));
			})
			.AppendInterval(2f)
			.AppendCallback(() => {
				// Are there more reveals to perform?
				bool pendingReveals = CheckPendingReveals();
				if(!pendingReveals) {
					InstanceManager.menuSceneController.hud.animator.ForceShow(true);
					for(int i = 0; i < m_toHideOnUnlockAnim.Length; i++) {
						m_toHideOnUnlockAnim[i].ForceShow(true);
					}
                    for (int i = 0; i < m_toHideOnTeaseAnim.Length; i++) {
                        m_toHideOnTeaseAnim[i].ForceShow(true);
                    }

					InstanceManager.menuSceneController.dragonSelector.OnSelectedDragonChanged(DragonManager.currentDragon, DragonManager.currentDragon);
					InstanceManager.menuSceneController.dragonScroller.FocusDragon(DragonManager.currentDragon.def.sku, true);
				}

				// Toggle animating mode
				SetAnimationFlag(false, !pendingReveals, 0.5f); // [AOC] After some delay to wait for the scroll anim to return
			})
			.SetAutoKill(true)
			.Play();
	}

	/// <summary>
	/// Launch the reveal animation!
	/// </summary>
	/// <param name="_revealDragonSku">The revealed dragon sku.</param>
	private void LaunchRevealAnim(string _revealDragonSku) {
		// Aux vars
		MenuDragonSlot slot = InstanceManager.menuSceneController.dragonScroller.GetDragonSlot(_revealDragonSku);
		IDragonData dragonData = DragonManager.GetDragonData(_revealDragonSku);

		DOTween.Sequence()
			.AppendCallback(() => {
				InstanceManager.menuSceneController.hud.animator.ForceHide(true, false);
				for(int i = 0; i < m_toHideOnUnlockAnim.Length; i++) {
					m_toHideOnUnlockAnim[i].ForceHide(true, false);
				}
                for (int i = 0; i < m_toHideOnTeaseAnim.Length; i++) {
                    m_toHideOnTeaseAnim[i].ForceHide(true, false);
                }
				if (!dragonData.isTeased) {
					// Do not desactivate to allow async loading
					slot.animator.ForceHide(false, false);
				}

				// Toggle animating mode
				SetAnimationFlag(true, true);
			})
			.AppendInterval(0.1f)	// Avoid 0 duration
			.AppendCallback(() => {
				InstanceManager.menuSceneController.dragonScroller.FocusDragon(_revealDragonSku, true);
			})
			.AppendInterval(1f)	// Avoid 0 duration
			.AppendCallback(() => {
				if (!dragonData.isTeased) {
					slot.animator.ForceShow(true);
				}

				// SFX
				AudioController.Play(UIConstants.GetDragonTierSFX(dragonData.tier));

				// Equip default disguise to clear shadow effect
				MenuDragonPreview preview = InstanceManager.menuSceneController.dragonScroller.GetDragonPreview(_revealDragonSku);
				preview.equip.EquipDisguise("");

				// Tell the loader to not use the shadow material again (dynamic loading fix HDK-1956)
				InstanceManager.menuSceneController.dragonScroller.GetDragonSlot(_revealDragonSku).dragonLoader.useShadowMaterial = false;
			})
			.AppendInterval(2f)
			.AppendCallback(() => {			
				dragonData.Reveal();

				// Are there more dragons to reveal?
				bool pendingReveals = CheckPendingReveals();
				if(!pendingReveals) {
					// No more dragons to reveal! Go back to current dragon
					InstanceManager.menuSceneController.hud.animator.ForceShow(true);
					for(int i = 0; i < m_toHideOnUnlockAnim.Length; i++) {
						m_toHideOnUnlockAnim[i].ForceShow(true);
					}
					for(int i = 0; i < m_toHideOnTeaseAnim.Length; i++) {
						m_toHideOnTeaseAnim[i].ForceShow(true);
					}
					InstanceManager.menuSceneController.dragonSelector.OnSelectedDragonChanged(DragonManager.currentDragon, DragonManager.currentDragon);
					InstanceManager.menuSceneController.dragonScroller.FocusDragon(DragonManager.currentDragon.def.sku, true);
				}

				// Toggle animating mode
				SetAnimationFlag(false, !pendingReveals, 0.5f); // [AOC] After some delay to wait for the scroll anim to return
			})
			.SetAutoKill(true)
			.Play();
	}

	/// <summary>
	/// Check the current states of the dragons and determines whether there are 
	/// dragons pending to be TEASED/REVEALED.
	/// Initializes the <c>m_dragonToTease</c> and <c>m_dragonToReveal</c> vars.
	/// </summary>
	/// <returns>Whether there are pending reveals or not.</returns>
	private bool CheckPendingReveals() {
		// Check dragons to tease
		// [AOC] Special case: if dragon scroll tutorial hasn't been yet completed, 
		//		 mark target dragons as already teased to prevent conflict with the tutorial scroll animation
		List<IDragonData> toTease = DragonManager.GetDragonsByLockState(IDragonData.LockState.TEASE);
		List<IDragonData> toReveal = DragonManager.GetDragonsByLockState(IDragonData.LockState.REVEAL);
		if(UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.DRAGON_SELECTION)) {
			// Dragon scroll tutorial completed, pick first dragon to tease/reveal
			m_dragonToTease = toTease.First();
			m_dragonToReveal = toReveal.First();
		} else {
			// Dragon scroll tutorial hasn't been completed! Don't launch tease/reveal animations
			m_dragonToTease = null;
			m_dragonToReveal = null;

			// Mark as teased to prevent launching the reveal anim in the future
			for(int i = 0; i < toTease.Count; ++i) {
				toTease[i].Tease();
			}

			// Mark as revealed to prevent launching the reveal anim in the future
			for(int i = 0; i < toReveal.Count; ++i) {
				toTease[i].Reveal();
			}
		}

		return m_dragonToTease != null || m_dragonToReveal != null;
	}

	/// <summary>
	/// Toggles the "animating" flag on or off and performs several actions.
	/// </summary>
	/// <param name="_animating">The animating state of the screen.</param>
	/// <param name="_triggerActions">Trigger actions when starting/finishing the animation?</param>
	/// <param name="_actionsDelay">Delay before performing pre/post animation actions.</param>
	private void SetAnimationFlag(bool _animating, bool _triggerActions, float _actionsDelay = 0f) {
		// Store flag
		m_isAnimating = _animating;

		// Lock/Unlock all UI input
		Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, _animating);

		// Delayed actions
		if(_triggerActions) {
			UbiBCN.CoroutineManager.DelayedCall(() => {
				// Toggle OTA flow
				if(m_assetsDownloadFlow != null) {
					m_assetsDownloadFlow.Toggle(!m_isAnimating);   // Don't allow while animating
				}
			}, _actionsDelay);    // Don't delay when starting the animation
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The screen is about to open.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Reset animating flag
		SetAnimationFlag(false, true);

		// If a dragon was just unlocked, prepare a nice unlock animation sequence!
		if(!string.IsNullOrEmpty(GameVars.unlockedDragonSku)) {
			// Do anim!
			LaunchUnlockAnim(GameVars.unlockedDragonSku, m_initialDelay, m_scrollDuration, false);

			// Reset flag
			GameVars.unlockedDragonSku = string.Empty;
		}

		// Initialize the assets download flow for ALL assets
		// Only show if player has already been notified
		Downloadables.Handle allHandle = HDAddressablesManager.Instance.GetHandleForAllDownloadables();
		if(!allHandle.NeedsToRequestPermission()) {
			// Init the assets download flow. Don't show popups though, the menu interstitial popups controller will take care of it
			m_assetsDownloadFlow.InitWithHandle(allHandle);
		}
	}

	/// <summary>
	/// The current menu screen has changed (animation starts now).
	/// </summary>
	/// <param name="_from">Source screen.</param>
	/// <param name="_to">Target screen.</param>
	private void OnTransitionStarted(MenuScreen _from, MenuScreen _to) {
		// Hide all dragons that are not meant to be displayed
		if(this.enabled) {	// [AOC] To replicate previous logic where the event was subscribed on the Enable/Disable scope
			List<IDragonData> dragonsByOrder = DragonManager.GetDragonsByOrder(IDragonData.Type.CLASSIC);
			foreach(IDragonData data in dragonsByOrder) {
				if(data.lockState == IDragonData.LockState.HIDDEN || data.lockState == IDragonData.LockState.TEASE) {
					MenuDragonSlot slot = InstanceManager.menuSceneController.dragonScroller.GetDragonSlot(data.def.sku);
					slot.animator.Hide(true, false);    // Do not desactivate to allow async loading
				}
			}
		}

		// If leaving this screen
		if(_from == MenuScreen.DRAGON_SELECTION) {
			// Remove "new" flag from incubating eggs
			for(int i = 0; i < EggManager.INVENTORY_SIZE; i++) {
				if(EggManager.inventory[i] != null) {
					EggManager.inventory[i].isNew = false;
				}
			}

			// Save persistence to store current dragon
			PersistenceFacade.instance.Save_Request(true);
		}
	}

	/// <summary>
	/// The current menu screen has changed (animation ends now).
	/// </summary>
	/// <param name="_from">Source screen.</param>
	/// <param name="_to">Target screen.</param>
	private void OnTransitionEnd(MenuScreen _from, MenuScreen _to) {
		// If entering this screen
		if(_to == MenuScreen.DRAGON_SELECTION) {
			// If we have a dragon selection pending, do it now!
			if(!string.IsNullOrEmpty(m_pendingToSelectDragon)) {
				InstanceManager.menuSceneController.SetSelectedDragon(m_pendingToSelectDragon);
				m_pendingToSelectDragon = string.Empty;
			}
		}
	}    

    /// <summary>
    /// Play button has been pressed.
    /// </summary>
    public void OnPlayButton() {
		// Avoid spamming
		if(!InstanceManager.menuSceneController.transitionManager.transitionAllowed) return;

		// Check whether all assets required for the current dragon are available or not
		// [AOC] CAREFUL! Current dragon is not necessarily the selected one! Make sure we're checking the right set of assets.
		// Get assets download handle for current dragon
		string currentDragonSku = UsersManager.currentUser.currentClassicDragon;
		Downloadables.Handle currentDragonHandle = HDAddressablesManager.Instance.GetHandleForClassicDragon(currentDragonSku);
		if(!currentDragonHandle.IsAvailable()) {
			// Scroll back to current dragon
			InstanceManager.menuSceneController.SetSelectedDragon(currentDragonSku);

			// Resources not available, which means we need to download ALL
			m_assetsDownloadFlow.InitWithHandle(HDAddressablesManager.Instance.GetHandleForAllDownloadables());

			// If needed, show assets download popup. Download will be already in progress at this point.
			m_assetsDownloadFlow.OpenPopupByState(PopupAssetsDownloadFlow.PopupType.ANY);

			// Don't move to next screen
			return;
		}

		// Select target screen
		MenuScreen nextScreen = MenuScreen.MISSIONS;

		// If there is an active quest, go to the quest screen
		// Do it as well if the event is pending reward collection
		if ( UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_QUESTS_AT_RUN )
		{
			HDQuestManager quest = HDLiveDataManager.quest;
			if ( quest.EventExists() )	
			{
				if (quest.IsTeasing() || quest.IsRunning() || quest.IsRewardPending())
				{
					nextScreen = MenuScreen.GLOBAL_EVENTS;	
				}
			}
		}

		// Go to target screen
		InstanceManager.menuSceneController.GoToScreen(nextScreen);

		// Tutorial tracking
		if (!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.MISSIONS_INFO)) {
			HDTrackingManager.Instance.Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps._08_continue_clicked);
		}
	}

	/// <summary>
	/// Skins screen button has been pressed.
	/// </summary>
	public void OnSkinsButton() {
		// Avoid spamming
		if(!InstanceManager.menuSceneController.transitionManager.transitionAllowed) return;

		// Check whether all assets required for the selected dragon are available or not
		// Get assets download handle for current dragon
		string selectedDragonSku = InstanceManager.menuSceneController.selectedDragon;
		Downloadables.Handle dragonHandle = HDAddressablesManager.Instance.GetHandleForClassicDragon(selectedDragonSku);
		if(!dragonHandle.IsAvailable()) {
			// Resources not available, which means we need to download ALL
			m_assetsDownloadFlow.InitWithHandle(HDAddressablesManager.Instance.GetHandleForAllDownloadables());

			// If needed, show assets download popup
			m_assetsDownloadFlow.OpenPopupByState(PopupAssetsDownloadFlow.PopupType.ANY, AssetsDownloadFlow.Context.PLAYER_CLICKS_ON_SKINS);

			// Don't move to next screen
			return;
		}

		// All checks passed, go to target screen
		InstanceManager.menuSceneController.GoToScreen(MenuScreen.SKINS);
	}
}
