﻿// MenuDragonSceneController.cs
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

	private bool m_isAnimating = false;
	private bool isAnimating {
		get { return m_isAnimating; }
		set { m_isAnimating = value; }
	}

	private MenuScreen m_goToScreen = MenuScreen.NONE;
	private DragonData m_dragonToTease = null;
	private DragonData m_dragonToReveal = null;

    private bool m_showPendingTransactions = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	void Start(){
		if (GlobalEventManager.user != null && GlobalEventManager.Connected() ){
			if (GlobalEventManager.currentEvent == null && GlobalEventManager.user.globalEvents.Count <= 0){
				// ask for live events again
				GlobalEventManager.TMP_RequestCustomizer();
			}
		}
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
        m_showPendingTransactions = false;

        // Subscribe to external events.
        Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnTransitionStarted);
        Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnTransitionEnded);

        // Check whether we need to move to another screen
        // Check order is relevant!
        m_goToScreen = MenuScreen.NONE;

		// Check pending rewards
		if(UsersManager.currentUser.rewardStack.Count > 0) {
			m_goToScreen = MenuScreen.PENDING_REWARD;
			return;
		}

		if ( UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_GLOBAL_EVENTS_AT_RUN ) 
		{
			// Check global events rewards
			GlobalEvent ge = GlobalEventManager.currentEvent;
			if (ge != null) {
				ge.UpdateState();
				if (ge.isRewardAvailable) {
					m_goToScreen = MenuScreen.EVENT_REWARD;
					return;
				}
			}
		}

        // Lowest priority: show pending transactions. They're showing here because we know that the currencies are visible for the user on this screen
        m_showPendingTransactions = TransactionManager.instance.Flow_NeedsToShowPendingTransactions();        
    }

	/// <summary>
	/// Raises the disable event.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe to external events.
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnTransitionStarted);
        Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnTransitionEnded);        
    }

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Do we have a screen change pending?
		if(m_goToScreen != MenuScreen.NONE) {
			// Which screen?
			switch(m_goToScreen) {
				case MenuScreen.EVENT_REWARD: {
					EventRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.EVENT_REWARD).ui.GetComponent<EventRewardScreen>();
					scr.StartFlow();
					InstanceManager.menuSceneController.GoToScreen(MenuScreen.EVENT_REWARD);
				} break;

				case MenuScreen.PENDING_REWARD: {
					PendingRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.PENDING_REWARD).ui.GetComponent<PendingRewardScreen>();
					scr.StartFlow(true);
					InstanceManager.menuSceneController.GoToScreen(MenuScreen.PENDING_REWARD);
				} break;
			}

			// Clear var
			m_goToScreen = MenuScreen.NONE;
			return;
		}

		// Cheat for simulating dragon unlock
		#if UNITY_EDITOR
		if(Input.GetKeyDown(KeyCode.U)) {
			int order = DragonManager.currentDragon.def.GetAsInt("order");
			if(order < DragonManager.dragonsByOrder.Count - 1) {	// Exclude if playing with last dragon
				DragonData nextDragonData = DragonManager.dragonsByOrder[order + 1];
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
	/// Launch the unlock animation!
	/// </summary>
	/// <param name="_unlockedDragonSku">Unlocked dragon sku.</param>
	/// <param name="_initialDelay">Initial delay before launching the unlock animation.</param>
	/// <param name="_scrollDuration">Use it to sync with scrolling to target dragon.</param>
	/// <param name="_gotoDragonUnlockScreen">Whether to go the DragonUnlockScreen after the animation or not.</param>
	public void LaunchUnlockAnim(string _unlockedDragonSku, float _initialDelay, float _scrollDuration, bool _gotoDragonUnlockScreen) {
		// Program lock animation sequence
		DOTween.Sequence()
			.AppendCallback(() => {
				// Lock all input
				Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, true);

				// Disable normal behaviour
				m_lockIcon.GetComponent<MenuShowConditionally>().enabled = false;
				if(_unlockedDragonSku == InstanceManager.menuSceneController.selectedDragon) {
					// Target dragon is already selected, make sure lock icon is visible!
					m_lockIcon.GetComponent<ShowHideAnimator>().ForceShow();
				}

				// Toggle flag
				isAnimating = true;
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
			.AppendInterval(0.5f)		// Add some delay before unlocking input to avoid issues when spamming touch (fixes issue https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-765)
			.AppendCallback(() => {
				// Unlock input
				// Add some delay to avoid issues when spamming touch (fixes issue https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-765)
				Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, false);

				// Toggle flag
				isAnimating = false;
			})
			.SetAutoKill(true)
			.Play();
	}

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

				// Toggle flag
				isAnimating = true;

				// Throw out some fireworks!
				InstanceManager.menuSceneController.dragonScroller.LaunchDragonPurchasedFX();

				// Trigger SFX
				AudioController.Play("hd_unlock_dragon");
			})
			.AppendInterval(1f)		// Add some delay before unlocking input to avoid issues when spamming touch (fixes issue https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-765)
			.AppendCallback(() => {
				// Unlock input
				Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, false);

				// Toggle flag
				isAnimating = false;

				// Check for new tease/reveals
				CheckPendingReveals();
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
		DragonData dragonData = DragonManager.GetDragonData(_teaseDragonSku);

		DOTween.Sequence()
			.AppendCallback(() => {
				// Lock all input
				Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, true);

				InstanceManager.menuSceneController.hud.animator.ForceHide(true, false);
				for(int i = 0; i < m_toHideOnUnlockAnim.Length; i++) {
					m_toHideOnUnlockAnim[i].ForceHide(true, false);
				}

				slot.animator.ForceHide(false);

				// Toggle flag
				isAnimating = true;
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
				Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, false);

				CheckPendingReveals();

				if (m_dragonToTease == null && m_dragonToReveal == null) {
					InstanceManager.menuSceneController.hud.animator.ForceShow(true);
					for(int i = 0; i < m_toHideOnUnlockAnim.Length; i++) {
						m_toHideOnUnlockAnim[i].ForceShow(true);
					}
					InstanceManager.menuSceneController.dragonSelector.OnSelectedDragonChanged(DragonManager.currentDragon, DragonManager.currentDragon);
					InstanceManager.menuSceneController.dragonScroller.FocusDragon(DragonManager.currentDragon.def.sku, true);
				}

				// Toggle flag
				isAnimating = false;
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
		DragonData dragonData = DragonManager.GetDragonData(_revealDragonSku);

		DOTween.Sequence()
			.AppendCallback(() => {
				// Lock all input
				Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, true);

				InstanceManager.menuSceneController.hud.animator.ForceHide(true, false);
				for(int i = 0; i < m_toHideOnUnlockAnim.Length; i++) {
					m_toHideOnUnlockAnim[i].ForceHide(true, false);
				}

				if (!dragonData.isTeased) {
					slot.animator.ForceHide(false);
				}

				// Toggle flag
				isAnimating = true;
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

				MenuDragonPreview preview = InstanceManager.menuSceneController.dragonScroller.GetDragonPreview(_revealDragonSku);
				preview.equip.EquipDisguise("");
			})
			.AppendInterval(2f)
			.AppendCallback(() => {			
				Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, false);
			
				dragonData.Reveal();
				CheckPendingReveals();

				if (m_dragonToTease == null && m_dragonToReveal == null) {
					InstanceManager.menuSceneController.hud.animator.ForceShow(true);
					for(int i = 0; i < m_toHideOnUnlockAnim.Length; i++) {
						m_toHideOnUnlockAnim[i].ForceShow(true);
					}
					InstanceManager.menuSceneController.dragonSelector.OnSelectedDragonChanged(DragonManager.currentDragon, DragonManager.currentDragon);
					InstanceManager.menuSceneController.dragonScroller.FocusDragon(DragonManager.currentDragon.def.sku, true);
				}

				// Toggle flag
				isAnimating = false;
			})
			.SetAutoKill(true)
			.Play();
	}

	/// <summary>
	/// Check the current states of the dragons and determines whether there are 
	/// dragons pending to be TEASED/REVEALED.
	/// Initializes the <c>m_dragonToTease</c> and <c>m_dragonToReveal</c> vars.
	/// </summary>
	private void CheckPendingReveals() {
		// Check dragons to tease
		// [AOC] Special case: if dragon scroll tutorial hasn't been yet completed, 
		//		 mark target dragons as already teased to prevent conflict with the tutorial scroll animation
		List<DragonData> toTease = DragonManager.GetDragonsByLockState(DragonData.LockState.TEASE);
		List<DragonData> toReveal = DragonManager.GetDragonsByLockState(DragonData.LockState.REVEAL);
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
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The screen is about to open.
	/// </summary>
	public void OnOpenPreAnimation() {
		// If a dragon was just unlocked, prepare a nice unlock animation sequence!
		if(!string.IsNullOrEmpty(GameVars.unlockedDragonSku)) {
			// Do anim!
			LaunchUnlockAnim(GameVars.unlockedDragonSku, m_initialDelay, m_scrollDuration, false);

			// Reset flag
			GameVars.unlockedDragonSku = string.Empty;
		} else {
			// Check whether we need to launch any other animation
			CheckPendingReveals();
		}
	}

	/// <summary>
	/// The current menu screen has changed (animation starts now).
	/// </summary>
	/// <param name="_from">Source screen.</param>
	/// <param name="_to">Target screen.</param>
	private void OnTransitionStarted(MenuScreen _from, MenuScreen _to) {
		// Hide all dragons that are not meant to be displayed
		foreach (DragonData data in DragonManager.dragonsByOrder) {
			if (data.lockState == DragonData.LockState.HIDDEN || data.lockState == DragonData.LockState.TEASE) {
				MenuDragonSlot slot = InstanceManager.menuSceneController.dragonScroller.GetDragonSlot(data.def.sku);
				slot.animator.Hide(true);
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
    private void OnTransitionEnded(MenuScreen _from, MenuScreen _to) {
        if (_to == MenuScreen.DRAGON_SELECTION && m_showPendingTransactions)
        {
            m_showPendingTransactions = false;
            TransactionManager.instance.Flow_PerformPendingTransactions(InstanceManager.menuSceneController.GetUICanvasGO());
        }        
    }

    /// <summary>
    /// Play button has been pressed.
    /// </summary>
    public void OnPlayButton() {
		// Select target screen
		MenuScreen nextScreen = MenuScreen.MISSIONS;

		// If there is an active global event, go to the events screen
		// Do it as well if the event is pending reward collection
		if(GlobalEventManager.currentEvent != null
			&& (GlobalEventManager.currentEvent.isActive || GlobalEventManager.currentEvent.isRewardAvailable)
			&& UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_GLOBAL_EVENTS_AT_RUN) {
			nextScreen = MenuScreen.GLOBAL_EVENTS;
		}

		// Go to target screen
		InstanceManager.menuSceneController.GoToScreen(nextScreen);

		// Tutorial tracking
		if (!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.MISSIONS_INFO)) {
			HDTrackingManager.Instance.Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps._08_continue_clicked);
		}
	}
}