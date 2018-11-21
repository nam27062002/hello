// MenuDragonUnlockClassicDragon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/11/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Text;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Control unlock/acquire/unavailable UI for classic dragons in the menu.
/// Depending on the selected dragon lockState, a different object will be displayed.
/// TODO:
/// - Anim delays and sync to match dragon change animation
/// - Check active dragon discounts
/// </summary>
public class MenuDragonUnlockClassicDragon : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private const string UNLOCK_WITH_HC_RESOURCES_FLOW_NAME = "UNLOCK_DRAGON_HC";	// Unlock and acquire a locked dragon using HC
	private const string UNLOCK_WITH_SC_RESOURCES_FLOW_NAME = "UNLOCK_DRAGON_SC";	// Acquire an already unlocked dragon using SC

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Localizer m_hcPriceText = null;
	[SerializeField] private Localizer m_scPriceText = null;
	[SerializeField] private Localizer m_unavailableInfoText = null;
	[Space]
	[SerializeField] private ShowHideAnimator m_hcButtonAnim = null;
	[SerializeField] private ShowHideAnimator m_scButtonAnim = null;
	[SerializeField] private ShowHideAnimator m_unavailableInfoAnim = null;

	// Internal
	private bool m_firstEnablePassed = false;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Force a hide and then apply for the first time with current values and without animation
		TriggerAnimator(m_hcButtonAnim, false, false, false);
		TriggerAnimator(m_scButtonAnim, false, false, false);
		TriggerAnimator(m_unavailableInfoAnim, false, false, false);
		Refresh(InstanceManager.menuSceneController.selectedDragonData, false, false);
	}

	/// <summary>
	/// First update
	/// </summary>
	private void Start() {
		// Subscribe to external events
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Refresh! Don't animate if it's the first time
		Refresh(InstanceManager.menuSceneController.selectedDragonData, m_firstEnablePassed, false);
		m_firstEnablePassed = true;
	}
	
	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}

	/// <summary>
	/// Refresh with data from currently selected dragon.
	/// </summary>
	/// <param name="_data">The data of the selected dragon.</param>
	/// <param name="_animate">Whether to trigger animations or not.</param>
	/// <param name="_resetAnim">Whether to reset animations or not.</param>
	public void Refresh(IDragonData _data, bool _animate, bool _resetAnim) {
		// Aux vars
		bool show = true;

		// Update hc unlock button
		if(m_hcPriceText != null) {
			// Display?
			show = CheckUnlockWithPC(_data);
			TriggerAnimator(m_hcButtonAnim, show, _animate, _resetAnim);

			// Refresh info
			if(show) {
				// Set text
				m_hcPriceText.Localize(
					m_hcPriceText.tid, 
					StringUtils.FormatNumber(_data.def.GetAsLong("unlockPricePC"))
				);
			}
		}

		// Update sc unlock button
		if(m_scPriceText != null) {
			// Display?
			show = CheckUnlockWithSC(_data);
			TriggerAnimator(m_scButtonAnim, show, _animate, _resetAnim);

			// Refresh info
			if(show) {
				// Set text
				m_scPriceText.Localize(
					m_scPriceText.tid,
					StringUtils.FormatNumber(_data.def.GetAsLong("unlockPriceCoins"))
				);
			}
		}

		// Update unavailable info
		if(m_unavailableInfoText != null) {
			// Display?
			show = CheckUnavailable(_data);
			TriggerAnimator(m_unavailableInfoAnim, show, _animate, _resetAnim);

			// Refresh info
			if(show) {
				// Get list of dragons required to unlock and compose a string with their names
				string separator = LocalizationManager.SharedInstance.Localize("TID_GEN_LIST_SEPARATOR");
				StringBuilder sb = new StringBuilder();
				for(int i = 0; i < _data.unlockFromDragons.Count; ++i) {
					if(i > 0) sb.Append(", ");
					sb.Append(DragonManager.GetDragonData(_data.unlockFromDragons[i]).def.GetLocalized("tidName"));
				}

				// Set text
				m_unavailableInfoText.Localize(
					m_unavailableInfoText.tid,
					sb.ToString()
				);
			}
		}
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Launch animation according to this class needs.
	/// </summary>
	/// <param name="_anim">Animation to be triggered.</param>
	/// <param name="_show">Whether to show or hide.</param>
	/// <param name="_animate">Whether to animate or not.</param>
	/// <param name="_resetAnim">Whether to reset animation or not.</param>
	private void TriggerAnimator(ShowHideAnimator _anim, bool _show, bool _animate, bool _resetAnim) {
		// Nothing if animator is not valid
		if(_anim == null) return;

		// Let animator do its magic
		// a) Restart animation when:
		if(_show && _anim.visible		// Showing and already visible
	   	&& _animate && _resetAnim		// Use Animations and Reset Animations parameters are true
	    && this.isActiveAndEnabled) {   // The parent object is active
			_anim.RestartShow();
		}

		// b) Don't restart animation for the rest of cases
		else {
			_anim.ForceSet(_show, _animate && this.isActiveAndEnabled);	// Don't animate if the parent object is not active
		}
	}

	//------------------------------------------------------------------//
	// STATIC METHODS													//
	// These methods are defined static so they can be reused in other	//
	// areas of the game without duplicating code.						//
	//------------------------------------------------------------------//
	/// <summary>
	/// Checks whether the given dragon can be unlocked with PC.
	/// </summary>
	/// <returns>Whether the given dragon can be unlocked with PC.</returns>
	/// <param name="_data">Dragon to evaluate.</param>
	public static bool CheckUnlockWithPC(IDragonData _data) {
		// [AOC] TODO!! Check dragon discounts?
		return _data.lockState == IDragonData.LockState.LOCKED;
	}

	/// <summary>
	/// Checks whether the given dragon can be unlocked with SC.
	/// </summary>
	/// <returns>Whether the given dragon can be unlocked with SC.</returns>
	/// <param name="_data">Dragon to evaluate.</param>
	public static bool CheckUnlockWithSC(IDragonData _data) {
		return _data.lockState == IDragonData.LockState.AVAILABLE;
	}

	/// <summary>
	/// Checks whether the unavailable message should be displayed for the given dragon.
	/// </summary>
	/// <returns>Whether the given dragon is unavailable.</returns>
	/// <param name="_data">Dragon to evaluate.</param>
	public static bool CheckUnavailable(IDragonData _data) {
		return _data.lockState == IDragonData.LockState.LOCKED_UNAVAILABLE;
	}

	/// <summary>
	/// Unlocks and acquires the given dragon using PC.
	/// Will check that the given dragon can actually be acquired using PC.
	/// Will perform all needed post actions such as tracking, menu animations, etc.
	/// </summary>
	/// <param name="_data">Data of the dragon to be unlocked.</param>
	public static void UnlockWithPC(IDragonData _data) {
		// Make sure the dragon we unlock is the currently selected one and that we can actually do it
		if(CheckUnlockWithPC(_data)) {
			// Get price and start purchase flow
			ResourcesFlow purchaseFlow = new ResourcesFlow(UNLOCK_WITH_HC_RESOURCES_FLOW_NAME);
			purchaseFlow.OnSuccess.AddListener(OnUnlockSuccess);
			purchaseFlow.Begin(
				_data.def.GetAsLong("unlockPricePC"),
				UserProfile.Currency.HARD,
				HDTrackingManager.EEconomyGroup.UNLOCK_DRAGON,
				_data.def
			);
		}
	}

	/// <summary>
	/// Unlocks and acquires the given dragon using SC.
	/// Will check that the given dragon can actually be acquired using SC.
	/// Will perform all needed post actions such as tracking, menu animations, etc.
	/// </summary>
	/// <param name="_data">Data of the dragon to be unlocked.</param>
	public static void UnlockWithSC(IDragonData _data) {
		// Make sure the dragon we unlock is the currently selected one and that we can actually do it
		if(CheckUnlockWithSC(_data)) {
			// Get price and start purchase flow
			ResourcesFlow purchaseFlow = new ResourcesFlow(UNLOCK_WITH_SC_RESOURCES_FLOW_NAME);
			purchaseFlow.OnSuccess.AddListener(OnUnlockSuccess);
			purchaseFlow.Begin(
				_data.def.GetAsLong("unlockPriceCoins"),
				UserProfile.Currency.SOFT,
				HDTrackingManager.EEconomyGroup.UNLOCK_DRAGON,
				_data.def
			);
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Selected dragon has changed.
	/// </summary>
	/// <param name="_dragonSku">Selected dragon sku.</param>
	public void OnDragonSelected(string _dragonSku) {
		Refresh(DragonManager.GetDragonData(_dragonSku), true, true);
	}

	/// <summary>
	/// A dragon has been acquired
	/// </summary>
	/// <param name="_data">The data of the acquired dragon.</param>
	public void OnDragonAcquired(IDragonData _data) {
		Refresh(_data, true, false);
	}

	/// <summary>
	/// Check whether this object can be displayed or not.
	/// </summary>
	/// <returns><c>true</c> if all the conditions to display this object are met, <c>false</c> otherwise.</returns>
	/// <param name="_dragonSku">Target dragon sku.</param>
	/// <param name="_screen">Target screen.</param>
	public bool OnShowCheck(string _dragonSku, MenuScreen _screen) {
		// Show only if the target dragon is locked
		IDragonData dragonData = DragonManager.GetDragonData(_dragonSku);
		return dragonData.lockState == IDragonData.LockState.LOCKED;
	}

	/// <summary>
	/// The unlock button has been pressed.
	/// </summary>
	public void OnUnlockWithPC() {
		// Use static method to unlock currently selected dragon
		UnlockWithPC(InstanceManager.menuSceneController.selectedDragonData);
	}

	/// <summary>
	/// The unlock button has been pressed.
	/// </summary>
	public void OnUnlockWithSC() {
		// Use static method to unlock currently selected dragon
		UnlockWithSC(InstanceManager.menuSceneController.selectedDragonData);
	}

	/// <summary>
	/// The unlock resources flow has been successful.
	/// </summary>
	/// <param name="_flow">The flow that triggered the event.</param>
	private static void OnUnlockSuccess(ResourcesFlow _flow) {
		// Aux vars
		IDragonData dragonData = DragonManager.GetDragonData(_flow.itemDef.sku);
		bool wasntLocked = dragonData.GetLockState() <= IDragonData.LockState.LOCKED;

		// Just acquire target dragon!
		dragonData.Acquire();

		// Track
		HDTrackingManager.Instance.Notify_DragonUnlocked(dragonData.def.sku, dragonData.GetOrder());

		// Show a nice animation!
		// Different animations depending on whether the unlock was done via PC or SC
		MenuDragonScreenController screenController = InstanceManager.menuSceneController.GetScreenData(MenuScreen.DRAGON_SELECTION).ui.GetComponent<MenuDragonScreenController>();
		if(_flow.name == UNLOCK_WITH_HC_RESOURCES_FLOW_NAME) {
			screenController.LaunchUnlockAnim(dragonData.def.sku, 0.2f, 0.1f, true);
		} else if(_flow.name == UNLOCK_WITH_SC_RESOURCES_FLOW_NAME) {
			screenController.LaunchAcquireAnim(dragonData.def.sku);
		}
	}
}
