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
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Control unlock/acquire/unavailable UI for classic dragons in the menu.
/// Depending on the selected dragon lockState, a different object will be displayed.
/// TODO:
/// - Check active dragon discounts
/// </summary>
public class MenuDragonUnlockClassicDragon : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public const string UNLOCK_WITH_HC_RESOURCES_FLOW_NAME = "UNLOCK_DRAGON_HC";	// Unlock and acquire a locked dragon using HC
	public const string UNLOCK_WITH_SC_RESOURCES_FLOW_NAME = "UNLOCK_DRAGON_SC";	// Acquire an already unlocked dragon using SC

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private UIDragonPriceSetup m_hcPriceSetup = null;
	[SerializeField] private UIDragonPriceSetup m_scPriceSetup = null;
	[Space]
	[SerializeField] private Localizer m_unavailableInfoText = null;
	[Space]
	[SerializeField] private ShowHideAnimator m_changeAnim = null;
	[Space]
	[SerializeField] private GameObject m_hcRoot = null;
	[SerializeField] private GameObject m_scRoot = null;
	[SerializeField] private GameObject m_unavailableRoot = null;

	// Internal
	private bool m_firstEnablePassed = false;
	private Coroutine m_delayedShowCoroutine = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Hide all elements and then apply for the first time with current values and without animation
		Toggle(m_hcRoot, false);
		Toggle(m_scRoot, false);
		Toggle(m_unavailableRoot, false);
		Refresh(InstanceManager.menuSceneController.selectedDragonData, false);
	}

	/// <summary>
	/// First update
	/// </summary>
	private void Start() {
		// Subscribe to external events
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
        Messenger.AddListener<IDragonData>(MessengerEvents.MODIFIER_ECONOMY_DRAGON_PRICE_CHANGED, OnModifierChanged);
    }

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Refresh! Don't animate if it's the first time
		Refresh(InstanceManager.menuSceneController.selectedDragonData, m_firstEnablePassed);
		m_firstEnablePassed = true;
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
        Messenger.AddListener<IDragonData>(MessengerEvents.MODIFIER_ECONOMY_DRAGON_PRICE_CHANGED, OnModifierChanged);
    }

	/// <summary>
	/// Refresh with data from given dragon and trigger animations.
	/// </summary>
	/// <param name="_data">The data of the selected dragon.</param>
	/// <param name="_animate">Whether to trigger animations or not.</param>
	public void Refresh(IDragonData _data, bool _animate) {
		// Stop any pending coroutines
		if(m_delayedShowCoroutine != null) {
			StopCoroutine(m_delayedShowCoroutine);
			m_delayedShowCoroutine = null;
		}

		// Trigger animation?
		if(_animate && m_changeAnim != null) {
			// If object is visible, hide first and Refresh the info when the object is hidden
			if(m_changeAnim.visible) {
				// Trigger hide animation
				m_changeAnim.Hide();

				// Refresh info once the object is hidden
				m_delayedShowCoroutine = UbiBCN.CoroutineManager.DelayedCall(() => {
					// Refresh info
					RefreshInfo(_data);

					// Trigger show animation
					m_changeAnim.Show();
				}, m_changeAnim.tweenDuration);	// Use hide animation duration as delay for the coroutine
			} else {
				//  Object already hidden, refresh info and trigger show animation
				RefreshInfo(_data);
				m_changeAnim.Show();
			}
		} else {
			// Just refresh info immediately
			RefreshInfo(_data);
			if(m_changeAnim != null) m_changeAnim.Show(false);
		}
	}

	/// <summary>
	/// Refresh texts and visibility to match given dragon.
	/// Doesn't trigger any animation.
	/// </summary>
	/// <param name="_data">Data.</param>
	private void RefreshInfo(IDragonData _data) {
		// Aux vars
		bool show = true;

		// Update hc unlock button
		// Display?
		show = CheckUnlockWithPC(_data);
		Toggle(m_hcRoot, show);

		// Refresh info
		if(show && m_hcPriceSetup != null) {
			// [AOC] UIDragonPriceSetup makes it easy for us!
			m_hcPriceSetup.InitFromData(_data, UserProfile.Currency.HARD);
		}

		// Update hc unlock button
		// Display?
		show = CheckUnlockWithSC(_data);
		Toggle(m_scRoot, show);

		// Refresh info
		if(show && m_scPriceSetup != null) {
			// [AOC] UIDragonPriceSetup makes it easy for us!
			m_scPriceSetup.InitFromData(_data, UserProfile.Currency.SOFT);
		}

		// Update unavailable info
		if(m_unavailableInfoText != null) {
			// Display?
			show = CheckUnavailable(_data);
			Toggle(m_unavailableRoot, show);

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
	/// Activate/deactivate a GameObject, checking its validity first.
	/// </summary>
	/// <param name="_target">Target GameObject.</param>
	/// <param name="_activate">Toggle on or off?</param>
	private void Toggle(GameObject _target, bool _activate) {
		if(_target == null) return;
		_target.SetActive(_activate);
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
		// Check lock state
		bool show = false;
		switch(_data.lockState) {
			case IDragonData.LockState.LOCKED:
			case IDragonData.LockState.AVAILABLE: {
				show = true;
			} break;

			case IDragonData.LockState.LOCKED_UNAVAILABLE: {
				show = _data.HasPriceModifier(UserProfile.Currency.HARD);	// If there is a discount for this dragon, show even when unavailable
			} break;
		}

		return show;
	}

    /// <summary>
    /// Checks whether the given dragon can be unlocked with SC.
    /// </summary>
    /// <returns>Whether the given dragon can be unlocRefreshCooldownTimersked with SC.</returns>
    /// <param name="_data">Dragon to evaluate.</param>
    public static bool CheckUnlockWithSC(IDragonData _data) {
		// [AOC] Discounts affect visibility?
		return _data.lockState == IDragonData.LockState.AVAILABLE;
	}

	/// <summary>
	/// Checks whether the unavailable message should be displayed for the given dragon.
	/// </summary>
	/// <returns>Whether the given dragon is unavailable.</returns>
	/// <param name="_data">Dragon to evaluate.</param>
	public static bool CheckUnavailable(IDragonData _data) {
		// Check lock state
		bool show = false;
		switch(_data.lockState) {
			case IDragonData.LockState.LOCKED_UNAVAILABLE: {
				show = !_data.HasPriceModifier(UserProfile.Currency.HARD);	// If there is a discount for this dragon, don't show the unavailable message
			} break;
		}

		return show;
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
				_data.GetPriceModified(UserProfile.Currency.HARD),
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
			// [AOC] From 1.18 on, don't trigger the missing SC flow for dragon
			//		 purchases (we are displaying the HC button next to it)
			// Check whether we have enough SC
			long priceSC = _data.GetPriceModified(UserProfile.Currency.SOFT);
			if(priceSC > UsersManager.currentUser.coins) {
				// Not enough SC! Show a message
				UIFeedbackText.CreateAndLaunch(
					LocalizationManager.SharedInstance.Localize("TID_SC_NOT_ENOUGH"),	// [AOC] TODO!! Improve text?
					GameConstants.Vector2.center,
					InstanceManager.menuSceneController.hud.transform as RectTransform,
					"NotEnoughSCError"
				);
			} else {
				// There shouldn't be any problem to perform the transaction, do
				// it via a ResourcesFlow to avoid duplicating code / missing steps
				ResourcesFlow purchaseFlow = new ResourcesFlow(UNLOCK_WITH_SC_RESOURCES_FLOW_NAME);
				purchaseFlow.OnSuccess.AddListener(OnUnlockSuccess);
				purchaseFlow.Begin(
					priceSC,
					UserProfile.Currency.SOFT,
					HDTrackingManager.EEconomyGroup.UNLOCK_DRAGON,
					_data.def
				);
			}
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
		Refresh(DragonManager.GetDragonData(_dragonSku), true);
	}

	/// <summary>
	/// A dragon has been acquired
	/// </summary>
	/// <param name="_data">The data of the acquired dragon.</param>
	public void OnDragonAcquired(IDragonData _data) {
		Refresh(_data, true);
	}

    /// <summary>
    /// A modifider that changes the unlock/price of a dragon has changed.
    /// </summary>
    /// <param name="_data">Dragon Data.</param>
    public void OnModifierChanged(IDragonData _data) {
        if (InstanceManager.menuSceneController.selectedDragonData == _data) {
            Refresh(_data, false);
        }
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
