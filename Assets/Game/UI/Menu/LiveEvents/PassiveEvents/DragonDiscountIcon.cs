// DragonDiscountIcon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/11/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the dragon discount icon in the menu HUD.
/// </summary>
public class DragonDiscountIcon : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float UPDATE_FREQUENCY = 1f;  // Seconds

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ShowHideAnimator m_rootAnim = null;
	[SerializeField] private TextMeshProUGUI m_timerText = null;
	[Space]
	[SerializeField] private UISpriteAddressablesLoader m_dragonIconLoader = null;
	[SerializeField] private Transform m_tierIconContainer = null;
	[SerializeField] private TextMeshProUGUI m_discountText = null;
	[Space]
	[SerializeField] private GameObject m_arrow = null;
	[Space]
	[SerializeField] private Graphic[] m_toTint = null;

	// Internal properties for comfort
	private OfferPackDragonDiscount targetDiscount {
		get {
			return OffersManager.activeDragonDiscount;
		}
	}

	private ModEconomyDragonPrice mod {
		get { 
			if(targetDiscount != null) {
				return targetDiscount.mod; 
			}
			return null;
		}
	}

	private IDragonData targetDragonData {
		get {
			if(mod != null) {
				return DragonManager.GetDragonData(mod.dragonSku);
			}
			return null;
		}
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected void Awake() {
		// Start hidden
		m_rootAnim.ForceHide(false);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	protected virtual void Start() {
		// Perform a firs refresh
		RefreshData(true);

		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.OFFERS_RELOADED, OnOffersReloaded);
		Messenger.AddListener<List<OfferPack>>(MessengerEvents.OFFERS_CHANGED, OnOffersChanged);
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenTransition);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected virtual void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.OFFERS_RELOADED, OnOffersReloaded);
		Messenger.RemoveListener<List<OfferPack>>(MessengerEvents.OFFERS_CHANGED, OnOffersChanged);
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenTransition);
    }

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	protected virtual void OnEnable() {
		// Program a periodic update
		InvokeRepeating("UpdatePeriodic", 0f, UPDATE_FREQUENCY);

		// Get latest data from the manager
		RefreshData(true);
	}

	/// <summary>
	/// Called periodically - to avoid doing stuff every frame.
	/// </summary>
	protected virtual void UpdatePeriodic() {
		// Skip if we're not active - probably in a screen we don't belong to
		if(!isActiveAndEnabled) return;

		// Refresh the timer!
		RefreshTimer(true);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	protected virtual void OnDisable() {
		// Cancel periodic update
		CancelInvoke();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get latest data from the manager and update visuals.
	/// </summary>
	/// <param name="_checkVisibility">Whether to refresh visibility as well or not.</param>
	protected virtual void RefreshData(bool _checkVisibility) {
		// Refresh visibility
		if(_checkVisibility) RefreshVisibility();

		// Nothing else to do if there is no active discount
		if(targetDiscount == null) return;

		// Update the timer
		RefreshTimer(false);

		// Refresh Dragon icon
		if(m_dragonIconLoader != null) {
			if(targetDragonData != null) {
                string iconName = IDragonData.GetDefaultDisguise(targetDragonData.sku).Get("icon");
				m_dragonIconLoader.LoadAsync(iconName);
            }
		}

		// Tier icon
		if(m_tierIconContainer != null) {
			bool show = false;

			// Remove the placeholder
			m_tierIconContainer.transform.DestroyAllChildren(true);

			if (targetDragonData != null) {
				show = true;

				// Set the actual tier icon
				GameObject tierIconPrefab = UIConstants.GetTierIcon(targetDragonData.tier);
				Instantiate(tierIconPrefab, m_tierIconContainer);
			}

			m_tierIconContainer.gameObject.SetActive(show);
		}

		// Color tint
		if(m_toTint != null && targetDragonData != null) {
			for(int i = 0; i < m_toTint.Length; ++i) {
				if(m_toTint[i] != null) {
					m_toTint[i].color = UIConstants.GetDragonTierColor(targetDragonData.tier);
				}
			}
		}

		// Discount text
		if(m_discountText != null) {
			bool show = false;
			if(targetDragonData != null) {
				show = true;
				m_discountText.text = LocalizationManager.SharedInstance.Localize(
					"TID_OFFER_DISCOUNT_PERCENTAGE",
					StringUtils.FormatNumber(Mathf.Abs(targetDragonData.GetPriceModifier(UserProfile.Currency.HARD)), 0)
				);
			}
			m_discountText.gameObject.SetActive(show);
		}
	}

	/// <summary>
	/// Check whether the icon can be displayed or not and does it.
	/// </summary>
	public void RefreshVisibility() {
        // Check conditions
        bool show = CheckVisibility(out bool allowAnim);

		// Apply!
		if(m_rootAnim != null) {
			if(allowAnim) {
				m_rootAnim.Set(show);
			} else {
				m_rootAnim.ForceSet(show, false);
            }
		}
	}

	/// <summary>
	/// Perform all the required checks to see if the icon should be displayed or not.
	/// </summary>
	/// <param name="_allowAnim">Out parameter to tell whether we can show the animation or we just force visibility without animation.</param>
	/// <returns>Whether the icon should be displayed or not.</returns>
	protected virtual bool CheckVisibility(out bool _allowAnim) {
		// Allow animation by default
		_allowAnim = true;

		// Only show in the menu
		if(InstanceManager.menuSceneController == null) {
			_allowAnim = false;
			return false;
		}

		// Never during tutorial
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_DRAGON_DISCOUNTS_AT_RUN) {
			_allowAnim = false;
			return false;
		}

		// Only show in some specific screens
		MenuScreen currentScreen = InstanceManager.menuSceneController.currentScreen;
		if(currentScreen != MenuScreen.DRAGON_SELECTION) return false;

		// Don't show if there is no active discount
		if(targetDiscount == null) return false;
		if(!targetDiscount.isActive) return false;

		// Don't show if mod not valid
		if(mod == null) return false;

		// Don't show if target dragon not valid
		if(targetDragonData == null) return false;

		// Don't show if selected dragon is the target dragon
		if(InstanceManager.menuSceneController.selectedDragon == targetDragonData.sku) {
			return false;
		}

		// Don't show arrow if the target dragon is previous to the current selected dragon
		if(m_arrow != null) {
			bool showArrow = InstanceManager.menuSceneController.selectedDragonData.GetOrder() < targetDragonData.GetOrder();
			m_arrow.SetActive(showArrow);
		}

		return true;
	}

	/// <summary>
	/// Refresh the timer. To be called periodically.
	/// </summary>
	/// <param name="_checkExpiration">Refresh data if timer reaches 0?</param>
	private void RefreshTimer(bool _checkExpiration) {
		// Is discount still valid?
		if(targetDiscount == null) return;

		// Update text
		double remainingSeconds = targetDiscount.remainingTime.TotalSeconds;
		if(m_timerText != null) {
			// Set text
			if(m_timerText.gameObject.activeSelf) {
				m_timerText.text = TimeUtils.FormatTime(
					System.Math.Max(0, remainingSeconds), // Just in case, never go negative
					TimeUtils.EFormat.ABBREVIATIONS,
					2
				);
			}
		}

		// Manage timer expiration when the icon is visible
		if(_checkExpiration && remainingSeconds <= 0) {
			RefreshData(true);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The offers manager has been reloaded.
	/// </summary>
	private void OnOffersReloaded() {
		RefreshData(true);
	}

	/// <summary>
	/// Offers list has changed.
	/// </summary>
	private void OnOffersChanged(List<OfferPack> offersChanged = null) {
		RefreshData(true);
	}

	/// <summary>
	/// Selected dragon has changed.
	/// </summary>
	/// <param name="_dragonSku">Sku of the newly selected dragon.</param>
	private void OnDragonSelected(string _dragonSku) {
		RefreshVisibility();
	}

	/// <summary>
	/// The menu screen change animation has started.
	/// </summary>
	/// <param name="_from">Screen we come from.</param>
	/// <param name="_to">Screen we're going to.</param>
	private void OnMenuScreenTransition(MenuScreen _from, MenuScreen _to) {
		RefreshVisibility();
	}

	/// <summary>
	/// The scroll to target button has been pressed.
	/// </summary>
	public void OnScrollToTargetDragon() {
		// Just do it
		if(targetDragonData != null) {
			InstanceManager.menuSceneController.SetSelectedDragon(targetDragonData.sku);
		}
	}
}