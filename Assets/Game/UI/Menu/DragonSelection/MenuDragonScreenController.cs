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
	[Space]
	[SerializeField] private NavigationShowHideAnimator[] m_toHideOnUnlockAnim = null;

	private bool m_goToGlobalEventRewardScreen;
	private DragonData m_dragonToTease = null;
	private DragonData m_dragonToReveal = null;

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
		m_goToGlobalEventRewardScreen = false;

		GlobalEvent ge = GlobalEventManager.currentEvent;
		if (ge != null) {
			ge.UpdateState();
			if (ge.isRewardAvailable) {
				m_goToGlobalEventRewardScreen = true;
			}
		}

		m_dragonToTease = DragonManager.GetDragonsByLockState(DragonData.LockState.TEASE).First();
		m_dragonToReveal = DragonManager.GetDragonsByLockState(DragonData.LockState.REVEAL).First();

		// Subscribe to external events.
		Messenger.AddListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnNavigationScreenChanged);
	}

	/// <summary>
	/// Raises the disable event.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe to external events.
		Messenger.RemoveListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnNavigationScreenChanged);
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		if (m_goToGlobalEventRewardScreen) {
			EventRewardScreen scr = InstanceManager.menuSceneController.GetScreen(MenuScreens.REWARD).GetComponent<EventRewardScreen>();
			scr.StartFlow();
			InstanceManager.menuSceneController.screensController.GoToScreen((int)MenuScreens.REWARD);
			m_goToGlobalEventRewardScreen = false;
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
		if (!InstanceManager.menuSceneController.dragonScroller.cameraAnimator.isTweening) {
			if (m_dragonToReveal != null) {
				LaunchRevealAnim(m_dragonToReveal.def.sku);
			} else if (m_dragonToTease != null) {
				LaunchTeaseAnim(m_dragonToTease.def.sku);
			}
			// we'll launch only one animation at a time

			m_dragonToReveal = null;
			m_dragonToTease = null;
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
				Messenger.Broadcast<bool>(EngineEvents.UI_LOCK_INPUT, true);
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
					MenuShowConditionally showConditionally = m_toHideOnUnlockAnim[i].GetComponent<MenuShowConditionally>();
					if(showConditionally != null) {
						showConditionally.enabled = false;
					}
				}
				InstanceManager.menuSceneController.hud.animator.ForceHide(true, false);

				// Prepare lock icon animation
				// Disable normal behaviour
				m_lockIcon.GetComponent<MenuShowConditionally>().enabled = false;
				m_lockIcon.GetComponent<ShowHideAnimator>().RestartShow();

				// Show icon unlock animation
				//m_lockIcon.animator.ResetTrigger("idle");	// Just in case initial delay is 0, both triggers would be set at the same frame and animation wouldn't work
				m_lockIcon.animator.SetTrigger("unlock");

				// Trigger SFX
				AudioController.Play("hd_unlock_dragon");
			})
			.AppendInterval(m_unlockAnimDuration)
			.AppendCallback(() => {
				// Unlock input
				Messenger.Broadcast<bool>(EngineEvents.UI_LOCK_INPUT, false);

				// Restore lock icon to the idle state (otherwise default values will get corrupted when deactivating the object)
				m_lockIcon.GetComponent<MenuShowConditionally>().enabled = true;
				m_lockIcon.GetComponent<ShowHideAnimator>().ForceHide(false, false);
				m_lockIcon.animator.SetTrigger("idle");

				// Restore all hidden items
				for(int i = 0; i < m_toHideOnUnlockAnim.Length; i++) {
					// Leave them hidden if changing screens!
					if(!_gotoDragonUnlockScreen) {
						m_toHideOnUnlockAnim[i].Show(true);
					}

					// Re-enable all disabled ShowConditionally components
					MenuShowConditionally showConditionally = m_toHideOnUnlockAnim[i].GetComponent<MenuShowConditionally>();
					if(showConditionally != null) {
						showConditionally.enabled = true;
					}
				}

				// Navigate to dragon unlock screen if required
				if(_gotoDragonUnlockScreen) {
					InstanceManager.menuSceneController.screensController.GoToScreen((int)MenuScreens.DRAGON_UNLOCK);
				}

				// Throw out some fireworks!
				InstanceManager.menuSceneController.dragonScroller.LaunchDragonPurchasedFX();
			})
			.SetAutoKill(true)
			.Play();
	}

	private void LaunchTeaseAnim(string _teaseDragonSku) {
		DOTween.Sequence()
			.AppendCallback(() => {
				// Lock all input
				Messenger.Broadcast<bool>(EngineEvents.UI_LOCK_INPUT, true);
				MenuDragonSlot slot = InstanceManager.menuSceneController.dragonScroller.GetDragonSlot(_teaseDragonSku);
				slot.animator.ForceHide(false);
			})
			.AppendInterval(0.1f)	// Avoid 0 duration
			.AppendCallback(() => {
				InstanceManager.menuSceneController.dragonScroller.FocusDragon(_teaseDragonSku, true);
			})
			.AppendInterval(1f)	// Avoid 0 duration
			.AppendCallback(() => {
				MenuDragonPreview preview = InstanceManager.menuSceneController.dragonScroller.GetDragonPreview(_teaseDragonSku);
				preview.equip.EquipDisguiseShadow();

				MenuDragonSlot slot = InstanceManager.menuSceneController.dragonScroller.GetDragonSlot(_teaseDragonSku);
				slot.animator.ForceShow(true);

				DragonManager.GetDragonData(_teaseDragonSku).Tease();
			})
			.AppendInterval(2f)
			.AppendCallback(() => {
				Messenger.Broadcast<bool>(EngineEvents.UI_LOCK_INPUT, false);
				m_dragonToTease = DragonManager.GetDragonsByLockState(DragonData.LockState.TEASE).First();
				m_dragonToReveal = DragonManager.GetDragonsByLockState(DragonData.LockState.REVEAL).First();

				if (m_dragonToTease == null && m_dragonToReveal == null) {
					InstanceManager.menuSceneController.dragonScroller.FocusDragon(DragonManager.currentDragon.def.sku, true);
				}
			})
			.SetAutoKill(true)
			.Play();
	}

	private void LaunchRevealAnim(string _revealDragonSku) {
		DOTween.Sequence()
			.AppendCallback(() => {
				// Lock all input
				Messenger.Broadcast<bool>(EngineEvents.UI_LOCK_INPUT, true);
				if (!DragonManager.GetDragonData(_revealDragonSku).isTeased) {
					MenuDragonSlot slot = InstanceManager.menuSceneController.dragonScroller.GetDragonSlot(_revealDragonSku);
					slot.animator.ForceHide(false);
				}
			})
			.AppendInterval(0.1f)	// Avoid 0 duration
			.AppendCallback(() => {
				InstanceManager.menuSceneController.dragonScroller.FocusDragon(_revealDragonSku, true);
			})
			.AppendInterval(1f)	// Avoid 0 duration
			.AppendCallback(() => {
				if (!DragonManager.GetDragonData(_revealDragonSku).isTeased) {
					MenuDragonSlot slot = InstanceManager.menuSceneController.dragonScroller.GetDragonSlot(_revealDragonSku);
					slot.animator.ForceShow(true);
				}

				MenuDragonPreview preview = InstanceManager.menuSceneController.dragonScroller.GetDragonPreview(_revealDragonSku);
				preview.equip.EquipDisguise("");

				DragonManager.GetDragonData(_revealDragonSku).Reveal();
			})
			.AppendInterval(2f)
			.AppendCallback(() => {			
				Messenger.Broadcast<bool>(EngineEvents.UI_LOCK_INPUT, false);
				m_dragonToTease = DragonManager.GetDragonsByLockState(DragonData.LockState.TEASE).First();
				m_dragonToReveal = DragonManager.GetDragonsByLockState(DragonData.LockState.REVEAL).First();

				if (m_dragonToTease == null && m_dragonToReveal == null) {
					InstanceManager.menuSceneController.dragonScroller.FocusDragon(DragonManager.currentDragon.def.sku, true);
				}
			})
			.SetAutoKill(true)
			.Play();
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
		}
	}

	/// <summary>
	/// Navigation screen has changed (animation starts now).
	/// </summary>
	/// <param name="_event">Event data.</param>
	private void OnNavigationScreenChanged(NavigationScreenSystem.ScreenChangedEventData _event) {

		foreach (DragonData data in DragonManager.dragonsByOrder) {
			if (data.lockState == DragonData.LockState.HIDDEN || data.lockState == DragonData.LockState.TEASE) {
				MenuDragonSlot slot = InstanceManager.menuSceneController.dragonScroller.GetDragonSlot(data.def.sku);
				slot.animator.Hide(true);
			}
		}

		// Only if it comes from the main screen navigation system
		if(_event.dispatcher != InstanceManager.menuSceneController.screensController) return;

		// If leaving this screen
		if(_event.fromScreenIdx == (int)MenuScreens.DRAGON_SELECTION) {
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

	public void OnPlayButton() {
		if (!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.MISSIONS_INFO)) {
			HDTrackingManager.Instance.Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps._08_continue_clicked);
		}
	}
}