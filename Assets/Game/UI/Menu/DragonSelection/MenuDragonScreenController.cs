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
	[SerializeField] private NavigationShowHideAnimator m_bottomBarAnim = null;
	[SerializeField] private NavigationShowHideAnimator m_unlockButtonsAnim = null;
	[SerializeField] private NavigationShowHideAnimator m_arrowsAnim = null;
	[Space]
	[SerializeField] private float m_initialDelay = 1f;
	[SerializeField] private float m_unlockAnimDuration = 1f;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
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
		// Cheat for simulating dragon unlock
		#if UNITY_EDITOR
		if(Input.GetKeyDown(KeyCode.U)) {
			int order = DragonManager.currentDragon.def.GetAsInt("order");
			if(order < DragonManager.dragonsByOrder.Count - 1) {	// Exclude if playing with last dragon
				DragonData nextDragonData = DragonManager.dragonsByOrder[order + 1];
				if(nextDragonData != null) {
					InstanceManager.menuSceneController.dragonSelector.SetSelectedDragon(DragonManager.currentDragon.def.sku);
					DOVirtual.DelayedCall(1f, () => { LaunchUnlockAnim(nextDragonData.def.sku, m_initialDelay); });
				}
			}
		}
		#endif
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Launch the unlock animation!
	/// </summary>
	/// <param name="_unlockedDragonSku">Unlocked dragon sku.</param>
	/// <param name="_initialDelay">Initial delay before launching the unlock animation. Use it to sync with scrolling to target dragon.</param>
	public void LaunchUnlockAnim(string _unlockedDragonSku, float _initialDelay) {
		// Clean screen
		// Don't disable elements, otherwise they won't be enabled on the next screen change!
		m_bottomBarAnim.ForceHide(true, false);
		m_unlockButtonsAnim.ForceHide(true, false);
		m_arrowsAnim.ForceHide(true, false);
		InstanceManager.menuSceneController.hud.animator.ForceHide(true, false);

		// Program lock animation sequence
		DOTween.Sequence()
			.AppendCallback(() => {
				// Lock all input
				Messenger.Broadcast<bool>(EngineEvents.UI_LOCK_INPUT, true);

				// Prepare lock icon animation
				// Disable normal behaviour
				m_lockIcon.GetComponent<MenuShowConditionally>().enabled = false;
				m_lockIcon.GetComponent<ShowHideAnimator>().RestartShow();
				m_lockIcon.animator.SetTrigger("idle");

				// Navigate to target dragon (should be next dragon)
				InstanceManager.menuSceneController.dragonSelector.SetSelectedDragon(_unlockedDragonSku);
			})
			.AppendInterval(Mathf.Max(0.1f, _initialDelay))	// Sync with dragon scroll duration. Avoid 0 duration, otherwise lock animator gets broken
			.AppendCallback(() => {
				// Show icon unlock animation
				m_lockIcon.animator.ResetTrigger("idle");	// Just in case initial delay is 0, both triggers would be set at the same frame and animation wouldn't work
				m_lockIcon.animator.SetTrigger("unlock");
			})
			.AppendInterval(m_unlockAnimDuration)
			.AppendCallback(() => {
				// Unlock input
				Messenger.Broadcast<bool>(EngineEvents.UI_LOCK_INPUT, false);

				// Restore lock icon to the idle state (otherwise default values will get corrupted when deactivating the object)
				m_lockIcon.GetComponent<MenuShowConditionally>().enabled = true;
				m_lockIcon.GetComponent<ShowHideAnimator>().ForceHide(false, false);
				m_lockIcon.animator.SetTrigger("idle");

				// Navigate to dragon unlock screen!
				InstanceManager.menuSceneController.screensController.GoToScreen((int)MenuScreens.DRAGON_UNLOCK);
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
			LaunchUnlockAnim(GameVars.unlockedDragonSku, m_initialDelay);

			// Reset flag
			GameVars.unlockedDragonSku = string.Empty;
		}
	}

	/// <summary>
	/// Navigation screen has changed (animation starts now).
	/// </summary>
	/// <param name="_event">Event data.</param>
	private void OnNavigationScreenChanged(NavigationScreenSystem.ScreenChangedEventData _event) {
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
			PersistenceManager.Save(true);
		}
	}
}